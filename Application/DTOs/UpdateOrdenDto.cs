using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateOrdenDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "La orden debe tener al menos un producto")]
        public List<int> ProductoIds { get; set; } = new();
    }
}
