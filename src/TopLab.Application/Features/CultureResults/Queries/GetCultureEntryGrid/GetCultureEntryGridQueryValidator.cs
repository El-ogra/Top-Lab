using FluentValidation;

namespace TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;

public sealed class GetCultureEntryGridQueryValidator : AbstractValidator<GetCultureEntryGridQuery>
{
    public GetCultureEntryGridQueryValidator() => RuleFor(x => x.PatientTestId).GreaterThan(0);
}
