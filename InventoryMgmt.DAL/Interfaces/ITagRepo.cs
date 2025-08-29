using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface ITagRepo : IRepo<Tag>
    {
        Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count);
        Task<Tag?> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm);
        Task<Tag> GetOrCreateTagAsync(string tagName);
        Task IncrementUsageCountAsync(int tagId);
        Task DecrementUsageCountAsync(int tagId);
    }
}
