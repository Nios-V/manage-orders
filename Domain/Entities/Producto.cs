namespace Domain.Entities
{
    public class Producto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Precio { get; set; }
    }
}
