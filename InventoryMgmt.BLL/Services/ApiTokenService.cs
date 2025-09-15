using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public class ApiTokenService : IApiTokenService
    {
        private readonly IInventoryRepo _inventoryRepo;

        public ApiTokenService(IInventoryRepo inventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
        }

        public async Task<string> GenerateTokenForInventoryAsync(int inventoryId)
        {
            // Get the inventory
            var inventory = await _inventoryRepo.GetByIdAsync(inventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventory not found", nameof(inventoryId));

            // Generate a unique token (GUID + inventory ID + timestamp hash)
            string uniqueString = $"{Guid.NewGuid()}-{inventoryId}-{DateTime.UtcNow.Ticks}";
            string token = GenerateHash(uniqueString);

            // Store the token
            inventory.ApiToken = token;
            _inventoryRepo.Update(inventory);
            await _inventoryRepo.SaveChangesAsync();

            return token;
        }
        
        public async Task<bool> RevokeTokenAsync(int inventoryId)
        {
            // Get the inventory
            var inventory = await _inventoryRepo.GetByIdAsync(inventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventory not found", nameof(inventoryId));

            // Clear the token
            inventory.ApiToken = null;
            _inventoryRepo.Update(inventory);
            await _inventoryRepo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token, int? inventoryId = null)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            // If inventoryId is provided, check if token matches for that inventory
            if (inventoryId.HasValue)
            {
                var inventory = await _inventoryRepo.GetByIdAsync(inventoryId.Value);
                return inventory != null && inventory.ApiToken == token;
            }
            
            // Otherwise, check if token exists in any inventory
            var inventories = await _inventoryRepo.GetAllAsync();
            return inventories.Any(i => i.ApiToken == token);
        }

        public async Task<int?> GetInventoryIdFromTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var inventories = await _inventoryRepo.GetAllAsync();
            var inventory = inventories.FirstOrDefault(i => i.ApiToken == token);
            
            return inventory?.Id;
        }
        
        public async Task<string?> GetTokenForInventoryAsync(int inventoryId)
        {
            var inventory = await _inventoryRepo.GetByIdAsync(inventoryId);
            return inventory?.ApiToken;
        }

        // Helper method to generate a hash from a string
        private string GenerateHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                
                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString().Substring(0, 40); // Return first 40 chars for a shorter token
            }
        }
    }
}