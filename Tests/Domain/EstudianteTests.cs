using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Tests.Domain;

public class EstudianteTests
{
    [Fact]
    public void Estudiante_ConDatosValidos_DebeCrearseCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nombre = "Juan Pérez";
        var correo = "juan.perez@email.com";
        var passwordHash = "hashedPassword123";

        // Act
        var estudiante = new Estudiante
        {
            Id = id,
            Nombre = nombre,
            Correo = correo,
            PasswordHash = passwordHash,
            Rol = "Estudiante"
        };

        // Assert
        estudiante.Should().NotBeNull();
        estudiante.Id.Should().Be(id);
        estudiante.Nombre.Should().Be(nombre);
        estudiante.Correo.Should().Be(correo);
        estudiante.PasswordHash.Should().Be(passwordHash);
        estudiante.Rol.Should().Be("Estudiante");
    }

    [Fact]
    public void Estudiante_ConCorreoVacio_DebeTenerCorreoVacio()
    {
        // Arrange & Act
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = "Test",
            Correo = "",
            PasswordHash = "hash",
            Rol = "Estudiante"
        };

        // Assert
        estudiante.Correo.Should().BeEmpty();
    }

    [Fact]
    public void Estudiante_ConNombreVacio_DebeTenerNombreVacio()
    {
        // Arrange & Act
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = "",
            Correo = "test@email.com",
            PasswordHash = "hash",
            Rol = "Estudiante"
        };

        // Assert
        estudiante.Nombre.Should().BeEmpty();
    }

    [Fact]
    public void Estudiante_ConRolAdmin_DebeTenerRolAdmin()
    {
        // Arrange & Act
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = "Admin User",
            Correo = "admin@email.com",
            PasswordHash = "hash",
            Rol = "Admin"
        };

        // Assert
        estudiante.Rol.Should().Be("Admin");
    }

    [Fact]
    public void Estudiante_ConPasswordHashVacio_DebeTenerPasswordHashVacio()
    {
        // Arrange & Act
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = "Test",
            Correo = "test@email.com",
            PasswordHash = "",
            Rol = "Estudiante"
        };

        // Assert
        estudiante.PasswordHash.Should().BeEmpty();
    }

    [Fact]
    public void Estudiante_ConInscripciones_DebeTenerColeccionInicializada()
    {
        // Arrange & Act
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = "Test",
            Correo = "test@email.com",
            PasswordHash = "hash",
            Rol = "Estudiante"
        };

        // Assert
        estudiante.Inscripciones.Should().NotBeNull();
        estudiante.Inscripciones.Should().BeEmpty();
    }

    [Fact]
    public void Estudiante_ConRolPorDefecto_DebeTenerRolEstudiante()
    {
        // Arrange & Act
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = "Test",
            Correo = "test@email.com",
            PasswordHash = "hash"
        };

        // Assert
        estudiante.Rol.Should().Be("Estudiante");
    }
} 