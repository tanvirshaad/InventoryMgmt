using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace InventoryMgmt.BLL.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] Version { get; set; } = Array.Empty<byte>();
        public string CustomIdFormat { get; set; } = string.Empty;

        // Advanced Custom ID Configuration
        public string CustomIdElements { get; set; } = string.Empty;
        public List<CustomIdElement> CustomIdElementList { get; set; } = new();

        // Custom field configurations
        public CustomFieldConfig TextField1 { get; set; } = new();
        public CustomFieldConfig TextField2 { get; set; } = new();
        public CustomFieldConfig TextField3 { get; set; } = new();
        public CustomFieldConfig MultiTextField1 { get; set; } = new();
        public CustomFieldConfig MultiTextField2 { get; set; } = new();
        public CustomFieldConfig MultiTextField3 { get; set; } = new();
        public CustomFieldConfig NumericField1 { get; set; } = new();
        public CustomFieldConfig NumericField2 { get; set; } = new();
        public CustomFieldConfig NumericField3 { get; set; } = new();
        public CustomFieldConfig DocumentField1 { get; set; } = new();
        public CustomFieldConfig DocumentField2 { get; set; } = new();
        public CustomFieldConfig DocumentField3 { get; set; } = new();
        public CustomFieldConfig BooleanField1 { get; set; } = new();
        public CustomFieldConfig BooleanField2 { get; set; } = new();
        public CustomFieldConfig BooleanField3 { get; set; } = new();

        // Navigation properties
        public UserDto? Owner { get; set; }
        public CategoryDto? Category { get; set; }
        public List<TagDto> Tags { get; set; } = new();
        public List<UserDto> AccessUsers { get; set; } = new();
        public int ItemsCount { get; set; }
    }

    public class CustomFieldConfig
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool ShowInTable { get; set; }
        public NumericFieldConfig? NumericConfig { get; set; }
    }
}
