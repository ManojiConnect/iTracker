using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Application.Services;

public class CloudUtilities : ICloudUtilities
{
    public async Task<string> UploadFile(string containerName, string fileName, IFormFile file)
    {
        string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", containerName);

        // Create the folder if it doesn't exist
        if (!Directory.Exists(defaultPath))
            Directory.CreateDirectory(defaultPath);

        // Combine the folder and filename to get the full path
        var filePath = Path.Combine(defaultPath, fileName);

        // Copy the file to the specified path
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return filePath;
    }
}
