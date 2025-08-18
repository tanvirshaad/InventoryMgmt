using Azure;
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

        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryTag> InventoryTags { get; set; }
        public DbSet<InventoryAccess> InventoryAccesses { get; set; }
        public DbSet<InventoryCustomIdFormat> InventoryCustomIdFormats { get; set; }
        public DbSet<InventoryFieldConfiguration> InventoryFieldConfigurations { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys and relationships
            modelBuilder.Entity<InventoryTag>()
                .HasKey(it => new { it.InventoryId, it.TagId });

            modelBuilder.Entity<InventoryTag>()
                .HasOne(it => it.Inventory)
                .WithMany(i => i.InventoryTags)
                .HasForeignKey(it => it.InventoryId);

            modelBuilder.Entity<InventoryTag>()
                .HasOne(it => it.Tag)
                .WithMany(t => t.InventoryTags)
                .HasForeignKey(it => it.TagId);

            // Configure one-to-one relationships
            modelBuilder.Entity<InventoryCustomIdFormat>()
                .HasKey(f => f.InventoryId);

            modelBuilder.Entity<InventoryCustomIdFormat>()
                .HasOne(f => f.Inventory)
                .WithOne(i => i.CustomIdFormat)
                .HasForeignKey<InventoryCustomIdFormat>(f => f.InventoryId);

            modelBuilder.Entity<InventoryFieldConfiguration>()
                .HasKey(f => f.InventoryId);

            modelBuilder.Entity<InventoryFieldConfiguration>()
                .HasOne(f => f.Inventory)
                .WithOne(i => i.FieldConfiguration)
                .HasForeignKey<InventoryFieldConfiguration>(f => f.InventoryId);

            // Configure unique constraints
            modelBuilder.Entity<Item>()
                .HasIndex(i => new { i.InventoryId, i.CustomId })
                .IsUnique();

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.ItemId, l.UserId })
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // Configure row version for optimistic locking
            modelBuilder.Entity<Inventory>()
                .Property(i => i.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Item>()
                .Property(i => i.RowVersion)
                .IsRowVersion();

            // Configure decimal precision
            modelBuilder.Entity<Item>()
                .Property(i => i.NumericField1Value)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Item>()
                .Property(i => i.NumericField2Value)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Item>()
                .Property(i => i.NumericField3Value)
                .HasPrecision(18, 4);

            // Seed default categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Equipment", Description = "Office and technical equipment" },
                new Category { Id = 2, Name = "Furniture", Description = "Office furniture and fixtures" },
                new Category { Id = 3, Name = "Books", Description = "Books and publications" },
                new Category { Id = 4, Name = "Documents", Description = "Important documents and files" },
                new Category { Id = 5, Name = "Other", Description = "Miscellaneous items" }
            );

            // Modify relationships to avoid multiple cascade paths

            // Fix for Comments table
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.Comments)
                .HasForeignKey("CreatedById1")
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict instead of Cascade

            // Fix for InventoryAccesses table
            modelBuilder.Entity<InventoryAccess>()
                .HasOne(ia => ia.User)
                .WithMany(u => u.InventoryAccesses)
                .HasForeignKey("UserId1")
                .OnDelete(DeleteBehavior.Restrict);

            // Fix for Items table
            modelBuilder.Entity<Item>()
                .HasOne(i => i.CreatedBy)
                .WithMany(u => u.CreatedItems)
                .HasForeignKey("CreatedById1")
                .OnDelete(DeleteBehavior.Restrict);

            // Fix for Likes table
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey("UserId1")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
