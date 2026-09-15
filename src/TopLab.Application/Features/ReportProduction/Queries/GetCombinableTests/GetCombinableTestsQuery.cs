using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Queries.GetCombinableTests;

public sealed record GetCombinableTestsQuery(int PatientId)
    : IRequest<Result<IReadOnlyList<CombinableTestDto>>>;