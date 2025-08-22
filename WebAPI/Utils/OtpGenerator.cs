namespace WebAPI.Utils;

public static class OtpGenerator
{
    private static readonly Random _random = new Random();

    public static string GenerateOtp()
    {
        return _random.Next(100000, 999999).ToString();
    }

    public static string HashOtp(string otp)
    {
        return BCrypt.Net.BCrypt.HashPassword(otp);
    }

    public static bool VerifyOtp(string otp, string hashedOtp)
    {
        return BCrypt.Net.BCrypt.Verify(otp, hashedOtp);
    }
}