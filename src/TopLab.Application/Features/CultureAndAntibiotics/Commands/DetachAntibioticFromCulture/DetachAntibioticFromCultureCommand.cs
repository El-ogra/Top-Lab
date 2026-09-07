using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;

public sealed record DetachAntibioticFromCultureCommand(int TestId, int AntibioticId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}