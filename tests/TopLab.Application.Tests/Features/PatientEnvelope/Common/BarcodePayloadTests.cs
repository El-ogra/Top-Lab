using TopLab.Application.Features.PatientEnvelope.Common;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientEnvelope.Common;

public class BarcodePayloadTests
{
    [Fact]
    public void For_FlagOnWithLabId_ReturnsLabId()
    {
        Assert.Equal("100", BarcodePayload.For(7, "100", printLabIdInstead: true));
    }

    [Fact]
    public void For_FlagOnWithNullLabId_FallsBackToPatientId()
    {
        Assert.Equal("7", BarcodePayload.For(7, null, printLabIdInstead: true));
    }

    [Fact]
    public void For_FlagOnWithEmptyLabId_FallsBackToPatientId()
    {
        Assert.Equal("7", BarcodePayload.For(7, string.Empty, printLabIdInstead: true));
    }

    [Fact]
    public void For_FlagOnWithWhitespaceLabId_FallsBackToPatientId()
    {
        Assert.Equal("7", BarcodePayload.For(7, "   ", printLabIdInstead: true));
    }

    [Fact]
    public void For_FlagOffWithLabId_ReturnsPatientId()
    {
        Assert.Equal("7", BarcodePayload.For(7, "100", printLabIdInstead: false));
    }
}
