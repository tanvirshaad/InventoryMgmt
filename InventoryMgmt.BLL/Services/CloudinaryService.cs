using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InventoryMgmt.BLL.DTOs;
using Microsoft.AspNetCore.Http;
using InventoryMgmt.BLL.Interfaces;

namespace InventoryMgmt.BLL.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            if (string.IsNullOrEmpty(config.Value.CloudName))
                throw new ArgumentException("Cloudinary CloudName is missing or empty");
                
            if (string.IsNullOrEmpty(config.Value.ApiKey))
                throw new ArgumentException("Cloudinary ApiKey is missing or empty");
                
            if (string.IsNullOrEmpty(config.Value.ApiSecret))
                throw new ArgumentException("Cloudinary ApiSecret is missing or empty");
                
            Console.WriteLine($"Creating Cloudinary account with CloudName: {config.Value.CloudName}");
                
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation()
                    .Width(1000).Height(1000).Crop("limit")
                    .Quality("auto:good")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
    }
}
