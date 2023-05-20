namespace Core.Specifications.Contract.FileAsset;

public class FileByContractIdSpec : Specification<Entities.FileAsset>
{
    public FileByContractIdSpec(int contractId) : base(f => f.ContractId == contractId && f.TrainingLogId == null)
    {
    }
}