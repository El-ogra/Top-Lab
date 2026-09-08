using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;

public sealed class SettleAccountInFullCommandValidator : AbstractValidator<SettleAccountInFullCommand>
{
    public SettleAccountInFullCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
