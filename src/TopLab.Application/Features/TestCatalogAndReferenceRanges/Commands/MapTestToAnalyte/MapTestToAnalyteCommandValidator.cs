using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.MapTestToAnalyte;

public sealed class MapTestToAnalyteCommandValidator : AbstractValidator<MapTestToAnalyteCommand>
{
    public MapTestToAnalyteCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0).WithMessage("معرف التحليل غير صالح.");
        RuleFor(x => x.AnalyteId).GreaterThan(0).WithMessage("معرف المادة التحليلية غير صالح.");
    }
}
