using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API.Dto.Certification;
using API.Dto.Coach;
using API.Dto.Contract;
using API.Dto.Contract.Report;
using API.Dto.Contract.Status;
using API.ErrorResponses;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Entities.Auth;
using Core.Entities.Status;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Interfaces.User;
using Core.Specifications;
using Core.Specifications.Cert;
using Core.Specifications.Contract;
using Core.Specifications.Contract.Report;
using Core.Specifications.TrainingCourse;
using Core.Specifications.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IContractService _contractService;

        public AdminController(IMapper mapper, IUserService userService, IContractService contractService, IUnitOfWork unitOfWork) 
        {
            _mapper = mapper;
            _userService = userService;
            _contractService = contractService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("cert-requests")]
        public async Task<ActionResult<IReadOnlyList<CertRequestInfo>>> GetCertRequests([FromQuery] PaginationParam param,[Required] CertStatus status)
        {
            var repo = _unitOfWork.Repository<CertificateSubmission>();
            var certFilterSpec = new CertWithFilterSpec(param, status, true);
            var certCountSpec = new CertWithFilterSpec(param, status, false);

            var certList = await repo.ListAsync(certFilterSpec);
            foreach (var c in certList)
            {
                c.MediaAssets = new List<MediaAsset>();
                c.MediaAssets.Add(await _userService.GetAvatar(c.Coach.Id));
            }
            var count = await repo.CountAsync(certCountSpec);

            var data = _mapper.Map<IReadOnlyList<CertificateSubmission>, IReadOnlyList<CertRequestInfo>>(certList);

            return Ok(new Pagination<CertRequestInfo>()
            {
                PageIndex = param.PageIndex,
                PageSize = param.PageSize,
                Data = data,
                Count = count
            });
        }

        [HttpGet("cert-request-detail/{certId:int}")]
        public async Task<ActionResult<CertRequestDetail>> GetCertRequestsDetail(int certId)
        {
            var certDetailSpec = new CertWithMediaSpec(certId);

            var cert = await _unitOfWork.Repository<CertificateSubmission>().GetBySpecificationAsync(certDetailSpec);
            if (cert == null) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            var data = _mapper.Map<CertificateSubmission, CertRequestDetail>(cert);

            return Ok(data);
        }

        [HttpPut("cert-request-status/{id:int}")]
        public async Task<IActionResult> UpdateCoachCertRequestStatus(int id, [Required] StatusOption option, [FromBody] string reason = "")
        {
            var repo = _unitOfWork.Repository<CertificateSubmission>();
            var cert = await repo.GetBySpecificationAsync(
                new Specification<CertificateSubmission>(c => c.Id == id));
            if (cert == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
            if (cert.Status != CertStatus.Pending) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            
            //Update cert status
            if (option == StatusOption.Denied && string.IsNullOrEmpty(reason))
                return BadRequest(new ErrorResponse(400, "Vui lòng điền lý do từ chối"));
            
            var status = Enum.Parse<CertStatus>(option.ToString());
            cert.Status = status;
            cert.Reason = reason;
            repo.Update(cert);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0) return StatusCode(500, "Có lỗi xảy ra");
            
            if (status == CertStatus.Denied) return Ok("Cập nhật thành cộng");
            
            //Update coach
            var coach = await _userService.GetUserAsync(new Specification<AppUser>(u =>
                u.Id == cert.CoachId));
            coach!.IsVerified = true;
            var coachUpdatedRes = await _userService.UpdateUserAsync(coach);
            if (!coachUpdatedRes) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Cập nhật thành công");
        }

        [HttpGet("coaches")]
        public async Task<ActionResult<Pagination<CoachListInfoForAdmin>>> GetCoaches([FromQuery] PaginationParam param)
        {
            var coaches = await _userService.ListUserAsync(new UserByRoleWithAvatarSpec(param, Role.COACH, true));
            var count = await _userService.CountUsersAsync(new UserByRoleSpec(param, Role.COACH, false));
            var data = _mapper.Map<IReadOnlyList<AppUser>, IReadOnlyList<CoachListInfoForAdmin>>(coaches);
            return Ok(new Pagination<CoachListInfoForAdmin>()
            {
                Count = count,
                PageIndex = param.PageIndex,
                PageSize = param.PageSize,
                Data = data
            });
        }

        [HttpPut("coach-account-status/{userId}")]
        public async Task<ActionResult> UpdateAccountStatus(string userId)
        {
            var user = await _userService.GetUserAsync(new Specification<AppUser>(c => c.Id == userId));
            
            if (user == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy người dùng"));
            user.IsLocked = !user.IsLocked;

            var result = await _userService.UpdateUserAsync(user);

            return !result ? StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra")) : Ok("Cập nhật thành công");
        }

        [HttpGet("reports")]
        public async Task<ActionResult<IReadOnlyList<ReportForAdmin>>> GetReports([FromQuery] PaginationParam param, 
            [FromQuery] [Required(ErrorMessage = "Vui lòng chọn trạng thái")] ReportStatus status)
        {
            var repo = _unitOfWork.Repository<Report>();
            var reports = await repo.ListAsync(new ReportByStatusWithMediaSpec(param, status ,true));
            var count = await repo.CountAsync(new ReportByStatusWithMediaSpec(param, status ,false));
            var data = _mapper.Map<IReadOnlyList<Report>, IReadOnlyList<ReportForAdmin>>(reports);
            foreach (var report in data)
            {
                var client = await _contractService.GetClientByContractIdAsync(report.ContractId);
                report.ClientEmail = client.Email;
                report.ClientFullName = client.Fullname;
            }
            return Ok(new Pagination<ReportForAdmin>()
            {
                Data = data,
                Count = count,
                PageIndex = param.PageIndex,
                PageSize = param.PageSize
            });
        }
        
        [HttpPut("report/{reportId:int}")]
        public async Task<IActionResult> UpdateReportStatus(int reportId, 
            [FromQuery] [Required(ErrorMessage = "Vui lòng lựa chọn")] ReportOptionForAdmin optionForAdmin,
            [FromBody] [Required(ErrorMessage = "Vui lòng điền lí do từ chối hoặc phương án giải quyết")] string message)
        {
            var adminId = User.FindFirstValue("Id");
            var report = await _unitOfWork.Repository<Report>().GetBySpecificationAsync(new ReportSpec(reportId));
            if (report == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy"));
            
            if (optionForAdmin == ReportOptionForAdmin.Solve)
            {
                report.ReportStatus = ReportStatus.Solved;
                report.SolutionDesc = message;
            }
            else
            {
                report.ReportStatus = ReportStatus.Rejected;
                report.RejectReason = message;
            }

            report.AdminId = adminId;
            
            _unitOfWork.Repository<Report>().Update(report);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Cập nhật thành công");
        }

        [HttpPut("report/{reportId:int}/contract-status")]
        public async Task<IActionResult> UpdateContractStatus(int reportId, 
            [FromBody] [Required(ErrorMessage = "Vui lòng nhập lí do")] string reason,
            [FromQuery] [Required] AdminContractStatusOption option)
        {
            var report = await _unitOfWork.Repository<Report>()
                .GetBySpecificationAsync(new ReportWithContractSpec(reportId));
            if (report == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy"));

            var status = option == AdminContractStatusOption.Cancel ? ContractStatus.Canceled : ContractStatus.Complete;
            
            report.Contract.Status = status;
            report.Contract.CancelReason = reason;
            
            _unitOfWork.Repository<Contract>().Update(report.Contract);
            var res = await _unitOfWork.CompleteAsync();
            
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Cập nhật thành công");
        }

        [HttpPost("report/{reportId:int}/new-contract")]
        public async Task<IActionResult> CreateContract(int reportId, [FromBody] ContractByAdminRequest request)
        {
            var adminName = User.FindFirstValue("Fullname");
            
            var report = await _unitOfWork.Repository<Report>()
                .GetBySpecificationAsync(new ReportWithContractSpec(reportId));
            if (report == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy"));
            if (report.ReportStatus != ReportStatus.Pending) return BadRequest(new ErrorResponse(400, "Trạng thái hợp đồng không hợp lệ"));
            
            var coachCount = await _userService.CountUsersAsync(new Specification<AppUser>(u => u.Id == request.CoachId));
            if (coachCount == 0) return BadRequest(new ErrorResponse(400, "Không tìm thấy huấn luyện viên"));
            
            var course = await _unitOfWork.Repository<TrainingCourse>()
                .GetBySpecificationAsync(new TrainingCourseSpec(request.CoachId, request.CourseId));
            if (course == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy khoá tập"));
            
            var activeContract = await _unitOfWork.Repository<Contract>().CountAsync(new ContractSpec(report.Contract.ClientId, request.CoachId));
            if (activeContract != 0)
                return BadRequest(new ErrorResponse(400, "Khách hàng đang tập luyện với huấn luyện viên này"));
            
            var newContractOption = new Contract()
            {
                CoachId = request.CoachId,
                ClientId = report.Contract.ClientId,
                Description = string.IsNullOrEmpty(request.Description)
                    ? $"Hợp đồng đền bù. Được tạo bởi {adminName}"
                    : request.Description,
                Status = ContractStatus.Active,
                CreatedBy = User.FindFirstValue("Username")
            };

            var res = await _contractService.CreateContractByAdminAsync(report, newContractOption, course, report.ContractId,
                request.CancelReason);
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            
            return Ok("Tạo hợp đồng thành công. Hợp đồng cũ đã bị huỷ");
        }

        [HttpGet("created-contracts")]
        public async Task<ActionResult<Pagination<ContractForAdmin>>> GetCreatedContracts([FromQuery] PaginationParam param)
        {
            var adminUsername = User.FindFirstValue("Username");
            var repo = _unitOfWork.Repository<Contract>();
            var spec = new ContractSpec(param, adminUsername);
            var contracts = await repo.ListAsync(spec);
            var count = await repo.CountAsync(spec);
            var data = _mapper.Map<IReadOnlyList<Contract>, IReadOnlyList<ContractForAdmin>>(contracts);
            return Ok(new Pagination<ContractForAdmin>()
            {
                Count = count,
                Data = data,
                PageSize = param.PageSize,
                PageIndex = param.PageIndex
            });
        }

        [HttpPost("voucher/{reportId:int}")]
        public async Task<IActionResult> AddVoucherForClient(int reportId, 
            [Required(ErrorMessage = "Vui lòng nhập id của khiếu nại")]
            [Range(1, 100, ErrorMessage = "Vui lòng nhập số trong khoảng 1-100")] int discount,
            [FromBody] [Required(ErrorMessage = "Vui lòng nhập mô tả")] string desc)
        {
            var report = await _unitOfWork.Repository<Report>()
                .GetBySpecificationAsync(new ReportWithContractSpec(reportId));
            if (report == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy"));
            var newVoucher = new Voucher()
            {
                ClientId = report.Contract.ClientId,
                Code = Generator.RandomString(),
                Desc = desc,
                Discount = discount
            };
            _unitOfWork.Repository<Voucher>().Add(newVoucher);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Thêm thành công");
        }

        [HttpPut("coach-warning/{coachId}")]
        public async Task<IActionResult> WarningCoach(string coachId)
        {
            var coach = 
                await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == coachId));
            if (coach == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy huấn luyện viên"));
            coach.WarningCount += 1;
            if (coach.WarningCount >= 3) coach.IsLocked = true;
            var res = await _userService.UpdateUserAsync(coach);
            if (!res) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Cảnh cáo thành công");
        }
    }
}
