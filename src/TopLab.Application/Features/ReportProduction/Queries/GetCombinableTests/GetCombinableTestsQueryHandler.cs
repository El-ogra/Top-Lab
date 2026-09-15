using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ReportProduction.Queries.GetCombinableTests;

public sealed class GetCombinableTestsQueryHandler
    : IRequestHandler<GetCombinableTestsQuery, Result<IReadOnlyList<CombinableTestDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCombinableTestsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<CombinableTestDto>>> Handle(
        GetCombinableTestsQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<IReadOnlyList<CombinableTestDto>>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var catalog = _db.Set<Test>().ToDictionary(t => t.Id.Value);

        var rows = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == request.PatientId && pt.IsReviewed)
            .ToList();

        IReadOnlyList<CombinableTestDto> items = rows
            .OrderBy(pt => pt.Id.Value)
            .Select(pt =>
            {
                catalog.TryGetValue(pt.TestId.Value, out var test);
                return new CombinableTestDto(
                    pt.Id.Value,
                    pt.TestId.Value,
                    test?.Name ?? string.Empty,
                    test?.TestCode ?? string.Empty,
                    test == null ? 0 : (int)test.ResultKind);
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CombinableTestDto>>.Success(items));
    }
}