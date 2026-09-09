using TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class GetProfileEntryGridQueryHandlerTests
{
    [Fact]
    public async Task Grid_ListsConfiguredAnalytes_WithNamesAndFrozenRanges()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.SeedAnalyteWithRange(db, 2);
        ProfileResultsSeed.AddProfile(db, 10, 1, 2);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);

        var item = ProfileResultItem.Create(
            ProfileResultItemId.Create(501),
            pt.Id,
            AnalyteId.Create(1),
            "4.2",
            "mg",
            null,
            isVerified: true);
        db.ProfileResultItems.Add(item);
        db.ProfileResultItemReferenceRangeSnapshots.Add(
            ProfileResultItemReferenceRangeSnapshot.Create(
                item.Id,
                item.AnalyteId,
                null,
                AgeUnit.Year,
                0,
                100,
                1m,
                5m,
                null,
                null,
                DateTime.UtcNow));

        var result = await new GetProfileEntryGridQueryHandler(db).Handle(new GetProfileEntryGridQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("Profile10", dto.ProfileName);
        Assert.Equal("P1", dto.PatientFullName);
        Assert.Equal(2, dto.Items.Count);
        var entered = dto.Items.Single(i => i.AnalyteId == 1);
        Assert.Equal("4.2", entered.ResultValue);
        Assert.True(entered.IsVerified);
        Assert.False(entered.IsPrinted);
        Assert.NotNull(entered.FrozenRange);
        Assert.Equal(1m, entered.FrozenRange!.MinValue);
        Assert.Equal(5m, entered.FrozenRange.MaxValue);
        Assert.Equal("Report A1", entered.FrozenRange.AnalyteName);
        var untouched = dto.Items.Single(i => i.AnalyteId == 2);
        Assert.Equal(0, untouched.ProfileResultItemId);
        Assert.Null(untouched.ResultValue);
        Assert.Null(untouched.FrozenRange);
    }

    [Fact]
    public async Task Grid_MissingTest_NotFound()
    {
        var db = new FakeApplicationDbContext();

        var result = await new GetProfileEntryGridQueryHandler(db).Handle(new GetProfileEntryGridQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task Grid_NoProfile_NotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);

        var result = await new GetProfileEntryGridQueryHandler(db).Handle(new GetProfileEntryGridQuery(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("البروفايل غير موجود.", result.Error!.Message);
    }
}