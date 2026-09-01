using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateEnvelopeSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateEnvelopeSettingsCommandValidatorTests
{
    private readonly UpdateEnvelopeSettingsCommandValidator _validator = new();

    private static IReadOnlyList<EnvelopePrintItemPositionDto> Positions() =>
    [
        new("Name", true, 1.0m, 1.0m),
        new("Code", true, 1.0m, 2.0m),
        new("ReferralEntity", true, 1.0m, 3.0m),
        new("Date", true, 1.0m, 4.0m)
    ];

    private static UpdateEnvelopeSettingsCommand Valid() => new(
        TopMarginCm: 3.0m,
        HeaderFooterMode: HeaderFooterMode.None,
        SuppressCaptions: false,
        Positions: Positions());

    [Fact]
    public void Valid_Passes()
    {
        var result = _validator.Validate(Valid());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UnknownItemName_Fails()
    {
        var positions = new List<EnvelopePrintItemPositionDto>
        {
            new("Name", true, 1.0m, 1.0m),
            new("Unknown", true, 1.0m, 2.0m),
            new("ReferralEntity", true, 1.0m, 3.0m),
            new("Date", true, 1.0m, 4.0m)
        };
        var result = _validator.Validate(Valid() with { Positions = positions });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void WrongNumberOfItems_Fails()
    {
        var positions = new List<EnvelopePrintItemPositionDto>
        {
            new("Name", true, 1.0m, 1.0m),
            new("Code", true, 1.0m, 2.0m),
            new("ReferralEntity", true, 1.0m, 3.0m)
        };
        var result = _validator.Validate(Valid() with { Positions = positions });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OffsetOutOfRange_Fails()
    {
        var positions = new List<EnvelopePrintItemPositionDto>
        {
            new("Name", true, 31m, 1.0m),
            new("Code", true, 1.0m, 2.0m),
            new("ReferralEntity", true, 1.0m, 3.0m),
            new("Date", true, 1.0m, 4.0m)
        };
        var result = _validator.Validate(Valid() with { Positions = positions });
        Assert.False(result.IsValid);
    }
}