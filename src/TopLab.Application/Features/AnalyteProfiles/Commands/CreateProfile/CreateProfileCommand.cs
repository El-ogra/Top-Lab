using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;

public sealed record CreateProfileCommand(
    string Name,
    int SpecializedTestId,
    decimal FixedPrice,
    IReadOnlyList<int> AnalyteIds)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}