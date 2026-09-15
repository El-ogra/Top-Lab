using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Queries.GetMultiPatientHistory;

public sealed record GetMultiPatientHistoryQuery(IReadOnlyList<int> PatientIds)
    : IRequest<Result<MultiPatientHistoryDto>>;