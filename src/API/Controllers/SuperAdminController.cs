using API.Dto.Auth;
using API.Dto.User;
using API.ErrorResponses;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Entities.Auth;
using Core.Interfaces.User;
using Core.Specifications;
using Core.Specifications.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = "SUPER_ADMIN")]
public class SuperAdminController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    public SuperAdminController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet("admins")]
    public async Task<ActionResult<Pagination<AppUser>>> GetAdmins([FromQuery] PaginationParam param)
    {
        var userWithRoleSpec = new UserByRoleWithAvatarSpec(param, Role.ADMIN, true);
        var userWithFilterForCountSpec = new UserByRoleSpec(param, Role.ADMIN, false);
        var users = await _userService.ListUserAsync(userWithRoleSpec);
        var count = await _userService.CountUsersAsync(userWithFilterForCountSpec);
        var data = _mapper.Map<IReadOnlyList<AppUser>, IReadOnlyList<AdminBasicInfo>>(users);
        return Ok(new Pagination<AdminBasicInfo>()
        {
            PageIndex = param.PageIndex,
            PageSize = param.PageSize,
            Data = data,
            Count = count
        });
    }

    [HttpPost("admin")]
    public async Task<ActionResult<RegisterResponse>> CreateAdmin([FromBody] AdminCreateRequest registerRequest)
    {
        var errors = new Dictionary<string, CustomErrorObject>();

        if (await _userService.GetUserAsync(
                new Specification<AppUser>(u => u.UserName == registerRequest.Username)) != null)
        {
            errors.Add("Username", new CustomErrorObject()
            {
                Message = "Tên người dùng đã được sử dụng",
                ErrorCode = "EXISTED_USERNAME"
            });
        }
        if (await _userService.GetUserAsync(
                new Specification<AppUser>(u => u.Email == registerRequest.Email)) != null)
        {
            errors.Add("Email", new CustomErrorObject()
            {
                Message = "Email đã được sử dụng",
                ErrorCode = "EXISTED_EMAIL"
            });
        }

        if (errors.Count > 0)
        {
            return new BadRequestObjectResult(new UserValidationErrorResponse()
            {
                Errors = errors
            });
        }

        var newAdmin = new AppUser()
        {
            Fullname = registerRequest.Fullname,
            UserName = registerRequest.Username,
            Email = registerRequest.Email,
            EmailConfirmed = true,
            Note = registerRequest.Note,
            PhoneNumber = registerRequest.PhoneNumber,
            Role = Role.ADMIN
        };
        
        var registerResult = await _userService.RegisterAsync(newAdmin, registerRequest.Password);
        if (!registerResult) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
        await _userService.AddDefaultAvatarAsync(newAdmin.Id);
        return Ok(new RegisterResponse()
        {
            Email = registerRequest.Email,
            Username = registerRequest.Username,
            Role = Role.ADMIN.ToString()
        });
    }

    [HttpPut("admin/status/{id}")]
    public async Task<IActionResult> UpdateAdminLockedStatus(string id)
    {
        var admin = await _userService.GetUserAsync(
            new Specification<AppUser>(u => u.Id == id && u.Role == Role.ADMIN));
        if (admin == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy"));
        admin.IsLocked = !admin.IsLocked;
        var res = await _userService.UpdateUserAsync(admin);
        if (!res) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
        return Ok("Đã cập nhật");
    }
}