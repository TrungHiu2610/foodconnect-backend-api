using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FoodConnect.Backend.Application.Commons.Helpers;

public static class VNPayHelper
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }
    public static string BuildRawData(Dictionary<string, string> parameters)
    {
        var data = new StringBuilder();
        
        var sortedParams = parameters
            .OrderBy(p => p.Key)
            .Where(p => !string.IsNullOrEmpty(p.Value));

        foreach (var param in sortedParams)
        {
            // Use WebUtility.UrlEncode for both key and value (VNPay standard)
            data.Append(WebUtility.UrlEncode(param.Key) + "=" + WebUtility.UrlEncode(param.Value) + "&");
        }

        // Remove last '&'
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }
    public static string BuildQueryString(Dictionary<string, string> parameters)
    {
        return BuildRawData(parameters);
    }

    public static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();
        var pairs = queryString.Split('&');

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                result[HttpUtility.UrlDecode(keyValue[0])] = HttpUtility.UrlDecode(keyValue[1]);
            }
        }

        return result;
    }
}
