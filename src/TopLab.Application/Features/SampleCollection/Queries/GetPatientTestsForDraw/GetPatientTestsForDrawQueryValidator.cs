using FluentValidation;

namespace TopLab.Application.Features.SampleCollection.Queries.GetPatientTestsForDraw;

public sealed class GetPatientTestsForDrawQueryValidator : AbstractValidator<GetPatientTestsForDrawQuery>
{
    public GetPatientTestsForDrawQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
