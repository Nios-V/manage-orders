using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("productos")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoRepository _repository;

        public ProductoController(IProductoRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> Get()
        {
            var productos = await _repository.GetAllAsync();
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetById(int id)
        {
            var producto = await _repository.GetByIdAsync(id);
            if (producto == null)
                return NotFound();
            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Producto producto)
        {
            await _repository.AddAsync(producto);
            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
        }
    }
}
