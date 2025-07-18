namespace Application.DTOs
{
    public class ProfesorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<string> Materias { get; set; } = new();
    }
} 