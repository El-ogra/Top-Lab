using FluentValidation;

namespace TopLab.Application.Features.SampleCollection.Queries.GetPatientsWithUncollectedSamples;

public sealed class GetPatientsWithUncollectedSamplesQueryValidator : AbstractValidator<GetPatientsWithUncollectedSamplesQuery>
{
    public GetPatientsWithUncollectedSamplesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500).WithMessage("معاملات الترقيم غير صالحة.");
    }
}
