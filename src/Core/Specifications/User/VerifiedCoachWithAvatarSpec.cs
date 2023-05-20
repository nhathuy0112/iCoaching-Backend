using System.Linq.Expressions;
using Core.Entities;
using Core.Entities.Auth;

namespace Core.Specifications.User;

public class VerifiedCoachWithAvatarSpec : Specification<AppUser>
{
    public VerifiedCoachWithAvatarSpec(PaginationParam param, bool isFilter) : base(u => 
        (string.IsNullOrEmpty(param.Search) 
         || u.Fullname.ToLower().Contains(param.Search)
         || u.Email.ToLower().Contains(param.Search)) 
        && u.Role == Role.COACH && u.IsVerified == true && !u.IsLocked)
    {
        AddInclude(u => u.MediasAssets.Where(md => md.IsAvatar == true));
        if(!isFilter) return;
        ApplyPaging(param.PageSize * (param.PageIndex -1), 
            param.PageSize);
        if (string.IsNullOrEmpty(param.Sort)) return;
        switch (param.Sort)
        {
            case "fullnameAsc":
                AddOrderBy(u => u.Fullname);
                break;
            case "fullnameDesc":
                AddOrderByDescending(u => u.Fullname);
                break;
            default:
                AddOrderBy(n => n.Fullname);
                break;
        }
    }
}