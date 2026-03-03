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

        public async Task<IEnumerable<ProductoDto>> GetAllProductsAsync()
        {
            var productos = await _repository.GetAllAsync();
            return productos.Select((p) => new ProductoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio
            });
        }

        public async Task<ProductoDto> GetProductByIdAsync(int id)
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
