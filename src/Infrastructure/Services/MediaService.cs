using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Http;
using System.Net;
using Core.Interfaces.Base;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class MediaService : IMediaService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IUnitOfWork _unitOfWork;

        public MediaService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _cloudinary = new Cloudinary(new Account
            (
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            ));
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMediaAsync(IFormFile file, MediaAsset mediaOption)
        {
            var uploadResult = await UploadMediaFileAsync(file, mediaOption.IsVideo, mediaOption.IsAvatar);

            if (uploadResult.StatusCode != HttpStatusCode.OK) return false;

            var media = new MediaAsset
            {
                Url = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
                IsAvatar = mediaOption.IsAvatar,
                UserId = mediaOption.UserId,
                CertificateSubmissionId = mediaOption.CertificateSubmissionId,
                OnPortfolio = mediaOption.OnPortfolio,
                ReportId = mediaOption.ReportId,
                TrainingLogId = mediaOption.TrainingLogId,
                IsVideo = mediaOption.IsVideo
                
            };

            _unitOfWork.Repository<MediaAsset>().Add(media);

            var result = await _unitOfWork.CompleteAsync();

            return result != 0;

        }

        public async Task<bool> DeleteMediaAsync(MediaAsset mediaAsset)
        {
            var deleteParams = new DeletionParams(mediaAsset.PublicId)
            {
                ResourceType = mediaAsset.IsVideo ? ResourceType.Video : ResourceType.Image
            };
            var result = await _cloudinary.DestroyAsync(deleteParams);
            if (result.Result != "ok") return false;
            _unitOfWork.Repository<MediaAsset>().Delete(mediaAsset);
            var res = await _unitOfWork.CompleteAsync();
            return res != 0;
        }

        public async Task<bool> UpdateImageAsync(IFormFile file, int id)
        {
            var photo = await _unitOfWork.Repository<MediaAsset>().GetByIdAsync(id);

            var uploadResult = await UploadMediaFileAsync(file, false ,photo.IsAvatar);

            if (uploadResult.StatusCode != HttpStatusCode.OK) return false;

            await _cloudinary.DestroyAsync(new DeletionParams(photo.PublicId));

            photo.Url = uploadResult.SecureUrl.AbsoluteUri;
            photo.PublicId = uploadResult.PublicId;

            _unitOfWork.Repository<MediaAsset>().Update(photo);
            var result = await _unitOfWork.CompleteAsync();

            return result != 0;
        }

        public async Task<UploadResult> UploadMediaFileAsync(IFormFile file, bool forVideo, bool isAvatar = false)
        {
            var uploadResult = new RawUploadResult();

            if (file.Length <= 0) return uploadResult;
            await using var stream = file.OpenReadStream();
            if (forVideo)
            {
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "ICoaching-Videos"
                };
                uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
            }
            else
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "ICoaching-Photos",
                    Transformation = isAvatar ? new Transformation().Crop("fill").Gravity("face").Width(500).Height(500) : 
                        new Transformation().Crop("fill").Gravity("face")
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            return uploadResult;
        }

        public async Task<bool> SaveMediaFiles(IFormFile[] files, ICollection<MediaAsset> collection, MediaAsset mediaAsset, bool forVideo = false)
        {
            foreach (var file in files)
            {
                var cloudinaryResult = await UploadMediaFileAsync(file, forVideo);
                
                if (cloudinaryResult.StatusCode != HttpStatusCode.OK)
                    return false;
                
                collection.Add(new MediaAsset()
                {
                    Url = cloudinaryResult.SecureUrl.AbsoluteUri,
                    PublicId = cloudinaryResult.PublicId,
                    UserId = mediaAsset.UserId,
                    IsAvatar = mediaAsset.IsAvatar,
                    IsVideo = mediaAsset.IsVideo,
                    IsCert = mediaAsset.IsCert,
                    ReportId = mediaAsset.ReportId,
                    OnPortfolio = mediaAsset.OnPortfolio,
                    TrainingLogId = mediaAsset.TrainingLogId
                });
            }

            return true;
        }

    }
}
