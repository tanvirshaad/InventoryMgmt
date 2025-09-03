using System;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using DotNetEnv;

namespace InventoryMgmt.BLL.Services
{
    public class DirectCloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public DirectCloudinaryService()
        {
            // Get values directly from environment or use hardcoded values as fallback
            var cloudName = Env.GetString("CLOUDINARY_CLOUD_NAME") ?? "dizix71bi";
            var apiKey = Env.GetString("CLOUDINARY_API_KEY") ?? "553389272478195";
            var apiSecret = Env.GetString("CLOUDINARY_API_SECRET") ?? "ktVBAr95SG8DnKS7ADaNHSwmGY0";
            
            Console.WriteLine($"DirectCloudinaryService - CloudName: {cloudName}, ApiKey: {apiKey.Substring(0, 3)}..., ApiSecret: {apiSecret.Substring(0, 3)}...");

            var acc = new Account(cloudName, apiKey, apiSecret);
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
