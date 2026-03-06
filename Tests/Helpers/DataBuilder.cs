using Application.DTOs;
using Domain.Entities;

namespace Tests.Helpers
{
    public class DataBuilder
    {
        public static Cliente BuildCliente(
            int id = 1,
            string nombre = "Nicolas Caceres",
            string email = "nico@example.com",
            string rol = Roles.Cliente)
        {
            return new Cliente
            {
                Id = id,
                Nombre = nombre,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Rol = rol
            };
        }

        public static Cliente BuildAdmin(int id = 99)
            => BuildCliente(id: id, nombre: "Admin", email: "admin@example.com", rol: Roles.Admin);

        public static Producto BuildProducto(
            int id = 1,
            string nombre = "Audifonos JBL",
            decimal precio = 99.99m)
            => new() { Id = id, Nombre = nombre, Precio = precio };

        public static List<Producto> BuildProductos(int count = 3)
            => Enumerable.Range(1, count)
                .Select(i => BuildProducto(id: i, nombre: $"Producto {i}", precio: i * 50m))
                .ToList();

        public static Orden BuildOrden(int id = 1, int clienteId = 1, decimal total = 200m)
        {
            return new Orden
            {
                Id = id,
                ClienteId = clienteId,
                Cliente = BuildCliente(id: clienteId),
                FechaCreacion = DateTime.UtcNow,
                Total = total,
                OrdenProductos = new List<OrdenProducto>()
            };
        }

        public static Orden BuildOrdenConProductos(int cantidadProductos = 2)
        {
            var productos = BuildProductos(cantidadProductos);
            var ordenProductos = productos.Select(p => new OrdenProducto
            {
                ProductoId = p.Id,
                Producto = p
            }).ToList();

            return new Orden
            {
                Id = 1,
                ClienteId = 1,
                Cliente = BuildCliente(),
                FechaCreacion = DateTime.UtcNow,
                Total = productos.Sum(p => p.Precio),
                OrdenProductos = ordenProductos
            };
        }

        public static CreateOrdenDto BuildCreateOrdenDto(List<int>? productoIds = null)
            => new() { ProductoIds = productoIds ?? new List<int> { 1, 2 } };

        public static List<Orden> BuildOrdenes(int cantidad = 3)
        {
            var lista = new List<Orden>();
            for (int i = 1; i <= cantidad; i++)
                lista.Add(BuildOrden(id: i, clienteId: i, total: i * 100m));
            return lista;
        }

        public static CreateProductoDto BuildCreateProductoDto(
            string nombre = "Producto Nuevo",
            decimal precio = 99.99m)
            => new() { Nombre = nombre, Precio = precio };

        public static UpdateOrdenDto BuildUpdateOrdenDto(List<int>? productoIds = null)
            => new() { ProductoIds = productoIds ?? new List<int> { 1 } };

        public static PaginationDto BuildPaginationDto(int page = 1, int size = 10)
            => new() { Page = page, Size = size };
    }
}
