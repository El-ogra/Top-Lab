using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public sealed record ExecuteBulkPrintCommand(IReadOnlyList<BulkPrintDecision> Decisions)
    : IRequest<Result<IReadOnlyList<BulkPrintOutcomeDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.PrintResults;
}
