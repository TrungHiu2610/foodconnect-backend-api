using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string? prefix, string fileName);
        Task<Stream> DownloadFileAsync(string fileKey);
        Task<bool> DeleteFileAsync(string fileKey);
    }
}
