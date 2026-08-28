using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

/// <summary>Central order/result line. Auditable; hosts T-audit columns.</summary>
public sealed class PatientTest : AuditableEntity<PatientTestId>
{
    public PatientId PatientId { get; private set; } = default!;

    public TestId TestId { get; private set; } = default!;

    public decimal PriceAtOrderTime { get; private set; }

    public bool IsUrine { get; private set; }

    public bool IsStool { get; private set; }

    public bool IsBlood { get; private set; }

    public bool IsSemen { get; private set; }

    public bool IsCsf { get; private set; }

    public bool IsTakenOutsideLab { get; private set; }

    public bool IsSampleDrawn { get; private set; }

    public DateTime? SampleDrawnAtUtc { get; private set; }

    public string? ResultValue { get; private set; }

    public ResultFlag? ResultFlag { get; private set; }

    public string? Notes { get; private set; }

    public int? EnteredByUserId { get; private set; }

    public DateTime? EnteredAtUtc { get; private set; }

    public bool IsReviewed { get; private set; }

    public int? ReviewedByUserId { get; private set; }

    public DateTime? ReviewedAtUtc { get; private set; }

    public bool IsPrinted { get; private set; }

    public int PrintCount { get; private set; }

    public int? LastPrintedByUserId { get; private set; }

    public DateTime? LastPrintedAtUtc { get; private set; }

    public bool IsDelivered { get; private set; }

    public int? DeliveredByUserId { get; private set; }

    public DateTime? DeliveredAtUtc { get; private set; }

    public bool IsExported { get; private set; }

    public DateTime? ExportedAtUtc { get; private set; }

    private PatientTest()
    {
    }

    private PatientTest(
        PatientTestId id,
        PatientId patientId,
        TestId testId,
        decimal priceAtOrderTime,
        bool isUrine,
        bool isStool,
        bool isBlood,
        bool isSemen,
        bool isCsf,
        bool isTakenOutsideLab,
        bool isSampleDrawn,
        DateTime? sampleDrawnAtUtc)
        : base(id)
    {
        PatientId = patientId;
        TestId = testId;
        PriceAtOrderTime = priceAtOrderTime;
        IsUrine = isUrine;
        IsStool = isStool;
        IsBlood = isBlood;
        IsSemen = isSemen;
        IsCsf = isCsf;
        IsTakenOutsideLab = isTakenOutsideLab;
        IsSampleDrawn = isSampleDrawn;
        SampleDrawnAtUtc = sampleDrawnAtUtc;
    }

    public static PatientTest Create(
        PatientTestId id,
        PatientId patientId,
        TestId testId,
        decimal priceAtOrderTime,
        bool isUrine = false,
        bool isStool = false,
        bool isBlood = false,
        bool isSemen = false,
        bool isCsf = false,
        bool isTakenOutsideLab = false,
        bool isSampleDrawn = false,
        DateTime? sampleDrawnAtUtc = null)
    {
        return new PatientTest(id, patientId, testId, priceAtOrderTime, isUrine, isStool, isBlood, isSemen, isCsf, isTakenOutsideLab, isSampleDrawn, sampleDrawnAtUtc);
    }

    public void EnterResult(string? resultValue, ResultFlag? flag, int? enteredByUserId, DateTime? enteredAtUtc, string? notes = null)
    {
        ResultValue = resultValue;
        ResultFlag = flag;
        EnteredByUserId = enteredByUserId;
        EnteredAtUtc = enteredAtUtc;
        Notes = notes;
    }

    public void MarkReviewed(int reviewedByUserId, DateTime reviewedAtUtc)
    {
        IsReviewed = true;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = reviewedAtUtc;
    }

    public void MarkPrinted(int printedByUserId, DateTime printedAtUtc)
    {
        IsPrinted = true;
        PrintCount++;
        LastPrintedByUserId = printedByUserId;
        LastPrintedAtUtc = printedAtUtc;
    }

    public void MarkDelivered(int deliveredByUserId, DateTime deliveredAtUtc)
    {
        IsDelivered = true;
        DeliveredByUserId = deliveredByUserId;
        DeliveredAtUtc = deliveredAtUtc;
    }

    public void MarkExported(DateTime exportedAtUtc)
    {
        IsExported = true;
        ExportedAtUtc = exportedAtUtc;
    }

    public void MarkSampleDrawn(DateTime drawnAtUtc)
    {
        IsSampleDrawn = true;
        SampleDrawnAtUtc = drawnAtUtc;
    }
}
