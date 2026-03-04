using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repository;

        public ProductoService(IProductoRepository repository)
        {
            _repository = repository;
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
            var producto = await _repository.GetByIdAsync(id);
            if (producto == null)
            {
                return null;
            }

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio
            };
        }
    }
}
