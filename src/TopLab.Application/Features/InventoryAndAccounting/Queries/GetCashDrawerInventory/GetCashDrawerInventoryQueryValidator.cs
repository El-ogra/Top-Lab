using FluentValidation;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetCashDrawerInventory;

public sealed class GetCashDrawerInventoryQueryValidator : AbstractValidator<GetCashDrawerInventoryQuery>
{
    public GetCashDrawerInventoryQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
