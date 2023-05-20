using System.ComponentModel.DataAnnotations;
using API.Dto.Certification;
using API.Dto.Media;
using API.ErrorResponses;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Entities.Status;
using Core.Interfaces;
using Core.Specifications;
using Core.Specifications.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.Dto.Coach;
using API.Dto.CoachingRequest;
using Core.Specifications.TrainingCourse;
using Core.Specifications.Cert;
using API.Dto.Contract;
using API.Dto.Contract.Status;
using API.Filters;
using Core.Interfaces.Base;
using Core.Interfaces.User;
using Core.Specifications.CoachingRequest;
using Core.Specifications.Contract;
using Hangfire;
using Infrastructure.Utils;

namespace API.Controllers
{
    [Authorize(Roles = "COACH")]
    public class CoachController : BaseApiController
    {
        private readonly IMediaService _mediaService;
        private readonly IUserService _userService;
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public CoachController(
            IMediaService mediaService, 
            IUserService userService, 
            IMapper mapper,
            IContractService contractService, 
            IMailService mailService, 
            IUnitOfWork unitOfWork, 
            IConfiguration configuration) 
        {
            _mediaService = mediaService;
            _userService = userService;
            _mapper = mapper;
            _contractService = contractService;
            _mailService = mailService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpPost("portfolio-photos")]
        public async Task<ActionResult<IReadOnlyCollection<PortfolioPhoto>>> AddPhotos([Required(ErrorMessage = "Vui lòng upload ảnh")] IFormFile[] files)
        {
            var coachId = User.FindFirstValue("Id");

            foreach (var file in files)
            {
                var success = await _mediaService.AddMediaAsync(file, new MediaAsset
                {
                    IsAvatar = false,
                    OnPortfolio = true,
                    UserId = coachId
                });

                if (!success) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra hoặc sai định dạng ảnh"));

            }

            var spec = new MediaByUserIdSpec(coachId, true);
            var mediaList = (await _unitOfWork.Repository<MediaAsset>().ListAsync(spec)).Take(files.Length).ToList();

            var data = _mapper.Map<IReadOnlyList<MediaAsset>, IReadOnlyList<DisplayedPhoto>>(mediaList);

            return Ok(data);
        }

        [HttpGet("portfolio-photos")]
        public async Task<ActionResult<Pagination<PortfolioPhoto>>> GetPortfolioPhotos([FromQuery] PaginationParam param)
        {
            var coachId = User.FindFirstValue("Id");
            var repo = _unitOfWork.Repository<MediaAsset>();
            var mediaSpec = new MediaWithFilterSpec(param, coachId, true, false, true);
            var countSpec = new MediaWithFilterSpec(param, coachId, true, false, false);

            var photoList = await repo.ListAsync(mediaSpec);

            var data = _mapper.Map<IReadOnlyList<MediaAsset>, IReadOnlyList<DisplayedPhoto>>(photoList);

            var count = await repo.CountAsync(countSpec);

            return Ok(new Pagination<DisplayedPhoto>
            {
                Data = data,
                Count = count,
                PageIndex = param.PageIndex,
                PageSize = param.PageSize
            });
        }

        [HttpDelete("photo-remove/{id:int}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var coachId = User.FindFirstValue("Id");
            var image = await _unitOfWork.Repository<MediaAsset>().GetBySpecificationAsync(new MediaAssetSpec(id, coachId));
            if (image == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
            var result = await _mediaService.DeleteMediaAsync(image);
            if (!result) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Xoá thành công");
        }

        [HttpPost("certification-request")]
        public async Task<ActionResult<CertRequestInfo>> SubmitCert(
            [Required(ErrorMessage = "Vui lòng thêm ảnh chứng chỉ")] IFormFile[] certImages, 
            [Required(ErrorMessage = "Vui lòng thêm ảnh danh tính")] IFormFile[] idImages)
        {
            var repo = _unitOfWork.Repository<CertificateSubmission>();
            var coachId = User.FindFirstValue("Id");
            var coach = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == coachId));
            
            if (coach!.IsVerified!.Value) return BadRequest(new ErrorResponse(400, "Bạn đã được xác nhận chứng chỉ trước đó")); 
            
            var certSubmission = await repo.GetBySpecificationAsync(new CertWithMediaSpec(coachId));
            
            if (certSubmission is { Status: CertStatus.Pending })
                return BadRequest(new ErrorResponse(400, "Bạn đã yêu cầu xác nhận chứng chỉ trước đó"));

            var isUpdate = false;
            
            if (certSubmission is { Status: CertStatus.Denied })
            {
                foreach (var img in certSubmission.MediaAssets)
                {
                    await _mediaService.DeleteMediaAsync(img);
                }
                certSubmission.MediaAssets = new List<MediaAsset>();
                certSubmission.Status = CertStatus.Pending;
                certSubmission.Reason = "";
                isUpdate = true;
            }
            else
            {
                certSubmission = new CertificateSubmission
                {
                    CoachId = coachId,
                    Status = CertStatus.Pending,
                    MediaAssets = new List<MediaAsset>()
                };
            }

            // cert img saving
            var mediaAssetOption = new MediaAsset()
            {
                UserId = coachId,
                IsCert = true
            };

            var certImagesSavingRes =
                await _mediaService.SaveMediaFiles(certImages, certSubmission.MediaAssets, mediaAssetOption);
            
            if (!certImagesSavingRes) 
                return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra khi upload ảnh chứng chỉ hoặc sai định dạng ảnh"));

            // id img saving
            mediaAssetOption.IsCert = false;
            var idImagesSavingRes = await _mediaService.SaveMediaFiles(idImages, certSubmission.MediaAssets, mediaAssetOption);
            if (!idImagesSavingRes) 
                return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra khi upload ảnh thông tin cá nhân hoặc sai định dạng ảnh"));

            if (isUpdate) repo.Update(certSubmission);
            else repo.Add(certSubmission);

            var result = await _unitOfWork.CompleteAsync();

            if (result == 0) 
                return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Nộp chứng chỉ thành công");
        }

        [HttpGet("certification-request")]
        public async Task<ActionResult<CertRequestInfo>> GetCert()
        {
            var coachId = User.FindFirstValue("Id");
            var spec = new CertWithMediaSpec(coachId);

            var cert = await _unitOfWork.Repository<CertificateSubmission>().GetBySpecificationAsync(spec);

            if (cert is null) return Ok();

            var data = _mapper.Map<CertRequestDetailForCoach>(cert);

            return Ok(data);
        }

        [HttpGet("about-me")]
        public async Task<ActionResult<string>> GetAboutMe()
        {
            var user = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == User.FindFirstValue("Id")));
            return Ok(string.IsNullOrEmpty(user!.AboutMe) ? "" : user.AboutMe);
        }

        [HttpPost("about-me")]
        public async Task<IActionResult> UpdateAboutMe([FromBody] string? content)
        {
            var user = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == User.FindFirstValue("Id")));
            user!.AboutMe = content;
            var result = await _userService.UpdateUserAsync(user);
            if (!result) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            return Ok("Đã cập nhật");
        }

        [HttpGet("training-courses")]
        public async Task<ActionResult<Pagination<TrainingCourse>>> GetCourses([FromQuery] PaginationParam param)
        {
            var repo = _unitOfWork.Repository<TrainingCourse>();
            var courses = await repo.ListAsync(new TrainingCourseSpec(User.FindFirstValue("Id"),
                param, true));
            var count = await repo.CountAsync(new TrainingCourseSpec(User.FindFirstValue("Id"), param, false));
            var data = _mapper.Map<IReadOnlyList<TrainingCourse>, IReadOnlyList<TrainingCourseResponse>>(courses);
            return Ok(new Pagination<TrainingCourseResponse>()
            {
                Count = count,
                Data = data,
                PageIndex = param.PageIndex,
                PageSize = param.PageSize
            });
        }

        [HttpPost("training-course")]
        public async Task<ActionResult<TrainingCourseBaseDto>> AddTrainingCourse(TrainingCourseBaseDto trainingCourse)
        {
            var coachId = User.FindFirstValue("Id");
            var newCourse = new TrainingCourse()
            {
                CoachId = coachId,
                Duration = trainingCourse.Duration,
                Description = trainingCourse.Description,
                Name = trainingCourse.Name,
                Price = trainingCourse.Price
            };
            _unitOfWork.Repository<TrainingCourse>().Add(newCourse);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            return Ok(_mapper.Map<TrainingCourseResponse>(newCourse));
        }

        [HttpPut("training-course/{id:int}")]
        public async Task<ActionResult<TrainingCourseBaseDto>> UpdateTrainingCourse(TrainingCourseBaseDto trainingCourseBaseDto, int id)
        {
            var repo = _unitOfWork.Repository<TrainingCourse>();
            var course = await repo.GetBySpecificationAsync(new TrainingCourseSpec(User.FindFirstValue("Id"), id));
            if (course == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
            course.Name = trainingCourseBaseDto.Name;
            course.Description = trainingCourseBaseDto.Description;
            course.Duration = trainingCourseBaseDto.Duration;
            course.Description = trainingCourseBaseDto.Description;
            course.Price = trainingCourseBaseDto.Price;
            repo.Update(course);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            return Ok(_mapper.Map<TrainingCourseResponse>(course));
        }

        [HttpDelete("training-course/{id:int}")]
        public async Task<IActionResult> DeleteTrainingCourse(int id)
        {
            var repo = _unitOfWork.Repository<TrainingCourse>();
            var course = await repo.GetBySpecificationAsync(new TrainingCourseSpec(User.FindFirstValue("Id"), id));
            if (course == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
            repo.Delete(course);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            return Ok("Xoá thành công");
        }

        [HttpGet("training-course/{id:int}")]
        public async Task<ActionResult<TrainingCourseResponse>> GetTrainingCourseById(int id)
        {
            var course = await _unitOfWork.Repository<TrainingCourse>().GetBySpecificationAsync(new TrainingCourseSpec(User.FindFirstValue("Id"), id));
            if (course == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
            return Ok(_mapper.Map<TrainingCourseResponse>(course));
        }

        [ServiceFilter(typeof(CoachVerificationFilter))]
        [HttpGet("contracts")]
        public async Task<ActionResult<Pagination<ContractForCoach>>> GetCoachingContract([FromQuery] PaginationParam param, 
            [Required(ErrorMessage = "Vui lòng chọn trạng thái")] ContractStatusDto statusDto)
        {
            var status = Enum.Parse<ContractStatus>(statusDto.ToString());
            var coachId = User.FindFirstValue("Id");

            var filterSpec = new ContractWithUserSpec(param, coachId, status, true, true);
            var countSpec = new ContractWithUserSpec(param, coachId, status, false, true);

            var contracts = await _unitOfWork.Repository<Contract>().ListAsync(filterSpec);
            var count = await _unitOfWork.Repository<Contract>().CountAsync(countSpec);

            var data = _mapper.Map<IReadOnlyList<Contract>, IReadOnlyList<ContractForCoach>>(contracts);
            return Ok(new Pagination<ContractForCoach>()
            {
                Count = count,
                Data = data,
                PageIndex = param.PageIndex,
                PageSize = param.PageSize
            });
        }
        
        [ServiceFilter(typeof(CoachVerificationFilter))]
        [HttpGet("coaching-requests")]
        public async Task<ActionResult<Pagination<CoachingRequestForCoach>>> GetCoachingRequests([FromQuery] PaginationParam param, [FromQuery] [Required(ErrorMessage = "Vui lòng chọn trạng thái của yêu cầu")] CoachRequestStatus coachRequestStatus)
        {
            var repo = _unitOfWork.Repository<CoachingRequest>();
            var coachId = User.FindFirstValue("Id");
            var coachingRequests = await repo.ListAsync(new CoachingRequestWithUserAndCourseSpec(coachId, param, coachRequestStatus.ToString(),true, true));
            var data =
                _mapper.Map<IReadOnlyList<CoachingRequest>, IReadOnlyList<CoachingRequestForCoach>>(coachingRequests);
            var count = await repo.CountAsync(new CoachingRequestWithUserAndCourseSpec(coachId, param, coachRequestStatus.ToString(),true, false));
            return Ok(new Pagination<CoachingRequestForCoach>()
            {
                PageIndex = param.PageIndex,
                PageSize = param.PageSize,
                Count = count,
                Data = data
            });
        }

        [ServiceFilter(typeof(CoachVerificationFilter))]
        [HttpPut("coaching-request/{id:int}")]
        public async Task<IActionResult> UpdateCoachingRequestStatus(int id, [FromQuery] [Required(ErrorMessage = "Vui lòng lựa chọn")] CoachingRequestOption options, 
            [FromBody] string reason = "")
        {
            var coachId = User.FindFirstValue("Id");
            var coachingRequest = await _unitOfWork.Repository<CoachingRequest>().GetBySpecificationAsync(new CoachingRequestWithUserAndCourseSpec(id, false));
            
            if (coachingRequest == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
            
            if (coachingRequest.CoachId != coachId || 
                coachingRequest.Status != CoachingRequestStatus.Pending) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));

            switch (options)
            {
                case CoachingRequestOption.Accept:
                    var addContractRes = await _contractService.CreateContractFromCoachingRequestAsync(coachingRequest);
                    if (addContractRes == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
                    return Ok("Tạo hợp đồng thành công");
                
                case CoachingRequestOption.Reject when reason.Length == 0:
                    return BadRequest(new ErrorResponse(400, "Vui lòng nhập lí do từ chối"));
                
                case CoachingRequestOption.Reject when reason.Length > 0:
                    coachingRequest.Status = CoachingRequestStatus.CoachRejected;
                    coachingRequest.RejectReason = reason;

                    // Reset voucher
                    if (coachingRequest.VoucherCode != null)
                    {
                        var voucherRepo = _unitOfWork.Repository<Voucher>();
                        var voucher = await voucherRepo.GetBySpecificationAsync(new Specification<Voucher>(v =>
                                v.Code == coachingRequest.VoucherCode));
                        voucher!.IsUsed = false;
                        voucherRepo.Update(voucher);
                    }
                    
                    _unitOfWork.Repository<CoachingRequest>().Update(coachingRequest);
                    var res = await _unitOfWork.CompleteAsync();
                    if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

                    // No sending mail when it is testing users
                    if (!coachingRequest.Client.Email.ToLower().Contains("client"))
                    {
                        // Send mail
                        var mailBody = $"<p style=\"font-size: 16px;\">Yêu cầu cho {coachingRequest.Course.Name} " +
                                       $"với số tiền {CurrencyHelper.GetVnd(coachingRequest.Course.Price)} " +
                                       $"đã bị từ chối. <br>" +
                                       $"Hệ thống chúng tôi sẽ thực hiện hoàn tiền lại cho bạn.</p>";
                        _mailService.SendMail(coachingRequest.Client.Email, "Thông báo hoàn tiền", mailBody, MailType.Raw);
                    }
                    break;
            }
            return Ok("Cập nhật thành công");
        }

        [HttpPut("contract/{contractId:int}/completion")]
        public async Task<IActionResult> EndContract(int contractId)
        {
            var coachId = User.FindFirstValue("Id");
            var repo = _unitOfWork.Repository<Contract>();
            var contract = await repo.GetBySpecificationAsync(new ContractSpec(coachId, contractId));
            if (contract == null) return BadRequest(new ErrorResponse(400, "Không tồn tại"));
            if (contract.Status != ContractStatus.Active) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            var isComplete = await _contractService.IsLogCompleteAsync(contract.Id);
            if (!isComplete) return BadRequest(new ErrorResponse(400, "Vui lòng hoàn thành đủ số buổi huấn luyện"));
            contract.Status = ContractStatus.Pending;

            var expDate = DateConverter.AddFromStartDate(_configuration["Contract:AutoEndTime"],
                _configuration["Contract:AutoEndUnit"], DateTime.Now);
            var jobId = BackgroundJob.Schedule<IContractService>(s => s.AutoCompleteContractAsync(contractId),expDate);
            
            contract.BackgroundJobId = jobId;
            repo.Update(contract);
            var res = await _unitOfWork.CompleteAsync();
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            return Ok($"Thành công. Thời hạn để khách hàng xác nhận hoặc từ chối {expDate}");
        }

    }
}
