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

            // Find the highest ID number used for this inventory to use as the next sequence
            var items = await _dataAccess.ItemData.FindAsync(
                i => i.InventoryId == itemDto.InventoryId);
            
            // Get the next available sequence number for the custom ID
            int nextSequence = 1;
            if (items.Any())
            {
                // Try to generate a unique ID by using the highest ID + 1
                nextSequence = items.Max(i => i.Id) + 1;
            }
            
            // Generate a unique custom ID
            string customId;
            bool isUnique = false;
            int attempts = 0;
            const int maxAttempts = 10;
            
            do
            {
                customId = string.IsNullOrEmpty(inventory.CustomIdElements)
                    ? _inventoryService.CustomIdService.GenerateCustomId(inventory.CustomIdFormat ?? "{SEQUENCE}", nextSequence + attempts)
                    : _inventoryService.CustomIdService.GenerateAdvancedCustomId(
                        JsonSerializer.Deserialize<List<CustomIdElement>>(inventory.CustomIdElements) ?? new List<CustomIdElement>(),
                        nextSequence + attempts);
                
                // Check if this custom ID is already in use in this inventory
                var exists = await _dataAccess.ItemData.ExistsAsync(i => 
                    i.InventoryId == itemDto.InventoryId && i.CustomId == customId);
                
                isUnique = !exists;
                attempts++;
                
                // If we've tried too many times, add a timestamp to ensure uniqueness
                if (attempts >= maxAttempts && !isUnique)
                {
                    customId = $"{customId}_{DateTime.Now.Ticks}";
                    isUnique = true;
                }
            } while (!isUnique);

            var item = _mapper.Map<Item>(itemDto);
            item.CustomId = customId;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            item.LikesCount = 0;

            await _dataAccess.ItemData.AddAsync(item);
            await _dataAccess.ItemData.SaveChangesAsync();

            return _mapper.Map<ItemDto>(item);
        }

        public async Task<ItemDto?> UpdateItemAsync(ItemDto itemDto)
        {
            try
            {
                var existingItem = await _dataAccess.ItemData.GetByIdAsync(itemDto.Id);
                if (existingItem == null) return null;

                // Check optimistic concurrency
                if (!existingItem.Version.SequenceEqual(itemDto.Version))
                {
                    // Instead of returning null, return the current version of the item
                    // This way, the controller can use this data to refresh the form
                    var freshItem = await _dataAccess.ItemData.GetByIdAsync(itemDto.Id);
                    return freshItem != null ? _mapper.Map<ItemDto>(freshItem) : null;
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

                return _mapper.Map<ItemDto>(existingItem);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflicts gracefully by getting the latest version
                var freshItem = await _dataAccess.ItemData.GetByIdAsync(itemDto.Id);
                return freshItem != null ? _mapper.Map<ItemDto>(freshItem) : null;
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
    }
}
