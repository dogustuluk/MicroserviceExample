using Microsoft.EntityFrameworkCore;
using Order.API.Models.Entities;

namespace Order.API.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Models.Entities.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }


}
