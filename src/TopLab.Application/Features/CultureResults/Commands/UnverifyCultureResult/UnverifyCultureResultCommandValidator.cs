using FluentValidation;

namespace TopLab.Application.Features.CultureResults.Commands.UnverifyCultureResult;

public sealed class UnverifyCultureResultCommandValidator : AbstractValidator<UnverifyCultureResultCommand>
{
    public UnverifyCultureResultCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرف تحليل المريض غير صالح.");
    }
}
