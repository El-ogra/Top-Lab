using FluentValidation;

namespace TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;

public sealed class GenerateEntityIdCodeCommandValidator : AbstractValidator<GenerateEntityIdCodeCommand>
{
    public GenerateEntityIdCodeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("الجهة الخارجية غير موجودة.");
    }
}
