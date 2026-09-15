using FluentValidation;

namespace TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;

public sealed class GetSentOutSamplesQueryValidator : AbstractValidator<GetSentOutSamplesQuery>
{
    public GetSentOutSamplesQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("معاملات الترقيم غير صالحة.");
    }
}
