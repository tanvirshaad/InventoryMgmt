using InventoryMgmt.DAL.Data;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Repos
{
    public class ItemLikeRepo : Repo<ItemLike>, IItemLikeRepo
    {
        public ItemLikeRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ItemLike>> GetLikesByItemIdAsync(int itemId)
        {
            return await _dbSet
                .Include(il => il.User)
                .Where(il => il.ItemId == itemId)
                .OrderByDescending(il => il.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ItemLike>> GetLikesByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(il => il.Item)
                .ThenInclude(i => i.Inventory)
                .ThenInclude(inv => inv.Category)
                .Where(il => il.UserId == userId)
                .OrderByDescending(il => il.CreatedAt)
                .ToListAsync();
        }

        public async Task<ItemLike?> GetLikeAsync(int itemId, int userId)
        {
            return await _dbSet
                .Include(il => il.Item)
                .Include(il => il.User)
                .FirstOrDefaultAsync(il => il.ItemId == itemId && il.UserId == userId);
        }

        public async Task<bool> HasUserLikedItemAsync(int itemId, int userId)
        {
            return await _dbSet
                .AnyAsync(il => il.ItemId == itemId && il.UserId == userId);
        }

        public async Task LikeItemAsync(int itemId, int userId)
        {
            var existingLike = await GetLikeAsync(itemId, userId);
            if (existingLike == null)
            {
                var like = new ItemLike
                {
                    ItemId = itemId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await AddAsync(like);
                
                // Update the item's like count
                var item = await _context.Items.FindAsync(itemId);
                if (item != null)
                {
                    item.LikesCount++;
                    _context.Items.Update(item);
                }

                await SaveChangesAsync();
            }
        }

        public async Task UnlikeItemAsync(int itemId, int userId)
        {
            var like = await GetLikeAsync(itemId, userId);
            if (like != null)
            {
                Remove(like);
                
                // Update the item's like count
                var item = await _context.Items.FindAsync(itemId);
                if (item != null && item.LikesCount > 0)
                {
                    item.LikesCount--;
                    _context.Items.Update(item);
                }

                await SaveChangesAsync();
            }
        }

        public async Task<int> GetLikeCountByItemIdAsync(int itemId)
        {
            return await _dbSet
                .CountAsync(il => il.ItemId == itemId);
        }
    }
}
