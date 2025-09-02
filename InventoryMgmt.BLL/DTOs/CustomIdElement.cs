using System;
using System.Text.Json.Serialization;

namespace InventoryMgmt.BLL.DTOs
{
    public class CustomIdElement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Type { get; set; } = string.Empty; // Fixed, 20-bit random, 32-bit random, 6-digit random, 9-digit random, GUID, Date/time, Sequence
        
        [JsonIgnore]
        public string EscapedValue 
        {
            get { return Value; } // Use the raw value directly
        }
        
        public string Value { get; set; } = string.Empty; // Format string or fixed text
        
        public string Description { get; set; } = string.Empty; // Help text
        
        public int Order { get; set; } = 0;
        
        // Helper method to ensure we preserve all characters
        public string GetCleanValue()
        {
            // For fixed type, return the exact value
            if (Type?.ToLower() == "fixed")
            {
                return Value ?? string.Empty;
            }
            
            return Value ?? string.Empty;
        }
    }
}
