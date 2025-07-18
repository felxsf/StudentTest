using Application.Interfaces;
using Application.Services;
using Application.DTOs;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Application;

public class EstudianteServiceTests
{
    private readonly Mock<IEstudianteRepository> _mockRepository;
    private readonly EstudianteService _service;

    public EstudianteServiceTests()
    {
        _mockRepository = new Mock<IEstudianteRepository>();
        _service = new EstudianteService(_mockRepository.Object);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantesAsync_CuandoHayEstudiantes_DebeRetornarListaDeEstudiantes()
    {
        // Arrange
        var estudiantes = new List<Estudiante>
        {
            new Estudiante { Id = Guid.NewGuid(), Nombre = "Juan Pérez", Correo = "juan@email.com", Rol = "Estudiante" },
            new Estudiante { Id = Guid.NewGuid(), Nombre = "María García", Correo = "maria@email.com", Rol = "Estudiante" }
        };

        _mockRepository.Setup(r => r.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(estudiantes);

        // Act
        var result = await _service.ObtenerTodosLosEstudiantesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Nombre.Should().Be("Juan Pérez");
        result.Last().Nombre.Should().Be("María García");
        
        _mockRepository.Verify(r => r.ObtenerTodosLosEstudiantesAsync(), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantesAsync_CuandoNoHayEstudiantes_DebeRetornarListaVacia()
    {
        // Arrange
        _mockRepository.Setup(r => r.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(new List<Estudiante>());

        // Act
        var result = await _service.ObtenerTodosLosEstudiantesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        
        _mockRepository.Verify(r => r.ObtenerTodosLosEstudiantesAsync(), Times.Once);
    }

    [Fact]
    public async Task ObtenerPerfilEstudianteAsync_CuandoEstudianteExiste_DebeRetornarEstudiante()
    {
        // Arrange
        var id = Guid.NewGuid();
        var estudiante = new Estudiante 
        { 
            Id = id, 
            Nombre = "Juan Pérez", 
            Correo = "juan@email.com", 
            Rol = "Estudiante" 
        };

        _mockRepository.Setup(r => r.ObtenerEstudiantePorIdAsync(id)).ReturnsAsync(estudiante);

        // Act
        var result = await _service.ObtenerPerfilEstudianteAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Nombre.Should().Be("Juan Pérez");
        
        _mockRepository.Verify(r => r.ObtenerEstudiantePorIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_CuandoDatosSonValidos_DebeRegistrarEstudiante()
    {
        // Arrange
        var estudianteDto = new EstudianteDto
        {
            Nombre = "Nuevo Estudiante",
            Correo = "nuevo@email.com",
            Password = "password123"
        };

        var estudianteId = Guid.NewGuid();
        _mockRepository.Setup(r => r.CorreoExisteAsync(estudianteDto.Correo)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.AgregarEstudianteAsync(It.IsAny<Estudiante>())).ReturnsAsync(estudianteId);

        // Act
        var result = await _service.RegistrarAsync(estudianteDto);

        // Assert
        result.Should().Be(estudianteId);
        
        _mockRepository.Verify(r => r.CorreoExisteAsync(estudianteDto.Correo), Times.Once);
        _mockRepository.Verify(r => r.AgregarEstudianteAsync(It.IsAny<Estudiante>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_CuandoCorreoYaExiste_DebeLanzarExcepcion()
    {
        // Arrange
        var estudianteDto = new EstudianteDto
        {
            Nombre = "Nuevo Estudiante",
            Correo = "existente@email.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.CorreoExisteAsync(estudianteDto.Correo)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegistrarAsync(estudianteDto));
        
        _mockRepository.Verify(r => r.CorreoExisteAsync(estudianteDto.Correo), Times.Once);
        _mockRepository.Verify(r => r.AgregarEstudianteAsync(It.IsAny<Estudiante>()), Times.Never);
    }

    [Fact]
    public async Task EliminarEstudianteAsync_CuandoEstudianteExiste_DebeEliminarEstudiante()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.EliminarEstudianteAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _service.EliminarEstudianteAsync(id);

        // Assert
        result.Should().BeTrue();
        
        _mockRepository.Verify(r => r.EliminarEstudianteAsync(id), Times.Once);
    }

    [Fact]
    public async Task EliminarEstudianteAsync_CuandoEstudianteNoExiste_DebeRetornarFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.EliminarEstudianteAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _service.EliminarEstudianteAsync(id);

        // Assert
        result.Should().BeFalse();
        
        _mockRepository.Verify(r => r.EliminarEstudianteAsync(id), Times.Once);
    }

    [Fact]
    public async Task BuscarPorCorreoAsync_CuandoEstudianteExiste_DebeRetornarEstudiante()
    {
        // Arrange
        var correo = "juan@email.com";
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = "Juan Pérez",
            Correo = correo,
            Rol = "Estudiante"
        };

        _mockRepository.Setup(r => r.BuscarPorCorreoAsync(correo)).ReturnsAsync(estudiante);

        // Act
        var result = await _mockRepository.Object.BuscarPorCorreoAsync(correo);

        // Assert
        result.Should().NotBeNull();
        result.Correo.Should().Be(correo);
        
        _mockRepository.Verify(r => r.BuscarPorCorreoAsync(correo), Times.Once);
    }

    [Fact]
    public async Task BuscarPorCorreoAsync_CuandoEstudianteNoExiste_DebeRetornarNull()
    {
        // Arrange
        var correo = "inexistente@email.com";
        _mockRepository.Setup(r => r.BuscarPorCorreoAsync(correo)).ReturnsAsync((Estudiante)null);

        // Act
        var result = await _mockRepository.Object.BuscarPorCorreoAsync(correo);

        // Assert
        result.Should().BeNull();
        
        _mockRepository.Verify(r => r.BuscarPorCorreoAsync(correo), Times.Once);
    }
} 