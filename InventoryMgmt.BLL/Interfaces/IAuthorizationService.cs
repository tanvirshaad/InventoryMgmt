using InventoryMgmt.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    public interface IAuthorizationService
    {
        // Inventory permissions
        Task<UserInventoryPermissions> GetUserInventoryPermissionsAsync(int? userId, int inventoryId);
        Task<bool> CanViewInventoryAsync(int? userId, int inventoryId);
        Task<bool> CanEditInventoryAsync(int? userId, int inventoryId);
        Task<bool> CanManageInventoryAsync(int? userId, int inventoryId);
        Task<bool> CanDeleteInventoryAsync(int? userId, int inventoryId);
        Task<bool> CanGrantAccessAsync(int? userId, int inventoryId);

        // Item permissions
        Task<bool> CanCreateItemAsync(int? userId, int inventoryId);
        Task<bool> CanEditItemAsync(int? userId, int itemId);
        Task<bool> CanDeleteItemAsync(int? userId, int itemId);
        Task<bool> CanLikeItemAsync(int? userId, int itemId);

        // Comment permissions
        Task<bool> CanCommentAsync(int? userId, int inventoryId);
        Task<bool> CanEditCommentAsync(int? userId, int commentId);
        Task<bool> CanDeleteCommentAsync(int? userId, int commentId);

        // User management permissions (Admin only)
        Task<bool> CanManageUsersAsync(int? userId);
        Task<bool> CanManageAdminsAsync(int? userId);
        Task<bool> CanBlockUserAsync(int? userId, int targetUserId);

        // General role checks
        Task<bool> IsAdminAsync(int? userId);
        Task<bool> IsUserAsync(int? userId);
        Task<UserRole> GetUserRoleAsync(int? userId);
    }
}
