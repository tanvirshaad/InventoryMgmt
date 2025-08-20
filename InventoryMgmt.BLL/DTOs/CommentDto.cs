using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryMgmt.BLL.DTOs;

namespace InventoryMgmt.BLL.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int InventoryId { get; set; }
        public string CreatedById { get; set; } = string.Empty;

        public UserDto CreatedBy { get; set; } = null!;
    }
}
