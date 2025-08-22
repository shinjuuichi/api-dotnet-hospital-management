using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Utils.Query;

public class QueryOptionsFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;

        var page = int.TryParse(request.Query["page"], out var p) ? Math.Max(1, p) : 1;
        var pageSize = int.TryParse(request.Query["pageSize"], out var ps) ? Math.Min(200, Math.Max(1, ps)) : 20;
        var sortBy = request.Query["sortBy"].FirstOrDefault();
        var sortDir = request.Query["sortDir"].FirstOrDefault()?.ToLower();
        if (sortDir != "desc") sortDir = "asc";
        var search = request.Query["search"].FirstOrDefault();

        var filters = new Dictionary<string, string>();
        foreach (var key in request.Query.Keys.Where(k => k.StartsWith("filters[")))
        {
            if (key.StartsWith("filters[") && key.EndsWith("]"))
            {
                var filterName = key.Substring(8, key.Length - 9);
                var filterValue = request.Query[key].FirstOrDefault();
                if (!string.IsNullOrEmpty(filterValue))
                {
                    filters[filterName] = filterValue;
                }
            }
        }

        var queryOptions = new QueryOptions
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDir = sortDir,
            Search = search,
            Filters = filters.Count > 0 ? filters : null
        };

        context.HttpContext.Items["QueryOptions"] = queryOptions;

        base.OnActionExecuting(context);
    }
}
