using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetLabPrintTextQueryHandlerTests
{
    [Fact]
    public async Task GetLabPrintText_EmptyStore_ReturnsDefaults()
    {
        var store = new FakeLabPrintTextStore();
        var handler = new GetLabPrintTextQueryHandler(store);
        var result = await handler.Handle(new GetLabPrintTextQuery { Scope = LabPrintTextScope.Report }, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value!.LabName);
    }

    [Fact]
    public async Task GetLabPrintText_ReturnsStoredContent()
    {
        var store = new FakeLabPrintTextStore();
        store.Store[LabPrintTextScope.Receipt] = new LabPrintTextDto("مختبر النور", "شارع", "3566", "Arial", 12);
        var handler = new GetLabPrintTextQueryHandler(store);
        var result = await handler.Handle(new GetLabPrintTextQuery { Scope = LabPrintTextScope.Receipt }, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("مختبر النور", result.Value!.LabName);
        Assert.Equal("Arial", result.Value.FontFamily);
        Assert.Equal(12, result.Value.FontSizePt);
    }
}