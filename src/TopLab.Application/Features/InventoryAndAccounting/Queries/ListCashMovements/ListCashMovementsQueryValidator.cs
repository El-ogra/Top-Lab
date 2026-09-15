using FluentValidation;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.ListCashMovements;

public sealed class ListCashMovementsQueryValidator : AbstractValidator<ListCashMovementsQuery>
{
    public ListCashMovementsQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
