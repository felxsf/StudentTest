namespace Domain.Entities
{
    public class Estudiante
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public string Rol { get; set; } = "Estudiante"; // puede ser "Estudiante", "Admin", etc.
    }
}
