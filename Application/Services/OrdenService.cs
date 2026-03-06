using Application.Cache;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;

namespace Application.Services
{
    public class OrdenService : IOrdenService
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly ICacheService _cache;

        public OrdenService(IOrdenRepository ordenRepository, IProductoRepository productoRepository, ICacheService cache)
        {
            _ordenRepository = ordenRepository;
            _productoRepository = productoRepository;
            _cache = cache;
        }

        public async Task<OrdenDto> CreateOrderAsync(CreateOrdenDto createOrdenDto, int clienteId)
        {
            var orden = new Orden
            {
                ClienteId = clienteId,
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

            orden.Total = DescuentoService.AplicarDescuento(subtotal, orden.OrdenProductos.Count);
            await _ordenRepository.AddAsync(orden);

            return new OrdenDto
            {
                Id = orden.Id,
                ClienteId = clienteId,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total
            };
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var exists = await _ordenRepository.GetByIdAsync(id);
            if (exists == null) return false;
            await _ordenRepository.DeleteAsync(id);

            await _cache.RemoveAsync(CacheKeys.Orden(id));

            return true;
        }

        public async Task<PaginatedDto<OrdenDto>> GetAllOrdersAsync(PaginationDto pagination)
        {
            var (items, total) = await _ordenRepository.GetPagedAsync(pagination.Page, pagination.Size);

            return new PaginatedDto<OrdenDto>
            {
                Data = items.Select(o => new OrdenDto
                {
                    Id = o.Id,
                    ClienteId = o.ClienteId,
                    FechaCreacion = o.FechaCreacion,
                    Total = o.Total
                }),
                ActualPage = pagination.Page,
                PageSize = pagination.Size,
                Total = total
            };
        }

        public async Task<DetailedOrdenDto?> GetOrderByIdAsync(int id)
        {
            var cacheKey = CacheKeys.Orden(id);

            var cached = await _cache.GetAsync<DetailedOrdenDto>(cacheKey);
            if (cached is not null) return cached;

            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null) return null;

            var dto = new DetailedOrdenDto
            {
                Id = orden.Id,
                ClienteId = orden.ClienteId,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total,
                Productos = orden.OrdenProductos.Select(op => new ProductoDto
                {
                    Id = op.ProductoId,
                    Nombre = op.Producto.Nombre,
                    Precio = op.Producto.Precio
                }).ToList()
            };

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
            return dto;
        }

        public async Task<OrdenDto?> UpdateOrderAsync(int id, UpdateOrdenDto updateOrdenDto)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden is null) return null;

            orden.OrdenProductos.Clear();
            decimal subtotal = 0;

            foreach (var productoId in updateOrdenDto.ProductoIds)
            {
                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto != null)
                {
                    orden.OrdenProductos.Add(new OrdenProducto { ProductoId = producto.Id });
                    subtotal += producto.Precio;
                }
            }

            orden.Total = DescuentoService.AplicarDescuento(subtotal, orden.OrdenProductos.Count);
            await _ordenRepository.UpdateAsync(orden);

            await _cache.RemoveAsync(CacheKeys.Orden(id));

            return new OrdenDto
            {
                Id = orden.Id,
                ClienteId = orden.ClienteId,
                FechaCreacion = orden.FechaCreacion,
                Total = orden.Total
            };
        }
    }
}
