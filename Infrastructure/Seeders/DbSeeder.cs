using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Clientes.AnyAsync()) return;

            var clientes = new List<Cliente>
            {
                new()
                {
                    Nombre = "Administrador",
                    Email = "admin@manageorders.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
                    Rol = Roles.Admin
                },
                new()
                {
                    Nombre = "Nicolas Caceres",
                    Email = "nico@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Nico1234!"),
                    Rol = Roles.Cliente
                },
                new()
                {
                    Nombre = "Andres Parra",
                    Email = "andres@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Andres1234!"),
                    Rol = Roles.Cliente
                }
            };

            await context.Clientes.AddRangeAsync(clientes);
            await context.SaveChangesAsync();

            Console.WriteLine("Clientes registrados");
        }
    }
}