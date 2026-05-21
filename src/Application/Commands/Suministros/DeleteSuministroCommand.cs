using Domain.Interfaces;
using MediatR;

namespace Application.Commands.Suministros;

public record DeleteSuministroCommand(List<long> Ids) : IRequest<bool>;

public class DeleteSuministroCommandHandler : IRequestHandler<DeleteSuministroCommand, bool>
{
    private readonly ISuministroRepository _suministroRepository;

    public DeleteSuministroCommandHandler(ISuministroRepository suministroRepository)
    {
        _suministroRepository = suministroRepository;
    }

    public async Task<bool> Handle(DeleteSuministroCommand request, CancellationToken cancellationToken)
    {
        foreach (var id in request.Ids)
        {
            var suministro = await _suministroRepository.GetByIdAsync(id);
            if (suministro != null)
                await _suministroRepository.DeleteAsync(suministro);
        }
        return true;
    }
}
