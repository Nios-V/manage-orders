using Application.DTOs;

namespace Application.Interfaces
{
    public interface IOrdenService
    {
        Task<IEnumerable<OrdenDto>> GetAllOrdersAsync();
        Task<DetailedOrdenDto> GetOrderByIdAsync(int id);
        Task<OrdenDto> CreateOrderAsync(CreateOrdenDto createOrdenDto);
        Task<OrdenDto> UpdateOrderAsync(int id, UpdateOrdenDto updateOrdenDto);
        Task<bool> DeleteOrderAsync(int id);
    }
}
