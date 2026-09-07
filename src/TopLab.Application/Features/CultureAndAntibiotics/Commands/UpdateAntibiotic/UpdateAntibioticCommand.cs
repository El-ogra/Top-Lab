using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;

public sealed record UpdateAntibioticCommand(
    int Id,
    string Name,
    bool IsPregnancyFlagged,
    bool IsChildrenFlagged)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}