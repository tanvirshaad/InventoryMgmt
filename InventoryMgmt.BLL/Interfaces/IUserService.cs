using InventoryMgmt.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm);
        Task<IEnumerable<UserDto>> GetAdminUsersAsync();
        Task BlockUserAsync(int userId);
        Task UnblockUserAsync(int userId);
        Task SetUserRoleAsync(int userId, string role);
        Task DeleteUserAsync(int userId);
        Task<bool> UserExistsAsync(int userId);
    }
}
