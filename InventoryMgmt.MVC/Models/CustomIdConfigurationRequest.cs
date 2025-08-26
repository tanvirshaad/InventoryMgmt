using System.Collections.Generic;
using System.Text.Json.Serialization;
using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.MVC.Models
{
    public class CustomIdConfigurationRequest
    {
        [JsonPropertyName("inventoryId")]
        public int InventoryId { get; set; }
        
        [JsonPropertyName("elements")]
        public List<CustomIdElement> Elements { get; set; } = new List<CustomIdElement>();
        
        public override string ToString()
        {
            return $"CustomIdConfigurationRequest: InventoryId={InventoryId}, Elements.Count={Elements?.Count ?? 0}";
        }
    }
}
