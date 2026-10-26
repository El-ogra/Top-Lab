using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;

/// <summary>
/// Clears the patient's account (canonical reference 1.3.2.7, «تصفير الحساب»).
/// <para>
/// Voids every non-voided operation of that patient through the existing
/// <c>IsVoided</c>/<c>Void()</c> mechanism. Nothing is hard-deleted, the rows and the audit
/// trail stay, and other patients are untouched. Idempotent: when nothing remains to void the
/// command still succeeds. Ungated like <c>RecordPaymentCommand</c> (no new permission code).
/// </para>
/// </summary>
public sealed record ClearPatientAccountCommand(
    int PatientId) : IRequest<Result<int>>;
