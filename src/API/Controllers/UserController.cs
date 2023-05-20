using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using API.Dto.Auth;
using API.Dto.User;
using API.ErrorResponses;
using AutoMapper;
using Core.Entities;
using Core.Entities.Auth;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Interfaces.User;
using Core.Specifications;
using Core.Specifications.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UserController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;

    public UserController(IUserService userService, IMapper mapper, IMediaService mediaService, IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _mapper = mapper;
        _mediaService = mediaService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("confirm-email")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ContentResult> ConfirmEmail(string id, string token)
    {
        var css = new StringBuilder("display: flex; align-items: center; justify-content: center; color: #cd5c5c; font-size: 24px");
        var html = new StringBuilder($"<div style=\"{css}\"><p>TEMPT</p></div>");
        
        // Encode vietnamese message
        string Encode(string str)
        {
            return HtmlEncoder.Default.Encode(str);
        }
        
        var response = new ContentResult()
        {
            ContentType = "text/html"
        };
        
        var user = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == id));
        // Check user
        if (user == null)
        {
            html.Replace("TEMPT", Encode("Người dùng không hợp lệ"));
            response.Content = html.ToString();
            return response;
        }

        // Check email
        if (user.EmailConfirmed)
        {
            html.Replace("TEMPT", Encode("Email này đã được xác nhận trước đó. Vui lòng đóng cửa sổ này"));
            response.Content = html.ToString();
            return response;
        }
        
        var result = await _userService.ConfirmEmailAsync(user, token);
        if (result)
        {
            html.Replace("#cd5c5c", "#3cb371");
            html.Replace("TEMPT", Encode("Email của bạn đã được xác nhận. Bạn có thể đóng cửa sổ này"));
        }
        else
        {
            html.Replace("TEMPT", Encode("Có lỗi xảy ra"));
        }
        
        response.Content = html.ToString();
        return response;
    }

    [HttpGet("email-resending")]
    public async Task<IActionResult> ResendConfirmEmail([FromQuery] string email)
    {
        var existedUser = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Email == email));
        if (existedUser is null)
        {
            return BadRequest(new ErrorResponse(400, "Tài khoản không tồn tại"));
        }

        if (existedUser.EmailConfirmed)
        {
            return BadRequest(new ErrorResponse(400, "Tài khoản này đã được xác thực trước đó."));
        }
        
        var callBackUrl =
            $"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", "User", new { id = existedUser.Id, token = "TEMPT"})}";

        var sendResult = await _userService.SendUrlAsync(existedUser, callBackUrl, "Verify");

        return sendResult
            ? Ok("Email xác thực đã được gửi")
            : StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
    }

    [HttpGet("password-forgot")]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {
        var existedUser = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Email == email));
        if (existedUser is null)
        {
            return BadRequest(new ErrorResponse(400, "Người dùng không tồn tại"));
        }

        if (!existedUser.EmailConfirmed)
        {
            return BadRequest(new ErrorResponse(400, "Tài khoản chưa được xác thực. Vui lòng kiểm tra email để xác thực tài khoản."));
        }

        var callBackUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("Index","ResetPassword", new {id = existedUser.Id, token = "TEMPT"} )}";

        var sendResult = await _userService.SendUrlAsync(existedUser, callBackUrl, "Reset");
        return sendResult
            ? Ok("Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra email.")
            : StatusCode(500, new ErrorResponse(500, "Lỗi gửi email"));
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest registerRequest)
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

        if (await _userService.GetUserAsync(
                new Specification<AppUser>(u => u.PhoneNumber == registerRequest.PhoneNumber)) != null)
        {
            errors.Add("Phone", new CustomErrorObject()
            {
                Message = "Số điện thoại đã được sử dụng",
                ErrorCode = "EXISTED_PHONE"
            });
        }

        if (errors.Count > 0)
        {
            return new BadRequestObjectResult(new UserValidationErrorResponse()
            {
                Errors = errors
            });
        }

        var newUser = new AppUser()
        {
            Fullname = registerRequest.Fullname,
            UserName = registerRequest.Username,
            Email = registerRequest.Email,
            PhoneNumber = registerRequest.PhoneNumber,
            Gender = Enum.Parse<Gender>(registerRequest.Gender),
            Dob = DateTime.ParseExact(registerRequest.Dob, new []{"dd/mm/yyyy", "dd-mm-yyyy", "dd.mm.yyyy"}, CultureInfo.InvariantCulture),
            Role = registerRequest.IsCoach ? Role.COACH : Role.CLIENT,
            IsVerified = registerRequest.IsCoach ? false : null
        };
        
        var registerResult = await _userService.RegisterAsync(newUser, registerRequest.Password);
        
        if (!registerResult) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
        
        // Send mail to user
        var callBackUrl =
            $"{Request.Scheme}://{Request.Host}{Url.Action("Index", "Mail", new { id = newUser.Id, token = "TEMPT" })}";
            
        var mailSendingResult = await _userService.SendUrlAsync(newUser, callBackUrl, "Verify");
            
        if (mailSendingResult)
        {
            await _userService.AddDefaultAvatarAsync(newUser.Id);
            return Ok(new RegisterResponse()
            {
                Email = registerRequest.Email,
                Username = registerRequest.Username,
                Phone = registerRequest.PhoneNumber,
                Role = registerRequest.IsCoach ? "Coach" : "Client"
            });
        }

        await _userService.DeleteUserAsync(newUser);
        return StatusCode(500, new ErrorResponse(500, "Lỗi gửi email hoặc email không tồn tại"));
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginRequest loginRequest)
    {
        var existedUser = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
        
        if (existedUser == null)
        {
            return Unauthorized(new ErrorResponse(401,"Tài khoản hoặc mật khẩu không đúng"));
        }

        if (existedUser.IsLocked)
        {
            return Unauthorized(new ErrorResponse(401, "Tài khoản đã bị khoá"));
        }

        if (existedUser.EmailConfirmed == false)
        {
            return Unauthorized(new ErrorResponse(401, "Email chưa được xác nhận"));
        }

        var tokens = await _userService.GenerateTokenAsync(existedUser);
        return Ok(new TokenDto()
        {
            AccessToken = tokens[0],
            RefreshToken = tokens[1]
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenDto>> Refresh([FromBody] TokenDto refreshRequest)
    {
        var tokens = await _userService.RefreshLoginAsync(refreshRequest.AccessToken, refreshRequest.RefreshToken);
        if (tokens == null)
        {
            return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
        }
        return Ok(new TokenDto()
        {
            AccessToken = tokens[0],
            RefreshToken = tokens[1]
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
    {
        var logoutResult = await _userService.LogoutAsync(logoutRequest.CurrentRefreshToken);
        if (logoutResult)
        {
            return Ok();
        }

        return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userService.GetUserAsync(new Specification<AppUser>(u =>
            u.Id == User.FindFirstValue("Id")));
        if (user is null)
        {
            return Unauthorized(new ErrorResponse(401, "Người dùng không tồn tại"));
        }
        var data = _mapper.Map<BaseUserProfile>(user);
        if (user.Role == Role.COACH)
        {
            data = _mapper.Map<CoachProfileResponse>(user);
        }
        return Ok(data);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] BaseUserProfile profile)
    {
        var user = await _userService.GetUserAsync(new Specification<AppUser>(u =>
            u.Id == User.FindFirstValue("Id")));
        var errors = new Dictionary<string, CustomErrorObject>();
        var isEmailUpdated = false;
        //Check phone number
        if (profile.PhoneNumber != user!.PhoneNumber)
        {
            if (await _userService.GetUserAsync(new Specification<AppUser>(u =>
                    u.PhoneNumber == profile.PhoneNumber)) is not null)
            {
                errors.Add("Phone", new CustomErrorObject()
                {
                    Message = "Số điện thoại đã được sử dụng",
                    ErrorCode = "EXISTED_PHONE"
                });
            }
        }
        //Check email
        if (profile.Email != user.Email)
        {
            if (await _userService.GetUserAsync(new Specification<AppUser>(u =>
                    u.Email == profile.Email)) is not null)
            {
                errors.Add("Email", new CustomErrorObject()
                {
                    Message = "Email đã được sử dụng",
                    ErrorCode = "EXISTED_EMAIL"
                });
                return BadRequest(new UserValidationErrorResponse()
                {
                    Errors = errors
                });

            }
            isEmailUpdated = true;
            user.EmailConfirmed = false;
        }

        user.Fullname = profile.Fullname;
        user.Gender = Enum.Parse<Gender>(profile.Gender);
        user.Dob = DateTime.ParseExact(profile.Dob, new []{"dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"}, CultureInfo.InvariantCulture);
        user.Email = profile.Email;
        user.PhoneNumber = profile.PhoneNumber;

        var result = await _userService.UpdateUserAsync(user);
        if(!result) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

        if (!isEmailUpdated) return Ok("Cập nhật thành công");
        // Logout user
        _userService.LogoutAll(user);
            
        // Send email confirm
        var callBackUrl =
            $"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", "User", new { id = user.Id, token = "TEMPT"})}";
        var sendResult = await _userService.SendUrlAsync(user, callBackUrl, "Verify");
        if (!sendResult)
            return BadRequest(new ErrorResponse(500, "Không gửi được email xác nhận"));
        return Ok("Cập nhật thành công. Vui lòng xác nhận lại email để đăng nhập");

    }

    [HttpPut("avatar")]
    [Authorize]
    public async Task<IActionResult> UpdateAvatar([Required(ErrorMessage = "Vui lòng upload ảnh")] IFormFile file)
    {
        var userId = User.FindFirstValue("Id");

        var mediaSpec = new MediaWithUserSpec(userId, true);

        var avatar = await _unitOfWork.Repository<MediaAsset>().ListAsync(mediaSpec);
        
        var success = avatar.Count == 0 ?
            await _mediaService.AddMediaAsync(file, new MediaAsset
            {
                IsAvatar = true,
                OnPortfolio = true,
                UserId = userId
            }) :
            await _mediaService.UpdateImageAsync(file, avatar[0].Id);

        if (!success) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

        return Ok("Cập nhật thành công");
    }

    [HttpGet("avatar")]
    [Authorize]
    public async Task<IActionResult> GetAvatar()
    {
        var userId = User.FindFirstValue("Id");

        var mediaSpec = new MediaWithUserSpec(userId, true);

        var avatar = await _unitOfWork.Repository<MediaAsset>().ListAsync(mediaSpec);

        return Ok(avatar.Count==0 ? "": avatar[0].Url);
    }
}