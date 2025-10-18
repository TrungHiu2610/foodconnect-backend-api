using Amazon.S3;
using Amazon.S3.Model;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class AwsS3FileStorageService : IFileStorageService
    {
        private readonly AwsOptions _options;
        private readonly IAmazonS3 _s3Client;

        public AwsS3FileStorageService(IOptions<AwsOptions> options, IAmazonS3 s3Client)
        {
            _options = options.Value;
            _s3Client = new AmazonS3Client(_options.AccessKey,_options.SecretKey, Amazon.RegionEndpoint.GetBySystemName(_options.Region));
        }
        public async Task<string> UploadFileAsync(IFormFile file, string? prefix)
        {
            try
            {
                var fileName = file.FileName;
                var uniqueName = $"{Guid.NewGuid()}/{fileName}";
                var fileKey = string.IsNullOrEmpty(prefix) ? uniqueName : $"{prefix}/{uniqueName}";

                var request = new PutObjectRequest()
                {
                    BucketName = _options.BucketName,
                    Key = fileKey,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(request);

                return $"{_options.PublicBaseUrl}/{fileKey}";
            }
            catch(Exception ex)
            {
                throw new Exception($"UploadFileAsync throw exception: {ex.Message}");
            }
        }
        public async Task<Stream> DownloadFileAsync(string fileKey)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = fileKey
                };

                var response = await _s3Client.GetObjectAsync(request);

                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                throw new Exception($"DownloadFileAsync throw exception: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string fileKey)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = fileKey
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                throw new Exception($"DownloadFileAsync throw exception: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFilesAsync(List<string> fileKeys)
        {
            try
            {
                var objects = fileKeys.Select(key => new KeyVersion { Key = key }).ToList();
                var request = new DeleteObjectsRequest
                {
                    BucketName = _options.BucketName,
                    Objects = objects
                };
                var response = await _s3Client.DeleteObjectsAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                throw new Exception($"DeleteFilesAsync throw exception: {ex.Message}");
            }
        }
    }
}
