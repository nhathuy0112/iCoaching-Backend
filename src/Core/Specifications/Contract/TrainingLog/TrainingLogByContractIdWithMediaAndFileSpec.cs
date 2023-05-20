namespace Core.Specifications.Contract.TrainingLog;

public class TrainingLogByContractIdWithMediaAndFileSpec : Specification<Entities.TrainingLog>
{
    public TrainingLogByContractIdWithMediaAndFileSpec(int contractId) : base(tl => tl.ContractId == contractId)
    {
        AddInclude(tl => tl.FileAssets);
        AddInclude(tl => tl.MediaAssets);
    }

    public TrainingLogByContractIdWithMediaAndFileSpec(int contractId, int logId) : base(tl => tl.ContractId == contractId && tl.Id == logId)
    {
        AddInclude(tl => tl.FileAssets);
        AddInclude(tl => tl.MediaAssets);
    }
}