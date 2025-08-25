namespace InventoryMgmt.MVC.Models
{
    public class AddCommentRequest
    {
        public int InventoryId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
