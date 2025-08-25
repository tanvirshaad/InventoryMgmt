using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class Item
    {
        public int Id { get; set; }

        public int InventoryId { get; set; }

        [MaxLength(100)]
        public string CustomId { get; set; } = string.Empty;

        public int CreatedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] Version { get; set; } = Array.Empty<byte>();

        public int LikesCount { get; set; }

        // Custom field values
        [MaxLength(500)]
        public string? TextField1Value { get; set; }
        [MaxLength(500)]
        public string? TextField2Value { get; set; }
        [MaxLength(500)]
        public string? TextField3Value { get; set; }

        [MaxLength(2000)]
        public string? MultiTextField1Value { get; set; }
        [MaxLength(2000)]
        public string? MultiTextField2Value { get; set; }
        [MaxLength(2000)]
        public string? MultiTextField3Value { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? NumericField1Value { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NumericField2Value { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NumericField3Value { get; set; }

        [MaxLength(500)]
        public string? DocumentField1Value { get; set; }
        [MaxLength(500)]
        public string? DocumentField2Value { get; set; }
        [MaxLength(500)]
        public string? DocumentField3Value { get; set; }

        public bool? BooleanField1Value { get; set; }
        public bool? BooleanField2Value { get; set; }
        public bool? BooleanField3Value { get; set; }

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
    }
}
