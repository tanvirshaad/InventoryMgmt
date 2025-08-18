using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class InventoryCustomIdFormat
    {
        public int InventoryId { get; set; }

        [Required]
        public string FormatJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
    }
}
