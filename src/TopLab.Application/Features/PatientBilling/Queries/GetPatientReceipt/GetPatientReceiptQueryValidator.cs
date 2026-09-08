using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;

public sealed class GetPatientReceiptQueryValidator : AbstractValidator<GetPatientReceiptQuery>
{
    public GetPatientReceiptQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
