using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

public enum LabPrintTextScope
{
    Report = 0,
    Receipt = 1,
    Envelope = 2
}

/// <summary>
/// Port for the workstation-local plain-text lab identification and font
/// choices (no colors, no images — ADR-0027). Implemented by a JSON store
/// under %ProgramData%\TopLab\lab-print-text.json, one record per scope.
/// </summary>
public interface ILabPrintTextStore
{
    Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default);

    Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default);
}