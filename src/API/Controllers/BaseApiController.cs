using API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ServiceFilter(typeof(IsLockedFilter))]
[ApiController]
[Route("/api/[controller]")]
public class BaseApiController : ControllerBase
{
}