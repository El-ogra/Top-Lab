using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class Test : AuditableEntity<TestId>
{
    public string Name { get; private set; } = default!;

    public string ReportName { get; private set; } = default!;

    public string ReceiptName { get; private set; } = default!;

    public TestGroupId? TestGroupId { get; private set; }

    public string? Barcode { get; private set; }

    public int CompletionDurationMinutes { get; private set; }

    public bool IsSentOut { get; private set; }

    public decimal? SentOutCostPrice { get; private set; }

    public decimal PatientPrice { get; private set; }

    public decimal? LabToLabPrice { get; private set; }

    public ResultKind ResultKind { get; private set; }

    public bool IsCultureType { get; private set; }

    private Test()
    {
    }

    private Test(
        TestId id,
        string name,
        string reportName,
        string receiptName,
        int completionDurationMinutes,
        decimal patientPrice,
        ResultKind resultKind,
        bool isCultureType,
        TestGroupId? testGroupId,
        string? barcode,
        bool isSentOut,
        decimal? sentOutCostPrice,
        decimal? labToLabPrice)
        : base(id)
    {
        Name = name;
        ReportName = reportName;
        ReceiptName = receiptName;
        CompletionDurationMinutes = completionDurationMinutes;
        PatientPrice = patientPrice;
        ResultKind = resultKind;
        IsCultureType = isCultureType;
        TestGroupId = testGroupId;
        Barcode = barcode;
        IsSentOut = isSentOut;
        SentOutCostPrice = sentOutCostPrice;
        LabToLabPrice = labToLabPrice;
    }

    public static Test Create(
        TestId id,
        string name,
        string reportName,
        string receiptName,
        int completionDurationMinutes,
        decimal patientPrice,
        ResultKind resultKind = ResultKind.Simple,
        bool isCultureType = false,
        TestGroupId? testGroupId = null,
        string? barcode = null,
        bool isSentOut = false,
        decimal? sentOutCostPrice = null,
        decimal? labToLabPrice = null)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(reportName) || string.IsNullOrWhiteSpace(receiptName))
        {
            throw new ArgumentException("Name/ReportName/ReceiptName required.");
        }

        if (completionDurationMinutes <= 0)
        {
            throw new ArgumentException("CompletionDurationMinutes must be > 0.", nameof(completionDurationMinutes));
        }

        if (isSentOut && sentOutCostPrice is null)
        {
            throw new ArgumentException("SentOutCostPrice required when IsSentOut=true.", nameof(sentOutCostPrice));
        }

        return new Test(id, name.Trim(), reportName.Trim(), receiptName.Trim(), completionDurationMinutes, patientPrice, resultKind, isCultureType, testGroupId, barcode, isSentOut, sentOutCostPrice, labToLabPrice);
    }

    public void Update(
        string name,
        string reportName,
        string receiptName,
        int completionDurationMinutes,
        decimal patientPrice,
        TestGroupId? testGroupId,
        string? barcode,
        bool isSentOut,
        decimal? sentOutCostPrice,
        decimal? labToLabPrice)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(reportName) || string.IsNullOrWhiteSpace(receiptName))
        {
            throw new ArgumentException("Name/ReportName/ReceiptName required.");
        }

        if (completionDurationMinutes <= 0)
        {
            throw new ArgumentException("CompletionDurationMinutes must be > 0.", nameof(completionDurationMinutes));
        }

        if (isSentOut && sentOutCostPrice is null)
        {
            throw new ArgumentException("SentOutCostPrice required when IsSentOut=true.", nameof(sentOutCostPrice));
        }

        Name = name.Trim();
        ReportName = reportName.Trim();
        ReceiptName = receiptName.Trim();
        CompletionDurationMinutes = completionDurationMinutes;
        PatientPrice = patientPrice;
        TestGroupId = testGroupId;
        Barcode = barcode;
        IsSentOut = isSentOut;
        SentOutCostPrice = sentOutCostPrice;
        LabToLabPrice = labToLabPrice;
    }
}
