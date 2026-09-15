using FluentValidation;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;

public sealed class GetElementInventoryQueryValidator : AbstractValidator<GetElementInventoryQuery>
{
    public GetElementInventoryQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
