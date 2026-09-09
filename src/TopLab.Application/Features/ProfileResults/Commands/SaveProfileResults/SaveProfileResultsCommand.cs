using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ProfileResults.Commands.SaveProfileResults;

public sealed record ProfileItemInput(
    int AnalyteId,
    string ResultValue,
    string? Unit,
    int? Flag);

public sealed record SaveProfileResultsCommand(
    int PatientTestId,
    string? Comment,
    IReadOnlyList<ProfileItemInput> Items)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.EditResults;
}