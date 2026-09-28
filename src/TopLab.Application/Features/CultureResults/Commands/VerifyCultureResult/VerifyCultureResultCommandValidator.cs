using FluentValidation;

namespace TopLab.Application.Features.CultureResults.Commands.VerifyCultureResult;

public sealed class VerifyCultureResultCommandValidator : AbstractValidator<VerifyCultureResultCommand>
{
    public VerifyCultureResultCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرف تحليل المريض غير صالح.");
    }
}
