using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientRegistration.Commands.RenameMedicalConditionType;

/// <summary>
/// STAGE-01 (baseline 1.2.2.3): renames a catalogue row from the maintenance
/// screen. Seeded rows are refused — they belong to the migration's
/// <c>HasData</c> snapshot and must not drift from it.
/// </summary>
public sealed record RenameMedicalConditionTypeCommand(
    int MedicalConditionTypeId,
    string Name)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
