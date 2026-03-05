using Application.DTOs;
using Application.Interfaces;
using API.Controllers;
using Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Controllers
{
    public class OrdenControllerTests
    {
        private readonly Mock<IOrdenService> _serviceMock;
        private readonly OrdenController _ordenController;

        public OrdenControllerTests()
        {
            _serviceMock = new Mock<IOrdenService>();
            _ordenController = new OrdenController(_serviceMock.Object);
        }

        // [GET] /

        [Fact]
        public async Task Get_ReturnsOkWithPaginatedResult()
        {
            var paginacion = DataBuilder.BuildPaginationDto();
            var paginado = new PaginatedDto<OrdenDto>
            {
                Data = new List<OrdenDto> { new() { Id = 1, Cliente = "Nicolas" } },
                Total = 1,
                ActualPage = 1,
                PageSize = 10
            };
            _serviceMock.Setup((s) => s.GetAllOrdersAsync(paginacion)).ReturnsAsync(paginado);

            var resultado = await _ordenController.Get(paginacion);

            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal(200, okResult.StatusCode);

            var data = Assert.IsType<PaginatedDto<OrdenDto>>(okResult.Value);
            Assert.Single(data.Data);
        }

        // [GET] /{id}

        [Fact]
        public async Task GetById_WhenExists_ReturnsOkWithOrder()
        {
            var orden = new DetailedOrdenDto
            {
                Id = 1,
                Cliente = "Andres",
                Total = 200m,
                Productos = new List<ProductoDto> { new() { Id = 1, Nombre = "Audifonos" } }
            };
            _serviceMock.Setup((s) => s.GetOrderByIdAsync(1)).ReturnsAsync(orden);

            var resultado = await _ordenController.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var data = Assert.IsType<DetailedOrdenDto>(okResult.Value);
            Assert.Equal(1, data.Id);
            Assert.Single(data.Productos);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup((s) => s.GetOrderByIdAsync(9999)).ReturnsAsync((DetailedOrdenDto?)null);

            var resultado = await _ordenController.GetById(9999);

            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        // [POST] /

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedWithOrder()
        {
            var dto = DataBuilder.BuildCreateOrdenDto();
            var ordenCreada = new OrdenDto { Id = 5, Cliente = dto.Cliente, Total = 200m };
            _serviceMock.Setup((s) => s.CreateOrderAsync(dto)).ReturnsAsync(ordenCreada);

            var resultado = await _ordenController.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(5, createdResult.RouteValues!["id"]);

            var data = Assert.IsType<OrdenDto>(createdResult.Value);
            Assert.Equal(5, data.Id);
        }

        [Fact]
        public async Task Create_PointsToGetById_InCreatedAtAction()
        {
            var dto = DataBuilder.BuildCreateOrdenDto();
            _serviceMock.Setup((s) => s.CreateOrderAsync(dto))
                .ReturnsAsync(new OrdenDto { Id = 1 });

            var resultado = await _ordenController.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(nameof(OrdenController.GetById), createdResult.ActionName);
        }

        // [PUT] /{id}

        [Fact]
        public async Task Update_WhenExists_ReturnsNoContent()
        {
            var updateDto = DataBuilder.BuildUpdateOrdenDto();
            _serviceMock.Setup((s) => s.UpdateOrderAsync(1, updateDto))
                .ReturnsAsync(new OrdenDto { Id = 1 });

            var resultado = await _ordenController.Update(1, updateDto);

            Assert.IsType<NoContentResult>(resultado);
        }

        [Fact]
        public async Task Update_WhenNotFound_ReturnsNotFound()
        {
            var updateDto = DataBuilder.BuildUpdateOrdenDto();
            _serviceMock.Setup((s) => s.UpdateOrderAsync(99, updateDto))
                .ReturnsAsync((OrdenDto?)null);

            var resultado = await _ordenController.Update(99, updateDto);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // [DELETE] /{id}

        [Fact]
        public async Task Delete_WhenExists_ReturnsNoContent()
        {
            _serviceMock.Setup((s) => s.DeleteOrderAsync(1)).ReturnsAsync(true);

            var resultado = await _ordenController.Delete(1);

            Assert.IsType<NoContentResult>(resultado);
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup((s) => s.DeleteOrderAsync(99)).ReturnsAsync(false);

            var resultado = await _ordenController.Delete(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Delete_CallsServiceOnce()
        {
            _serviceMock.Setup((s) => s.DeleteOrderAsync(1)).ReturnsAsync(true);

            await _ordenController.Delete(1);

            _serviceMock.Verify((s) => s.DeleteOrderAsync(1), Times.Once);
        }
    }
}
