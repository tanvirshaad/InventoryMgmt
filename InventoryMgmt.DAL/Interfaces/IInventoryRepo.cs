using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InventoryMgmt.DAL.Interfaces
{
    public interface IInventoryRepo: IRepo<Inventory>
    {
        Task<IEnumerable<Inventory>> GetLatestInventoriesAsync(int count);
        Task<IEnumerable<Inventory>> GetMostPopularInventoriesAsync(int count);
        Task<IEnumerable<Inventory>> SearchInventoriesAsync(string searchTerm);
        Task<IEnumerable<Inventory>> GetInventoriesByTagAsync(int tagId);
        Task<IEnumerable<Inventory>> GetUserOwnedInventoriesAsync(int userId);
        Task<IEnumerable<Inventory>> GetUserAccessibleInventoriesAsync(int userId);
    }
}
