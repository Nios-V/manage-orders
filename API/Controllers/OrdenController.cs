using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("ordenes")]
    public class OrdenController : ControllerBase
    {
        private readonly IOrdenRepository _repository;

        public OrdenController(IOrdenRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Orden>>> Get()
        {
            var ordenes = await _repository.GetAllAsync(); //TODO: add pagination
            return Ok(ordenes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Orden>> GetById(int id)
        {
            var orden = await _repository.GetByIdAsync(id);
            if (orden == null)
                return NotFound();
            return Ok(orden);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Orden orden)
        {
            await _repository.AddAsync(orden);
            return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] Orden orden)
        {
            if (id != orden.Id)
                return BadRequest();
            var existingOrden = await _repository.GetByIdAsync(id);
            if (existingOrden == null)
                return NotFound();
            await _repository.UpdateAsync(orden);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existingOrden = await _repository.GetByIdAsync(id);
            if (existingOrden == null)
                return NotFound();
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}