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
    public class UserRepo : Repo<User>, IUserRepo
    {
        public UserRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserByUserNameAsync(string userName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet
                .Where(u => !u.IsBlocked)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetBlockedUsersAsync()
        {
            return await _dbSet
                .Where(u => u.IsBlocked)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _dbSet
                .Where(u => u.FirstName!.ToLower().Contains(lowerSearchTerm) ||
                           u.LastName!.ToLower().Contains(lowerSearchTerm) ||
                           u.UserName.ToLower().Contains(lowerSearchTerm) ||
                           u.Email.ToLower().Contains(lowerSearchTerm))
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _dbSet
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            return await _dbSet
                .AnyAsync(u => u.UserName.ToLower() == userName.ToLower());
        }

        public async Task<User?> GetUserWithInventoriesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.OwnedInventories)
                .ThenInclude(i => i.Category)
                .Include(u => u.InventoryAccesses)
                .ThenInclude(ia => ia.Inventory)
                .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
