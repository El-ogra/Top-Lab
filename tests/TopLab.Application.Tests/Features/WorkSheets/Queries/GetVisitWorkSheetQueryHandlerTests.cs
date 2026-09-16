using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.WorkSheets.Queries;

public class GetVisitWorkSheetQueryHandlerTests
{
    private static Patient MakePatient(int id, bool deleted = false)
    {
        var patient = Patient.Create(PatientId.Create(id), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        if (deleted)
        {
            patient.SoftDelete();
        }

        return patient;
    }

    private static Test MakeTest(int id, string code, int? groupId = null, string? barcode = null)
    {
        return Test.Create(
            TestId.Create(id), $"Test {code}", $"Test {code}", $"Test {code}", code, 60, 100m,
            testGroupId: groupId.HasValue ? TestGroupId.Create(groupId.Value) : null,
            barcode: barcode);
    }

    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(7));
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(5), "G5"));
        db.Tests.Add(MakeTest(10, "T10", groupId: 5, barcode: "BC-10"));
        db.Tests.Add(MakeTest(11, "T11", barcode: "BC-11"));
        db.PatientTests.Add(PatientTest.Create(
            PatientTestId.Create(100), PatientId.Create(7), TestId.Create(10), 100m,
            isUrine: true, isBlood: true));
        db.PatientTests.Add(PatientTest.Create(
            PatientTestId.Create(101), PatientId.Create(7), TestId.Create(11), 50m,
            isTakenOutsideLab: true));
        return db;
    }

    [Fact]
    public async Task GetVisitWorkSheet_MixedLines_AssemblesSectionsAndFlags()
    {
        var db = BuildDb();
        var handler = new GetVisitWorkSheetQueryHandler(db);

        var result = await handler.Handle(new GetVisitWorkSheetQuery(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value!.PatientId);
        Assert.Equal("Ahmed", result.Value.PatientFullName);
        Assert.Equal(2, result.Value.TotalTests);
        Assert.Equal(2, result.Value.Sections.Sum(s => s.Lines.Count));
        var grouped = Assert.Single(result.Value.Sections, s => s.SectionName == "G5");
        Assert.Equal("BC-10", Assert.Single(grouped.Lines).Barcode);
        var ungrouped = Assert.Single(result.Value.Sections, s => s.SectionName == "بدون مجموعة");
        Assert.Equal("BC-11", Assert.Single(ungrouped.Lines).Barcode);
        Assert.Equal(2, result.Value.Samples.Count);
        var urine = Assert.Single(result.Value.Samples, s => s.PatientTestId == 100);
        Assert.True(urine.IsUrine);
        Assert.True(urine.IsBlood);
        Assert.False(urine.IsTakenOutsideLab);
        var outside = Assert.Single(result.Value.Samples, s => s.PatientTestId == 101);
        Assert.True(outside.IsTakenOutsideLab);
    }

    [Fact]
    public async Task GetVisitWorkSheet_EmptyVisit_ReturnsEmptySections_StillPrintable()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(7));
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        var handler = new GetVisitWorkSheetQueryHandler(db);

        var result = await handler.Handle(new GetVisitWorkSheetQuery(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalTests);
        Assert.Empty(result.Value.Sections);
        Assert.Empty(result.Value.Samples);
    }

    [Fact]
    public async Task GetVisitWorkSheet_UnknownPatient_NotFound()
    {
        var db = BuildDb();
        var handler = new GetVisitWorkSheetQueryHandler(db);

        var result = await handler.Handle(new GetVisitWorkSheetQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetVisitWorkSheet_SoftDeletedPatient_NotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(7, deleted: true));
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        var handler = new GetVisitWorkSheetQueryHandler(db);

        var result = await handler.Handle(new GetVisitWorkSheetQuery(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public void GetVisitWorkSheet_Requires_PrintWorksheet()
    {
        var authorized = (IAuthorizedRequest)new GetVisitWorkSheetQuery(7);

        Assert.Equal("PRINT_WORKSHEET", authorized.RequiredPermissionCode);
    }
}

public class GetVisitWorkSheetQueryValidatorTests
{
    private readonly GetVisitWorkSheetQueryValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new GetVisitWorkSheetQuery(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new GetVisitWorkSheetQuery(7));

        Assert.True(result.IsValid);
    }
}
