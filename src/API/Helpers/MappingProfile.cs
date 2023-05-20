using System.Globalization;
using API.Dto.Certification;
using API.Dto.Client;
using API.Dto.Coach;
using API.Dto.CoachingRequest;
using API.Dto.Contract;
using API.Dto.Contract.File;
using API.Dto.Contract.Report;
using API.Dto.Contract.TrainingLog;
using API.Dto.Media;
using API.Dto.User;
using AutoMapper;
using Core.Entities;

namespace API.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AppUser, BaseUserProfile>()
            .ForMember(destination => destination.Dob,
                option => option.MapFrom(source => source.Dob!.Value.ToShortDateString()));

        CreateMap<AppUser, CoachProfileResponse>()
            .ForMember(destination => destination.Dob,
                option => option.MapFrom(source => source.Dob!.Value.ToShortDateString()));

        CreateMap<AppUser, AdminBasicInfo>()
            .ForMember(d => d.Dob, option => option.Ignore())
            .ForMember(d => d.Gender, option => option.Ignore())
            .ForMember(d => d.AvatarUrl, option => option.MapFrom(s => s.MediasAssets.First().Url));
            

        CreateMap<MediaAsset, DisplayedPhoto>();

        CreateMap<TrainingCourse, TrainingCourseResponse>()
            .ForMember(d => d.Price,
                option =>
                    option.MapFrom(s => CurrencyHelper.GetVnd(s.Price)));

        CreateMap<AppUser, CoachListInfoForAdmin>()
            .ForMember(d => d.Age,
                option => option.MapFrom(s => DateTime.Now.Year - s.Dob!.Value.Year))
            .ForMember(d => d.Dob,
                option => option.Ignore())
            .ForMember(d => d.AvatarUrl, option => option.MapFrom(s => s.MediasAssets.First().Url));

        #region CertificateSubmission -> CertRequestInfo

        CreateMap<CertificateSubmission, CertRequestInfo>()
            .ForMember(destination => destination.CertId,
                option => option.MapFrom(source => source.Id))
            .ForMember(destination => destination.Username,
                option => option.MapFrom(source => source.Coach.UserName))
            .ForMember(destination => destination.Fullname,
                option => option.MapFrom(source => source.Coach.Fullname))
            .ForMember(destination => destination.Gender,
                option => option.MapFrom(source => source.Coach.Gender))
            .ForMember(destination => destination.Age,
                option => option.MapFrom(source => DateTime.Now.Year - source.Coach.Dob!.Value.Year))
            .ForMember(destination => destination.Email,
                option => option.MapFrom(source => source.Coach.Email))
            .ForMember(destination => destination.PhoneNumber,
                option => option.MapFrom(source => source.Coach.PhoneNumber))
            .ForMember(d => d.AvatarUrl, o => o.MapFrom(s => s.MediaAssets.First().Url));
        #endregion

        #region CertificateSubmission -> CertRequestDetail
        CreateMap<CertificateSubmission, CertRequestDetail>()
            .ForMember(destination => destination.CertId,
                option => option.MapFrom(source => source.Id))
            .ForMember(destination => destination.Username,
                option => option.MapFrom(source => source.Coach.UserName))
            .ForMember(destination => destination.Fullname,
                option => option.MapFrom(source => source.Coach.Fullname))
            .ForMember(destination => destination.Gender,
                option => option.MapFrom(source => source.Coach.Gender))
            .ForMember(destination => destination.Age,
                option => option.MapFrom(source => DateTime.Now.Year - source.Coach.Dob!.Value.Year))
            .ForMember(destination => destination.Email,
                option => option.MapFrom(source => source.Coach.Email))
            .ForMember(destination => destination.PhoneNumber,
                option => option.MapFrom(source => source.Coach.PhoneNumber))
            .ForMember(destination => destination.AvatarUrl,
                option => option.MapFrom(source => 
                    source.Coach.MediasAssets!.FirstOrDefault(p => p.IsAvatar)!.Url))
            .ForMember(destination => destination.IdImages,
                option => option.MapFrom(source => 
                    source.MediaAssets.Where(m => m.IsCert == false).Select(m => m.Url)))
            .ForMember(destination => destination.CertImages,
                option => option.MapFrom(source => 
                    source.MediaAssets.Where(m => m.IsCert == true).Select(m => m.Url)));
        #endregion

        CreateMap<AppUser, CoachInfoForGuest>()
            .ForMember(d => d.Age,
                option => option.MapFrom(s => DateTime.Now.Year - s.Dob!.Value.Year))
            .ForMember(d => d.AvatarUrl,
                option => option.MapFrom(s =>
                    (s.MediasAssets == null || s.MediasAssets.Count == 0) ? "" : s.MediasAssets.First().Url));

        CreateMap<MediaAsset, CoachPortfolioPhoto>();

        #region Contract -> ContractForCoach
        CreateMap<Contract, ContractForCoach>()
            .ForMember(destination => destination.ClientName,
                option => option.MapFrom(source => source.Client.Fullname))
            .ForMember(destination => destination.ClientGender,
                option => option.MapFrom(source => source.Client.Gender))
            .ForMember(destination => destination.ClientAge,
                option => option.MapFrom(source => DateTime.Now.Year - source.Client.Dob!.Value.Year))
            .ForMember(destination => destination.ClientEmail,
                option => option.MapFrom(source => source.Client.Email))
            .ForMember(destination => destination.ClientPhoneNumber,
                option => option.MapFrom(source => source.Client.PhoneNumber));
        #endregion

        #region Contract -> ContractForClient

        CreateMap<Contract, ContractForClient>()
            .ForMember(destination => destination.CoachName,
                option => option.MapFrom(source => source.Coach.Fullname))
            .ForMember(destination => destination.CoachGender,
                option => option.MapFrom(source => source.Coach.Gender))
            .ForMember(destination => destination.CoachEmail,
                option => option.MapFrom(source => source.Coach.Email))
            .ForMember(destination => destination.CoachPhoneNumber,
                option => option.MapFrom(source => source.Coach.PhoneNumber));
        #endregion

        #region Contract -> ContractForAdmin

        CreateMap<Contract, ContractForAdmin>()
            .ForMember(destination => destination.ClientName,
                option => option.MapFrom(source => source.Client.Fullname))
            .ForMember(destination => destination.ClientGender,
                option => option.MapFrom(source => source.Client.Gender))
            .ForMember(destination => destination.ClientAge,
                option => option.MapFrom(source => DateTime.Now.Year - source.Client.Dob!.Value.Year))
            .ForMember(destination => destination.ClientEmail,
                option => option.MapFrom(source => source.Client.Email))
            .ForMember(destination => destination.ClientPhoneNumber,
                option => option.MapFrom(source => source.Client.PhoneNumber));

        #endregion


        #region CoachingRequest -> CoachingRequestForClient

        CreateMap<CoachingRequest, CoachingRequestForClient>()
            .ForMember(d => d.CourseName,
                option => option.MapFrom(s => s.Course.Name))
            .ForMember(d => d.CoachName,
                option => option.MapFrom(s => s.Coach.Fullname))
            .ForMember(d => d.Duration, option => option.MapFrom(s => s.Course.Duration))
            .ForMember(d => d.PriceToPay,
                option => option.MapFrom(s =>
                    s.Discount == null
                        ? CurrencyHelper.GetVnd(s.Course.Price)
                        : CurrencyHelper.GetVnd((long)(s.Course.Price - (s.Course.Price * s.Discount / 100)))))
            .ForMember(d => d.Price, o => o.MapFrom(s => CurrencyHelper.GetVnd(s.Course.Price)));
        #endregion

        #region CoachingRequest -> CoachingRequestForCoach

        CreateMap<CoachingRequest, CoachingRequestForCoach>()
            .ForMember(d => d.CourseName,
                option => option.MapFrom(s => s.Course.Name))
            .ForMember(d => d.ClientName,
                option => option.MapFrom(s => s.Client.Fullname))
            .ForMember(d => d.Price,
                option => option.MapFrom(s => CurrencyHelper.GetVnd(s.Course.Price)))
            .ForMember(d => d.Duration,
                option => option.MapFrom(s => s.Course.Duration))
            .ForMember(d => d.Gender,
                option => option.MapFrom(s => s.Client.Gender))
            .ForMember(d => d.Email,
                option => option.MapFrom(s => s.Client.Email))
            .ForMember(d => d.Age,
                option => option.MapFrom(s => DateTime.Now.Year - s.Client.Dob!.Value.Year))
            .ForMember(d => d.PhoneNumber,
                option => option.MapFrom(s => s.Client.PhoneNumber));

        #endregion

        CreateMap<CertificateSubmission, CertRequestDetailForCoach>()
            .ForMember(d => d.CertUrls,
                option => option.MapFrom(s => s.MediaAssets.Where(m => m.IsCert == true).Select(m => m.Url)))
            .ForMember(d => d.IdUrls,
                option => option.MapFrom(s => s.MediaAssets.Where(m => m.IsCert == false).Select(m => m.Url)));


        CreateMap<AppUser, UserInfo>()
            .ForMember(d => d.Age, option => option.MapFrom(s => DateTime.Now.Year - s.Dob!.Value.Year));

        #region Contract -> ContractDetail

        CreateMap<Contract, ContractDetail>()
            .ForMember(d => d.Client,
                option => option.MapFrom(s => s.Client))
            .ForMember(d => d.Coach,
                option => option.MapFrom(s => s.Coach))
            .ForMember(d => d.Price,
                option => option.MapFrom(s => CurrencyHelper.GetVnd(s.Price)))
            .ForMember(d => d.CreatedDate,
                option => option.MapFrom(s => s.CreatedDate.ToString("dd/MM/yyyy")));

        #endregion

        CreateMap<FileAsset, FileResponse>()
            .ForMember(d => d.Date, option => option.MapFrom(s => s.Date.ToString("dd/MM/yyyy HH:mm:ss")))
            .ForMember(d => d.Size, option => option.MapFrom(s => FileSizeHelper.GetFileSize(s.Size)));


        #region Training log -> Training log response

        CreateMap<MediaAsset, MediaOnLogList>();
        CreateMap<TrainingLog, TrainingLogResponse>()
            .ForMember(d => d.Images,
                o => o.MapFrom(s => s.MediaAssets.Where(m => m.IsVideo == false)))
            .ForMember(d => d.Videos,
                o => o.MapFrom(s => s.MediaAssets.Where(m => m.IsVideo == true)))
            .ForMember(d => d.Files, o => o.MapFrom(s => s.FileAssets))
            .ForMember(d => d.TrainingDate,
                o => o.MapFrom(s => s.TrainingDate.Value.ToString("dd/MM/yyyy")))
            .ForMember(d => d.LastUpdatingDate,
                o => o.MapFrom(
                    s => s.LastUpdatingDate.Value.ToString("dd/MM/yyyy HH:mm:ss")));

        #endregion

        CreateMap<Report, ReportBaseDto>()
            .ForMember(d => d.CreatedDate,
                o => o.MapFrom(s => s.CreatedDate.ToString("dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo)))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.MediaAssets.Select(m => m.Url)));

        CreateMap<Report, ReportForAdmin>()
            .ForMember(d => d.CreatedDate,
                o => o.MapFrom(s => s.CreatedDate.ToString("dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo)))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.MediaAssets.Select(m => m.Url)));

        CreateMap<Voucher, VoucherDto>()
            .ForMember(d => d.Discount, o => o.MapFrom(s => s.Discount + "%"));
    }
}