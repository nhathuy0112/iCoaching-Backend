using System.Globalization;
using Core.Entities;
using Core.Entities.Status;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Specifications;
using Core.Specifications.CoachingRequest;
using Hangfire;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("Payment")]
[ApiExplorerSettings(IgnoreApi = true)]
public class PaymentController : Controller
{
    private readonly IVnPayService _vnPayService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    
    public PaymentController(IVnPayService vnPayService, IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _vnPayService = vnPayService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }
    
    [Route("result/{requestId:int}/{userId}")]
    public async Task<IActionResult> Index(int requestId, string userId)
    {
        var repo = _unitOfWork.Repository<CoachingRequest>();
        var response = _vnPayService.GetFullResponseData(Request.Query);
        var coachingRequest = await repo.GetBySpecificationAsync(new CoachingRequestSpec(requestId));
        if (coachingRequest == null || coachingRequest.ClientId != userId)
        {
            ViewBag.Message = "Không hợp lệ";
            return View();
        }

        ViewBag.GoBack = _configuration["Payment:ClientGoBack"].Replace("USERID", userId);
        
        if (response is { VnPayCallbackResult: true, VnPayResponseCode: "00" })
        {
            if (coachingRequest.Status == CoachingRequestStatus.Canceled)
            {
                ViewBag.Message = "Bạn đã huỷ yêu cầu trong lúc thanh toán. Hệ thống sẽ hoàn tiền lại cho bạn";
                return View();
            }
            coachingRequest!.Status = CoachingRequestStatus.Pending;
            
            repo.Update(coachingRequest);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0)
            {
                ViewBag.Message = "Có lỗi xảy ra";
                return View();
            }
            
            // Schedule for auto cancel after x day if coach does not response
            var expDate = DateConverter.AddFromStartDate(_configuration["Payment:PendingExpTime"],
                _configuration["Payment:PendingExpUnit"], DateTime.Now);
             BackgroundJob.Schedule<ITrainingService>( 
                 x => x.CancelTimeoutRequestAsync(requestId, CoachingRequestStatus.Pending), expDate);

             ViewBag.Message = $"Thanh toán thành công";
             ViewBag.IsSuccess = true;
        }
        else
        {
            ViewBag.Message = "Thanh toán thất bại hoặc VNPay có lỗi";
        }

        return View();
    }
}