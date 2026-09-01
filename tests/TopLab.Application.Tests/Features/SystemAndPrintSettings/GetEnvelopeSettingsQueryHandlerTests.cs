using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetEnvelopeSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetEnvelopeSettingsQueryHandlerTests
{
    private static void Seed(FakeApplicationDbContext db)
    {
        db.EnvelopeSettings.Add(EnvelopeSettings.CreateDefault());
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Code", true, 1.0m, 2.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("ReferralEntity", true, 1.0m, 3.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Date", true, 1.0m, 4.0m));
    }

    [Fact]
    public async Task GetEnvelopeSettings_ReturnsRowAndFourPositionsInCanonicalOrder()
    {
        var db = new FakeApplicationDbContext();
        Seed(db);

        var handler = new GetEnvelopeSettingsQueryHandler(db);
        var result = await handler.Handle(new GetEnvelopeSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3.0m, result.Value!.TopMarginCm);

        var names = result.Value.Positions.Select(p => p.ItemName).ToList();
        Assert.Equal(4, names.Count);
        Assert.Equal(["Name", "Code", "ReferralEntity", "Date"], names);
    }
}