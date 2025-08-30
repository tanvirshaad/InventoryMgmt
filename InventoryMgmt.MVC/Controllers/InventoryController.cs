using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryMgmt.DAL.EF.TableModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventoryMgmt.MVC.Models; // Added for CustomIdPreviewRequest, CustomIdConfigurationRequest, CustomFieldsRequest, AddCommentRequest
using InventoryMgmt.MVC.Attributes;
using System.Text; // Added for StringBuilder
using System.Linq; // Added for Any()
using Microsoft.AspNetCore.SignalR; // Added for HubContext
using InventoryMgmt.MVC.Hubs; // Added for CommentHub

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
            // Check if user can view this inventory
            if (!await CanCurrentUserViewInventoryAsync(id))
            {
                return NotFoundOrForbiddenResult();
            }

            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
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
                                    await _inventoryService.AddTagsToInventoryAsync(result.Id, tags);
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

            var preview = _inventoryService.GenerateCustomId(format, 1);
            return Json(new { preview });
        }

        [HttpPost]
        public IActionResult GenerateAdvancedCustomIdPreview([FromBody] CustomIdPreviewRequest request)
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
            System.Diagnostics.Debug.WriteLine($"GetCustomIdElements called for inventory ID: {id}");
            
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Inventory with ID {id} not found");
                return NotFound();
            }

            System.Diagnostics.Debug.WriteLine($"Found inventory: {inventory.Title} (ID: {inventory.Id})");
            System.Diagnostics.Debug.WriteLine($"Raw CustomIdElements JSON: {inventory.CustomIdElements}");
            System.Diagnostics.Debug.WriteLine($"Deserialized elements count: {inventory.CustomIdElementList?.Count ?? 0}");
            
            // Ensure we return a valid list even if CustomIdElementList is null
            var elements = inventory.CustomIdElementList ?? new List<CustomIdElement>();
            
            System.Diagnostics.Debug.WriteLine($"Returning {elements.Count} elements");
            
            if (elements.Any())
            {
                foreach (var element in elements)
                {
                    System.Diagnostics.Debug.WriteLine($"Element: Id={element.Id}, Type={element.Type}, Value={element.Value}, Order={element.Order}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No elements to return");
            }

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

                var result = await _inventoryService.UpdateCustomIdConfigurationAsync(request.InventoryId, request.Elements);
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
            System.Diagnostics.Debug.WriteLine($"GetCustomFields called for inventory ID: {id}");
            
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Inventory with ID {id} not found");
                return NotFound();
            }

            System.Diagnostics.Debug.WriteLine($"Found inventory: {inventory.Title} (ID: {inventory.Id})");
            
            // Debug output all field configurations
            System.Diagnostics.Debug.WriteLine("TextField1: " + (string.IsNullOrEmpty(inventory.TextField1.Name) ? "empty" : inventory.TextField1.Name));
            System.Diagnostics.Debug.WriteLine("TextField2: " + (string.IsNullOrEmpty(inventory.TextField2.Name) ? "empty" : inventory.TextField2.Name));
            System.Diagnostics.Debug.WriteLine("TextField3: " + (string.IsNullOrEmpty(inventory.TextField3.Name) ? "empty" : inventory.TextField3.Name));
            
            System.Diagnostics.Debug.WriteLine("NumericField1: " + (string.IsNullOrEmpty(inventory.NumericField1.Name) ? "empty" : inventory.NumericField1.Name));
            System.Diagnostics.Debug.WriteLine("NumericField2: " + (string.IsNullOrEmpty(inventory.NumericField2.Name) ? "empty" : inventory.NumericField2.Name));
            System.Diagnostics.Debug.WriteLine("NumericField3: " + (string.IsNullOrEmpty(inventory.NumericField3.Name) ? "empty" : inventory.NumericField3.Name));
            
            System.Diagnostics.Debug.WriteLine("BooleanField1: " + (string.IsNullOrEmpty(inventory.BooleanField1.Name) ? "empty" : inventory.BooleanField1.Name));
            System.Diagnostics.Debug.WriteLine("BooleanField2: " + (string.IsNullOrEmpty(inventory.BooleanField2.Name) ? "empty" : inventory.BooleanField2.Name));
            System.Diagnostics.Debug.WriteLine("BooleanField3: " + (string.IsNullOrEmpty(inventory.BooleanField3.Name) ? "empty" : inventory.BooleanField3.Name));

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
                System.Diagnostics.Debug.WriteLine($"Added text field 1: {inventory.TextField1.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added text field 2: {inventory.TextField2.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added text field 3: {inventory.TextField3.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added multitext field 1: {inventory.MultiTextField1.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added multitext field 2: {inventory.MultiTextField2.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added multitext field 3: {inventory.MultiTextField3.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added numeric field 1: {inventory.NumericField1.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added numeric field 2: {inventory.NumericField2.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added numeric field 3: {inventory.NumericField3.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added document field 1: {inventory.DocumentField1.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added document field 2: {inventory.DocumentField2.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added document field 3: {inventory.DocumentField3.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added boolean field 1: {inventory.BooleanField1.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added boolean field 2: {inventory.BooleanField2.Name}");
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
                System.Diagnostics.Debug.WriteLine($"Added boolean field 3: {inventory.BooleanField3.Name}");
            }

            System.Diagnostics.Debug.WriteLine($"Total fields added: {fields.Count}");
            
            // Enhanced debug information
            var debugInfo = new 
            {
                inventoryId = id,
                fieldsCount = fields.Count,
                rawFieldStatus = new 
                {
                    TextField1Present = !string.IsNullOrEmpty(inventory.TextField1.Name),
                    TextField2Present = !string.IsNullOrEmpty(inventory.TextField2.Name),
                    TextField3Present = !string.IsNullOrEmpty(inventory.TextField3.Name),
                    NumericField1Present = !string.IsNullOrEmpty(inventory.NumericField1.Name),
                    NumericField2Present = !string.IsNullOrEmpty(inventory.NumericField2.Name),
                    NumericField3Present = !string.IsNullOrEmpty(inventory.NumericField3.Name),
                    BooleanField1Present = !string.IsNullOrEmpty(inventory.BooleanField1.Name),
                    BooleanField2Present = !string.IsNullOrEmpty(inventory.BooleanField2.Name),
                    BooleanField3Present = !string.IsNullOrEmpty(inventory.BooleanField3.Name)
                }
            };
            
            return Json(new { fields, debug = debugInfo });
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
                    bool clearResult = await _inventoryService.ClearAllCustomFieldsAsync(request.InventoryId);
                    return Json(new { success = clearResult });
                }

                var result = await _inventoryService.UpdateCustomFieldsAsync(request.InventoryId, request.Fields);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugCustomFields(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            // Collect all field data directly from the database model
            var rawData = new
            {
                // Text fields
                TextField1 = new
                {
                    Name = inventory.TextField1.Name,
                    Description = inventory.TextField1.Description,
                    ShowInTable = inventory.TextField1.ShowInTable
                },
                TextField2 = new
                {
                    Name = inventory.TextField2.Name,
                    Description = inventory.TextField2.Description,
                    ShowInTable = inventory.TextField2.ShowInTable
                },
                TextField3 = new
                {
                    Name = inventory.TextField3.Name,
                    Description = inventory.TextField3.Description,
                    ShowInTable = inventory.TextField3.ShowInTable
                },
                // Numeric fields
                NumericField1 = new
                {
                    Name = inventory.NumericField1.Name,
                    Description = inventory.NumericField1.Description,
                    ShowInTable = inventory.NumericField1.ShowInTable,
                    IsInteger = inventory.NumericField1.NumericConfig?.IsInteger,
                    MinValue = inventory.NumericField1.NumericConfig?.MinValue,
                    MaxValue = inventory.NumericField1.NumericConfig?.MaxValue
                },
                NumericField2 = new
                {
                    Name = inventory.NumericField2.Name,
                    Description = inventory.NumericField2.Description,
                    ShowInTable = inventory.NumericField2.ShowInTable,
                    IsInteger = inventory.NumericField2.NumericConfig?.IsInteger,
                    MinValue = inventory.NumericField2.NumericConfig?.MinValue,
                    MaxValue = inventory.NumericField2.NumericConfig?.MaxValue
                },
                NumericField3 = new
                {
                    Name = inventory.NumericField3.Name,
                    Description = inventory.NumericField3.Description,
                    ShowInTable = inventory.NumericField3.ShowInTable,
                    IsInteger = inventory.NumericField3.NumericConfig?.IsInteger,
                    MinValue = inventory.NumericField3.NumericConfig?.MinValue,
                    MaxValue = inventory.NumericField3.NumericConfig?.MaxValue
                },
                // Boolean fields
                BooleanField1 = new
                {
                    Name = inventory.BooleanField1.Name,
                    Description = inventory.BooleanField1.Description,
                    ShowInTable = inventory.BooleanField1.ShowInTable
                },
                BooleanField2 = new
                {
                    Name = inventory.BooleanField2.Name,
                    Description = inventory.BooleanField2.Description,
                    ShowInTable = inventory.BooleanField2.ShowInTable
                },
                BooleanField3 = new
                {
                    Name = inventory.BooleanField3.Name,
                    Description = inventory.BooleanField3.Description,
                    ShowInTable = inventory.BooleanField3.ShowInTable
                },
            };
            
            return Json(new { 
                id = inventory.Id, 
                title = inventory.Title, 
                customFields = rawData,
                lastUpdated = inventory.UpdatedAt
            });
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
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            if (!await CanCurrentUserCommentAsync(request.InventoryId))
            {
                return Json(new { success = false, message = "Access denied" });
            }

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
        
        #region Tag Management
        
        /// <summary>
        /// Gets all tags for an inventory
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInventoryTags(int id)
        {
            var tags = await _inventoryService.GetInventoryTagsAsync(id);
            return Json(new { tags = tags });
        }
        
        /// <summary>
        /// Searches for tags that match the provided search term (for autocomplete)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchTags(string term)
        {
            var tags = await _inventoryService.SearchTagsAsync(term);
            return Json(tags);
        }
        
        /// <summary>
        /// Gets the most popular tags
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPopularTags(int count = 10)
        {
            var tags = await _inventoryService.GetPopularTagsAsync(count);
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
            
            var result = await _inventoryService.AddTagToInventoryAsync(inventoryId, tagName);
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
            
            var result = await _inventoryService.AddTagsToInventoryAsync(inventoryId, tagNames);
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
            
            var result = await _inventoryService.RemoveTagFromInventoryAsync(inventoryId, tagId);
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
        
        /// <summary>
        /// A diagnostic page to test the frontend interaction with the custom fields API endpoints.
        /// This will help identify if the issue is with AJAX calls or data processing.
        /// </summary>
        [HttpGet]
        public IActionResult TestCustomFieldsPage(int id = 5)
        {
            return Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>Custom Fields Test Page</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .container { max-width: 1200px; margin: 0 auto; }
        .card { border: 1px solid #ccc; padding: 15px; margin-bottom: 20px; border-radius: 5px; }
        .heading { background-color: #f5f5f5; padding: 10px; margin: -15px -15px 15px -15px; border-radius: 5px 5px 0 0; }
        pre { background-color: #f9f9f9; padding: 10px; border-radius: 5px; overflow: auto; max-height: 300px; }
        button { padding: 8px 12px; margin: 5px; cursor: pointer; }
        input, select { padding: 8px; margin: 5px 0; width: 100%; box-sizing: border-box; }
        .field-container { border: 1px solid #eee; padding: 10px; margin-bottom: 10px; border-radius: 3px; }
        .status { padding: 10px; margin: 10px 0; border-radius: 3px; }
        .success { background-color: #d4edda; color: #155724; }
        .error { background-color: #f8d7da; color: #721c24; }
        .controls { margin: 15px 0; display: flex; flex-wrap: wrap; }
        .results { margin-top: 20px; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>Custom Fields API Test</h1>
        <p>This page helps diagnose issues with the custom fields API endpoints.</p>
        
        <div class='card'>
            <div class='heading'>
                <h3>Inventory ID: <span id='inventoryId'>" + id + @"</span></h3>
            </div>
            <div class='controls'>
                <button id='fetchBtn'>Fetch Custom Fields</button>
                <button id='saveDemoBtn'>Save Demo Fields</button>
                <button id='clearBtn'>Clear All Fields</button>
                <button id='fetchDtoBtn'>Get DTO Debug</button>
                <button id='fetchDbBtn'>Get DB Debug</button>
            </div>
            
            <div class='status' id='status'></div>
            
            <div class='results'>
                <h4>API Response:</h4>
                <pre id='results'>Click a button above to see results</pre>
            </div>
        </div>
        
        <div class='card'>
            <div class='heading'>
                <h3>Add New Custom Field</h3>
            </div>
            <div id='newFieldForm'>
                <div class='field-container'>
                    <label for='fieldType'>Field Type:</label>
                    <select id='fieldType'>
                        <option value='text'>Text</option>
                        <option value='multitext'>Multi-line Text</option>
                        <option value='numeric'>Numeric</option>
                        <option value='boolean'>Boolean</option>
                        <option value='document'>Document</option>
                    </select>
                    
                    <label for='fieldName'>Field Name:</label>
                    <input type='text' id='fieldName' placeholder='Enter field name'>
                    
                    <label for='fieldDescription'>Description:</label>
                    <input type='text' id='fieldDescription' placeholder='Enter field description'>
                    
                    <label>
                        <input type='checkbox' id='showInTable'> Show in table
                    </label>
                    
                    <div id='numericOptions' style='display:none; padding: 10px; margin-top: 10px; border: 1px dashed #ccc;'>
                        <h4>Numeric Options</h4>
                        <label>
                            <input type='checkbox' id='isInteger'> Integer only
                        </label>
                        <label for='minValue'>Minimum Value:</label>
                        <input type='number' id='minValue' placeholder='Min value'>
                        <label for='maxValue'>Maximum Value:</label>
                        <input type='number' id='maxValue' placeholder='Max value'>
                    </div>
                </div>
                <button id='addFieldBtn'>Add Field</button>
            </div>
        </div>

        <div class='card'>
            <div class='heading'>
                <h3>Current Fields</h3>
            </div>
            <div id='currentFields'>No fields loaded yet</div>
            <div class='controls'>
                <button id='saveCurrentBtn' disabled>Save Changes</button>
            </div>
        </div>
    </div>
    
    <script>
        // Store fields data globally
        let fields = [];
        const inventoryId = parseInt(document.getElementById('inventoryId').textContent);
        
        // Element references
        const statusEl = document.getElementById('status');
        const resultsEl = document.getElementById('results');
        const currentFieldsEl = document.getElementById('currentFields');
        const fieldTypeEl = document.getElementById('fieldType');
        const numericOptionsEl = document.getElementById('numericOptions');
        
        // Show status message
        function showStatus(message, isError = false) {
            statusEl.textContent = message;
            statusEl.className = 'status ' + (isError ? 'error' : 'success');
        }
        
        // Display results in the pre element
        function showResults(data) {
            resultsEl.textContent = typeof data === 'string' ? data : JSON.stringify(data, null, 2);
        }
        
        // Fetch custom fields from API
        document.getElementById('fetchBtn').addEventListener('click', () => {
            showStatus('Fetching custom fields...');
            
            fetch(`/Inventory/GetCustomFields?id=${inventoryId}`)
                .then(response => response.json())
                .then(data => {
                    showStatus(`Fetched ${data.fields.length} custom fields`);
                    showResults(data);
                    
                    // Store and display fields
                    fields = data.fields;
                    renderCurrentFields();
                })
                .catch(error => {
                    showStatus('Error fetching fields: ' + error.message, true);
                    console.error('Error:', error);
                });
        });
        
        // Save demo fields
        document.getElementById('saveDemoBtn').addEventListener('click', () => {
            const demoFields = [
                {
                    type: 'text',
                    id: 'text-field-1',
                    name: 'Demo Text Field',
                    description: 'This is a demo text field',
                    showInTable: true,
                    order: 0
                },
                {
                    type: 'numeric',
                    id: 'numeric-field-1',
                    name: 'Demo Numeric Field',
                    description: 'This is a demo numeric field',
                    showInTable: true,
                    numericConfig: {
                        isInteger: true,
                        minValue: 0,
                        maxValue: 100
                    },
                    order: 1
                },
                {
                    type: 'boolean',
                    id: 'boolean-field-1',
                    name: 'Demo Boolean Field',
                    description: 'This is a demo boolean field',
                    showInTable: true,
                    order: 2
                }
            ];
            
            saveFields(demoFields);
        });
        
        // Clear all fields
        document.getElementById('clearBtn').addEventListener('click', () => {
            if (confirm('Are you sure you want to clear all custom fields?')) {
                saveFields([]);
            }
        });
        
        // Get DTO debug
        document.getElementById('fetchDtoBtn').addEventListener('click', () => {
            showStatus('Fetching DTO debug info...');
            
            fetch(`/Inventory/DtoDebug?id=${inventoryId}`)
                .then(response => response.json())
                .then(data => {
                    showStatus('Fetched DTO debug info');
                    showResults(data);
                })
                .catch(error => {
                    showStatus('Error fetching DTO debug: ' + error.message, true);
                    console.error('Error:', error);
                });
        });
        
        // Get DB debug
        document.getElementById('fetchDbBtn').addEventListener('click', () => {
            showStatus('Fetching database debug info...');
            
            fetch(`/Inventory/DatabaseDebug?id=${inventoryId}`)
                .then(response => response.json())
                .then(data => {
                    showStatus('Fetched database debug info');
                    showResults(data);
                })
                .catch(error => {
                    showStatus('Error fetching DB debug: ' + error.message, true);
                    console.error('Error:', error);
                });
        });
        
        // Handle field type change
        fieldTypeEl.addEventListener('change', () => {
            numericOptionsEl.style.display = 
                fieldTypeEl.value === 'numeric' ? 'block' : 'none';
        });
        
        // Add new field
        document.getElementById('addFieldBtn').addEventListener('click', () => {
            const type = fieldTypeEl.value;
            const name = document.getElementById('fieldName').value.trim();
            const description = document.getElementById('fieldDescription').value.trim();
            const showInTable = document.getElementById('showInTable').checked;
            
            if (!name) {
                showStatus('Field name is required', true);
                return;
            }
            
            // Find available slot for this field type
            let slotNumber = 1;
            for (const field of fields) {
                if (field.type === type && field.id === `${type}-field-${slotNumber}`) {
                    slotNumber++;
                    if (slotNumber > 3) {
                        showStatus(`Maximum of 3 ${type} fields allowed`, true);
                        return;
                    }
                }
            }
            
            const newField = {
                type: type,
                id: `${type}-field-${slotNumber}`,
                name: name,
                description: description,
                showInTable: showInTable,
                order: fields.length
            };
            
            // Add numeric config if applicable
            if (type === 'numeric') {
                const isInteger = document.getElementById('isInteger').checked;
                const minValue = document.getElementById('minValue').value;
                const maxValue = document.getElementById('maxValue').value;
                
                newField.numericConfig = {
                    isInteger: isInteger,
                    minValue: minValue ? parseFloat(minValue) : null,
                    maxValue: maxValue ? parseFloat(maxValue) : null
                };
            }
            
            // Add to fields array
            fields.push(newField);
            
            // Re-render fields
            renderCurrentFields();
            
            // Clear form
            document.getElementById('fieldName').value = '';
            document.getElementById('fieldDescription').value = '';
            document.getElementById('showInTable').checked = false;
            document.getElementById('isInteger').checked = false;
            document.getElementById('minValue').value = '';
            document.getElementById('maxValue').value = '';
            
            showStatus(`Added new ${type} field: ${name}`);
            document.getElementById('saveCurrentBtn').disabled = false;
        });
        
        // Save current fields
        document.getElementById('saveCurrentBtn').addEventListener('click', () => {
            saveFields(fields);
        });
        
        // Save fields to API
        function saveFields(fieldsToSave) {
            showStatus('Saving fields...');
            
            fetch('/Inventory/SaveCustomFields', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    inventoryId: inventoryId,
                    fields: fieldsToSave
                })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showStatus('Fields saved successfully');
                    // Re-fetch fields to show updated data
                    document.getElementById('fetchBtn').click();
                } else {
                    showStatus('Error saving fields: ' + (data.error || 'Unknown error'), true);
                }
                showResults(data);
            })
            .catch(error => {
                showStatus('Error saving fields: ' + error.message, true);
                console.error('Error:', error);
            });
        }
        
        // Render current fields
        function renderCurrentFields() {
            if (fields.length === 0) {
                currentFieldsEl.innerHTML = '<p>No fields configured</p>';
                return;
            }
            
            let html = '';
            for (const field of fields) {
                html += `
                <div class='field-container'>
                    <h4>${field.name} (${field.type})</h4>
                    <p><strong>ID:</strong> ${field.id}</p>
                    <p><strong>Description:</strong> ${field.description || 'None'}</p>
                    <p><strong>Show in table:</strong> ${field.showInTable ? 'Yes' : 'No'}</p>
                    ${field.numericConfig ? `
                        <p><strong>Numeric config:</strong> 
                            Integer: ${field.numericConfig.isInteger ? 'Yes' : 'No'}, 
                            Min: ${field.numericConfig.minValue !== null ? field.numericConfig.minValue : 'None'}, 
                            Max: ${field.numericConfig.maxValue !== null ? field.numericConfig.maxValue : 'None'}
                        </p>
                    ` : ''}
                    <button class='removeBtn' data-id='${field.id}'>Remove</button>
                </div>
                `;
            }
            currentFieldsEl.innerHTML = html;
            
            // Add event listeners to remove buttons
            document.querySelectorAll('.removeBtn').forEach(btn => {
                btn.addEventListener('click', () => {
                    const id = btn.getAttribute('data-id');
                    fields = fields.filter(f => f.id !== id);
                    renderCurrentFields();
                    showStatus(`Removed field with ID: ${id}`);
                    document.getElementById('saveCurrentBtn').disabled = false;
                });
            });
            
            document.getElementById('saveCurrentBtn').disabled = false;
        }
        
        // Initialize by fetching fields
        document.getElementById('fetchBtn').click();
    </script>
</body>
</html>
", "text/html");
        }

        /// <summary>
        /// Shows the complete DTO that's returned by the service for debugging purposes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DtoDebug(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            // Extract DTO information for debugging
            var dtoInfo = new
            {
                Id = inventory.Id,
                Title = inventory.Title,
                CustomFieldConfigs = new
                {
                    TextField1 = inventory.TextField1,
                    TextField2 = inventory.TextField2,
                    TextField3 = inventory.TextField3,
                    MultiTextField1 = inventory.MultiTextField1,
                    MultiTextField2 = inventory.MultiTextField2,
                    MultiTextField3 = inventory.MultiTextField3,
                    NumericField1 = inventory.NumericField1,
                    NumericField2 = inventory.NumericField2,
                    NumericField3 = inventory.NumericField3,
                    DocumentField1 = inventory.DocumentField1,
                    DocumentField2 = inventory.DocumentField2,
                    DocumentField3 = inventory.DocumentField3,
                    BooleanField1 = inventory.BooleanField1,
                    BooleanField2 = inventory.BooleanField2,
                    BooleanField3 = inventory.BooleanField3
                }
            };

            return Json(new
            {
                dto = dtoInfo,
                message = "This shows the complete DTO structure returned from the service"
            });
        }

        /// <summary>
        /// Directly queries the database to show raw field values for an inventory.
        /// This is useful for debugging database storage issues.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DatabaseDebug(int id)
        {
            try
            {
                // Get direct access to the DbContext through the repository
                var rawInventory = await _inventoryService.GetRawInventoryDataAsync(id);
                
                if (rawInventory == null)
                {
                    return NotFound(new { message = $"Inventory with ID {id} not found in database" });
                }

                // Extract all custom field data for display
                var customFieldData = new
                {
                    // Text Fields
                    TextField1 = new
                    {
                        Name = rawInventory.TextField1Name,
                        Description = rawInventory.TextField1Description,
                        ShowInTable = rawInventory.TextField1ShowInTable
                    },
                    TextField2 = new
                    {
                        Name = rawInventory.TextField2Name,
                        Description = rawInventory.TextField2Description,
                        ShowInTable = rawInventory.TextField2ShowInTable
                    },
                    TextField3 = new
                    {
                        Name = rawInventory.TextField3Name,
                        Description = rawInventory.TextField3Description,
                        ShowInTable = rawInventory.TextField3ShowInTable
                    },
                    
                    // MultiText Fields
                    MultiTextField1 = new
                    {
                        Name = rawInventory.MultiTextField1Name,
                        Description = rawInventory.MultiTextField1Description,
                        ShowInTable = rawInventory.MultiTextField1ShowInTable
                    },
                    MultiTextField2 = new
                    {
                        Name = rawInventory.MultiTextField2Name,
                        Description = rawInventory.MultiTextField2Description,
                        ShowInTable = rawInventory.MultiTextField2ShowInTable
                    },
                    MultiTextField3 = new
                    {
                        Name = rawInventory.MultiTextField3Name,
                        Description = rawInventory.MultiTextField3Description,
                        ShowInTable = rawInventory.MultiTextField3ShowInTable
                    },
                    
                    // Numeric Fields
                    NumericField1 = new
                    {
                        Name = rawInventory.NumericField1Name,
                        Description = rawInventory.NumericField1Description,
                        ShowInTable = rawInventory.NumericField1ShowInTable,
                        IsInteger = rawInventory.NumericField1IsInteger,
                        MinValue = rawInventory.NumericField1MinValue,
                        MaxValue = rawInventory.NumericField1MaxValue
                    },
                    NumericField2 = new
                    {
                        Name = rawInventory.NumericField2Name,
                        Description = rawInventory.NumericField2Description,
                        ShowInTable = rawInventory.NumericField2ShowInTable,
                        IsInteger = rawInventory.NumericField2IsInteger,
                        MinValue = rawInventory.NumericField2MinValue,
                        MaxValue = rawInventory.NumericField2MaxValue
                    },
                    NumericField3 = new
                    {
                        Name = rawInventory.NumericField3Name,
                        Description = rawInventory.NumericField3Description,
                        ShowInTable = rawInventory.NumericField3ShowInTable,
                        IsInteger = rawInventory.NumericField3IsInteger,
                        MinValue = rawInventory.NumericField3MinValue,
                        MaxValue = rawInventory.NumericField3MaxValue
                    },
                    
                    // Document Fields
                    DocumentField1 = new
                    {
                        Name = rawInventory.DocumentField1Name,
                        Description = rawInventory.DocumentField1Description,
                        ShowInTable = rawInventory.DocumentField1ShowInTable
                    },
                    DocumentField2 = new
                    {
                        Name = rawInventory.DocumentField2Name,
                        Description = rawInventory.DocumentField2Description,
                        ShowInTable = rawInventory.DocumentField2ShowInTable
                    },
                    DocumentField3 = new
                    {
                        Name = rawInventory.DocumentField3Name,
                        Description = rawInventory.DocumentField3Description,
                        ShowInTable = rawInventory.DocumentField3ShowInTable
                    },
                    
                    // Boolean Fields
                    BooleanField1 = new
                    {
                        Name = rawInventory.BooleanField1Name,
                        Description = rawInventory.BooleanField1Description,
                        ShowInTable = rawInventory.BooleanField1ShowInTable
                    },
                    BooleanField2 = new
                    {
                        Name = rawInventory.BooleanField2Name,
                        Description = rawInventory.BooleanField2Description,
                        ShowInTable = rawInventory.BooleanField2ShowInTable
                    },
                    BooleanField3 = new
                    {
                        Name = rawInventory.BooleanField3Name,
                        Description = rawInventory.BooleanField3Description,
                        ShowInTable = rawInventory.BooleanField3ShowInTable
                    }
                };

                return Json(new
                {
                    id = rawInventory.Id,
                    title = rawInventory.Title,
                    category = rawInventory.CategoryId,
                    owner = rawInventory.OwnerId,
                    created = rawInventory.CreatedAt,
                    updated = rawInventory.UpdatedAt,
                    customFieldData = customFieldData,
                    message = "This is raw data directly from the database entity"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error querying database",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
