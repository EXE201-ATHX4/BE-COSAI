
using System.Globalization;
using System.Text;

namespace Core.Utils
{
    public class CoreHelper
    {
        public static DateTimeOffset SystemTimeNow => TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
        public static DateTime SystemTimeNows => TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now).DateTime;

        public static string ConvertVnString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Chuẩn hóa chuỗi Unicode về dạng chuẩn (dấu tách biệt)
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Chuyển đổi về dạng không dấu, chuyển thường và loại bỏ khoảng trắng thừa
            var result = stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant()
                .Trim()
                .Replace(" ", "");

            return result;
        }

        //public static string HashPassword(string passwordHasher)
        //{
        //    return BCrypt.Net.BCrypt.HashPassword(passwordHasher);
        //}

        //public static bool VerifyPassword(string password1, string password2)
        //{
        //    if (string.IsNullOrWhiteSpace(password2))
        //    {
        //        throw new ArgumentException("Mật khẩu đã lưu không thể rỗng.");
        //    }
        //    return BCrypt.Net.BCrypt.Verify(password1, password2);
        //}
    }
}

