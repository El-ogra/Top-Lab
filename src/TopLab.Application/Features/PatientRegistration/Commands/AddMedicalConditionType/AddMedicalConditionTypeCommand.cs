using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddMedicalConditionType;

/// <summary>
/// STAGE-01 (baseline 1.2.2.3): adds a row to the medical-history catalogue
/// from the maintenance screen, so that ordinary catalogue changes do not
/// need a migration.
/// </summary>
public sealed record AddMedicalConditionTypeCommand(
    string Name,
    MedicalConditionCategory Category)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
