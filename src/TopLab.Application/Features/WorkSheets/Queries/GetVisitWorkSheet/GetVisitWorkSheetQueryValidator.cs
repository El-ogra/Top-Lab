using FluentValidation;

namespace TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;

public sealed class GetVisitWorkSheetQueryValidator : AbstractValidator<GetVisitWorkSheetQuery>
{
    public GetVisitWorkSheetQueryValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
