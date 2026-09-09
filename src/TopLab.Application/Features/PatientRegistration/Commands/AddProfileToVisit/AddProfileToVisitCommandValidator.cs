using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddProfileToVisit;

public sealed class AddProfileToVisitCommandValidator : AbstractValidator<AddProfileToVisitCommand>
{
    public AddProfileToVisitCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.ProfileId).GreaterThan(0).WithMessage("معرّف البروفايل غير صالح.");
    }
}