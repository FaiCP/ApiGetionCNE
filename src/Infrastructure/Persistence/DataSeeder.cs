using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db      = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher  = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await db.Database.MigrateAsync();

            await SeedDepartamentosAsync(db, logger);
            await SeedUsuariosAsync(db, hasher, logger);
            await SeedPersonalAsync(db, logger);
            await SeedCustodiosAsync(db, logger);
            await SeedHardwareAsync(db, logger);
            await SeedKitsAsync(db, logger);
            await SeedSuministrosAsync(db, logger);
            await SeedGestionActivosAsync(db, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante el seed de datos");
        }
    }

    // ─── Departamentos ────────────────────────────────────────────────────────

    private static async Task SeedDepartamentosAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Departamentos.AnyAsync()) return;

        var departamentos = new[]
        {
            new Departamento { Nombre = "Tecnología de la Información" },
            new Departamento { Nombre = "Recursos Humanos" },
            new Departamento { Nombre = "Administración" },
            new Departamento { Nombre = "Contabilidad" },
            new Departamento { Nombre = "Logística" },
        };

        await db.Departamentos.AddRangeAsync(departamentos);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} departamentos insertados", departamentos.Length);
    }

    // ─── Usuarios ─────────────────────────────────────────────────────────────

    private static async Task SeedUsuariosAsync(ApplicationDbContext db, IPasswordHasher hasher, ILogger logger)
    {
        if (await db.Usuarios.AnyAsync()) return;

        var usuarios = new[]
        {
            new Usuario
            {
                Nombre   = "admin",
                Password = hasher.HashPassword("Admin123!"),
                Cargo    = "Administrador de Sistemas",
                Email    = "admin@cne.gob.ec",
                Rol      = "Admin"
            },
            new Usuario
            {
                Nombre   = "jperez",
                Password = hasher.HashPassword("User123!"),
                Cargo    = "Técnico TI",
                Email    = "jperez@cne.gob.ec",
                Rol      = "User"
            },
        };

        await db.Usuarios.AddRangeAsync(usuarios);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} usuarios insertados (admin / jperez)", usuarios.Length);
    }

    // ─── Personal ─────────────────────────────────────────────────────────────

    private static async Task SeedPersonalAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Personas.AnyAsync()) return;

        var personal = new[]
        {
            new Persona { Nombre = "Carlos Mendoza",    Cedula = "0101234567", Cargo = "Director TI",          Email = "cmendoza@cne.gob.ec",    Fecha = new DateTime(2021, 3, 15) },
            new Persona { Nombre = "Ana Rodríguez",     Cedula = "0102345678", Cargo = "Analista de Sistemas",  Email = "arodriguez@cne.gob.ec",  Fecha = new DateTime(2020, 7, 1)  },
            new Persona { Nombre = "Luis Paredes",      Cedula = "0103456789", Cargo = "Técnico de Soporte",    Email = "lparedes@cne.gob.ec",    Fecha = new DateTime(2022, 1, 10) },
            new Persona { Nombre = "María Torres",      Cedula = "0104567890", Cargo = "Jefa Administrativa",   Email = "mtorres@cne.gob.ec",     Fecha = new DateTime(2019, 5, 20) },
            new Persona { Nombre = "Jorge Castillo",    Cedula = "0105678901", Cargo = "Contador",              Email = "jcastillo@cne.gob.ec",   Fecha = new DateTime(2020, 11, 3) },
            new Persona { Nombre = "Patricia Gómez",    Cedula = "0106789012", Cargo = "Asistente RRHH",        Email = "pgomez@cne.gob.ec",      Fecha = new DateTime(2023, 2, 28) },
            new Persona { Nombre = "Roberto Salazar",   Cedula = "0107890123", Cargo = "Coordinador Logística", Email = "rsalazar@cne.gob.ec",    Fecha = new DateTime(2021, 8, 14) },
            new Persona { Nombre = "Elena Vásquez",     Cedula = "0108901234", Cargo = "Secretaria",            Email = "evasquez@cne.gob.ec",    Fecha = new DateTime(2022, 6, 5)  },
            new Persona { Nombre = "Miguel Ávila",      Cedula = "0109012345", Cargo = "Técnico de Redes",      Email = "mavila@cne.gob.ec",      Fecha = new DateTime(2023, 4, 17) },
            new Persona { Nombre = "Sandra Mora",       Cedula = "0100123456", Cargo = "Auditora Interna",      Email = "smora@cne.gob.ec",       Fecha = new DateTime(2020, 9, 22) },
        };

        await db.Personas.AddRangeAsync(personal);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} personas insertadas", personal.Length);
    }

    // ─── Custodios ────────────────────────────────────────────────────────────

    private static async Task SeedCustodiosAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Custodios.AnyAsync()) return;

        var depts = await db.Departamentos.ToListAsync();
        long IdDept(string nombre) => depts.First(d => d.Nombre == nombre).Id;

        var custodios = new[]
        {
            new Custodio { Nombre = "Carlos Mendoza",  Cargo = "Director TI",          Cedula = "0101234567", IdDepartamento = IdDept("Tecnología de la Información") },
            new Custodio { Nombre = "Ana Rodríguez",   Cargo = "Analista de Sistemas",  Cedula = "0102345678", IdDepartamento = IdDept("Tecnología de la Información") },
            new Custodio { Nombre = "Luis Paredes",    Cargo = "Técnico de Soporte",    Cedula = "0103456789", IdDepartamento = IdDept("Tecnología de la Información") },
            new Custodio { Nombre = "María Torres",    Cargo = "Jefa Administrativa",   Cedula = "0104567890", IdDepartamento = IdDept("Administración") },
            new Custodio { Nombre = "Jorge Castillo",  Cargo = "Contador",              Cedula = "0105678901", IdDepartamento = IdDept("Contabilidad") },
            new Custodio { Nombre = "Patricia Gómez",  Cargo = "Asistente RRHH",        Cedula = "0106789012", IdDepartamento = IdDept("Recursos Humanos") },
            new Custodio { Nombre = "Roberto Salazar", Cargo = "Coordinador Logística", Cedula = "0107890123", IdDepartamento = IdDept("Logística") },
            new Custodio { Nombre = "Elena Vásquez",   Cargo = "Secretaria",            Cedula = "0108901234", IdDepartamento = IdDept("Administración") },
        };

        await db.Custodios.AddRangeAsync(custodios);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} custodios insertados", custodios.Length);
    }

    // ─── Hardware ─────────────────────────────────────────────────────────────

    private static async Task SeedHardwareAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Hardware.AnyAsync()) return;

        var hardware = new[]
        {
            new Hardware { IdEquipo = "PC-001", Marca = "Dell",    Modelo = "OptiPlex 7090",    NombreDispositivo = "Computadora",   Estado = "Activo",    Ubicacion = "Oficina TI",       CodigoCne = "CNE-001", FechaAdquisicion = new DateTime(2022, 3, 10), Valor = 1200.00m },
            new Hardware { IdEquipo = "PC-002", Marca = "HP",      Modelo = "ProDesk 400 G7",   NombreDispositivo = "Computadora",   Estado = "Activo",    Ubicacion = "Sala de Reuniones", CodigoCne = "CNE-002", FechaAdquisicion = new DateTime(2022, 5, 15), Valor = 1100.00m },
            new Hardware { IdEquipo = "LT-001", Marca = "Lenovo",  Modelo = "ThinkPad E15",     NombreDispositivo = "Laptop",        Estado = "Activo",    Ubicacion = "Gerencia",          CodigoCne = "CNE-003", FechaAdquisicion = new DateTime(2021, 8, 20), Valor = 1500.00m },
            new Hardware { IdEquipo = "LT-002", Marca = "HP",      Modelo = "EliteBook 840",    NombreDispositivo = "Laptop",        Estado = "Activo",    Ubicacion = "Contabilidad",      CodigoCne = "CNE-004", FechaAdquisicion = new DateTime(2021, 11, 5), Valor = 1400.00m },
            new Hardware { IdEquipo = "LT-003", Marca = "Dell",    Modelo = "Latitude 5520",    NombreDispositivo = "Laptop",        Estado = "Prestado",  Ubicacion = "RRHH",              CodigoCne = "CNE-005", FechaAdquisicion = new DateTime(2023, 1, 18), Valor = 1350.00m },
            new Hardware { IdEquipo = "MN-001", Marca = "Samsung", Modelo = "27\" LF27T352",   NombreDispositivo = "Monitor",       Estado = "Activo",    Ubicacion = "Oficina TI",       CodigoCne = "CNE-006", FechaAdquisicion = new DateTime(2022, 3, 10), Valor = 350.00m  },
            new Hardware { IdEquipo = "MN-002", Marca = "LG",      Modelo = "24MK430H",         NombreDispositivo = "Monitor",       Estado = "Activo",    Ubicacion = "Administración",    CodigoCne = "CNE-007", FechaAdquisicion = new DateTime(2022, 7, 22), Valor = 280.00m  },
            new Hardware { IdEquipo = "PR-001", Marca = "Epson",   Modelo = "EcoTank L3150",    NombreDispositivo = "Impresora",     Estado = "Activo",    Ubicacion = "Administración",    CodigoCne = "CNE-008", FechaAdquisicion = new DateTime(2020, 4, 14), Valor = 420.00m  },
            new Hardware { IdEquipo = "PR-002", Marca = "HP",      Modelo = "LaserJet Pro M404", NombreDispositivo = "Impresora",    Estado = "En Reparación", Ubicacion = "Contabilidad", CodigoCne = "CNE-009", FechaAdquisicion = new DateTime(2019, 9, 30), Valor = 650.00m  },
            new Hardware { IdEquipo = "SW-001", Marca = "Cisco",   Modelo = "Catalyst 2960",    NombreDispositivo = "Switch",        Estado = "Activo",    Ubicacion = "Rack Principal",    CodigoCne = "CNE-010", FechaAdquisicion = new DateTime(2020, 1, 8),  Valor = 2200.00m },
            new Hardware { IdEquipo = "RT-001", Marca = "MikroTik",Modelo = "RB4011iGS",        NombreDispositivo = "Router",        Estado = "Activo",    Ubicacion = "Rack Principal",    CodigoCne = "CNE-011", FechaAdquisicion = new DateTime(2021, 6, 25), Valor = 480.00m  },
            new Hardware { IdEquipo = "PC-003", Marca = "Dell",    Modelo = "OptiPlex 3080",    NombreDispositivo = "Computadora",   Estado = "Inactivo",  Ubicacion = "Bodega",            CodigoCne = "CNE-012", FechaAdquisicion = new DateTime(2019, 2, 17), Valor = 850.00m  },
            new Hardware { IdEquipo = "LT-004", Marca = "Asus",    Modelo = "ExpertBook B1",    NombreDispositivo = "Laptop",        Estado = "Activo",    Ubicacion = "Logística",         CodigoCne = "CNE-013", FechaAdquisicion = new DateTime(2023, 3, 12), Valor = 1250.00m },
            new Hardware { IdEquipo = "SC-001", Marca = "Epson",   Modelo = "Perfection V39",   NombreDispositivo = "Escáner",       Estado = "Activo",    Ubicacion = "Administración",    CodigoCne = "CNE-014", FechaAdquisicion = new DateTime(2022, 10, 3), Valor = 190.00m  },
            new Hardware { IdEquipo = "UPS-001",Marca = "APC",     Modelo = "Back-UPS 1200VA",  NombreDispositivo = "UPS",           Estado = "Activo",    Ubicacion = "Rack Principal",    CodigoCne = "CNE-015", FechaAdquisicion = new DateTime(2021, 4, 29), Valor = 320.00m  },
        };

        await db.Hardware.AddRangeAsync(hardware);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} equipos de hardware insertados", hardware.Length);

        // Características de computadoras
        var caracteristicas = new[]
        {
            new CaracteristicaComputadora { IdEquipo = "PC-001", Ram = "16GB DDR4",  Rom = "512GB SSD", Procesador = "Intel Core i7-10700" },
            new CaracteristicaComputadora { IdEquipo = "PC-002", Ram = "8GB DDR4",   Rom = "256GB SSD", Procesador = "Intel Core i5-10500" },
            new CaracteristicaComputadora { IdEquipo = "LT-001", Ram = "16GB DDR4",  Rom = "512GB SSD", Procesador = "Intel Core i7-1165G7" },
            new CaracteristicaComputadora { IdEquipo = "LT-002", Ram = "8GB DDR4",   Rom = "256GB SSD", Procesador = "Intel Core i5-1135G7" },
            new CaracteristicaComputadora { IdEquipo = "LT-003", Ram = "16GB DDR4",  Rom = "512GB SSD", Procesador = "Intel Core i7-1185G7" },
            new CaracteristicaComputadora { IdEquipo = "PC-003", Ram = "4GB DDR4",   Rom = "1TB HDD",   Procesador = "Intel Core i3-10100"  },
            new CaracteristicaComputadora { IdEquipo = "LT-004", Ram = "8GB DDR4",   Rom = "512GB SSD", Procesador = "Intel Core i5-1235U"  },
        };

        await db.CaracteristicasComputadora.AddRangeAsync(caracteristicas);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} características de computadora insertadas", caracteristicas.Length);
    }

    // ─── Kits ─────────────────────────────────────────────────────────────────

    private static async Task SeedKitsAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Kits.AnyAsync()) return;

        var kits = new[]
        {
            new Kit { Insumo = "Tóner",          Cantidad = 10, Estado = "Disponible", Marca = "HP",      Serie = "CF230A",   Modelo = "30A",       Observacion = "Para impresora LaserJet" },
            new Kit { Insumo = "Tóner",          Cantidad = 5,  Estado = "Disponible", Marca = "Epson",   Serie = "C13T664",  Modelo = "664",       Observacion = "Para EcoTank L3150"       },
            new Kit { Insumo = "Cable UTP",      Cantidad = 50, Estado = "Disponible", Marca = "Belden",  Serie = "CAT6-001", Modelo = "Cat6 1m",   Observacion = "Cables de red 1 metro"    },
            new Kit { Insumo = "Cable HDMI",     Cantidad = 15, Estado = "Disponible", Marca = "Ugreen",  Serie = "HM-002",   Modelo = "2m HDMI",   Observacion = "Cables de video 2 metros" },
            new Kit { Insumo = "Teclado",        Cantidad = 8,  Estado = "Disponible", Marca = "Logitech",Serie = "K120",     Modelo = "K120",      Observacion = "Teclados USB estándar"    },
            new Kit { Insumo = "Mouse",          Cantidad = 12, Estado = "Disponible", Marca = "Logitech",Serie = "B100",     Modelo = "B100",      Observacion = "Mouse USB óptico"         },
            new Kit { Insumo = "Disco Duro",     Cantidad = 4,  Estado = "Disponible", Marca = "Seagate", Serie = "ST1000",   Modelo = "Barracuda 1TB", Observacion = "Repuestos HDD"        },
            new Kit { Insumo = "Memoria RAM",    Cantidad = 6,  Estado = "Disponible", Marca = "Kingston",Serie = "KVR32N22", Modelo = "8GB DDR4",  Observacion = "Módulos de expansión"     },
            new Kit { Insumo = "Patch Panel",    Cantidad = 2,  Estado = "Disponible", Marca = "Keystone",Serie = "PP-24P",   Modelo = "24 puertos",Observacion = "Para rack de cableado"    },
            new Kit { Insumo = "Fuente de poder",Cantidad = 3,  Estado = "Disponible", Marca = "Corsair", Serie = "CV450",    Modelo = "CV 450W",   Observacion = "Repuestos fuentes PC"     },
        };

        await db.Kits.AddRangeAsync(kits);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} kits insertados", kits.Length);
    }

    // ─── Suministros ──────────────────────────────────────────────────────────

    private static async Task SeedSuministrosAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Suministros.AnyAsync()) return;

        var suministros = new[]
        {
            new Suministro { IdEquipo = "SUM-001", TipoSuministro = "Tóner",      FechaRetiro = new DateTime(2024, 1, 15), IdEquipoAsignado = "PR-002" },
            new Suministro { IdEquipo = "SUM-002", TipoSuministro = "Tóner",      FechaRetiro = new DateTime(2024, 3, 22), IdEquipoAsignado = "PR-001" },
            new Suministro { IdEquipo = "SUM-003", TipoSuministro = "Disco Duro", FechaRetiro = new DateTime(2024, 2, 10), IdEquipoAsignado = "PC-003" },
            new Suministro { IdEquipo = "SUM-004", TipoSuministro = "Batería UPS",FechaRetiro = new DateTime(2023, 12, 5),IdEquipoAsignado = "UPS-001"},
            new Suministro { IdEquipo = "SUM-005", TipoSuministro = "Memoria RAM",FechaRetiro = new DateTime(2024, 4, 8), IdEquipoAsignado = "PC-001" },
        };

        await db.Suministros.AddRangeAsync(suministros);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} suministros insertados", suministros.Length);
    }

    // ─── Gestión de activos ───────────────────────────────────────────────────

    private static async Task SeedGestionActivosAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.GestionActivos.AnyAsync()) return;

        var custodios = await db.Custodios.ToListAsync();
        long IdCust(string cedula) => custodios.First(c => c.Cedula == cedula).Id;

        var gestion = new[]
        {
            new GestionActivo { IdEquipo = "PC-001", IdCustodio = IdCust("0101234567"), FechaAsignacion = new DateTime(2022, 3, 12) },
            new GestionActivo { IdEquipo = "PC-002", IdCustodio = IdCust("0102345678"), FechaAsignacion = new DateTime(2022, 5, 17) },
            new GestionActivo { IdEquipo = "LT-001", IdCustodio = IdCust("0104567890"), FechaAsignacion = new DateTime(2021, 8, 22) },
            new GestionActivo { IdEquipo = "LT-002", IdCustodio = IdCust("0105678901"), FechaAsignacion = new DateTime(2021, 11, 8) },
            new GestionActivo { IdEquipo = "LT-003", IdCustodio = IdCust("0106789012"), FechaAsignacion = new DateTime(2023, 1, 20) },
            new GestionActivo { IdEquipo = "LT-004", IdCustodio = IdCust("0107890123"), FechaAsignacion = new DateTime(2023, 3, 15) },
            new GestionActivo { IdEquipo = "MN-001", IdCustodio = IdCust("0101234567"), FechaAsignacion = new DateTime(2022, 3, 12) },
            new GestionActivo { IdEquipo = "MN-002", IdCustodio = IdCust("0108901234"), FechaAsignacion = new DateTime(2022, 7, 25) },
            new GestionActivo { IdEquipo = "PR-001", IdCustodio = IdCust("0108901234"), FechaAsignacion = new DateTime(2020, 4, 16) },
            new GestionActivo { IdEquipo = "SC-001", IdCustodio = IdCust("0104567890"), FechaAsignacion = new DateTime(2022, 10, 5) },
        };

        await db.GestionActivos.AddRangeAsync(gestion);
        await db.SaveChangesAsync();
        logger.LogInformation("Seed: {Count} asignaciones de gestión de activos insertadas", gestion.Length);
    }
}
