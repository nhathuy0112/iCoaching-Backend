using Core.Entities;
using Core.Entities.Status;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Specifications;
using Core.Specifications.CoachingRequest;

namespace Infrastructure.Services;

public class TrainingService : ITrainingService
{
    private readonly IUnitOfWork _unitOfWork;

    public TrainingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CancelTimeoutRequestAsync(int requestId, CoachingRequestStatus status)
    {
        var coachingRequest =
            await _unitOfWork.Repository<CoachingRequest>().GetBySpecificationAsync(new CoachingRequestSpec(requestId));
        if (coachingRequest == null) return;
        if (coachingRequest!.Status != status) return;
        coachingRequest.Status = CoachingRequestStatus.Canceled;
        coachingRequest.CancelReason = "Hết hạn";
        //Reset voucher
        if (coachingRequest.VoucherCode != null)
        {
            var voucher = await _unitOfWork.Repository<Voucher>()
                .GetBySpecificationAsync(new Specification<Voucher>(v => v.Code == coachingRequest.VoucherCode));
            voucher!.IsUsed = false;
            _unitOfWork.Repository<Voucher>().Update(voucher);
        }
        _unitOfWork.Repository<CoachingRequest>().Update(coachingRequest);
        await _unitOfWork.CompleteAsync();
    }
    
}