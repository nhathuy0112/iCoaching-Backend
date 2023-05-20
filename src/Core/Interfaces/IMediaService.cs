using System.Collections;
using CloudinaryDotNet.Actions;
using Core.Entities;
using Core.Specifications;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces
{
    public interface IMediaService
    {
        Task<bool> AddMediaAsync(IFormFile file, MediaAsset mediaOption);
        Task<bool> DeleteMediaAsync(MediaAsset mediaAsset);
        Task<bool> UpdateImageAsync(IFormFile file, int id);
        Task<bool> SaveMediaFiles(IFormFile[] files, ICollection<MediaAsset> collection, MediaAsset mediaOptionObject, bool forVideo = false);
    }
}
