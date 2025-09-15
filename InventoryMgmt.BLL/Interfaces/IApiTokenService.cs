using InventoryMgmt.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    public interface IApiTokenService
    {
        /// <summary>
        /// Generates a new API token for the specified inventory
        /// </summary>
        /// <param name="inventoryId">The ID of the inventory</param>
        /// <returns>The generated token</returns>
        Task<string> GenerateTokenForInventoryAsync(int inventoryId);

        /// <summary>
        /// Validates if the provided token is valid for the specified inventory
        /// </summary>
        /// <param name="token">The API token to validate</param>
        /// <param name="inventoryId">Optional inventory ID to check against</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateTokenAsync(string token, int? inventoryId = null);

        /// <summary>
        /// Gets the inventory ID associated with a token
        /// </summary>
        /// <param name="token">The API token</param>
        /// <returns>The associated inventory ID or null if token is invalid</returns>
        Task<int?> GetInventoryIdFromTokenAsync(string token);
        
        /// <summary>
        /// Gets the token for an inventory if it exists
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <returns>The token or null if no token exists</returns>
        Task<string?> GetTokenForInventoryAsync(int inventoryId);

        /// <summary>
        /// Revokes an existing API token for the specified inventory
        /// </summary>
        /// <param name="inventoryId">The ID of the inventory</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RevokeTokenAsync(int inventoryId);
    }
}