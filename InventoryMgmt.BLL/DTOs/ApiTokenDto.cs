using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.DTOs
{
    public class ApiTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public int InventoryId { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
    }
}