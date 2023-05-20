using System.Linq.Expressions;

namespace Core.Specifications.Contract;

public class ContractByUserWithCoachAndClientSpec : Specification<Entities.Contract>
{
    public ContractByUserWithCoachAndClientSpec(int id, string userId, bool forAdmin) : base(c => c.Id == id 
        && (forAdmin || (c.ClientId == userId || c.CoachId == userId)) )
    {
        AddInclude(c => c.Client);
        AddInclude(c => c.Coach);
    }
}