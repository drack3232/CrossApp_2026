using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OrderRow> Orders => Set<OrderRow>();
    public DbSet<OrderLineRow> OrderLines => Set<OrderLineRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<OrderRow>();
        order.HasKey(o => o.Id);
        order.Property(o => o.Id).HasMaxLength(32);
        order.Property(o => o.Customer).IsRequired().HasMaxLength(200);
        order.Property(o => o.Address).IsRequired().HasMaxLength(300);
        order.Property(o => o.Comment).HasMaxLength(500);
        order.Property(o => o.IsConfirmed).IsRequired();

// ++1
order.Property(o => o.CreatedAt).IsRequired();
order.HasIndex(o => o.Customer);

        order.HasMany(o => o.Lines)
            .WithOne(l => l.Order)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderLineRow>(line =>
        {
            line.HasKey(l => l.Id);
            line.Property(l => l.OrderId).IsRequired().HasMaxLength(32);
            line.Property(l => l.ProductId).IsRequired().HasMaxLength(64);
            line.Property(l => l.Name).IsRequired().HasMaxLength(200);
            line.Property(l => l.Price).HasPrecision(18, 2).IsRequired();
            line.Property(l => l.Quantity).IsRequired();
        });
    }
}