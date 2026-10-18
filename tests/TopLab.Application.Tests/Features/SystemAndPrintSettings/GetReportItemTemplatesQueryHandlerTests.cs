using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportItemTemplates;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetReportItemTemplatesQueryHandlerTests
{
    [Fact]
    public async Task EmptyDatabase_FallsBackToCatalogForEveryCanonicalKeyInOrder()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetReportItemTemplatesQueryHandler(db);

        var result = await handler.Handle(new GetReportItemTemplatesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var keys = result.Value!.Select(i => i.ItemKey).ToList();
        Assert.Equal(ReportItemKeys.All, keys);
    }

    [Fact]
    public async Task ConfiguredRow_IsReturnedInsteadOfDefault()
    {
        var db = new FakeApplicationDbContext();
        db.ReportItemTemplates.Add(ReportItemTemplate.Create(
            ReportItemKeys.TestName, "اسم مخصص", "بادئة", showHide: false, listMembership: "L", freeTextOverride: true));

        var handler = new GetReportItemTemplatesQueryHandler(db);
        var result = await handler.Handle(new GetReportItemTemplatesQuery(), CancellationToken.None);

        var row = result.Value!.Single(i => i.ItemKey == ReportItemKeys.TestName);
        Assert.Equal("اسم مخصص", row.Label);
        Assert.Equal("بادئة", row.ReferenceValueExpression);
        Assert.False(row.ShowHide);
        Assert.Equal("L", row.ListMembership);
        Assert.True(row.FreeTextOverride);
    }
}
