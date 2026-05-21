using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

/// <summary>
/// Extensión para registrar servicios de Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        // Registrar DbContext - InMemory para Testing, SqlServer para el resto
        if (environment?.IsEnvironment("Testing") == true)
        {
            var dbName = configuration["InMemoryDatabaseName"] ?? $"GestorAdmiTestDb_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")!,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });
        }

        // Registrar repositorios
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IHardwareRepository, HardwareRepository>();
        services.AddScoped<ICustodioRepository, CustodioRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
        services.AddScoped<IPersonalRepository, PersonalRepository>();
        services.AddScoped<IKitRepository, KitRepository>();
        services.AddScoped<ISuministroRepository, SuministroRepository>();
        services.AddScoped<IGestionActivoRepository, GestionActivoRepository>();
        services.AddScoped<IHistorialCustodioRepository, HistorialCustodioRepository>();

        // Registrar servicios
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
