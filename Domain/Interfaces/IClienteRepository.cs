using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> GetByEmailAsync(string email);
        Task<Cliente?> GetByIdAsync(int id);
        Task AddAsync(Cliente cliente);
        Task<bool> ExistsAsync(string email);
    }
}
