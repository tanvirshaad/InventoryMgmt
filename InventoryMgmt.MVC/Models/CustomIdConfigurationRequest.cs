using System.Collections.Generic;
using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class CustomIdConfigurationRequest
    {
        public int InventoryId { get; set; }
        public List<CustomIdElement> Elements { get; set; } = new();
    }
}
