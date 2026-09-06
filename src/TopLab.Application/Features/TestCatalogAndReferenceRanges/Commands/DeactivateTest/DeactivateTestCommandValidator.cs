using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTest;

public sealed class DeactivateTestCommandValidator : AbstractValidator<DeactivateTestCommand>
{
    public DeactivateTestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");
    }
}