using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.SaveAnalyteReferenceRange;

public sealed class SaveAnalyteReferenceRangeCommandValidator : AbstractValidator<SaveAnalyteReferenceRangeCommand>
{
    public SaveAnalyteReferenceRangeCommandValidator()
    {
        RuleFor(x => x.AnalyteId).GreaterThan(0).WithMessage("معرّف المادة التحليلية غير صالح.");

        RuleForEach(x => x.Bands)
            .ChildRules(b =>
            {
                b.RuleFor(x => x.AgeMax).GreaterThanOrEqualTo(x => x.AgeMin).WithMessage("الحد الأقصى للعمر لا يمكن أن يكون أصغر من الحد الأدنى.");
                b.RuleFor(x => x.MaxValue).GreaterThanOrEqualTo(x => x.MinValue).WithMessage("القيمة العليا لا يمكن أن تكون أصغر من القيمة الدنيا.");
            });

        RuleFor(x => x.Bands)
            .Must(bands => bands is null || bands.Count == 0 || bands.Count(b => b.AgeUnit == AgeUnit.Year) <= 100)
            .WithMessage("عدد النطاقات كبير جداً.");
    }
}