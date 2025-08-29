using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class InventoryTag
    {
        public int InventoryId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
