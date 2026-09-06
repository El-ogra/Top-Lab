using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTest;

public sealed class ReactivateTestCommandValidator : AbstractValidator<ReactivateTestCommand>
{
    public ReactivateTestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");
    }
}