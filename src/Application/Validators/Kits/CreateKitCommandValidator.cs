using FluentValidation;

namespace Application.Validators.Kits;

public class CreateKitCommandValidator : AbstractValidator<Commands.Kits.CreateKitCommand>
{
    public CreateKitCommandValidator()
    {
        RuleFor(x => x.Insumo)
            .NotEmpty().WithMessage("El insumo es obligatorio.");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es obligatorio.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
    }
}