using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProductoRepository
    {
        Task<(IEnumerable<Producto> Items, int Total)> GetPagedAsync(int page, int size);
        Task AddAsync(Producto producto);
        Task<Producto?> GetByIdAsync(int id);
    }
}
