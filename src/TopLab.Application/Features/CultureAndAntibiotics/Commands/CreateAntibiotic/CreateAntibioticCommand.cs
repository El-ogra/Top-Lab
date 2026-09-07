using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;

public sealed record CreateAntibioticCommand(
    string Name,
    bool IsPregnancyFlagged,
    bool IsChildrenFlagged)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}