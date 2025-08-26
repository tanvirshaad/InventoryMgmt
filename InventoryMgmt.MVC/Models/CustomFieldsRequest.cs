using System.Collections.Generic;
using System.Text.Json.Serialization;
using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class CustomFieldsRequest
    {
        [JsonPropertyName("inventoryId")]
        public int InventoryId { get; set; }
        
        [JsonPropertyName("fields")]
        public List<CustomFieldData> Fields { get; set; } = new List<CustomFieldData>();
        
        public override string ToString()
        {
            return $"CustomFieldsRequest: InventoryId={InventoryId}, Fields.Count={Fields?.Count ?? 0}";
        }
    }
}
