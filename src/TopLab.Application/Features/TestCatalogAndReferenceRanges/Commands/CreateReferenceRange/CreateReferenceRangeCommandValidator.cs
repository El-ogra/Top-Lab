using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateReferenceRange;

public sealed class CreateReferenceRangeCommandValidator : AbstractValidator<CreateReferenceRangeCommand>
{
    public CreateReferenceRangeCommandValidator()
    {
        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");

        RuleFor(x => x.AgeMin)
            .GreaterThanOrEqualTo(0).WithMessage("العمر الأدنى يجب ألا يكون سالبًا.");

        RuleFor(x => x.AgeMax)
            .GreaterThanOrEqualTo(x => x.AgeMin).WithMessage("العمر الأقصى يجب ألا يقل عن العمر الأدنى.");

        RuleFor(x => x.MinValue)
            .LessThanOrEqualTo(x => x.MaxValue).WithMessage("القيمة الدنيا يجب ألا تتجاوز القيمة العليا.");

        RuleFor(x => x.AgeUnit)
            .IsInEnum().WithMessage("وحدة العمر غير صالحة.");

        RuleFor(x => x.Sex)
            .IsInEnum().WithMessage("الجنس غير صالح.");

        RuleFor(x => x.LowComment)
            .MaximumLength(500).WithMessage("التعليق يجب ألا يتجاوز 500 حرفًا.");

        RuleFor(x => x.HighComment)
            .MaximumLength(500).WithMessage("التعليق يجب ألا يتجاوز 500 حرفًا.");
    }
}