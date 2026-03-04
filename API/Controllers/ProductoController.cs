using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("productos")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> Get()
        {
            var productos = await _productoService.GetAllProductsAsync();
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetById(int id)
        {
            var producto = await _productoService.GetProductByIdAsync(id);
            if (producto == null)
                return NotFound();
            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> Create([FromBody] CreateProductoDto dto)
        {
            var producto = await _productoService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
        }
    }
}