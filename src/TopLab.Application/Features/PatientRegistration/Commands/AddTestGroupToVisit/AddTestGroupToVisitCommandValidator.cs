using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddTestGroupToVisit;

public sealed class AddTestGroupToVisitCommandValidator : AbstractValidator<AddTestGroupToVisitCommand>
{
    public AddTestGroupToVisitCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.TestGroupId).GreaterThan(0);
    }
}