using TopLab.Application.Features.Utilities.Queries.ComputeStopwatchElapsed;
using TopLab.Application.Features.Utilities.Queries.ConvertMeasurementUnit;
using TopLab.Application.Features.Utilities.Queries.EvaluateCalculation;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.Utilities;

public class ConvertMeasurementUnitQueryHandlerTests
{
    private static ConvertMeasurementUnitQueryHandler Handler() => new();

    [Fact]
    public async Task HappyPath_ConvertsNumberForNumber()
    {
        var result = await Handler().Handle(
            new ConvertMeasurementUnitQuery(1m, "g", "mg"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1000m, result.Value!.ConvertedValue);
        Assert.Equal("g", result.Value.FromUnit);
        Assert.Equal("mg", result.Value.ToUnit);
    }

    [Fact]
    public async Task UnknownPair_ReturnsValidation_FrozenMessage()
    {
        var result = await Handler().Handle(
            new ConvertMeasurementUnitQuery(1m, "mg", "IU"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("زوج الوحدات غير مدعوم.", result.Error!.Message);
    }

    [Fact]
    public void Validator_RejectsBlankUnits()
    {
        var validator = new ConvertMeasurementUnitQueryValidator();
        var outcome = validator.Validate(new ConvertMeasurementUnitQuery(1m, "", "mg"));
        Assert.False(outcome.IsValid);
    }
}

public class EvaluateCalculationQueryHandlerTests
{
    private static EvaluateCalculationQueryHandler Handler() => new();

    [Fact]
    public async Task HappyPath_ReturnsResult()
    {
        var result = await Handler().Handle(
            new EvaluateCalculationQuery("2+3*4"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(14m, result.Value!.Result);
    }

    [Fact]
    public async Task DivisionByZero_ReturnsValidation_FrozenMessage()
    {
        var result = await Handler().Handle(
            new EvaluateCalculationQuery("1/0"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن القسمة على صفر.", result.Error!.Message);
    }

    [Fact]
    public async Task Malformed_ReturnsValidation_FrozenMessage()
    {
        var result = await Handler().Handle(
            new EvaluateCalculationQuery("abc"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التعبير الحسابي غير صالح.", result.Error!.Message);
    }

    [Fact]
    public void Validator_RejectsEmptyExpression()
    {
        var validator = new EvaluateCalculationQueryValidator();
        Assert.False(validator.Validate(new EvaluateCalculationQuery("")).IsValid);
    }
}

public class ComputeStopwatchElapsedQueryHandlerTests
{
    [Fact]
    public async Task ExplicitEnd_ComputesElapsed()
    {
        var start = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 3, 15, 10, 0, 30, DateTimeKind.Utc);
        var handler = new ComputeStopwatchElapsedQueryHandler(new FakeDateTimeProvider());

        var result = await handler.Handle(
            new ComputeStopwatchElapsedQuery(start, end),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TimeSpan.FromSeconds(30), result.Value!.Elapsed);
    }

    [Fact]
    public async Task OmittedEnd_UsesProviderNow()
    {
        var now = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var clock = new FakeDateTimeProvider { UtcNow = now };
        var handler = new ComputeStopwatchElapsedQueryHandler(clock);
        var start = now.AddMinutes(-5);

        var result = await handler.Handle(
            new ComputeStopwatchElapsedQuery(start, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TimeSpan.FromMinutes(5), result.Value!.Elapsed);
        Assert.Equal(now, result.Value.EndUtc);
    }

    [Fact]
    public async Task InvertedBounds_ReturnsValidation_FrozenMessage()
    {
        var start = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        var end = start.AddSeconds(-1);
        var handler = new ComputeStopwatchElapsedQueryHandler(new FakeDateTimeProvider());

        var result = await handler.Handle(
            new ComputeStopwatchElapsedQuery(start, end),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("وقت النهاية يسبق وقت البداية.", result.Error!.Message);
    }
}
