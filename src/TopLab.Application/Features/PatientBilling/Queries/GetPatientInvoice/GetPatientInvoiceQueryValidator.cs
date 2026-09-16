using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientInvoice;

public sealed class GetPatientInvoiceQueryValidator : AbstractValidator<GetPatientInvoiceQuery>
{
    public GetPatientInvoiceQueryValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
