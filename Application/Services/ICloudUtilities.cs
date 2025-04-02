using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Application.Services;

public interface ICloudUtilities
{
    public Task<string> UploadFile(string containerName, string fileName, IFormFile file);

}
