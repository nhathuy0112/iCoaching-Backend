using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using API.Dto.Coach;
using API.Dto.Contract;
using API.Dto.Contract.File;
using API.Dto.Contract.Report;
using API.Dto.Contract.TrainingLog;
using API.ErrorResponses;
using API.Filters;
using AutoMapper;
using Core.Entities;
using Core.Entities.Status;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Specifications.Contract;
using Core.Specifications.Contract.FileAsset;
using Core.Specifications.Contract.Report;
using Core.Specifications.Contract.TrainingLog;
using Core.Specifications.Media;
using Hangfire;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Roles = "ADMIN,COACH,CLIENT")]
    public class ContractController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IContractService _contractService;
        private readonly IMediaService _mediaService;
        private readonly IUnitOfWork _unitOfWork;

        public ContractController(IMapper mapper, IContractService contractService, IMediaService mediaService, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _contractService = contractService;
            _mediaService = mediaService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{contractId:int}/detail")]
        public async Task<ActionResult<ContractDetail>> GetContractDetail(int contractId)
        {
            var userId = User.FindFirstValue("Id");
            var isAdmin = User.IsInRole("ADMIN");
            var contract = await _unitOfWork.Repository<Contract>().GetBySpecificationAsync(new ContractByUserWithCoachAndClientSpec(contractId, userId, isAdmin));
            if (contract == null) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            var response = _mapper.Map<ContractDetail>(contract);
            response.IsReported = await _contractService.IsReportedAsync(contractId);
            response.IsComplete = await _contractService.IsLogCompleteAsync(contractId);
            return Ok(response);
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpPost("{contractId:int}/file")]
        [Authorize(Roles = "COACH")]
        public async Task<IActionResult> SaveFile(int contractId, [Required(ErrorMessage = "Vui lòng tải lên ít nhất 1 file")] IFormFile[] files)
        {
            var errs = new List<string>();
            foreach (var file in files)
            {
                var res = await _contractService.SaveFileAsync(file, new FileAsset()
                {
                    ContractId = contractId,
                });
                if (!res) errs.Add($"Không tải lên được file {file.FileName}");
            }

            return errs.Count == 0 ? Ok("Lưu file thành công") : Ok(errs);
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpGet("{contractId:int}/files")]
        [Authorize(Roles = "COACH, CLIENT")]
        public async Task<ActionResult<IReadOnlyList<FileResponse>>> GetFiles(int contractId)
        {
            var files = await _unitOfWork.Repository<FileAsset>().ListAsync(new FileByContractIdSpec(contractId));
            var data = _mapper.Map<IReadOnlyList<FileAsset>, IReadOnlyList<FileResponse>>(files);
            return Ok(data);
        }
        
        [ServiceFilter(typeof(ContractFilter))]
        [HttpGet("{contractId:int}/file/downloading/{fileId:int}")]
        [Authorize(Roles = "COACH, CLIENT")]
        public async Task<IActionResult> DownloadFile(int contractId, int fileId)
        {
            var file = await _unitOfWork.Repository<FileAsset>().GetBySpecificationAsync(new FileSpec(fileId, contractId));
            if (file == null) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            
            var stream = await _contractService.GetFileDownloadingStream(file.DownloadUrl);
            if (stream == null) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return File(stream, "application/octet-stream", file.FileName);
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpDelete("{contractId:int}/file/{fileId:int}")]
        [Authorize(Roles = "COACH")]
        public async Task<IActionResult> DeleteFile(int contractId, int fileId)
        {
            var file = await _unitOfWork.Repository<FileAsset>().GetBySpecificationAsync(new FileSpec(fileId, contractId));
            if (file == null) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));

            var res = await _contractService.DeleteFileAsync(file);
            if (!res) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

            return Ok("Xoá thành công");
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpGet("{contractId:int}/logs")]
        public async Task<ActionResult<IReadOnlyList<TrainingCourseResponse>>> GetTrainingLogs(int contractId)
        {
            var logs = await _unitOfWork.Repository<TrainingLog>().ListAsync(new TrainingLogByContractIdWithMediaAndFileSpec(contractId));
            var data = _mapper.Map<IReadOnlyList<TrainingLog>, IReadOnlyList<TrainingLogResponse>>(logs);
            return Ok(data);
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpGet("{contractId:int}/log/{logId:int}")]
        [Authorize(Roles = "COACH")]
        public async Task<ActionResult<TrainingLogResponse>> GetTrainingLogDetail(int contractId, int logId)
        {
            var trainingLog =
                await _unitOfWork.Repository<TrainingLog>().GetBySpecificationAsync(new TrainingLogByContractIdWithMediaAndFileSpec(contractId, logId));
            if (trainingLog == null) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            return Ok(_mapper.Map<TrainingLogResponse>(trainingLog));
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpPut("{contractId:int}/log/{logId:int}")]
        [Authorize(Roles = "COACH")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateTrainingLogs(int contractId, int logId,
            [FromForm] TrainingLogUpdateRequest request)
        {
            var trainingLog = await _unitOfWork.Repository<TrainingLog>().GetBySpecificationAsync(new TrainingLogSpec(logId, contractId));
            var uploadError = new List<string>();
            if (trainingLog == null) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));

            // Update training date
            
            var newTrainingDate = DateTime.ParseExact(request.TrainingDate, 
                new[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy" }, CultureInfo.InvariantCulture);
            var isValidDate = await _contractService.IsValidTrainingDateAsync(contractId, trainingLog.DateNo, newTrainingDate);
            if (!isValidDate) return BadRequest(new ErrorResponse(400, "Không được nhỏ hơn ngày tập luyện trước đó hoặc ngày bắt đầu hợp đồng"));
            trainingLog.TrainingDate = newTrainingDate;

            // Update note
            trainingLog.Note = request.Note;
            
            // Add images
            if (request.Images != null)
            {
                foreach (var img in request.Images)
                {
                    var res = await _mediaService.AddMediaAsync(img, new MediaAsset()
                    {
                        UserId = User.FindFirstValue("Id"),
                        TrainingLogId = trainingLog.Id
                    });

                    if (!res) uploadError.Add($"Không thêm được ảnh {img.FileName}");
                }
            }

            // Add videos
            if (request.Videos != null)
            {
                foreach (var vid in request.Videos)
                {
                    var res = await _mediaService.AddMediaAsync(vid, new MediaAsset()
                    {
                        UserId = User.FindFirstValue("Id"),
                        TrainingLogId = trainingLog.Id,
                        IsVideo = true
                    });

                    if (!res) uploadError.Add($"Không thêm được video {vid.FileName}");
                }
            }

            // Add files
            if (request.Files != null)
            {
                foreach (var file in request.Files)
                {
                    var fileRes = await _contractService.SaveFileAsync(file, new FileAsset()
                    {
                        ContractId = contractId,
                        TrainingLogId = trainingLog.Id
                    });

                    if (!fileRes) uploadError.Add($"Lỗi tải lên tập tin {file.FileName}");
                }
            }

            trainingLog.LastUpdatingDate = DateTime.Now;
            trainingLog.Status = TrainingLogStatus.Complete;
            
            _unitOfWork.Repository<TrainingLog>().Update(trainingLog);
            var updateRes = await _unitOfWork.CompleteAsync();
            
            if (updateRes != 0) return uploadError.Count == 0 ? Ok("Cập nhật thành công") : Ok(uploadError);
            
            return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));

        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpDelete("{contractId:int}/log/{logId:int}/media/{mediaId:int}")]
        [Authorize(Roles = "COACH")]
        public async Task<IActionResult> DeleteTrainingLogMediaFile(int contractId, int logId, int mediaId)
        {
            var logCount = await _unitOfWork.Repository<TrainingLog>().CountAsync(new TrainingLogSpec(logId, contractId));
            if (logCount == 0) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            var media = await _unitOfWork.Repository<MediaAsset>().GetBySpecificationAsync(new MediaAssetSpec(mediaId, logId));
            if (media == null) return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            var res = await _mediaService.DeleteMediaAsync(media);
            if (!res) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            return Ok("Xoá thành công");
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpPost("{contractId:int}/report")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> ReportContract(int contractId, [FromForm] ReportRequest request)
        {
            var newReport = new Report()
            {
                ContractId = contractId,
                CreatedDate = DateTime.Now,
                ReportStatus = ReportStatus.Pending,
                Detail = request.Desc,
                MediaAssets = new List<MediaAsset>()
            };

            var mediaAssetOption = new MediaAsset()
            {
                UserId = User.FindFirstValue("Id")
            };

            var imagesSavingRes =
                await _mediaService.SaveMediaFiles(request.Files, newReport.MediaAssets, mediaAssetOption);

            if (!imagesSavingRes) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra hoặc sai định dạng file"));

            var contract = await _unitOfWork.Repository<Contract>().GetBySpecificationAsync(new ContractSpec(contractId));
            
            if (contract!.Status is not (ContractStatus.Active or ContractStatus.Pending))
                return BadRequest(new ErrorResponse(400, "Không hợp lệ"));
            
            _unitOfWork.Repository<Report>().Add(newReport);
            
            if (!string.IsNullOrEmpty(contract.BackgroundJobId))
            {
                RecurringJob.RemoveIfExists(contract.BackgroundJobId);
            }
                
            _unitOfWork.Repository<Contract>().Update(contract);
            var res = await _unitOfWork.CompleteAsync();
            
            if (res == 0) return StatusCode(500, new ErrorResponse(500, "Có lỗi xảy ra"));
            
            return Ok("Báo cáo thành công");
        }

        [ServiceFilter(typeof(ContractFilter))]
        [HttpGet("{contractId:int}/reports")]
        [Authorize(Roles = "CLIENT, ADMIN")]
        public async Task<ActionResult<IReadOnlyList<ReportBaseDto>>> GetReportsOfContract(int contractId)
        {
            var reports = await _unitOfWork.Repository<Report>().ListAsync(new ReportByContractIdWithMediaSpec(contractId));
            var response = _mapper.Map<IReadOnlyList<Report>, IReadOnlyList<ReportBaseDto>>(reports);
            return Ok(response);
        }
    }
}
