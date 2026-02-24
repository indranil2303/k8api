using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.persistence;

public class LocaldbContext : DbContext
{
    // Constructor accepts DbContextOptions for DI configuration
    public LocaldbContext(DbContextOptions<LocaldbContext> options)
        : base(options)
    {
    }
    public DbSet<Product> Products { get; set; } = default!;

    // Optional: Override OnModelCreating for advanced model configuration or seeding
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    {
        base.OnModelCreating(modelBuilder);
        // Example: Seed initial data
        modelBuilder.Entity<Product>()
        .HasData(new Product { Id = Guid.NewGuid(), Name = "Prod I - A", Price = 99.00m },
            new Product { Id = Guid.NewGuid(), Name = "Prod I - B", Price = 149.99m },
            new Product { Id = Guid.NewGuid(), Name = "Prod II", Price = 49.99m },
            new Product { Id = Guid.NewGuid(), Name = "Prod III", Price = 79.00m },
            new Product { Id = Guid.NewGuid(), Name = "Prod III - B", Price = 299.00m },
            new Product { Id = Guid.NewGuid(), Name = "Prod IV - A", Price = 199.99m },
            new Product { Id = Guid.NewGuid(), Name = "Prod V", Price = 89.99m },
            new Product { Id = Guid.NewGuid(), Name = "Prod VI", Price = 199.00m });

         modelBuilder.Entity<Product>().HasKey(x => x.Id);
         modelBuilder.Entity<Product>().Property(x => x.Name).IsRequired().HasMaxLength(80);
         modelBuilder.Entity<Product>().Property(x => x.Price).HasColumnType("decimal(15,2)");
    }
}