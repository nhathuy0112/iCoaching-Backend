using Core.Entities.Status;

namespace Core.Specifications.Contract.Report;

public class ReportByStatusWithMediaSpec : Specification<Entities.Report>
{
    public ReportByStatusWithMediaSpec(PaginationParam param, ReportStatus status ,bool isFilter) : base(r => r.ReportStatus == status)
    {
        if (!isFilter) return;
        AddInclude(r => r.MediaAssets);
        ApplyPaging(param.PageSize * (param.PageIndex - 1),
            param.PageSize);
        AddOrderBy(r => r.CreatedDate);
    }
}