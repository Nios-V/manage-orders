using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Tests.Helpers;

namespace Tests.Services
{
    public class OrdenServiceTests
    {
        private readonly Mock<IOrdenRepository> _ordenRepoMock;
        private readonly Mock<IProductoRepository> _productoRepoMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly OrdenService _orderService;

        public OrdenServiceTests()
        {
            _ordenRepoMock = new Mock<IOrdenRepository>();
            _productoRepoMock = new Mock<IProductoRepository>();
            _cacheMock = new Mock<ICacheService>();
            _orderService = new OrdenService(_ordenRepoMock.Object, _productoRepoMock.Object, _cacheMock.Object);
        }

        // GetAllOrdersAsync

        [Fact]
        public async Task GetAllOrdersAsync_WithData_ReturnsPaginatedResult()
        {
            var ordenes = DataBuilder.BuildOrdenes(3);
            var paginacion = DataBuilder.BuildPaginationDto(page: 1, size: 10);

            _ordenRepoMock
                .Setup((r) => r.GetPagedAsync(1, 10))
                .ReturnsAsync((ordenes, 3));

            var resultado = await _orderService.GetAllOrdersAsync(paginacion);

            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Total);
            Assert.Equal(1, resultado.ActualPage);
            Assert.Equal(10, resultado.PageSize);
            Assert.Equal(3, resultado.Data.Count());
        }

        [Fact]
        public async Task GetAllOrdersAsync_WithNoData_ReturnsEmptyPaginated()
        {
            var paginacion = DataBuilder.BuildPaginationDto();
            _ordenRepoMock
                .Setup((r) => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((new List<Orden>(), 0));

            var resultado = await _orderService.GetAllOrdersAsync(paginacion);

            Assert.Equal(0, resultado.Total);
            Assert.Empty(resultado.Data);
        }

        [Fact]
        public async Task GetAllOrdersAsync_OrderMapping_IsCorrect()
        {
            var orden = DataBuilder.BuildOrden(id: 1, cliente: "Nicolas Caceres", total: 450m);
            var paginacion = DataBuilder.BuildPaginationDto();

            _ordenRepoMock
                .Setup((r) => r.GetPagedAsync(1, 10))
                .ReturnsAsync((new List<Orden> { orden }, 1));

            var resultado = await _orderService.GetAllOrdersAsync(paginacion);
            var dto = resultado.Data.First();

            Assert.Equal(1, dto.Id);
            Assert.Equal("Nicolas Caceres", dto.Cliente);
            Assert.Equal(450m, dto.Total);
        }

        // GetOrderByIdAsync

        [Fact]
        public async Task GetOrderByIdAsync_WhenExists_ReturnsDetailedDto()
        {
            var orden = DataBuilder.BuildOrdenConProductos(cantidadProductos: 2);
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);

            var resultado = await _orderService.GetOrderByIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Equal(orden.Id, resultado.Id);
            Assert.Equal(orden.Cliente, resultado.Cliente);
            Assert.Equal(2, resultado.Productos.Count);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenNotFound_ReturnsNull()
        {
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(99)).ReturnsAsync((Orden?)null);

            var resultado = await _orderService.GetOrderByIdAsync(99);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ProductMapping_IsCorrect()
        {
            var producto = DataBuilder.BuildProducto(id: 5, nombre: "Monitor OLED", precio: 400m);
            var orden = new Orden
            {
                Id = 1,
                Cliente = "Andres Test",
                Total = 400m,
                FechaCreacion = DateTime.UtcNow,
                OrdenProductos = new List<OrdenProducto>
                {
                    new() { ProductoId = 5, Producto = producto }
                }
            };
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);

            var resultado = await _orderService.GetOrderByIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Single(resultado.Productos);
            Assert.Equal(5, resultado.Productos[0].Id);
            Assert.Equal("Monitor OLED", resultado.Productos[0].Nombre);
            Assert.Equal(400m, resultado.Productos[0].Precio);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenCacheHit_DoesNotCallRepository()
        {
            var cachedOrden = new DetailedOrdenDto { Id = 1, Cliente = "Cacheado" };
            _cacheMock
                .Setup((c) => c.GetAsync<DetailedOrdenDto>("orden:1"))
                .ReturnsAsync(cachedOrden);

            var resultado = await _orderService.GetOrderByIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Equal("Cacheado", resultado.Cliente);
            _ordenRepoMock.Verify((r) => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenCacheMiss_SavesResultInCache()
        {
            _cacheMock
                .Setup((c) => c.GetAsync<DetailedOrdenDto>("orden:1"))
                .ReturnsAsync((DetailedOrdenDto?)null);

            var orden = DataBuilder.BuildOrdenConProductos();
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);
            _cacheMock.Setup((c) => c.SetAsync(It.IsAny<string>(), It.IsAny<DetailedOrdenDto>(), It.IsAny<TimeSpan?>()))
                .Returns(Task.CompletedTask);

            await _orderService.GetOrderByIdAsync(1);

            _cacheMock.Verify((c) => c.SetAsync(
                "orden:1",
                It.IsAny<DetailedOrdenDto>(),
                It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenNotFound_DoesNotSaveInCache()
        {
            _cacheMock
                .Setup((c) => c.GetAsync<DetailedOrdenDto>(It.IsAny<string>()))
                .ReturnsAsync((DetailedOrdenDto?)null);
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(9999)).ReturnsAsync((Orden?)null);

            var resultado = await _orderService.GetOrderByIdAsync(9999);

            Assert.Null(resultado);
            _cacheMock.Verify((c) => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<DetailedOrdenDto>(),
                It.IsAny<TimeSpan?>()), Times.Never);
        }

        // CreateOrderAsync

        [Fact]
        public async Task CreateOrderAsync_WithValidProducts_CreatesOrderAndReturnsDto()
        {
            var dto = DataBuilder.BuildCreateOrdenDto(cliente: "Andres", productoIds: new List<int> { 1, 2 });
            var producto1 = DataBuilder.BuildProducto(id: 1, precio: 100m);
            var producto2 = DataBuilder.BuildProducto(id: 2, precio: 150m);

            _productoRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(producto1);
            _productoRepoMock.Setup((r) => r.GetByIdAsync(2)).ReturnsAsync(producto2);
            _ordenRepoMock.Setup((r) => r.AddAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);

            var resultado = await _orderService.CreateOrderAsync(dto);

            Assert.NotNull(resultado);
            Assert.Equal("Andres", resultado.Cliente);
            _ordenRepoMock.Verify((r) => r.AddAsync(It.IsAny<Orden>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenTotalExceedsThreshold_Applies10PercentDiscount()
        {
            // 2 productos de 300 cada uno = 600 total → 10% desc → 540
            var dto = DataBuilder.BuildCreateOrdenDto(productoIds: new List<int> { 1, 2 });
            _productoRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(DataBuilder.BuildProducto(id: 1, precio: 300m));
            _productoRepoMock.Setup((r) => r.GetByIdAsync(2)).ReturnsAsync(DataBuilder.BuildProducto(id: 2, precio: 300m));
            _ordenRepoMock.Setup((r) => r.AddAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);

            var resultado = await _orderService.CreateOrderAsync(dto);

            Assert.Equal(540m, resultado.Total);
        }

        [Fact]
        public async Task CreateOrderAsync_With6ProductsAndHighTotal_Applies15PercentDiscount()
        {
            var ids = new List<int> { 1, 2, 3, 4, 5, 6 };
            var dto = DataBuilder.BuildCreateOrdenDto(productoIds: ids);

            foreach (var id in ids)
            {
                _productoRepoMock
                    .Setup((r) => r.GetByIdAsync(id))
                    .ReturnsAsync(DataBuilder.BuildProducto(id: id, precio: 100m));
            }
            _ordenRepoMock.Setup((r) => r.AddAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);

            var resultado = await _orderService.CreateOrderAsync(dto);

            Assert.Equal(510m, resultado.Total);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenProductNotFound_IsIgnored()
        {
            var dto = DataBuilder.BuildCreateOrdenDto(productoIds: new List<int> { 1, 99 });
            _productoRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(DataBuilder.BuildProducto(id: 1, precio: 100m));
            _productoRepoMock.Setup((r) => r.GetByIdAsync(99)).ReturnsAsync((Producto?)null);
            _ordenRepoMock.Setup((r) => r.AddAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);

            var resultado = await _orderService.CreateOrderAsync(dto);

            Assert.Equal(100m, resultado.Total);
            _ordenRepoMock.Verify((r) => r.AddAsync(It.Is<Orden>(o => o.OrdenProductos.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WithEmptyList_CreatesOrderWithZeroTotal()
        {
            var dto = DataBuilder.BuildCreateOrdenDto(productoIds: new List<int>());
            _ordenRepoMock.Setup((r) => r.AddAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);

            var resultado = await _orderService.CreateOrderAsync(dto);

            Assert.Equal(0m, resultado.Total);
        }

        // UpdateOrderAsync

        [Fact]
        public async Task UpdateOrderAsync_WhenExists_UpdatesAndReturnsDto()
        {
            var orden = DataBuilder.BuildOrden(id: 1, cliente: "Nicolas", total: 100m);
            var updateDto = DataBuilder.BuildUpdateOrdenDto(cliente: "Andres", total: 500m);

            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);
            _ordenRepoMock.Setup((r) => r.UpdateAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);

            var resultado = await _orderService.UpdateOrderAsync(1, updateDto);

            Assert.NotNull(resultado);
            Assert.Equal("Andres", resultado.Cliente);
            Assert.Equal(500m, resultado.Total);
            _ordenRepoMock.Verify((r) => r.UpdateAsync(It.IsAny<Orden>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderAsync_WhenNotFound_ReturnsNull()
        {
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(99)).ReturnsAsync((Orden?)null);

            var resultado = await _orderService.UpdateOrderAsync(99, DataBuilder.BuildUpdateOrdenDto());

            Assert.Null(resultado);
            _ordenRepoMock.Verify((r) => r.UpdateAsync(It.IsAny<Orden>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderAsync_DoesNotModifyCreationDate()
        {
            var fechaOriginal = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            var orden = DataBuilder.BuildOrden();
            orden.FechaCreacion = fechaOriginal;

            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);
            _ordenRepoMock.Setup((r) => r.UpdateAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);

            var resultado = await _orderService.UpdateOrderAsync(1, DataBuilder.BuildUpdateOrdenDto());

            Assert.Equal(fechaOriginal, resultado!.FechaCreacion);
        }

        [Fact]
        public async Task UpdateOrderAsync_WhenExists_InvalidatesCache()
        {
            var orden = DataBuilder.BuildOrden();
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);
            _ordenRepoMock.Setup((r) => r.UpdateAsync(It.IsAny<Orden>())).Returns(Task.CompletedTask);
            _cacheMock.Setup((c) => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            await _orderService.UpdateOrderAsync(1, DataBuilder.BuildUpdateOrdenDto());

            _cacheMock.Verify((c) => c.RemoveAsync("orden:1"), Times.Once);
        }

        // DeleteOrderAsync

        [Fact]
        public async Task DeleteOrderAsync_WhenExists_DeletesAndReturnsTrue()
        {
            var orden = DataBuilder.BuildOrden(id: 1);
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);
            _ordenRepoMock.Setup((r) => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var resultado = await _orderService.DeleteOrderAsync(1);

            Assert.True(resultado);
            _ordenRepoMock.Verify((r) => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenNotFound_ReturnsFalse()
        {
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(99)).ReturnsAsync((Orden?)null);

            var resultado = await _orderService.DeleteOrderAsync(99);

            Assert.False(resultado);
            _ordenRepoMock.Verify((r) => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenExists_InvalidatesCache()
        {
            var orden = DataBuilder.BuildOrden();
            _ordenRepoMock.Setup((r) => r.GetByIdAsync(1)).ReturnsAsync(orden);
            _ordenRepoMock.Setup((r) => r.DeleteAsync(1)).Returns(Task.CompletedTask);
            _cacheMock.Setup((c) => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            await _orderService.DeleteOrderAsync(1);

            _cacheMock.Verify((c) => c.RemoveAsync("orden:1"), Times.Once);
        }
    }
}
