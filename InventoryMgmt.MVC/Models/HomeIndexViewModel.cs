using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<InventoryDto> LatestInventories { get; set; } = new List<InventoryDto>();
        public IEnumerable<InventoryDto> PopularInventories { get; set; } = new List<InventoryDto>();
        public IEnumerable<TagDto> PopularTags { get; set; } = new List<TagDto>();
        public IEnumerable<InventoryDto> UserOwnedInventories { get; set; } = new List<InventoryDto>();
        public IEnumerable<InventoryDto> UserAccessibleInventories { get; set; } = new List<InventoryDto>();
    }
}
