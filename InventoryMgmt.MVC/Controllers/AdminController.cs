using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.MVC.Attributes;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> BlockUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (!await _authorizationService.CanBlockUserAsync(currentUserId, userId))
            {
                return ForbiddenResult();
            }

            await _userService.BlockUserAsync(userId);
            TempData["Success"] = "User has been blocked successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            await _userService.UnblockUserAsync(userId);
            TempData["Success"] = "User has been unblocked successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(int userId)
        {
            await _userService.SetUserRoleAsync(userId, "Admin");
            TempData["Success"] = "User has been promoted to admin.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == userId)
            {
                // Admin can remove their own admin access
                await _userService.SetUserRoleAsync(userId, "User");
                TempData["Success"] = "Your admin access has been removed.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                await _userService.SetUserRoleAsync(userId, "User");
                TempData["Success"] = "Admin access has been removed from user.";
                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == userId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            await _userService.DeleteUserAsync(userId);
            TempData["Success"] = "User has been deleted successfully.";
            return RedirectToAction(nameof(Users));
        }
    }
}
