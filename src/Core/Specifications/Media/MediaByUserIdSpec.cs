using System.Linq.Expressions;
using Core.Entities;

namespace Core.Specifications.Media;

public class MediaByUserIdSpec : Specification<MediaAsset>
{
    // For avatar
    public MediaByUserIdSpec(string userId) : base(m => m.UserId == userId && m.IsAvatar == true)
    {
    }

    public MediaByUserIdSpec(string userId, bool onPortfolio) : base(m => m.UserId == userId && m.IsAvatar == false && m.OnPortfolio == onPortfolio)
    {
        AddOrderByDescending(m => m.Id);
    }
}