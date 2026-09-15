using FluentValidation;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetPatientSamplesDetail;

public sealed class GetPatientSamplesDetailQueryValidator : AbstractValidator<GetPatientSamplesDetailQuery>
{
    public GetPatientSamplesDetailQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
