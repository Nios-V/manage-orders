using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class OrdenService : IOrdenService
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IProductoRepository _productoRepository;

        public OrdenService(IOrdenRepository ordenRepository, IProductoRepository productoRepository)
        {
            _ordenRepository = ordenRepository;
            _productoRepository = productoRepository;
        }

        public async Task<OrdenDto> CreateOrderAsync(CreateOrdenDto createOrdenDto)
        {
            var orden = new Orden
            {
                Cliente = createOrdenDto.Cliente,
                FechaCreacion = DateTime.UtcNow,
                OrdenProductos = new List<OrdenProducto>()
            };

            decimal subtotal = 0;
            foreach (var id in createOrdenDto.ProductoIds)
            {
                var producto = await _productoRepository.GetByIdAsync(id);
                if (producto != null)
                {
                    orden.OrdenProductos.Add(new OrdenProducto { ProductoId = producto.Id });
                    subtotal += producto.Precio;
                }
            }

            orden.Total = subtotal;
            await _ordenRepository.AddAsync(orden);

            return new OrdenDto
            {
                Id = orden.Id,
                Cliente = orden.Cliente,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total
            };
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var exists = await _ordenRepository.GetByIdAsync(id);
            if (exists == null) return false;
            await _ordenRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<OrdenDto>> GetAllOrdersAsync()
        {
            var ordenes = await _ordenRepository.GetAllAsync();
            return ordenes.Select((o) => new OrdenDto
            {
                Id = o.Id,
                Cliente = o.Cliente,
                FechaCreacion = o.FechaCreacion,
                Total = o.Total
            });
        }

        public async Task<DetailedOrdenDto?> GetOrderByIdAsync(int id)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null) return null;

            return new DetailedOrdenDto
            {
                Id = orden.Id,
                Cliente = orden.Cliente,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total,
                Productos = orden.OrdenProductos.Select(op => new ProductoDto
                {
                    Id = op.ProductoId,
                    Nombre = op.Producto.Nombre,
                    Precio = op.Producto.Precio
                }).ToList()
            };
        }

        public async Task<OrdenDto?> UpdateOrderAsync(int id, UpdateOrdenDto updateOrdenDto)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null) return null;

            orden.Cliente = updateOrdenDto.Cliente;
            orden.Total = updateOrdenDto.Total;

            await _ordenRepository.UpdateAsync(orden);

            return new OrdenDto
            {
                Id = orden.Id,
                Cliente = orden.Cliente,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total
            };
        }
    }
}
