using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetSummary;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;
using Xunit;

namespace TopLab.Application.Tests.Features.WorkSheets;

public class WorkSheetValidatorTests
{
    private static readonly DateOnly Sep1 = new(2026, 9, 1);
    private static readonly DateOnly Sep3 = new(2026, 9, 3);

    private const string PeriodMessage = "بداية الفترة يجب ألا تتجاوز نهايتها.";

    [Fact]
    public async Task ByLog_FromAfterTo_IsInvalid()
    {
        var validator = new GetWorkSheetByWorkGroupLogQueryValidator();
        var result = await validator.ValidateAsync(new GetWorkSheetByWorkGroupLogQuery(7, From: Sep3, To: Sep1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == PeriodMessage);
    }

    [Fact]
    public async Task ByLog_ZeroId_IsInvalid()
    {
        var validator = new GetWorkSheetByWorkGroupLogQueryValidator();
        var result = await validator.ValidateAsync(new GetWorkSheetByWorkGroupLogQuery(0, From: Sep1, To: Sep3));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "معرف سجل مجموعة العمل غير صالح.");
    }

    [Fact]
    public async Task ByTestGroup_FromAfterTo_IsInvalid()
    {
        var validator = new GetWorkSheetByTestGroupQueryValidator();
        var result = await validator.ValidateAsync(new GetWorkSheetByTestGroupQuery(From: Sep3, To: Sep1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == PeriodMessage);
    }

    [Fact]
    public async Task ByTestGroup_NonPositiveGroupId_IsInvalid()
    {
        var validator = new GetWorkSheetByTestGroupQueryValidator();
        var result = await validator.ValidateAsync(
            new GetWorkSheetByTestGroupQuery(TestGroupId: -1, From: Sep1, To: Sep3));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "معرف مجموعة التحاليل غير صالح.");
    }

    [Fact]
    public async Task Summary_FromAfterTo_IsInvalid()
    {
        var validator = new GetWorkSheetSummaryQueryValidator();
        var result = await validator.ValidateAsync(new GetWorkSheetSummaryQuery(From: Sep3, To: Sep1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == PeriodMessage);
    }

    [Fact]
    public async Task TestCount_FromAfterTo_IsInvalid()
    {
        var validator = new GetWorkSheetTestCountByPeriodQueryValidator();
        var result = await validator.ValidateAsync(new GetWorkSheetTestCountByPeriodQuery(From: Sep3, To: Sep1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == PeriodMessage);
    }
}
