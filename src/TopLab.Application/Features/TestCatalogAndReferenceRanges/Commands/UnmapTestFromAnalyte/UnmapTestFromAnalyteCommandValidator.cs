using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UnmapTestFromAnalyte;

public sealed class UnmapTestFromAnalyteCommandValidator : AbstractValidator<UnmapTestFromAnalyteCommand>
{
    public UnmapTestFromAnalyteCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0).WithMessage("معرف التحليل غير صالح.");
    }
}
