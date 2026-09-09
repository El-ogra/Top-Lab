using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

/// <summary>
/// One line of a specialised-profile result. Carries the configured
/// <see cref="AnalyteId"/> (required catalog identity; the name is resolved from the
/// catalog for display). The historical freeze at entry time lives in
/// <see cref="ProfileResultItemReferenceRangeSnapshot"/>; reports read only that
/// snapshot, never the live analyte range (Decision 1).
/// </summary>
public sealed class ProfileResultItem : Entity<ProfileResultItemId>
{
    public PatientTestId PatientTestId { get; private set; } = default!;

    public AnalyteId AnalyteId { get; private set; } = default!;

    public string ResultValue { get; private set; } = default!;

    public string? Unit { get; private set; }

    public ProfileResultFlag? Flag { get; private set; }

    public bool IsVerified { get; private set; }

    public bool IsPrinted { get; private set; }

    public int PrintCount { get; private set; }

    public int? LastPrintedByUserId { get; private set; }

    public DateTime? LastPrintedAtUtc { get; private set; }

    private ProfileResultItem()
    {
    }

    private ProfileResultItem(
        ProfileResultItemId id,
        PatientTestId patientTestId,
        AnalyteId analyteId,
        string resultValue,
        string? unit,
        ProfileResultFlag? flag,
        bool isVerified,
        bool isPrinted,
        int printCount,
        int? lastPrintedByUserId,
        DateTime? lastPrintedAtUtc)
        : base(id)
    {
        PatientTestId = patientTestId;
        AnalyteId = analyteId;
        ResultValue = resultValue;
        Unit = unit;
        Flag = flag;
        IsVerified = isVerified;
        IsPrinted = isPrinted;
        PrintCount = printCount;
        LastPrintedByUserId = lastPrintedByUserId;
        LastPrintedAtUtc = lastPrintedAtUtc;
    }

    public static ProfileResultItem Create(
        ProfileResultItemId id,
        PatientTestId patientTestId,
        AnalyteId analyteId,
        string resultValue,
        string? unit = null,
        ProfileResultFlag? flag = null,
        bool isVerified = false,
        bool isPrinted = false)
    {
        ArgumentNullException.ThrowIfNull(patientTestId);
        ArgumentNullException.ThrowIfNull(analyteId);

        if (string.IsNullOrWhiteSpace(resultValue))
        {
            throw new ArgumentException("ResultValue required.");
        }

        return new ProfileResultItem(id, patientTestId, analyteId, resultValue.Trim(), unit, flag, isVerified, isPrinted, 0, null, null);
    }

    /// <summary>Draft edit. Guarded for unprinted drafts only; printed items use <see cref="Amend"/>.</summary>
    public void Update(string resultValue, string? unit, ProfileResultFlag? flag)
    {
        if (IsPrinted)
        {
            throw new InvalidOperationException("Printed profile items cannot be updated; use Amend.");
        }

        if (string.IsNullOrWhiteSpace(resultValue))
        {
            throw new ArgumentException("ResultValue required.");
        }

        ResultValue = resultValue.Trim();
        Unit = unit;
        Flag = flag;
    }

    /// <summary>Idempotent verify: no-op when already verified; only pre-print.</summary>
    public void Verify()
    {
        if (IsPrinted)
        {
            throw new InvalidOperationException("Printed profile items cannot be verified.");
        }

        IsVerified = true;
    }

    /// <summary>Idempotent unverify: no-op when already unverified; only pre-print.</summary>
    public void Unverify()
    {
        if (IsPrinted)
        {
            throw new InvalidOperationException("Printed profile items cannot be unverified.");
        }

        IsVerified = false;
    }

    /// <summary>Idempotent print: allowed when already printed (reprint); requires verified.</summary>
    public void MarkPrinted(int printedByUserId, DateTime printedAtUtc)
    {
        if (!IsVerified)
        {
            throw new InvalidOperationException("Profile item not verified.");
        }

        IsPrinted = true;
        PrintCount++;
        LastPrintedByUserId = printedByUserId;
        LastPrintedAtUtc = printedAtUtc;
    }

    /// <summary>
    /// Narrowly-scoped post-print amendment (Decision 3): changes only the active
    /// value/unit/flag on the existing row. No unprint, no new report version, no
    /// lifecycle change. The command handler persists a matching immutable
    /// <see cref="ProfileResultAmendment"/> in the same transaction.
    /// </summary>
    public void Amend(string resultValue, string? unit, ProfileResultFlag? flag)
    {
        if (string.IsNullOrWhiteSpace(resultValue))
        {
            throw new ArgumentException("ResultValue required.");
        }

        ResultValue = resultValue.Trim();
        Unit = unit;
        Flag = flag;
    }
}