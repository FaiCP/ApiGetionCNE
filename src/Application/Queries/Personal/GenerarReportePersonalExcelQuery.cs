using Application.Common;
using Application.DTOs.Documentos;
using Application.Interfaces;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Queries.Personal;

public record GenerarReportePersonalExcelQuery : IRequest<byte[]>;

public class GenerarReportePersonalExcelQueryHandler : IRequestHandler<GenerarReportePersonalExcelQuery, byte[]>
{
    private readonly IGestionActivoRepository _gestionRepo;
    private readonly IPersonalRepository _personalRepo;
    private readonly IExcelService _excelService;
    private readonly ReportSettings _settings;

    public GenerarReportePersonalExcelQueryHandler(
        IGestionActivoRepository gestionRepo,
        IPersonalRepository personalRepo,
        IExcelService excelService,
        IOptions<ReportSettings> settings)
    {
        _gestionRepo = gestionRepo;
        _personalRepo = personalRepo;
        _excelService = excelService;
        _settings = settings.Value;
    }

    public async Task<byte[]> Handle(GenerarReportePersonalExcelQuery request, CancellationToken cancellationToken)
    {
        var fechaLimite = DateTime.Now.AddMonths(-_settings.RangoMesesReporte);
        var añoActual = DateTime.Now.Year;

        var equipos = await _gestionRepo.GetAllActiveWithDetailsAsync();
        var personal = await _personalRepo.GetAllActiveAsync();

        var reporte = new List<ReportePersonalItemDto>();

        foreach (var g in equipos.Where(e => e.FechaAsignacion >= fechaLimite && e.FechaAsignacion?.Year == añoActual))
        {
            var custodio = g.Custodio?.Nombre ?? "";
            reporte.Add(new ReportePersonalItemDto(
                g.FechaAsignacion,
                _settings.EntregaPersonalNombre,
                custodio,
                "X", null,
                $"{g.Hardware?.NombreDispositivo}, Marca:{g.Hardware?.Marca}, Modelo:{g.Hardware?.Modelo}, Serie:{g.Hardware?.CodigoCne}"
            ));
        }

        foreach (var p in personal.Where(p => p.Fecha >= fechaLimite && p.Fecha?.Year == añoActual))
        {
            reporte.Add(new ReportePersonalItemDto(
                p.Fecha,
                _settings.EntregaPersonalNombre,
                p.Nombre,
                null, "X",
                _settings.CredencialesDescripcion
            ));
        }

        return _excelService.GenerarReportePersonal(reporte);
    }
}