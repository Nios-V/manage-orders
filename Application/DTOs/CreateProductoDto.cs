using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateProductoDto
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio del producto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
    }
}
