using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace WebAPI.Utils.Query;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, QueryOptions options)
    {
        if (options.Filters == null || !options.Filters.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var filter in options.Filters)
        {
            var propertyExpression = GetPropertyExpression(parameter, filter.Key);
            if (propertyExpression == null)
                continue;

            var filterExpression = BuildFilterExpression(propertyExpression, filter.Value);
            if (filterExpression == null)
                continue;

            combinedExpression = combinedExpression == null
                ? filterExpression
                : Expression.AndAlso(combinedExpression, filterExpression);
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            query = query.Where(lambda);
        }

        if (!string.IsNullOrEmpty(options.Search))
        {
            query = query.ApplySearch(options.Search);
        }

        return query;
    }

    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var searchExpression = BuildSearchExpression<T>(parameter, searchTerm);

        if (searchExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, QueryOptions options)
    {
        if (string.IsNullOrEmpty(options.SortBy))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyExpression = GetPropertyExpression(parameter, options.SortBy);

        if (propertyExpression == null)
            return query;

        var lambda = Expression.Lambda(propertyExpression, parameter);
        var methodName = options.SortDir?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

        var orderByMethod = typeof(Queryable).GetMethods()
            .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2)?
            .MakeGenericMethod(typeof(T), propertyExpression.Type);

        if (orderByMethod == null)
            return query;

        return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, lambda })!;
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, QueryOptions options)
    {
        return query.Skip((options.Page - 1) * options.PageSize).Take(options.PageSize);
    }

    private static Expression? GetPropertyExpression(ParameterExpression parameter, string propertyName)
    {
        try
        {
            var properties = propertyName.Split('.');
            Expression expression = parameter;

            foreach (var property in properties)
            {
                var propertyInfo = expression.Type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                    return null;

                expression = Expression.Property(expression, propertyInfo);
            }

            return expression;
        }
        catch
        {
            return null;
        }
    }

    private static Expression? BuildFilterExpression(Expression propertyExpression, string filterValue)
    {
        try
        {
            if (filterValue.StartsWith(">="))
            {
                var value = filterValue.Substring(2);
                var constantExpression = GetConstantExpression(propertyExpression.Type, value);
                return constantExpression != null ? Expression.GreaterThanOrEqual(propertyExpression, constantExpression) : null;
            }
            else if (filterValue.StartsWith("<="))
            {
                var value = filterValue.Substring(2);
                var constantExpression = GetConstantExpression(propertyExpression.Type, value);
                return constantExpression != null ? Expression.LessThanOrEqual(propertyExpression, constantExpression) : null;
            }
            else if (filterValue.StartsWith(">"))
            {
                var value = filterValue.Substring(1);
                var constantExpression = GetConstantExpression(propertyExpression.Type, value);
                return constantExpression != null ? Expression.GreaterThan(propertyExpression, constantExpression) : null;
            }
            else if (filterValue.StartsWith("<"))
            {
                var value = filterValue.Substring(1);
                var constantExpression = GetConstantExpression(propertyExpression.Type, value);
                return constantExpression != null ? Expression.LessThan(propertyExpression, constantExpression) : null;
            }
            else if (filterValue.StartsWith("!="))
            {
                var value = filterValue.Substring(2);
                var constantExpression = GetConstantExpression(propertyExpression.Type, value);
                return constantExpression != null ? Expression.NotEqual(propertyExpression, constantExpression) : null;
            }
            else
            {
                if (propertyExpression.Type == typeof(string))
                {
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var constantExpression = Expression.Constant(filterValue);
                    return containsMethod != null ? Expression.Call(propertyExpression, containsMethod, constantExpression) : null;
                }
                else
                {
                    var constantExpression = GetConstantExpression(propertyExpression.Type, filterValue);
                    return constantExpression != null ? Expression.Equal(propertyExpression, constantExpression) : null;
                }
            }
        }
        catch
        {
            return null;
        }
    }

    private static Expression? BuildSearchExpression<T>(ParameterExpression parameter, string searchTerm)
    {
        var stringProperties = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .ToList();

        if (!stringProperties.Any())
            return null;

        Expression? searchExpression = null;
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        var searchConstant = Expression.Constant(searchTerm.ToLower());

        foreach (var property in stringProperties)
        {
            var propertyExpression = Expression.Property(parameter, property);
            var nullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
            var toLowerCall = Expression.Call(propertyExpression, toLowerMethod!);
            var containsCall = Expression.Call(toLowerCall, containsMethod!, searchConstant);
            var propertySearch = Expression.AndAlso(nullCheck, containsCall);

            searchExpression = searchExpression == null ? propertySearch : Expression.OrElse(searchExpression, propertySearch);
        }

        return searchExpression;
    }

    public static async Task<PagedResult<TDto>> ToPagedResultAsync<T, TDto>(
        this IQueryable<T> query,
        QueryOptions options,
        Func<T, TDto> mapFunc)
    {
        var total = await query.CountAsync();

        var items = await query
            .ApplyPaging(options)
            .ToListAsync();

        var mappedItems = items.Select(mapFunc).ToList();

        return new PagedResult<TDto>(mappedItems, options.Page, options.PageSize, total);
    }

    private static Expression? GetConstantExpression(Type propertyType, string value)
    {
        try
        {
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (targetType == typeof(string))
            {
                return Expression.Constant(value);
            }
            else if (targetType == typeof(int))
            {
                return int.TryParse(value, out var intValue) ? Expression.Constant(intValue, propertyType) : null;
            }
            else if (targetType == typeof(bool))
            {
                return bool.TryParse(value, out var boolValue) ? Expression.Constant(boolValue, propertyType) : null;
            }
            else if (targetType == typeof(DateTime))
            {
                return DateTime.TryParse(value, out var dateValue) ? Expression.Constant(dateValue, propertyType) : null;
            }
            else if (targetType == typeof(decimal))
            {
                return decimal.TryParse(value, out var decimalValue) ? Expression.Constant(decimalValue, propertyType) : null;
            }
            else if (targetType == typeof(double))
            {
                return double.TryParse(value, out var doubleValue) ? Expression.Constant(doubleValue, propertyType) : null;
            }
            else if (targetType.IsEnum)
            {
                return Enum.TryParse(targetType, value, true, out var enumValue) ? Expression.Constant(enumValue, propertyType) : null;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
