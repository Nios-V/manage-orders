using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Orden> Ordenes => Set<Orden>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<OrdenProducto> OrdenProductos => Set<OrdenProducto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrdenProducto>()
                .HasKey(op => new { op.OrdenId, op.ProductoId });

            modelBuilder.Entity<OrdenProducto>()
                .HasOne(op => op.Orden)
                .WithMany(o => o.OrdenProductos)
                .HasForeignKey(op => op.OrdenId);

            modelBuilder.Entity<OrdenProducto>()
                .HasOne(op => op.Producto)
                .WithMany()
                .HasForeignKey(op => op.ProductoId);

            modelBuilder.Entity<Orden>().Property(o => o.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);

            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Laptop Pro", Precio = 1500.00m },
                new Producto { Id = 2, Nombre = "Mouse Gamer", Precio = 25.50m },
                new Producto { Id = 3, Nombre = "Monitor OLED 4K", Precio = 400.00m },
                new Producto { Id = 4, Nombre = "Teclado Mecánico", Precio = 150.00m },
                new Producto { Id = 5, Nombre = "Silla Ergonómica", Precio = 500.00m }
            );
        }
    }
}