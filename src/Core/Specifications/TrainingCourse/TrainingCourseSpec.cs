using System.Linq.Expressions;

namespace Core.Specifications.TrainingCourse;

public class TrainingCourseSpec : Specification<Entities.TrainingCourse>
{
    //For filter
    public TrainingCourseSpec(string coachId, PaginationParam param, bool isFilter) : base(c => c.CoachId == coachId 
        && (string.IsNullOrEmpty(param.Search) || c.Name.ToLower().Contains(param.Search)))
    {
        if(!isFilter) return;
        AddOrderBy(c => c.Price);
        ApplyPaging(param.PageSize * (param.PageIndex -1), 
            param.PageSize);
        if (string.IsNullOrEmpty(param.Sort)) return;
        switch (param.Sort)
        {
            case "nameAsc":
                AddOrderBy(c => c.Name);
                break;
            case "nameDesc":
                AddOrderByDescending(c => c.Name);
                break;
            case "priceDesc":
                AddOrderByDescending(c => c.Price);
                break;
            default:
                AddOrderBy(c => c.Price);
                break;
        }
    }

    //For detail
    public TrainingCourseSpec(string coachId, int courseId) : base(c => c.Id == courseId && c.CoachId == coachId)
    {
    }
}