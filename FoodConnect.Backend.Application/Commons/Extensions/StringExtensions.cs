using System.Globalization;
using System.Text;

namespace FoodConnect.Backend.Application.Commons.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Removes Vietnamese diacritics from a string
    /// Example: "Hủ tiếu" -> "Hu tieu"
    /// </summary>
    public static string RemoveVietnameseDiacritics(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Normalize to decomposed form (separate base character and diacritic)
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            // Get Unicode category
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            
            // Skip non-spacing marks (diacritics)
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Special Vietnamese characters that don't follow standard decomposition
        var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        
        result = result.Replace('đ', 'd').Replace('Đ', 'D');
        
        return result;
    }

    /// <summary>
    /// Normalizes text for search (removes diacritics and converts to lowercase)
    /// Example: "Hủ Tiếu Nam Vang" -> "hu tieu nam vang"
    /// </summary>
    public static string NormalizeForSearch(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return text.RemoveVietnameseDiacritics().ToLower().Trim();
    }
}
