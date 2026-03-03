using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProductoRepository
    {
        Task<IEnumerable<Producto>> GetAllAsync();
        Task AddAsync(Producto producto);
        Task<Producto?> GetByIdAsync(int id);
    }
}
