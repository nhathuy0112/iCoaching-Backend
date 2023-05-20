using System.Linq.Expressions;
using Core.Entities.Status;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Core.Specifications.Contract;

public class ContractSpec : Specification<Entities.Contract>
{
    public ContractSpec(string clientId, string coachId) : base(c => c.ClientId == clientId && c.CoachId == coachId 
        && (c.Status == ContractStatus.Active))
    {
    }

    public ContractSpec(int id) : base(c => c.Id == id)
    {
    }

    public ContractSpec(string userId, int contractId) : base(c => (c.ClientId == userId || c.CoachId == userId) && c.Id == contractId)
    {
    }

    public ContractSpec(PaginationParam param, string adminUsername) : base(c => c.CreatedBy == adminUsername)
    {
        ApplyPaging(param.PageSize * (param.PageIndex - 1),
            param.PageSize);
        AddInclude(c => c.Client);
    }
}