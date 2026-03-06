using Application.DTOs;
using Application.Interfaces;
using API.Controllers;
using Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

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

        private void SetUser(int clienteId, string rol = "Cliente")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, clienteId.ToString()),
                new(ClaimTypes.Role, rol)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _ordenController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        // [GET] /

        [Fact]
        public async Task Get_ReturnsOkWithPaginatedResult()
        {
            SetUser(clienteId: 1);
            var paginacion = DataBuilder.BuildPaginationDto();
            var paginado = new PaginatedDto<OrdenDto>
            {
                Data = new List<OrdenDto> { new() { Id = 1, ClienteId = 1, ClienteNombre = "Nicolas" } },
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
            SetUser(clienteId: 1);
            var orden = new DetailedOrdenDto
            {
                Id = 1,
                ClienteId = 1,
                ClienteNombre = "Andres",
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
            SetUser(clienteId: 1);
            _serviceMock.Setup((s) => s.GetOrderByIdAsync(9999)).ReturnsAsync((DetailedOrdenDto?)null);

            var resultado = await _ordenController.GetById(9999);

            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        // [POST] /

        [Fact]
        public async Task Create_PassesClienteIdFromTokenToService()
        {
            // El clienteId viene del token JWT (id: 7), no del body
            SetUser(clienteId: 7);
            var dto = DataBuilder.BuildCreateOrdenDto();
            var ordenCreada = new OrdenDto { Id = 5, ClienteId = 7, Total = 200m };
            _serviceMock.Setup((s) => s.CreateOrderAsync(dto, 7)).ReturnsAsync(ordenCreada);

            var resultado = await _ordenController.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(5, createdResult.RouteValues!["id"]);
            _serviceMock.Verify((s) => s.CreateOrderAsync(dto, 7), Times.Once);
        }

        [Fact]
        public async Task Create_PointsToGetById_InCreatedAtAction()
        {
            SetUser(clienteId: 1);
            var dto = DataBuilder.BuildCreateOrdenDto();
            _serviceMock.Setup((s) => s.CreateOrderAsync(dto, 1))
                .ReturnsAsync(new OrdenDto { Id = 1 });

            var resultado = await _ordenController.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(nameof(OrdenController.GetById), createdResult.ActionName);
        }

        // [PUT] /{id}

        [Fact]
        public async Task Update_WhenExists_ReturnsNoContent()
        {
            SetUser(clienteId: 1, rol: "Admin");
            var updateDto = DataBuilder.BuildUpdateOrdenDto();
            _serviceMock.Setup((s) => s.UpdateOrderAsync(1, updateDto))
                .ReturnsAsync(new OrdenDto { Id = 1 });

            var resultado = await _ordenController.Update(1, updateDto);

            Assert.IsType<NoContentResult>(resultado);
        }

        [Fact]
        public async Task Update_WhenNotFound_ReturnsNotFound()
        {
            SetUser(clienteId: 1, rol: "Admin");
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
            SetUser(clienteId: 1, rol: "Admin");
            _serviceMock.Setup((s) => s.DeleteOrderAsync(1)).ReturnsAsync(true);

            var resultado = await _ordenController.Delete(1);

            Assert.IsType<NoContentResult>(resultado);
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsNotFound()
        {
            SetUser(clienteId: 1, rol: "Admin");
            _serviceMock.Setup((s) => s.DeleteOrderAsync(99)).ReturnsAsync(false);

            var resultado = await _ordenController.Delete(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Delete_CallsServiceOnce()
        {
            SetUser(clienteId: 1, rol: "Admin");
            _serviceMock.Setup((s) => s.DeleteOrderAsync(1)).ReturnsAsync(true);

            await _ordenController.Delete(1);

            _serviceMock.Verify((s) => s.DeleteOrderAsync(1), Times.Once);
        }
    }
}