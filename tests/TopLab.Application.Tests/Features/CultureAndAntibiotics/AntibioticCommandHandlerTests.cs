using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

public class AntibioticCommandHandlerTests
{
    private static Test BuildCultureTest(int id, string name = "Culture", bool isCultureType = true)
        => Test.Create(
            TestId.Create(id),
            name,
            reportName: name,
            receiptName: name,
            testCode: $"C{id}",
            completionDurationMinutes: 30,
            patientPrice: 100m,
            resultKind: ResultKind.Culture,
            isCultureType: isCultureType);

    private static Antibiotic BuildAntibiotic(int id, string name = "Amoxicillin")
        => Antibiotic.Create(AntibioticId.Create(id), name);

    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Add(BuildCultureTest(1));
        db.Add(BuildAntibiotic(10, "Amoxicillin"));
        db.Add(BuildAntibiotic(11, "Ciprofloxacin"));
        return db;
    }

    [Fact]
    public async Task Create_HappyPath_AddsAndReturnsId()
    {
        var db = BuildDb();
        var handler = new CreateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new CreateAntibioticCommand("Tetracycline", false, false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(db.Antibiotics, a => a.Name == "Tetracycline");
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_TrimsWhitespaceBeforeDuplicateCheck()
    {
        var db = BuildDb();
        db.Add(BuildAntibiotic(20, "Tetracycline"));
        var handler = new CreateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new CreateAntibioticCommand("  Tetracycline  ", false, false),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("المضاد الحيوي موجود بالفعل", result.Error.Message);
    }

    [Fact]
    public async Task Create_DuplicateExactName_ReturnsConflict()
    {
        var db = BuildDb();
        var handler = new CreateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new CreateAntibioticCommand("Amoxicillin", false, false),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("المضاد الحيوي موجود بالفعل", result.Error.Message);
    }

    [Fact]
    public async Task Create_BlankName_ReturnsValidation()
    {
        var db = BuildDb();
        var handler = new CreateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new CreateAntibioticCommand("   ", false, false),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("اسم المضاد الحيوي مطلوب.", result.Error.Message);
    }

    [Fact]
    public async Task Update_HappyPath_ChangesNameAndFlags()
    {
        var db = BuildDb();
        var handler = new UpdateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new UpdateAntibioticCommand(10, "  NewName  ", true, true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ab = db.Antibiotics.First(a => a.Id.Value == 10);
        Assert.Equal("NewName", ab.Name);
        Assert.True(ab.IsPregnancyFlagged);
        Assert.True(ab.IsChildrenFlagged);
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        var db = BuildDb();
        var handler = new UpdateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new UpdateAntibioticCommand(999, "X", false, false),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المضاد الحيوي غير موجود.", result.Error.Message);
    }

    [Fact]
    public async Task Update_DuplicateNameExcludingSelf_ReturnsConflict()
    {
        var db = BuildDb();
        var handler = new UpdateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new UpdateAntibioticCommand(10, "Ciprofloxacin", false, false),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("المضاد الحيوي موجود بالفعل", result.Error.Message);
    }

    [Fact]
    public async Task Update_SameNameAsItself_Succeeds()
    {
        var db = BuildDb();
        var handler = new UpdateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new UpdateAntibioticCommand(10, "Amoxicillin", true, false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ab = db.Antibiotics.First(a => a.Id.Value == 10);
        Assert.Equal("Amoxicillin", ab.Name);
        Assert.True(ab.IsPregnancyFlagged);
    }

    [Fact]
    public async Task Update_BlankName_ReturnsValidation()
    {
        var db = BuildDb();
        var handler = new UpdateAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new UpdateAntibioticCommand(10, "  ", false, false),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Delete_Unreferenced_Succeeds()
    {
        var db = BuildDb();
        var handler = new DeleteAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new DeleteAntibioticCommand(11),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(db.Antibiotics, a => a.Id.Value == 11);
    }

    [Fact]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        var db = BuildDb();
        var handler = new DeleteAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new DeleteAntibioticCommand(999),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المضاد الحيوي غير موجود.", result.Error.Message);
    }

    [Fact]
    public async Task Delete_AttachedToCulture_BlockedWithAttachmentMessage()
    {
        var db = BuildDb();
        db.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(10)));
        var handler = new DeleteAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new DeleteAntibioticCommand(10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف المضاد الحيوي لارتباطه بمزرعة.", result.Error.Message);
        Assert.Contains(db.Antibiotics, a => a.Id.Value == 10);
    }

    [Fact]
    public async Task Delete_HasResult_BlockedWithResultsMessage()
    {
        var db = BuildDb();
        db.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(1),
            PatientTestId.Create(1),
            AntibioticId.Create(10),
            SensitivityCategory.HighlyFor));
        var handler = new DeleteAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new DeleteAntibioticCommand(10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف المضاد الحيوي لوجود نتائج مسجلة به.", result.Error.Message);
    }

    [Fact]
    public async Task Delete_AttachmentCheck_TakesPrecedenceOverResultCheck()
    {
        var db = BuildDb();
        db.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(10)));
        db.Add(CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(1),
            PatientTestId.Create(1),
            AntibioticId.Create(10),
            SensitivityCategory.HighlyFor));
        var handler = new DeleteAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new DeleteAntibioticCommand(10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف المضاد الحيوي لارتباطه بمزرعة.", result.Error.Message);
    }

    [Fact]
    public async Task Delete_SaveTimeReferenceConflict_MapsToResultsMessage()
    {
        var db = new ReferenceConflictFakeApplicationDbContext { ThrowReferenceConflict = true };
        db.Inner.Add(BuildAntibiotic(10, "Amoxicillin"));
        var handler = new DeleteAntibioticCommandHandler(db);

        var result = await handler.Handle(
            new DeleteAntibioticCommand(10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف المضاد الحيوي لوجود نتائج مسجلة به.", result.Error.Message);
    }
}