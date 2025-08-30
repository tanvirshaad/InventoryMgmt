using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.DTOs
{
    public enum InventoryPermission
    {
        None = 0,
        Read = 1,
        Write = 2,
        Manage = 3,
        FullControl = 4
    }

    public class UserInventoryPermissions
    {
        public int UserId { get; set; }
        public int InventoryId { get; set; }
        public InventoryPermission Permission { get; set; }
        public bool IsOwner { get; set; }
        public bool HasWriteAccess { get; set; }
        public bool IsPublic { get; set; }
        public UserRole UserRole { get; set; }
    }
}
