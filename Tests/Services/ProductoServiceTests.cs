using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Tests.Helpers;
using Moq;

namespace Tests.Services
{
    public class ProductoServiceTests
    {
        private readonly Mock<IProductoRepository> _repoMock;
        private readonly ProductoService _productoService;

        public ProductoServiceTests()
        {
            _repoMock = new Mock<IProductoRepository>();
            _productoService = new ProductoService(_repoMock.Object);
        }

        // GetAllProductsAsync

        [Fact]
        public async Task GetAllProductsAsync_WithData_ReturnsPaginatedResult()
        {
            var productos = DataBuilder.BuildProductos(3);
            var paginacion = DataBuilder.BuildPaginationDto();

            _repoMock.Setup((r) => r.GetPagedAsync(1, 10)).ReturnsAsync((productos, 3));

            var resultado = await _productoService.GetAllProductsAsync(paginacion);

            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Total);
            Assert.Equal(3, resultado.Data.Count());
            Assert.Equal(1, resultado.ActualPage);
        }

        [Fact]
        public async Task GetAllProductsAsync_WithNoData_ReturnsEmptyPaginated()
        {
            var paginacion = DataBuilder.BuildPaginationDto();
            _repoMock
                .Setup((r) => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((new List<Producto>(), 0));

            var resultado = await _productoService.GetAllProductsAsync(paginacion);

            Assert.Equal(0, resultado.Total);
            Assert.Empty(resultado.Data);
        }

        [Fact]
        public async Task GetAllProductsAsync_ProductMapping_IsCorrect()
        {
            var producto = DataBuilder.BuildProducto(id: 7, nombre: "RAM DDR5 16GB", precio: 150m);
            var paginacion = DataBuilder.BuildPaginationDto();

            _repoMock
                .Setup((r) => r.GetPagedAsync(1, 10))
                .ReturnsAsync((new List<Producto> { producto }, 1));

            var resultado = await _productoService.GetAllProductsAsync(paginacion);
            var dto = resultado.Data.First();

            Assert.Equal(7, dto.Id);
            Assert.Equal("RAM DDR5 16GB", dto.Nombre);
            Assert.Equal(150m, dto.Precio);
        }

        [Fact]
        public async Task GetAllProductsAsync_OnFirstPage_HasNoPrevious()
        {
            // 12 items, page 1, size 5 → tiene siguiente pero no tiene anterior
            var paginacion = new PaginationDto { Page = 1, Size = 5 };
            _repoMock
                .Setup((r) => r.GetPagedAsync(1, 5))
                .ReturnsAsync((DataBuilder.BuildProductos(5), 12));

            var resultado = await _productoService.GetAllProductsAsync(paginacion);

            Assert.True(resultado.HasNext);
            Assert.False(resultado.HasPrevious);
        }

        // GetProductByIdAsync

        [Fact]
        public async Task GetProductByIdAsync_WhenExists_ReturnsDto()
        {
            var producto = DataBuilder.BuildProducto(id: 3, nombre: "Mouse Logitech", precio: 25.50m);
            _repoMock.Setup((r) => r.GetByIdAsync(3)).ReturnsAsync(producto);

            var resultado = await _productoService.GetProductByIdAsync(3);

            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Id);
            Assert.Equal("Mouse Logitech", resultado.Nombre);
            Assert.Equal(25.50m, resultado.Precio);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenNotFound_ReturnsNull()
        {
            _repoMock.Setup((r) => r.GetByIdAsync(9999)).ReturnsAsync((Producto?)null);

            var resultado = await _productoService.GetProductByIdAsync(9999);

            Assert.Null(resultado);
        }

        // CreateProductAsync

        [Fact]
        public async Task CreateProductAsync_WithValidData_CreatesAndReturnsDto()
        {
            var dto = DataBuilder.BuildCreateProductoDto(nombre: "Laptop Pro", precio: 1500m);
            _repoMock.Setup((r) => r.AddAsync(It.IsAny<Producto>())).Returns(Task.CompletedTask);

            var resultado = await _productoService.CreateProductAsync(dto);

            Assert.NotNull(resultado);
            Assert.Equal("Laptop Pro", resultado.Nombre);
            Assert.Equal(1500m, resultado.Precio);
        }

        [Fact]
        public async Task CreateProductAsync_CallsRepositoryOnce()
        {
            var dto = DataBuilder.BuildCreateProductoDto();
            _repoMock.Setup((r) => r.AddAsync(It.IsAny<Producto>())).Returns(Task.CompletedTask);

            await _productoService.CreateProductAsync(dto);

            // Verifica que se llamó al repo con los datos correctos
            _repoMock.Verify((r) => r.AddAsync(It.Is<Producto>(p => p.Nombre == dto.Nombre && p.Precio == dto.Precio)),
                Times.Once);
        }
    }
}
