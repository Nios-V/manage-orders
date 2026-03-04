using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateOrdenDto
    {
        [Required]
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
