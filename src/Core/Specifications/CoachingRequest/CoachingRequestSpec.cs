using System.Linq.Expressions;
using Core.Entities.Status;

namespace Core.Specifications.CoachingRequest;

public class CoachingRequestSpec : Specification<Entities.CoachingRequest>
{
    public CoachingRequestSpec(string clientId, int courseId) : base(cq => cq.ClientId == clientId && cq.CourseId == courseId)
    {
    }

    public CoachingRequestSpec(string clientId, string coachId) : base(cq => 
        cq.ClientId == clientId && cq.CoachId == coachId 
        && (cq.Status == CoachingRequestStatus.Pending || cq.Status == CoachingRequestStatus.Init))
    {
    }

    public CoachingRequestSpec(string userId, PaginationParam param) : base(cq => cq.ClientId == userId || cq.CoachId == userId)
    {
    }

    public CoachingRequestSpec(int id) : base(cq => cq.Id == id)
    {
    }
}