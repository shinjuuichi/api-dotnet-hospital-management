using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Filters;

public class ResponseWrapperFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
        {
            var value = objectResult.Value;
            if (value != null && IsAlreadyWrapped(value))
            {
                return; 
            }

            var wrappedResponse = new
            {
                code = 200,
                status = true,
                data = value,
                message = "OK"
            };

            objectResult.Value = wrappedResponse;
            objectResult.StatusCode = 200; 
        }
        else if (context.Result is OkResult)
        {
            context.Result = new ObjectResult(new
            {
                code = 200,
                status = true,
                data = (object?)null,
                message = "OK"
            })
            {
                StatusCode = 200
            };
        }
        else if (context.Result is CreatedResult createdResult)
        {
            context.Result = new ObjectResult(new
            {
                code = 200,
                status = true,
                data = createdResult.Value,
                message = "OK"
            })
            {
                StatusCode = 200
            };
        }
        else if (context.Result is CreatedAtActionResult createdAtActionResult)
        {
            context.Result = new ObjectResult(new
            {
                code = 200,
                status = true,
                data = createdAtActionResult.Value,
                message = "OK"
            })
            {
                StatusCode = 200
            };
        }
    }

    private static bool IsAlreadyWrapped(object value)
    {
        var type = value.GetType();
        var hasCode = type.GetProperty("Code") != null || type.GetProperty("code") != null;
        var hasStatus = type.GetProperty("Status") != null || type.GetProperty("status") != null;
        var hasData = type.GetProperty("Data") != null || type.GetProperty("data") != null;
        var hasMessage = type.GetProperty("Message") != null || type.GetProperty("message") != null;

        return hasCode && hasStatus && hasData && hasMessage;
    }
}
