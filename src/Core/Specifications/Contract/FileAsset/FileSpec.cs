namespace Core.Specifications.Contract.FileAsset;

public class FileSpec : Specification<Entities.FileAsset>
{
    public FileSpec(int fileId) : base(f => f.Id == fileId)
    {
    }

    public FileSpec(int fileId, int contractId) : base(f => f.Id == fileId && f.ContractId == contractId)
    {
    }
}