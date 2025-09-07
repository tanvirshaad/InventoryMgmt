using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string CustomId { get; set; } = string.Empty;
        public string? CreatedById { get; set; } = "1"; // Default value to prevent validation errors
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] Version { get; set; } = Array.Empty<byte>();
        public int LikesCount { get; set; }
        
        // Basic information - Name is kept for compatibility but populated from TextField1Value
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;

        // Custom field values
        public string? TextField1Value { get; set; }
        public string? TextField2Value { get; set; }
        public string? TextField3Value { get; set; }
        public string? MultiTextField1Value { get; set; }
        public string? MultiTextField2Value { get; set; }
        public string? MultiTextField3Value { get; set; }
        public decimal? NumericField1Value { get; set; }
        public decimal? NumericField2Value { get; set; }
        public decimal? NumericField3Value { get; set; }
        public string? DocumentField1Name { get; set; }
        public string? DocumentField1Value { get; set; }
        public string? DocumentField2Name { get; set; }
        public string? DocumentField2Value { get; set; }
        public string? DocumentField3Name { get; set; }
        public string? DocumentField3Value { get; set; }
        public bool? BooleanField1Value { get; set; }
        public bool? BooleanField2Value { get; set; }
        public bool? BooleanField3Value { get; set; }

        // Navigation properties
        public UserDto? CreatedBy { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }
}
