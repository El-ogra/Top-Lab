using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;

public sealed record DeleteAntibioticCommand(int Id)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}