using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetResultEntry;

public sealed class GetResultEntryQueryValidator : AbstractValidator<GetResultEntryQuery>
{
    public GetResultEntryQueryValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
    }
}
