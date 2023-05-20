using Core.Entities;
using Core.Interfaces.User;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MailController : Controller
{
    private readonly IUserService _userService;

    public MailController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index(string id, string token)
    {
        var user = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == id));
        // Check user
        if (user == null)
        {
            ViewBag.Message = "Người dùng không hợp lệ";
            return View();
        }

        // Check email
        if (user.EmailConfirmed)
        {
            ViewBag.Message = "Email này đã được xác nhận trước đó";
            return View();
        }
        
        var isSuccess = await _userService.ConfirmEmailAsync(user, token);
        if (isSuccess)
        {
            ViewBag.IsSuccess = true;
            ViewBag.Message = "Email của bạn đã được xác nhận";
            
        }
        else
        {
            ViewBag.Message = "Lỗi";
        }
        
        return View();
    }
}