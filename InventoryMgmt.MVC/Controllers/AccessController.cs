using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.MVC.Attributes;
using Microsoft.AspNetCore.Mvc;

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
        [RequireInventoryPermission(InventoryPermission.FullControl, "inventoryId")]
        public async Task<IActionResult> GrantAccess(int inventoryId, string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                TempData["Error"] = "Email address is required.";
                return RedirectToAction(nameof(Manage), new { inventoryId });
            }

            var user = await _userService.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["Error"] = "User not found with the provided email address.";
                return RedirectToAction(nameof(Manage), new { inventoryId });
            }

            try
            {
                await _inventoryService.GrantUserAccessAsync(inventoryId, user.Id);
                TempData["Success"] = $"Access granted to {user.FirstName} {user.LastName}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to grant access: {ex.Message}";
            }

            return RedirectToAction(nameof(Manage), new { inventoryId });
        }

        [HttpPost]
        [RequireInventoryPermission(InventoryPermission.FullControl, "inventoryId")]
        public async Task<IActionResult> RevokeAccess(int inventoryId, int userId)
        {
            try
            {
                await _inventoryService.RevokeUserAccessAsync(inventoryId, userId);
                TempData["Success"] = "Access revoked successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to revoke access: {ex.Message}";
            }

            return RedirectToAction(nameof(Manage), new { inventoryId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new { users = new object[0] });
            }

            var users = await _userService.SearchUsersAsync(term);
            var userList = users.Select(u => new
            {
                id = u.Id,
                email = u.Email,
                name = $"{u.FirstName} {u.LastName}",
                displayText = $"{u.FirstName} {u.LastName} ({u.Email})"
            }).Take(10);

            return Json(new { users = userList });
        }
    }
}
