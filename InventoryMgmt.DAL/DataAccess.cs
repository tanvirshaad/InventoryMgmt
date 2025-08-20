using InventoryMgmt.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL
{
    public class DataAccess
    {
        private readonly ApplicationDbContext db;

        public DataAccess(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }
    }
}
