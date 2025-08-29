using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface IInventoryTagRepo : IRepo<InventoryTag>
    {
        Task<IEnumerable<InventoryTag>> GetTagsByInventoryIdAsync(int inventoryId);
        Task<IEnumerable<InventoryTag>> GetInventoriesByTagIdAsync(int tagId);
        Task<InventoryTag?> GetInventoryTagAsync(int inventoryId, int tagId);
        Task AddTagToInventoryAsync(int inventoryId, int tagId);
        Task RemoveTagFromInventoryAsync(int inventoryId, int tagId);
        Task<bool> IsTagAssignedToInventoryAsync(int inventoryId, int tagId);
    }
}
