using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Application.DTOs
{
    public class CreateOrdenDto
    {
        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        public string Cliente { get; set; } = string.Empty;
        [Required]
        [MinLength(1, ErrorMessage = "La orden debe tener al menos un producto")]
        public List<int> ProductoIds { get; set; } = new();
    }
}
