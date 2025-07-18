using API.Controllers;
using Application.Interfaces;
using Application.DTOs;
using API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.API;

public class EstudiantesControllerTests
{
    private readonly Mock<IEstudianteService> _mockService;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly EstudiantesController _controller;

    public EstudiantesControllerTests()
    {
        _mockService = new Mock<IEstudianteService>();
        _mockLoggingService = new Mock<ILoggingService>();
        _controller = new EstudiantesController(_mockService.Object, _mockLoggingService.Object);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantes_CuandoHayEstudiantes_DebeRetornarOkConLista()
    {
        // Arrange
        var estudiantes = new List<EstudianteDto>
        {
            new EstudianteDto { Id = Guid.NewGuid(), Nombre = "Juan Pérez", Correo = "juan@email.com" },
            new EstudianteDto { Id = Guid.NewGuid(), Nombre = "María García", Correo = "maria@email.com" }
        };

        _mockService.Setup(s => s.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(estudiantes);

        // Act
        var result = await _controller.ObtenerTodosLosEstudiantes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEstudiantes = okResult.Value.Should().BeOfType<List<EstudianteDto>>().Subject;
        returnedEstudiantes.Should().HaveCount(2);
        
        _mockService.Verify(s => s.ObtenerTodosLosEstudiantesAsync(), Times.Once);
        _mockLoggingService.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantes_CuandoNoHayEstudiantes_DebeRetornarOkConListaVacia()
    {
        // Arrange
        _mockService.Setup(s => s.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(new List<EstudianteDto>());

        // Act
        var result = await _controller.ObtenerTodosLosEstudiantes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEstudiantes = okResult.Value.Should().BeOfType<List<EstudianteDto>>().Subject;
        returnedEstudiantes.Should().BeEmpty();
        
        _mockService.Verify(s => s.ObtenerTodosLosEstudiantesAsync(), Times.Once);
    }

    [Fact]
    public async Task Registrar_CuandoDatosSonValidos_DebeRetornarOk()
    {
        // Arrange
        var estudianteDto = new EstudianteDto
        {
            Nombre = "Nuevo Estudiante",
            Correo = "nuevo@email.com",
            Password = "password123"
        };

        var estudianteId = Guid.NewGuid();
        _mockService.Setup(s => s.RegistrarAsync(estudianteDto)).ReturnsAsync(estudianteId);

        // Act
        var result = await _controller.Registrar(estudianteDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<dynamic>().Subject;
        
        _mockService.Verify(s => s.RegistrarAsync(estudianteDto), Times.Once);
        _mockLoggingService.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        _mockLoggingService.Verify(l => l.LogAudit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Login_CuandoCredencialesSonValidas_DebeRetornarOk()
    {
        // Arrange
        var authRequest = new AuthRequestDto
        {
            Correo = "juan@email.com",
            Password = "password123"
        };

        var authResponse = new AuthResponseDto
        {
            Token = "jwt_token_here",
            Nombre = "Juan Pérez",
            Correo = "juan@email.com",
            Rol = "Estudiante"
        };

        _mockService.Setup(s => s.LoginAsync(authRequest)).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(authRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<AuthResponseDto>().Subject;
        returnedResponse.Token.Should().Be("jwt_token_here");
        returnedResponse.Nombre.Should().Be("Juan Pérez");
        returnedResponse.Correo.Should().Be("juan@email.com");
        returnedResponse.Rol.Should().Be("Estudiante");
        
        _mockService.Verify(s => s.LoginAsync(authRequest), Times.Once);
        _mockLoggingService.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        _mockLoggingService.Verify(l => l.LogSecurity(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Login_CuandoCredencialesSonInvalidas_DebeLanzarExcepcion()
    {
        // Arrange
        var authRequest = new AuthRequestDto
        {
            Correo = "juan@email.com",
            Password = "wrongpassword"
        };

        _mockService.Setup(s => s.LoginAsync(authRequest))
            .ThrowsAsync(new UnauthorizedAccessException("Credenciales inválidas"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Login(authRequest));
        
        _mockService.Verify(s => s.LoginAsync(authRequest), Times.Once);
        _mockLoggingService.Verify(l => l.LogSecurity(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMiPerfil_CuandoUsuarioAutenticado_DebeRetornarPerfil()
    {
        // Arrange
        var estudianteId = Guid.NewGuid();
        var estudiante = new EstudianteDto
        {
            Id = estudianteId,
            Nombre = "Juan Pérez",
            Correo = "juan@email.com"
        };

        _mockService.Setup(s => s.ObtenerPerfilEstudianteAsync(estudianteId)).ReturnsAsync(estudiante);

        // Act
        var result = await _controller.ObtenerMiPerfil();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEstudiante = okResult.Value.Should().BeOfType<EstudianteDto>().Subject;
        returnedEstudiante.Id.Should().Be(estudianteId);
        
        _mockService.Verify(s => s.ObtenerPerfilEstudianteAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task InscribirMaterias_CuandoDatosSonValidos_DebeRetornarOk()
    {
        // Arrange
        var inscripcionDto = new InscripcionDto
        {
            EstudianteId = Guid.NewGuid(),
            MateriasIds = new List<int> { 1, 2, 3 }
        };

        // Act
        var result = await _controller.InscribirMaterias(inscripcionDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        
        _mockService.Verify(s => s.InscribirMateriasAsync(inscripcionDto), Times.Once);
        _mockLoggingService.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerCompañeros_CuandoMateriaExiste_DebeRetornarCompañeros()
    {
        // Arrange
        var materiaId = 1;
        var compañeros = new List<string> { "Juan Pérez", "María García", "Carlos López" };

        _mockService.Setup(s => s.ObtenerNombresPorMateria(materiaId)).ReturnsAsync(compañeros);

        // Act
        var result = await _controller.ObtenerCompañeros(materiaId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCompañeros = okResult.Value.Should().BeOfType<IEnumerable<string>>().Subject;
        returnedCompañeros.Should().HaveCount(3);
        
        _mockService.Verify(s => s.ObtenerNombresPorMateria(materiaId), Times.Once);
    }
} 