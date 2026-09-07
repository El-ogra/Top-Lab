using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Queries.GetAntibiotics;
using TopLab.Application.Features.CultureAndAntibiotics.Queries.GetCultureAntibiotics;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

public class AntibioticQueryHandlerTests
{
    private static Test BuildCultureTest(int id, string name = "Culture")
        => Test.Create(
            TestId.Create(id),
            name,
            reportName: name,
            receiptName: name,
            testCode: $"C{id}",
            completionDurationMinutes: 30,
            patientPrice: 100m,
            resultKind: ResultKind.Culture,
            isCultureType: true);

    [Fact]
    public async Task GetAntibiotics_NoFilter_OrdersByName()
    {
        var db = new FakeApplicationDbContext();
        db.Add(Antibiotic.Create(AntibioticId.Create(1), "Ciprofloxacin"));
        db.Add(Antibiotic.Create(AntibioticId.Create(2), "Amoxicillin"));
        db.Add(Antibiotic.Create(AntibioticId.Create(3), "Tetracycline"));
        var handler = new GetAntibioticsQueryHandler(db);

        var result = await handler.Handle(
            new GetAntibioticsQuery(null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Equal("Amoxicillin", result.Value[0].Name);
        Assert.Equal("Ciprofloxacin", result.Value[1].Name);
        Assert.Equal("Tetracycline", result.Value[2].Name);
    }

    [Fact]
    public async Task GetAntibiotics_SearchTerm_FiltersAndTrims()
    {
        var db = new FakeApplicationDbContext();
        db.Add(Antibiotic.Create(AntibioticId.Create(1), "Ciprofloxacin"));
        db.Add(Antibiotic.Create(AntibioticId.Create(2), "Amoxicillin"));
        db.Add(Antibiotic.Create(AntibioticId.Create(3), "Tetracycline"));
        var handler = new GetAntibioticsQueryHandler(db);

        var result = await handler.Handle(
            new GetAntibioticsQuery("  Cipro  "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Ciprofloxacin", result.Value![0].Name);
    }

    [Fact]
    public async Task GetAntibiotics_NoMatch_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        db.Add(Antibiotic.Create(AntibioticId.Create(1), "Amoxicillin"));
        var handler = new GetAntibioticsQueryHandler(db);

        var result = await handler.Handle(
            new GetAntibioticsQuery("nope"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetCultureAntibiotics_HappyPath_ReturnsAttachedWithCount()
    {
        var db = new FakeApplicationDbContext();
        db.Add(BuildCultureTest(1, "Blood Culture"));
        db.Add(Antibiotic.Create(AntibioticId.Create(10), "Amoxicillin", true, false));
        db.Add(Antibiotic.Create(AntibioticId.Create(11), "Ciprofloxacin", false, true));
        db.Add(Antibiotic.Create(AntibioticId.Create(12), "Tetracycline"));
        db.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(10)));
        db.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(11)));
        var handler = new GetCultureAntibioticsQueryHandler(db);

        var result = await handler.Handle(
            new GetCultureAntibioticsQuery(1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TestId);
        Assert.Equal("Blood Culture", result.Value.TestName);
        Assert.Equal(2, result.Value.AttachedCount);
        Assert.Equal(2, result.Value.Antibiotics.Count);
        Assert.Equal("Amoxicillin", result.Value.Antibiotics[0].Name);
        Assert.True(result.Value.Antibiotics[0].IsPregnancyFlagged);
    }

    [Fact]
    public async Task GetCultureAntibiotics_NoAttachments_ReturnsZeroCount()
    {
        var db = new FakeApplicationDbContext();
        db.Add(BuildCultureTest(1));
        var handler = new GetCultureAntibioticsQueryHandler(db);

        var result = await handler.Handle(
            new GetCultureAntibioticsQuery(1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.AttachedCount);
        Assert.Empty(result.Value.Antibiotics);
    }

    [Fact]
    public async Task GetCultureAntibiotics_MissingTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetCultureAntibioticsQueryHandler(db);

        var result = await handler.Handle(
            new GetCultureAntibioticsQuery(999),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error.Message);
    }

    [Fact]
    public async Task GetCultureAntibiotics_NonCultureTest_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        db.Add(Test.Create(
            TestId.Create(1),
            "CBC",
            reportName: "CBC",
            receiptName: "CBC",
            testCode: "CBC1",
            completionDurationMinutes: 30,
            patientPrice: 50m,
            resultKind: ResultKind.Simple,
            isCultureType: false));
        var handler = new GetCultureAntibioticsQueryHandler(db);

        var result = await handler.Handle(
            new GetCultureAntibioticsQuery(1),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("التحليل المحدد ليس مزرعة.", result.Error.Message);
    }
}