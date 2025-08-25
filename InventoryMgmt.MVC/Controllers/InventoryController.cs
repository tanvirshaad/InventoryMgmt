using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryMgmt.DAL.EF.TableModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventoryMgmt.MVC.Models; // Added for CustomIdPreviewRequest, CustomIdConfigurationRequest, CustomFieldsRequest, AddCommentRequest
using System.Text; // Added for StringBuilder
using System.Linq; // Added for Any()
using Microsoft.AspNetCore.SignalR; // Added for HubContext
using InventoryMgmt.MVC.Hubs; // Added for CommentHub

namespace InventoryMgmt.MVC.Controllers
{
    public class InventoryController : Controller
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
            CommentService commentService) // Added for comments
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
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var canEdit = await _inventoryService.CanUserEditInventoryAsync(id, currentUserId, isAdmin);

            ViewBag.CanEdit = canEdit;
            ViewBag.CurrentTab = tab;
            ViewBag.CurrentUserId = currentUserId;

            if (tab == "items")
            {
                var items = await _itemService.GetInventoryItemsAsync(id, currentUserId);
                ViewBag.Items = items;
            }

            return View(inventory);
        }

        [Authorize]
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

                [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryDto inventoryDto)
        {
            // Remove this debug model error as it interferes with form submission
            // ModelState.AddModelError("", $"DEBUG: Form submitted successfully - Title: {inventoryDto.Title}, CategoryId: {inventoryDto.CategoryId}, IsPublic: {inventoryDto.IsPublic}");
            
            // Debug: Log the incoming data
            System.Diagnostics.Debug.WriteLine($"Create action called with Title: {inventoryDto.Title}, CategoryId: {inventoryDto.CategoryId}");
            
            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is valid");
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                System.Diagnostics.Debug.WriteLine($"CurrentUserId: {currentUserId}");
                
                // Find the first User in the database if we can't parse the current user ID
                if (!int.TryParse(currentUserId, out int userId))
                {
                    // Try to get the authenticated user from the database
                    var userEmail = User.FindFirstValue(ClaimTypes.Email);
                    var currentUser = await _authService.GetUserByEmailAsync(userEmail);
                    
                    if (currentUser != null)
                    {
                        userId = currentUser.Id;
                        System.Diagnostics.Debug.WriteLine($"Retrieved user ID from email: {userId}");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Could not determine current user. Please try logging out and back in.");
                        await PopulateCategoriesAsync();
                        return View(inventoryDto);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Using userId: {userId}");
                inventoryDto.OwnerId = userId;

                try
                {
                    System.Diagnostics.Debug.WriteLine("Calling CreateInventoryAsync...");
                    var result = await _inventoryService.CreateInventoryAsync(inventoryDto);
                    System.Diagnostics.Debug.WriteLine($"CreateInventoryAsync result: {(result != null ? $"Success with ID {result.Id}" : "null")}");
                    
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Redirecting to Details with ID: {result.Id}");
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
                    // Log the full exception for debugging
                    System.Diagnostics.Debug.WriteLine($"Inventory creation error: {ex}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ModelState is invalid");
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
                }
            }

            await PopulateCategoriesAsync();
            return View(inventoryDto);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(id, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            await PopulateCategoriesAsync();
            return View(inventory);
        }

        [Authorize]
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
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var canEdit = await _inventoryService.CanUserEditInventoryAsync(id, currentUserId, isAdmin);
                if (!canEdit)
                {
                    return Forbid();
                }

                var result = await _inventoryService.UpdateInventoryAsync(inventoryDto);
                if (result != null)
                {
                    return RedirectToAction(nameof(Details), new { id = result.Id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update inventory");
                }
            }

            await PopulateCategoriesAsync();
            return View(inventoryDto);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(id, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            return View(inventory);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(id, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            var result = await _inventoryService.DeleteInventoryAsync(id);
            if (result)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete inventory");
                return View(inventory);
            }
        }

        [Authorize]
        public async Task<IActionResult> MyInventories()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userId, out int userIdInt))
            {
                var ownedInventories = await _inventoryService.GetUserOwnedInventoriesAsync(userIdInt);
                var accessibleInventories = await _inventoryService.GetUserAccessibleInventoriesAsync(userIdInt);

                            ViewBag.OwnedInventories = ownedInventories;
                ViewBag.AccessibleInventories = accessibleInventories;

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCustomIdPreview(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return Json(new { preview = "" });
            }

            var preview = _inventoryService.GenerateCustomId(format, 1);
            return Json(new { preview });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAdvancedCustomIdPreview([FromBody] CustomIdPreviewRequest request)
        {
            if (request?.Elements == null || !request.Elements.Any())
            {
                return Json(new { preview = "" });
            }

            var preview = _inventoryService.GenerateAdvancedCustomId(request.Elements, 1);
            return Json(new { preview });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomIdElements(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            return Json(new { elements = inventory.CustomIdElementList });
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomIdConfiguration([FromBody] CustomIdConfigurationRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(request.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            var result = await _inventoryService.UpdateCustomIdConfigurationAsync(request.InventoryId, request.Elements);
            return Json(new { success = result });
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
            
            // Add configured fields
            if (!string.IsNullOrEmpty(inventory.TextField1.Name)) fields.Add(new { type = "text", config = inventory.TextField1 });
            if (!string.IsNullOrEmpty(inventory.TextField2.Name)) fields.Add(new { type = "text", config = inventory.TextField2 });
            if (!string.IsNullOrEmpty(inventory.TextField3.Name)) fields.Add(new { type = "text", config = inventory.TextField3 });
            
            if (!string.IsNullOrEmpty(inventory.MultiTextField1.Name)) fields.Add(new { type = "multitext", config = inventory.MultiTextField1 });
            if (!string.IsNullOrEmpty(inventory.MultiTextField2.Name)) fields.Add(new { type = "multitext", config = inventory.MultiTextField2 });
            if (!string.IsNullOrEmpty(inventory.MultiTextField3.Name)) fields.Add(new { type = "multitext", config = inventory.MultiTextField3 });
            
            if (!string.IsNullOrEmpty(inventory.NumericField1.Name)) fields.Add(new { type = "numeric", config = inventory.NumericField1 });
            if (!string.IsNullOrEmpty(inventory.NumericField2.Name)) fields.Add(new { type = "numeric", config = inventory.NumericField2 });
            if (!string.IsNullOrEmpty(inventory.NumericField3.Name)) fields.Add(new { type = "numeric", config = inventory.NumericField3 });
            
            if (!string.IsNullOrEmpty(inventory.DocumentField1.Name)) fields.Add(new { type = "document", config = inventory.DocumentField1 });
            if (!string.IsNullOrEmpty(inventory.DocumentField2.Name)) fields.Add(new { type = "document", config = inventory.DocumentField2 });
            if (!string.IsNullOrEmpty(inventory.DocumentField3.Name)) fields.Add(new { type = "document", config = inventory.DocumentField3 });
            
            if (!string.IsNullOrEmpty(inventory.BooleanField1.Name)) fields.Add(new { type = "boolean", config = inventory.BooleanField1 });
            if (!string.IsNullOrEmpty(inventory.BooleanField2.Name)) fields.Add(new { type = "boolean", config = inventory.BooleanField2 });
            if (!string.IsNullOrEmpty(inventory.BooleanField3.Name)) fields.Add(new { type = "boolean", config = inventory.BooleanField3 });

            return Json(new { fields });
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomFields([FromBody] CustomFieldsRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var canEdit = await _inventoryService.CanUserEditInventoryAsync(request.InventoryId, currentUserId, isAdmin);
            if (!canEdit)
            {
                return Forbid();
            }

            var result = await _inventoryService.UpdateCustomFieldsAsync(request.InventoryId, request.Fields);
            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int id)
        {
            var comments = await _commentService.GetCommentsByInventoryIdAsync(id);
            var commentDtos = comments.Select(c => new
            {
                id = c.Id,
                content = c.Content,
                userName = $"{c.User?.FirstName} {c.User?.LastName}",
                createdAt = c.CreatedAt
            });

            return Json(new { comments = commentDtos });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var commentDto = new CommentDto
            {
                InventoryId = request.InventoryId,
                Content = request.Content,
                UserId = currentUserId
            };

            var result = await _commentService.AddCommentAsync(commentDto);
            if (result)
            {
                // Notify other users via SignalR
                await _hubContext.Clients.All.SendAsync("CommentAdded", request.InventoryId);
            }

            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccessUsers(int id)
        {
            var users = await _inventoryService.GetInventoryAccessUsersAsync(id);
            var userDtos = users.Select(u => new
            {
                id = u.Id,
                firstName = u.FirstName,
                lastName = u.LastName,
                email = u.Email
            });

            return Json(new { users = userDtos });
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
