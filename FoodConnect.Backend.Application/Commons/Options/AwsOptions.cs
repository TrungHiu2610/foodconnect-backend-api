using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.Options
{
    public class AwsOptions
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string PublicBaseUrl { get; set; }
        public SESSettings SES { get; set; }
    }
    public class SESSettings
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
}
