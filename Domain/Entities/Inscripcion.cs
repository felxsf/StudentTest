namespace Domain.Entities
{
    public class Inscripcion
    {
        public int Id { get; set; }
        public Guid EstudianteId { get; set; }
        public Estudiante Estudiante { get; set; } = null!;
        public int MateriaId { get; set; }
        public Materia Materia { get; set; } = null!;
    }
}
