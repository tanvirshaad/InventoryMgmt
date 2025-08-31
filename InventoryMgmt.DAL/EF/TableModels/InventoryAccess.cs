using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class InventoryAccess
    {
        public int InventoryId { get; set; }
        public int UserId { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public InventoryAccessPermission Permission { get; set; } = InventoryAccessPermission.Write;

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
