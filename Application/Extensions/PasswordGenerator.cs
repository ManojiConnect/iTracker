using System;
using System.Security.Cryptography;
using System.Text;

public static class PasswordGenerator
{
    public static string GenerateRandomPassword(int length = 12)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=";
        var randomBytes = new byte[length];
        RandomNumberGenerator.Fill(randomBytes);
        
        var result = new StringBuilder(length);
        foreach (var b in randomBytes)
        {
            result.Append(validChars[b % validChars.Length]);
        }
        
        return result.ToString();
    }
} 