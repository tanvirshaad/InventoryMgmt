using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public class CommentService
    {
        private readonly IRepo<Comment> _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(IRepo<Comment> commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByInventoryIdAsync(int inventoryId)
        {
            var comments = await _commentRepository.GetAllAsync();
            var filteredComments = comments.Where(c => c.InventoryId == inventoryId)
                                         .OrderBy(c => c.CreatedAt);
            return _mapper.Map<IEnumerable<CommentDto>>(filteredComments);
        }

        public async Task<bool> AddCommentAsync(CommentDto commentDto)
        {
            var comment = _mapper.Map<Comment>(commentDto);
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null) return false;

            // Only allow the comment author or admin to delete
            if (comment.UserId.ToString() != userId)
            {
                return false;
            }

            _commentRepository.Remove(comment);
            await _commentRepository.SaveChangesAsync();
            return true;
        }
    }
}
