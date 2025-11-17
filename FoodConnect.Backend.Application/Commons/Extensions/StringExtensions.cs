using System.Globalization;
using System.Text;

namespace FoodConnect.Backend.Application.Commons.Extensions;

public static class StringExtensions
{
    public static string RemoveVietnameseDiacritics(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        
        result = result.Replace('đ', 'd').Replace('Đ', 'D');
        
        return result;
    }

    public static string NormalizeForSearch(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return text.RemoveVietnameseDiacritics().ToLower().Trim();
    }
}
