using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

public class CultureAntibioticAttachmentCommandHandlerTests
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
        db.Add(BuildCultureTest(1, "Blood Culture"));
        db.Add(BuildCultureTest(2, "CBC", isCultureType: false));
        db.Add(BuildAntibiotic(10, "Amoxicillin"));
        db.Add(BuildAntibiotic(11, "Ciprofloxacin"));
        return db;
    }

    [Fact]
    public async Task Attach_HappyPath_AddsAttachmentRow()
    {
        var db = BuildDb();
        var handler = new AttachAntibioticToCultureCommandHandler(db);

        var result = await handler.Handle(
            new AttachAntibioticToCultureCommand(1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.CultureAntibioticAttachments);
        Assert.Contains(db.CultureAntibioticAttachments,
            a => a.TestId.Value == 1 && a.AntibioticId.Value == 10);
    }

    [Fact]
    public async Task Attach_MissingTest_ReturnsNotFound()
    {
        var db = BuildDb();
        var handler = new AttachAntibioticToCultureCommandHandler(db);

        var result = await handler.Handle(
            new AttachAntibioticToCultureCommand(999, 10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error.Message);
    }

    [Fact]
    public async Task Attach_NonCultureTest_ReturnsValidation()
    {
        var db = BuildDb();
        var handler = new AttachAntibioticToCultureCommandHandler(db);

        var result = await handler.Handle(
            new AttachAntibioticToCultureCommand(2, 10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("التحليل المحدد ليس مزرعة.", result.Error.Message);
    }

    [Fact]
    public async Task Attach_MissingAntibiotic_ReturnsNotFound()
    {
        var db = BuildDb();
        var handler = new AttachAntibioticToCultureCommandHandler(db);

        var result = await handler.Handle(
            new AttachAntibioticToCultureCommand(1, 999),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المضاد الحيوي غير موجود.", result.Error.Message);
    }

    [Fact]
    public async Task Attach_DuplicateAttachment_ReturnsConflict()
    {
        var db = BuildDb();
        db.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(10)));
        var handler = new AttachAntibioticToCultureCommandHandler(db);

        var result = await handler.Handle(
            new AttachAntibioticToCultureCommand(1, 10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("المضاد الحيوي مضاف بالفعل لهذه المزرعة.", result.Error.Message);
    }

    [Fact]
    public async Task Detach_HappyPath_RemovesAttachmentRow()
    {
        var db = BuildDb();
        db.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(10)));
        var handler = new DetachAntibioticFromCultureCommandHandler(db);

        var result = await handler.Handle(
            new DetachAntibioticFromCultureCommand(1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.CultureAntibioticAttachments);
    }

    [Fact]
    public async Task Detach_MissingAttachment_ReturnsNotFound()
    {
        var db = BuildDb();
        var handler = new DetachAntibioticFromCultureCommandHandler(db);

        var result = await handler.Handle(
            new DetachAntibioticFromCultureCommand(1, 10),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المضاد الحيوي غير مضاف لهذه المزرعة.", result.Error.Message);
    }
}