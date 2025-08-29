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
    public class ItemRepo : Repo<Item>, IItemRepo
    {
        public ItemRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Item>> GetItemsByInventoryIdAsync(int inventoryId)
        {
            return await _dbSet
                .Include(i => i.CreatedBy)
                .Include(i => i.Likes)
                .Where(i => i.InventoryId == inventoryId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Item?> GetItemByCustomIdAsync(int inventoryId, string customId)
        {
            return await _dbSet
                .Include(i => i.CreatedBy)
                .Include(i => i.Inventory)
                .Include(i => i.Likes)
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId && i.CustomId == customId);
        }

        public async Task<IEnumerable<Item>> GetLatestItemsAsync(int count)
        {
            return await _dbSet
                .Include(i => i.CreatedBy)
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv.Category)
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Item>> GetMostLikedItemsAsync(int count)
        {
            return await _dbSet
                .Include(i => i.CreatedBy)
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv.Category)
                .Include(i => i.Likes)
                .OrderByDescending(i => i.LikesCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _dbSet
                .Include(i => i.CreatedBy)
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv.Category)
                .Where(i => i.CustomId.ToLower().Contains(lowerSearchTerm) ||
                           (i.TextField1Value != null && i.TextField1Value.ToLower().Contains(lowerSearchTerm)) ||
                           (i.TextField2Value != null && i.TextField2Value.ToLower().Contains(lowerSearchTerm)) ||
                           (i.TextField3Value != null && i.TextField3Value.ToLower().Contains(lowerSearchTerm)) ||
                           (i.MultiTextField1Value != null && i.MultiTextField1Value.ToLower().Contains(lowerSearchTerm)) ||
                           (i.MultiTextField2Value != null && i.MultiTextField2Value.ToLower().Contains(lowerSearchTerm)) ||
                           (i.MultiTextField3Value != null && i.MultiTextField3Value.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Item>> GetItemsByUserAsync(int userId)
        {
            return await _dbSet
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv.Category)
                .Where(i => i.CreatedById == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsCustomIdUniqueInInventoryAsync(int inventoryId, string customId, int? excludeItemId = null)
        {
            var query = _dbSet.Where(i => i.InventoryId == inventoryId && i.CustomId == customId);
            
            if (excludeItemId.HasValue)
            {
                query = query.Where(i => i.Id != excludeItemId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<string> GenerateNextCustomIdAsync(int inventoryId)
        {
            var lastItem = await _dbSet
                .Where(i => i.InventoryId == inventoryId)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            if (lastItem == null)
            {
                return "1";
            }

            // Try to parse the last custom ID as a number and increment
            if (int.TryParse(lastItem.CustomId, out int lastNumber))
            {
                return (lastNumber + 1).ToString();
            }

            // If not a number, get the count and add 1
            var count = await _dbSet.CountAsync(i => i.InventoryId == inventoryId);
            return (count + 1).ToString();
        }
    }
}
