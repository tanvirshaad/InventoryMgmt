using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface ICommentRepo : IRepo<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsByInventoryIdAsync(int inventoryId);
        Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(int userId);
        Task<IEnumerable<Comment>> GetLatestCommentsAsync(int count);
        Task<int> GetCommentCountByInventoryIdAsync(int inventoryId);
    }
}
