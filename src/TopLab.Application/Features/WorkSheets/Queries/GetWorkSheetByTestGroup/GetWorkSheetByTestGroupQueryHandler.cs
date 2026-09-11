using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;

public sealed class GetWorkSheetByTestGroupQueryHandler
    : IRequestHandler<GetWorkSheetByTestGroupQuery, Result<WorkSheetDto>>
{
    private const int UngroupedSectionId = 0;
    private const string UngroupedSectionName = "بدون مجموعة";

    private readonly IApplicationDbContext _db;

    public GetWorkSheetByTestGroupQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<WorkSheetDto>> Handle(
        GetWorkSheetByTestGroupQuery request, CancellationToken cancellationToken)
    {
        var (from, to) = WorkSheetPeriod.Resolve(request.From, request.To);

        var testsById = _db.Set<Test>().ToDictionary(t => t.Id.Value);
        var groupsById = _db.Set<TestGroup>().ToDictionary(g => g.Id.Value);

        List<(int SectionId, string SectionName, HashSet<int> TestIds)> sectionDefs;
        if (request.TestIds is { Count: > 0 })
        {
            var selected = request.TestIds.Distinct().ToList();
            foreach (var testId in selected)
            {
                if (!testsById.ContainsKey(testId))
                {
                    return Task.FromResult(Result<WorkSheetDto>.Failure(Error.NotFound("التحليل غير موجود")));
                }
            }

            sectionDefs = selected
                .Select(id => testsById[id])
                .Where(t => t.TestGroupId != null)
                .GroupBy(t => t.TestGroupId!.Value)
                .Select(g => (
                    SectionId: g.Key,
                    SectionName: groupsById.TryGetValue(g.Key, out var group) ? group.Name : UngroupedSectionName,
                    TestIds: g.Select(t => t.Id.Value).ToHashSet()))
                .OrderBy(d => d.SectionName)
                .ToList();

            var ungrouped = selected
                .Select(id => testsById[id])
                .Where(t => t.TestGroupId == null)
                .Select(t => t.Id.Value)
                .ToHashSet();
            if (ungrouped.Count > 0)
            {
                sectionDefs.Add((UngroupedSectionId, UngroupedSectionName, ungrouped));
            }
        }
        else if (request.TestGroupId.HasValue)
        {
            if (!groupsById.TryGetValue(request.TestGroupId.Value, out var group))
            {
                return Task.FromResult(Result<WorkSheetDto>.Failure(Error.NotFound("مجموعة التحاليل غير موجودة.")));
            }

            sectionDefs =
            [
                (group.Id.Value, group.Name, testsById.Values
                    .Where(t => t.TestGroupId != null && t.TestGroupId.Value == group.Id.Value)
                    .Select(t => t.Id.Value)
                    .ToHashSet()),
            ];
        }
        else
        {
            sectionDefs = groupsById.Values
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .Select(g => (g.Id.Value, g.Name, testsById.Values
                    .Where(t => t.TestGroupId != null && t.TestGroupId.Value == g.Id.Value)
                    .Select(t => t.Id.Value)
                    .ToHashSet()))
                .ToList();
            sectionDefs.Add((UngroupedSectionId, UngroupedSectionName, testsById.Values
                .Where(t => t.TestGroupId == null)
                .Select(t => t.Id.Value)
                .ToHashSet()));
        }

        var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<WorkSheetDto>.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود.")));
        }

        var sections = sectionDefs
            .Select(def => new WorkSheetSectionDto(
                def.SectionId,
                def.SectionName,
                WorkSheetLines.Select(_db, def.TestIds, from, to)))
            .ToList();

        return Task.FromResult(Result<WorkSheetDto>.Success(new WorkSheetDto(
            from,
            to,
            "TestGroup",
            sections,
            sections.Sum(s => s.Lines.Count),
            settings.PrintFileExternalBarcode,
            settings.PrintDateTimeOnTubeBarcode,
            settings.PrintLabIdInsteadOfPatientId)));
    }
}
