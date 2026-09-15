using FluentValidation;

namespace TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;

public sealed class GetSentOutLabAccountQueryValidator : AbstractValidator<GetSentOutLabAccountQuery>
{
    public GetSentOutLabAccountQueryValidator()
    {
        RuleFor(x => x.ExternalLabEntityId)
            .GreaterThan(0).WithMessage("الجهة الخارجية غير موجودة.");

        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
