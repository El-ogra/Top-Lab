using FluentValidation;

namespace TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDeposit;

public sealed class RecordCashDepositCommandValidator : AbstractValidator<RecordCashDepositCommand>
{
    public RecordCashDepositCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0m)
            .WithMessage("المبلغ يجب أن يكون أكبر من صفر.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}
