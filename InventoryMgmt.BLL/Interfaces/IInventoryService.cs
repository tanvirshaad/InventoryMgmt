using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Services;
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
        
        /// <summary>
        /// Gets a list of items for a specific inventory with optional paging
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="page">Optional page number (1-based)</param>
        /// <param name="pageSize">Optional page size</param>
        /// <returns>Collection of items</returns>
        Task<IEnumerable<ItemDto>> GetInventoryItemsAsync(int inventoryId, int? page = null, int? pageSize = null);
        
        // Reference to services instead of duplicating methods
        ITagService TagService { get; }
        IInventoryAccessService AccessService { get; }
        ICustomFieldService CustomFieldService { get; }
        ICustomIdService CustomIdService { get; }
    }
}
