using FluentValidation;

namespace Application.Validators.Suministros;

public class CreateSuministroCommandValidator : AbstractValidator<Commands.Suministros.CreateSuministroCommand>
{
    public CreateSuministroCommandValidator()
    {
        RuleFor(x => x.IdEquipo)
            .NotEmpty().WithMessage("El ID del equipo es obligatorio.");

        RuleFor(x => x.TipoSuministro)
            .NotEmpty().WithMessage("El tipo de suministro es obligatorio.");
    }
}