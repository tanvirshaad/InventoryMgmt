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
    public class CategoryRepo : Repo<Category>, ICategoryRepo
    {
        public CategoryRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithInventoryCountAsync()
        {
            return await _dbSet
                .Include(c => c.Inventories)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Category>> GetMostUsedCategoriesAsync(int count)
        {
            return await _dbSet
                .Include(c => c.Inventories)
                .OrderByDescending(c => c.Inventories.Count)
                .Take(count)
                .ToListAsync();
        }
    }
}
