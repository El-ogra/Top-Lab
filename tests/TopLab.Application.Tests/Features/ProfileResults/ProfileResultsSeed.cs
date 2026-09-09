using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Tests.Features.ProfileResults;

internal static class ProfileResultsSeed
{
    internal static Patient MakePatient(int id = 1, Sex sex = Sex.Male, int age = 30, AgeUnit unit = AgeUnit.Year)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", sex, age, unit, DateTime.UtcNow);
    }

    internal static Test MakeSpecializedTest(int id)
    {
        return Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 60, 500m, ResultKind.SpecializedProfile);
    }

    /// <summary>Analyte + single placeholder range + one matching band (any age/sex, 1..5).</summary>
    internal static (Analyte Analyte, AnalyteReferenceRange Range, AnalyteReferenceRangeBand Band) SeedAnalyteWithRange(
        FakeApplicationDbContext db,
        int analyteId,
        decimal min = 1m,
        decimal max = 5m)
    {
        var analyte = Analyte.Create(AnalyteId.Create(analyteId), $"A{analyteId}", $"Report A{analyteId}");
        var range = AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(analyteId + 10000), analyte.Id);
        var band = AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(analyteId + 20000),
            range.Id,
            AgeUnit.Year,
            0,
            100,
            min,
            max,
            null);
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(range);
        db.AnalyteReferenceRangeBands.Add(band);
        return (analyte, range, band);
    }

    internal static Profile AddProfile(FakeApplicationDbContext db, int testId, params int[] analyteIds)
    {
        var profile = Profile.Create(ProfileId.Create(1), $"Profile{testId}", TestId.Create(testId), ResultKind.SpecializedProfile, 300m);
        db.Profiles.Add(profile);
        for (var i = 0; i < analyteIds.Length; i++)
        {
            var link = ProfileAnalyte.Create(
                ProfileAnalyteId.Create(i + 1),
                profile.Id,
                AnalyteId.Create(analyteIds[i]));
            db.ProfileAnalytes.Add(link);
            profile.AddAnalyte(link);
        }

        return profile;
    }

    internal static PatientTest AddProfileTest(FakeApplicationDbContext db, int patientTestId, int patientId, int testId)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(patientTestId),
            PatientId.Create(patientId),
            TestId.Create(testId),
            300m);
        db.PatientTests.Add(pt);
        return pt;
    }

    internal static ProfileResultItem AddPrintedItem(
        FakeApplicationDbContext db,
        int itemId,
        PatientTest pt,
        int analyteId,
        string resultValue = "4")
    {
        var item = ProfileResultItem.Create(
            ProfileResultItemId.Create(itemId),
            pt.Id,
            AnalyteId.Create(analyteId),
            resultValue,
            "mg",
            ProfileResultFlag.Low,
            isVerified: true,
            isPrinted: true);
        db.ProfileResultItems.Add(item);
        return item;
    }
}
