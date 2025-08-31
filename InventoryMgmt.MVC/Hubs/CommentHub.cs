using Microsoft.AspNetCore.SignalR;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AuthService = InventoryMgmt.BLL.Services.AuthorizationService;

namespace InventoryMgmt.MVC.Hubs
{
    public class CommentHub : Hub
    {
        private readonly CommentService _commentService;
        private readonly AuthService _authService;

        public CommentHub(CommentService commentService, AuthService authService)
        {
            _commentService = commentService;
            _authService = authService;
        }

        public async Task JoinInventoryGroup(int inventoryId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"inventory_{inventoryId}");
        }

        public async Task LeaveInventoryGroup(int inventoryId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"inventory_{inventoryId}");
        }
        
        [Authorize]
        public async Task SendComment(int inventoryId, string content)
        {
            // Get the user ID from the claims
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User is not authenticated");
            }

            // Check if user can comment
            var userIdInt = int.Parse(userId);
            if (!await _authService.CanCommentAsync(userIdInt, inventoryId))
            {
                throw new HubException("You don't have permission to comment on this inventory");
            }

            // Add the comment through the service
            var commentDto = new CommentDto
            {
                InventoryId = inventoryId,
                Content = content,
                UserId = userId
            };

            var result = await _commentService.AddCommentAsync(commentDto);
            if (result)
            {
                // Notify all clients in the inventory group that a comment was added
                await Clients.Group($"inventory_{inventoryId}").SendAsync("CommentAdded", inventoryId);
            }
            else
            {
                throw new HubException("Failed to add comment");
            }
        }

        public async Task CommentAdded(int inventoryId)
        {
            // Notify all clients in the inventory group
            await Clients.Group($"inventory_{inventoryId}").SendAsync("CommentAdded", inventoryId);
        }
    }
}
