using TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class InsertHistoryResultCommandValidatorTests
{
    [Fact]
    public async Task BothIdsPositive_IsValid()
    {
        var validator = new InsertHistoryResultCommandValidator();
        var result = await validator.ValidateAsync(new InsertHistoryResultCommand(10, 20));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(10, 0)]
    [InlineData(0, 0)]
    public async Task AnyNonPositiveId_IsInvalid(int current, int source)
    {
        var validator = new InsertHistoryResultCommandValidator();
        var result = await validator.ValidateAsync(new InsertHistoryResultCommand(current, source));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            (e.PropertyName == nameof(InsertHistoryResultCommand.PatientTestId) && current <= 0) ||
            (e.PropertyName == nameof(InsertHistoryResultCommand.SourcePatientTestId) && source <= 0));
    }
}
