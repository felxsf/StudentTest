using Application.Interfaces;
using Application.Services;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Events;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Iniciando aplicación StudentTest API...");

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
        {
            TableName = "Logs",
            AutoCreateSqlTable = true
        }
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddSingleton(Log.Logger);

// Servicios básicos
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "StudentTest",
            ValidAudience = "StudentTest",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_super_secreta_123!_muy_larga_para_jwt_256_bits_minimo_requerido"))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               .WithExposedHeaders("Content-Disposition"); // Para descargas de archivos
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mover UseCors al inicio del pipeline, antes de UseHttpsRedirection
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Middleware de logging
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    
    try
    {
        await next();
        
        var elapsed = DateTime.UtcNow - start;
        Log.Information(
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds);
    }
    catch (Exception ex)
    {
        var elapsed = DateTime.UtcNow - start;
        Log.Error(
            ex,
            "HTTP {RequestMethod} {RequestPath} failed after {Elapsed:0.0000} ms",
            context.Request.Method,
            context.Request.Path,
            elapsed.TotalMilliseconds);
        throw;
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed data simple
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        Console.WriteLine("Iniciando seed de datos...");
        await DataSeeder.SeedDataAsync(context);
        Console.WriteLine("Seed de datos completado exitosamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error durante el seed de datos: {ex.Message}");
    }
}

Console.WriteLine("Aplicación StudentTest API iniciada correctamente");
app.Run();
