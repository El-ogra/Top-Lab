using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;

public sealed class ClearAllTestsCommandValidator : AbstractValidator<ClearAllTestsCommand>
{
    public ClearAllTestsCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرف المريض غير صالح.");
    }
}
