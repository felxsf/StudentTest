namespace Application.DTOs
{
    public class InscripcionDto
    {
        public Guid EstudianteId { get; set; }
        public List<int> MateriasIds { get; set; } = new();
    }
}
