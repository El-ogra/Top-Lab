using FluentValidation;

namespace TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;
public sealed class SaveCultureResultsCommandValidator : AbstractValidator<SaveCultureResultsCommand>
{
    public SaveCultureResultsCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0); RuleFor(x => x.Sample).MaximumLength(100); RuleFor(x => x.OrganismA).MaximumLength(150); RuleFor(x => x.OrganismB).MaximumLength(150); RuleFor(x => x.OrganismC).MaximumLength(150); RuleFor(x => x.CultureCondition).MaximumLength(200); RuleFor(x => x.ColonyCount).MaximumLength(50);
        RuleForEach(x => x.Sensitivities).Must(x => x.SensitivityCategory is >= 0 and <= 3);
        RuleFor(x => x.Sensitivities).Must(x => x is null || x.Select(y => y.AntibioticId).Distinct().Count() == x.Count).WithMessage("تكرار المضاد الحيوي في نفس النتيجة.");
    }
}
