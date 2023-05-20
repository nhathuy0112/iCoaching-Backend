using API.Dto.Auth;
using Core.Entities;
using Core.Interfaces.User;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ResetPasswordController : Controller
{
    private readonly IUserService _userService;

    public ResetPasswordController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index(string id, string token)
    {
        var user = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == id));
        if (user is null)
        {
            ViewBag.Message = "Không tìm thấy người dùng";
            return View();
        }

        if (user.PasswordResetToken == null || !token.Equals(user.PasswordResetToken))
        {
            ViewBag.Message = "Mã không hợp lệ";
            return View();
        }

        if (DateTime.UtcNow > user.PasswordResetTokenExpiry)
        {
            ViewBag.Message = "Email đã hết hạn";
            return View();
        }

        var model = new ResetPasswordModel()
        {
            Id = id,
            Token = token,
            Password = "",
            ConfirmPassword = ""
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel data)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState;
            return View("Index", data);
        }
        var user = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == data.Id));
        if (user is null)
        {
            ViewBag.Message = "Không tìm thấy người dùng";
            return View("Index");
        }

        var result = await _userService.ResetPasswordAsync(user, data.Token,data.Password);
        if (result)
        {
            ViewBag.IsSuccess = true;
            ViewBag.Message = "Đặt lại mật khẩu thành công. Bạn có thể đóng cửa sổ này";
            return View("Index");
        }

        ViewBag.Message = "Có lỗi xảy ra. Đóng cửa sổ này và thử lại";
        return View("Index");
    }
}