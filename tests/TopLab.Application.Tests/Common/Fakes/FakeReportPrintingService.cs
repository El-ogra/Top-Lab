using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Hand-rolled stub of <see cref="IReportPrintingService"/> for ReportProduction
/// print-command tests: records every token handed to the port and can be told to
/// return a canned failure. Defaults to <see cref="Result.Success"/>.
/// </summary>
public sealed class FakeReportPrintingService : IReportPrintingService
{
    public List<string> Tokens { get; } = new();

    public Result? NextResult { get; set; }

    public Task<Result> PrintReportAsync(string reportToken, CancellationToken cancellationToken = default)
    {
        Tokens.Add(reportToken);
        return Task.FromResult(NextResult ?? Result.Success());
    }
}