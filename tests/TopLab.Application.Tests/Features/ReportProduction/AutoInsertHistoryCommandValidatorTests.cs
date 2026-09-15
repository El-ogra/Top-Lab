using TopLab.Application.Features.ReportProduction.Commands.AutoInsertHistory;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class AutoInsertHistoryCommandValidatorTests
{
    [Fact]
    public async Task PositivePatientTestId_IsValid()
    {
        var validator = new AutoInsertHistoryCommandValidator();
        var result = await validator.ValidateAsync(new AutoInsertHistoryCommand(10));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ZeroPatientTestId_IsInvalid()
    {
        var validator = new AutoInsertHistoryCommandValidator();
        var result = await validator.ValidateAsync(new AutoInsertHistoryCommand(0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AutoInsertHistoryCommand.PatientTestId));
    }
}
