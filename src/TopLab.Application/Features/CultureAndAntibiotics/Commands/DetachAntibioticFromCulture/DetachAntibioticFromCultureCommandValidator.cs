using FluentValidation;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;

public sealed class DetachAntibioticFromCultureCommandValidator
    : AbstractValidator<DetachAntibioticFromCultureCommand>
{
    public DetachAntibioticFromCultureCommandValidator()
    {
        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");

        RuleFor(x => x.AntibioticId)
            .GreaterThan(0).WithMessage("معرّف المضاد الحيوي غير صالح.");
    }
}