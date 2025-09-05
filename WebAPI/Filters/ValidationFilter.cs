using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, string[]>();
            
            foreach (var modelError in context.ModelState)
            {
                var errorMessages = modelError.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                if (errorMessages.Length > 0)
                {
                    errors[ToCamelCase(modelError.Key)] = errorMessages;
                }
            }

            var errorResponse = new
            {
                code = 422,
                status = false,
                data = new { errors = errors },
                message = "Validation failed."
            };

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = 422
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Not needed for this filter
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            return input;

        return char.ToLower(input[0]) + input[1..];
    }
}
