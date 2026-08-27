using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for report printing. Implemented in Infrastructure (Printing/). Declared here
/// so Application/Presentation depend only on the abstraction (ADR-0005, Architecture §4.3).
/// </summary>
public interface IReportPrintingService
{
    Task<Result> PrintReportAsync(string reportToken, CancellationToken cancellationToken = default);
}
