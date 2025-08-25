using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsBlocked { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public string PreferredTheme { get; set; } = "light";
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
