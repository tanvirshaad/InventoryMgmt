using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface IUserRepo : IRepo<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUserNameAsync(string userName);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetBlockedUsersAsync();
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsUserNameExistsAsync(string userName);
        Task<User?> GetUserWithInventoriesAsync(int userId);
    }
}
