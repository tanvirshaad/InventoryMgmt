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

namespace InventoryMgmt.BLL.Services
{
    public class ItemService
    {
        private readonly IRepo<Item> _itemRepository;
        private readonly IRepo<ItemLike> _itemLikeRepository;
        private readonly IInventoryRepo _inventoryRepository;
        private readonly InventoryService _inventoryService;
        private readonly IMapper _mapper;

        public ItemService(
            IRepo<Item> itemRepository,
            IRepo<ItemLike> itemLikeRepository,
            IInventoryRepo inventoryRepository,
            InventoryService inventoryService,
            IMapper mapper)
        {
            _itemRepository = itemRepository;
            _itemLikeRepository = itemLikeRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ItemDto>> GetInventoryItemsAsync(int inventoryId, string? currentUserId = null)
        {
            var items = await _itemRepository.FindAsync(
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
                var likedItemIds = await _itemLikeRepository.FindAsync(
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
            var item = await _itemRepository.GetByIdAsync(id, i => i.CreatedBy, i => i.Inventory, i => i.Likes);
            if (item == null) return null;

            var itemDto = _mapper.Map<ItemDto>(item);

            if (!string.IsNullOrEmpty(currentUserId))
            {
                int.TryParse(currentUserId, out int userIdInt);
                var isLiked = await _itemLikeRepository.ExistsAsync(il =>
                    il.ItemId == id && il.UserId == userIdInt);
                itemDto.IsLikedByCurrentUser = isLiked;
            }

            return itemDto;
        }

        public async Task<ItemDto> CreateItemAsync(ItemDto itemDto)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(itemDto.InventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventory not found");

            // Find the highest ID number used for this inventory to use as the next sequence
            var items = await _itemRepository.FindAsync(
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
                    ? _inventoryService.GenerateCustomId(inventory.CustomIdFormat ?? "{SEQUENCE}", nextSequence + attempts)
                    : _inventoryService.GenerateAdvancedCustomId(
                        JsonSerializer.Deserialize<List<CustomIdElement>>(inventory.CustomIdElements) ?? new List<CustomIdElement>(),
                        nextSequence + attempts);
                
                // Check if this custom ID is already in use in this inventory
                var exists = await _itemRepository.ExistsAsync(i => 
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

            await _itemRepository.AddAsync(item);
            await _itemRepository.SaveChangesAsync();

            return _mapper.Map<ItemDto>(item);
        }

        public async Task<ItemDto?> UpdateItemAsync(ItemDto itemDto)
        {
            var existingItem = await _itemRepository.GetByIdAsync(itemDto.Id);
            if (existingItem == null) return null;

            // Check optimistic concurrency
            if (!existingItem.Version.SequenceEqual(itemDto.Version))
            {
                throw new DbUpdateConcurrencyException("The item has been modified by another user.");
            }

            _mapper.Map(itemDto, existingItem);
            existingItem.UpdatedAt = DateTime.UtcNow;

            _itemRepository.Update(existingItem);
            await _itemRepository.SaveChangesAsync();

            return _mapper.Map<ItemDto>(existingItem);
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null) return false;

            _itemRepository.Remove(item);
            await _itemRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleLikeAsync(int itemId, string userId)
        {
            int.TryParse(userId, out int userIdInt);
            var existingLike = await _itemLikeRepository.FindFirstAsync(il =>
                il.ItemId == itemId && il.UserId == userIdInt);

            if (existingLike != null)
            {
                _itemLikeRepository.Remove(existingLike);

                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item != null)
                {
                    item.LikesCount = Math.Max(0, item.LikesCount - 1);
                    _itemRepository.Update(item);
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

                await _itemLikeRepository.AddAsync(newLike);

                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item != null)
                {
                    item.LikesCount++;
                    _itemRepository.Update(item);
                }
            }

            await _itemRepository.SaveChangesAsync();
            return existingLike == null; // Return true if like was added, false if removed
        }
    }
}
