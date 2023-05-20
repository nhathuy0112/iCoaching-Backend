using System.Linq.Expressions;
using Core.Entities;
using Core.Entities.Auth;

namespace Core.Specifications.User;

public class UserByRoleWithAvatarSpec : Specification<AppUser>
{
    public UserByRoleWithAvatarSpec(PaginationParam param, Role role, bool isFilter) : base(u => 
        (string.IsNullOrEmpty(param.Search) 
         || u.Fullname.ToLower().Contains(param.Search)
         || u.UserName.ToLower().Contains(param.Search) 
         || u.Email.ToLower().Contains(param.Search)) && u.Role == role)
    {
        AddInclude(u => u.MediasAssets.Where(md => md.IsAvatar == true));
        if(!isFilter) return;
        AddOrderByDescending(u => u.WarningCount);
        ApplyPaging(param.PageSize * (param.PageIndex -1), 
            param.PageSize);
    }

    public UserByRoleWithAvatarSpec(Role role, string id) : base(u => u.Id == id && u.Role == role)
    {
        AddInclude(u => u.MediasAssets.Where(m => m.IsAvatar == true));
    }
}