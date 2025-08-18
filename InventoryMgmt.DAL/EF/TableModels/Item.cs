using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public byte[] RowVersion { get; set; } = new byte[0];

        // Foreign keys
        public int InventoryId { get; set; }
        [Required]
        public string CreatedById { get; set; } = string.Empty;

        // Custom field values
        public string? TextField1Value { get; set; }
        public string? TextField2Value { get; set; }
        public string? TextField3Value { get; set; }

        public string? MultilineTextField1Value { get; set; }
        public string? MultilineTextField2Value { get; set; }
        public string? MultilineTextField3Value { get; set; }

        public decimal? NumericField1Value { get; set; }
        public decimal? NumericField2Value { get; set; }
        public decimal? NumericField3Value { get; set; }

        public string? DocumentField1Value { get; set; }
        public string? DocumentField2Value { get; set; }
        public string? DocumentField3Value { get; set; }

        public bool? BooleanField1Value { get; set; }
        public bool? BooleanField2Value { get; set; }
        public bool? BooleanField3Value { get; set; }

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
