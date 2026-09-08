using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Commands.EnterResult;

public sealed record EnterResultCommand(
    int PatientTestId,
    string? ResultValue,
    int? ResultFlag = null,
    string? Notes = null) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.EditResults;
}
