using System.Linq.Expressions;
using Core.Entities.Status;

namespace Core.Specifications.CoachingRequest;

public class CoachingRequestWithUserAndCourseSpec : Specification<Entities.CoachingRequest>
{
    public CoachingRequestWithUserAndCourseSpec(string userId, PaginationParam param, string status, bool forCoach, bool isFilter) : base(cq => 
        ( forCoach ? cq.CoachId == userId : cq.ClientId == userId) 
        && (string.IsNullOrEmpty(param.Search) || 
            cq.Course.Name.ToLower().Contains(param.Search) ||
            ( forCoach ? cq.Client.Fullname.ToLower().Contains(param.Search) : cq.Coach.Fullname.ToLower().Contains(param.Search))) 
        && cq.Status == Enum.Parse<CoachingRequestStatus>(status))
            
    {
        if (!isFilter) return;
        AddInclude(cq => cq.Course);
        switch (forCoach)
        {
            case false:
                AddInclude(cq => cq.Coach);
                break;
            case true:
                AddInclude(cq => cq.Client);
                break;
        }

        ApplyPaging(param.PageSize * (param.PageIndex - 1), param.PageSize);
        AddOrderBy(cq => cq.Coach.Fullname);
        if (string.IsNullOrEmpty(param.Sort)) return;
        switch (param.Sort)
        {
            //Sort here
        }
    }

    public CoachingRequestWithUserAndCourseSpec(int id, bool forCoach) : base(cq => cq.Id == id)
    {
        AddInclude(cq => cq.Course);
        if(forCoach) AddInclude(cq => cq.Coach);
        else AddInclude(cq => cq.Client);
        
    }
}