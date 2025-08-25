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
            var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
            if (inventory == null) return false;

            // Reset all field configurations
            ResetCustomFieldConfigurations(inventory);

            // Apply new field configurations
            foreach (var field in fields.OrderBy(f => f.Order))
            {
                ApplyCustomFieldConfiguration(inventory, field);
            }

            inventory.UpdatedAt = DateTime.UtcNow;
            _inventoryRepository.Update(inventory);
            await _inventoryRepository.SaveChangesAsync();
            return true;
        }

        private void ResetCustomFieldConfigurations(Inventory inventory)
        {
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
            switch (field.Type.ToLower())
            {
                case "text":
                    if (string.IsNullOrEmpty(inventory.TextField1Name))
                    {
                        inventory.TextField1Name = field.Name;
                        inventory.TextField1Description = field.Description;
                        inventory.TextField1ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.TextField2Name))
                    {
                        inventory.TextField2Name = field.Name;
                        inventory.TextField2Description = field.Description;
                        inventory.TextField2ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.TextField3Name))
                    {
                        inventory.TextField3Name = field.Name;
                        inventory.TextField3Description = field.Description;
                        inventory.TextField3ShowInTable = field.ShowInTable;
                    }
                    break;

                case "multitext":
                    if (string.IsNullOrEmpty(inventory.MultiTextField1Name))
                    {
                        inventory.MultiTextField1Name = field.Name;
                        inventory.MultiTextField1Description = field.Description;
                        inventory.MultiTextField1ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.MultiTextField2Name))
                    {
                        inventory.MultiTextField2Name = field.Name;
                        inventory.MultiTextField2Description = field.Description;
                        inventory.MultiTextField2ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.MultiTextField3Name))
                    {
                        inventory.MultiTextField3Name = field.Name;
                        inventory.MultiTextField3Description = field.Description;
                        inventory.MultiTextField3ShowInTable = field.ShowInTable;
                    }
                    break;

                case "numeric":
                    if (string.IsNullOrEmpty(inventory.NumericField1Name))
                    {
                        inventory.NumericField1Name = field.Name;
                        inventory.NumericField1Description = field.Description;
                        inventory.NumericField1ShowInTable = field.ShowInTable;
                        
                        if (field.NumericConfig != null)
                        {
                            inventory.NumericField1IsInteger = field.NumericConfig.IsInteger;
                            inventory.NumericField1MinValue = field.NumericConfig.MinValue;
                            inventory.NumericField1MaxValue = field.NumericConfig.MaxValue;
                            inventory.NumericField1StepValue = field.NumericConfig.StepValue;
                            inventory.NumericField1DisplayFormat = field.NumericConfig.DisplayFormat;
                        }
                    }
                    else if (string.IsNullOrEmpty(inventory.NumericField2Name))
                    {
                        inventory.NumericField2Name = field.Name;
                        inventory.NumericField2Description = field.Description;
                        inventory.NumericField2ShowInTable = field.ShowInTable;
                        
                        if (field.NumericConfig != null)
                        {
                            inventory.NumericField2IsInteger = field.NumericConfig.IsInteger;
                            inventory.NumericField2MinValue = field.NumericConfig.MinValue;
                            inventory.NumericField2MaxValue = field.NumericConfig.MaxValue;
                            inventory.NumericField2StepValue = field.NumericConfig.StepValue;
                            inventory.NumericField2DisplayFormat = field.NumericConfig.DisplayFormat;
                        }
                    }
                    else if (string.IsNullOrEmpty(inventory.NumericField3Name))
                    {
                        inventory.NumericField3Name = field.Name;
                        inventory.NumericField3Description = field.Description;
                        inventory.NumericField3ShowInTable = field.ShowInTable;
                        
                        if (field.NumericConfig != null)
                        {
                            inventory.NumericField3IsInteger = field.NumericConfig.IsInteger;
                            inventory.NumericField3MinValue = field.NumericConfig.MinValue;
                            inventory.NumericField3MaxValue = field.NumericConfig.MaxValue;
                            inventory.NumericField3StepValue = field.NumericConfig.StepValue;
                            inventory.NumericField3DisplayFormat = field.NumericConfig.DisplayFormat;
                        }
                    }
                    break;

                case "document":
                    if (string.IsNullOrEmpty(inventory.DocumentField1Name))
                    {
                        inventory.DocumentField1Name = field.Name;
                        inventory.DocumentField1Description = field.Description;
                        inventory.DocumentField1ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.DocumentField2Name))
                    {
                        inventory.DocumentField2Name = field.Name;
                        inventory.DocumentField2Description = field.Description;
                        inventory.DocumentField2ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.DocumentField3Name))
                    {
                        inventory.DocumentField3Name = field.Name;
                        inventory.DocumentField3Description = field.Description;
                        inventory.DocumentField3ShowInTable = field.ShowInTable;
                    }
                    break;

                case "boolean":
                    if (string.IsNullOrEmpty(inventory.BooleanField1Name))
                    {
                        inventory.BooleanField1Name = field.Name;
                        inventory.BooleanField1Description = field.Description;
                        inventory.BooleanField1ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.BooleanField2Name))
                    {
                        inventory.BooleanField2Name = field.Name;
                        inventory.BooleanField2Description = field.Description;
                        inventory.BooleanField2ShowInTable = field.ShowInTable;
                    }
                    else if (string.IsNullOrEmpty(inventory.BooleanField3Name))
                    {
                        inventory.BooleanField3Name = field.Name;
                        inventory.BooleanField3Description = field.Description;
                        inventory.BooleanField3ShowInTable = field.ShowInTable;
                    }
                    break;
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
    }
}
