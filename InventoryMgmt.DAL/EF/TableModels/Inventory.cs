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

        [MaxLength(2000)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public byte[] RowVersion { get; set; } = new byte[0];

        // Foreign keys
        [Required]
        public string CreatedById { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual User CreatedBy { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
        public virtual ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual InventoryCustomIdFormat? CustomIdFormat { get; set; }
        public virtual InventoryFieldConfiguration? FieldConfiguration { get; set; }
    }
}
