using System.Collections.Generic;
using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class CustomIdPreviewRequest
    {
        public List<CustomIdElement> Elements { get; set; } = new();
    }
}
