using System.Security.Claims;
using API.ErrorResponses;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.User;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class CoachVerificationFilter : IAsyncActionFilter
{
    private readonly IUserService _userService;

    public CoachVerificationFilter(IUserService userService)
    {
        _userService = userService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var coach = await _userService.GetUserAsync(new Specification<AppUser>(u =>
            u.Id == context.HttpContext.User.FindFirstValue("Id")));
        if (coach.IsVerified!.Value) await next();
        context.Result = new BadRequestObjectResult(new ErrorResponse(400, "Không hợp lệ. Vui lòng xác minh chứng chỉ"));
    }
}