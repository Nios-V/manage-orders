using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IOrdenRepository
    {
        Task<Orden?> GetByIdAsync(int id);
        Task<IEnumerable<Orden>> GetAllAsync(); //TODO: Agregar paginación
        Task AddAsync(Orden orden);
        Task UpdateAsync(Orden orden);
        Task DeleteAsync(int id);
    }
}
