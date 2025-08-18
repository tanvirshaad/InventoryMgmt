using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string Email { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsBlocked { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public string PreferredTheme { get; set; } = "light";

        // Navigation properties
        public virtual ICollection<Inventory> OwnedInventories { get; set; } = new List<Inventory>();
        public virtual ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();
        public virtual ICollection<Item> CreatedItems { get; set; } = new List<Item>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
