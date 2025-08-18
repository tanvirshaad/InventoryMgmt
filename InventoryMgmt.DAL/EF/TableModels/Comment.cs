using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int InventoryId { get; set; }
        [Required]
        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
    }
}
