using AutoMapper;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using Microsoft.EntityFrameworkCore;
using InventoryMgmt.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InventoryMgmt.DAL;

namespace InventoryMgmt.BLL.Services
{
    public class ItemService
    {
        private readonly DataAccess _dataAccess;
        private readonly InventoryService _inventoryService;
        private readonly IMapper _mapper;

        public ItemService(
            DataAccess _da,
            InventoryService inventoryService,
            IMapper mapper)
        {
            _dataAccess = _da;
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ItemDto>> GetInventoryItemsAsync(int inventoryId, string? currentUserId = null)
        {
            var items = await _dataAccess.ItemData.FindAsync(
                i => i.InventoryId == inventoryId,
                i => i.CreatedBy,
                i => i.Likes);

            var itemDtos = _mapper.Map<IEnumerable<ItemDto>>(items);

            if (!string.IsNullOrEmpty(currentUserId))
            {
                int.TryParse(currentUserId, out int userIdInt);
                
                // Get all item ids from our result set
                var itemIds = items.Select(i => i.Id).ToList();
                
                // Get likes for these specific items by the current user
                var likedItemIds = await _dataAccess.ItemLikeData.FindAsync(
                    il => il.UserId == userIdInt && itemIds.Contains(il.ItemId));

                var likedIds = likedItemIds.Select(il => il.ItemId).ToHashSet();

                foreach (var item in itemDtos)
                {
                    item.IsLikedByCurrentUser = likedIds.Contains(item.Id);
                }
            }

            return itemDtos;
        }

        public async Task<ItemDto?> GetItemByIdAsync(int id, string? currentUserId = null)
        {
            var item = await _dataAccess.ItemData.GetByIdAsync(id, i => i.CreatedBy, i => i.Inventory, i => i.Likes);
            if (item == null) return null;

            var itemDto = _mapper.Map<ItemDto>(item);

            if (!string.IsNullOrEmpty(currentUserId))
            {
                int.TryParse(currentUserId, out int userIdInt);
                var isLiked = await _dataAccess.ItemLikeData.ExistsAsync(il =>
                    il.ItemId == id && il.UserId == userIdInt);
                itemDto.IsLikedByCurrentUser = isLiked;
            }

            return itemDto;
        }

        public async Task<ItemDto> CreateItemAsync(ItemDto itemDto)
        {
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(itemDto.InventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventory not found");

            // Generate a unique custom ID using the inventory's configuration
            var customId = await _inventoryService.CustomIdService.GenerateUniqueCustomIdAsync(itemDto.InventoryId);

            var item = _mapper.Map<Item>(itemDto);
            item.CustomId = customId;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            item.LikesCount = 0;

            await _dataAccess.ItemData.AddAsync(item);
            await _dataAccess.ItemData.SaveChangesAsync();

            return _mapper.Map<ItemDto>(item);
        }

        public async Task<UpdateResult<ItemDto>> UpdateItemAsync(ItemDto itemDto)
        {
            try
            {
                var existingItem = await _dataAccess.ItemData.GetByIdAsync(itemDto.Id);
                if (existingItem == null) 
                    return UpdateResult<ItemDto>.Error("Item not found.");

                // Check optimistic concurrency
                if (!existingItem.Version.SequenceEqual(itemDto.Version))
                {
                    // Concurrency conflict detected - return the current version of the item
                    var freshItem = await _dataAccess.ItemData.GetByIdAsync(itemDto.Id);
                    var freshItemDto = freshItem != null ? _mapper.Map<ItemDto>(freshItem) : null;
                    return UpdateResult<ItemDto>.ConcurrencyConflict(freshItemDto!);
                }

                // Validate custom ID uniqueness within the inventory if it has changed
                if (existingItem.CustomId != itemDto.CustomId)
                {
                    // Check if the new custom ID is unique within the inventory
                    var isUnique = await _inventoryService.CustomIdService.IsCustomIdUniqueInInventoryAsync(
                        existingItem.InventoryId, itemDto.CustomId, existingItem.Id);
                    
                    if (!isUnique)
                    {
                        throw new InvalidOperationException($"Custom ID '{itemDto.CustomId}' is already in use in this inventory.");
                    }

                    // Get the inventory's current format configuration for validation
                    var inventory = await _dataAccess.InventoryData.GetByIdAsync(existingItem.InventoryId);
                    if (inventory != null && !string.IsNullOrEmpty(inventory.CustomIdElements))
                    {
                        var elements = JsonSerializer.Deserialize<List<CustomIdElement>>(inventory.CustomIdElements);
                        if (elements != null && elements.Any())
                        {
                            var isValidFormat = _inventoryService.CustomIdService.ValidateCustomIdFormat(itemDto.CustomId, elements);
                            if (!isValidFormat)
                            {
                                var errorMessage = _inventoryService.CustomIdService.GetCustomIdValidationErrorMessage(itemDto.CustomId, elements);
                                throw new InvalidOperationException($"Custom ID format is invalid: {errorMessage}");
                            }
                        }
                    }
                }

                // Manual property updates to avoid navigation property conflicts
                existingItem.CustomId = itemDto.CustomId;
                existingItem.TextField1Value = itemDto.Name;
                existingItem.TextField2Value = itemDto.TextField2Value;
                existingItem.TextField3Value = itemDto.TextField3Value;
                existingItem.MultiTextField1Value = itemDto.Description;
                existingItem.MultiTextField2Value = itemDto.MultiTextField2Value;
                existingItem.MultiTextField3Value = itemDto.MultiTextField3Value;
                existingItem.NumericField1Value = itemDto.NumericField1Value;
                existingItem.NumericField2Value = itemDto.NumericField2Value;
                existingItem.NumericField3Value = itemDto.NumericField3Value;
                existingItem.DocumentField1Value = itemDto.DocumentField1Value;
                existingItem.DocumentField2Value = itemDto.DocumentField2Value;
                existingItem.DocumentField3Value = itemDto.DocumentField3Value;
                existingItem.BooleanField1Value = itemDto.BooleanField1Value;
                existingItem.BooleanField2Value = itemDto.BooleanField2Value;
                existingItem.BooleanField3Value = itemDto.BooleanField3Value;
                existingItem.UpdatedAt = DateTime.UtcNow;

                _dataAccess.ItemData.Update(existingItem);
                await _dataAccess.ItemData.SaveChangesAsync();

                // Return successful update
                var updatedItemDto = _mapper.Map<ItemDto>(existingItem);
                return UpdateResult<ItemDto>.Success(updatedItemDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflicts gracefully by getting the latest version
                var freshItem = await _dataAccess.ItemData.GetByIdAsync(itemDto.Id);
                var freshItemDto = freshItem != null ? _mapper.Map<ItemDto>(freshItem) : null;
                return UpdateResult<ItemDto>.ConcurrencyConflict(freshItemDto!);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Item_Inventory_CustomId") == true)
            {
                // Handle unique constraint violation at the database level
                throw new InvalidOperationException($"Custom ID '{itemDto.CustomId}' is already in use in this inventory.");
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _dataAccess.ItemData.GetByIdAsync(id);
            if (item == null) return false;

            _dataAccess.ItemData.Remove(item);
            await _dataAccess.ItemData.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleLikeAsync(int itemId, string userId)
        {
            int.TryParse(userId, out int userIdInt);
            var existingLike = await _dataAccess.ItemLikeData.FindFirstAsync(il =>
                il.ItemId == itemId && il.UserId == userIdInt);

            if (existingLike != null)
            {
                _dataAccess.ItemLikeData.Remove(existingLike);

                var item = await _dataAccess.ItemData.GetByIdAsync(itemId);
                if (item != null)
                {
                    item.LikesCount = Math.Max(0, item.LikesCount - 1);
                    _dataAccess.ItemData.Update(item);
                }
            }
            else
            {
                var newLike = new ItemLike
                {
                    ItemId = itemId,
                    UserId = userIdInt,
                    CreatedAt = DateTime.UtcNow
                };

                await _dataAccess.ItemLikeData.AddAsync(newLike);

                var item = await _dataAccess.ItemData.GetByIdAsync(itemId);
                if (item != null)
                {
                    item.LikesCount++;
                    _dataAccess.ItemData.Update(item);
                }
            }

            await _dataAccess.ItemData.SaveChangesAsync();
            return existingLike == null; // Return true if like was added, false if removed
        }

        /// <summary>
        /// Validates a custom ID for an item within its inventory context
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="customId">The custom ID to validate</param>
        /// <param name="excludeItemId">Item ID to exclude from uniqueness check (for updates)</param>
        /// <returns>Validation result with success status and error message</returns>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateCustomIdAsync(int inventoryId, string customId, int? excludeItemId = null)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(customId))
                return (false, "Custom ID cannot be empty.");

            if (customId.Length > 100)
                return (false, "Custom ID cannot be longer than 100 characters.");

            // Check uniqueness within inventory
            var isUnique = await _inventoryService.CustomIdService.IsCustomIdUniqueInInventoryAsync(
                inventoryId, customId, excludeItemId);
            
            if (!isUnique)
                return (false, $"Custom ID '{customId}' is already in use in this inventory.");

            // Get inventory format configuration for validation
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
            if (inventory != null && !string.IsNullOrEmpty(inventory.CustomIdElements))
            {
                var elements = JsonSerializer.Deserialize<List<CustomIdElement>>(inventory.CustomIdElements);
                if (elements != null && elements.Any())
                {
                    var isValidFormat = _inventoryService.CustomIdService.ValidateCustomIdFormat(customId, elements);
                    if (!isValidFormat)
                    {
                        var errorMessage = _inventoryService.CustomIdService.GetCustomIdValidationErrorMessage(customId, elements);
                        var exampleId = await _inventoryService.CustomIdService.GenerateValidCustomIdExampleAsync(inventoryId);
                        return (false, $"{errorMessage} Expected format example: {exampleId}");
                    }
                }
            }

            return (true, string.Empty);
        }
    }
}
