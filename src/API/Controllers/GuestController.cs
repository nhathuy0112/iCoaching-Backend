using System.Security.Claims;
using API.Dto.Coach;
using API.ErrorResponses;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Entities.Auth;
using Core.Interfaces.Base;
using Core.Interfaces.User;
using Core.Specifications;
using Core.Specifications.CoachingRequest;
using Core.Specifications.Contract;
using Core.Specifications.Media;
using Core.Specifications.TrainingCourse;
using Core.Specifications.User;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/api")]
public class GuestController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GuestController(IUserService userService, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("coaches")]
    public async Task<ActionResult<Pagination<CoachInfoForGuest>>> GetCoaches([FromQuery] PaginationParam param)
    {
        var coaches = await _userService.ListUserAsync(new VerifiedCoachWithAvatarSpec(param, true));
        var count = await _userService.CountUsersAsync(new VerifiedCoachWithAvatarSpec(param, false));
        var data = _mapper.Map<IReadOnlyList<AppUser>, IReadOnlyList<CoachInfoForGuest>>(coaches);
        return Ok(new Pagination<CoachInfoForGuest>()
        {
            Count = count,
            PageIndex = param.PageIndex,
            PageSize = param.PageSize,
            Data = data
        });
    }

    [HttpGet("coach/{id}/profile")]
    public async Task<ActionResult<CoachInfoForGuest>> GetCoachProfileById(string id)
    {
        var coach = await _userService.GetUserAsync(new UserByRoleWithAvatarSpec(Role.COACH, id));
        if (coach == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy huấn luyện viên"));
        var response = _mapper.Map<CoachInfoForGuest>(coach);
        return Ok(response);
    }
    
    [HttpGet("coach/{id}/about-me")]
    public async Task<IActionResult> GetCoachAboutMeById(string id)
    {
        var coach = await _userService.GetUserAsync(new UserByRoleSpec(Role.COACH, id));
        if (coach == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy huấn luyện viên"));
        return Ok(string.IsNullOrEmpty(coach.AboutMe) ? "" : coach.AboutMe);
    }

    [HttpGet("coach/{id}/photos")]
    public async Task<ActionResult<Pagination<CoachPortfolioPhoto>>> GetCoachPhotosById([FromQuery] PaginationParam param, string id)
    {
        var repo = _unitOfWork.Repository<MediaAsset>();
        var coach = await _userService.GetUserAsync(new UserByRoleSpec(Role.COACH, id));
        if (coach == null) return BadRequest(new ErrorResponse(400, "Không tìm thấy huấn luyện viên"));
        var photos = await repo.ListAsync(new MediaWithFilterSpec(param, coach.Id, true));
        var count = await repo.CountAsync(new MediaWithFilterSpec(param, coach.Id, false));
        var data = _mapper.Map<IReadOnlyList<MediaAsset>, IReadOnlyList<CoachPortfolioPhoto>>(photos);
        return Ok(new Pagination<CoachPortfolioPhoto>()
        {
            PageIndex = param.PageIndex,
            PageSize = param.PageSize,
            Data = data,
            Count = count
        });
    }

    [HttpGet("coach/{id}/training-courses")]
    public async Task<ActionResult<Pagination<TrainingCourseResponse>>> GetCourses([FromQuery] PaginationParam param, string id)
    {
        var coach = await _userService.CountUsersAsync(new UserByRoleSpec(Role.COACH, id));
        if (coach == 0) return BadRequest(new ErrorResponse(400, "Không tìm thấy huấn luyện viên"));
        
        var trainingCourseRepo = _unitOfWork.Repository<TrainingCourse>();
        var courses = await trainingCourseRepo.ListAsync(new TrainingCourseSpec(id,
            param, true));
        
        var count = await trainingCourseRepo.CountAsync(new TrainingCourseSpec(id, param, false));
        var data = _mapper.Map<IReadOnlyList<TrainingCourse>, IReadOnlyList<TrainingCourseResponse>>(courses);
        return Ok(new Pagination<TrainingCourseResponse>()
        {
            Count = count,
            Data = data,
            PageIndex = param.PageIndex,
            PageSize = param.PageSize
        });
    }
    
    [HttpGet("coach/{coachId}/training-course-detail/{id:int}")]
    public async Task<ActionResult<TrainingCourseResponse>> GetTrainingCourseById(string coachId, int id)
    {
        var course = await _unitOfWork.Repository<TrainingCourse>().GetBySpecificationAsync(new TrainingCourseSpec(coachId, id));
        if (course == null) return BadRequest(new ErrorResponse(400, "Không tồn tại khoá tập"));
        
        var response = _mapper.Map<TrainingCourseResponse>(course);

        var clientId = User.FindFirstValue("Id");
        if (clientId == null) return Ok(response);
        
        var client = await _userService.GetUserAsync(new Specification<AppUser>(u => u.Id == clientId && u.Role == Role.CLIENT));
        if (client == null) return Ok(response);
        
        var trainingRequest =
            await _unitOfWork.Repository<CoachingRequest>().CountAsync(new CoachingRequestSpec(clientId, coachId));
        var activeContract = await _unitOfWork.Repository<Contract>().CountAsync(new ContractSpec(clientId, coachId));
        response.IsClientRequested = false;

        if (trainingRequest != 0 || activeContract != 0) response.IsClientRequested = true;
        
        return Ok(response);
    }
}