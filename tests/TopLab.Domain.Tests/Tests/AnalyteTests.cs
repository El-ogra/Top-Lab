using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class AnalyteTests
{
    private static Analyte CreateAnalyte(AnalyteId id, string name = "Glucose", string reportName = "Glucose")
    {
        return Analyte.Create(id, name, reportName);
    }

    private static AnalyteReferenceRange CreateRange(AnalyteId analyteId)
    {
        return AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(1), analyteId);
    }

    private static AnalyteReferenceRangeBand CreateBand(
        AnalyteReferenceRangeId rangeId,
        Sex? sex = null,
        AgeUnit ageUnit = AgeUnit.Year,
        int ageMin = 0,
        int ageMax = 120,
        decimal min = 1m,
        decimal max = 10m)
    {
        return AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(1),
            rangeId,
            ageUnit,
            ageMin,
            ageMax,
            min,
            max,
            sex);
    }

    [Fact]
    public void Create_Guards_BlankName()
    {
        Assert.Throws<ArgumentException>(() => Analyte.Create(AnalyteId.Create(1), " ", "Glucose"));
        Assert.Throws<ArgumentException>(() => Analyte.Create(AnalyteId.Create(1), "Glucose", string.Empty));
    }

    [Fact]
    public void Create_TrimsNameAndReportName()
    {
        var analyte = Analyte.Create(AnalyteId.Create(1), " Glucose ", " Glucose Report ");

        Assert.Equal("Glucose", analyte.Name);
        Assert.Equal("Glucose Report", analyte.ReportName);
        Assert.True(analyte.IsActive);
    }

    [Fact]
    public void AttachCurrentRange_OneRangeAggregate_Enforced()
    {
        var analyte = CreateAnalyte(AnalyteId.Create(1));
        var range = CreateRange(analyte.Id);

        analyte.AttachCurrentRange(range);

        Assert.Same(range, analyte.CurrentRange);
        Assert.Throws<InvalidOperationException>(() => analyte.AttachCurrentRange(CreateRange(analyte.Id)));
    }

    [Fact]
    public void AttachCurrentRange_RejectsRangeOfAnotherAnalyte()
    {
        var analyte = CreateAnalyte(AnalyteId.Create(1));
        var otherRange = CreateRange(AnalyteId.Create(99));

        Assert.Throws<ArgumentException>(() => analyte.AttachCurrentRange(otherRange));
    }

    [Fact]
    public void OneAnalyte_BelongsToMultipleProfiles()
    {
        var analyte = CreateAnalyte(AnalyteId.Create(1));
        var test1 = Test.Create(TestId.Create(10), "P1", "P1", "P1", "T10", 30, 200m, ResultKind.SpecializedProfile);
        var test2 = Test.Create(TestId.Create(11), "P2", "P2", "P2", "T11", 30, 250m, ResultKind.SpecializedProfile);
        var profile1 = Profile.Create(ProfileId.Create(1), "Profile 1", test1.Id, test1.ResultKind, 200m);
        var profile2 = Profile.Create(ProfileId.Create(2), "Profile 2", test2.Id, test2.ResultKind, 250m);

        profile1.AddAnalyte(ProfileAnalyte.Create(ProfileAnalyteId.Create(1), profile1.Id, analyte.Id));
        profile2.AddAnalyte(ProfileAnalyte.Create(ProfileAnalyteId.Create(2), profile2.Id, analyte.Id));

        Assert.Single(profile1.Analytes);
        Assert.Single(profile2.Analytes);
        Assert.Equal(analyte.Id, Assert.Single(profile1.Analytes).AnalyteId);
        Assert.Equal(analyte.Id, Assert.Single(profile2.Analytes).AnalyteId);
    }

    [Fact]
    public void RangeAggregate_MatchesBand_BySexAgeUnit()
    {
        var analyte = CreateAnalyte(AnalyteId.Create(1));
        var range = CreateRange(analyte.Id);
        range.AddBand(CreateBand(range.Id, Sex.Male, AgeUnit.Year, 0, 12, 1m, 5m));
        range.AddBand(CreateBand(range.Id, Sex.Female, AgeUnit.Year, 0, 12, 2m, 6m));
        analyte.AttachCurrentRange(range);

        var male = range.Match(Sex.Male, AgeUnit.Year, 5);
        var female = range.Match(Sex.Female, AgeUnit.Year, 5);

        Assert.NotNull(male);
        Assert.Equal(5m, male!.MaxValue);
        Assert.NotNull(female);
        Assert.Equal(6m, female!.MaxValue);
    }
}