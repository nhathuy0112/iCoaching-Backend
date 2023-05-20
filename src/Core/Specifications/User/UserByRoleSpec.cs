using System.Linq.Expressions;
using Core.Entities;
using Core.Entities.Auth;

namespace Core.Specifications.User;

public class UserByRoleSpec : Specification<AppUser>
{
    public UserByRoleSpec(PaginationParam param, Role role, bool isFilter) : base(u => 
        (string.IsNullOrEmpty(param.Search) 
         || u.UserName.ToLower().Contains(param.Search)
         || u.Fullname.ToLower().Contains(param.Search)
         || u.Email.ToLower().Contains(param.Search)) && u.Role == role)
    {
        if(!isFilter) return;
        AddOrderBy(u => u.Fullname);
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
            case "gender":
                AddOrderBy(u => u.Gender!);
                break;
            default:
                AddOrderBy(n => n.Fullname);
                break;
        }
    }

    public UserByRoleSpec(Role role, string id) : base(u => u.Id == id && u.Role == role)
    {
    }
}