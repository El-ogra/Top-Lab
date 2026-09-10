using FluentValidation;

namespace TopLab.Application.Features.PatientSearch.Queries.GetVisitDetail;

public sealed class GetVisitDetailQueryValidator : AbstractValidator<GetVisitDetailQuery>
{
    public GetVisitDetailQueryValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرف المريض غير صالح.");
    }
}