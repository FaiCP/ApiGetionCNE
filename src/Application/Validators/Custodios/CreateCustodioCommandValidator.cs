using FluentValidation;

namespace Application.Validators.Custodios;

public class CreateCustodioCommandValidator : AbstractValidator<Commands.Custodios.CreateCustodioCommand>
{
    public CreateCustodioCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Cedula)
            .NotEmpty().WithMessage("La cédula es obligatoria.")
            .MaximumLength(20).WithMessage("La cédula no puede exceder 20 caracteres.");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("El cargo es obligatorio.");
    }
}