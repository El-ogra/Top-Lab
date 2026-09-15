using FluentValidation;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientAudit;

public sealed class GetPatientAuditQueryValidator
    : AbstractValidator<GetPatientAuditQuery>
{
    public GetPatientAuditQueryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرف غير صالح.");
    }
}
