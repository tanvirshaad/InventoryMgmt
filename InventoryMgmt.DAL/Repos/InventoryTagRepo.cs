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
    public class InventoryTagRepo : Repo<InventoryTag>, IInventoryTagRepo
    {
        public InventoryTagRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<InventoryTag>> GetTagsByInventoryIdAsync(int inventoryId)
        {
            return await _dbSet
                .Include(it => it.Tag)
                .Where(it => it.InventoryId == inventoryId)
                .OrderBy(it => it.Tag.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTag>> GetInventoriesByTagIdAsync(int tagId)
        {
            return await _dbSet
                .Include(it => it.Inventory)
                .ThenInclude(i => i.Owner)
                .Include(it => it.Inventory.Category)
                .Where(it => it.TagId == tagId)
                .OrderByDescending(it => it.Inventory.UpdatedAt)
                .ToListAsync();
        }

        public async Task<InventoryTag?> GetInventoryTagAsync(int inventoryId, int tagId)
        {
            return await _dbSet
                .Include(it => it.Tag)
                .Include(it => it.Inventory)
                .FirstOrDefaultAsync(it => it.InventoryId == inventoryId && it.TagId == tagId);
        }

        public async Task AddTagToInventoryAsync(int inventoryId, int tagId)
        {
            var existingTag = await GetInventoryTagAsync(inventoryId, tagId);
            if (existingTag == null)
            {
                var inventoryTag = new InventoryTag
                {
                    InventoryId = inventoryId,
                    TagId = tagId
                };

                await AddAsync(inventoryTag);
                await SaveChangesAsync();
            }
        }

        public async Task RemoveTagFromInventoryAsync(int inventoryId, int tagId)
        {
            var inventoryTag = await GetInventoryTagAsync(inventoryId, tagId);
            if (inventoryTag != null)
            {
                Remove(inventoryTag);
                await SaveChangesAsync();
            }
        }

        public async Task<bool> IsTagAssignedToInventoryAsync(int inventoryId, int tagId)
        {
            return await _dbSet
                .AnyAsync(it => it.InventoryId == inventoryId && it.TagId == tagId);
        }
    }
}
