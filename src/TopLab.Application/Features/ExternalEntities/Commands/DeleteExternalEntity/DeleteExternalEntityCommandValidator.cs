using FluentValidation;

namespace TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;

public sealed class DeleteExternalEntityCommandValidator : AbstractValidator<DeleteExternalEntityCommand>
{
    public DeleteExternalEntityCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("الجهة الخارجية غير موجودة.");
    }
}
