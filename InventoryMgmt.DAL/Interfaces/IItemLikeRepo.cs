using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface IItemLikeRepo : IRepo<ItemLike>
    {
        Task<IEnumerable<ItemLike>> GetLikesByItemIdAsync(int itemId);
        Task<IEnumerable<ItemLike>> GetLikesByUserIdAsync(int userId);
        Task<ItemLike?> GetLikeAsync(int itemId, int userId);
        Task<bool> HasUserLikedItemAsync(int itemId, int userId);
        Task LikeItemAsync(int itemId, int userId);
        Task UnlikeItemAsync(int itemId, int userId);
        Task<int> GetLikeCountByItemIdAsync(int itemId);
    }
}
