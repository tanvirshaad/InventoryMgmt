using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class Like
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Item Item { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
