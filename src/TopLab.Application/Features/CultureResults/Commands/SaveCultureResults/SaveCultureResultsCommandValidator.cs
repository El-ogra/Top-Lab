using FluentValidation;

namespace TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;
public sealed class SaveCultureResultsCommandValidator : AbstractValidator<SaveCultureResultsCommand>
{
    public SaveCultureResultsCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0); RuleFor(x => x.Sample).MaximumLength(100); RuleFor(x => x.OrganismA).MaximumLength(150); RuleFor(x => x.OrganismB).MaximumLength(150); RuleFor(x => x.OrganismC).MaximumLength(150); RuleFor(x => x.CultureCondition).MaximumLength(200); RuleFor(x => x.ColonyCount).MaximumLength(50);
        RuleForEach(x => x.Sensitivities).Must(x => x.SensitivityCategory is null or (>= 0 and <= 3));
        RuleForEach(x => x.Sensitivities).Must(x => x.InhibitionZoneMm is null or (>= 0 and <= 100)).WithMessage("منطقة التثبيط يجب أن تكون بين 0 و 100 مم.");
        RuleFor(x => x.Microscopy!.PusCells).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Microscopy!.RedBloodCells).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Microscopy!.EpithelialCells).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Microscopy!.Crystals).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Microscopy!.Fungi).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Microscopy!.OthersOne).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Microscopy!.OthersTwo).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Microscopy!.OthersThree).MaximumLength(20).When(x => x.Microscopy is not null);
        RuleFor(x => x.Sensitivities).Must(x => x is null || x.Select(y => y.AntibioticId).Distinct().Count() == x.Count).WithMessage("تكرار المضاد الحيوي في نفس النتيجة.");
    }
}
