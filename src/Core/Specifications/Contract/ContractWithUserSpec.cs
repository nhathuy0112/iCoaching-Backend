using System.Linq.Expressions;
using Core.Entities.Status;

namespace Core.Specifications.Contract
{
    public class ContractWithUserSpec : Specification<Entities.Contract>
    {
        public ContractWithUserSpec(PaginationParam param, string userId, ContractStatus status, bool isFilter, bool forCoach) :
            base(c =>
                (string.IsNullOrEmpty(param.Search) ||
                 (forCoach ? c.Client.Fullname.ToLower().Contains(param.Search) : c.Coach.Fullname.ToLower().Contains(param.Search)) ||
                (forCoach ? c.Client.Email.ToLower().Contains(param.Search) : c.Coach.Email.ToLower().Contains(param.Search))) &&
                c.Status == status &&
                (forCoach? c.CoachId == userId : c.ClientId == userId))
        {
            if (forCoach) AddInclude(c => c.Client);
            else AddInclude(c => c.Coach);
            if (!isFilter) return;
            ApplyPaging(param.PageSize * (param.PageIndex - 1),
                param.PageSize);
        }

        public ContractWithUserSpec(int id, bool getCoach) : base(c => c.Id == id)
        {
            if (getCoach) AddInclude(c => c.Coach);
            else AddInclude(c => c.Client);
        }
    }
}
