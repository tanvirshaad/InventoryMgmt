using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.MVC.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace InventoryMgmt.MVC.Controllers
{
    public class ItemController : BaseController
    {
        private readonly ItemService _itemService;
        private readonly IInventoryService _inventoryService;
        private readonly IAuthService _authService;

        public ItemController(
            ItemService itemService,
            IInventoryService inventoryService,
            IAuthService authService,
            InventoryMgmt.BLL.Interfaces.IAuthorizationService authorizationService) : base(authorizationService)
        {
            _itemService = itemService;
            _inventoryService = inventoryService;
            _authService = authService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = GetCurrentUserId();
            var item = await _itemService.GetItemByIdAsync(id, currentUserId?.ToString());

            if (item == null)
            {
                return View("~/Views/Item/ItemNotFound.cshtml");
            }

            // Check if user can view the inventory this item belongs to
            if (!await CanCurrentUserViewInventoryAsync(item.InventoryId))
            {
                return NotFoundOrForbiddenResult();
            }

            var canEditItem = await CanCurrentUserEditItemAsync(id);
            var canDeleteItem = await CanCurrentUserDeleteItemAsync(id);
            var canLikeItem = await CanCurrentUserLikeItemAsync(id);

            ViewBag.CanEditItem = canEditItem;
            ViewBag.CanDeleteItem = canDeleteItem;
            ViewBag.CanLikeItem = canLikeItem;
            ViewBag.CurrentUserId = currentUserId;

            return View(item);
        }

        [RequireAuthenticated]
        public async Task<IActionResult> Create(int inventoryId)
        {
            // Check if user can create items in this inventory
            if (!await CanCurrentUserCreateItemAsync(inventoryId))
            {
                return View("AccessDenied");
            }

            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                return View("~/Views/Inventory/InventoryNotFound.cshtml");
            }

            // Prepare the custom fields for the view
            await PrepareCustomFieldsForView(inventoryId, ViewBag);
            
            return View(new ItemDto { InventoryId = inventoryId });
        }

        [RequireAuthenticated]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemDto itemDto)
        {
            if (ModelState.IsValid)
            {
                // Check if user can create items in this inventory
                if (!await CanCurrentUserCreateItemAsync(itemDto.InventoryId))
                {
                    return ForbiddenResult();
                }

                var currentUserId = GetCurrentUserId();
                // Fix: Initialize CreatedById with a valid value to prevent validation errors
                itemDto.CreatedById = currentUserId?.ToString() ?? "1"; // Use a default ID if no user ID is found
                
                // Make sure we have a name value
                if (string.IsNullOrWhiteSpace(itemDto.Name))
                {
                    // Fall back to TextField1Value if no name provided
                    itemDto.Name = itemDto.TextField1Value ?? "Unnamed Item";
                }
                
                // Store the Name in TextField1Value for database persistence
                itemDto.TextField1Value = itemDto.Name;
                
                // Validate custom fields
                var inventory = await _inventoryService.GetInventoryByIdAsync(itemDto.InventoryId);
                if (inventory != null)
                {
                    // Validate numeric fields
                    ValidateNumericFields(itemDto, inventory, ModelState);
                    
                    // Validate text fields (for max length)
                    ValidateTextFields(itemDto, inventory, ModelState);
                }
                
                if (!ModelState.IsValid)
                {
                    // Get the custom fields for the form and return the view with validation errors
                    await PrepareCustomFieldsForView(itemDto.InventoryId, ViewBag);
                    ViewBag.Inventory = inventory;
                    return View(itemDto);
                }
                
                try
                {
                    var result = await _itemService.CreateItemAsync(itemDto);

                    if (result != null)
                    {
                        // Add success toast message and ensure it persists through redirect
                        TempData["ToastMessage"] = "Item created successfully!";
                        TempData["ToastType"] = "success";
                        TempData.Keep("ToastMessage");
                        TempData.Keep("ToastType");
                        
                        return RedirectToAction("Details", "Inventory", new { id = itemDto.InventoryId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create item");
                        TempData["ToastMessage"] = "Failed to create item";
                        TempData["ToastType"] = "error";
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating item: {ex.Message}");
                    // Add error toast message
                    TempData["ToastMessage"] = $"Error creating item: {ex.Message}";
                    TempData["ToastType"] = "error";
                    // Log the full exception for debugging
                    System.Diagnostics.Debug.WriteLine($"Item creation error: {ex}");
                }
            }

            // Prepare the custom fields for the view
            await PrepareCustomFieldsForView(itemDto.InventoryId, ViewBag);
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
                return View("~/Views/Item/ItemNotFound.cshtml");
            }

            var canEdit = await _inventoryService.AccessService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return View("AccessDenied");
            }
            
            // Get the inventory to get custom field configurations
            var inventory = await _inventoryService.GetInventoryByIdAsync(item.InventoryId);
            if (inventory == null)
            {
                return View("~/Views/Inventory/InventoryNotFound.cshtml");
            }
            
            // Prepare the custom fields for the view
            await PrepareCustomFieldsForView(item.InventoryId, ViewBag);

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

                var canEdit = await _inventoryService.AccessService.CanUserEditInventoryAsync(itemDto.InventoryId, currentUserId, isAdmin);
                if (!canEdit)
                {
                    return Forbid();
                }
                
                // Make sure we have a name value
                if (string.IsNullOrWhiteSpace(itemDto.Name))
                {
                    // Fall back to TextField1Value if no name provided
                    itemDto.Name = itemDto.TextField1Value ?? "Unnamed Item";
                }
                
                // Store the Name in TextField1Value for database persistence
                itemDto.TextField1Value = itemDto.Name;
                
                // Validate custom fields
                var inventory = await _inventoryService.GetInventoryByIdAsync(itemDto.InventoryId);
                if (inventory != null)
                {
                    // Validate numeric fields
                    ValidateNumericFields(itemDto, inventory, ModelState);
                    
                    // Validate text fields (for max length)
                    ValidateTextFields(itemDto, inventory, ModelState);
                }
                
                if (!ModelState.IsValid)
                {
                    // Get the custom fields for the form and return the view with validation errors
                    await PrepareCustomFieldsForView(itemDto.InventoryId, ViewBag);
                    ViewBag.Inventory = inventory;
                    return View(itemDto);
                }

                var result = await _itemService.UpdateItemAsync(itemDto);
                if (result != null)
                {
                    // Check if it's a new version due to concurrency
                    if (!result.Version.SequenceEqual(itemDto.Version))
                    {
                        // This is a concurrency resolution - update the form with latest data
                        ModelState.Clear(); // Clear validation errors
                        ModelState.AddModelError("", "The item has been updated with the latest version from the database. Please review and save your changes again.");
                        
                        // Reload the inventory and custom fields to ensure they're available
                        await PrepareCustomFieldsForView(result.InventoryId, ViewBag);
                        ViewBag.Inventory = await _inventoryService.GetInventoryByIdAsync(result.InventoryId);
                        
                        // Add toast message
                        TempData["ToastMessage"] = "Item has been updated by another user. Please review your changes.";
                        TempData["ToastType"] = "warning";
                        
                        // Return the refreshed item
                        return View(result);
                    }
                    
                    // Regular successful update - set persistent toast message
                    TempData["ToastMessage"] = "Item updated successfully!";
                    TempData["ToastType"] = "success";
                    TempData.Keep("ToastMessage");  // Make sure TempData persists through redirect
                    TempData.Keep("ToastType");
                    
                    return RedirectToAction("Details", "Inventory", new { id = itemDto.InventoryId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update item. Please try again.");
                    
                    // Add toast message for error
                    TempData["ToastMessage"] = "Failed to update item. Please try again.";
                    TempData["ToastType"] = "error";
                    
                    // Reload the inventory and custom fields to ensure they're available
                    await PrepareCustomFieldsForView(itemDto.InventoryId, ViewBag);
                    ViewBag.Inventory = await _inventoryService.GetInventoryByIdAsync(itemDto.InventoryId);
                }
            }

            // Make sure custom fields are loaded when returning the view
            await PrepareCustomFieldsForView(itemDto.InventoryId, ViewBag);
            ViewBag.Inventory = await _inventoryService.GetInventoryByIdAsync(itemDto.InventoryId);
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
                return View("~/Views/Item/ItemNotFound.cshtml");
            }

            var canEdit = await _inventoryService.AccessService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return View("AccessDenied");
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

            var canEdit = await _inventoryService.AccessService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            var result = await _itemService.DeleteItemAsync(id);
            if (result)
            {
                // Add success toast message
                TempData["ToastMessage"] = "Item deleted successfully!";
                TempData["ToastType"] = "success";
                
                return RedirectToAction("Details", "Inventory", new { id = item.InventoryId });
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete item");
                
                // Add error toast message
                TempData["ToastMessage"] = "Failed to delete item";
                TempData["ToastType"] = "error";
                
                return View(item);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new { 
                    success = false, 
                    message = "No items selected for deletion" 
                });
            }

            if (!User.Identity!.IsAuthenticated)
            {
                return Unauthorized(new { 
                    success = false, 
                    message = "You must be logged in to delete items",
                    requiresAuthentication = true
                });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var successCount = 0;
            var inventoryId = 0;
            var failedDueToPermissions = 0;
            var notFoundCount = 0;

            foreach (var id in ids)
            {
                var item = await _itemService.GetItemByIdAsync(id, currentUserId);
                if (item == null)
                {
                    notFoundCount++;
                    continue;
                }

                // Store the inventory ID for redirection
                if (inventoryId == 0)
                {
                    inventoryId = item.InventoryId;
                }

                var canEdit = await _inventoryService.AccessService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
                if (!canEdit)
                {
                    failedDueToPermissions++;
                    continue;
                }

                var result = await _itemService.DeleteItemAsync(id);
                if (result)
                {
                    successCount++;
                }
            }

            var message = $"Successfully deleted {successCount} of {ids.Count} items";
            if (notFoundCount > 0)
            {
                message += $" ({notFoundCount} items not found)";
            }
            if (failedDueToPermissions > 0)
            {
                message += $" ({failedDueToPermissions} items you don't have permission to delete)";
            }

            return Json(new { 
                success = successCount > 0, 
                message = message,
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
                return NotFound(new { 
                    success = false, 
                    message = "Item not found" 
                });
            }

            var canEdit = await _inventoryService.AccessService.CanUserEditInventoryAsync(originalItem.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return StatusCode(403, new {
                    success = false,
                    message = "You don't have permission to duplicate this item"
                });
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
                        itemId = result.Id,
                        inventoryId = result.InventoryId
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Failed to duplicate item" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error duplicating item: {ex.Message}" 
                });
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

            var canEdit = await _inventoryService.AccessService.CanUserEditInventoryAsync(item.InventoryId, currentUserId, isAdmin);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Unauthorized(new { 
                    success = false, 
                    message = "You must be logged in to like items",
                    requiresAuthentication = true
                });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { 
                    success = false, 
                    message = "Unable to determine your user ID",
                    requiresAuthentication = true
                });
            }

            try
            {
                // First check if the item exists
                var itemExists = await _itemService.GetItemByIdAsync(id, currentUserId);
                if (itemExists == null)
                {
                    return NotFound(new {
                        success = false,
                        message = "Item not found"
                    });
                }
                
                // Check if user has permission to like this item
                var canLike = await CanCurrentUserLikeItemAsync(id);
                if (!canLike)
                {
                    return StatusCode(403, new {
                        success = false,
                        message = "You don't have permission to like this item"
                    });
                }
                
                var result = await _itemService.ToggleLikeAsync(id, currentUserId);
                
                // Get the updated like count to return to the client
                var item = await _itemService.GetItemByIdAsync(id, currentUserId);
                int likesCount = item?.LikesCount ?? 0;
                
                return Json(new { 
                    success = true, 
                    isLiked = result, 
                    likesCount = likesCount,
                    liked = result, // For backward compatibility with existing code
                    message = result ? "Item liked" : "Item unliked"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error: {ex.Message}" 
                });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GenerateItemCustomId(int inventoryId)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Unauthorized(new { 
                    success = false, 
                    message = "You must be logged in to generate custom IDs",
                    requiresAuthentication = true
                });
            }
            
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
                if (inventory == null)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "Inventory not found" 
                    });
                }
                
                // Check if user has permission to create items in this inventory
                var canCreate = await CanCurrentUserCreateItemAsync(inventoryId);
                if (!canCreate)
                {
                    return StatusCode(403, new {
                        success = false,
                        message = "You don't have permission to create items in this inventory"
                    });
                }
                
                string customId;
                if (!string.IsNullOrEmpty(inventory.CustomIdFormat))
                {
                    // Use the inventory's custom ID format
                    customId = _inventoryService.CustomIdService.GenerateCustomId(inventory.CustomIdFormat, new Random().Next(1, 9999));
                }
                else
                {
                    // Use a default format
                    customId = $"ITEM-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                }
                
                return Json(new { 
                    success = true, 
                    customId = customId,
                    message = "Custom ID generated successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error generating custom ID: {ex.Message}" 
                });
            }
        }
        
        #region Private Helper Methods
        
        private void ValidateNumericFields(ItemDto itemDto, InventoryDto inventory, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            // Validate NumericField1
            if (inventory.NumericField1.NumericConfig != null && itemDto.NumericField1Value.HasValue)
            {
                if (inventory.NumericField1.NumericConfig.IsInteger && itemDto.NumericField1Value % 1 != 0)
                {
                    modelState.AddModelError("NumericField1Value", $"{inventory.NumericField1.Name} must be an integer.");
                }
                
                if (inventory.NumericField1.NumericConfig.MinValue.HasValue && 
                    itemDto.NumericField1Value < inventory.NumericField1.NumericConfig.MinValue)
                {
                    modelState.AddModelError("NumericField1Value", 
                        $"{inventory.NumericField1.Name} must be at least {inventory.NumericField1.NumericConfig.MinValue}.");
                }
                
                if (inventory.NumericField1.NumericConfig.MaxValue.HasValue && 
                    itemDto.NumericField1Value > inventory.NumericField1.NumericConfig.MaxValue)
                {
                    modelState.AddModelError("NumericField1Value", 
                        $"{inventory.NumericField1.Name} must be at most {inventory.NumericField1.NumericConfig.MaxValue}.");
                }
            }
            
            // Validate NumericField2
            if (inventory.NumericField2.NumericConfig != null && itemDto.NumericField2Value.HasValue)
            {
                if (inventory.NumericField2.NumericConfig.IsInteger && itemDto.NumericField2Value % 1 != 0)
                {
                    modelState.AddModelError("NumericField2Value", $"{inventory.NumericField2.Name} must be an integer.");
                }
                
                if (inventory.NumericField2.NumericConfig.MinValue.HasValue && 
                    itemDto.NumericField2Value < inventory.NumericField2.NumericConfig.MinValue)
                {
                    modelState.AddModelError("NumericField2Value", 
                        $"{inventory.NumericField2.Name} must be at least {inventory.NumericField2.NumericConfig.MinValue}.");
                }
                
                if (inventory.NumericField2.NumericConfig.MaxValue.HasValue && 
                    itemDto.NumericField2Value > inventory.NumericField2.NumericConfig.MaxValue)
                {
                    modelState.AddModelError("NumericField2Value", 
                        $"{inventory.NumericField2.Name} must be at most {inventory.NumericField2.NumericConfig.MaxValue}.");
                }
            }
            
            // Validate NumericField3
            if (inventory.NumericField3.NumericConfig != null && itemDto.NumericField3Value.HasValue)
            {
                if (inventory.NumericField3.NumericConfig.IsInteger && itemDto.NumericField3Value % 1 != 0)
                {
                    modelState.AddModelError("NumericField3Value", $"{inventory.NumericField3.Name} must be an integer.");
                }
                
                if (inventory.NumericField3.NumericConfig.MinValue.HasValue && 
                    itemDto.NumericField3Value < inventory.NumericField3.NumericConfig.MinValue)
                {
                    modelState.AddModelError("NumericField3Value", 
                        $"{inventory.NumericField3.Name} must be at least {inventory.NumericField3.NumericConfig.MinValue}.");
                }
                
                if (inventory.NumericField3.NumericConfig.MaxValue.HasValue && 
                    itemDto.NumericField3Value > inventory.NumericField3.NumericConfig.MaxValue)
                {
                    modelState.AddModelError("NumericField3Value", 
                        $"{inventory.NumericField3.Name} must be at most {inventory.NumericField3.NumericConfig.MaxValue}.");
                }
            }
        }
        
        private void ValidateTextFields(ItemDto itemDto, InventoryDto inventory, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            // Example max lengths - adjust as needed
            const int MaxShortTextLength = 255;
            const int MaxLongTextLength = 2000;
            
            // Validate text fields
            if (!string.IsNullOrEmpty(inventory.TextField1.Name) && 
                !string.IsNullOrEmpty(itemDto.TextField1Value) && 
                itemDto.TextField1Value.Length > MaxShortTextLength)
            {
                modelState.AddModelError("TextField1Value", 
                    $"{inventory.TextField1.Name} must be at most {MaxShortTextLength} characters.");
            }
            
            if (!string.IsNullOrEmpty(inventory.TextField2.Name) && 
                !string.IsNullOrEmpty(itemDto.TextField2Value) && 
                itemDto.TextField2Value.Length > MaxShortTextLength)
            {
                modelState.AddModelError("TextField2Value", 
                    $"{inventory.TextField2.Name} must be at most {MaxShortTextLength} characters.");
            }
            
            if (!string.IsNullOrEmpty(inventory.TextField3.Name) && 
                !string.IsNullOrEmpty(itemDto.TextField3Value) && 
                itemDto.TextField3Value.Length > MaxShortTextLength)
            {
                modelState.AddModelError("TextField3Value", 
                    $"{inventory.TextField3.Name} must be at most {MaxShortTextLength} characters.");
            }
            
            // Validate multitext fields
            if (!string.IsNullOrEmpty(inventory.MultiTextField1.Name) && 
                !string.IsNullOrEmpty(itemDto.MultiTextField1Value) && 
                itemDto.MultiTextField1Value.Length > MaxLongTextLength)
            {
                modelState.AddModelError("MultiTextField1Value", 
                    $"{inventory.MultiTextField1.Name} must be at most {MaxLongTextLength} characters.");
            }
            
            if (!string.IsNullOrEmpty(inventory.MultiTextField2.Name) && 
                !string.IsNullOrEmpty(itemDto.MultiTextField2Value) && 
                itemDto.MultiTextField2Value.Length > MaxLongTextLength)
            {
                modelState.AddModelError("MultiTextField2Value", 
                    $"{inventory.MultiTextField2.Name} must be at most {MaxLongTextLength} characters.");
            }
            
            if (!string.IsNullOrEmpty(inventory.MultiTextField3.Name) && 
                !string.IsNullOrEmpty(itemDto.MultiTextField3Value) && 
                itemDto.MultiTextField3Value.Length > MaxLongTextLength)
            {
                modelState.AddModelError("MultiTextField3Value", 
                    $"{inventory.MultiTextField3.Name} must be at most {MaxLongTextLength} characters.");
            }
        }
        
        private async Task PrepareCustomFieldsForView(int inventoryId, dynamic viewBag)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                return;
            }
            
            // Get the custom field configurations for this inventory
            var customFields = new List<object>();
            
            // Add text fields
            if (!string.IsNullOrEmpty(inventory.TextField1.Name)) 
                customFields.Add(new { 
                    type = "text", 
                    id = "TextField1Value", 
                    name = inventory.TextField1.Name, 
                    description = inventory.TextField1.Description,
                    maxLength = 255, // Max length for short text fields
                    required = true  // Make the first field required as it's used for the name
                });
            
            if (!string.IsNullOrEmpty(inventory.TextField2.Name)) 
                customFields.Add(new { 
                    type = "text", 
                    id = "TextField2Value", 
                    name = inventory.TextField2.Name, 
                    description = inventory.TextField2.Description,
                    maxLength = 255,
                    required = inventory.TextField2.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.TextField3.Name)) 
                customFields.Add(new { 
                    type = "text", 
                    id = "TextField3Value", 
                    name = inventory.TextField3.Name, 
                    description = inventory.TextField3.Description,
                    maxLength = 255,
                    required = inventory.TextField3.Required
                });
            
            // Add multitext fields
            if (!string.IsNullOrEmpty(inventory.MultiTextField1.Name)) 
                customFields.Add(new { 
                    type = "multitext", 
                    id = "MultiTextField1Value", 
                    name = inventory.MultiTextField1.Name, 
                    description = inventory.MultiTextField1.Description,
                    maxLength = 2000,
                    required = inventory.MultiTextField1.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.MultiTextField2.Name)) 
                customFields.Add(new { 
                    type = "multitext", 
                    id = "MultiTextField2Value", 
                    name = inventory.MultiTextField2.Name, 
                    description = inventory.MultiTextField2.Description,
                    maxLength = 2000,
                    required = inventory.MultiTextField2.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.MultiTextField3.Name)) 
                customFields.Add(new { 
                    type = "multitext", 
                    id = "MultiTextField3Value", 
                    name = inventory.MultiTextField3.Name, 
                    description = inventory.MultiTextField3.Description,
                    maxLength = 2000,
                    required = inventory.MultiTextField3.Required
                });
            
            // Add numeric fields
            if (!string.IsNullOrEmpty(inventory.NumericField1.Name)) 
                customFields.Add(new { 
                    type = "numeric", 
                    id = "NumericField1Value", 
                    name = inventory.NumericField1.Name, 
                    description = inventory.NumericField1.Description,
                    isInteger = inventory.NumericField1.NumericConfig?.IsInteger,
                    minValue = inventory.NumericField1.NumericConfig?.MinValue,
                    maxValue = inventory.NumericField1.NumericConfig?.MaxValue,
                    required = inventory.NumericField1.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.NumericField2.Name)) 
                customFields.Add(new { 
                    type = "numeric", 
                    id = "NumericField2Value", 
                    name = inventory.NumericField2.Name, 
                    description = inventory.NumericField2.Description,
                    isInteger = inventory.NumericField2.NumericConfig?.IsInteger,
                    minValue = inventory.NumericField2.NumericConfig?.MinValue,
                    maxValue = inventory.NumericField2.NumericConfig?.MaxValue,
                    required = inventory.NumericField2.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.NumericField3.Name)) 
                customFields.Add(new { 
                    type = "numeric", 
                    id = "NumericField3Value", 
                    name = inventory.NumericField3.Name, 
                    description = inventory.NumericField3.Description,
                    isInteger = inventory.NumericField3.NumericConfig?.IsInteger,
                    minValue = inventory.NumericField3.NumericConfig?.MinValue,
                    maxValue = inventory.NumericField3.NumericConfig?.MaxValue,
                    required = inventory.NumericField3.Required
                });
            
            // Add boolean fields
            if (!string.IsNullOrEmpty(inventory.BooleanField1.Name)) 
                customFields.Add(new { 
                    type = "boolean", 
                    id = "BooleanField1Value", 
                    name = inventory.BooleanField1.Name, 
                    description = inventory.BooleanField1.Description,
                    required = inventory.BooleanField1.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.BooleanField2.Name)) 
                customFields.Add(new { 
                    type = "boolean", 
                    id = "BooleanField2Value", 
                    name = inventory.BooleanField2.Name, 
                    description = inventory.BooleanField2.Description,
                    required = inventory.BooleanField2.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.BooleanField3.Name)) 
                customFields.Add(new { 
                    type = "boolean", 
                    id = "BooleanField3Value", 
                    name = inventory.BooleanField3.Name, 
                    description = inventory.BooleanField3.Description,
                    required = inventory.BooleanField3.Required
                });
            
            // Add document fields
            if (!string.IsNullOrEmpty(inventory.DocumentField1.Name)) 
                customFields.Add(new { 
                    type = "document", 
                    id = "DocumentField1Value", 
                    name = inventory.DocumentField1.Name, 
                    description = inventory.DocumentField1.Description,
                    required = inventory.DocumentField1.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.DocumentField2.Name)) 
                customFields.Add(new { 
                    type = "document", 
                    id = "DocumentField2Value", 
                    name = inventory.DocumentField2.Name, 
                    description = inventory.DocumentField2.Description,
                    required = inventory.DocumentField2.Required
                });
            
            if (!string.IsNullOrEmpty(inventory.DocumentField3.Name)) 
                customFields.Add(new { 
                    type = "document", 
                    id = "DocumentField3Value", 
                    name = inventory.DocumentField3.Name, 
                    description = inventory.DocumentField3.Description,
                    required = inventory.DocumentField3.Required
                });

            // Set up the ViewBag for the view
            viewBag.CustomFieldsJson = System.Text.Json.JsonSerializer.Serialize(customFields);
            viewBag.CustomFields = customFields;
            viewBag.Inventory = inventory;
        }
        
        #endregion
    }
}
