using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class Test : AuditableEntity<TestId>
{
    public const int MaxTestCodeLength = 50;

    public string Name { get; private set; } = default!;

    public string ReportName { get; private set; } = default!;

    public string ReceiptName { get; private set; } = default!;

    public string TestCode { get; private set; } = default!;

    public bool IsActive { get; private set; }

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
        string testCode,
        int completionDurationMinutes,
        decimal patientPrice,
        ResultKind resultKind,
        bool isCultureType,
        TestGroupId? testGroupId,
        string? barcode,
        bool isSentOut,
        decimal? sentOutCostPrice,
        decimal? labToLabPrice,
        bool isActive)
        : base(id)
    {
        Name = name;
        ReportName = reportName;
        ReceiptName = receiptName;
        TestCode = testCode;
        CompletionDurationMinutes = completionDurationMinutes;
        PatientPrice = patientPrice;
        ResultKind = resultKind;
        IsCultureType = isCultureType;
        TestGroupId = testGroupId;
        Barcode = barcode;
        IsSentOut = isSentOut;
        SentOutCostPrice = sentOutCostPrice;
        LabToLabPrice = labToLabPrice;
        IsActive = isActive;
    }

    public static Test Create(
        TestId id,
        string name,
        string reportName,
        string receiptName,
        string testCode,
        int completionDurationMinutes,
        decimal patientPrice,
        ResultKind resultKind = ResultKind.Simple,
        bool isCultureType = false,
        TestGroupId? testGroupId = null,
        string? barcode = null,
        bool isSentOut = false,
        decimal? sentOutCostPrice = null,
        decimal? labToLabPrice = null,
        bool isActive = true)
    {
        Guard(name, reportName, receiptName, testCode, completionDurationMinutes, patientPrice, isSentOut, sentOutCostPrice);

        return new Test(id, name.Trim(), reportName.Trim(), receiptName.Trim(), testCode.Trim(), completionDurationMinutes, patientPrice, resultKind, isCultureType, testGroupId, barcode, isSentOut, sentOutCostPrice, labToLabPrice, isActive);
    }

    public void Update(
        string name,
        string reportName,
        string receiptName,
        string testCode,
        int completionDurationMinutes,
        decimal patientPrice,
        TestGroupId? testGroupId,
        string? barcode,
        bool isSentOut,
        decimal? sentOutCostPrice,
        decimal? labToLabPrice)
    {
        Guard(name, reportName, receiptName, testCode, completionDurationMinutes, patientPrice, isSentOut, sentOutCostPrice);

        Name = name.Trim();
        ReportName = reportName.Trim();
        ReceiptName = receiptName.Trim();
        TestCode = testCode.Trim();
        CompletionDurationMinutes = completionDurationMinutes;
        PatientPrice = patientPrice;
        TestGroupId = testGroupId;
        Barcode = barcode;
        IsSentOut = isSentOut;
        SentOutCostPrice = sentOutCostPrice;
        LabToLabPrice = labToLabPrice;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }

    private static void Guard(
        string name,
        string reportName,
        string receiptName,
        string testCode,
        int completionDurationMinutes,
        decimal patientPrice,
        bool isSentOut,
        decimal? sentOutCostPrice)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(reportName) || string.IsNullOrWhiteSpace(receiptName))
        {
            throw new ArgumentException("Name/ReportName/ReceiptName required.");
        }

        if (string.IsNullOrWhiteSpace(testCode))
        {
            throw new ArgumentException("TestCode required.", nameof(testCode));
        }

        if (testCode.Trim().Length > MaxTestCodeLength)
        {
            throw new ArgumentException($"TestCode must be at most {MaxTestCodeLength} characters.", nameof(testCode));
        }

        if (completionDurationMinutes <= 0)
        {
            throw new ArgumentException("CompletionDurationMinutes must be > 0.", nameof(completionDurationMinutes));
        }

        if (patientPrice < 0)
        {
            throw new ArgumentException("PatientPrice must be >= 0.", nameof(patientPrice));
        }

        if (isSentOut && sentOutCostPrice is null)
        {
            throw new ArgumentException("SentOutCostPrice required when IsSentOut=true.", nameof(sentOutCostPrice));
        }
    }
}