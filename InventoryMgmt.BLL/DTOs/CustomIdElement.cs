using System;

namespace InventoryMgmt.BLL.DTOs
{
    public class CustomIdElement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty; // Fixed, 20-bit random, 32-bit random, 6-digit random, 9-digit random, GUID, Date/time, Sequence
        public string Value { get; set; } = string.Empty; // Format string or fixed text
        public string Description { get; set; } = string.Empty; // Help text
        public int Order { get; set; } = 0;
    }
}
