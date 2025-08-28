using AutoMapper;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace InventoryMgmt.BLL.Services
{
    public class InventoryService
    {
        private readonly IInventoryRepo _inventoryRepository;
        private readonly IRepo<Tag> _tagRepository;
        private readonly IRepo<InventoryTag> _inventoryTagRepository;
        private readonly IRepo<InventoryAccess> _inventoryUserAccessRepository;
        private readonly IRepo<User> _userRepository; // Added for GetInventoryAccessUsersAsync
        private readonly IMapper _mapper;

        public InventoryService(
            IInventoryRepo inventoryRepository,
            IRepo<Tag> tagRepository,
            IRepo<InventoryTag> inventoryTagRepository,
            IRepo<InventoryAccess> inventoryUserAccessRepository,
            IRepo<User> userRepository, // Added for GetInventoryAccessUsersAsync
            IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _tagRepository = tagRepository;
            _inventoryTagRepository = inventoryTagRepository;
            _inventoryUserAccessRepository = inventoryUserAccessRepository;
            _userRepository = userRepository; // Added for GetInventoryAccessUsersAsync
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventoryDto>> GetLatestInventoriesAsync(int count)
        {
            var inventories = await _inventoryRepository.GetLatestInventoriesAsync(count);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<IEnumerable<InventoryDto>> GetMostPopularInventoriesAsync(int count)
        {
            var inventories = await _inventoryRepository.GetMostPopularInventoriesAsync(count);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<IEnumerable<InventoryDto>> SearchInventoriesAsync(string searchTerm)
        {
            var inventories = await _inventoryRepository.SearchInventoriesAsync(searchTerm);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<InventoryDto?> GetInventoryByIdAsync(int id)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(id,
                i => i.Owner,
                i => i.Category,
                i => i.InventoryTags,
                i => i.UserAccesses);
            return _mapper.Map<InventoryDto>(inventory);
        }

        public async Task<InventoryDto> CreateInventoryAsync(InventoryDto inventoryDto)
        {
            try
            {
                var inventory = _mapper.Map<Inventory>(inventoryDto);
                inventory.CreatedAt = DateTime.UtcNow;
                inventory.UpdatedAt = DateTime.UtcNow;

                await _inventoryRepository.AddAsync(inventory);
                await _inventoryRepository.SaveChangesAsync();

                return _mapper.Map<InventoryDto>(inventory);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"CreateInventoryAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<InventoryDto?> UpdateInventoryAsync(InventoryDto inventoryDto)
        {
            var existingInventory = await _inventoryRepository.GetByIdAsync(inventoryDto.Id);
            if (existingInventory == null) return null;

            // Check optimistic concurrency
            if (!existingInventory.Version.SequenceEqual(inventoryDto.Version))
            {
                throw new DbUpdateConcurrencyException("The inventory has been modified by another user.");
            }

            _mapper.Map(inventoryDto, existingInventory);
            existingInventory.UpdatedAt = DateTime.UtcNow;

            _inventoryRepository.Update(existingInventory);
            await _inventoryRepository.SaveChangesAsync();

            return _mapper.Map<InventoryDto>(existingInventory);
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(id);
            if (inventory == null) return false;

            _inventoryRepository.Remove(inventory);
            await _inventoryRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InventoryDto>> GetUserOwnedInventoriesAsync(int userId)
        {
            var inventories = await _inventoryRepository.GetUserOwnedInventoriesAsync(userId);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<IEnumerable<InventoryDto>> GetUserAccessibleInventoriesAsync(int userId)
        {
            var inventories = await _inventoryRepository.GetUserAccessibleInventoriesAsync(userId);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<bool> CanUserEditInventoryAsync(int inventoryId, string userId, bool isAdmin = false)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
            if (inventory == null) return false;

            int.TryParse(userId, out int userIdInt);
            if (isAdmin || inventory.OwnerId == userIdInt) return true;

            if (inventory.IsPublic) return true;

            var hasAccess = await _inventoryUserAccessRepository.ExistsAsync(ua =>
                ua.InventoryId == inventoryId && ua.UserId == userIdInt);
            return hasAccess;
        }

        public string GenerateCustomId(string format, int sequenceNumber)
        {
            // Simple implementation - in a real app, this would be more sophisticated
            var result = format;
            var random = new Random();

            // Replace placeholders
            result = result.Replace("{SEQUENCE}", sequenceNumber.ToString("D4"));
            result = result.Replace("{RANDOM}", random.Next(1000, 9999).ToString());
            result = result.Replace("{GUID}", Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper());
            result = result.Replace("{DATE}", DateTime.Now.ToString("yyyyMMdd"));
            result = result.Replace("{TIME}", DateTime.Now.ToString("HHmm"));

            return result;
        }

        public string GenerateAdvancedCustomId(List<CustomIdElement> elements, int sequenceNumber)
        {
            var result = new StringBuilder();
            var random = new Random();

            foreach (var element in elements.OrderBy(e => e.Order))
            {
                switch (element.Type.ToLower())
                {
                    case "fixed":
                        result.Append(element.Value);
                        break;
                    case "20-bit random":
                        var random20Bit = random.Next(0, 1048576); // 2^20
                        result.Append(FormatRandomValue(random20Bit, element.Value));
                        break;
                    case "32-bit random":
                        var random32Bit = random.Next();
                        result.Append(FormatRandomValue(random32Bit, element.Value));
                        break;
                    case "6-digit random":
                        var random6Digit = random.Next(100000, 999999);
                        result.Append(FormatRandomValue(random6Digit, element.Value));
                        break;
                    case "9-digit random":
                        var random9Digit = random.Next(100000000, 999999999);
                        result.Append(FormatRandomValue(random9Digit, element.Value));
                        break;
                    case "guid":
                        var guid = Guid.NewGuid();
                        result.Append(FormatGuid(guid, element.Value));
                        break;
                    case "date/time":
                        result.Append(FormatDateTime(DateTime.Now, element.Value));
                        break;
                    case "sequence":
                        result.Append(FormatSequence(sequenceNumber, element.Value));
                        break;
                }
            }

            return result.ToString();
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

            if (format.StartsWith("X"))
            {
                var digits = format.Substring(1);
                if (int.TryParse(digits, out int digitCount))
                {
                    return value.ToString($"X{digitCount}");
                }
            }
            else if (format.StartsWith("D"))
            {
                var digits = format.Substring(1);
                if (int.TryParse(digits, out int digitCount))
                {
                    return value.ToString($"D{digitCount}");
                }
            }

            return value.ToString();
        }

        private string FormatGuid(Guid guid, string format)
        {
            if (string.IsNullOrEmpty(format)) return guid.ToString("N");

            switch (format.ToLower())
            {
                case "n": return guid.ToString("N");
                case "d": return guid.ToString("D");
                case "b": return guid.ToString("B");
                case "p": return guid.ToString("P");
                default: return guid.ToString("N");
            }
        }

        private string FormatDateTime(DateTime dateTime, string format)
        {
            if (string.IsNullOrEmpty(format)) return dateTime.ToString("yyyy");

            try
            {
                return dateTime.ToString(format);
            }
            catch
            {
                return dateTime.ToString("yyyy");
            }
        }

        private string FormatSequence(int sequence, string format)
        {
            if (string.IsNullOrEmpty(format)) return sequence.ToString("D3");

            if (format.StartsWith("D"))
            {
                var digits = format.Substring(1);
                if (int.TryParse(digits, out int digitCount))
                {
                    return sequence.ToString($"D{digitCount}");
                }
            }

            return sequence.ToString("D3");
        }

        public async Task<bool> UpdateCustomIdConfigurationAsync(int inventoryId, List<CustomIdElement> elements)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
            if (inventory == null) return false;

            // Serialize elements to JSON
            var elementsJson = JsonSerializer.Serialize(elements);
            inventory.CustomIdElements = elementsJson;
            inventory.UpdatedAt = DateTime.UtcNow;

            _inventoryRepository.Update(inventory);
            await _inventoryRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCustomFieldsAsync(int inventoryId, List<CustomFieldData> fields)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateCustomFieldsAsync called for inventory {inventoryId} with {fields?.Count ?? 0} fields");

            try
            {
                // Validate input
                if (inventoryId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Invalid inventory ID: {inventoryId}");
                    return false;
                }

                if (fields == null)
                {
                    fields = new List<CustomFieldData>();
                    System.Diagnostics.Debug.WriteLine("WARNING: fields parameter was null, created empty list");
                }

                // Get the inventory record
                var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
                if (inventory == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Inventory with ID {inventoryId} not found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Found inventory: ID={inventory.Id}, Title={inventory.Title}, OwnerId={inventory.OwnerId}");

                // Debug log current state before modification
                LogCurrentFieldState(inventory);

                // IMPORTANT: First clear all fields not included in the request
                // This ensures deleted fields actually get removed

                // Create a dictionary of fields by ID for quick lookup
                var fieldDictionary = new Dictionary<string, CustomFieldData>();
                foreach (var field in fields)
                {
                    if (!string.IsNullOrEmpty(field.Id))
                    {
                        fieldDictionary[field.Id] = field;
                    }
                }

                // Clear any field that isn't in the request
                // First clear text fields if not in the request
                if (!fieldDictionary.ContainsKey("text-field-1")) ClearTextField1(inventory);
                if (!fieldDictionary.ContainsKey("text-field-2")) ClearTextField2(inventory);
                if (!fieldDictionary.ContainsKey("text-field-3")) ClearTextField3(inventory);

                // Clear numeric fields if not in the request
                if (!fieldDictionary.ContainsKey("numeric-field-1")) ClearNumericField1(inventory);
                if (!fieldDictionary.ContainsKey("numeric-field-2")) ClearNumericField2(inventory);
                if (!fieldDictionary.ContainsKey("numeric-field-3")) ClearNumericField3(inventory);

                // Clear boolean fields if not in the request
                if (!fieldDictionary.ContainsKey("boolean-field-1")) ClearBooleanField1(inventory);
                if (!fieldDictionary.ContainsKey("boolean-field-2")) ClearBooleanField2(inventory);
                if (!fieldDictionary.ContainsKey("boolean-field-3")) ClearBooleanField3(inventory);

                // Clear multitext fields if not in the request
                if (!fieldDictionary.ContainsKey("multitext-field-1")) ClearMultiTextField1(inventory);
                if (!fieldDictionary.ContainsKey("multitext-field-2")) ClearMultiTextField2(inventory);
                if (!fieldDictionary.ContainsKey("multitext-field-3")) ClearMultiTextField3(inventory);

                // Clear document fields if not in the request
                if (!fieldDictionary.ContainsKey("document-field-1")) ClearDocumentField1(inventory);
                if (!fieldDictionary.ContainsKey("document-field-2")) ClearDocumentField2(inventory);
                if (!fieldDictionary.ContainsKey("document-field-3")) ClearDocumentField3(inventory);

                // Now update fields that are in the request
                System.Diagnostics.Debug.WriteLine($"Processing {fields.Count} field configurations");

                // Process text fields
                UpdateFieldIfProvided(fieldDictionary, "text-field-1", inventory, ApplyTextField1);
                UpdateFieldIfProvided(fieldDictionary, "text-field-2", inventory, ApplyTextField2);
                UpdateFieldIfProvided(fieldDictionary, "text-field-3", inventory, ApplyTextField3);

                // Process numeric fields
                UpdateFieldIfProvided(fieldDictionary, "numeric-field-1", inventory, ApplyNumericField1);
                UpdateFieldIfProvided(fieldDictionary, "numeric-field-2", inventory, ApplyNumericField2);
                UpdateFieldIfProvided(fieldDictionary, "numeric-field-3", inventory, ApplyNumericField3);

                // Process boolean fields
                UpdateFieldIfProvided(fieldDictionary, "boolean-field-1", inventory, ApplyBooleanField1);
                UpdateFieldIfProvided(fieldDictionary, "boolean-field-2", inventory, ApplyBooleanField2);
                UpdateFieldIfProvided(fieldDictionary, "boolean-field-3", inventory, ApplyBooleanField3);

                // Process multitext fields
                UpdateFieldIfProvided(fieldDictionary, "multitext-field-1", inventory, ApplyMultiTextField1);
                UpdateFieldIfProvided(fieldDictionary, "multitext-field-2", inventory, ApplyMultiTextField2);
                UpdateFieldIfProvided(fieldDictionary, "multitext-field-3", inventory, ApplyMultiTextField3);

                // Process document fields
                UpdateFieldIfProvided(fieldDictionary, "document-field-1", inventory, ApplyDocumentField1);
                UpdateFieldIfProvided(fieldDictionary, "document-field-2", inventory, ApplyDocumentField2);
                UpdateFieldIfProvided(fieldDictionary, "document-field-3", inventory, ApplyDocumentField3);

                // Log final state after applying changes
                LogCurrentFieldState(inventory);

                inventory.UpdatedAt = DateTime.UtcNow;
                _inventoryRepository.Update(inventory);

                System.Diagnostics.Debug.WriteLine("Saving changes to database");
                await _inventoryRepository.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Changes saved successfully");

                // Verify the changes were saved by re-fetching the inventory
                var savedInventory = await _inventoryRepository.GetByIdAsync(inventoryId);
                if (savedInventory != null)
                {
                    System.Diagnostics.Debug.WriteLine("Verification: Re-fetched inventory after save");
                    LogCurrentFieldState(savedInventory);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpdateCustomFieldsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow to let controller handle it
            }
        }

        private void LogCurrentFieldState(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Current Field State:");

            // Text fields
            System.Diagnostics.Debug.WriteLine($"TextField1: {(string.IsNullOrEmpty(inventory.TextField1Name) ? "Empty" : inventory.TextField1Name)}");
            System.Diagnostics.Debug.WriteLine($"TextField2: {(string.IsNullOrEmpty(inventory.TextField2Name) ? "Empty" : inventory.TextField2Name)}");
            System.Diagnostics.Debug.WriteLine($"TextField3: {(string.IsNullOrEmpty(inventory.TextField3Name) ? "Empty" : inventory.TextField3Name)}");

            // MultiText fields
            System.Diagnostics.Debug.WriteLine($"MultiTextField1: {(string.IsNullOrEmpty(inventory.MultiTextField1Name) ? "Empty" : inventory.MultiTextField1Name)}");
            System.Diagnostics.Debug.WriteLine($"MultiTextField2: {(string.IsNullOrEmpty(inventory.MultiTextField2Name) ? "Empty" : inventory.MultiTextField2Name)}");
            System.Diagnostics.Debug.WriteLine($"MultiTextField3: {(string.IsNullOrEmpty(inventory.MultiTextField3Name) ? "Empty" : inventory.MultiTextField3Name)}");

            // Numeric fields
            System.Diagnostics.Debug.WriteLine($"NumericField1: {(string.IsNullOrEmpty(inventory.NumericField1Name) ? "Empty" : inventory.NumericField1Name)}");
            System.Diagnostics.Debug.WriteLine($"NumericField2: {(string.IsNullOrEmpty(inventory.NumericField2Name) ? "Empty" : inventory.NumericField2Name)}");
            System.Diagnostics.Debug.WriteLine($"NumericField3: {(string.IsNullOrEmpty(inventory.NumericField3Name) ? "Empty" : inventory.NumericField3Name)}");

            // Boolean fields
            System.Diagnostics.Debug.WriteLine($"BooleanField1: {(string.IsNullOrEmpty(inventory.BooleanField1Name) ? "Empty" : inventory.BooleanField1Name)}");
            System.Diagnostics.Debug.WriteLine($"BooleanField2: {(string.IsNullOrEmpty(inventory.BooleanField2Name) ? "Empty" : inventory.BooleanField2Name)}");
            System.Diagnostics.Debug.WriteLine($"BooleanField3: {(string.IsNullOrEmpty(inventory.BooleanField3Name) ? "Empty" : inventory.BooleanField3Name)}");
        }

        /// <summary>
        /// Completely clears all custom field configurations from the inventory.
        /// Use this only when the user explicitly wants to reset all fields.
        /// </summary>
        private void ResetCustomFieldConfigurations(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("!!!! RESETTING ALL CUSTOM FIELDS TO NULL !!!!");

            // Text fields
            inventory.TextField1Name = null;
            inventory.TextField1Description = null;
            inventory.TextField1ShowInTable = false;
            inventory.TextField2Name = null;
            inventory.TextField2Description = null;
            inventory.TextField2ShowInTable = false;
            inventory.TextField3Name = null;
            inventory.TextField3Description = null;
            inventory.TextField3ShowInTable = false;

            // Multi-text fields
            inventory.MultiTextField1Name = null;
            inventory.MultiTextField1Description = null;
            inventory.MultiTextField1ShowInTable = false;
            inventory.MultiTextField2Name = null;
            inventory.MultiTextField2Description = null;
            inventory.MultiTextField2ShowInTable = false;
            inventory.MultiTextField3Name = null;
            inventory.MultiTextField3Description = null;
            inventory.MultiTextField3ShowInTable = false;

            // Numeric fields
            inventory.NumericField1Name = null;
            inventory.NumericField1Description = null;
            inventory.NumericField1ShowInTable = false;
            inventory.NumericField2Name = null;
            inventory.NumericField2Description = null;
            inventory.NumericField2ShowInTable = false;
            inventory.NumericField3Name = null;
            inventory.NumericField3Description = null;
            inventory.NumericField3ShowInTable = false;

            // Document fields
            inventory.DocumentField1Name = null;
            inventory.DocumentField1Description = null;
            inventory.DocumentField1ShowInTable = false;
            inventory.DocumentField2Name = null;
            inventory.DocumentField2Description = null;
            inventory.DocumentField2ShowInTable = false;
            inventory.DocumentField3Name = null;
            inventory.DocumentField3Description = null;
            inventory.DocumentField3ShowInTable = false;

            // Boolean fields
            inventory.BooleanField1Name = null;
            inventory.BooleanField1Description = null;
            inventory.BooleanField1ShowInTable = false;
            inventory.BooleanField2Name = null;
            inventory.BooleanField2Description = null;
            inventory.BooleanField2ShowInTable = false;
            inventory.BooleanField3Name = null;
            inventory.BooleanField3Description = null;
            inventory.BooleanField3ShowInTable = false;
        }

        private void ApplyCustomFieldConfiguration(Inventory inventory, CustomFieldData field)
        {
            if (field == null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Attempted to apply null field");
                return;
            }

            if (string.IsNullOrEmpty(field.Name))
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Attempted to apply field with empty name");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Applying field configuration: Type={field.Type}, Name={field.Name}, Description={field.Description}, ShowInTable={field.ShowInTable}, ID={field.Id}");

            try
            {
                // If the field has a specific ID like "text-field-1", place it in the corresponding slot
                if (!string.IsNullOrEmpty(field.Id) && field.Id.Contains("-field-"))
                {
                    var idParts = field.Id.Split('-');
                    if (idParts.Length >= 3)
                    {
                        var fieldType = idParts[0];  // e.g., "text"
                        var fieldNumber = int.TryParse(idParts[2], out int num) ? num : 0; // e.g., "1" from "text-field-1"

                        System.Diagnostics.Debug.WriteLine($"Field has explicit ID pattern: type={fieldType}, number={fieldNumber}");

                        // Apply the field to the specific slot based on ID
                        ApplyFieldToSpecificSlot(inventory, field, fieldType, fieldNumber);
                        return;
                    }
                }

                // Default application logic if no specific ID pattern
                switch (field.Type.ToLower())
                {
                    case "text":
                        System.Diagnostics.Debug.WriteLine("Processing TEXT field");
                        if (string.IsNullOrEmpty(inventory.TextField1Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using TextField1");
                            inventory.TextField1Name = field.Name;
                            inventory.TextField1Description = field.Description;
                            inventory.TextField1ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.TextField2Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using TextField2");
                            inventory.TextField2Name = field.Name;
                            inventory.TextField2Description = field.Description;
                            inventory.TextField2ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.TextField3Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using TextField3");
                            inventory.TextField3Name = field.Name;
                            inventory.TextField3Description = field.Description;
                            inventory.TextField3ShowInTable = field.ShowInTable;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("WARNING: All text fields are already in use, skipping");
                        }
                        break;

                    case "multitext":
                        System.Diagnostics.Debug.WriteLine("Processing MULTITEXT field");
                        if (string.IsNullOrEmpty(inventory.MultiTextField1Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using MultiTextField1");
                            inventory.MultiTextField1Name = field.Name;
                            inventory.MultiTextField1Description = field.Description;
                            inventory.MultiTextField1ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.MultiTextField2Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using MultiTextField2");
                            inventory.MultiTextField2Name = field.Name;
                            inventory.MultiTextField2Description = field.Description;
                            inventory.MultiTextField2ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.MultiTextField3Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using MultiTextField3");
                            inventory.MultiTextField3Name = field.Name;
                            inventory.MultiTextField3Description = field.Description;
                            inventory.MultiTextField3ShowInTable = field.ShowInTable;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("WARNING: All multitext fields are already in use, skipping");
                        }
                        break;

                    case "numeric":
                        System.Diagnostics.Debug.WriteLine("Processing NUMERIC field");
                        if (string.IsNullOrEmpty(inventory.NumericField1Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using NumericField1");
                            inventory.NumericField1Name = field.Name;
                            inventory.NumericField1Description = field.Description;
                            inventory.NumericField1ShowInTable = field.ShowInTable;

                            // Set numeric-specific properties
                            if (field.NumericConfig != null)
                            {
                                inventory.NumericField1IsInteger = field.NumericConfig.IsInteger;
                                inventory.NumericField1MinValue = field.NumericConfig.MinValue;
                                inventory.NumericField1MaxValue = field.NumericConfig.MaxValue;
                            }
                        }
                        else if (string.IsNullOrEmpty(inventory.NumericField2Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using NumericField2");
                            inventory.NumericField2Name = field.Name;
                            inventory.NumericField2Description = field.Description;
                            inventory.NumericField2ShowInTable = field.ShowInTable;

                            // Set numeric-specific properties
                            if (field.NumericConfig != null)
                            {
                                inventory.NumericField2IsInteger = field.NumericConfig.IsInteger;
                                inventory.NumericField2MinValue = field.NumericConfig.MinValue;
                                inventory.NumericField2MaxValue = field.NumericConfig.MaxValue;
                            }
                        }
                        else if (string.IsNullOrEmpty(inventory.NumericField3Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using NumericField3");
                            inventory.NumericField3Name = field.Name;
                            inventory.NumericField3Description = field.Description;
                            inventory.NumericField3ShowInTable = field.ShowInTable;

                            // Set numeric-specific properties
                            if (field.NumericConfig != null)
                            {
                                inventory.NumericField3IsInteger = field.NumericConfig.IsInteger;
                                inventory.NumericField3MinValue = field.NumericConfig.MinValue;
                                inventory.NumericField3MaxValue = field.NumericConfig.MaxValue;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("WARNING: All numeric fields are already in use, skipping");
                        }
                        break;

                    case "document":
                        System.Diagnostics.Debug.WriteLine("Processing DOCUMENT field");
                        if (string.IsNullOrEmpty(inventory.DocumentField1Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using DocumentField1");
                            inventory.DocumentField1Name = field.Name;
                            inventory.DocumentField1Description = field.Description;
                            inventory.DocumentField1ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.DocumentField2Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using DocumentField2");
                            inventory.DocumentField2Name = field.Name;
                            inventory.DocumentField2Description = field.Description;
                            inventory.DocumentField2ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.DocumentField3Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using DocumentField3");
                            inventory.DocumentField3Name = field.Name;
                            inventory.DocumentField3Description = field.Description;
                            inventory.DocumentField3ShowInTable = field.ShowInTable;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("WARNING: All document fields are already in use, skipping");
                        }
                        break;

                    case "boolean":
                        System.Diagnostics.Debug.WriteLine("Processing BOOLEAN field");
                        if (string.IsNullOrEmpty(inventory.BooleanField1Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using BooleanField1");
                            inventory.BooleanField1Name = field.Name;
                            inventory.BooleanField1Description = field.Description;
                            inventory.BooleanField1ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.BooleanField2Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using BooleanField2");
                            inventory.BooleanField2Name = field.Name;
                            inventory.BooleanField2Description = field.Description;
                            inventory.BooleanField2ShowInTable = field.ShowInTable;
                        }
                        else if (string.IsNullOrEmpty(inventory.BooleanField3Name))
                        {
                            System.Diagnostics.Debug.WriteLine("Using BooleanField3");
                            inventory.BooleanField3Name = field.Name;
                            inventory.BooleanField3Description = field.Description;
                            inventory.BooleanField3ShowInTable = field.ShowInTable;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("WARNING: All boolean fields are already in use, skipping");
                        }
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"WARNING: Unknown field type: {field.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ApplyCustomFieldConfiguration: {ex.Message}");
            }
        }

        public async Task<IEnumerable<UserDto>> GetInventoryAccessUsersAsync(int inventoryId)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
            if (inventory == null) return Enumerable.Empty<UserDto>();

            if (inventory.IsPublic)
            {
                // Return all users for public inventories
                var allUsers = await _userRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<UserDto>>(allUsers);
            }
            else
            {
                // Return only users with explicit access
                var accessUsers = await _inventoryUserAccessRepository.FindAsync(ua => ua.InventoryId == inventoryId);
                var userIds = accessUsers.Select(ua => ua.UserId);
                var users = await _userRepository.FindAsync(u => userIds.Contains(u.Id));
                return _mapper.Map<IEnumerable<UserDto>>(users);
            }
        }
        private void UpdateFieldIfProvided(Dictionary<string, CustomFieldData> fieldDictionary, string fieldId, Inventory inventory, Action<Inventory, CustomFieldData> applyFunc)
        {
            if (fieldDictionary.TryGetValue(fieldId, out var field))
            {
                System.Diagnostics.Debug.WriteLine($"Updating field {fieldId} with value: {field.Name}");
                applyFunc(inventory, field);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No update provided for field {fieldId}, preserving existing value");
            }
        }

        public async Task<bool> ClearAllCustomFieldsAsync(int inventoryId)
        {
            System.Diagnostics.Debug.WriteLine($"ClearAllCustomFieldsAsync called for inventory {inventoryId}");

            try
            {
                // Validate input
                if (inventoryId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Invalid inventory ID: {inventoryId}");
                    return false;
                }

                // Get the inventory record
                var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
                if (inventory == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Inventory with ID {inventoryId} not found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Found inventory: ID={inventory.Id}, Title={inventory.Title}, OwnerId={inventory.OwnerId}");

                // Reset all field configurations
                ResetCustomFieldConfigurations(inventory);

                // Save changes
                inventory.UpdatedAt = DateTime.UtcNow;
                _inventoryRepository.Update(inventory);

                System.Diagnostics.Debug.WriteLine("Saving changes to database after clearing all fields");
                await _inventoryRepository.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Changes saved successfully");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ClearAllCustomFieldsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow to let controller handle it
            }
        }

        private void ApplyTextField1(Inventory inventory, CustomFieldData field)
        {
            inventory.TextField1Name = field.Name;
            inventory.TextField1Description = field.Description;
            inventory.TextField1ShowInTable = field.ShowInTable;
        }

        private void ApplyTextField2(Inventory inventory, CustomFieldData field)
        {
            inventory.TextField2Name = field.Name;
            inventory.TextField2Description = field.Description;
            inventory.TextField2ShowInTable = field.ShowInTable;
        }

        private void ApplyTextField3(Inventory inventory, CustomFieldData field)
        {
            inventory.TextField3Name = field.Name;
            inventory.TextField3Description = field.Description;
            inventory.TextField3ShowInTable = field.ShowInTable;
        }

        private void ApplyNumericField1(Inventory inventory, CustomFieldData field)
        {
            inventory.NumericField1Name = field.Name;
            inventory.NumericField1Description = field.Description;
            inventory.NumericField1ShowInTable = field.ShowInTable;
            if (field.NumericConfig != null)
            {
                inventory.NumericField1IsInteger = field.NumericConfig.IsInteger;
                inventory.NumericField1MinValue = field.NumericConfig.MinValue;
                inventory.NumericField1MaxValue = field.NumericConfig.MaxValue;
            }
        }

        private void ApplyNumericField2(Inventory inventory, CustomFieldData field)
        {
            inventory.NumericField2Name = field.Name;
            inventory.NumericField2Description = field.Description;
            inventory.NumericField2ShowInTable = field.ShowInTable;
            if (field.NumericConfig != null)
            {
                inventory.NumericField2IsInteger = field.NumericConfig.IsInteger;
                inventory.NumericField2MinValue = field.NumericConfig.MinValue;
                inventory.NumericField2MaxValue = field.NumericConfig.MaxValue;
            }
        }

        private void ApplyNumericField3(Inventory inventory, CustomFieldData field)
        {
            inventory.NumericField3Name = field.Name;
            inventory.NumericField3Description = field.Description;
            inventory.NumericField3ShowInTable = field.ShowInTable;
            if (field.NumericConfig != null)
            {
                inventory.NumericField3IsInteger = field.NumericConfig.IsInteger;
                inventory.NumericField3MinValue = field.NumericConfig.MinValue;
                inventory.NumericField3MaxValue = field.NumericConfig.MaxValue;
            }
        }

        private void ApplyBooleanField1(Inventory inventory, CustomFieldData field)
        {
            inventory.BooleanField1Name = field.Name;
            inventory.BooleanField1Description = field.Description;
            inventory.BooleanField1ShowInTable = field.ShowInTable;
        }

        private void ApplyBooleanField2(Inventory inventory, CustomFieldData field)
        {
            inventory.BooleanField2Name = field.Name;
            inventory.BooleanField2Description = field.Description;
            inventory.BooleanField2ShowInTable = field.ShowInTable;
        }

        private void ApplyBooleanField3(Inventory inventory, CustomFieldData field)
        {
            inventory.BooleanField3Name = field.Name;
            inventory.BooleanField3Description = field.Description;
            inventory.BooleanField3ShowInTable = field.ShowInTable;
        }

        private void ApplyMultiTextField1(Inventory inventory, CustomFieldData field)
        {
            inventory.MultiTextField1Name = field.Name;
            inventory.MultiTextField1Description = field.Description;
            inventory.MultiTextField1ShowInTable = field.ShowInTable;
        }

        private void ApplyMultiTextField2(Inventory inventory, CustomFieldData field)
        {
            inventory.MultiTextField2Name = field.Name;
            inventory.MultiTextField2Description = field.Description;
            inventory.MultiTextField2ShowInTable = field.ShowInTable;
        }

        private void ApplyMultiTextField3(Inventory inventory, CustomFieldData field)
        {
            inventory.MultiTextField3Name = field.Name;
            inventory.MultiTextField3Description = field.Description;
            inventory.MultiTextField3ShowInTable = field.ShowInTable;
        }

        private void ApplyDocumentField1(Inventory inventory, CustomFieldData field)
        {
            inventory.DocumentField1Name = field.Name;
            inventory.DocumentField1Description = field.Description;
            inventory.DocumentField1ShowInTable = field.ShowInTable;
        }

        private void ApplyDocumentField2(Inventory inventory, CustomFieldData field)
        {
            inventory.DocumentField2Name = field.Name;
            inventory.DocumentField2Description = field.Description;
            inventory.DocumentField2ShowInTable = field.ShowInTable;
        }

        private void ApplyDocumentField3(Inventory inventory, CustomFieldData field)
        {
            inventory.DocumentField3Name = field.Name;
            inventory.DocumentField3Description = field.Description;
            inventory.DocumentField3ShowInTable = field.ShowInTable;
        }

        private void ApplyFieldToSpecificSlot(Inventory inventory, CustomFieldData field, string fieldType, int fieldNumber)
        {
            System.Diagnostics.Debug.WriteLine($"Applying field to specific slot: type={fieldType}, number={fieldNumber}");

            switch (fieldType.ToLower())
            {
                case "text":
                    switch (fieldNumber)
                    {
                        case 1:
                            System.Diagnostics.Debug.WriteLine("Setting TextField1");
                            inventory.TextField1Name = field.Name;
                            inventory.TextField1Description = field.Description;
                            inventory.TextField1ShowInTable = field.ShowInTable;
                            break;
                        case 2:
                            System.Diagnostics.Debug.WriteLine("Setting TextField2");
                            inventory.TextField2Name = field.Name;
                            inventory.TextField2Description = field.Description;
                            inventory.TextField2ShowInTable = field.ShowInTable;
                            break;
                        case 3:
                            System.Diagnostics.Debug.WriteLine("Setting TextField3");
                            inventory.TextField3Name = field.Name;
                            inventory.TextField3Description = field.Description;
                            inventory.TextField3ShowInTable = field.ShowInTable;
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine($"Invalid text field number: {fieldNumber}");
                            break;
                    }
                    break;

                case "multitext":
                    switch (fieldNumber)
                    {
                        case 1:
                            System.Diagnostics.Debug.WriteLine("Setting MultiTextField1");
                            inventory.MultiTextField1Name = field.Name;
                            inventory.MultiTextField1Description = field.Description;
                            inventory.MultiTextField1ShowInTable = field.ShowInTable;
                            break;
                        case 2:
                            System.Diagnostics.Debug.WriteLine("Setting MultiTextField2");
                            inventory.MultiTextField2Name = field.Name;
                            inventory.MultiTextField2Description = field.Description;
                            inventory.MultiTextField2ShowInTable = field.ShowInTable;
                            break;
                        case 3:
                            System.Diagnostics.Debug.WriteLine("Setting MultiTextField3");
                            inventory.MultiTextField3Name = field.Name;
                            inventory.MultiTextField3Description = field.Description;
                            inventory.MultiTextField3ShowInTable = field.ShowInTable;
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine($"Invalid multitext field number: {fieldNumber}");
                            break;
                    }
                    break;

                case "numeric":
                    switch (fieldNumber)
                    {
                        case 1:
                            System.Diagnostics.Debug.WriteLine("Setting NumericField1");
                            inventory.NumericField1Name = field.Name;
                            inventory.NumericField1Description = field.Description;
                            inventory.NumericField1ShowInTable = field.ShowInTable;

                            if (field.NumericConfig != null)
                            {
                                inventory.NumericField1IsInteger = field.NumericConfig.IsInteger;
                                inventory.NumericField1MinValue = field.NumericConfig.MinValue;
                                inventory.NumericField1MaxValue = field.NumericConfig.MaxValue;
                            }
                            break;
                        case 2:
                            System.Diagnostics.Debug.WriteLine("Setting NumericField2");
                            inventory.NumericField2Name = field.Name;
                            inventory.NumericField2Description = field.Description;
                            inventory.NumericField2ShowInTable = field.ShowInTable;

                            if (field.NumericConfig != null)
                            {
                                inventory.NumericField2IsInteger = field.NumericConfig.IsInteger;
                                inventory.NumericField2MinValue = field.NumericConfig.MinValue;
                                inventory.NumericField2MaxValue = field.NumericConfig.MaxValue;
                            }
                            break;
                        case 3:
                            System.Diagnostics.Debug.WriteLine("Setting NumericField3");
                            inventory.NumericField3Name = field.Name;
                            inventory.NumericField3Description = field.Description;
                            inventory.NumericField3ShowInTable = field.ShowInTable;

                            if (field.NumericConfig != null)
                            {
                                inventory.NumericField3IsInteger = field.NumericConfig.IsInteger;
                                inventory.NumericField3MinValue = field.NumericConfig.MinValue;
                                inventory.NumericField3MaxValue = field.NumericConfig.MaxValue;
                            }
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine($"Invalid numeric field number: {fieldNumber}");
                            break;
                    }
                    break;

                case "document":
                    switch (fieldNumber)
                    {
                        case 1:
                            System.Diagnostics.Debug.WriteLine("Setting DocumentField1");
                            inventory.DocumentField1Name = field.Name;
                            inventory.DocumentField1Description = field.Description;
                            inventory.DocumentField1ShowInTable = field.ShowInTable;
                            break;
                        case 2:
                            System.Diagnostics.Debug.WriteLine("Setting DocumentField2");
                            inventory.DocumentField2Name = field.Name;
                            inventory.DocumentField2Description = field.Description;
                            inventory.DocumentField2ShowInTable = field.ShowInTable;
                            break;
                        case 3:
                            System.Diagnostics.Debug.WriteLine("Setting DocumentField3");
                            inventory.DocumentField3Name = field.Name;
                            inventory.DocumentField3Description = field.Description;
                            inventory.DocumentField3ShowInTable = field.ShowInTable;
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine($"Invalid document field number: {fieldNumber}");
                            break;
                    }
                    break;

                case "boolean":
                    switch (fieldNumber)
                    {
                        case 1:
                            System.Diagnostics.Debug.WriteLine("Setting BooleanField1");
                            inventory.BooleanField1Name = field.Name;
                            inventory.BooleanField1Description = field.Description;
                            inventory.BooleanField1ShowInTable = field.ShowInTable;
                            break;
                        case 2:
                            System.Diagnostics.Debug.WriteLine("Setting BooleanField2");
                            inventory.BooleanField2Name = field.Name;
                            inventory.BooleanField2Description = field.Description;
                            inventory.BooleanField2ShowInTable = field.ShowInTable;
                            break;
                        case 3:
                            System.Diagnostics.Debug.WriteLine("Setting BooleanField3");
                            inventory.BooleanField3Name = field.Name;
                            inventory.BooleanField3Description = field.Description;
                            inventory.BooleanField3ShowInTable = field.ShowInTable;
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine($"Invalid boolean field number: {fieldNumber}");
                            break;
                    }
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown field type in specific slot assignment: {fieldType}");
                    break;
            }
        }

        /// <summary>
        /// Gets raw inventory data directly from the database, bypassing DTOs.
        /// This is useful for debugging database storage issues.
        /// </summary>
        public async Task<Inventory?> GetRawInventoryDataAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching raw inventory data for ID: {id}");

                // Get the inventory entity directly from the database
                var inventory = await _inventoryRepository.GetByIdAsync(id);

                if (inventory == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inventory with ID {id} not found");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Found inventory: {inventory.Title} (ID: {inventory.Id})");
                System.Diagnostics.Debug.WriteLine($"Custom field values from DB:");

                // Log all the field values for debugging
                System.Diagnostics.Debug.WriteLine($"TextField1Name: '{inventory.TextField1Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"TextField2Name: '{inventory.TextField2Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"TextField3Name: '{inventory.TextField3Name ?? "null"}'");

                System.Diagnostics.Debug.WriteLine($"MultiTextField1Name: '{inventory.MultiTextField1Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"MultiTextField2Name: '{inventory.MultiTextField2Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"MultiTextField3Name: '{inventory.MultiTextField3Name ?? "null"}'");

                System.Diagnostics.Debug.WriteLine($"NumericField1Name: '{inventory.NumericField1Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"NumericField2Name: '{inventory.NumericField2Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"NumericField3Name: '{inventory.NumericField3Name ?? "null"}'");

                System.Diagnostics.Debug.WriteLine($"DocumentField1Name: '{inventory.DocumentField1Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"DocumentField2Name: '{inventory.DocumentField2Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"DocumentField3Name: '{inventory.DocumentField3Name ?? "null"}'");

                System.Diagnostics.Debug.WriteLine($"BooleanField1Name: '{inventory.BooleanField1Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"BooleanField2Name: '{inventory.BooleanField2Name ?? "null"}'");
                System.Diagnostics.Debug.WriteLine($"BooleanField3Name: '{inventory.BooleanField3Name ?? "null"}'");

                return inventory;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetRawInventoryDataAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }


        // Clear field methods - used to reset fields not included in the update request
        private void ClearTextField1(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing TextField1");
            inventory.TextField1Name = null;
            inventory.TextField1Description = null;
            inventory.TextField1ShowInTable = false;
        }

        private void ClearTextField2(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing TextField2");
            inventory.TextField2Name = null;
            inventory.TextField2Description = null;
            inventory.TextField2ShowInTable = false;
        }

        private void ClearTextField3(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing TextField3");
            inventory.TextField3Name = null;
            inventory.TextField3Description = null;
            inventory.TextField3ShowInTable = false;
        }

        private void ClearNumericField1(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing NumericField1");
            inventory.NumericField1Name = null;
            inventory.NumericField1Description = null;
            inventory.NumericField1ShowInTable = false;
            inventory.NumericField1IsInteger = false;
            inventory.NumericField1MinValue = null;
            inventory.NumericField1MaxValue = null;
        }

        private void ClearNumericField2(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing NumericField2");
            inventory.NumericField2Name = null;
            inventory.NumericField2Description = null;
            inventory.NumericField2ShowInTable = false;
            inventory.NumericField2IsInteger = false;
            inventory.NumericField2MinValue = null;
            inventory.NumericField2MaxValue = null;
        }

        private void ClearNumericField3(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing NumericField3");
            inventory.NumericField3Name = null;
            inventory.NumericField3Description = null;
            inventory.NumericField3ShowInTable = false;
            inventory.NumericField3IsInteger = false;
            inventory.NumericField3MinValue = null;
            inventory.NumericField3MaxValue = null;
        }

        private void ClearBooleanField1(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing BooleanField1");
            inventory.BooleanField1Name = null;
            inventory.BooleanField1Description = null;
            inventory.BooleanField1ShowInTable = false;
        }

        private void ClearBooleanField2(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing BooleanField2");
            inventory.BooleanField2Name = null;
            inventory.BooleanField2Description = null;
            inventory.BooleanField2ShowInTable = false;
        }

        private void ClearBooleanField3(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing BooleanField3");
            inventory.BooleanField3Name = null;
            inventory.BooleanField3Description = null;
            inventory.BooleanField3ShowInTable = false;
        }

        private void ClearMultiTextField1(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing MultiTextField1");
            inventory.MultiTextField1Name = null;
            inventory.MultiTextField1Description = null;
            inventory.MultiTextField1ShowInTable = false;
        }

        private void ClearMultiTextField2(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing MultiTextField2");
            inventory.MultiTextField2Name = null;
            inventory.MultiTextField2Description = null;
            inventory.MultiTextField2ShowInTable = false;
        }

        private void ClearMultiTextField3(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing MultiTextField3");
            inventory.MultiTextField3Name = null;
            inventory.MultiTextField3Description = null;
            inventory.MultiTextField3ShowInTable = false;
        }

        private void ClearDocumentField1(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing DocumentField1");
            inventory.DocumentField1Name = null;
            inventory.DocumentField1Description = null;
            inventory.DocumentField1ShowInTable = false;
        }

        private void ClearDocumentField2(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing DocumentField2");
            inventory.DocumentField2Name = null;
            inventory.DocumentField2Description = null;
            inventory.DocumentField2ShowInTable = false;
        }

        private void ClearDocumentField3(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Clearing DocumentField3");
            inventory.DocumentField3Name = null;
            inventory.DocumentField3Description = null;
            inventory.DocumentField3ShowInTable = false;
        }
    }
}
