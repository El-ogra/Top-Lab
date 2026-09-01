using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateEnvelopeSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateEnvelopeSettingsCommandHandlerTests
{
    private static void Seed(FakeApplicationDbContext db)
    {
        db.EnvelopeSettings.Add(EnvelopeSettings.CreateDefault());
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Code", true, 1.0m, 2.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("ReferralEntity", true, 1.0m, 3.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Date", true, 1.0m, 4.0m));
    }

    private static IReadOnlyList<EnvelopePrintItemPositionDto> Positions() =>
    [
        new("Name", false, 2.5m, 1.5m),
        new("Code", true, 1.0m, 2.0m),
        new("ReferralEntity", true, 1.0m, 3.0m),
        new("Date", true, 1.0m, 4.0m)
    ];

    private static UpdateEnvelopeSettingsCommand DefaultCommand() => new(
        TopMarginCm: 2.0m,
        HeaderFooterMode: HeaderFooterMode.Words,
        SuppressCaptions: true,
        Positions: Positions());

    [Fact]
    public async Task UpdateEnvelopeSettings_RoundTrips_AllPositions()
    {
        var db = new FakeApplicationDbContext();
        Seed(db);

        var handler = new UpdateEnvelopeSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2.0m, db.EnvelopeSettings[0].TopMarginCm);
        Assert.True(db.EnvelopeSettings[0].SuppressCaptions);

        var name = db.EnvelopePrintItemPositions.Single(p => p.ItemName == "Name");
        Assert.False(name.IsEnabled);
        Assert.Equal(2.5m, name.LeftOffsetCm);
        Assert.Equal(1.5m, name.TopOffsetCm);
    }

    [Fact]
    public async Task UpdateEnvelopeSettings_InvalidPosition_AtomicNoChange()
    {
        var db = new FakeApplicationDbContext();
        Seed(db);

        // One invalid offset (31) — the domain guard throws mid-way; verify nothing mutated.
        var positions = new List<EnvelopePrintItemPositionDto>
        {
            new("Name", false, 31m, 1.5m),
            new("Code", true, 1.0m, 2.0m),
            new("ReferralEntity", true, 1.0m, 3.0m),
            new("Date", true, 1.0m, 4.0m)
        };
        var cmd = DefaultCommand() with { Positions = positions };
        var handler = new UpdateEnvelopeSettingsCommandHandler(db);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.Handle(cmd, CancellationToken.None));

        // Atomicity per plan: one failing item leaves every position unchanged.
        foreach (var position in db.EnvelopePrintItemPositions)
        {
            Assert.Equal(1.0m, position.LeftOffsetCm);
        }

        Assert.True(db.EnvelopePrintItemPositions.Single(p => p.ItemName == "Name").IsEnabled);
        Assert.True(db.EnvelopePrintItemPositions.All(p => p.IsEnabled));
    }

    [Fact]
    public async Task UpdateEnvelopeSettings_MissingRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateEnvelopeSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, result.Error!.Type);
    }
}