using Core.Entities;
using Core.Entities.Auth;

namespace Core.Specifications.Media
{
    public class MediaWithUserSpec : Specification<MediaAsset>
    {
        public MediaWithUserSpec(string publicId) : base(t => t.PublicId.Equals(publicId))
        {
            AddInclude(t => t.User);
        }

        public MediaWithUserSpec(string userId, bool? isAvatar) : 
            base(t => 
                t.UserId.Equals(userId) && 
                (!isAvatar.HasValue || t.IsAvatar == isAvatar))
        {
            AddInclude(t => t.User);
        }
        public MediaWithUserSpec(string userId, bool onPortfolio, bool isAvatar) :
            base(p => p.OnPortfolio == onPortfolio && p.IsAvatar == isAvatar && p.UserId == userId)
        {
            AddInclude(t => t.User);
        }
    }
}
