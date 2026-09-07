using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.RemoveTestFromVisit;

public sealed class RemoveTestFromVisitCommandValidator : AbstractValidator<RemoveTestFromVisitCommand>
{
    public RemoveTestFromVisitCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف تحليل المريض غير صالح.");
    }
}