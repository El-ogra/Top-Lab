using FluentValidation;

namespace TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDisbursement;

public sealed class RecordCashDisbursementCommandValidator : AbstractValidator<RecordCashDisbursementCommand>
{
    public RecordCashDisbursementCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0m)
            .WithMessage("المبلغ يجب أن يكون أكبر من صفر.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}
