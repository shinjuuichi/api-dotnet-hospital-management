namespace WebAPI.Utils;

public static class CryptoPassword
{
    public static string EncryptPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool IsPasswordCorrect(string hashedPassword, string? plainTextPassword)
    {
        return plainTextPassword != null && BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
    }
}
