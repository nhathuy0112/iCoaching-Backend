using System.Linq.Expressions;
using Core.Entities.Status;

namespace Core.Specifications.CoachingRequest;

public class CoachingRequestWithCourseSpec : Specification<Entities.CoachingRequest>
{
    public CoachingRequestWithCourseSpec(int id, string clientId) : base(cq => 
        cq.Id == id && cq.Status == CoachingRequestStatus.Init && cq.ClientId == clientId)
    {
        AddInclude(cq => cq.Course);
    }

    public CoachingRequestWithCourseSpec(int id) : base(cq => cq.Id == id)
    {
        AddInclude(cq => cq.Course);
    }
}