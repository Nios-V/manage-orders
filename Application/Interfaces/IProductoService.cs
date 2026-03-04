using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductoService
    {
        Task<PaginatedDto<ProductoDto>> GetAllProductsAsync(PaginationDto paginationDto);
        Task<ProductoDto?> GetProductByIdAsync(int id);
        Task<ProductoDto> CreateProductAsync(CreateProductoDto createProductoDto);
    }
}
