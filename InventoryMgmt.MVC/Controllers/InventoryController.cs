using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryMgmt.DAL.EF.TableModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventoryMgmt.MVC.Models;
using InventoryMgmt.MVC.Attributes;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using InventoryMgmt.MVC.Hubs;

namespace InventoryMgmt.MVC.Controllers
{
    public class InventoryController : BaseController
    {
        private readonly InventoryService _inventoryService;
        private readonly ItemService _itemService;
        private readonly CategoryService _categoryService;
        private readonly IAuthService _authService;
        private readonly IHubContext<CommentHub> _hubContext; // Added for SignalR
        private readonly CommentService _commentService; // Added for comments

        public InventoryController(
            InventoryService inventoryService,
            ItemService itemService,
            CategoryService categoryService,
            IAuthService authService,
            IHubContext<CommentHub> hubContext, // Added for SignalR
            CommentService commentService, // Added for comments
            InventoryMgmt.BLL.Interfaces.IAuthorizationService authorizationService) : base(authorizationService)
        {
            _inventoryService = inventoryService;
            _itemService = itemService;
            _categoryService = categoryService;
            _authService = authService;
            _hubContext = hubContext; // Added for SignalR
            _commentService = commentService; // Added for comments
        }

        public async Task<IActionResult> Details(int id, string tab = "items")
        {
            // First check if inventory exists
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return View("InventoryNotFound");
            }
            
            // Then check if user can view this inventory
            if (!await CanCurrentUserViewInventoryAsync(id))
            {
                return NotFoundOrForbiddenResult();
            }

            var permissions = await GetCurrentUserInventoryPermissionsAsync(id);
            var currentUserId = GetCurrentUserId();

            ViewBag.CanEdit = permissions.Permission >= InventoryPermission.FullControl;
            ViewBag.CanAddItems = permissions.Permission >= InventoryPermission.Write;
            ViewBag.CanComment = await CanCurrentUserCommentAsync(id);
            ViewBag.CurrentTab = tab;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.UserPermissions = permissions;

            if (tab == "items")
            {
                var items = await _itemService.GetInventoryItemsAsync(id, currentUserId?.ToString());
                ViewBag.Items = items;
            }

            return View(inventory);
        }

        [RequireAuthenticated]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new InventoryDto());
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Test if we can access the database
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Json(new { success = true, categoriesCount = categories.Count() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [RequireAuthenticated]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryDto inventoryDto, string tagNames)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    ModelState.AddModelError("", "Could not determine current user. Please try logging out and back in.");
                    await PopulateCategoriesAsync();
                    return View(inventoryDto);
                }
                
                inventoryDto.OwnerId = currentUserId.Value;

                try
                {
                    var result = await _inventoryService.CreateInventoryAsync(inventoryDto);
                    
                    if (result != null)
                    {
                        // Process tags if provided
                        if (!string.IsNullOrEmpty(tagNames))
                        {
                            try
                            {
                                var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tagNames);
                                if (tags != null && tags.Any())
                                {
                                    await _inventoryService.TagService.AddTagsToInventoryAsync(result.Id, tags);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log the error but continue since the inventory was created successfully
                                System.Diagnostics.Debug.WriteLine($"Error processing tags: {ex.Message}");
                            }
                        }
                        
                        return RedirectToAction(nameof(Details), new { id = result.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create inventory - service returned null");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating inventory: {ex.Message}");
                }
            }

            await PopulateCategoriesAsync();
            return View(inventoryDto);
        }

        [RequireInventoryPermission(InventoryPermission.FullControl)]
        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            await PopulateCategoriesAsync();
            return View(inventory);
        }

        [RequireInventoryPermission(InventoryPermission.FullControl)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryDto inventoryDto)
        {
            if (id != inventoryDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _inventoryService.UpdateInventoryAsync(inventoryDto);
                    if (result != null)
                    {
                        // Tags are managed via AJAX, so we don't need to process them here
                        return RedirectToAction(nameof(Details), new { id = result.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update inventory");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while updating the inventory.");
                }
            }

            await PopulateCategoriesAsync();
            return View(inventoryDto);
        }

        [RequireInventoryPermission(InventoryPermission.FullControl)]
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        [RequireInventoryPermission(InventoryPermission.FullControl)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _inventoryService.DeleteInventoryAsync(id);
            if (result)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                ModelState.AddModelError("", "Failed to delete inventory");
                return View(inventory);
            }
        }

        [RequireAuthenticated]
        public async Task<IActionResult> MyInventories()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var ownedInventories = await _inventoryService.GetUserOwnedInventoriesAsync(userId.Value);
            var accessibleInventories = await _inventoryService.GetUserAccessibleInventoriesAsync(userId.Value);

            ViewBag.OwnedInventories = ownedInventories;
            ViewBag.AccessibleInventories = accessibleInventories;

            return View();
        }

        [HttpGet]
        public IActionResult GenerateCustomIdPreview(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return Json(new { preview = "" });
            }

            var preview = _inventoryService.CustomIdService.GenerateCustomId(format, 1);
            return Json(new { preview });
        }

        [HttpPost]
        public IActionResult GenerateAdvancedCustomIdPreview([FromBody] CustomIdPreviewRequest request)
        {
            if (request?.Elements == null || !request.Elements.Any())
            {
                return Json(new { preview = "" });
            }

            // Generate the preview
            var preview = _inventoryService.CustomIdService.GenerateAdvancedCustomId(request.Elements, 1);
            
            // Return the preview exactly as is - the client will handle the display
            return Json(new { 
                preview = preview
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomIdElements(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            
            // Ensure we return a valid list even if CustomIdElementList is null
            var elements = inventory.CustomIdElementList ?? new List<CustomIdElement>();

            // Return a clean JSON result with the elements
            return Json(new { elements = elements });
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomIdConfiguration([FromBody] CustomIdConfigurationRequest request)
        {
            try
            {
                if (request == null || request.InventoryId <= 0)
                {
                    return Json(new { success = false, error = "Invalid request" });
                }

                // Check permissions
                if (!await CanCurrentUserManageInventoryAsync(request.InventoryId))
                {
                    return Json(new { success = false, error = "Access denied" });
                }

                if (request.Elements == null)
                {
                    request.Elements = new List<CustomIdElement>();
                }

                var result = await _inventoryService.CustomIdService.UpdateCustomIdConfigurationAsync(request.InventoryId, request.Elements);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomFields(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var fields = new List<object>();
            
            // Add configured fields with proper ids
            if (!string.IsNullOrEmpty(inventory.TextField1.Name)) 
            {
                fields.Add(new { 
                    type = "text", 
                    id = "text-field-1",
                    name = inventory.TextField1.Name,
                    description = inventory.TextField1.Description,
                    showInTable = inventory.TextField1.ShowInTable,
                    order = 0
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.TextField2.Name)) 
            {
                fields.Add(new { 
                    type = "text", 
                    id = "text-field-2",
                    name = inventory.TextField2.Name,
                    description = inventory.TextField2.Description,
                    showInTable = inventory.TextField2.ShowInTable,
                    order = 1
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.TextField3.Name)) 
            {
                fields.Add(new { 
                    type = "text", 
                    id = "text-field-3",
                    name = inventory.TextField3.Name,
                    description = inventory.TextField3.Description,
                    showInTable = inventory.TextField3.ShowInTable,
                    order = 2
                });
            }
            
            // MultiText fields
            if (!string.IsNullOrEmpty(inventory.MultiTextField1.Name)) 
            {
                fields.Add(new { 
                    type = "multitext", 
                    id = "multitext-field-1",
                    name = inventory.MultiTextField1.Name,
                    description = inventory.MultiTextField1.Description,
                    showInTable = inventory.MultiTextField1.ShowInTable,
                    order = 3
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.MultiTextField2.Name)) 
            {
                fields.Add(new { 
                    type = "multitext", 
                    id = "multitext-field-2",
                    name = inventory.MultiTextField2.Name,
                    description = inventory.MultiTextField2.Description,
                    showInTable = inventory.MultiTextField2.ShowInTable,
                    order = 4
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.MultiTextField3.Name)) 
            {
                fields.Add(new { 
                    type = "multitext", 
                    id = "multitext-field-3",
                    name = inventory.MultiTextField3.Name,
                    description = inventory.MultiTextField3.Description,
                    showInTable = inventory.MultiTextField3.ShowInTable,
                    order = 5
                });
            }
            
            // Numeric fields
            if (!string.IsNullOrEmpty(inventory.NumericField1.Name)) 
            {
                fields.Add(new { 
                    type = "numeric", 
                    id = "numeric-field-1",
                    name = inventory.NumericField1.Name,
                    description = inventory.NumericField1.Description,
                    showInTable = inventory.NumericField1.ShowInTable,
                    numericConfig = inventory.NumericField1.NumericConfig,
                    order = 6
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.NumericField2.Name)) 
            {
                fields.Add(new { 
                    type = "numeric", 
                    id = "numeric-field-2",
                    name = inventory.NumericField2.Name,
                    description = inventory.NumericField2.Description,
                    showInTable = inventory.NumericField2.ShowInTable,
                    numericConfig = inventory.NumericField2.NumericConfig,
                    order = 7
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.NumericField3.Name)) 
            {
                fields.Add(new { 
                    type = "numeric", 
                    id = "numeric-field-3",
                    name = inventory.NumericField3.Name,
                    description = inventory.NumericField3.Description,
                    showInTable = inventory.NumericField3.ShowInTable,
                    numericConfig = inventory.NumericField3.NumericConfig,
                    order = 8
                });
            }
            
            // Document fields
            if (!string.IsNullOrEmpty(inventory.DocumentField1.Name)) 
            {
                fields.Add(new { 
                    type = "document", 
                    id = "document-field-1",
                    name = inventory.DocumentField1.Name,
                    description = inventory.DocumentField1.Description,
                    showInTable = inventory.DocumentField1.ShowInTable,
                    order = 9
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.DocumentField2.Name)) 
            {
                fields.Add(new { 
                    type = "document", 
                    id = "document-field-2",
                    name = inventory.DocumentField2.Name,
                    description = inventory.DocumentField2.Description,
                    showInTable = inventory.DocumentField2.ShowInTable,
                    order = 10
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.DocumentField3.Name)) 
            {
                fields.Add(new { 
                    type = "document", 
                    id = "document-field-3",
                    name = inventory.DocumentField3.Name,
                    description = inventory.DocumentField3.Description,
                    showInTable = inventory.DocumentField3.ShowInTable,
                    order = 11
                });
            }
            
            // Boolean fields
            if (!string.IsNullOrEmpty(inventory.BooleanField1.Name)) 
            {
                fields.Add(new { 
                    type = "boolean", 
                    id = "boolean-field-1",
                    name = inventory.BooleanField1.Name,
                    description = inventory.BooleanField1.Description,
                    showInTable = inventory.BooleanField1.ShowInTable,
                    order = 12
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.BooleanField2.Name)) 
            {
                fields.Add(new { 
                    type = "boolean", 
                    id = "boolean-field-2",
                    name = inventory.BooleanField2.Name,
                    description = inventory.BooleanField2.Description,
                    showInTable = inventory.BooleanField2.ShowInTable,
                    order = 13
                });
            }
            
            if (!string.IsNullOrEmpty(inventory.BooleanField3.Name)) 
            {
                fields.Add(new { 
                    type = "boolean", 
                    id = "boolean-field-3",
                    name = inventory.BooleanField3.Name,
                    description = inventory.BooleanField3.Description,
                    showInTable = inventory.BooleanField3.ShowInTable,
                    order = 14
                });
            }
            
            return Json(new { fields });
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomFields([FromBody] CustomFieldsRequest request)
        {
            try
            {
                if (request == null || request.InventoryId <= 0)
                {
                    return Json(new { success = false, error = "Invalid request" });
                }

                // Check permissions
                if (!await CanCurrentUserManageInventoryAsync(request.InventoryId))
                {
                    return Json(new { success = false, error = "Access denied" });
                }

                if (request.Fields == null)
                {
                    request.Fields = new List<CustomFieldData>();
                }

                if (request.Fields.Count == 0)
                {
                    // Clear all fields
                    bool clearResult = await _inventoryService.CustomFieldService.ClearAllCustomFieldsAsync(request.InventoryId);
                    return Json(new { success = clearResult });
                }

                var result = await _inventoryService.CustomFieldService.UpdateCustomFieldsAsync(request.InventoryId, request.Fields);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetComments(int id)
        {
            var comments = await _commentService.GetCommentsByInventoryIdAsync(id);
            var commentDtos = comments.Select(c => new
            {
                id = c.Id,
                content = c.Content, // Keep raw markdown content
                userName = $"{c.User?.FirstName} {c.User?.LastName}",
                createdAt = c.CreatedAt,
                userId = c.UserId
            });

            return Json(new { comments = commentDtos });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            if (!await CanCurrentUserCommentAsync(request.InventoryId))
            {
                return Json(new { success = false, message = "Access denied" });
            }
            
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Json(new { success = false, message = "Comment content cannot be empty" });
            }

            // Keep original markdown content
            var commentDto = new CommentDto
            {
                InventoryId = request.InventoryId,
                Content = request.Content,
                UserId = currentUserId.Value.ToString()
            };

            var result = await _commentService.AddCommentAsync(commentDto);
            if (result)
            {
                // Notify other users via SignalR
                // Make sure we're using the same event name that the client is listening for
                await _hubContext.Clients.Group($"inventory_{request.InventoryId}").SendAsync("CommentAdded", request.InventoryId);
            }

            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccessUsers(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            // If inventory is public, we don't need to return access users
            if (inventory.IsPublic)
            {
                return Json(new { isPublic = true, users = new object[0] });
            }

            var users = await _inventoryService.AccessService.GetInventoryAccessUsersAsync(id);
            var userDtos = users.Select(u => new
            {
                id = u.Id,
                firstName = u.FirstName,
                lastName = u.LastName,
                email = u.Email
            });

            return Json(new { isPublic = false, users = userDtos });
        }

        [HttpGet]
        public async Task<IActionResult> ExportData(int id, string format = "csv")
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var items = await _itemService.GetInventoryItemsAsync(id);
            
            if (format.ToLower() == "csv")
            {
                var csv = GenerateCsvExport(inventory, items);
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"{inventory.Title}_items.csv");
            }
            else if (format.ToLower() == "excel")
            {
                // Implement Excel export
                return Json(new { message = "Excel export not implemented yet" });
            }

            return BadRequest("Unsupported format");
        }

        [HttpGet]
        public async Task<IActionResult> ExportSettings(int id, string format = "json")
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            if (format.ToLower() == "json")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(inventory, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"{inventory.Title}_settings.json");
            }

            return BadRequest("Unsupported format");
        }
        
        #region Tag Management
        
        /// <summary>
        /// Gets all tags for an inventory
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInventoryTags(int id)
        {
            var tags = await _inventoryService.TagService.GetInventoryTagsAsync(id);
            return Json(new { tags = tags });
        }
        
        /// <summary>
        /// Searches for tags that match the provided search term (for autocomplete)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchTags(string term)
        {
            var tags = await _inventoryService.TagService.SearchTagsAsync(term);
            return Json(tags);
        }
        
        /// <summary>
        /// Gets the most popular tags
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPopularTags(int count = 10)
        {
            var tags = await _inventoryService.TagService.GetPopularTagsAsync(count);
            return Json(new { tags = tags });
        }
        
        /// <summary>
        /// Adds a tag to an inventory
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddTag(int inventoryId, string tagName)
        {
            if (!await CanCurrentUserManageInventoryAsync(inventoryId))
            {
                return Json(new { success = false, message = "Access denied" });
            }
            
            var result = await _inventoryService.TagService.AddTagToInventoryAsync(inventoryId, tagName);
            return Json(new { success = result });
        }
        
        /// <summary>
        /// Adds multiple tags to an inventory
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddTags(int inventoryId, [FromBody] List<string> tagNames)
        {
            if (!await CanCurrentUserManageInventoryAsync(inventoryId))
            {
                return Json(new { success = false, message = "Access denied" });
            }
            
            var result = await _inventoryService.TagService.AddTagsToInventoryAsync(inventoryId, tagNames);
            return Json(new { success = result });
        }
        
        /// <summary>
        /// Removes a tag from an inventory
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveTag(int inventoryId, int tagId)
        {
            if (!await CanCurrentUserManageInventoryAsync(inventoryId))
            {
                return Json(new { success = false, message = "Access denied" });
            }
            
            var result = await _inventoryService.TagService.RemoveTagFromInventoryAsync(inventoryId, tagId);
            return Json(new { success = result });
        }
        
        #endregion

        private string GenerateCsvExport(InventoryDto inventory, IEnumerable<ItemDto> items)
        {
            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("Custom ID,Created By,Created At,Updated At");
            
            // Add custom field headers
            var customFields = new List<string>();
            if (!string.IsNullOrEmpty(inventory.TextField1.Name)) customFields.Add(inventory.TextField1.Name);
            if (!string.IsNullOrEmpty(inventory.TextField2.Name)) customFields.Add(inventory.TextField2.Name);
            if (!string.IsNullOrEmpty(inventory.TextField3.Name)) customFields.Add(inventory.TextField3.Name);
            // Add other field types...
            
            if (customFields.Any())
            {
                csv.AppendLine(string.Join(",", customFields));
            }
            
            // Data rows
            foreach (var item in items)
            {
                var row = new List<string>
                {
                    item.CustomId,
                    item.CreatedBy?.FirstName + " " + item.CreatedBy?.LastName,
                    item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    item.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                // Add custom field values
                if (!string.IsNullOrEmpty(inventory.TextField1.Name)) row.Add(item.TextField1Value ?? "");
                if (!string.IsNullOrEmpty(inventory.TextField2.Name)) row.Add(item.TextField2Value ?? "");
                if (!string.IsNullOrEmpty(inventory.TextField3.Name)) row.Add(item.TextField3Value ?? "");
                // Add other field values...
                
                csv.AppendLine(string.Join(",", row));
            }
            
            return csv.ToString();
        }

        private async Task PopulateCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
            catch
            {
                ViewBag.Categories = new SelectList(new List<CategoryDto>(), "Id", "Name");
            }
        }
        

    }
}
