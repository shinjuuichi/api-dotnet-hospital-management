using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Base;

[ApiController]
public abstract class BaseController : ControllerBase
{
    // Controllers can just return Ok() directly
    // The middleware will handle wrapping the response
}
