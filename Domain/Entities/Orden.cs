namespace Domain.Entities
{
    public class Orden
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }
        public List<OrdenProducto> OrdenProductos { get; set; } = new();
    }
}
