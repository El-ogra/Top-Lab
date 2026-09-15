using FluentValidation;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;

public sealed class GetPatientTestAuditQueryValidator
    : AbstractValidator<GetPatientTestAuditQuery>
{
    public GetPatientTestAuditQueryValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرف غير صالح.");
    }
}
