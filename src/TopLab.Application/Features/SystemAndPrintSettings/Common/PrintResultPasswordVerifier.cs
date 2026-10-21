using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Common;

/// <summary>
/// STAGE-01 (baseline 1.2.2.4.2 / 1.2.2.4.2.1): the single place that answers
/// "may this result be printed?".
/// </summary>
/// <remarks>
/// STAGE-01 owns the requirement and the verifier; the print entry points that
/// call it belong to STAGE-06, STAGE-08 and STAGE-10. The plain password is
/// compared against the stored hash and is never persisted.
/// </remarks>
public sealed class PrintResultPasswordVerifier
{
    private readonly IApplicationDbContext _db;

    public PrintResultPasswordVerifier(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>True when a password is required before a result may be printed.</summary>
    public bool IsRequired()
    {
        var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        return settings is not null && settings.PrintResultPasswordEnabled;
    }

    /// <summary>
    /// True when the candidate satisfies the requirement. When the requirement is
    /// off, every candidate is accepted.
    /// </summary>
    public bool Verify(string? candidate)
    {
        var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);

        if (settings is null || !settings.PrintResultPasswordEnabled)
        {
            return true;
        }

        return PrintResultPasswordHasher.Verify(candidate, settings.PrintResultPasswordHash);
    }
}
