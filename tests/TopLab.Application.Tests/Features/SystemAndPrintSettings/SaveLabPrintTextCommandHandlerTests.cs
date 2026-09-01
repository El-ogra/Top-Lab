using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public sealed class SaveLabPrintTextCommandHandlerTests
{
    [Fact]
    public async Task Handle_SavesRoundTripThroughStore()
    {
        var store = new FakeLabPrintTextStore();
        var handler = new SaveLabPrintTextCommandHandler(store);
        var cmd = new SaveLabPrintTextCommand(LabPrintTextScope.Report, "معمل التحاليل", "شارع النصر", "01234567890", "Arial", 14);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        Assert.Single(store.Store);
        Assert.Equal("معمل التحاليل", store.Store[LabPrintTextScope.Report].LabName);
        Assert.Equal(14, store.Store[LabPrintTextScope.Report].FontSizePt);
    }

    [Fact]
    public void Validator_EmptyLabName_IsInvalid()
    {
        var validator = new SaveLabPrintTextCommandValidator();
        var command = new SaveLabPrintTextCommand(LabPrintTextScope.Report, "", "addr", "phone", "Arial", 14);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_ZeroFontSize_IsInvalid()
    {
        var validator = new SaveLabPrintTextCommandValidator();
        var command = new SaveLabPrintTextCommand(LabPrintTextScope.Report, "Lab", "addr", "phone", "Arial", 0);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}