using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;

public sealed record GetPatientTestHistoryQuery(
    int PatientId,
    DateOnly? FromUtc = null,
    DateOnly? ToUtc = null,
    int? TestId = null)
    : IRequest<Result<PatientHistoryDto>>;