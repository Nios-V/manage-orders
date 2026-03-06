using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IJWTService _jwtService;

        public AuthService(IClienteRepository clienteRepository, IJWTService jwtService)
        {
            _clienteRepository = clienteRepository;
            _jwtService = jwtService;
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
        {
            var cliente = await _clienteRepository.GetByEmailAsync(loginDto.Email);
            if (cliente is null)
                throw new UnauthorizedAccessException("Credenciales inválidas");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, cliente.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas");

            var token = _jwtService.GenerateToken(cliente);

            return new TokenResponseDto
            {
                Token = token,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Rol = cliente.Rol,
                Expiration = DateTime.UtcNow.AddHours(8)
            };
        }

        public async Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var exists = await _clienteRepository.ExistsAsync(registerDto.Email);
            if (exists)
                throw new ArgumentException("Email ya registrado");

            var cliente = new Cliente
            {
                Nombre = registerDto.Nombre,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Rol = Roles.Cliente
            };
            await _clienteRepository.AddAsync(cliente);

            var token = _jwtService.GenerateToken(cliente);
            return new TokenResponseDto
            {
                Token = token,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Rol = cliente.Rol,
                Expiration = DateTime.UtcNow.AddHours(8)
            };

        }
    }
}
