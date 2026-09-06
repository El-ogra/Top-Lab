using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetWorkGroupLogs;

public sealed record GetWorkGroupLogsQuery() : IRequest<Result<IReadOnlyList<WorkGroupLogDto>>>;