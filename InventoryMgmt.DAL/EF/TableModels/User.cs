using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public bool IsBlocked { get; set; }

        [MaxLength(10)]
        public string PreferredLanguage { get; set; } = "en";

        [MaxLength(10)]
        public string PreferredTheme { get; set; } = "light";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? Role { get; set; } = "User";

        public virtual ICollection<Inventory> OwnedInventories { get; set; } = new List<Inventory>();
        public virtual ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();
        public virtual ICollection<Item> CreatedItems { get; set; } = new List<Item>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<ItemLike> ItemLikes { get; set; } = new List<ItemLike>();
    }
}
