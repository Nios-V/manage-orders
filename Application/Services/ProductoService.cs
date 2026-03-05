using Application.Cache;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repository;
        private readonly ICacheService _cache;

        public ProductoService(IProductoRepository repository, ICacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<ProductoDto> CreateProductAsync(CreateProductoDto createProductoDto)
        {
            var producto = new Producto
            {
                Nombre = createProductoDto.Nombre,
                Precio = createProductoDto.Precio
            };

            await _repository.AddAsync(producto);

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio
            };
        }

        public async Task<PaginatedDto<ProductoDto>> GetAllProductsAsync(PaginationDto pagination)
        {
            var (items, total) = await _repository.GetPagedAsync(pagination.Page, pagination.Size);
            return new PaginatedDto<ProductoDto>
            {
                Data = items.Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio
                }).ToList(),
                ActualPage = pagination.Page,
                PageSize = pagination.Size,
                Total = total
            };
        }

        public async Task<ProductoDto?> GetProductByIdAsync(int id)
        {
            var cacheKey = CacheKeys.Producto(id);

            var cached = await _cache.GetAsync<ProductoDto>(cacheKey);
            if (cached is not null) return cached;

            var producto = await _repository.GetByIdAsync(id);
            if (producto == null)
            {
                return null;
            }

            var dto = new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio
            };

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
            return dto;
        }
    }
}
