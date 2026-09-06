using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;

public sealed class SearchTestCatalogQueryHandler : IRequestHandler<SearchTestCatalogQuery, Result<IReadOnlyList<TestSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public SearchTestCatalogQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<TestSummaryDto>>> Handle(SearchTestCatalogQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<Test>().AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        if (request.TestGroupId is not null)
        {
            var groupIdValue = request.TestGroupId.Value;
            query = query.Where(t => t.TestGroupId != null && t.TestGroupId.Value == groupIdValue);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();

            var matchingGroupIds = _db.Set<TestGroup>()
                .Where(g => request.IncludeInactive || g.IsActive)
                .Where(g => g.Name.Contains(term))
                .Select(g => g.Id.Value)
                .ToList();

            if (matchingGroupIds.Count > 0)
            {
                query = query.Where(t =>
                    t.Name.Contains(term) ||
                    t.ReportName.Contains(term) ||
                    (t.Barcode != null && t.Barcode.Contains(term)) ||
                    t.TestCode.Equals(term) ||
                    (t.TestGroupId != null && matchingGroupIds.Contains(t.TestGroupId.Value)));
            }
            else
            {
                query = query.Where(t =>
                    t.Name.Contains(term) ||
                    t.ReportName.Contains(term) ||
                    (t.Barcode != null && t.Barcode.Contains(term)) ||
                    t.TestCode.Equals(term));
            }
        }

        var tests = query.OrderBy(t => t.Name).ToList();

        var groupNameById = _db.Set<TestGroup>().ToDictionary(g => g.Id, g => g.Name);

        var dtos = tests
            .Select(t => new TestSummaryDto(
                t.Id.Value,
                t.TestCode,
                t.Name,
                t.ReportName,
                t.TestGroupId?.Value,
                t.TestGroupId is not null && groupNameById.TryGetValue(t.TestGroupId, out var groupName) ? groupName : null,
                t.Barcode,
                t.CompletionDurationMinutes,
                t.IsSentOut,
                t.PatientPrice,
                t.LabToLabPrice,
                t.IsActive))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<TestSummaryDto>>.Success(dtos));
    }
}