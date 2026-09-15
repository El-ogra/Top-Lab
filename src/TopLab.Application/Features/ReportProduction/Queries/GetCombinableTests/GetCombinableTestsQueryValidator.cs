using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Queries.GetCombinableTests;

public sealed class GetCombinableTestsQueryValidator : AbstractValidator<GetCombinableTestsQuery>
{
    public GetCombinableTestsQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
    }
}