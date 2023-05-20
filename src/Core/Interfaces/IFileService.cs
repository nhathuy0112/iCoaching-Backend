using Microsoft.AspNetCore.Http;

namespace Core.Interfaces;

public interface IFileService
{
    Task<string?> UploadAsync(IFormFile file);
    Task<Stream?> DownloadAsync(string downloadUrl);
    Task<bool> DeleteAsync(string url);
}