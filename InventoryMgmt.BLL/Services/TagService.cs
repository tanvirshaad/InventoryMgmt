using AutoMapper;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetInventoryTagsAsync(int inventoryId);
        Task<IEnumerable<TagDto>> SearchTagsAsync(string searchTerm);
        Task<bool> AddTagToInventoryAsync(int inventoryId, string tagName);
        Task<bool> AddTagsToInventoryAsync(int inventoryId, List<string> tagNames);
        Task<bool> RemoveTagFromInventoryAsync(int inventoryId, int tagId);
        Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10);
    }

    public class TagService : ITagService
    {
        private readonly DataAccess _dataAccess;
        private readonly IMapper _mapper;

        public TagService(DataAccess dataAccess, IMapper mapper)
        {
            _dataAccess = dataAccess;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all tags associated with an inventory
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <returns>Collection of tag DTOs</returns>
        public async Task<IEnumerable<TagDto>> GetInventoryTagsAsync(int inventoryId)
        {
            var inventoryTags = await _dataAccess.InventoryTagData.GetTagsByInventoryIdAsync(inventoryId);
            return _mapper.Map<IEnumerable<TagDto>>(inventoryTags.Select(it => it.Tag));
        }

        /// <summary>
        /// Searches for tags that match the provided search term
        /// </summary>
        /// <param name="searchTerm">The search term to filter tags</param>
        /// <returns>Collection of tag DTOs that match the search term</returns>
        public async Task<IEnumerable<TagDto>> SearchTagsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<TagDto>();

            var tags = await _dataAccess.TagData.SearchTagsAsync(searchTerm);
            return _mapper.Map<IEnumerable<TagDto>>(tags);
        }

        /// <summary>
        /// Adds a tag to an inventory. If the tag doesn't exist, it will be created.
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="tagName">The tag name to add</param>
        /// <returns>True if the operation was successful</returns>
        public async Task<bool> AddTagToInventoryAsync(int inventoryId, string tagName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tagName))
                    return false;

                // Trim and normalize the tag name
                tagName = tagName.Trim();

                // Get or create the tag
                var tag = await _dataAccess.TagData.GetOrCreateTagAsync(tagName);

                // Add the tag to the inventory if it doesn't already exist
                if (!await _dataAccess.InventoryTagData.IsTagAssignedToInventoryAsync(inventoryId, tag.Id))
                {
                    await _dataAccess.InventoryTagData.AddTagToInventoryAsync(inventoryId, tag.Id);

                    // Increment the tag usage count
                    await _dataAccess.TagData.IncrementUsageCountAsync(tag.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding tag to inventory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds multiple tags to an inventory. If any tag doesn't exist, it will be created.
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="tagNames">List of tag names to add</param>
        /// <returns>True if all tags were added successfully</returns>
        public async Task<bool> AddTagsToInventoryAsync(int inventoryId, List<string> tagNames)
        {
            if (tagNames == null || !tagNames.Any())
                return false;

            bool allSucceeded = true;

            foreach (var tagName in tagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var result = await AddTagToInventoryAsync(inventoryId, tagName);
                if (!result)
                {
                    allSucceeded = false;
                }
            }

            return allSucceeded;
        }

        /// <summary>
        /// Removes a tag from an inventory
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <param name="tagId">The tag ID to remove</param>
        /// <returns>True if the operation was successful</returns>
        public async Task<bool> RemoveTagFromInventoryAsync(int inventoryId, int tagId)
        {
            try
            {
                // Check if the tag is assigned to the inventory
                if (await _dataAccess.InventoryTagData.IsTagAssignedToInventoryAsync(inventoryId, tagId))
                {
                    await _dataAccess.InventoryTagData.RemoveTagFromInventoryAsync(inventoryId, tagId);

                    // Decrement the tag usage count
                    await _dataAccess.TagData.DecrementUsageCountAsync(tagId);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing tag from inventory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the most popular tags
        /// </summary>
        /// <param name="count">The number of tags to return</param>
        /// <returns>Collection of the most used tags</returns>
        public async Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10)
        {
            var tags = await _dataAccess.TagData.GetMostUsedTagsAsync(count);
            return _mapper.Map<IEnumerable<TagDto>>(tags);
        }
    }
}
