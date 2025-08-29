using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface IItemRepo : IRepo<Item>
    {
        Task<IEnumerable<Item>> GetItemsByInventoryIdAsync(int inventoryId);
        Task<Item?> GetItemByCustomIdAsync(int inventoryId, string customId);
        Task<IEnumerable<Item>> GetLatestItemsAsync(int count);
        Task<IEnumerable<Item>> GetMostLikedItemsAsync(int count);
        Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm);
        Task<IEnumerable<Item>> GetItemsByUserAsync(int userId);
        Task<bool> IsCustomIdUniqueInInventoryAsync(int inventoryId, string customId, int? excludeItemId = null);
        Task<string> GenerateNextCustomIdAsync(int inventoryId);
    }
}
