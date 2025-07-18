namespace Application.DTOs
{
    public class MateriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public int ProfesorId { get; set; }
        public string ProfesorNombre { get; set; } = string.Empty;
    }
} 