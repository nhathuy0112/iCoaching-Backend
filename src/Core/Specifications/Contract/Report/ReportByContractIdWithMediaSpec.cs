namespace Core.Specifications.Contract.Report;

public class ReportByContractIdWithMediaSpec : Specification<Entities.Report>
{
    public ReportByContractIdWithMediaSpec(int contractId) : base(r => r.ContractId == contractId)
    {
        AddInclude(r => r.MediaAssets);
        AddOrderBy(r => r.CreatedDate);
    }
}