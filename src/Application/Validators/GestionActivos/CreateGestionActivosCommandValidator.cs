using FluentValidation;

namespace Application.Validators.GestionActivos;

public class CreateGestionActivosCommandValidator : AbstractValidator<Commands.GestionActivos.CreateGestionActivosCommand>
{
    public CreateGestionActivosCommandValidator()
    {
        RuleFor(x => x.Asignaciones)
            .NotEmpty().WithMessage("Debe incluir al menos una asignación.");

        RuleForEach(x => x.Asignaciones).ChildRules(asignacion =>
        {
            asignacion.RuleFor(a => a.IdEquipo)
                .NotEmpty().WithMessage("El ID del equipo es obligatorio.");

            asignacion.RuleFor(a => a.IdCustodio)
                .GreaterThan(0).WithMessage("El ID del custodio es obligatorio.");
        });
    }
}