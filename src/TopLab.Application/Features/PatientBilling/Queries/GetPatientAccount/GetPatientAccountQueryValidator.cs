using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;

public sealed class GetPatientAccountQueryValidator : AbstractValidator<GetPatientAccountQuery>
{
    public GetPatientAccountQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
