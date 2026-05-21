using Application.Common;
using Application.DTOs.Documentos;
using Application.Interfaces;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Queries.Personal;

public record GenerarReportePersonalPdfQuery : IRequest<byte[]>;

public class GenerarReportePersonalPdfQueryHandler : IRequestHandler<GenerarReportePersonalPdfQuery, byte[]>
{
    private readonly IGestionActivoRepository _gestionRepo;
    private readonly IPersonalRepository _personalRepo;
    private readonly IPdfService _pdfService;
    private readonly ReportSettings _settings;

    public GenerarReportePersonalPdfQueryHandler(
        IGestionActivoRepository gestionRepo,
        IPersonalRepository personalRepo,
        IPdfService pdfService,
        IOptions<ReportSettings> settings)
    {
        _gestionRepo = gestionRepo;
        _personalRepo = personalRepo;
        _pdfService = pdfService;
        _settings = settings.Value;
    }

    public async Task<byte[]> Handle(GenerarReportePersonalPdfQuery request, CancellationToken cancellationToken)
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

            if (g.FechaDevolucion >= fechaLimite && g.FechaDevolucion?.Year == añoActual)
            {
                reporte.Add(new ReportePersonalItemDto(
                    g.FechaDevolucion,
                    custodio,
                    _settings.EntregaPersonalNombre,
                    "X", null,
                    $"{g.Hardware?.NombreDispositivo}, Marca:{g.Hardware?.Marca}, Modelo:{g.Hardware?.Modelo}, Serie:{g.Hardware?.CodigoCne}"
                ));
            }
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

        return await _pdfService.GenerarReportePersonalAsync(reporte);
    }
}