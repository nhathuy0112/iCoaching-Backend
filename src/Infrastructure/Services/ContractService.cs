using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Core.Entities.Status;
using Core.Interfaces.Base;
using Core.Specifications.Contract;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        public ContractService(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public Task<int> CreateContractFromCoachingRequestAsync(CoachingRequest coachingRequest)
        {
            if (coachingRequest.Status != CoachingRequestStatus.Pending) return Task.FromResult(0);
            
            var newContract = new Contract()
            {
                CoachId = coachingRequest.CoachId,
                ClientId = coachingRequest.ClientId,
                CourseName = coachingRequest.Course.Name,
                Duration = coachingRequest.Course.Duration,
                Price = coachingRequest.Course.Price,
                CourseDescription = coachingRequest.Course.Description,
                CreatedDate = DateTime.Now,
                Status = ContractStatus.Active,
                Logs = new List<TrainingLog>()
            };
            for (var i = 1; i <= newContract.Duration; i++)
            {
                newContract.Logs.Add(new TrainingLog()
                {
                    DateNo = i,
                    Status = TrainingLogStatus.Init
                });
            }
            _unitOfWork.Repository<Contract>().Add(newContract);
            _unitOfWork.Repository<CoachingRequest>().Delete(coachingRequest);
            return _unitOfWork.CompleteAsync();
        }

        public async Task<bool> SaveFileAsync(IFormFile file, FileAsset fileAssetOption)
        {
            var url = await _fileService.UploadAsync(file);
            if (url == null) return false;
            var newFileAsset = new FileAsset()
            {
                FileName = file.FileName,
                DownloadUrl = url,
                ContractId = fileAssetOption.ContractId,
                TrainingLogId = fileAssetOption.TrainingLogId,
                Size = file.Length,
                Date = DateTime.Now
            };
            _unitOfWork.Repository<FileAsset>().Add(newFileAsset);
            var res = await _unitOfWork.CompleteAsync();
            return res != 0;
        }
        
        public async Task<bool> DeleteFileAsync(FileAsset fileAsset)
        {
            var deleteRes = await _fileService.DeleteAsync(fileAsset.DownloadUrl);
            if (!deleteRes) return false;
            _unitOfWork.Repository<FileAsset>().Delete(fileAsset);
            var res = await _unitOfWork.CompleteAsync();
            return res != 0;
        }

        public async Task<bool> IsValidTrainingDateAsync(int contractId, int currentLogDate, DateTime trainingDate)
        {
            var contract = await _unitOfWork.Repository<Contract>()
                .GetBySpecificationAsync(new ContractSpec(contractId));
            if (contract == null) return false;
            if (contract.CreatedDate.Date > trainingDate.Date) return false;
            if (currentLogDate == 1) return true;
            var preLogDate = await _unitOfWork.Repository<TrainingLog>().GetBySpecificationAsync(new Specification<TrainingLog>(tl =>
                tl.ContractId == contractId && tl.DateNo == currentLogDate - 1));
            if (preLogDate!.TrainingDate == null) return false;
            return preLogDate!.TrainingDate < trainingDate;
        }

        public async Task<AppUser> GetClientByContractIdAsync(int contractId)
        {
            var contract = await  _unitOfWork.Repository<Contract>().GetBySpecificationAsync(new ContractWithUserSpec(contractId, false));
            return contract!.Client;
        }

        public async Task<int> CreateContractByAdminAsync(Report report, Contract newContractOption, TrainingCourse course, int oldContractId, string? cancelReason)
        {
            var oldContract = await _unitOfWork.Repository<Contract>()
                .GetBySpecificationAsync(new ContractSpec(oldContractId));
            oldContract!.Status = ContractStatus.Canceled;
            oldContract.CancelReason = string.IsNullOrEmpty(cancelReason) ? "Bị khiếu nại" : cancelReason;
            _unitOfWork.Repository<Contract>().Update(oldContract);
            
            var newContract = new Contract()
            {
                CoachId = newContractOption.CoachId,
                ClientId = newContractOption.ClientId,
                CourseName = course.Name,
                Duration = course.Duration,
                Price = course.Price,
                CourseDescription = course.Description,
                Description = newContractOption.Description,
                Status = ContractStatus.Active,
                CreatedDate = DateTime.Now,
                CreatedBy = newContractOption.CreatedBy,
                Logs = new List<TrainingLog>()
            };

            for (var i = 1; i <= newContract.Duration; i++)
            {
                newContract.Logs.Add(new TrainingLog()
                {
                    DateNo = i,
                    Status = TrainingLogStatus.Init
                });
            }
            
            _unitOfWork.Repository<Contract>().Add(newContract);

            report.ReportStatus = ReportStatus.Solved;
            report.SolutionDesc = $"Tạo hợp đồng mới";
            _unitOfWork.Repository<Report>().Update(report);

            return await _unitOfWork.CompleteAsync();
        }

        public Task<Stream?> GetFileDownloadingStream(string downloadUrl)
        {
            return _fileService.DownloadAsync(downloadUrl);
        }
        
        public async Task<int> AutoCompleteContractAsync(int contractId)
        {
            var repo = _unitOfWork.Repository<Contract>();
            var contract = await repo.GetBySpecificationAsync(new ContractSpec(contractId));
            if (contract!.Status != ContractStatus.Pending) return 0;
            contract.Status = ContractStatus.Complete;
            repo.Update(contract);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> IsLogCompleteAsync(int contractId)
        {
            var logCount = await _unitOfWork.Repository<TrainingLog>().CountAsync(
                new Specification<TrainingLog>(l => l.ContractId == contractId && l.Status == TrainingLogStatus.Init));
            return logCount == 0;
        }

        public async Task<bool> IsReportedAsync(int contractId)
        {
            var reportCount = await _unitOfWork.Repository<Report>().CountAsync(
                new Specification<Report>(r => r.ContractId == contractId && r.ReportStatus == ReportStatus.Pending));
            return reportCount != 0;
        }
    }
}
