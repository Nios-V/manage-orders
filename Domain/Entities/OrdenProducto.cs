namespace Domain.Entities
{
    public class OrdenProducto
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public Orden Orden { get; set; } = null!;
        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;

    }
}
