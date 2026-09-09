using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

/// <summary>
/// Immutable audit record of a post-print profile-result amendment (Decision 3).
/// Creation-only by design: no public setters, no update/delete API. The command
/// handler writes the active-item change and this row in a single relational
/// <c>SaveChangesAsync</c> so both commit or both roll back.
/// </summary>
public sealed class ProfileResultAmendment : Entity<ProfileResultAmendmentId>
{
    public const int MaxReasonLength = 500;

    public ProfileResultItemId ProfileResultItemId { get; private set; } = default!;

    public int AmendedByUserId { get; private set; }

    public DateTime AmendedAtUtc { get; private set; }

    public string OldResultValue { get; private set; } = default!;

    public string? OldUnit { get; private set; }

    public ProfileResultFlag? OldFlag { get; private set; }

    public string NewResultValue { get; private set; } = default!;

    public string? NewUnit { get; private set; }

    public ProfileResultFlag? NewFlag { get; private set; }

    public string? Reason { get; private set; }

    private ProfileResultAmendment()
    {
    }

    private ProfileResultAmendment(
        ProfileResultAmendmentId id,
        ProfileResultItemId profileResultItemId,
        int amendedByUserId,
        DateTime amendedAtUtc,
        string oldResultValue,
        string? oldUnit,
        ProfileResultFlag? oldFlag,
        string newResultValue,
        string? newUnit,
        ProfileResultFlag? newFlag,
        string? reason)
        : base(id)
    {
        ProfileResultItemId = profileResultItemId;
        AmendedByUserId = amendedByUserId;
        AmendedAtUtc = amendedAtUtc;
        OldResultValue = oldResultValue;
        OldUnit = oldUnit;
        OldFlag = oldFlag;
        NewResultValue = newResultValue;
        NewUnit = newUnit;
        NewFlag = newFlag;
        Reason = reason;
    }

    public static ProfileResultAmendment Create(
        ProfileResultAmendmentId id,
        ProfileResultItemId profileResultItemId,
        int amendedByUserId,
        DateTime amendedAtUtc,
        string oldResultValue,
        string? oldUnit,
        ProfileResultFlag? oldFlag,
        string newResultValue,
        string? newUnit,
        ProfileResultFlag? newFlag,
        string? reason)
    {
        ArgumentNullException.ThrowIfNull(profileResultItemId);

        if (amendedByUserId <= 0)
        {
            throw new ArgumentException("AmendedByUserId must be > 0.", nameof(amendedByUserId));
        }

        if (string.IsNullOrWhiteSpace(oldResultValue))
        {
            throw new ArgumentException("OldResultValue required.");
        }

        if (string.IsNullOrWhiteSpace(newResultValue))
        {
            throw new ArgumentException("NewResultValue required.");
        }

        if (reason?.Length > MaxReasonLength)
        {
            throw new ArgumentException($"Reason must be at most {MaxReasonLength} characters.");
        }

        return new ProfileResultAmendment(
            id,
            profileResultItemId,
            amendedByUserId,
            amendedAtUtc,
            oldResultValue.Trim(),
            oldUnit,
            oldFlag,
            newResultValue.Trim(),
            newUnit,
            newFlag,
            reason);
    }
}