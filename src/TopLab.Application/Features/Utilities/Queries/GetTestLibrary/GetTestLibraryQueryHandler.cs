using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.Utilities.Queries.GetTestLibrary;

public sealed class GetTestLibraryQueryHandler
    : IRequestHandler<GetTestLibraryQuery, Result<IReadOnlyList<TestLibraryEntryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetTestLibraryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<TestLibraryEntryDto>>> Handle(
        GetTestLibraryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TestGroupId.HasValue
            && !_db.Set<TestGroup>().Any(g => g.Id.Value == request.TestGroupId.Value))
        {
            return Task.FromResult(Result<IReadOnlyList<TestLibraryEntryDto>>.Failure(
                Error.NotFound("مجموعة التحاليل غير موجودة.", "NotFound")));
        }

        var query = _db.Set<Test>().AsQueryable();
        if (request.TestGroupId.HasValue)
        {
            var groupId = request.TestGroupId.Value;
            query = query.Where(t => t.TestGroupId != null && t.TestGroupId.Value == groupId);
        }

        if (!string.IsNullOrWhiteSpace(request.NameFilter))
        {
            var filter = request.NameFilter.Trim();
            query = query.Where(t => t.Name.Contains(filter));
        }

        var tests = query
            .OrderBy(t => t.Name)
            .ToList();

        var groupIds = tests
            .Where(t => t.TestGroupId is not null)
            .Select(t => t.TestGroupId!.Value)
            .Distinct()
            .ToList();
        var groups = _db.Set<TestGroup>()
            .Where(g => groupIds.Contains(g.Id.Value))
            .ToDictionary(g => g.Id.Value, g => g.Name);

        IReadOnlyList<TestLibraryEntryDto> rows = tests
            .Select(t =>
            {
                string? groupName = null;
                if (t.TestGroupId is not null)
                {
                    groupName = groups.TryGetValue(t.TestGroupId.Value, out var name)
                        ? name
                        : t.TestGroupId.Value.ToString();
                }

                return new TestLibraryEntryDto(t.Id.Value, t.Name, t.TestCode, groupName);
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<TestLibraryEntryDto>>.Success(rows));
    }
}
