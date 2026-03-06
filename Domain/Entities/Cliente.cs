namespace Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = Roles.Cliente;
        public List<Orden> Ordenes { get; set; } = new();
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Cliente = "Cliente";
    }
}