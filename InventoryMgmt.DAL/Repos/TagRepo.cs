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
    public class TagRepo : Repo<Tag>, ITagRepo
    {
        public TagRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count)
        {
            return await _dbSet
                .OrderByDescending(t => t.UsageCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(t => t.Name.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag> GetOrCreateTagAsync(string tagName)
        {
            var existingTag = await GetTagByNameAsync(tagName);
            if (existingTag != null)
            {
                return existingTag;
            }

            var newTag = new Tag
            {
                Name = tagName.Trim(),
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await AddAsync(newTag);
            await SaveChangesAsync();
            return newTag;
        }

        public async Task IncrementUsageCountAsync(int tagId)
        {
            var tag = await GetByIdAsync(tagId);
            if (tag != null)
            {
                tag.UsageCount++;
                Update(tag);
                await SaveChangesAsync();
            }
        }

        public async Task DecrementUsageCountAsync(int tagId)
        {
            var tag = await GetByIdAsync(tagId);
            if (tag != null && tag.UsageCount > 0)
            {
                tag.UsageCount--;
                Update(tag);
                await SaveChangesAsync();
            }
        }
    }
}
