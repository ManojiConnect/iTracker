using System.Security.Cryptography;
using System.Text;

namespace Application.Services;
public static class PasswordGenerator
{
    public static string GeneratePassword()
    {
        int length = 8;
        const string lowercaseChars = "abcdefghjkmnopqrstuvwxyz";
        const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digitChars = "1234567890";
        const string specialChars = "@#$&";

        string validChars = lowercaseChars + uppercaseChars + digitChars + specialChars;
        char[] charArray = validChars.ToCharArray();

        using (var crypto = RandomNumberGenerator.Create())
        {
            byte[] data = new byte[length];
            crypto.GetBytes(data);

            StringBuilder passwordBuilder = new StringBuilder();
            passwordBuilder.Append(lowercaseChars[data[0] % lowercaseChars.Length]);
            passwordBuilder.Append(uppercaseChars[data[1] % uppercaseChars.Length]);
            passwordBuilder.Append(digitChars[data[2] % digitChars.Length]);
            passwordBuilder.Append(specialChars[data[3] % specialChars.Length]);

            for (int i = 4; i < length; i++)
            {
                passwordBuilder.Append(charArray[data[i] % charArray.Length]);
            }

            return passwordBuilder.ToString();
        }
    }
}

