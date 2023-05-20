using Core.Entities;
using Core.Entities.Payment;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces;

public interface IVnPayService
{
    string CreatePaymentUrl(CoachingRequest coachingRequest, string callBackUrl, string ipAddress);
    PaymentResponse GetFullResponseData(IQueryCollection collection);
}