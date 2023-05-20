using CG.Web.MegaApiClient;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IMegaApiClient _megaApiClient;
    private readonly IConfiguration _configuration;
    
    public FileService (IConfiguration configuration)
    {
        _configuration = configuration;
        _megaApiClient = new MegaApiClient();
    }

    private async Task LoginAsync()
    {
        if (_megaApiClient.IsLoggedIn) return;
        await _megaApiClient.LoginAsync(_configuration["MegaAPI:Email"], _configuration["MegaAPI:Password"]);
    }

    private INode CreateDefaultFolder(string folderName)
    {
        var nodes = _megaApiClient.GetNodes();
        var parentNode = nodes.First(x => x.Type == NodeType.Root);
        var dirNode = nodes.FirstOrDefault(x => x.Type == NodeType.Directory && x.Name == folderName);
        return dirNode ?? _megaApiClient.CreateFolder(folderName, parentNode);
    }


    public async Task<string?> UploadAsync(IFormFile file)
    {
        try
        {
            await LoginAsync();
            
            var rootDir = CreateDefaultFolder("iCoaching");
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var uploadedFile = await _megaApiClient.UploadAsync(memoryStream, file.FileName, rootDir);
            if (uploadedFile == null) return null;
            var downloadUrl = await _megaApiClient.GetDownloadLinkAsync(uploadedFile);
            
            await _megaApiClient.LogoutAsync();
            
            return downloadUrl.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return null;
        }
        
    }

    public async Task<Stream?> DownloadAsync(string downloadUrl)
    {
        try
        {
            await _megaApiClient.LoginAnonymousAsync();
            var fileUri = new Uri(downloadUrl);
            var stream = await _megaApiClient.DownloadAsync(fileUri);
            await _megaApiClient.LogoutAsync();
            return stream;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return null;
        }
        
    }

    public async Task<bool> DeleteAsync(string url)
    {
        try
        {
            await LoginAsync();
            
            var fileUri = new Uri(url);
            var node = await _megaApiClient.GetNodeFromLinkAsync(fileUri);

            var nodes = await _megaApiClient.GetNodesAsync();
            var allFiles = nodes.Where(n => n.Type == NodeType.File).ToList();
            var myFile = allFiles.FirstOrDefault(f => f.Name == node.Name);
            await _megaApiClient.DeleteAsync(myFile, false);
            
            await _megaApiClient.LogoutAsync();
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return false;
        }
        
    }
}