using System.Linq.Expressions;

namespace Core.Specifications.Contract.Report;

public class ReportSpec : Specification<Entities.Report>
{
    public ReportSpec(int id) : base(r => r.Id == id)
    {
    }
}