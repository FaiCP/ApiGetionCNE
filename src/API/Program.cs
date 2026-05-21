using System.Threading.RateLimiting;
using Serilog;
using API.Middleware;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Validar configuracion critica de seguridad (solo en produccion y staging, no en Testing)
var jwtKey = builder.Configuration["Jwt:Key"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!builder.Environment.IsEnvironment("Testing"))
{
    if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        throw new InvalidOperationException("JWT:Key debe estar configurada con al menos 32 caracteres. Use variables de entorno o User Secrets.");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection debe estar configurada. Use variables de entorno o User Secrets.");
}

if (string.IsNullOrWhiteSpace(jwtKey))
    jwtKey = "fallback-dev-key-for-testing-only-min-32ch!!";

if (string.IsNullOrWhiteSpace(connectionString))
    connectionString = "InMemory";

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Registrar servicios de Application (MediatR, AutoMapper, FluentValidation)
builder.Services.AddApplicationServices(builder.Configuration);

// Registrar servicios de Infrastructure (DbContext y Repositorios)
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestorAdmi API",
        Version = "v1",
        Description = "API de gestión de inventario y activos - Migración a .NET Core",
        Contact = new OpenApiContact
        {
            Name = "GestorAdmi Team"
        }
    });

    // Incluir comentarios XML en Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // Configurar JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa solo el token JWT (sin la palabra Bearer)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200", "http://localhost:8080", "https://localhost:3000", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:5001" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configurar JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GestorAdmi",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GestorAdmiClient",
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Administrador", "Admin"));
    options.AddPolicy("RequireUserRole", policy =>
        policy.RequireAuthenticatedUser());
});

// Configurar rate limiting (solo en produccion y staging)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter("LoginPolicy", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });
    });
}

var app = builder.Build();

// Global exception handler - debe ser el primer middleware
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GestorAdmi API v1");
    options.RoutePrefix = "swagger";
});

app.UseSerilogRequestLogging();

// Usar CORS (debe ir antes de UseHttpsRedirection para que los preflights no sean redirigidos)
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
if (!app.Environment.IsEnvironment("Testing"))
    app.UseRateLimiter();

app.MapControllers();

// Log de inicio
Log.Information("GestorAdmi API iniciada en {Environment}", app.Environment.EnvironmentName);

// Seed de datos de prueba (solo si las tablas están vacías)
if (app.Environment.IsDevelopment())
    await Infrastructure.Persistence.DataSeeder.SeedAsync(app.Services);

app.Run();

// Exponer la clase Program para WebApplicationFactory en pruebas de integración
public partial class Program { }
