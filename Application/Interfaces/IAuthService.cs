using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<TokenResponseDto> LoginAsync(LoginDto loginDto);
    }
}
