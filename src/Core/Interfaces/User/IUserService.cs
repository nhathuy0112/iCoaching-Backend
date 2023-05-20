using Core.Entities;
using Core.Specifications;

namespace Core.Interfaces.User;

public interface IUserService
{
    Task<bool> RegisterAsync(AppUser newUser, string password);
    Task<AppUser?> LoginAsync(string username, string password);
    Task<string[]?> GenerateTokenAsync(AppUser user);
    Task<string[]?> RefreshLoginAsync(string accessToken, string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    Task<bool> ConfirmEmailAsync(AppUser user, string token);
    Task<bool> SendUrlAsync(AppUser user, string callBackUrl, string type);
    Task<bool> ResetPasswordAsync(AppUser user, string token, string newPassword);
    Task<bool> UpdateUserAsync(AppUser user);
    void LogoutAll(AppUser user);
    Task<AppUser?> GetUserAsync(ISpecification<AppUser> specification);
    Task<IReadOnlyList<AppUser>> ListUserAsync(ISpecification<AppUser> specification);
    Task<int> CountUsersAsync(ISpecification<AppUser> specification);
    Task<bool> DeleteUserAsync(AppUser user);
    Task<int> AddDefaultAvatarAsync(string userId);
    Task<MediaAsset> GetAvatar(string userId);
}