using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJWTService
    {
        string GenerateToken(Cliente cliente);
    }
}
