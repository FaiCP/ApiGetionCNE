using Application.DTOs.Documentos;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.Personal;

public record GenerarActaPersonalPdfQuery(List<long> Ids) : IRequest<byte[]>;

public class GenerarActaPersonalPdfQueryHandler : IRequestHandler<GenerarActaPersonalPdfQuery, byte[]>
{
    private readonly IPersonalRepository _personalRepo;
    private readonly IPdfService _pdfService;

    public GenerarActaPersonalPdfQueryHandler(IPersonalRepository personalRepo, IPdfService pdfService)
    {
        _personalRepo = personalRepo;
        _pdfService = pdfService;
    }

    public async Task<byte[]> Handle(GenerarActaPersonalPdfQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
            throw new ValidationException("Debe proporcionar al menos un ID de personal.");

        var personas = await _personalRepo.GetByIdsAsync(request.Ids);
        if (personas.Count == 0)
            throw new NotFoundException("Personal", string.Join(",", request.Ids));

        var first = personas[0];
        var dto = new PersonalActaItemDto(first.Nombre, first.Cedula, first.Cargo, first.Fecha, first.Email ?? string.Empty);
        return _pdfService.GenerarActaPersonal(dto);
    }
}
