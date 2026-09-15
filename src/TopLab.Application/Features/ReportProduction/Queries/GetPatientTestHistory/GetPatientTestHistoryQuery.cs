using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;

public sealed record GetPatientTestHistoryQuery(int PatientId)
    : IRequest<Result<PatientHistoryDto>>;