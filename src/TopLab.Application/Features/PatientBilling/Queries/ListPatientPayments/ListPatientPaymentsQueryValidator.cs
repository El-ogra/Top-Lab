using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;

public sealed class ListPatientPaymentsQueryValidator : AbstractValidator<ListPatientPaymentsQuery>
{
    public ListPatientPaymentsQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500).WithMessage("معاملات الترقيم غير صالحة.");
    }
}
