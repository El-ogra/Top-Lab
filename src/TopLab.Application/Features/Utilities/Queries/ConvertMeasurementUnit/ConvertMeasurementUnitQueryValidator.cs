using FluentValidation;

namespace TopLab.Application.Features.Utilities.Queries.ConvertMeasurementUnit;

public sealed class ConvertMeasurementUnitQueryValidator : AbstractValidator<ConvertMeasurementUnitQuery>
{
    public ConvertMeasurementUnitQueryValidator()
    {
        RuleFor(x => x.FromUnit)
            .NotEmpty()
            .WithMessage("زوج الوحدات غير مدعوم.");

        RuleFor(x => x.ToUnit)
            .NotEmpty()
            .WithMessage("زوج الوحدات غير مدعوم.");
    }
}
