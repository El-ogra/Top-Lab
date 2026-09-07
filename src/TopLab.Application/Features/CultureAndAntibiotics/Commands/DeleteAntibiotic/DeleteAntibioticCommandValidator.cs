using FluentValidation;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;

public sealed class DeleteAntibioticCommandValidator : AbstractValidator<DeleteAntibioticCommand>
{
    public DeleteAntibioticCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرّف المضاد الحيوي غير صالح.");
    }
}