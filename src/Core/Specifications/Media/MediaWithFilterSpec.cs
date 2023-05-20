using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specifications.Media
{
    public class MediaWithFilterSpec :Specification<MediaAsset>
    {
        public MediaWithFilterSpec(PaginationParam param, bool onPortfolio) : 
            base(p =>p.OnPortfolio == onPortfolio)
        {
            AddOrderBy(u => u.Id);
            ApplyPaging(param.PageSize * (param.PageIndex - 1),
                param.PageSize);
        }
        public MediaWithFilterSpec(PaginationParam param, bool onPortfolio, bool isAvatar) : 
            base(p => p.OnPortfolio == onPortfolio && p.IsAvatar == isAvatar)
        {
            AddOrderBy(u => u.Id);
            ApplyPaging(param.PageSize * (param.PageIndex - 1),
                param.PageSize);
        }
        public MediaWithFilterSpec(PaginationParam param, string userId, bool onPortfolio, bool isAvatar, bool isFilter) :
            base(p => (p.OnPortfolio == onPortfolio && p.IsAvatar == isAvatar && p.UserId == userId))
        {
            if (!isFilter) return;
            AddOrderBy(u => u.Id);
            ApplyPaging(param.PageSize * (param.PageIndex - 1),
                param.PageSize);
        }

        // For coach portfolio photos
        public MediaWithFilterSpec(PaginationParam param, string userId, bool isFilter) : base(p => p.OnPortfolio == true && p.UserId == userId)
        {
            if (!isFilter) return;
            ApplyPaging(param.PageSize * (param.PageIndex - 1),
                param.PageSize);
        }
    }
}
