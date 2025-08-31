using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public enum InventoryAccessPermission
    {
        None = 0,
        Read = 1,
        Write = 2,
        Manage = 3,
        FullControl = 4
    }
}
