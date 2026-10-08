using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for patient-envelope printing (Phase 1, REF-066 — owns all shared
/// envelope infrastructure).
/// Implemented in Infrastructure (Printing/). Declared here so
/// Application/Presentation depend only on the abstraction.
/// </summary>
public interface IEnvelopePrintingService
{
    Task<Result> PrintEnvelopeAsync(string envelopeToken, CancellationToken cancellationToken = default);
}
