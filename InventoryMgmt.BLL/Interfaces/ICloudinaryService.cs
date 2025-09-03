using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
