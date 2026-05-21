using FluentValidation;

namespace Application.Validators.Hardware;

public class CreateHardwareCommandValidator : AbstractValidator<Commands.Hardware.CreateHardwareCommand>
{
    public CreateHardwareCommandValidator()
    {
        RuleFor(x => x.IdEquipo)
            .NotEmpty().WithMessage("El ID del equipo es obligatorio.");

        RuleFor(x => x.NombreDispositivo)
            .NotEmpty().WithMessage("El nombre del dispositivo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del dispositivo no puede exceder 100 caracteres.");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("La marca es obligatoria.");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es obligatorio.");

        RuleFor(x => x.Ubicacion)
            .NotEmpty().WithMessage("La ubicación es obligatoria.");

        RuleFor(x => x.Valor)
            .GreaterThanOrEqualTo(0).When(x => x.Valor.HasValue)
            .WithMessage("El valor debe ser positivo.");
    }
}