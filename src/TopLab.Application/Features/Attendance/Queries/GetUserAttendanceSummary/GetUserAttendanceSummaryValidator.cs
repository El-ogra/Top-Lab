using FluentValidation;

namespace TopLab.Application.Features.Attendance.Queries.GetUserAttendanceSummary;

public sealed class GetUserAttendanceSummaryValidator : AbstractValidator<GetUserAttendanceSummaryQuery>
{
    public GetUserAttendanceSummaryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("المستخدم غير موجود.");

        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
