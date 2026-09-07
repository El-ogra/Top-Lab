using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;

public sealed class AddCustomGroupToVisitCommandValidator : AbstractValidator<AddCustomGroupToVisitCommand>
{
    public AddCustomGroupToVisitCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.CustomGroupId).GreaterThan(0).WithMessage("معرّف المجموعة غير صالح.");
    }
}