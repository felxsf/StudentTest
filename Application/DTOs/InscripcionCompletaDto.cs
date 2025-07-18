namespace Application.DTOs
{
    public class InscripcionCompletaDto
    {
        public int Id { get; set; }
        public Guid EstudianteId { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public string EstudianteCorreo { get; set; } = string.Empty;
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public int MateriaCreditos { get; set; }
        public int ProfesorId { get; set; }
        public string ProfesorNombre { get; set; } = string.Empty;
    }
} 