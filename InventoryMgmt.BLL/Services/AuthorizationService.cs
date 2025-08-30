using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserRepo _userRepo;
        private readonly IInventoryRepo _inventoryRepo;
        private readonly IInventoryAccessRepo _inventoryAccessRepo;
        private readonly IItemRepo _itemRepo;
        private readonly ICommentRepo _commentRepo;

        public AuthorizationService(
            IUserRepo userRepo,
            IInventoryRepo inventoryRepo,
            IInventoryAccessRepo inventoryAccessRepo,
            IItemRepo itemRepo,
            ICommentRepo commentRepo)
        {
            _userRepo = userRepo;
            _inventoryRepo = inventoryRepo;
            _inventoryAccessRepo = inventoryAccessRepo;
            _itemRepo = itemRepo;
            _commentRepo = commentRepo;
        }

        public async Task<UserInventoryPermissions> GetUserInventoryPermissionsAsync(int? userId, int inventoryId)
        {
            var inventory = await _inventoryRepo.GetByIdAsync(inventoryId);
            if (inventory == null)
            {
                return new UserInventoryPermissions
                {
                    UserId = userId ?? 0,
                    InventoryId = inventoryId,
                    Permission = InventoryPermission.None,
                    UserRole = UserRole.Anonymous
                };
            }

            var userRole = await GetUserRoleAsync(userId);
            var isOwner = userId.HasValue && inventory.OwnerId == userId.Value;
            var hasWriteAccess = false;

            if (userId.HasValue)
            {
                hasWriteAccess = await _inventoryAccessRepo.HasWriteAccessAsync(userId.Value, inventoryId);
            }

            InventoryPermission permission;

            if (userRole == UserRole.Admin)
            {
                permission = InventoryPermission.FullControl;
            }
            else if (isOwner)
            {
                permission = InventoryPermission.FullControl;
            }
            else if (hasWriteAccess)
            {
                permission = InventoryPermission.Write;
            }
            else if (inventory.IsPublic && userRole != UserRole.Anonymous)
            {
                // Give authenticated users Write permission on public inventories
                permission = InventoryPermission.Write;
            }
            else if (inventory.IsPublic || userRole != UserRole.Anonymous)
            {
                permission = InventoryPermission.Read;
            }
            else
            {
                permission = InventoryPermission.None;
            }

            return new UserInventoryPermissions
            {
                UserId = userId ?? 0,
                InventoryId = inventoryId,
                Permission = permission,
                IsOwner = isOwner,
                HasWriteAccess = hasWriteAccess,
                IsPublic = inventory.IsPublic,
                UserRole = userRole
            };
        }

        public async Task<bool> CanViewInventoryAsync(int? userId, int inventoryId)
        {
            var permissions = await GetUserInventoryPermissionsAsync(userId, inventoryId);
            return permissions.Permission >= InventoryPermission.Read;
        }

        public async Task<bool> CanEditInventoryAsync(int? userId, int inventoryId)
        {
            var permissions = await GetUserInventoryPermissionsAsync(userId, inventoryId);
            return permissions.Permission >= InventoryPermission.FullControl;
        }

        public async Task<bool> CanManageInventoryAsync(int? userId, int inventoryId)
        {
            var permissions = await GetUserInventoryPermissionsAsync(userId, inventoryId);
            return permissions.Permission >= InventoryPermission.FullControl;
        }

        public async Task<bool> CanDeleteInventoryAsync(int? userId, int inventoryId)
        {
            var permissions = await GetUserInventoryPermissionsAsync(userId, inventoryId);
            return permissions.Permission >= InventoryPermission.FullControl;
        }

        public async Task<bool> CanGrantAccessAsync(int? userId, int inventoryId)
        {
            var permissions = await GetUserInventoryPermissionsAsync(userId, inventoryId);
            return permissions.Permission >= InventoryPermission.FullControl;
        }

        public async Task<bool> CanCreateItemAsync(int? userId, int inventoryId)
        {
            if (!userId.HasValue) return false;

            var permissions = await GetUserInventoryPermissionsAsync(userId, inventoryId);
            return permissions.Permission >= InventoryPermission.Write;
        }

        public async Task<bool> CanEditItemAsync(int? userId, int itemId)
        {
            if (!userId.HasValue) return false;

            var item = await _itemRepo.GetByIdAsync(itemId);
            if (item == null) return false;

            var permissions = await GetUserInventoryPermissionsAsync(userId, item.InventoryId);
            
            // Admin or inventory owner can edit any item
            if (permissions.Permission >= InventoryPermission.FullControl)
                return true;

            // Item creator with write access can edit their own item
            if (permissions.Permission >= InventoryPermission.Write && item.CreatedById == userId.Value)
                return true;

            return false;
        }

        public async Task<bool> CanDeleteItemAsync(int? userId, int itemId)
        {
            if (!userId.HasValue) return false;

            var item = await _itemRepo.GetByIdAsync(itemId);
            if (item == null) return false;

            var permissions = await GetUserInventoryPermissionsAsync(userId, item.InventoryId);
            
            // Admin or inventory owner can delete any item
            if (permissions.Permission >= InventoryPermission.FullControl)
                return true;

            // Item creator with write access can delete their own item
            if (permissions.Permission >= InventoryPermission.Write && item.CreatedById == userId.Value)
                return true;

            return false;
        }

        public async Task<bool> CanLikeItemAsync(int? userId, int itemId)
        {
            if (!userId.HasValue) return false;

            var item = await _itemRepo.GetByIdAsync(itemId);
            if (item == null) return false;

            return await CanViewInventoryAsync(userId, item.InventoryId);
        }

        public async Task<bool> CanCommentAsync(int? userId, int inventoryId)
        {
            if (!userId.HasValue) return false;

            return await CanViewInventoryAsync(userId, inventoryId);
        }

        public async Task<bool> CanEditCommentAsync(int? userId, int commentId)
        {
            if (!userId.HasValue) return false;

            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null) return false;

            var userRole = await GetUserRoleAsync(userId);
            
            // Admin can edit any comment
            if (userRole == UserRole.Admin)
                return true;

            // User can edit their own comment
            return comment.UserId == userId.Value;
        }

        public async Task<bool> CanDeleteCommentAsync(int? userId, int commentId)
        {
            if (!userId.HasValue) return false;

            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null) return false;

            var userRole = await GetUserRoleAsync(userId);
            
            // Admin can delete any comment
            if (userRole == UserRole.Admin)
                return true;

            // Inventory owner can delete comments in their inventory
            var inventory = await _inventoryRepo.GetByIdAsync(comment.InventoryId);
            if (inventory != null && inventory.OwnerId == userId.Value)
                return true;

            // User can delete their own comment
            return comment.UserId == userId.Value;
        }

        public async Task<bool> CanManageUsersAsync(int? userId)
        {
            return await IsAdminAsync(userId);
        }

        public async Task<bool> CanManageAdminsAsync(int? userId)
        {
            return await IsAdminAsync(userId);
        }

        public async Task<bool> CanBlockUserAsync(int? userId, int targetUserId)
        {
            if (!await IsAdminAsync(userId) || !userId.HasValue) return false;

            // Admin can block any user except themselves
            return userId.Value != targetUserId;
        }

        public async Task<bool> IsAdminAsync(int? userId)
        {
            if (!userId.HasValue) return false;

            var user = await _userRepo.GetByIdAsync(userId.Value);
            return user != null && !user.IsBlocked && 
                   (user.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true);
        }

        public async Task<bool> IsUserAsync(int? userId)
        {
            if (!userId.HasValue) return false;

            var user = await _userRepo.GetByIdAsync(userId.Value);
            return user != null && !user.IsBlocked;
        }

        public async Task<UserRole> GetUserRoleAsync(int? userId)
        {
            if (!userId.HasValue) return UserRole.Anonymous;

            var user = await _userRepo.GetByIdAsync(userId.Value);
            if (user == null || user.IsBlocked) return UserRole.Anonymous;

            if (user.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
                return UserRole.Admin;

            return UserRole.User;
        }
    }
}
