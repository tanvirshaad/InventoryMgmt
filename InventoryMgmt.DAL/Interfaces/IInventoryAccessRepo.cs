using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface IInventoryAccessRepo : IRepo<InventoryAccess>
    {
        Task<IEnumerable<InventoryAccess>> GetAccessesByInventoryIdAsync(int inventoryId);
        Task<IEnumerable<InventoryAccess>> GetAccessesByUserIdAsync(int userId);
        Task<InventoryAccess?> GetAccessAsync(int inventoryId, int userId);
        Task<bool> HasUserAccessAsync(int inventoryId, int userId);
        Task<bool> HasWriteAccessAsync(int userId, int inventoryId);
        Task<InventoryAccessPermission> GetUserPermissionAsync(int inventoryId, int userId);
        Task GrantAccessAsync(int inventoryId, int userId, InventoryAccessPermission permission = InventoryAccessPermission.Write);
        Task UpdatePermissionAsync(int inventoryId, int userId, InventoryAccessPermission permission);
        Task RevokeAccessAsync(int inventoryId, int userId);
    }
}
