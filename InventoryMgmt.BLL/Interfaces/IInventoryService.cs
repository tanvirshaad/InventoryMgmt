using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetLatestInventoriesAsync(int count);
        Task<IEnumerable<InventoryDto>> GetMostPopularInventoriesAsync(int count);
        Task<IEnumerable<InventoryDto>> SearchInventoriesAsync(string searchTerm);
        Task<InventoryDto?> GetInventoryByIdAsync(int id);
        Task<InventoryDto> CreateInventoryAsync(InventoryDto inventoryDto);
        Task<InventoryDto?> UpdateInventoryAsync(InventoryDto inventoryDto);
        Task<bool> DeleteInventoryAsync(int id);
        Task<IEnumerable<InventoryDto>> GetUserOwnedInventoriesAsync(int userId);
        Task<IEnumerable<InventoryDto>> GetUserAccessibleInventoriesAsync(int userId);
        Task<bool> CanUserEditInventoryAsync(int inventoryId, string userId, bool isAdmin = false);
        Task<bool> UpdateCustomIdConfigurationAsync(int inventoryId, List<CustomIdElement> elements);
        Task<bool> UpdateCustomFieldsAsync(int inventoryId, List<CustomFieldData> fields);
        Task<IEnumerable<UserDto>> GetInventoryAccessUsersAsync(int inventoryId);
        Task<bool> ClearAllCustomFieldsAsync(int inventoryId);
        Task<Inventory?> GetRawInventoryDataAsync(int id);
        Task GrantUserAccessAsync(int inventoryId, int userId);
        Task RevokeUserAccessAsync(int inventoryId, int userId);
        string GenerateCustomId(string format, int itemNumber);
        string GenerateAdvancedCustomId(List<CustomIdElement> elements, int itemNumber);
    }
}
