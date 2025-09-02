using AutoMapper;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL;
using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public interface IInventoryAccessService
    {
        Task<bool> CanUserEditInventoryAsync(int inventoryId, string userId, bool isAdmin = false);
        Task<IEnumerable<UserDto>> GetInventoryAccessUsersAsync(int inventoryId);
        Task GrantUserAccessAsync(int inventoryId, int userId, InventoryPermission permission = InventoryPermission.Write);
        Task UpdateUserAccessPermissionAsync(int inventoryId, int userId, InventoryPermission permission);
        Task<InventoryPermission> GetUserAccessPermissionAsync(int inventoryId, int userId);
        Task RevokeUserAccessAsync(int inventoryId, int userId);
    }

    public class InventoryAccessService : IInventoryAccessService
    {
        private readonly DataAccess _dataAccess;
        private readonly IMapper _mapper;

        public InventoryAccessService(DataAccess dataAccess, IMapper mapper)
        {
            _dataAccess = dataAccess;
            _mapper = mapper;
        }

        public async Task<bool> CanUserEditInventoryAsync(int inventoryId, string userId, bool isAdmin = false)
        {
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
            if (inventory == null) return false;

            int.TryParse(userId, out int userIdInt);
            if (isAdmin || inventory.OwnerId == userIdInt) return true;

            if (inventory.IsPublic) return true;

            var hasAccess = await _dataAccess.InventoryAccessData.ExistsAsync(ua =>
                ua.InventoryId == inventoryId && ua.UserId == userIdInt);
            return hasAccess;
        }

        public async Task<IEnumerable<UserDto>> GetInventoryAccessUsersAsync(int inventoryId)
        {
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
            if (inventory == null) return Enumerable.Empty<UserDto>();

            if (inventory.IsPublic)
            {
                // Return all users for public inventories
                var allUsers = await _dataAccess.UserData.GetAllAsync();
                var userDtos = _mapper.Map<IEnumerable<UserDto>>(allUsers);

                // For public inventories, indicate that all users have Read access by default
                foreach (var userDto in userDtos)
                {
                    userDto.AccessPermission = InventoryPermission.Read;
                }

                return userDtos;
            }
            else
            {
                // Return only users with explicit access
                var accessUsers = await _dataAccess.InventoryAccessData.GetAccessesByInventoryIdAsync(inventoryId);

                // Map users with their permissions
                var userDtos = new List<UserDto>();
                foreach (var access in accessUsers)
                {
                    var userDto = _mapper.Map<UserDto>(access.User);
                    userDto.AccessPermission = MapToBllPermission(access.Permission);
                    userDtos.Add(userDto);
                }

                return userDtos;
            }
        }

        public async Task GrantUserAccessAsync(int inventoryId, int userId, InventoryPermission permission = InventoryPermission.Write)
        {
            // Map BLL InventoryPermission to DAL InventoryAccessPermission
            var dalPermission = MapToDalPermission(permission);
            await _dataAccess.InventoryAccessData.GrantAccessAsync(inventoryId, userId, dalPermission);
        }

        public async Task UpdateUserAccessPermissionAsync(int inventoryId, int userId, InventoryPermission permission)
        {
            // Map BLL InventoryPermission to DAL InventoryAccessPermission
            var dalPermission = MapToDalPermission(permission);
            await _dataAccess.InventoryAccessData.UpdatePermissionAsync(inventoryId, userId, dalPermission);
        }

        public async Task<InventoryPermission> GetUserAccessPermissionAsync(int inventoryId, int userId)
        {
            var dalPermission = await _dataAccess.InventoryAccessData.GetUserPermissionAsync(inventoryId, userId);
            // Map DAL InventoryAccessPermission to BLL InventoryPermission
            return MapToBllPermission(dalPermission);
        }

        public async Task RevokeUserAccessAsync(int inventoryId, int userId)
        {
            await _dataAccess.InventoryAccessData.RevokeAccessAsync(inventoryId, userId);
        }

        private InventoryAccessPermission MapToDalPermission(InventoryPermission permission)
        {
            return permission switch
            {
                InventoryPermission.None => InventoryAccessPermission.None,
                InventoryPermission.Read => InventoryAccessPermission.Read,
                InventoryPermission.Write => InventoryAccessPermission.Write,
                InventoryPermission.Manage => InventoryAccessPermission.Manage,
                InventoryPermission.FullControl => InventoryAccessPermission.FullControl,
                _ => InventoryAccessPermission.None
            };
        }

        private InventoryPermission MapToBllPermission(InventoryAccessPermission permission)
        {
            return permission switch
            {
                InventoryAccessPermission.None => InventoryPermission.None,
                InventoryAccessPermission.Read => InventoryPermission.Read,
                InventoryAccessPermission.Write => InventoryPermission.Write,
                InventoryAccessPermission.Manage => InventoryPermission.Manage,
                InventoryAccessPermission.FullControl => InventoryPermission.FullControl,
                _ => InventoryPermission.None
            };
        }
    }
}
