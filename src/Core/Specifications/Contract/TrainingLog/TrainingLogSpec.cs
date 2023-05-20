namespace Core.Specifications.Contract.TrainingLog;

public class TrainingLogSpec : Specification<Entities.TrainingLog>
{
    public TrainingLogSpec(int id) : base(tl => tl.Id == id)
    {
    }

    public TrainingLogSpec(int logId, int contractId) : base(tl => tl.Id == logId && contractId == tl.ContractId)
    {
    }
}