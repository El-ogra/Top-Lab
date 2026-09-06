using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class TestDeletionCascadeTests
{
    private static IForeignKey? FkToPrincipal<TDependent>(System.Type principal) where TDependent : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var dependent = ctx.Model.FindEntityType(typeof(TDependent));
        return dependent?.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == principal)
            .FirstOrDefault();
    }

    [Fact]
    public void DeletingTest_CascadesToReferenceRanges()
    {
        var fk = FkToPrincipal<ReferenceRange>(typeof(Test));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void DeletingWorkGroupLog_CascadesToWorkGroupLogItems()
    {
        var fk = FkToPrincipal<WorkGroupLogItem>(typeof(WorkGroupLog));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void DeletingTest_IsRestrictedForPatientTestHistory()
    {
        var fk = FkToPrincipal<TopLab.Domain.Results.PatientTest>(typeof(Test));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    // Baseline observation (deviation waived by owner for M-12): WorkGroupLogItem has
    // NO FK relationship to Test — the configuration deliberately suppresses it
    // ("removed explicit HasOne to avoid shadow") and the F5 baseline migration only
    // created the WorkGroupLogs FK. Adding Test->WorkGroupLogItem cascade would exceed
    // the locked M-12 migration scope (ADR-0028/0029 touch only Tests + TestGroups),
    // so this test asserts the absence to guard the documented baseline gap instead.
    [Fact]
    public void WorkGroupLogItem_HasNoRelationshipToTest_BaselineGapDocumented()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var dependent = ctx.Model.FindEntityType(typeof(WorkGroupLogItem));
        Assert.NotNull(dependent);
        Assert.DoesNotContain(
            dependent!.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Test));
    }
}