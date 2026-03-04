using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrdenService
    {
        Task<PaginatedDto<OrdenDto>> GetAllOrdersAsync(PaginationDto paginationDto);
        Task<DetailedOrdenDto?> GetOrderByIdAsync(int id);
        Task<OrdenDto> CreateOrderAsync(CreateOrdenDto createOrdenDto);
        Task<OrdenDto?> UpdateOrderAsync(int id, UpdateOrdenDto updateOrdenDto);
        Task<bool> DeleteOrderAsync(int id);
    }
}
