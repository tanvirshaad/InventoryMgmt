using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class InventoryDetailsViewModel
    {
        public InventoryDto Inventory { get; set; } = null!;
        public UserInventoryPermissions UserPermissions { get; set; } = null!;
        public IEnumerable<ItemDto> Items { get; set; } = new List<ItemDto>();
        public bool CanEdit => UserPermissions.Permission >= InventoryPermission.FullControl;
        public bool CanAddItems => UserPermissions.Permission >= InventoryPermission.Write;
        public bool CanComment => UserPermissions.UserRole != UserRole.Anonymous;
        public bool CanManageAccess => UserPermissions.Permission >= InventoryPermission.FullControl;
        public string CurrentTab { get; set; } = "items";
    }
}
