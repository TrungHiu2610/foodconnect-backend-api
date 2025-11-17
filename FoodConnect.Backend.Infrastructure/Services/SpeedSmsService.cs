using FoodConnect.Backend.Application.Commons.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class SpeedSmsService : ISmsService
    {
        private const int TYPE_QC = 1;
        private const int TYPE_CSKH = 2;
        private const int TYPE_BRANDNAME = 3;
        private const int TYPE_BRANDNAME_NOTIFY = 4;
        private const int TYPE_GATEWAY = 5;

        private const string ROOT_URL = "https://api.speedsms.vn/index.php";
        private readonly string _accessToken;
        private readonly ILogger<SpeedSmsService> _logger;

        public SpeedSmsService(IConfiguration configuration, ILogger<SpeedSmsService> logger)
        {
            _accessToken = configuration["SpeedSMS:ApiKey"] ?? string.Empty;
            _logger = logger;
        }

        public async Task SendOtpSmsAsync(string phoneNumber, string otp)
        {
            var content = $"[FoodConnect] Your FoodConnect verification code is: {otp}. Valid for 2 minutes.";
            var phones = new[] { phoneNumber };
            
            await Task.Run(() => SendSMS(phones, content, TYPE_BRANDNAME_NOTIFY, "0853947328"));
        }

        private string SendSMS(string[] phones, string content, int type, string sender)
        {
            string url = ROOT_URL + "/sms/send";
            
            if (phones.Length <= 0 || string.IsNullOrEmpty(content))
                throw new ArgumentException("Invalid phones or content");

            if (type == TYPE_BRANDNAME && string.IsNullOrEmpty(sender))
                throw new ArgumentException("Sender is required for BRANDNAME type");

            var credentials = new NetworkCredential(_accessToken, ":x");
            
            using (var client = new WebClient())
            {
                client.Credentials = credentials;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                var builder = new StringBuilder();
                builder.Append("{\"to\":[");

                for (int i = 0; i < phones.Length; i++)
                {
                    builder.Append($"\"{phones[i]}\"");
                    if (i < phones.Length - 1)
                    {
                        builder.Append(",");
                    }
                }
                
                var encodedContent = Uri.EscapeDataString(content);
                builder.Append($"], \"content\": \"{encodedContent}\", \"sms_type\":{type}");
                builder.Append($", \"sender\": \"{sender}\"");
                
                builder.Append("}");

                string json = builder.ToString();
                
                try
                {
                    _logger.LogInformation("SpeedSMS Request - URL: {Url}, JSON: {Json}", url, json);
                    var response = client.UploadString(url, json);
                    _logger.LogInformation("SpeedSMS Response: {Response}", response);
                    return response;
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            var errorResponse = reader.ReadToEnd();
                            _logger.LogError("SpeedSMS Error Response: {ErrorResponse}", errorResponse);
                            throw new Exception($"SpeedSMS API Error: {errorResponse}", ex);
                        }
                    }
                    
                    _logger.LogError(ex, "SpeedSMS WebException without response");
                    throw;
                }
            }
        }
    }
}
