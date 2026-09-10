using TopLab.Application.Features.PatientSearch.Queries.GetVisitHistory;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

/// <summary>
/// M-08 infrastructure proof on the real <see cref="ApplicationDbContext"/> over the
/// InMemory provider: two registrations sharing a LabId plus an unrelated registration
/// (different LabId) persist and reload, and the visit-history query returns exactly the
/// two siblings — the unrelated registration is excluded by the LabId row-grouping rule.
/// </summary>
public class PatientSearchPersistenceTests
{
    [Fact]
    public async Task VisitHistory_OnRealContext_GroupsOnlyLabIdSiblings()
    {
        var options = InMemoryContextFactory.Create();
        await using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        var visit1 = Patient.Create(
            PatientId.Create(1), "Sibling One", Sex.Male, 30, AgeUnit.Year,
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        visit1.AssignLabId(LabId.Create("LAB-100"));

        var visit2 = Patient.Create(
            PatientId.Create(2), "Sibling Two", Sex.Female, 41, AgeUnit.Year,
            new DateTime(2020, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        visit2.AssignLabId(LabId.Create("LAB-100"));

        var unrelated = Patient.Create(
            PatientId.Create(3), "Unrelated", Sex.Female, 22, AgeUnit.Year,
            new DateTime(2020, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        unrelated.AssignLabId(LabId.Create("LAB-200"));

        ctx.Patients.AddRange(visit1, visit2, unrelated);
        await ctx.SaveChangesAsync();

        var handler = new GetVisitHistoryQueryHandler(ctx);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var history = result.Value!;
        Assert.Equal("LAB-100", history.LabId);
        Assert.Equal("Sibling One", history.PatientFullName);
        Assert.Equal(2, history.Visits.Count);
        Assert.Equal(2, history.Visits[0].PatientId);
        Assert.Equal(1, history.Visits[1].PatientId);
        Assert.All(history.Visits, v => Assert.Equal("LAB-100", v.LabId));
        Assert.DoesNotContain(history.Visits, v => v.PatientId == 3);
    }
}