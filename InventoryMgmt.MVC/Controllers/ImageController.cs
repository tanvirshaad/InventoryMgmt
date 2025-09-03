using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.MVC.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.Interfaces;
using InventoryMgmt.DAL.EF.TableModels;
using System;

namespace InventoryMgmt.MVC.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IInventoryService _inventoryService;
        private readonly IItemRepo _itemRepo;
        private readonly InventoryMgmt.BLL.Interfaces.IAuthorizationService _authorizationService;

        public ImageController(
            ICloudinaryService cloudinaryService, 
            IInventoryService inventoryService,
            IItemRepo itemRepo,
            InventoryMgmt.BLL.Interfaces.IAuthorizationService authorizationService)
        {
            _cloudinaryService = cloudinaryService;
            _inventoryService = inventoryService;
            _itemRepo = itemRepo;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public IActionResult Upload(int? inventoryId = null, int? itemId = null, string? fieldName = null)
        {
            var model = new ImageUploadModel
            {
                InventoryId = inventoryId,
                ItemId = itemId,
                FieldName = fieldName
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ImageUploadModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check user permissions
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Forbid();
            }
            int userId = int.Parse(userIdClaim);
            
            // Handle Inventory image upload
            if (model.InventoryId.HasValue)
            {
                if (!await _authorizationService.CanEditInventoryAsync(userId, model.InventoryId.Value))
                {
                    return Forbid();
                }

                try
                {
                    string imageUrl = await _cloudinaryService.UploadImageAsync(model.Image);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Update inventory with the new image URL
                        var inventory = await _inventoryService.GetInventoryByIdAsync(model.InventoryId.Value);
                        if (inventory != null)
                        {
                            inventory.ImageUrl = imageUrl;
                            await _inventoryService.UpdateInventoryAsync(inventory);

                            TempData["SuccessMessage"] = "Image uploaded successfully";
                            return RedirectToAction("Details", "Inventory", new { id = model.InventoryId });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Inventory not found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error uploading image: {ex.Message}");
                }
            }
            
            // Handle Item document field upload
            else if (model.ItemId.HasValue && !string.IsNullOrEmpty(model.FieldName))
            {
                var item = await _itemRepo.GetByIdAsync(model.ItemId.Value);
                if (item == null)
                {
                    return NotFound();
                }

                // Check if user can edit this item (based on inventory permissions)
                if (!await _authorizationService.CanEditInventoryAsync(userId, item.InventoryId))
                {
                    return Forbid();
                }

                try
                {
                    string imageUrl = await _cloudinaryService.UploadImageAsync(model.Image);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Update the specific document field
                        switch (model.FieldName)
                        {
                            case "DocumentField1Value":
                                item.DocumentField1Value = imageUrl;
                                break;
                            case "DocumentField2Value":
                                item.DocumentField2Value = imageUrl;
                                break;
                            case "DocumentField3Value":
                                item.DocumentField3Value = imageUrl;
                                break;
                            default:
                                ModelState.AddModelError("", "Invalid field name specified");
                                return View(model);
                        }

                        item.UpdatedAt = DateTime.UtcNow;
                        _itemRepo.Update(item);
                        await _itemRepo.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Document uploaded successfully";
                        return RedirectToAction("Details", "Item", new { id = model.ItemId });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error uploading document: {ex.Message}");
                }
            }

            return View(model);
        }
    }
}
