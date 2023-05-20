using API.ErrorResponses;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("errors/{code:int}")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : BaseApiController
{
    public IActionResult Error(int code)
    {
        return new ObjectResult(new ErrorResponse(code, null));
    }
}