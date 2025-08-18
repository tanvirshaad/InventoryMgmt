using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class InventoryAccess
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
