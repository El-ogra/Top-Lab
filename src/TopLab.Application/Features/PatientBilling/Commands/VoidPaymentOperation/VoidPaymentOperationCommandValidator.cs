using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.VoidPaymentOperation;

public sealed class VoidPaymentOperationCommandValidator : AbstractValidator<VoidPaymentOperationCommand>
{
    public VoidPaymentOperationCommandValidator()
    {
        RuleFor(x => x.PaymentOperationId).GreaterThan(0).WithMessage("معرّف العملية غير صالح.");
    }
}
