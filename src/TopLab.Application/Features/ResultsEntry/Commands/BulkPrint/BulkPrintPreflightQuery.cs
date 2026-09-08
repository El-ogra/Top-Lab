using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public sealed record BulkPrintPreflightQuery(IReadOnlyList<int> PatientIds)
    : IRequest<Result<IReadOnlyList<BulkPrintPreflightDto>>>;
