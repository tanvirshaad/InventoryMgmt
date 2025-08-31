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
    public class InventoryRepo: Repo<Inventory>, IInventoryRepo
    {
        public InventoryRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Inventory>> GetLatestInventoriesAsync(int count)
        {
            return await _dbSet
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.Items)
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetMostPopularInventoriesAsync(int count)
        {
            return await _dbSet
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.Items)
                .OrderByDescending(i => i.Items.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> SearchInventoriesAsync(string searchTerm)
        {
            return await _dbSet
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.Items)
                .Where(i => i.Title.Contains(searchTerm) ||
                           (i.Description != null && i.Description.Contains(searchTerm)) ||
                           i.Owner.FirstName!.Contains(searchTerm) ||
                           i.Owner.LastName!.Contains(searchTerm) ||
                           i.Category.Name.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetInventoriesByTagAsync(int tagId)
        {
            return await _dbSet
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.InventoryTags)
                .Include(i => i.Items)
                .Where(i => i.InventoryTags.Any(it => it.TagId == tagId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetUserOwnedInventoriesAsync(int userId)
        {
            return await _dbSet
                .Include(i => i.Category)
                .Include(i => i.Items)
                .Where(i => i.OwnerId == userId)
                .OrderByDescending(i => i.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetUserAccessibleInventoriesAsync(int userId)
        {
            return await _dbSet
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.UserAccesses)
                .Include(i => i.Items)
                .Where(i => i.UserAccesses.Any(ua => ua.UserId == userId))
                .OrderByDescending(i => i.UpdatedAt)
                .ToListAsync();
        }
    }
}
