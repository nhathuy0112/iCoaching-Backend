using System.Net;
using System.Security.Claims;
using API.ErrorResponses;
using Core.Entities;
using Core.Interfaces.User;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class IsLockedFilter : IAsyncActionFilter
{
    private readonly IUserService _userService;

    public IsLockedFilter(IUserService userService)
    {
        _userService = userService;
    }


    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.User.FindFirstValue("Id");
        
        if (userId != null)
        {
            var user = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == userId));
        
            if (user.IsLocked)
            {
                context.Result = new BadRequestObjectResult(new Dictionary<string, CustomErrorObject>()
                {
                    {
                        "Account", new CustomErrorObject()
                        {
                            ErrorCode = "LOCKED",
                            Message = "Người dùng bị khoá"
                        }
                    }
                });
                return;
            }
        }

        await next();


    }
}