using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;

public sealed class GetPatientResultSheetQueryValidator : AbstractValidator<GetPatientResultSheetQuery>
{
    public GetPatientResultSheetQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
