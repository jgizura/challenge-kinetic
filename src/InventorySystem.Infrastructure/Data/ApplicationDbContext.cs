using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Apple iPhone 14", Price = 999.99M, Stock = 50, Category = "Electronics" },
                new Product { Id = 2, Name = "Samsung Galaxy S23", Price = 849.99M, Stock = 30, Category = "Electronics" },
                new Product { Id = 3, Name = "Sony WH-1000XM5 Headphones", Price = 399.99M, Stock = 20, Category = "Audio" },
                new Product { Id = 4, Name = "Dell XPS 13 Laptop", Price = 1299.99M, Stock = 15, Category = "Computers" },
                new Product { Id = 5, Name = "Apple Watch Series 8", Price = 429.99M, Stock = 25, Category = "Wearables" }
            );

            modelBuilder.Entity<Inventory>().Property(i => i.CreationDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Product>().Property(p => p.CreationDate).HasDefaultValueSql("GETDATE()");
        }
    }
}