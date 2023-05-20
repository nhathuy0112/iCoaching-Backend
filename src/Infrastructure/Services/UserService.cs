using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Core.Entities;
using Core.Entities.Auth;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Interfaces.User;
using Core.Specifications;
using Core.Specifications.Media;
using Core.Specifications.User;
using Infrastructure.Repositories;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IMailService _mailService;

    public UserService(UserManager<AppUser> userManager, IConfiguration configuration, IMailService mailService, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _configuration = configuration;
        _mailService = mailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ConfirmEmailAsync(AppUser user, string token)
    {
        if (user.MailToken is null || !token.Equals(user.MailToken))
        {
            return false;
        }
        if (DateTime.UtcNow > user.MailTokenExpiry)
        {
            return false;
        }
        
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(AppUser user, string token, string newPassword)
    {
        if (user.PasswordResetToken is null || !token.Equals(user.PasswordResetToken))
        {
            return false;
        }
        if (DateTime.UtcNow > user.PasswordResetTokenExpiry)
        {
            return false;
        }

        await _userManager.ResetPasswordAsync(user, token, newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> RegisterAsync(AppUser newUser, string password)
    {
        var createUserResult = await _userManager.CreateAsync(newUser,password);
        return createUserResult.Succeeded;
    }

    public async Task<AppUser?> LoginAsync(string username, string password)
    {
        var existedUser = await _userManager.FindByNameAsync(username);
        
        if (existedUser is null)
        {
            return null;
        }

        var isCorrectPassword = await _userManager.CheckPasswordAsync(existedUser, password);
        
        return !isCorrectPassword ? null : existedUser;
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var validatedRefreshToken = await GetValidatedRefreshToken(refreshToken);
        if (validatedRefreshToken == null) return false;
        validatedRefreshToken.IsRevoked = true;
        _unitOfWork.Repository<RefreshToken>().Update(validatedRefreshToken);
        var res = await _unitOfWork.CompleteAsync();
        return res != 0;
    }

    private async Task<string> CreateMailToken(AppUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        user.MailToken = token;
        user.MailTokenExpiry = DateTime.UtcNow.AddDays(1);
        await _userManager.UpdateAsync(user);
        return token;
    }

    private async Task<string> CreatePasswordResetToken(AppUser user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userManager.UpdateAsync(user);
        return token;
    }

    public async Task<bool> SendUrlAsync(AppUser user, string callBackUrl, string type)
    {
        var isReset = type.Equals("Reset");
        var token = isReset ? await CreatePasswordResetToken(user) : await CreateMailToken(user);
        var mailType = isReset ? MailType.Reset : MailType.Verify;
        var subject = isReset ? "Đặt lại mật khẩu" : "Xác nhận email";
        var url = callBackUrl.Replace("TEMPT", UrlEncoder.Default.Encode(token));
        var sendResult = await _mailService.SendMail(user.Email, subject, url, mailType);
        return sendResult;
    }

    public async Task<string[]?> GenerateTokenAsync(AppUser user)
    {
        var avatar = await _unitOfWork.Repository<MediaAsset>().GetBySpecificationAsync(new MediaByUserIdSpec(user.Id));
        //Add information of user
        var claims = new List<Claim>()
        {
            new Claim("Id", user.Id),
            new Claim("Username", user.UserName),
            new Claim("Fullname", user.Fullname),
            new Claim("Avatar", avatar == null ? "" : avatar.Url),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        if (user.Role == Role.COACH) claims.Add(new Claim("IsVerified", user.IsVerified.Value.ToString()));
        
        //Encode secret key
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:AccessTokenSecret"]));
                    
        //Create credential
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        //Describe access token
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateConverter.AddFromStartDate(_configuration["Jwt:AccessTokenExpirationTime"]
                ,_configuration["Jwt:AccessTokenExpUnit"], DateTime.UtcNow),
            SigningCredentials = credentials,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        
        // Generate a new refresh token
        var refreshTokenString = CreateRandomString();
        
        //Save new refresh token to db
        var refreshTokenModel = new RefreshToken()
        {
            JwtId = accessToken.Id,
            Token = refreshTokenString,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateConverter.AddFromStartDate(_configuration["Jwt:RefreshTokenExpirationTime"],
                _configuration["Jwt:RefreshTokenExpUnit"], DateTime.UtcNow),
            IsRevoked = false,
            IsUsed = false,
            UserId = user.Id
        };

        _unitOfWork.Repository<RefreshToken>().Add(refreshTokenModel);
        await _unitOfWork.CompleteAsync();
        
        return new[] { tokenHandler.WriteToken(accessToken), refreshTokenString };
    }

    public async Task<string[]?> RefreshLoginAsync(string accessToken, string refreshToken)
    {
        var tokenValidationParameter = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:AccessTokenSecret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
            var tokenInVerification = tokenHandler.ValidateToken(accessToken, tokenValidationParameter,
                out var validatedToken);
            
            // Check access token algorithm
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, 
                    StringComparison.InvariantCultureIgnoreCase);
                if (result is false)
                {
                    return null;
                }
            }
            
            // Check access token expiry date
            var utcExpiryDate = long.Parse(tokenInVerification.Claims
                .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)!.Value);
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expiryDate = dateTimeVal.AddSeconds(utcExpiryDate).ToUniversalTime();
            if (expiryDate > DateTime.UtcNow)
            {
                return null;
            }
            
            // Get refresh token from db
            var validatedRefreshToken = await GetValidatedRefreshToken(refreshToken);
            if (validatedRefreshToken == null)
            {
                return null;
            }
            
            // Check jti of access token in db
            var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)!.Value;

            if (validatedRefreshToken.JwtId != jti)
            {
                return null;
            }
            
            // Set using of this token
            validatedRefreshToken.IsUsed = true;
            _unitOfWork.Repository<RefreshToken>().Update(validatedRefreshToken);
            await _unitOfWork.CompleteAsync();

            return await GenerateTokenAsync(validatedRefreshToken.User);
    }

    private string CreateRandomString()
    {
        var random = new Random();
        var guid = Guid.NewGuid();
        var chars = guid + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" + DateTime.UtcNow.ToShortDateString();
        return new string(
            Enumerable.
                Repeat(chars, 100).
                Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private async Task<RefreshToken?> GetValidatedRefreshToken(string refreshToken)
    {
        var existedToken = await _unitOfWork.Repository<RefreshToken>()
            .GetBySpecificationAsync(new RefreshTokenByTokenWithUserSpec(refreshToken));
        
        // Check refresh token in db
        if (existedToken == null)
        {
            return null;
        }

        // Check refresh token is used
        if (existedToken.IsUsed)
        {
            return null;
        }

        // Check refresh token is revoked
        if (existedToken.IsRevoked)
        {
            return null;
        }
            
        // Check expiry date of refresh token
        if (existedToken.ExpiryDate < DateTime.UtcNow)
        {
            return null;
        }

        return existedToken;
    }

    public async Task<bool> UpdateUserAsync(AppUser user)
    {
        var res = await _userManager.UpdateAsync(user);
        return res.Succeeded;
    }

    public void LogoutAll(AppUser user)
    {
        _unitOfWork.Repository<RefreshToken>()
            .DeleteRange(new Specification<RefreshToken>(t => t.UserId == user.Id));
    }

    public Task<AppUser?> GetUserAsync(ISpecification<AppUser> specification)
    {
        return _userManager.FindBySpecAsync(specification);
    }

    public Task<IReadOnlyList<AppUser>> ListUserAsync(ISpecification<AppUser> specification)
    {
        return _userManager.ListAsync(specification);
    }

    public Task<int> CountUsersAsync(ISpecification<AppUser> specification)
    {
        return _userManager.CountAsync(specification);
    }

    public async Task<bool> DeleteUserAsync(AppUser user)
    {
        var res = await _userManager.DeleteAsync(user);
        return res.Succeeded;
    }

    public Task<int> AddDefaultAvatarAsync(string userId)
    {
        try
        {
            _unitOfWork.Repository<MediaAsset>().Add(new MediaAsset()
            {
                UserId = userId,
                PublicId = "ICoaching-Photos/base-ava_dp9skj.jpg",
                Url = "https://res.cloudinary.com/dh8i0cvtv/image/upload/v1678204037/ICoaching-Photos/base-ava_dp9skj.jpg",
                IsAvatar = true
            });
            return _unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return Task.FromResult(0);
        }
    }

    public async Task<MediaAsset> GetAvatar(string userId)
    {
        var u = await GetUserAsync(new UserWithAvatarSpec(userId));
        return u.MediasAssets.First();
    }
}