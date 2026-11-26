using System.Security.Cryptography;
namespace VeterinariaApi;
public static class PasswordHasher
{
    public static (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210_000, HashAlgorithmName.SHA512, 32);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }
    public static bool Verify(string password, string encodedHash, string encodedSalt)
    {
        var expected = Convert.FromBase64String(encodedHash);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(encodedSalt), 210_000, HashAlgorithmName.SHA512, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
