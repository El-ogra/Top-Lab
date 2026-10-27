using FluentValidation;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetAuditActivity;

public sealed class GetAuditActivityQueryValidator
    : AbstractValidator<GetAuditActivityQuery>
{
    public GetAuditActivityQueryValidator()
    {
        RuleFor(x => x).Must(x => x.From <= x.To).WithMessage("الفترة غير صالحة.");
        RuleFor(x => x.UserIds).Must(ids => ids is null || ids.All(id => id >= 0))
            .WithMessage("معرف المستخدم غير صالح.");
    }
}