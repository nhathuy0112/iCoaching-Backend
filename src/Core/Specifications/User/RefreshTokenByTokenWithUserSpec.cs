using System.Linq.Expressions;
using Core.Entities.Auth;

namespace Core.Specifications.User;

public class RefreshTokenByTokenWithUserSpec : Specification<RefreshToken>
{
    public RefreshTokenByTokenWithUserSpec(string token) : base(t => t.Token == token)
    {
        AddInclude(t => t.User);
    }
}