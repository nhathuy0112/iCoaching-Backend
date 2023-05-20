using Core.Entities;
using Core.Entities.Status;
using Core.Specifications;

namespace Core.Interfaces;

public interface ITrainingService
{
    Task CancelTimeoutRequestAsync(int requestId, CoachingRequestStatus status);
}