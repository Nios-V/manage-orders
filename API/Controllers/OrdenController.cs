using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("ordenes")]
    public class OrdenController : ControllerBase
    {
        private readonly IOrdenService _ordenService;

        public OrdenController(IOrdenService ordenService)
        {
            _ordenService = ordenService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDto<OrdenDto>>> Get([FromQuery] PaginationDto paginacion)
        {
            var resultado = await _ordenService.GetAllOrdersAsync(paginacion);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedOrdenDto>> GetById(int id)
        {
            var orden = await _ordenService.GetOrderByIdAsync(id);
            if (orden == null)
                return NotFound();
            return Ok(orden);
        }

        [HttpPost]
        public async Task<ActionResult<OrdenDto>> Create([FromBody] CreateOrdenDto dto)
        {
            var orden = await _ordenService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateOrdenDto dto)
        {
            var orden = await _ordenService.UpdateOrderAsync(id, dto);
            if (orden == null)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _ordenService.DeleteOrderAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}