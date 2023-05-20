using Core.Entities.Status;

namespace Core.Specifications.Contract;

public class ContractByUserIdWithReportSpec : Specification<Entities.Contract>
{
    public ContractByUserIdWithReportSpec(string userId, int contractId) : base(
        c => (c.ClientId == userId || c.CoachId == userId) && c.Id == contractId)
    {
        AddInclude(c => c.Reports.Where(r => r.ReportStatus == ReportStatus.Pending));
    }
}