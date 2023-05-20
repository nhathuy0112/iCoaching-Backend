using System.Linq.Expressions;
using Core.Entities;

namespace Core.Specifications.User;

public class UserWithAvatarSpec : Specification<AppUser>
{
    public UserWithAvatarSpec(string userId) : base(u => u.Id == userId)
    {
        AddInclude(u => u.MediasAssets.Where(m => m.IsAvatar == true));
    }
}