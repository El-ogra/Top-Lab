using TopLab.Application.Features.AnalyteProfiles.Commands.AddProfileAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.CreateAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;
using TopLab.Application.Features.AnalyteProfiles.Commands.DeactivateAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.RemoveProfileAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.SaveAnalyteReferenceRange;
using TopLab.Application.Features.AnalyteProfiles.Commands.UpdateAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.AnalyteProfiles;

public class AnalyteProfileCatalogTests
{
    private static Test MakeSpecializedTest(int id)
    {
        return Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 60, 500m, ResultKind.SpecializedProfile);
    }

    private static (Analyte Analyte, AnalyteReferenceRange Range) SeedAnalyteWithRange(FakeApplicationDbContext db, int id)
    {
        var analyte = Analyte.Create(AnalyteId.Create(id), $"A{id}", $"A{id}");
        var range = AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(id + 10000), analyte.Id);
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(range);
        return (analyte, range);
    }

    [Fact]
    public async Task CreateAnalyte_CreatesAnalyteAndSinglePlaceholderRange()
    {
        var db = new FakeApplicationDbContext();
        var handler = new CreateAnalyteCommandHandler(db);
        var result = await handler.Handle(new CreateAnalyteCommand("Glucose", "Glucose"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.Analytes);
        Assert.Single(db.AnalyteReferenceRanges);
        Assert.Equal(db.Analytes.Single().Id, db.AnalyteReferenceRanges.Single().AnalyteId);
    }

    [Fact]
    public async Task CreateAnalyte_DuplicateName_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(1), "Glucose", "Glucose"));

        var handler = new CreateAnalyteCommandHandler(db);
        var result = await handler.Handle(new CreateAnalyteCommand("Glucose", "Other"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("اسم المادة التحليلية مستخدم بالفعل", result.Error!.Message);
    }

    [Fact]
    public async Task UpdateAnalyte_Renames()
    {
        var db = new FakeApplicationDbContext();
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(1), "Glucose", "Glucose"));

        var handler = new UpdateAnalyteCommandHandler(db);
        var result = await handler.Handle(new UpdateAnalyteCommand(1, "Glucose U", "Glucose U"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Glucose U", db.Analytes.Single().Name);
    }

    [Fact]
    public async Task DeactivateAnalyte_Deactivates()
    {
        var db = new FakeApplicationDbContext();
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(1), "Glucose", "Glucose"));

        var handler = new DeactivateAnalyteCommandHandler(db);
        var result = await handler.Handle(new DeactivateAnalyteCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(db.Analytes.Single().IsActive);
    }

    [Fact]
    public async Task SaveAnalyteReferenceRange_ReplacesBands()
    {
        var db = new FakeApplicationDbContext();
        var (analyte, range) = SeedAnalyteWithRange(db, 1);
        db.AnalyteReferenceRangeBands.Add(AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(10), range.Id, AgeUnit.Year, 0, 100, 1m, 5m, null));

        var handler = new SaveAnalyteReferenceRangeCommandHandler(db);
        var result = await handler.Handle(
            new SaveAnalyteReferenceRangeCommand(1, new[]
            {
                new AnalyteBandInput(null, AgeUnit.Year, 20, 40, 2m, 6m, null, null),
                new AnalyteBandInput(null, AgeUnit.Year, 41, 100, 3m, 8m, null, null),
            }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.AnalyteReferenceRangeBands.Count);
        Assert.DoesNotContain(db.AnalyteReferenceRangeBands, b => b.MinValue == 1m);
        Assert.Equal(40, range.Bands.Single(b => b.MinValue == 2m).AgeMax);
    }

    [Fact]
    public async Task CreateProfile_WithRangedAnalytes_Success()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest(50));
        SeedAnalyteWithRange(db, 1);
        SeedAnalyteWithRange(db, 2);

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            new CreateProfileCommand("Liver", 50, 300m, new[] { 1, 2 }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var profile = Assert.Single(db.Profiles);
        Assert.Equal(300m, profile.FixedPrice);
        Assert.Equal(2, db.ProfileAnalytes.Count);
    }

    [Fact]
    public async Task CreateProfile_NonSpecializedTest_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(Test.Create(TestId.Create(50), "T50", "T50", "T50", "T50", 60, 100m, ResultKind.Simple));
        SeedAnalyteWithRange(db, 1);

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            new CreateProfileCommand("Liver", 50, 300m, new[] { 1 }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("البروفايل يُنشأ فقط لتحليل متخصص.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateProfile_DuplicateForTest_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest(50));
        db.Profiles.Add(Profile.Create(ProfileId.Create(60), "Liver", TestId.Create(50), ResultKind.SpecializedProfile, 300m));
        SeedAnalyteWithRange(db, 1);

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            new CreateProfileCommand("Liver 2", 50, 300m, new[] { 1 }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("هذا التحليل له بروفايل بالفعل.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateProfile_AnalyteWithoutRange_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest(50));
        var analyte = Analyte.Create(AnalyteId.Create(1), "A1", "A1");
        db.Analytes.Add(analyte);

        var handler = new CreateProfileCommandHandler(db);
        var result = await handler.Handle(
            new CreateProfileCommand("Liver", 50, 300m, new[] { 1 }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("يجب تحديد نطاق مرجعي للمادة التحليلية قبل إضافتها للبروفايل.", result.Error!.Message);
    }

    [Fact]
    public async Task AddProfileAnalyte_BeforeResults_Success()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest(50));
        db.Profiles.Add(Profile.Create(ProfileId.Create(60), "Liver", TestId.Create(50), ResultKind.SpecializedProfile, 300m));
        SeedAnalyteWithRange(db, 1);
        SeedAnalyteWithRange(db, 2);

        var handler = new AddProfileAnalyteCommandHandler(db);
        var result = await handler.Handle(new AddProfileAnalyteCommand(60, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.ProfileAnalytes);
        Assert.Equal(2, db.ProfileAnalytes.Single().AnalyteId.Value);
    }

    [Fact]
    public async Task AddProfileAnalyte_AfterResults_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest(50));
        db.Profiles.Add(Profile.Create(ProfileId.Create(60), "Liver", TestId.Create(50), ResultKind.SpecializedProfile, 300m));
        SeedAnalyteWithRange(db, 1);
        var pt = PatientTest.Create(PatientTestId.Create(70), PatientId.Create(1), TestId.Create(50), 300m);
        pt.EnterResult("12", ResultFlag.High, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new AddProfileAnalyteCommandHandler(db);
        var result = await handler.Handle(new AddProfileAnalyteCommand(60, 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن تعديل مكونات البروفايل بعد إدخال النتائج.", result.Error!.Message);
    }

    [Fact]
    public async Task RemoveProfileAnalyte_RemovesLink()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest(50));
        var profile = Profile.Create(ProfileId.Create(60), "Liver", TestId.Create(50), ResultKind.SpecializedProfile, 300m);
        var (analyte, _) = SeedAnalyteWithRange(db, 1);
        var link = ProfileAnalyte.Create(ProfileAnalyteId.Create(80), profile.Id, analyte.Id);
        profile.AddAnalyte(link);
        db.Profiles.Add(profile);
        db.ProfileAnalytes.Add(link);

        var handler = new RemoveProfileAnalyteCommandHandler(db);
        var result = await handler.Handle(new RemoveProfileAnalyteCommand(60, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.ProfileAnalytes);
        Assert.Empty(profile.Analytes);
    }

    [Fact]
    public async Task GetAnalyteDefinitions_IncludesBands()
    {
        var db = new FakeApplicationDbContext();
        var (_, range) = SeedAnalyteWithRange(db, 1);
        db.AnalyteReferenceRangeBands.Add(AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(10), range.Id, AgeUnit.Year, 0, 100, 1m, 5m, null));

        var handler = new GetAnalyteDefinitionsQueryHandler(db);
        var result = await handler.Handle(new GetAnalyteDefinitionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("A1", dto.Name);
        var band = Assert.Single(dto.Bands);
        Assert.Equal(1m, band.MinValue);
    }

    [Fact]
    public async Task GetProfileDefinitions_IncludesFixedPriceAndAnalytes()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeSpecializedTest(50));
        var profile = Profile.Create(ProfileId.Create(60), "Liver", TestId.Create(50), ResultKind.SpecializedProfile, 300m);
        var (analyte, _) = SeedAnalyteWithRange(db, 1);
        var link = ProfileAnalyte.Create(ProfileAnalyteId.Create(80), profile.Id, analyte.Id);
        profile.AddAnalyte(link);
        db.Profiles.Add(profile);
        db.ProfileAnalytes.Add(link);

        var handler = new GetProfileDefinitionsQueryHandler(db);
        var result = await handler.Handle(new GetProfileDefinitionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("Liver", dto.Name);
        Assert.Equal(50, dto.SpecializedTestId);
        Assert.Equal("T50", dto.SpecializedTestName);
        Assert.Equal(300m, dto.FixedPrice);
        Assert.Equal(new[] { 1 }, dto.AnalyteIds);
    }
}