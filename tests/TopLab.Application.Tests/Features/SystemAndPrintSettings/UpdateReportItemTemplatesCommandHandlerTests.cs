using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateReportItemTemplatesCommandHandlerTests
{
    private static ReportItemTemplateDto Dto(string key, string label) =>
        new(key, label, null, true, null, false);

    [Fact]
    public async Task MissingRow_IsCreated()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateReportItemTemplatesCommandHandler(db);

        var result = await handler.Handle(
            new UpdateReportItemTemplatesCommand([Dto(ReportItemKeys.TestName, "التحليل")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.ReportItemTemplates);
        Assert.Equal(ReportItemKeys.TestName, row.ItemKey);
        Assert.Equal("التحليل", row.Label);
    }

    [Fact]
    public async Task ExistingRow_IsUpdated_NotDuplicated()
    {
        var db = new FakeApplicationDbContext();
        db.ReportItemTemplates.Add(ReportItemTemplate.Create(ReportItemKeys.TestName, "قديم"));

        var handler = new UpdateReportItemTemplatesCommandHandler(db);
        var result = await handler.Handle(
            new UpdateReportItemTemplatesCommand(
                [new ReportItemTemplateDto(ReportItemKeys.TestName, "جديد", "بادئة", false, "L", true)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.ReportItemTemplates);
        Assert.Equal("جديد", row.Label);
        Assert.Equal("بادئة", row.ReferenceValueExpression);
        Assert.False(row.ShowHide);
        Assert.Equal("L", row.ListMembership);
        Assert.True(row.FreeTextOverride);
    }

    [Fact]
    public async Task MultipleRows_ArePersistedTogether()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateReportItemTemplatesCommandHandler(db);

        var result = await handler.Handle(
            new UpdateReportItemTemplatesCommand(
            [
                Dto(ReportItemKeys.TestName, "التحليل"),
                Dto(ReportItemKeys.ResultValue, "النتيجة")
            ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.ReportItemTemplates.Count);
    }
}
