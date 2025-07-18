using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
        public DbSet<Profesor> Profesores => Set<Profesor>();
        public DbSet<Materia> Materias => Set<Materia>();
        public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
        public DbSet<Log> Logs => Set<Log>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Estudiante>().HasIndex(e => e.Correo).IsUnique();
        }
    }
}
