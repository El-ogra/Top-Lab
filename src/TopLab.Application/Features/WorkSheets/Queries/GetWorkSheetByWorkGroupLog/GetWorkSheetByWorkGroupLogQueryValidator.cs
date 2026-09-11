using FluentValidation;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;

public sealed class GetWorkSheetByWorkGroupLogQueryValidator
    : AbstractValidator<GetWorkSheetByWorkGroupLogQuery>
{
    public GetWorkSheetByWorkGroupLogQueryValidator()
    {
        RuleFor(x => x.WorkGroupLogId)
            .GreaterThan(0).WithMessage("معرف سجل مجموعة العمل غير صالح.");

        RuleFor(x => x)
            .Must(q => WorkSheetPeriod.IsValid(q.From, q.To))
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
