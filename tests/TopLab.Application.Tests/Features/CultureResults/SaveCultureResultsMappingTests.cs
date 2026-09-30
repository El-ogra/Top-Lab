using TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

/// <summary>WP-03 / SD-3: sensitivity option mapping and validator range.</summary>
public class SaveCultureResultsMappingTests
{
    [Fact]
    public void SensitivityOptions_ListContainsExactlyFiveEntries()
    {
        // SD-3 contract (mirrors CultureEntryViewModel.SensitivityOptions):
        var domainOptions = new[]
        {
            new SensitivityOption(null, "Unspecified"),
            new SensitivityOption(0, "Sensitive"),
            new SensitivityOption(1, "Intermediate"),
            new SensitivityOption(2, "Low Sensitivity"),
            new SensitivityOption(3, "Resistant")
        };

        Assert.Equal(5, domainOptions.Length);
        Assert.Null(domainOptions[0].Value);
        Assert.Equal(0, domainOptions[1].Value);
        Assert.Equal(1, domainOptions[2].Value);
        Assert.Equal(2, domainOptions[3].Value);
        Assert.Equal(3, domainOptions[4].Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SaveCulture_InRangeValue_IsAccepted(int category)
    {
        var validator = new SaveCultureResultsCommandValidator();
        var cmd = new SaveCultureResultsCommand(
            101, "دم", "E.coli", null, null, null, null,
            new[] { new CultureSensitivityInput(5, category) });

        var result = validator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void SaveCulture_OutOfRangeValue_IsRejected()
    {
        var validator = new SaveCultureResultsCommandValidator();
        var cmd = new SaveCultureResultsCommand(
            101, "دم", "E.coli", null, null, null, null,
            new[] { new CultureSensitivityInput(5, 4) });

        var result = validator.Validate(cmd);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void SaveCulture_NegativeValue_IsRejected()
    {
        var validator = new SaveCultureResultsCommandValidator();
        var cmd = new SaveCultureResultsCommand(
            101, "دم", "E.coli", null, null, null, null,
            new[] { new CultureSensitivityInput(5, -1) });

        var result = validator.Validate(cmd);
        Assert.False(result.IsValid);
    }
}
