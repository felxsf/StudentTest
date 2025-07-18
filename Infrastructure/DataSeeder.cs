using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(AppDbContext context)
        {
            // Verificar si ya hay datos
            var hasData = await context.Profesores.AnyAsync();
            var hasAdmin = await context.Estudiantes.AnyAsync(e => e.Correo == "admin@admin.com");

            // Crear administrador por defecto si no existe
            if (!hasAdmin)
            {
                var adminPassword = HashPassword("Contraseña2025*");
                var admin = new Estudiante
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Administrador",
                    Correo = "admin@admin.com",
                    PasswordHash = adminPassword,
                    Rol = "Admin"
                };

                await context.Estudiantes.AddAsync(admin);
                await context.SaveChangesAsync();
                Console.WriteLine("Administrador creado: admin@admin.com / Contraseña2025*");
            }

            // Si ya hay datos, solo crear el admin y salir
            if (hasData)
                return;

            // Crear 5 profesores
            var profesores = new List<Profesor>
            {
                new Profesor { Nombre = "Dr. Carlos Mendoza" },
                new Profesor { Nombre = "Dra. Ana García" },
                new Profesor { Nombre = "Ing. Roberto Silva" },
                new Profesor { Nombre = "Lic. María López" },
                new Profesor { Nombre = "MSc. Juan Pérez" }
            };

            await context.Profesores.AddRangeAsync(profesores);
            await context.SaveChangesAsync();

            // Obtener los IDs generados
            var profesor1 = profesores[0];
            var profesor2 = profesores[1];
            var profesor3 = profesores[2];
            var profesor4 = profesores[3];
            var profesor5 = profesores[4];

            // Crear 10 materias (cada profesor imparte 2 materias)
            var materias = new List<Materia>
            {
                // Profesor 1
                new Materia { Nombre = "Matemáticas I", Creditos = 3, ProfesorId = profesor1.Id },
                new Materia { Nombre = "Física I", Creditos = 3, ProfesorId = profesor1.Id },
                
                // Profesor 2
                new Materia { Nombre = "Programación I", Creditos = 3, ProfesorId = profesor2.Id },
                new Materia { Nombre = "Bases de Datos", Creditos = 3, ProfesorId = profesor2.Id },
                
                // Profesor 3
                new Materia { Nombre = "Inglés Técnico", Creditos = 3, ProfesorId = profesor3.Id },
                new Materia { Nombre = "Comunicación", Creditos = 3, ProfesorId = profesor3.Id },
                
                // Profesor 4
                new Materia { Nombre = "Estadística", Creditos = 3, ProfesorId = profesor4.Id },
                new Materia { Nombre = "Investigación", Creditos = 3, ProfesorId = profesor4.Id },
                
                // Profesor 5
                new Materia { Nombre = "Ética Profesional", Creditos = 3, ProfesorId = profesor5.Id },
                new Materia { Nombre = "Gestión de Proyectos", Creditos = 3, ProfesorId = profesor5.Id }
            };

            await context.Materias.AddRangeAsync(materias);
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
} 