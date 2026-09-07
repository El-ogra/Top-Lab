using FluentValidation;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;

public sealed class AttachAntibioticToCultureCommandValidator
    : AbstractValidator<AttachAntibioticToCultureCommand>
{
    public AttachAntibioticToCultureCommandValidator()
    {
        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");

        RuleFor(x => x.AntibioticId)
            .GreaterThan(0).WithMessage("معرّف المضاد الحيوي غير صالح.");
    }
}