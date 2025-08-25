using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int CategoryId { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsPublic { get; set; }

        public int OwnerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] Version { get; set; } = Array.Empty<byte>();

        // Custom ID Configuration
        [MaxLength(500)]
        public string CustomIdFormat { get; set; } = string.Empty;

        // Advanced Custom ID Configuration (JSON)
        [MaxLength(2000)]
        public string CustomIdElements { get; set; } = string.Empty; // JSON array of ID elements

        // Custom Fields Configuration
        // Text fields (up to 3)
        [MaxLength(100)]
        public string? TextField1Name { get; set; }
        [MaxLength(200)]
        public string? TextField1Description { get; set; }
        public bool TextField1ShowInTable { get; set; }

        [MaxLength(100)]
        public string? TextField2Name { get; set; }
        [MaxLength(200)]
        public string? TextField2Description { get; set; }
        public bool TextField2ShowInTable { get; set; }

        [MaxLength(100)]
        public string? TextField3Name { get; set; }
        [MaxLength(200)]
        public string? TextField3Description { get; set; }
        public bool TextField3ShowInTable { get; set; }

        // Multiline text fields (up to 3)
        [MaxLength(100)]
        public string? MultiTextField1Name { get; set; }
        [MaxLength(200)]
        public string? MultiTextField1Description { get; set; }
        public bool MultiTextField1ShowInTable { get; set; }

        [MaxLength(100)]
        public string? MultiTextField2Name { get; set; }
        [MaxLength(200)]
        public string? MultiTextField2Description { get; set; }
        public bool MultiTextField2ShowInTable { get; set; }

        [MaxLength(100)]
        public string? MultiTextField3Name { get; set; }
        [MaxLength(200)]
        public string? MultiTextField3Description { get; set; }
        public bool MultiTextField3ShowInTable { get; set; }

        // Numeric fields (up to 3)
        [MaxLength(100)]
        public string? NumericField1Name { get; set; }
        [MaxLength(200)]
        public string? NumericField1Description { get; set; }
        public bool NumericField1ShowInTable { get; set; }
        public bool NumericField1IsInteger { get; set; }
        public decimal? NumericField1MinValue { get; set; }
        public decimal? NumericField1MaxValue { get; set; }
        public decimal NumericField1StepValue { get; set; } = 0.01m;
        [MaxLength(20)]
        public string? NumericField1DisplayFormat { get; set; }

        [MaxLength(100)]
        public string? NumericField2Name { get; set; }
        [MaxLength(200)]
        public string? NumericField2Description { get; set; }
        public bool NumericField2ShowInTable { get; set; }
        public bool NumericField2IsInteger { get; set; }
        public decimal? NumericField2MinValue { get; set; }
        public decimal? NumericField2MaxValue { get; set; }
        public decimal NumericField2StepValue { get; set; } = 0.01m;
        [MaxLength(20)]
        public string? NumericField2DisplayFormat { get; set; }

        [MaxLength(100)]
        public string? NumericField3Name { get; set; }
        [MaxLength(200)]
        public string? NumericField3Description { get; set; }
        public bool NumericField3ShowInTable { get; set; }
        public bool NumericField3IsInteger { get; set; }
        public decimal? NumericField3MinValue { get; set; }
        public decimal? NumericField3MaxValue { get; set; }
        public decimal NumericField3StepValue { get; set; } = 0.01m;
        [MaxLength(20)]
        public string? NumericField3DisplayFormat { get; set; }

        // Document/Image fields (up to 3)
        [MaxLength(100)]
        public string? DocumentField1Name { get; set; }
        [MaxLength(200)]
        public string? DocumentField1Description { get; set; }
        public bool DocumentField1ShowInTable { get; set; }

        [MaxLength(100)]
        public string? DocumentField2Name { get; set; }
        [MaxLength(200)]
        public string? DocumentField2Description { get; set; }
        public bool DocumentField2ShowInTable { get; set; }

        [MaxLength(100)]
        public string? DocumentField3Name { get; set; }
        [MaxLength(200)]
        public string? DocumentField3Description { get; set; }
        public bool DocumentField3ShowInTable { get; set; }

        // Boolean fields (up to 3)
        [MaxLength(100)]
        public string? BooleanField1Name { get; set; }
        [MaxLength(200)]
        public string? BooleanField1Description { get; set; }
        public bool BooleanField1ShowInTable { get; set; }

        [MaxLength(100)]
        public string? BooleanField2Name { get; set; }
        [MaxLength(200)]
        public string? BooleanField2Description { get; set; }
        public bool BooleanField2ShowInTable { get; set; }

        [MaxLength(100)]
        public string? BooleanField3Name { get; set; }
        [MaxLength(200)]
        public string? BooleanField3Description { get; set; }
        public bool BooleanField3ShowInTable { get; set; }

        // Navigation properties
        public virtual User Owner { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
        public virtual ICollection<InventoryAccess> UserAccesses { get; set; } = new List<InventoryAccess>();
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
