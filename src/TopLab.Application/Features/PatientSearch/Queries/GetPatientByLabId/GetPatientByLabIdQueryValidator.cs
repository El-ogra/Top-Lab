using FluentValidation;

namespace TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;

public sealed class GetPatientByLabIdQueryValidator : AbstractValidator<GetPatientByLabIdQuery>
{
    public GetPatientByLabIdQueryValidator()
    {
        RuleFor(x => x.LabId)
            .NotEmpty().WithMessage("كود المعمل مطلوب.")
            .MaximumLength(30).WithMessage("كود المعمل يجب ألا يتجاوز 30 حرفًا.");
    }
}