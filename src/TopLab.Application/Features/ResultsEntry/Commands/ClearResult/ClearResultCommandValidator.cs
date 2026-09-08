using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.ClearResult;

public sealed class ClearResultCommandValidator : AbstractValidator<ClearResultCommand>
{
    public ClearResultCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
    }
}
