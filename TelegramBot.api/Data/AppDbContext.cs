using TelegramBot.api.Models;

namespace TelegramBot.api.Data;


using Microsoft.EntityFrameworkCore;



public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Кофе" },
            new Category { Id = 2, Name = "Десерты" },
            new Category { Id = 3, Name = "Напитки" }
        );

        
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Капучино", Price = 1200, CategoryId = 1 },
            new Product { Id = 2, Name = "Латте", Price = 1300, CategoryId = 1 },
            new Product { Id = 3, Name = "Чизкейк", Price = 1800, CategoryId = 2 },
            new Product { Id = 4, Name = "Тирамису", Price = 2100, CategoryId = 2 },
            new Product { Id = 5, Name = "Лимонад", Price = 900, CategoryId = 3 },
            new Product { Id = 6, Name = "Чай Ташкентский", Price = 1000, CategoryId = 3 }
        );
    }
}