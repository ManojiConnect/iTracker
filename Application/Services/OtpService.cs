using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text;

namespace Application.Services;
public class OtpService
{
    private readonly IMemoryCache _cache;
    private readonly Random _random;

    public OtpService(IMemoryCache cache)
    {
        _cache = cache;
        _random = new Random();
    }

    // Generate a random OTP
    private string GenerateOtp()
    {
        int otpDigits = 4;
        StringBuilder sb = new();

        for (int i = 0; i < otpDigits; i++)
        {
            sb.Append(_random.Next(0, 10));
        }

        return sb.ToString();
    }

    // Send OTP to the user (Here, we just simulate sending an OTP via email or SMS)
    public string SetOtp(string email)
    {
        string otp = GenerateOtp();

        // Save the OTP in the cache with an expiration time (e.g., 60 seconds)
        TimeSpan cacheExpiration = TimeSpan.FromMinutes(5);
        _cache.Set(email, otp, cacheExpiration);

        return otp;
    }



    // Validate the user-entered OTP
    public bool ValidateOtp(string email, string userEnteredOtp)
    {
        // Retrieve the OTP from the cache
        if (_cache.TryGetValue(email, out string otpFromCache))
        {
            // Compare the user-entered OTP with the one in the cache
            if (otpFromCache == userEnteredOtp)
            {
                // OTP is valid; remove it from the cache
                _cache.Remove(email);
                return true;
            }
        }

        // OTP is either invalid or expired (not found in the cache)
        return false;
    }

}
