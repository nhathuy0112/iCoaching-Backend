using Core.Entities;
using Core.Specifications;
using Core.Entities.Auth;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces
{
    public interface IContractService
    {
        Task<int> CreateContractFromCoachingRequestAsync(CoachingRequest coachingRequest);
        Task<bool> SaveFileAsync(IFormFile file, FileAsset fileAssetOption);
        Task<Stream?> GetFileDownloadingStream(string downloadUrl);
        Task<bool> DeleteFileAsync(FileAsset fileAsset);
        Task<bool> IsValidTrainingDateAsync(int contractId, int currentLogDate, DateTime trainingDate);
        Task<AppUser> GetClientByContractIdAsync(int contractId);
        Task<int> CreateContractByAdminAsync(Report report, Contract newContractOption, TrainingCourse course, int oldContractId, string? cancelReason);
        Task<int> AutoCompleteContractAsync(int contractId);
        Task<bool> IsLogCompleteAsync(int contractId);
        Task<bool> IsReportedAsync(int contractId);
    }
}
