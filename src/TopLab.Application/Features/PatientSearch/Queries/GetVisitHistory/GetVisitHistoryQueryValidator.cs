using FluentValidation;

namespace TopLab.Application.Features.PatientSearch.Queries.GetVisitHistory;

public sealed class GetVisitHistoryQueryValidator : AbstractValidator<GetVisitHistoryQuery>
{
    public GetVisitHistoryQueryValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرف المريض غير صالح.");
    }
}