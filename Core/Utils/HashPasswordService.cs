using System.Security.Cryptography;
using System.Text;

public static class HashPasswordService
{
    public static string HashPasswordTwice(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var firstHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var secondHash = sha256.ComputeHash(firstHash);
            return Convert.ToBase64String(secondHash);
        }
    }
}
