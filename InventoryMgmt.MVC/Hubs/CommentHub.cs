using Microsoft.AspNetCore.SignalR;

namespace InventoryMgmt.MVC.Hubs
{
    public class CommentHub : Hub
    {
        public async Task JoinInventoryGroup(int inventoryId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"inventory_{inventoryId}");
        }

        public async Task LeaveInventoryGroup(int inventoryId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"inventory_{inventoryId}");
        }

        public async Task SendComment(int inventoryId, string comment)
        {
            await Clients.Group($"inventory_{inventoryId}").SendAsync("ReceiveComment", inventoryId, comment);
        }

        public async Task CommentAdded(int inventoryId)
        {
            await Clients.Group($"inventory_{inventoryId}").SendAsync("CommentAdded", inventoryId);
        }
    }
}
