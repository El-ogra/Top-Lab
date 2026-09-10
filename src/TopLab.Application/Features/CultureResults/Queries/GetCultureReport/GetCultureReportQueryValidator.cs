using FluentValidation;

namespace TopLab.Application.Features.CultureResults.Queries.GetCultureReport;

public sealed class GetCultureReportQueryValidator : AbstractValidator<GetCultureReportQuery>
{
    public GetCultureReportQueryValidator() => RuleFor(x => x.PatientTestId).GreaterThan(0);
}
