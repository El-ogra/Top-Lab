using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;

public sealed class GetResultWorklistQueryValidator : AbstractValidator<GetResultWorklistQuery>
{
    public GetResultWorklistQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500).WithMessage("معاملات الترقيم غير صالحة.");

        When(x => x.TestGroupId.HasValue, () =>
        {
            RuleFor(x => x.TestGroupId!.Value)
                .GreaterThan(0).WithMessage("معرّف مجموعة التحاليل غير صالح.");
        });

        When(x => x.ResultKind.HasValue, () =>
        {
            RuleFor(x => x.ResultKind!.Value)
                .InclusiveBetween(0, 2).WithMessage("نوع النتيجة غير صالح.");
        });
    }
}
