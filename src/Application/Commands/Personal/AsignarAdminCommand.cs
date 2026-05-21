using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.Personal;

public record AsignarAdminCommand(long PersonalId) : IRequest<bool>;

public class AsignarAdminCommandHandler : IRequestHandler<AsignarAdminCommand, bool>
{
    private readonly IPersonalRepository _personalRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public AsignarAdminCommandHandler(
        IPersonalRepository personalRepository,
        IUsuarioRepository usuarioRepository)
    {
        _personalRepository = personalRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<bool> Handle(AsignarAdminCommand request, CancellationToken cancellationToken)
    {
        var persona = await _personalRepository.GetByIdAsync(request.PersonalId)
            ?? throw new NotFoundException($"Personal con ID {request.PersonalId} no encontrado.");

        var nuevoAdmin = await _usuarioRepository.GetByEmailAsync(persona.Email)
            ?? throw new NotFoundException($"No existe un usuario registrado con email '{persona.Email}'.");

        // Revocar rol a todos los admins actuales (solo uno activo a la vez)
        var adminsActuales = await _usuarioRepository.GetAsync(
            u => (u.Rol == "Administrador" || u.Rol == "Admin") && u.Id != nuevoAdmin.Id);

        foreach (var admin in adminsActuales)
        {
            admin.Rol = "User";
            admin.UpdatedAt = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(admin);
        }

        if (nuevoAdmin.Rol == "Administrador")
            return true;

        nuevoAdmin.Rol = "Administrador";
        nuevoAdmin.UpdatedAt = DateTime.UtcNow;
        await _usuarioRepository.UpdateAsync(nuevoAdmin);

        return true;
    }
}
