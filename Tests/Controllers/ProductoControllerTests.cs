using Application.DTOs;
using Application.Interfaces;
using API.Controllers;
using Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Controllers
{
    public class ProductoControllerTests
    {
        private readonly Mock<IProductoService> _serviceMock;
        private readonly ProductoController _productoController;

        public ProductoControllerTests()
        {
            _serviceMock = new Mock<IProductoService>();
            _productoController = new ProductoController(_serviceMock.Object);
        }

        // [GET] /

        [Fact]
        public async Task Get_ReturnsOkWithPaginatedResult()
        {
            var paginacion = DataBuilder.BuildPaginationDto();
            var paginado = new PaginatedDto<ProductoDto>
            {
                Data = new List<ProductoDto>
                {
                    new() { Id = 1, Nombre = "Laptop", Precio = 1500m },
                    new() { Id = 2, Nombre = "Mouse",  Precio = 25m }
                },
                Total = 2,
                ActualPage = 1,
                PageSize = 10
            };
            _serviceMock.Setup((s) => s.GetAllProductsAsync(paginacion)).ReturnsAsync(paginado);

            var resultado = await _productoController.Get(paginacion);

            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(200, okResult.StatusCode);

            var data = Assert.IsType<PaginatedDto<ProductoDto>>(okResult.Value);
            Assert.Equal(2, data.Data.Count());
        }

        // [GET] /{id}

        [Fact]
        public async Task GetById_WhenExists_ReturnsOkWithProduct()
        {
            var producto = new ProductoDto { Id = 3, Nombre = "Monitor OLED", Precio = 400m };
            _serviceMock.Setup((s) => s.GetProductByIdAsync(3)).ReturnsAsync(producto);

            var resultado = await _productoController.GetById(3);

            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var data = Assert.IsType<ProductoDto>(okResult.Value);
            Assert.Equal(3, data.Id);
            Assert.Equal("Monitor OLED", data.Nombre);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup((s) => s.GetProductByIdAsync(99)).ReturnsAsync((ProductoDto?)null);

            var resultado = await _productoController.GetById(99);

            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        // [POST] / 

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithProduct()
        {
            var dto = DataBuilder.BuildCreateProductoDto(nombre: "Teclado", precio: 150m);
            var productoCreado = new ProductoDto { Id = 10, Nombre = "Teclado", Precio = 150m };
            _serviceMock.Setup((s) => s.CreateProductAsync(dto)).ReturnsAsync(productoCreado);

            var resultado = await _productoController.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(10, createdResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_PointsToGetById_InCreatedAtAction()
        {
            var dto = DataBuilder.BuildCreateProductoDto();
            _serviceMock.Setup((s) => s.CreateProductAsync(dto))
                .ReturnsAsync(new ProductoDto { Id = 1 });

            var resultado = await _productoController.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(nameof(ProductoController.GetById), createdResult.ActionName);
        }

        [Fact]
        public async Task Create_CallsServiceOnce()
        {
            var dto = DataBuilder.BuildCreateProductoDto();
            _serviceMock.Setup((s) => s.CreateProductAsync(dto))
                .ReturnsAsync(new ProductoDto { Id = 1 });

            await _productoController.Create(dto);

            _serviceMock.Verify((s) => s.CreateProductAsync(dto), Times.Once);
        }
    }
}
