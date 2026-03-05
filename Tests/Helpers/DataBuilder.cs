using Application.DTOs;
using Domain.Entities;

namespace Tests.Helpers
{
    public class DataBuilder
    {
        public static Producto BuildProducto(
            int id = 1,
            string nombre = "Audifonos JBL",
            decimal precio = 99.99m)
            => new() { Id = id, Nombre = nombre, Precio = precio };

        public static List<Producto> BuildProductos(int count = 3)
            => Enumerable.Range(1, count)
                .Select(i => BuildProducto(id: i, nombre: $"Producto {i}", precio: i * 50m))
                .ToList();

        public static Orden BuildOrden(
            int id = 1,
            string cliente = "Nicolas Caceres Test",
            decimal total = 123.25m,
            List<OrdenProducto>? ordenProductos = null)
            => new()
            {
                Id = id,
                Cliente = cliente,
                FechaCreacion = DateTime.UtcNow,
                Total = total,
                OrdenProductos = ordenProductos ?? new List<OrdenProducto>()
            };

        public static Orden BuildOrdenConProductos(int cantidadProductos = 2)
        {
            var productos = BuildProductos(cantidadProductos);
            var ordenProductos = productos
                .Select(p => new OrdenProducto
                {
                    ProductoId = p.Id,
                    Producto = p
                })
                .ToList();

            return new Orden
            {
                Id = 1,
                Cliente = "Nicolas Caceres Test",
                FechaCreacion = DateTime.UtcNow,
                Total = productos.Sum(p => p.Precio),
                OrdenProductos = ordenProductos
            };
        }

        public static List<Orden> BuildOrdenes(int count = 3)
            => Enumerable.Range(1, count)
                .Select(i => BuildOrden(id: i, cliente: $"Cliente {i}", total: i * 100m))
                .ToList();

        public static CreateProductoDto BuildCreateProductoDto(
            string nombre = "Producto Nuevo",
            decimal precio = 99.99m)
            => new() { Nombre = nombre, Precio = precio };

        public static CreateOrdenDto BuildCreateOrdenDto(
            string cliente = "Nicolas Caceres",
            List<int>? productoIds = null)
            => new()
            {
                Cliente = cliente,
                ProductoIds = productoIds ?? new List<int> { 1, 2 }
            };

        public static UpdateOrdenDto BuildUpdateOrdenDto(
            string cliente = "Cliente Actualizado",
            decimal total = 300m)
            => new() { Cliente = cliente, Total = total };

        public static PaginationDto BuildPaginationDto(int page = 1, int size = 10)
            => new() { Page = page, Size = size };
    }
}
