using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface ICategoryRepo : IRepo<Category>
    {
        Task<IEnumerable<Category>> GetCategoriesWithInventoryCountAsync();
        Task<Category?> GetCategoryByNameAsync(string name);
        Task<IEnumerable<Category>> GetMostUsedCategoriesAsync(int count);
    }
}
