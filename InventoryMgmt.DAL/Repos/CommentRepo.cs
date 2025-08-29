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
    public class CommentRepo : Repo<Comment>, ICommentRepo
    {
        public CommentRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetCommentsByInventoryIdAsync(int inventoryId)
        {
            return await _dbSet
                .Include(c => c.User)
                .Where(c => c.InventoryId == inventoryId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(c => c.Inventory)
                .ThenInclude(i => i.Category)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetLatestCommentsAsync(int count)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Inventory)
                .ThenInclude(i => i.Category)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetCommentCountByInventoryIdAsync(int inventoryId)
        {
            return await _dbSet
                .CountAsync(c => c.InventoryId == inventoryId);
        }
    }
}
