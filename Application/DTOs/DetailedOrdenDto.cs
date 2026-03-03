namespace Application.DTOs
{
    public class DetailedOrdenDto : OrdenDto
    {
        public List<ProductoDto> Productos { get; set; } = new();
    }
}
