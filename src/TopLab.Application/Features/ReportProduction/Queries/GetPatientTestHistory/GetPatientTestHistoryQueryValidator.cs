using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;

public sealed class GetPatientTestHistoryQueryValidator : AbstractValidator<GetPatientTestHistoryQuery>
{
    public GetPatientTestHistoryQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
    }
}