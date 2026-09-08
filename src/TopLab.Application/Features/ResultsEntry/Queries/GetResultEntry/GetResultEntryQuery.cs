using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetResultEntry;

public sealed record GetResultEntryQuery(int PatientTestId) : IRequest<Result<ResultEntryDto>>;
