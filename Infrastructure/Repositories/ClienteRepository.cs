using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Clientes.AnyAsync((c) => c.Email == email);
        }

        public async Task<Cliente?> GetByEmailAsync(string email)
        {
            return await _context.Clientes.FirstOrDefaultAsync((c) => c.Email == email);
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }
    }
}
