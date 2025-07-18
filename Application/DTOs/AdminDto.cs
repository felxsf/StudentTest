namespace Application.DTOs
{
    public class AdminDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CodigoAdmin { get; set; } = string.Empty; // Código secreto para crear administradores
    }
} 