using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;

/// <summary>
/// Prints the laboratory-order (requisition) slip for a visit from the
/// registration screen (<c>طلب تحاليل</c>). Gated on <c>ADD_EDIT_PATIENT</c>,
/// mirroring <c>PrintBarcodeCommand</c> at the same desk.
/// </summary>
public sealed record PrintLabOrderCommand(int PatientId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}
