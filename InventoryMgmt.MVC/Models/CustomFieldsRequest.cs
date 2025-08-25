using System.Collections.Generic;
using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class CustomFieldsRequest
    {
        public int InventoryId { get; set; }
        public List<CustomFieldData> Fields { get; set; } = new();
    }
}
