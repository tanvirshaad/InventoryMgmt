using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryMgmt.DAL;

namespace InventoryMgmt.BLL.Services
{
    public class CommentService
    {
        private readonly DataAccess _dataAccess;
        private readonly IMapper _mapper;

        public CommentService(DataAccess _da, IMapper mapper)
        {
            _dataAccess = _da;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByInventoryIdAsync(int inventoryId)
        {
            // Use the repository method instead of getting all comments and filtering in memory
            var comments = await _dataAccess.CommentData.GetCommentsByInventoryIdAsync(inventoryId);
            
            // Sort comments by creation date (oldest first)
            var sortedComments = comments.OrderBy(c => c.CreatedAt);
            
            // Map to DTOs
            return _mapper.Map<IEnumerable<CommentDto>>(sortedComments);
        }

        public async Task<bool> AddCommentAsync(CommentDto commentDto)
        {
            var comment = _mapper.Map<Comment>(commentDto);
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            await _dataAccess.CommentData.AddAsync(comment);
            await _dataAccess.CommentData.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _dataAccess.CommentData.GetByIdAsync(commentId);
            if (comment == null) return false;

            // Only allow the comment author or admin to delete
            if (comment.UserId.ToString() != userId)
            {
                return false;
            }

            _dataAccess.CommentData.Remove(comment);
            await _dataAccess.CommentData.SaveChangesAsync();
            return true;
        }
    }
}
