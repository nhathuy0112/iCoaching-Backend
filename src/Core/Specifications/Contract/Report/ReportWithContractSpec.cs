using System.Linq.Expressions;

namespace Core.Specifications.Contract.Report;

public class ReportWithContractSpec : Specification<Entities.Report>
{
    public ReportWithContractSpec(int id) : base(r => r.Id == id)
    {
        AddInclude(r => r.Contract);
    }
}