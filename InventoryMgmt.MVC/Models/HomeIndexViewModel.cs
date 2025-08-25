using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<InventoryDto> LatestInventories { get; set; } = new List<InventoryDto>();
        public IEnumerable<InventoryDto> PopularInventories { get; set; } = new List<InventoryDto>();
    }
}
