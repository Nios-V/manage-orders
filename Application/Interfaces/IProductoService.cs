using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDto>> GetAllProductsAsync();
        Task<ProductoDto> GetProductByIdAsync(int id);
        Task<ProductoDto> CreateProductAsync(CreateProductoDto createProductoDto);
    }
}
