namespace Application.DTOs
{
    public class EstudianteDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public List<string> MateriasInscritas { get; set; } = new();
    }
}
