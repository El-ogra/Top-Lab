using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;

public sealed record CreateProfileCommand(
    string Name,
    int SpecializedTestId,
    decimal FixedPrice,
    IReadOnlyList<int> AnalyteIds,
    // GAP-017: optional per-profile result styling. Null on any of the four keeps the
    // existing behaviour (global colours for abnormal, no colour for normal results).
    string? NormalFont = null,
    string? NormalColor = null,
    string? AbnormalFont = null,
    string? AbnormalBackgroundColor = null)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}