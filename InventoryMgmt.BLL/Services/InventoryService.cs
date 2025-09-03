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
        
        // Expose services as per the new interface requirements
        public ITagService TagService => _tagService;
        public IInventoryAccessService AccessService => _accessService;
        public ICustomFieldService CustomFieldService => _customFieldService;
        public ICustomIdService CustomIdService => _customIdService;

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

        // Access control methods have been removed and are now accessed through the AccessService property
        
        // Custom field methods have been removed and are now accessed through the CustomFieldService property
        
        // Custom ID methods have been removed and are now accessed through the CustomIdService property

        // Access control methods have been removed and are now accessed through the AccessService property

        // Tag methods have been removed and are now accessed through the TagService property
    }
}
