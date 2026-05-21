using FluentValidation;

namespace Application.Validators.Personal;

public class CreatePersonalCommandValidator : AbstractValidator<Commands.Personal.CreatePersonalCommand>
{
    public CreatePersonalCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Cedula)
            .NotEmpty().WithMessage("La cédula es obligatoria.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("El cargo es obligatorio.");
    }
}