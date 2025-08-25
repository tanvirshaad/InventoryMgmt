using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.DAL.EF.TableModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace InventoryMgmt.MVC.Controllers
{
    public class ItemController : Controller
    {
        private readonly ItemService _itemService;
        private readonly InventoryService _inventoryService;
        private readonly IAuthService _authService;

        public ItemController(
            ItemService itemService,
            InventoryService inventoryService,
            IAuthService authService)
        {
            _itemService = itemService;
            _inventoryService = inventoryService;
            _authService = authService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _itemService.GetItemByIdAsync(id, currentUserId);

            if (item == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            var canEdit = await _inventoryService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);

            ViewBag.CanEdit = canEdit;
            ViewBag.CurrentUserId = currentUserId;

            return View(item);
        }

        [Authorize]
        public async Task<IActionResult> Create(int inventoryId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(inventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                return NotFound();
            }

            ViewBag.Inventory = inventory;
            return View(new ItemDto { InventoryId = inventoryId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemDto itemDto)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var canEdit = await _inventoryService.CanUserEditInventoryAsync(itemDto.InventoryId, currentUserId, isAdmin);
                if (!canEdit)
                {
                    return Forbid();
                }

                // Fix: Initialize CreatedById with a valid value to prevent validation errors
                itemDto.CreatedById = currentUserId ?? "1"; // Use a default ID if no user ID is found
                
                try
                {
                    var result = await _itemService.CreateItemAsync(itemDto);

                    if (result != null)
                    {
                        return RedirectToAction("Details", "Inventory", new { id = itemDto.InventoryId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create item");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating item: {ex.Message}");
                    // Log the full exception for debugging
                    System.Diagnostics.Debug.WriteLine($"Item creation error: {ex}");
                }
            }

            var inventory = await _inventoryService.GetInventoryByIdAsync(itemDto.InventoryId);
            ViewBag.Inventory = inventory;
            return View(itemDto);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var item = await _itemService.GetItemByIdAsync(id, currentUserId);
            if (item == null)
            {
                return NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            return View(item);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemDto itemDto)
        {
            if (id != itemDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var canEdit = await _inventoryService.CanUserEditInventoryAsync(itemDto.InventoryId, currentUserId, isAdmin);
                if (!canEdit)
                {
                    return Forbid();
                }

                var result = await _itemService.UpdateItemAsync(itemDto);
                if (result != null)
                {
                    return RedirectToAction("Details", "Inventory", new { id = itemDto.InventoryId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update item");
                }
            }

            return View(itemDto);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var item = await _itemService.GetItemByIdAsync(id, currentUserId);
            if (item == null)
            {
                return NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            return View(item);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var item = await _itemService.GetItemByIdAsync(id, currentUserId);
            if (item == null)
            {
                return NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            var result = await _itemService.DeleteItemAsync(id);
            if (result)
            {
                return RedirectToAction("Details", "Inventory", new { id = item.InventoryId });
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete item");
                return View(item);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("No items selected for deletion");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var successCount = 0;
            var inventoryId = 0;

            foreach (var id in ids)
            {
                var item = await _itemService.GetItemByIdAsync(id, currentUserId);
                if (item == null)
                {
                    continue;
                }

                // Store the inventory ID for redirection
                if (inventoryId == 0)
                {
                    inventoryId = item.InventoryId;
                }

                var canEdit = await _inventoryService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
                if (!canEdit)
                {
                    continue;
                }

                var result = await _itemService.DeleteItemAsync(id);
                if (result)
                {
                    successCount++;
                }
            }

            return Json(new { 
                success = true, 
                message = $"Successfully deleted {successCount} of {ids.Count} items",
                inventoryId = inventoryId
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Duplicate(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var originalItem = await _itemService.GetItemByIdAsync(id, currentUserId);
            if (originalItem == null)
            {
                return NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(originalItem.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            // Create a new item based on the original one
            var newItem = new ItemDto
            {
                TextField1Value = $"{(originalItem.TextField1Value ?? originalItem.Name ?? "Unnamed Item")} (Copy)",
                MultiTextField1Value = originalItem.MultiTextField1Value ?? originalItem.Description,
                TextField2Value = originalItem.TextField2Value,
                TextField3Value = originalItem.TextField3Value,
                MultiTextField2Value = originalItem.MultiTextField2Value,
                MultiTextField3Value = originalItem.MultiTextField3Value,
                NumericField1Value = originalItem.NumericField1Value,
                NumericField2Value = originalItem.NumericField2Value,
                NumericField3Value = originalItem.NumericField3Value,
                DocumentField1Value = originalItem.DocumentField1Value,
                DocumentField2Value = originalItem.DocumentField2Value,
                DocumentField3Value = originalItem.DocumentField3Value,
                BooleanField1Value = originalItem.BooleanField1Value,
                BooleanField2Value = originalItem.BooleanField2Value,
                BooleanField3Value = originalItem.BooleanField3Value,
                InventoryId = originalItem.InventoryId,
                CreatedById = currentUserId ?? "1"
                // Note: CustomId will be auto-generated in ItemService
            };

            try
            {
                var result = await _itemService.CreateItemAsync(newItem);
                if (result != null)
                {
                    return Json(new { 
                        success = true, 
                        message = "Item duplicated successfully",
                        itemId = result.Id
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to duplicate item" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool isAjax = false)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var item = await _itemService.GetItemByIdAsync(id, currentUserId);
            if (item == null)
            {
                return isAjax ? Json(new { success = false, message = "Item not found" }) : NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return isAjax ? Json(new { success = false, message = "Not authorized" }) : Forbid();
            }

            var result = await _itemService.DeleteItemAsync(id);
            
            if (isAjax)
            {
                return Json(new { 
                    success = result, 
                    message = result ? "Item deleted successfully" : "Failed to delete item",
                    inventoryId = item.InventoryId
                });
            }
            
            if (result)
            {
                return RedirectToAction("Details", "Inventory", new { id = item.InventoryId });
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete item");
                return View(item);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int id)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Unauthorized();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _itemService.ToggleLikeAsync(id, currentUserId);
                return Json(new { success = true, isLiked = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
