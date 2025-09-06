using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.MVC.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InventoryMgmt.MVC.Controllers
{
    [RequireAdmin]
    public class AdminController : BaseController
    {
        private readonly IUserService _userService;

        public AdminController(InventoryMgmt.BLL.Interfaces.IAuthorizationService authorizationService, IUserService userService) 
            : base(authorizationService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (!await _authorizationService.CanBlockUserAsync(currentUserId, userId))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "You don't have permission to block this user" });
                }
                return ForbiddenResult();
            }

            await _userService.BlockUserAsync(userId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "User has been blocked successfully" });
            }
            
            TempData["Success"] = "User has been blocked successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            await _userService.UnblockUserAsync(userId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "User has been unblocked successfully" });
            }
            
            TempData["Success"] = "User has been unblocked successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(int userId)
        {
            await _userService.SetUserRoleAsync(userId, "Admin");
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "User has been promoted to admin" });
            }
            
            TempData["Success"] = "User has been promoted to admin.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(int userId)
        {
            var currentUserId = GetCurrentUserId();
            await _userService.SetUserRoleAsync(userId, "User");
            
            if (currentUserId == userId)
            {
                // Admin removed their own admin access
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = "Your admin access has been removed. Redirecting...",
                        redirect = "/Home/Index" 
                    });
                }
                
                TempData["Success"] = "Your admin access has been removed.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Admin access has been removed from user" });
                }
                
                TempData["Success"] = "Admin access has been removed from user.";
                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == userId)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "You cannot delete your own account" });
                }
                
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            await _userService.DeleteUserAsync(userId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "User has been deleted successfully" });
            }
            
            TempData["Success"] = "User has been deleted successfully.";
            return RedirectToAction(nameof(Users));
        }
        
        // Bulk action models
        public class BulkUserActionModel
        {
            [JsonPropertyName("userIds")]
            public List<int> UserIds { get; set; } = new List<int>();
        }
        
        // Bulk action endpoints for AJAX requests
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUsers([FromBody] BulkUserActionModel model)
        {
            if (model == null || model.UserIds == null || model.UserIds.Count == 0)
            {
                return Json(new { success = false, message = "No users selected" });
            }
            
            var currentUserId = GetCurrentUserId();
            var successCount = 0;
            
            foreach (var userId in model.UserIds)
            {
                if (await _authorizationService.CanBlockUserAsync(currentUserId, userId))
                {
                    await _userService.BlockUserAsync(userId);
                    successCount++;
                }
            }
            
            return Json(new { 
                success = true, 
                message = $"{successCount} user(s) blocked successfully" 
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUsers([FromBody] BulkUserActionModel model)
        {
            if (model == null || model.UserIds == null || model.UserIds.Count == 0)
            {
                return Json(new { success = false, message = "No users selected" });
            }
            
            var successCount = 0;
            
            foreach (var userId in model.UserIds)
            {
                await _userService.UnblockUserAsync(userId);
                successCount++;
            }
            
            return Json(new { 
                success = true, 
                message = $"{successCount} user(s) unblocked successfully" 
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteUsersToAdmin([FromBody] BulkUserActionModel model)
        {
            if (model == null || model.UserIds == null || model.UserIds.Count == 0)
            {
                return Json(new { success = false, message = "No users selected" });
            }
            
            var successCount = 0;
            
            foreach (var userId in model.UserIds)
            {
                await _userService.SetUserRoleAsync(userId, "Admin");
                successCount++;
            }
            
            return Json(new { 
                success = true, 
                message = $"{successCount} user(s) promoted to admin successfully" 
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUsersAdmin([FromBody] BulkUserActionModel model)
        {
            if (model == null || model.UserIds == null || model.UserIds.Count == 0)
            {
                return Json(new { success = false, message = "No users selected" });
            }
            
            var currentUserId = GetCurrentUserId();
            var successCount = 0;
            var selfRemoved = false;
            
            foreach (var userId in model.UserIds)
            {
                await _userService.SetUserRoleAsync(userId, "User");
                successCount++;
                
                if (userId == currentUserId)
                {
                    selfRemoved = true;
                }
            }
            
            if (selfRemoved)
            {
                return Json(new { 
                    success = true,
                    message = "Your admin access has been removed. Redirecting...",
                    redirect = "/Home/Index"
                });
            }
            
            return Json(new { 
                success = true, 
                message = $"{successCount} user(s) removed from admin role successfully" 
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUsers([FromBody] BulkUserActionModel model)
        {
            if (model == null || model.UserIds == null || model.UserIds.Count == 0)
            {
                return Json(new { success = false, message = "No users selected" });
            }
            
            var currentUserId = GetCurrentUserId();
            var successCount = 0;
            
            foreach (var userId in model.UserIds)
            {
                if (userId != currentUserId)
                {
                    await _userService.DeleteUserAsync(userId);
                    successCount++;
                }
            }
            
            return Json(new { 
                success = true, 
                message = $"{successCount} user(s) deleted successfully" 
            });
        }
    }
}
