using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrdenRepository : IOrdenRepository
    {
        private readonly AppDbContext _context;

        public OrdenRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Orden orden)
        {
            await _context.Ordenes.AddAsync(orden);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var orden = await _context.Ordenes.FindAsync(id);
            if (orden != null)
            {
                _context.Ordenes.Remove(orden);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(IEnumerable<Orden> Items, int Total)> GetPagedAsync(int page, int size)
        {
            var query = _context.Ordenes.AsQueryable();

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.FechaCreacion)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Orden?> GetByIdAsync(int id)
        {
            return await _context.Ordenes.Include((o) => o.OrdenProductos)
                .ThenInclude((op) => op.Producto)
                .FirstOrDefaultAsync((o) => o.Id == id);
        }

        public async Task UpdateAsync(Orden orden)
        {
            _context.Ordenes.Update(orden);
            await _context.SaveChangesAsync();
        }
    }
}
