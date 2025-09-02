using AutoMapper;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using InventoryMgmt.DAL;

namespace InventoryMgmt.BLL.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly DataAccess _dataAccess;
        private readonly IMapper _mapper;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICustomIdService _customIdService;
        private readonly ITagService _tagService;
        private readonly IInventoryAccessService _accessService;

        public InventoryService(
            DataAccess dataAccess,
            IMapper mapper,
            ICustomFieldService customFieldService,
            ICustomIdService customIdService,
            ITagService tagService,
            IInventoryAccessService accessService)
        {
            _dataAccess = dataAccess;
            _mapper = mapper;
            _customFieldService = customFieldService;
            _customIdService = customIdService;
            _tagService = tagService;
            _accessService = accessService;
        }

        public async Task<IEnumerable<InventoryDto>> GetLatestInventoriesAsync(int count)
        {
            var inventories = await _dataAccess.InventoryData.GetLatestInventoriesAsync(count);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<IEnumerable<InventoryDto>> GetMostPopularInventoriesAsync(int count)
        {
            var inventories = await _dataAccess.InventoryData.GetMostPopularInventoriesAsync(count);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<IEnumerable<InventoryDto>> SearchInventoriesAsync(string searchTerm)
        {
            var inventories = await _dataAccess.InventoryData.SearchInventoriesAsync(searchTerm);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<InventoryDto?> GetInventoryByIdAsync(int id)
        {
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(id,
                i => i.Owner,
                i => i.Category,
                i => i.InventoryTags.Select(it => it.Tag), // Include the Tag entity
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

                await _dataAccess.InventoryData.AddAsync(inventory);
                await _dataAccess.InventoryData.SaveChangesAsync();

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
            var existingInventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryDto.Id);
            if (existingInventory == null) return null;

            try
            {
                // Update only the properties we want to change, not the navigation properties
                existingInventory.Title = inventoryDto.Title;
                existingInventory.Description = inventoryDto.Description;
                existingInventory.CategoryId = inventoryDto.CategoryId;
                existingInventory.ImageUrl = inventoryDto.ImageUrl;
                existingInventory.IsPublic = inventoryDto.IsPublic;
                existingInventory.CustomIdFormat = inventoryDto.CustomIdFormat;
                existingInventory.CustomIdElements = inventoryDto.CustomIdElements;
                existingInventory.UpdatedAt = DateTime.UtcNow;

                // Update custom field configurations
                UpdateCustomFieldsFromDto(existingInventory, inventoryDto);

                _dataAccess.InventoryData.Update(existingInventory);
                await _dataAccess.InventoryData.SaveChangesAsync();

                return _mapper.Map<InventoryDto>(existingInventory);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency exception gracefully
                throw new InvalidOperationException("The inventory has been modified by another user. Please refresh the page and try again.");
            }
        }

        private void UpdateCustomFieldsFromDto(Inventory inventory, InventoryDto dto)
        {
            // Text fields
            inventory.TextField1Name = dto.TextField1.Name;
            inventory.TextField1Description = dto.TextField1.Description;
            inventory.TextField1ShowInTable = dto.TextField1.ShowInTable;
            
            inventory.TextField2Name = dto.TextField2.Name;
            inventory.TextField2Description = dto.TextField2.Description;
            inventory.TextField2ShowInTable = dto.TextField2.ShowInTable;
            
            inventory.TextField3Name = dto.TextField3.Name;
            inventory.TextField3Description = dto.TextField3.Description;
            inventory.TextField3ShowInTable = dto.TextField3.ShowInTable;

            // Multi-text fields
            inventory.MultiTextField1Name = dto.MultiTextField1.Name;
            inventory.MultiTextField1Description = dto.MultiTextField1.Description;
            inventory.MultiTextField1ShowInTable = dto.MultiTextField1.ShowInTable;
            
            inventory.MultiTextField2Name = dto.MultiTextField2.Name;
            inventory.MultiTextField2Description = dto.MultiTextField2.Description;
            inventory.MultiTextField2ShowInTable = dto.MultiTextField2.ShowInTable;
            
            inventory.MultiTextField3Name = dto.MultiTextField3.Name;
            inventory.MultiTextField3Description = dto.MultiTextField3.Description;
            inventory.MultiTextField3ShowInTable = dto.MultiTextField3.ShowInTable;

            // Numeric fields
            inventory.NumericField1Name = dto.NumericField1.Name;
            inventory.NumericField1Description = dto.NumericField1.Description;
            inventory.NumericField1ShowInTable = dto.NumericField1.ShowInTable;
            if (dto.NumericField1.NumericConfig != null)
            {
                inventory.NumericField1IsInteger = dto.NumericField1.NumericConfig.IsInteger;
                inventory.NumericField1MinValue = dto.NumericField1.NumericConfig.MinValue;
                inventory.NumericField1MaxValue = dto.NumericField1.NumericConfig.MaxValue;
                inventory.NumericField1StepValue = dto.NumericField1.NumericConfig.StepValue;
                inventory.NumericField1DisplayFormat = dto.NumericField1.NumericConfig.DisplayFormat;
            }
            
            inventory.NumericField2Name = dto.NumericField2.Name;
            inventory.NumericField2Description = dto.NumericField2.Description;
            inventory.NumericField2ShowInTable = dto.NumericField2.ShowInTable;
            if (dto.NumericField2.NumericConfig != null)
            {
                inventory.NumericField2IsInteger = dto.NumericField2.NumericConfig.IsInteger;
                inventory.NumericField2MinValue = dto.NumericField2.NumericConfig.MinValue;
                inventory.NumericField2MaxValue = dto.NumericField2.NumericConfig.MaxValue;
                inventory.NumericField2StepValue = dto.NumericField2.NumericConfig.StepValue;
                inventory.NumericField2DisplayFormat = dto.NumericField2.NumericConfig.DisplayFormat;
            }
            
            inventory.NumericField3Name = dto.NumericField3.Name;
            inventory.NumericField3Description = dto.NumericField3.Description;
            inventory.NumericField3ShowInTable = dto.NumericField3.ShowInTable;
            if (dto.NumericField3.NumericConfig != null)
            {
                inventory.NumericField3IsInteger = dto.NumericField3.NumericConfig.IsInteger;
                inventory.NumericField3MinValue = dto.NumericField3.NumericConfig.MinValue;
                inventory.NumericField3MaxValue = dto.NumericField3.NumericConfig.MaxValue;
                inventory.NumericField3StepValue = dto.NumericField3.NumericConfig.StepValue;
                inventory.NumericField3DisplayFormat = dto.NumericField3.NumericConfig.DisplayFormat;
            }

            // Document fields
            inventory.DocumentField1Name = dto.DocumentField1.Name;
            inventory.DocumentField1Description = dto.DocumentField1.Description;
            inventory.DocumentField1ShowInTable = dto.DocumentField1.ShowInTable;
            
            inventory.DocumentField2Name = dto.DocumentField2.Name;
            inventory.DocumentField2Description = dto.DocumentField2.Description;
            inventory.DocumentField2ShowInTable = dto.DocumentField2.ShowInTable;
            
            inventory.DocumentField3Name = dto.DocumentField3.Name;
            inventory.DocumentField3Description = dto.DocumentField3.Description;
            inventory.DocumentField3ShowInTable = dto.DocumentField3.ShowInTable;

            // Boolean fields
            inventory.BooleanField1Name = dto.BooleanField1.Name;
            inventory.BooleanField1Description = dto.BooleanField1.Description;
            inventory.BooleanField1ShowInTable = dto.BooleanField1.ShowInTable;
            
            inventory.BooleanField2Name = dto.BooleanField2.Name;
            inventory.BooleanField2Description = dto.BooleanField2.Description;
            inventory.BooleanField2ShowInTable = dto.BooleanField2.ShowInTable;
            
            inventory.BooleanField3Name = dto.BooleanField3.Name;
            inventory.BooleanField3Description = dto.BooleanField3.Description;
            inventory.BooleanField3ShowInTable = dto.BooleanField3.ShowInTable;
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(id);
            if (inventory == null) return false;

            _dataAccess.InventoryData.Remove(inventory);
            await _dataAccess.InventoryData.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InventoryDto>> GetUserOwnedInventoriesAsync(int userId)
        {
            var inventories = await _dataAccess.InventoryData.GetUserOwnedInventoriesAsync(userId);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<IEnumerable<InventoryDto>> GetUserAccessibleInventoriesAsync(int userId)
        {
            var inventories = await _dataAccess.InventoryData.GetUserAccessibleInventoriesAsync(userId);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }

        public async Task<bool> CanUserEditInventoryAsync(int inventoryId, string userId, bool isAdmin = false)
        {
            return await _accessService.CanUserEditInventoryAsync(inventoryId, userId, isAdmin);
        }

        public string GenerateCustomId(string format, int sequenceNumber)
        {
            return _customIdService.GenerateCustomId(format, sequenceNumber);
        }

        public string GenerateAdvancedCustomId(List<CustomIdElement> elements, int sequenceNumber)
        {
            return _customIdService.GenerateAdvancedCustomId(elements, sequenceNumber);
        }

        public async Task<bool> UpdateCustomIdConfigurationAsync(int inventoryId, List<CustomIdElement> elements)
        {
            return await _customIdService.UpdateCustomIdConfigurationAsync(inventoryId, elements);
        }

        public async Task<bool> UpdateCustomFieldsAsync(int inventoryId, List<CustomFieldData> fields)
        {
            return await _customFieldService.UpdateCustomFieldsAsync(inventoryId, fields);
        }

        public async Task<bool> ClearAllCustomFieldsAsync(int inventoryId)
        {
            return await _customFieldService.ClearAllCustomFieldsAsync(inventoryId);
        }

        /// <summary>
        /// Gets raw inventory data directly from the database, bypassing DTOs.
        /// This is useful for debugging database storage issues.
        /// </summary>
        public async Task<Inventory?> GetRawInventoryDataAsync(int id)
        {
            return await _customFieldService.GetRawInventoryDataAsync(id);
        }

        public async Task<IEnumerable<UserDto>> GetInventoryAccessUsersAsync(int inventoryId)
        {
            return await _accessService.GetInventoryAccessUsersAsync(inventoryId);
        }

        public async Task GrantUserAccessAsync(int inventoryId, int userId, InventoryPermission permission = InventoryPermission.Write)
        {
            await _accessService.GrantUserAccessAsync(inventoryId, userId, permission);
        }

        public async Task UpdateUserAccessPermissionAsync(int inventoryId, int userId, InventoryPermission permission)
        {
            await _accessService.UpdateUserAccessPermissionAsync(inventoryId, userId, permission);
        }

        public async Task<InventoryPermission> GetUserAccessPermissionAsync(int inventoryId, int userId)
        {
            return await _accessService.GetUserAccessPermissionAsync(inventoryId, userId);
        }

        public async Task RevokeUserAccessAsync(int inventoryId, int userId)
        {
            await _accessService.RevokeUserAccessAsync(inventoryId, userId);
        }

        #region Tag Methods

        /// <summary>
        /// Gets all tags associated with an inventory
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <returns>Collection of tag DTOs</returns>
        public async Task<IEnumerable<TagDto>> GetInventoryTagsAsync(int inventoryId)
        {
            return await _tagService.GetInventoryTagsAsync(inventoryId);
        }

        /// <summary>
        /// Searches for tags that match the provided search term
        /// </summary>
        /// <param name="searchTerm">The search term to filter tags</param>
        /// <returns>Collection of tag DTOs that match the search term</returns>
        public async Task<IEnumerable<TagDto>> SearchTagsAsync(string searchTerm)
        {
            return await _tagService.SearchTagsAsync(searchTerm);
        }

        /// <summary>
        /// Adds a tag to an inventory. If the tag doesn't exist, it will be created.
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="tagName">The tag name to add</param>
        /// <returns>True if the operation was successful</returns>
        public async Task<bool> AddTagToInventoryAsync(int inventoryId, string tagName)
        {
            return await _tagService.AddTagToInventoryAsync(inventoryId, tagName);
        }

        /// <summary>
        /// Adds multiple tags to an inventory. If any tag doesn't exist, it will be created.
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="tagNames">List of tag names to add</param>
        /// <returns>True if all tags were added successfully</returns>
        public async Task<bool> AddTagsToInventoryAsync(int inventoryId, List<string> tagNames)
        {
            return await _tagService.AddTagsToInventoryAsync(inventoryId, tagNames);
        }

        /// <summary>
        /// Removes a tag from an inventory
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="tagId">The tag ID to remove</param>
        /// <returns>True if the operation was successful</returns>
        public async Task<bool> RemoveTagFromInventoryAsync(int inventoryId, int tagId)
        {
            return await _tagService.RemoveTagFromInventoryAsync(inventoryId, tagId);
        }

        /// <summary>
        /// Gets the most popular tags
        /// </summary>
        /// <param name="count">The number of tags to return</param>
        /// <returns>Collection of the most used tags</returns>
        public async Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10)
        {
            return await _tagService.GetPopularTagsAsync(count);
        }

        #endregion
    }
}
