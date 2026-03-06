namespace Application.DTOs
{
    public class OrdenDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }
    }
}
