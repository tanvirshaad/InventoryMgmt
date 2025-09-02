using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryMgmt.MVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly InventoryMgmt.BLL.Interfaces.IAuthorizationService _authorizationService;

        protected BaseController(InventoryMgmt.BLL.Interfaces.IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        protected async Task<bool> IsCurrentUserAdminAsync()
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.IsAdminAsync(userId);
        }

        protected async Task<UserRole> GetCurrentUserRoleAsync()
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.GetUserRoleAsync(userId);
        }

        protected async Task<UserInventoryPermissions> GetCurrentUserInventoryPermissionsAsync(int inventoryId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.GetUserInventoryPermissionsAsync(userId, inventoryId);
        }

        protected async Task<bool> CanCurrentUserViewInventoryAsync(int inventoryId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanViewInventoryAsync(userId, inventoryId);
        }

        protected async Task<bool> CanCurrentUserEditInventoryAsync(int inventoryId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanEditInventoryAsync(userId, inventoryId);
        }

        protected async Task<bool> CanCurrentUserManageInventoryAsync(int inventoryId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanManageInventoryAsync(userId, inventoryId);
        }

        protected async Task<bool> CanCurrentUserDeleteInventoryAsync(int inventoryId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanDeleteInventoryAsync(userId, inventoryId);
        }

        protected async Task<bool> CanCurrentUserCreateItemAsync(int inventoryId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanCreateItemAsync(userId, inventoryId);
        }

        protected async Task<bool> CanCurrentUserEditItemAsync(int itemId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanEditItemAsync(userId, itemId);
        }

        protected async Task<bool> CanCurrentUserDeleteItemAsync(int itemId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanDeleteItemAsync(userId, itemId);
        }

        protected async Task<bool> CanCurrentUserLikeItemAsync(int itemId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanLikeItemAsync(userId, itemId);
        }

        protected async Task<bool> CanCurrentUserCommentAsync(int inventoryId)
        {
            var userId = GetCurrentUserId();
            return await _authorizationService.CanCommentAsync(userId, inventoryId);
        }

        protected IActionResult ForbiddenResult()
        {
            return StatusCode(403, "You don't have permission to perform this action.");
        }

        protected IActionResult NotFoundOrForbiddenResult()
        {
            if (User.Identity!.IsAuthenticated)
            {
                // User is logged in but doesn't have permission
                return View("AccessDenied");
            }
            else
            {
                // User is not logged in
                return View("~/Views/Inventory/PrivateInventory.cshtml");
            }
        }
    }
}
