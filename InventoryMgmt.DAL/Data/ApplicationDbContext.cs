using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryMgmt.DAL.EF.TableModels;

namespace InventoryMgmt.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryTag> InventoryTags { get; set; }
        public DbSet<InventoryAccess> InventoryAccesses { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemLike> ItemLikes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
            });

            // Configure composite keys
            builder.Entity<InventoryTag>()
                .HasKey(it => new { it.InventoryId, it.TagId });

            builder.Entity<InventoryAccess>()
                .HasKey(iua => new { iua.InventoryId, iua.UserId });

            builder.Entity<ItemLike>()
                .HasKey(il => new { il.ItemId, il.UserId });

            // Configure relationships
            builder.Entity<Inventory>()
                .HasOne(i => i.Owner)
                .WithMany(u => u.OwnedInventories)
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inventory>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Inventories)
                .HasForeignKey(i => i.CategoryId);

            builder.Entity<InventoryTag>()
                .HasOne(it => it.Inventory)
                .WithMany(i => i.InventoryTags)
                .HasForeignKey(it => it.InventoryId);

            builder.Entity<InventoryTag>()
                .HasOne(it => it.Tag)
                .WithMany(t => t.InventoryTags)
                .HasForeignKey(it => it.TagId);

            builder.Entity<InventoryAccess>()
                .HasOne(iua => iua.Inventory)
                .WithMany(i => i.UserAccesses)
                .HasForeignKey(iua => iua.InventoryId);

            builder.Entity<InventoryAccess>()
                .HasOne(iua => iua.User)
                .WithMany(u => u.InventoryAccesses)
                .HasForeignKey(iua => iua.UserId);

            builder.Entity<Item>()
                .HasOne(i => i.Inventory)
                .WithMany(inv => inv.Items)
                .HasForeignKey(i => i.InventoryId);

            builder.Entity<Item>()
                .HasOne(i => i.CreatedBy)
                .WithMany(u => u.CreatedItems)
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ItemLike>()
                .HasOne(il => il.Item)
                .WithMany(i => i.Likes)
                .HasForeignKey(il => il.ItemId);

            builder.Entity<ItemLike>()
                .HasOne(il => il.User)
                .WithMany(u => u.ItemLikes)
                .HasForeignKey(il => il.UserId);

            builder.Entity<Comment>()
                .HasOne(c => c.Inventory)
                .WithMany(i => i.Comments)
                .HasForeignKey(c => c.InventoryId);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            builder.Entity<Item>()
                .HasIndex(i => new { i.InventoryId, i.CustomId })
                .IsUnique()
                .HasDatabaseName("IX_Item_Inventory_CustomId");

            builder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // Seed initial data
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Equipment", Description = "Office equipment and devices" },
                new Category { Id = 2, Name = "Furniture", Description = "Office furniture and fixtures" },
                new Category { Id = 3, Name = "Books", Description = "Books and publications" },
                new Category { Id = 4, Name = "Documents", Description = "Important documents and records" },
                new Category { Id = 5, Name = "Other", Description = "Other miscellaneous items" }
            );
        }
    }
}
