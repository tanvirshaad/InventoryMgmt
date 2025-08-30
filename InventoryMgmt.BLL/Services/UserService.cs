using AutoMapper;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.Interfaces;
using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IMapper _mapper;

        public UserService(IUserRepo userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName));
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepo.FindFirstAsync(u => u.Email == email);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm)
        {
            var users = await _userRepo.FindAsync(u => 
                !u.IsBlocked && 
                (u.Email.Contains(searchTerm) || 
                 (u.FirstName != null && u.FirstName.Contains(searchTerm)) || 
                 (u.LastName != null && u.LastName.Contains(searchTerm)) ||
                 u.UserName.Contains(searchTerm)));
            return _mapper.Map<IEnumerable<UserDto>>(users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName));
        }

        public async Task BlockUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                user.IsBlocked = true;
                user.UpdatedAt = DateTime.UtcNow;
                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync();
            }
        }

        public async Task UnblockUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                user.IsBlocked = false;
                user.UpdatedAt = DateTime.UtcNow;
                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync();
            }
        }

        public async Task SetUserRoleAsync(int userId, string role)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                user.Role = role;
                user.UpdatedAt = DateTime.UtcNow;
                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync();
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                _userRepo.Remove(user);
                await _userRepo.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            return user != null;
        }
    }
}
