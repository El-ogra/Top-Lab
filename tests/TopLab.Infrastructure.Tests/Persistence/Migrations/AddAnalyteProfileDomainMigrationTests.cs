using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TopLab.Infrastructure.Persistence.Migrations;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Migrations;

public class AddAnalyteProfileDomainMigrationTests
{
    private static IReadOnlyList<MigrationOperation> RunUp()
    {
        var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        var method = typeof(AddAnalyteProfileDomain).GetMethod(
            "Up",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);
        method!.Invoke(new AddAnalyteProfileDomain(), new object[] { builder });
        return builder.Operations;
    }

    private static IReadOnlyList<MigrationOperation> RunDown()
    {
        var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        var method = typeof(AddAnalyteProfileDomain).GetMethod(
            "Down",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);
        method!.Invoke(new AddAnalyteProfileDomain(), new object[] { builder });
        return builder.Operations;
    }

    private static IEnumerable<SqlOperation> Sqls(IEnumerable<MigrationOperation> ops) =>
        ops.OfType<SqlOperation>();

    private static SqlOperation RequireSql(IEnumerable<MigrationOperation> ops, Func<string, bool> predicate)
    {
        var sql = Sqls(ops).SingleOrDefault(o => predicate(o.Sql));
        Assert.NotNull(sql);
        return sql!;
    }

    private static int GetIndex(IReadOnlyList<MigrationOperation> ops, MigrationOperation target)
    {
        for (var i = 0; i < ops.Count; i++)
        {
            if (ReferenceEquals(ops[i], target))
            {
                return i;
            }
        }
        Assert.Fail("Operation not found in migration.");
        return -1;
    }

    [Fact]
    public void Up_AddsAnalyteIdToProfileResultItems_NullableForBackfill()
    {
        var ops = RunUp();
        var add = ops.OfType<AddColumnOperation>().Single(o =>
            o.Table == "ProfileResultItems" && o.Name == "AnalyteId");
        Assert.True(add.IsNullable);
    }

    [Fact]
    public void Up_BackfillRunsBeforeAnalyteNameIsDropped()
    {
        var ops = RunUp();
        var backfillUpdate = ops.OfType<SqlOperation>().Single(o => o.Sql.Contains("WHERE AnalyteId IS NULL"));
        var drop = ops.OfType<DropColumnOperation>().Single(o =>
            o.Table == "ProfileResultItems" && o.Name == "AnalyteName");
        Assert.True(GetIndex(ops, backfillUpdate) < GetIndex(ops, drop),
            "The stop-guard (and therefore the whole backfill) must execute before AnalyteName is dropped.");
    }

    [Fact]
    public void Up_BackfillTightensAnalyteIdToNotNullBeforeDroppingName()
    {
        var ops = RunUp();
        var alter = RequireSql(ops, s => s.Contains("ALTER TABLE ProfileResultItems ALTER COLUMN AnalyteId int NOT NULL"));
        var backfillUpdate = Sqls(ops).Single(o => o.Sql.Contains("SET pri.AnalyteId = a.AnalyteId"));
        var drop = ops.OfType<DropColumnOperation>().Single(o => o.Name == "AnalyteName");
        Assert.True(
            GetIndex(ops, backfillUpdate) < GetIndex(ops, alter) && GetIndex(ops, alter) < GetIndex(ops, drop));
    }

    [Fact]
    public void Up_BackfillCreatesAnalytePerSimpleTestAndProfilesPerSpecialisedTest()
    {
        var ops = RunUp();
        var analyteInsert = RequireSql(ops, s => s.Contains("INSERT INTO Analytes"));
        Assert.Contains("WHERE t.ResultKind = 0", analyteInsert.Sql);
        Assert.Contains("MIN(t.ReportName)", analyteInsert.Sql);

        var testMap = RequireSql(ops, s => s.Contains("SET t.AnalyteId"));
        Assert.Contains("WHERE t.ResultKind = 0", testMap.Sql);

        var rangeInsert = RequireSql(ops, s => s.Contains("INSERT INTO AnalyteReferenceRanges"));
        var bandInsert = RequireSql(ops, s => s.Contains("INSERT INTO AnalyteReferenceRangeBands"));
        Assert.Contains("FROM ReferenceRanges", bandInsert.Sql);
        Assert.Contains("ResultKind = 0", bandInsert.Sql);

        var profileInsert = RequireSql(ops, s => s.Contains("INSERT INTO Profiles"));
        Assert.Contains("t.PatientPrice", profileInsert.Sql);
        Assert.Contains("WHERE t.ResultKind = 1", profileInsert.Sql);
    }

    [Fact]
    public void Up_MapsLegacyProfileResultItemsOnlyByDeterministicConfiguredNameMatch()
    {
        var ops = RunUp();
        var map = RequireSql(ops, s => s.Contains("SET pri.AnalyteId = a.AnalyteId"));
        Assert.Contains("ON a.Name = pri.AnalyteName", map.Sql);
    }

    [Fact]
    public void Up_StopsOnUnmappedLegacyProfileResultItem()
    {
        var ops = RunUp();
        RequireSql(ops, s => s.Contains("THROW 50001") && s.Contains("WHERE AnalyteId IS NULL"));
    }

    [Fact]
    public void Up_AddsRestrictForeignKeysOnAnalyteId()
    {
        var ops = RunUp();
        var fks = ops.OfType<AddForeignKeyOperation>()
            .Where(o => o.Columns.SequenceEqual(["AnalyteId"])).ToList();
        Assert.Contains(fks, o => o.Table == "ProfileResultItems" && o.PrincipalTable == "Analytes");
        Assert.Contains(fks, o => o.Table == "Tests" && o.PrincipalTable == "Analytes");
        Assert.All(fks.Where(o => o.PrincipalTable == "Analytes"), o => Assert.Equal(ReferentialAction.Restrict, o.OnDelete));
    }

    [Fact]
    public void Up_AddsUniqueIndexesForCatalogIntegrity()
    {
        var ops = RunUp();
        var indexes = ops.OfType<CreateIndexOperation>().ToList();

        var analyteName = indexes.Single(i => i.Name == "IX_Analytes_Name" && i.Table == "Analytes");
        Assert.True(analyteName.IsUnique);

        var range = indexes.Single(i => i.Name == "IX_AnalyteReferenceRanges_AnalyteId");
        Assert.True(range.IsUnique);

        var profileAnalyte = indexes.Single(i => i.Name == "IX_ProfileAnalytes_ProfileId_AnalyteId");
        Assert.True(profileAnalyte.IsUnique);

        var profileTest = indexes.Single(i => i.Name == "IX_Profiles_TestId");
        Assert.True(profileTest.IsUnique);

        Assert.Contains(indexes, i => i.Name == "IX_ProfileResultItems_AnalyteId" && !i.IsUnique);
        Assert.Contains(indexes, i => i.Name == "IX_Tests_AnalyteId" && !i.IsUnique);
    }

    [Fact]
    public void Down_ReconstructsAnalyteNameFromConfiguredIdentityBeforeDroppingAnalytes()
    {
        var ops = RunDown();
        var reconstruct = RequireSql(ops, s => s.Contains("SET pri.AnalyteName = a.Name")
            && s.Contains("JOIN Analytes a ON a.AnalyteId = pri.AnalyteId"));
        var dropAnalytes = ops.OfType<DropTableOperation>().Single(o => o.Name == "Analytes");
        Assert.True(GetIndex(ops, reconstruct) < GetIndex(ops, dropAnalytes),
            "Down must rebuild AnalyteName while Analytes and the ProfileResultItems.AnalyteId column still exist.");

        var addName = ops.OfType<AddColumnOperation>().Single(o =>
            o.Table == "ProfileResultItems" && o.Name == "AnalyteName");
        Assert.False(addName.IsNullable);
    }

    [Fact]
    public void Down_DropsAllSevenNewTablesAndTheAnalyteIdColumns()
    {
        var ops = RunDown();
        var dropped = ops.OfType<DropTableOperation>().Select(o => o.Name).ToList();
        foreach (var table in new[]
        {
            "AnalyteReferenceRangeBands", "ProfileAnalytes", "ProfileResultAmendments",
            "ProfileResultItemReferenceRangeSnapshots", "AnalyteReferenceRanges", "Profiles", "Analytes"
        })
        {
            Assert.Contains(table, dropped);
        }

        Assert.Contains(ops.OfType<DropColumnOperation>(), o => o.Table == "Tests" && o.Name == "AnalyteId");
        Assert.Contains(ops.OfType<DropColumnOperation>(), o => o.Table == "ProfileResultItems" && o.Name == "AnalyteId");
    }
}