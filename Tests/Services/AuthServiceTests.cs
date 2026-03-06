using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Auth;
using Moq;

namespace ManageOrders.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IClienteRepository> _repoMock;
        private readonly Mock<IJWTService> _jwtMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _repoMock = new Mock<IClienteRepository>();
            _jwtMock = new Mock<IJWTService>();
            _authService = new AuthService(_repoMock.Object, _jwtMock.Object);

            _jwtMock.Setup(j => j.GenerateToken(It.IsAny<Cliente>()))
                .Returns("fake.jwt.token");
        }

        // LoginAsync

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            var cliente = new Cliente
            {
                Id = 1,
                Nombre = "Nicolas",
                Email = "nico@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Rol = Roles.Cliente
            };
            _repoMock.Setup(r => r.GetByEmailAsync("nico@example.com")).ReturnsAsync(cliente);

            var dto = new LoginDto { Email = "nico@example.com", Password = "Password123!" };

            var result = await _authService.LoginAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("fake.jwt.token", result.Token);
            Assert.Equal("nico@example.com", result.Email);
            Assert.Equal(Roles.Cliente, result.Rol);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorized()
        {
            var cliente = new Cliente
            {
                Email = "nico@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
            };
            _repoMock.Setup(r => r.GetByEmailAsync("nico@example.com")).ReturnsAsync(cliente);

            var dto = new LoginDto { Email = "nico@example.com", Password = "WrongPassword" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_WhenEmailNotFound_ThrowsUnauthorized()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Cliente?)null);

            var dto = new LoginDto { Email = "noexiste@example.com", Password = "123" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(dto));
        }

        // RegisterAsync

        [Fact]
        public async Task RegisterAsync_WithNewEmail_CreatesClienteAndReturnsToken()
        {
            _repoMock.Setup(r => r.ExistsAsync("nuevo@example.com")).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var dto = new RegisterDto
            {
                Nombre = "Nuevo",
                Email = "nuevo@example.com",
                Password = "Password123!"
            };

            var result = await _authService.RegisterAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("fake.jwt.token", result.Token);
            Assert.Equal(Roles.Cliente, result.Rol);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ThrowsArgumentException()
        {
            _repoMock.Setup(r => r.ExistsAsync("existente@example.com")).ReturnsAsync(true);

            var dto = new RegisterDto
            {
                Nombre = "Test",
                Email = "existente@example.com",
                Password = "Password123!"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _authService.RegisterAsync(dto));
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_NewCliente_RoleIsAlwaysCliente()
        {
            _repoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var dto = new RegisterDto { Nombre = "Test", Email = "t@t.com", Password = "Pass123!" };

            var result = await _authService.RegisterAsync(dto);

            Assert.Equal(Roles.Cliente, result.Rol);
        }

        [Fact]
        public async Task RegisterAsync_PasswordIsHashed_NotStoredAsPlainText()
        {
            Cliente? clienteGuardado = null;
            _repoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Cliente>()))
                .Callback<Cliente>(c => clienteGuardado = c)
                .Returns(Task.CompletedTask);

            var dto = new RegisterDto { Nombre = "Test", Email = "t@t.com", Password = "MiPassword!" };

            await _authService.RegisterAsync(dto);

            Assert.NotNull(clienteGuardado);
            Assert.NotEqual("MiPassword!", clienteGuardado.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("MiPassword!", clienteGuardado.PasswordHash));
        }
    }
}