using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.MVC.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryMgmt.MVC.Controllers
{
    public class AccessController : BaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IUserService _userService;

        public AccessController(
            InventoryMgmt.BLL.Interfaces.IAuthorizationService authorizationService,
            IInventoryService inventoryService,
            IUserService userService) : base(authorizationService)
        {
            _inventoryService = inventoryService;
            _userService = userService;
        }

        [RequireInventoryPermission(InventoryPermission.FullControl, "inventoryId")]
        public async Task<IActionResult> Manage(int inventoryId)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                return NotFound();
            }

            var accessUsers = await _inventoryService.GetInventoryAccessUsersAsync(inventoryId);
            
            ViewBag.Inventory = inventory;
            ViewBag.AccessUsers = accessUsers;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireInventoryPermission(InventoryPermission.FullControl, "inventoryId")]
        public async Task<IActionResult> GrantAccess(int inventoryId, string userEmail, InventoryPermission permission = InventoryPermission.Write)
        {
            // Check if the inventory is public
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                return NotFound(new { success = false, error = "Inventory not found" });
            }

            // If inventory is public, access control is not applicable
            if (inventory.IsPublic)
            {
                return Json(new { success = false, error = "Access control is not applicable for public inventories" });
            }

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Json(new { success = false, error = "Email address is required" });
            }

            var user = await _userService.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found with the provided email address" });
            }

            // Check if the user already has access
            var accessUsers = await _inventoryService.GetInventoryAccessUsersAsync(inventoryId);
            var existingAccess = accessUsers.FirstOrDefault(a => a.Email == userEmail);
            
            if (existingAccess != null)
            {
                return Json(new { 
                    success = false, 
                    warning = true,
                    error = $"{user.FirstName} {user.LastName} already has {existingAccess.AccessPermission} access to this inventory." 
                });
            }

            try
            {
                await _inventoryService.GrantUserAccessAsync(inventoryId, user.Id, permission);
                return Json(new { 
                    success = true, 
                    message = $"Access granted to {user.FirstName} {user.LastName} with {permission} permission",
                    user = new {
                        id = user.Id,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        email = user.Email,
                        permission = permission.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error granting access: {ex}");
                return Json(new { success = false, error = $"Failed to grant access: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireInventoryPermission(InventoryPermission.FullControl, "inventoryId")]
        public async Task<IActionResult> UpdatePermission(int inventoryId, int userId, InventoryPermission permission)
        {
            // Check if the inventory is public
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                return NotFound(new { success = false, error = "Inventory not found" });
            }

            // If inventory is public, access control is not applicable
            if (inventory.IsPublic)
            {
                return Json(new { success = false, error = "Access control is not applicable for public inventories" });
            }
            
            try
            {
                await _inventoryService.UpdateUserAccessPermissionAsync(inventoryId, userId, permission);
                return Json(new { success = true, message = $"Permission updated successfully to {permission}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Failed to update permission: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireInventoryPermission(InventoryPermission.FullControl, "inventoryId")]
        public async Task<IActionResult> RevokeAccess(int inventoryId, int userId)
        {
            // Check if the inventory is public
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                return NotFound(new { success = false, error = "Inventory not found" });
            }

            // If inventory is public, access control is not applicable
            if (inventory.IsPublic)
            {
                return Json(new { success = false, error = "Access control is not applicable for public inventories" });
            }
            
            try
            {
                await _inventoryService.RevokeUserAccessAsync(inventoryId, userId);
                return Json(new { success = true, message = "Access revoked successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Failed to revoke access: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new object[0]);
            }

            var users = await _userService.SearchUsersAsync(term);
            var userList = users.Select(u => new
            {
                id = u.Id,
                email = u.Email,
                name = $"{u.FirstName} {u.LastName}",
                displayText = $"{u.FirstName} {u.LastName} ({u.Email})"
            }).Take(10);

            return Json(userList);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireInventoryPermission(InventoryPermission.FullControl, "inventoryId")]
        public async Task<IActionResult> AddUser(int inventoryId, string email)
        {
            // Check if the inventory is public
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                TempData["Error"] = "Inventory not found.";
                return RedirectToAction("Index", "Inventory");
            }

            // If inventory is public, access control is not applicable
            if (inventory.IsPublic)
            {
                TempData["Warning"] = "Access control is not applicable for public inventories. Make the inventory private first.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Email address is required.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found with the provided email address.";
                    return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
                }

                // Check if the user already has access
                var accessUsers = await _inventoryService.GetInventoryAccessUsersAsync(inventoryId);
                var existingAccess = accessUsers.FirstOrDefault(a => a.Email == email);
                
                if (existingAccess != null)
                {
                    TempData["Info"] = $"{user.FirstName} {user.LastName} already has {existingAccess.AccessPermission} access to this inventory.";
                    return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
                }

                // Grant the user access
                await _inventoryService.GrantUserAccessAsync(inventoryId, user.Id, InventoryPermission.Write);
                
                // Success message with more details
                TempData["Success"] = $"Success! Access granted to {user.FirstName} {user.LastName} ({user.Email}) with Write permission. " +
                                     $"They can now add and edit items in this inventory.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to grant access: {ex.Message}";
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error granting access: {ex}");
            }

            return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
        }
        
        [HttpGet]
        // Custom authorization will be handled inside the method
        public async Task<IActionResult> AddUserSimple(int inventoryId, string email)
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
            }

            // Check user permission manually
            var permissions = await _authorizationService.GetUserInventoryPermissionsAsync(userId, inventoryId);
            if (permissions.Permission < InventoryPermission.FullControl)
            {
                TempData["Error"] = "You don't have permission to add users to this inventory.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId });
            }
            
            // Check if the inventory is public
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                TempData["Error"] = "Inventory not found.";
                return RedirectToAction("Index", "Inventory");
            }

            // If inventory is public, access control is not applicable
            if (inventory.IsPublic)
            {
                TempData["Warning"] = "Access control is not applicable for public inventories. Make the inventory private first.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Email address is required.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found with the provided email address.";
                    return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
                }

                // Check if the user already has access
                var accessUsers = await _inventoryService.GetInventoryAccessUsersAsync(inventoryId);
                var existingAccess = accessUsers.FirstOrDefault(a => a.Email == email);
                
                if (existingAccess != null)
                {
                    TempData["Info"] = $"{user.FirstName} {user.LastName} already has {existingAccess.AccessPermission} access to this inventory.";
                    return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
                }

                // Grant the user access
                await _inventoryService.GrantUserAccessAsync(inventoryId, user.Id, InventoryPermission.Write);
                
                // Success message with more details
                TempData["Success"] = $"Success! Access granted to {user.FirstName} {user.LastName} ({user.Email}) with Write permission. " +
                                      $"They can now add and edit items in this inventory.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to grant access: {ex.Message}";
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error granting access: {ex}");
            }

            return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserAccess(int inventoryId, int userId)
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
            }

            // Check user permission manually
            var permissions = await _authorizationService.GetUserInventoryPermissionsAsync(currentUserId, inventoryId);
            if (permissions.Permission < InventoryPermission.FullControl)
            {
                TempData["Error"] = "You don't have permission to remove users from this inventory.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId });
            }
            
            // Check if the inventory is public
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                TempData["Error"] = "Inventory not found.";
                return RedirectToAction("Index", "Inventory");
            }

            // If inventory is public, access control is not applicable
            if (inventory.IsPublic)
            {
                TempData["Warning"] = "Access control is not applicable for public inventories.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
            }

            try
            {
                // Get user details before removing for better messaging
                var users = await _inventoryService.GetInventoryAccessUsersAsync(inventoryId);
                var userToRemove = users.FirstOrDefault(u => u.Id == userId);
                
                // Remove the user access
                await _inventoryService.RevokeUserAccessAsync(inventoryId, userId);
                
                // Set success message
                if (userToRemove != null)
                {
                    TempData["Success"] = $"Successfully removed {userToRemove.FirstName} {userToRemove.LastName} ({userToRemove.Email}) from this inventory.";
                }
                else
                {
                    TempData["Success"] = "Successfully removed user access.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to remove user: {ex.Message}";
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error removing access: {ex}");
            }

            return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "access" });
        }
    }
}
