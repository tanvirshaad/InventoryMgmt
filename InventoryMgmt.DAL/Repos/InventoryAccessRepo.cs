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
    public class InventoryAccessRepo : Repo<InventoryAccess>, IInventoryAccessRepo
    {
        public InventoryAccessRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<InventoryAccess>> GetAccessesByInventoryIdAsync(int inventoryId)
        {
            return await _dbSet
                .Include(ia => ia.User)
                .Where(ia => ia.InventoryId == inventoryId)
                .OrderBy(ia => ia.User.FirstName)
                .ThenBy(ia => ia.User.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryAccess>> GetAccessesByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ia => ia.Inventory)
                .ThenInclude(i => i.Category)
                .Include(ia => ia.Inventory.Owner)
                .Where(ia => ia.UserId == userId)
                .OrderByDescending(ia => ia.GrantedAt)
                .ToListAsync();
        }

        public async Task<InventoryAccess?> GetAccessAsync(int inventoryId, int userId)
        {
            return await _dbSet
                .Include(ia => ia.User)
                .Include(ia => ia.Inventory)
                .FirstOrDefaultAsync(ia => ia.InventoryId == inventoryId && ia.UserId == userId);
        }

        public async Task<bool> HasUserAccessAsync(int inventoryId, int userId)
        {
            return await _dbSet
                .AnyAsync(ia => ia.InventoryId == inventoryId && ia.UserId == userId);
        }

        public async Task GrantAccessAsync(int inventoryId, int userId)
        {
            var existingAccess = await GetAccessAsync(inventoryId, userId);
            if (existingAccess == null)
            {
                var access = new InventoryAccess
                {
                    InventoryId = inventoryId,
                    UserId = userId,
                    GrantedAt = DateTime.UtcNow
                };

                await AddAsync(access);
                await SaveChangesAsync();
            }
        }

        public async Task RevokeAccessAsync(int inventoryId, int userId)
        {
            var access = await GetAccessAsync(inventoryId, userId);
            if (access != null)
            {
                Remove(access);
                await SaveChangesAsync();
            }
        }
    }
}
