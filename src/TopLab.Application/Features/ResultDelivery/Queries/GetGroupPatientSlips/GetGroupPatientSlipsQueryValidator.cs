using FluentValidation;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetGroupPatientSlips;

public sealed class GetGroupPatientSlipsQueryValidator : AbstractValidator<GetGroupPatientSlipsQuery>
{
    public GetGroupPatientSlipsQueryValidator()
    {
        RuleFor(x => x.PatientIds)
            .NotEmpty().WithMessage("قائمة المرضى مطلوبة.");

        RuleForEach(x => x.PatientIds)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
