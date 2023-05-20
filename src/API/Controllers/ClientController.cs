using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using API.Dto.Client;
using API.Dto.CoachingRequest;
using API.Dto.Contract;
using API.Dto.Contract.Status;
using API.ErrorResponses;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Entities.Status;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Interfaces.User;
using Core.Specifications;
using Core.Specifications.CoachingRequest;
using Core.Specifications.Contract;
using Core.Specifications.TrainingCourse;
using Hangfire;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = "CLIENT")]
public class ClientController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IVnPayService _vnPayService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ClientController(IUserService userService, IMapper mapper, IVnPayService vnPayService, IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _mapper = mapper;
        _vnPayService = vnPayService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("coaching-request")]
    public async Task<IActionResult> RequestCoaching([Required(ErrorMessage = "Vui lòng nhập id của huấn luyện viên")] string coachId, 
        [Required(ErrorMessage = "Vui lòng nhập id của khoá tập")] int courseId, 
        string? voucherCode, 
        [FromBody] [Required(ErrorMessage = "Vui lòng nhập lời nhắn")] string message)
    {
        var coach = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == coachId && u.IsVerified == true));
        if (coach == null) return BadRequest(new ErrorResponse(400, "Huấn luyện viên không tồn tại"));
        
        var course = await _unitOfWork.Repository<TrainingCourse>().GetBySpecificationAsync(new TrainingCourseSpec(coachId, courseId));
        if (course == null) return BadRequest(new ErrorResponse(400, "Khoá tập không tồn tại"));

        var clientId = User.FindFirstValue("Id");
        
        var existedRequest =
            await _unitOfWork.Repository<CoachingRequest>().CountAsync(new CoachingRequestSpec(clientId, coachId));

        if (existedRequest != 0) return BadRequest(new ErrorResponse(400, "Bạn đã tạo 1 yêu cầu trước đó"));

        var contractCount = await _unitOfWork.Repository<Contract>().CountAsync(new ContractSpec(clientId, coachId));
        if (contractCount != 0) return BadRequest(new ErrorResponse(400, "Bạn đang tập luyện khoá này"));

        
        var newRequest = new CoachingRequest()
        {
            ClientId = clientId,
            CoachId = coachId,
            CourseId = courseId,
            Status = CoachingRequestStatus.Init,
            ClientMessage = message,
        };
        
        if (voucherCode != null)
        {
            var voucherRepo = _unitOfWork.Repository<Voucher>();
            var voucher = await voucherRepo.GetBySpecificationAsync(new Specification<Voucher>(v => v.Code == voucherCode && v.ClientId == clientId));
            
            if (voucher == null || voucher.IsUsed) 
                return BadRequest(new ErrorResponse(400, "Voucher không tồn tại hoặc đã được sử dụng"));
            
            if (voucher.Discount == 100) newRequest.Status = CoachingRequestStatus.Pending;
            
            else
            {
                var priceToPay = course.Price - course.Price * voucher.Discount / 100;
                if (priceToPay < 10000)
                    return BadRequest(new ErrorResponse(400, 
                        $"Vui lòng chọn voucher khác. Số tiền cần phải thanh toán: {CurrencyHelper.GetVnd(priceToPay)}.\n" +
                        $"Hạn mức thanh toán tối thiểu là 10,000 VNĐ."));
            }
            newRequest.Discount = voucher.Discount;
            newRequest.VoucherCode = voucher.Code;
            voucher.IsUsed = true;
            voucherRepo.Update(voucher);
        }
        
        _unitOfWork.Repository<CoachingRequest>().Add(newRequest);
        var res = await _unitOfWork.CompleteAsync();
        if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

        
        // Schedule for auto cancel after x day if client does not pay
        var expDate = DateConverter.AddFromStartDate(_configuration["Payment:InitExpTime"],
            _configuration["Payment:InitExpUnit"], DateTime.Now);
        BackgroundJob.Schedule<ITrainingService>( 
            x => x.CancelTimeoutRequestAsync(newRequest.Id, CoachingRequestStatus.Init), expDate);
        
        return Ok($"Đăng kí thành công. Thời gian hết hạn khi không thực hiện thanh toán: {expDate.ToString("dd/MM/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo)}");
        
    }

    [HttpGet("coaching-requests")]
    public async Task<ActionResult<Pagination<CoachingRequestForClient>>> GetCoachingRequests([FromQuery] PaginationParam param, 
        [FromQuery] [Required(ErrorMessage = "Vui lòng chọn trạng thái")] ClientRequestStatus clientRequestStatus)
    {
        var clientId = User.FindFirstValue("Id");
        var repo = _unitOfWork.Repository<CoachingRequest>();
        var coachingRequests = await repo.ListAsync(new CoachingRequestWithUserAndCourseSpec(clientId, param, clientRequestStatus.ToString(),false, true));
        var data =
            _mapper.Map<IReadOnlyList<CoachingRequest>, IReadOnlyList<CoachingRequestForClient>>(coachingRequests);
        var count = await repo.CountAsync(new CoachingRequestWithUserAndCourseSpec(clientId, param, clientRequestStatus.ToString(),false, false));
        return Ok(new Pagination<CoachingRequestForClient>()
        {
            PageIndex = param.PageIndex,
            PageSize = param.PageSize,
            Count = count,
            Data = data
        });
    }

    [HttpPut("coaching-request-cancellation/{id:int}")]
    public async Task<IActionResult> CancelCoachingRequest(int id, 
        [FromBody] [Required(ErrorMessage = "Vui lòng nhập lí do huỷ")] string reason = "")
    {
        var repo = _unitOfWork.Repository<CoachingRequest>();
        var clientId = User.FindFirstValue("Id");
        var coachingRequest = await repo.GetBySpecificationAsync(new CoachingRequestSpec(id));
        
        if (coachingRequest == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
        
        if (coachingRequest.ClientId != clientId || 
            coachingRequest.Status is not 
                (CoachingRequestStatus.Pending or CoachingRequestStatus.Init)) 
            return BadRequest(new ErrorResponse(400, "Không hợp lệ"));

        //Reset voucher
        if (coachingRequest.VoucherCode != null)
        {
            var voucherRepo = _unitOfWork.Repository<Voucher>();
            var voucher = await voucherRepo.GetBySpecificationAsync(new Specification<Voucher>(v => v.Code == coachingRequest.VoucherCode));
            voucher!.IsUsed = false;
            voucherRepo.Update(voucher);
        }
        
        coachingRequest.Status = CoachingRequestStatus.Canceled;
        coachingRequest.CancelReason = reason;
        repo.Update(coachingRequest);
        
        var res = await _unitOfWork.CompleteAsync();
        
        if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

        return Ok("Huỷ thành công");
    }

    [HttpGet("coaching-request/{id:int}/payment-url")]
    public async Task<IActionResult> GetPaymentUrl(int id)
    {
        var clientId = User.FindFirstValue("Id");
        var coachingRequest =
            await _unitOfWork.Repository<CoachingRequest>().GetBySpecificationAsync(new CoachingRequestWithCourseSpec(id, clientId));
        if (coachingRequest == null)
            return BadRequest(new ErrorResponse(400, "Không tìm thấy yêu cầu hoặc yêu cầu không hợp lệ"));

        var ipAddress = IPHelper.GetIpAddress(HttpContext);
        var callBackUrl = $"{Request.Scheme}://{Request.Host}/Payment/result/{id}/{clientId}";

        var url = _vnPayService.CreatePaymentUrl(coachingRequest, callBackUrl, ipAddress);
        return Ok(url);
    }

    [HttpGet("contracts")]
    public async Task<ActionResult<Pagination<ContractForClient>>> GetContract([FromQuery] PaginationParam param, 
        [Required(ErrorMessage = "Vui lòng chọn trạng thái hợp đồng")] ContractStatusDto statusDto)
    {
        var status = Enum.Parse<ContractStatus>(statusDto.ToString());
        var clientId = User.FindFirstValue("Id");
        var contracts =
            await _unitOfWork.Repository<Contract>().ListAsync(new ContractWithUserSpec(param, clientId, status, true, false));
        var count = await _unitOfWork.Repository<Contract>().CountAsync(new ContractWithUserSpec(param, clientId, status, false,
            false));
        var data = _mapper.Map<IReadOnlyList<Contract>, IReadOnlyList<ContractForClient>>(contracts);
        return Ok(new Pagination<ContractForClient>()
        {
            PageIndex = param.PageIndex,
            PageSize = param.PageSize,
            Data = data,
            Count = count
        });
    }

    [HttpGet("vouchers")]
    public async Task<ActionResult<IReadOnlyList<VoucherDto>>> GetVouchers()
    {
        var clientId = User.FindFirstValue("Id");
        var vouchers = await _unitOfWork.Repository<Voucher>()
            .ListAsync(new Specification<Voucher>(v => v.ClientId == clientId));
        var data = _mapper.Map<IReadOnlyList<Voucher>, IReadOnlyList<VoucherDto>>(vouchers);
        return Ok(data);
    }

    [HttpPut("contract/{contractId:int}/completion")]
    public async Task<IActionResult> CompleteContract(int contractId)
    {
        var clientId = User.FindFirstValue("Id");
        var repo = _unitOfWork.Repository<Contract>();
        var contract = await repo.GetBySpecificationAsync(new ContractByUserIdWithReportSpec(clientId, contractId));
        if (contract == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy"));
        if (contract.Reports.Count != 0)
            return BadRequest(new ErrorResponse(400, "Khiếu nại đang chờ được giải quyết"));
        if (contract.Status != ContractStatus.Pending) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
        contract.Status = ContractStatus.Complete;
        repo.Update(contract);
        var res = await _unitOfWork.CompleteAsync();
        if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

        return Ok("Hợp đồng đã được hoàn thành");
    }

}