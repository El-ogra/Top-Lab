using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Common;

public sealed record SystemSettingsDto(
    AccountType DefaultAccountType,
    bool SaveTreatingDoctorOnlyFromEntityWindow,
    bool EnablePatientNameSearchAssist,
    bool DisableAutoTitleInsertion,
    bool PrintFileExternalBarcode,
    bool PrintDateTimeOnTubeBarcode,
    bool PrintLabIdInsteadOfPatientId,
    bool AutoReviewAndComplete,
    bool PrintAccountInsteadOfDateOnReport,
    ResultScreenAccountDisplayMode ResultScreenAccountDisplayMode,
    bool DailyBackupEnabled,
    string? DailyBackupPath);

public sealed record ReportSettingsDto(
    decimal PageMarginLeftCm,
    decimal PageMarginBottomCm,
    decimal ReportTopSpaceCm,
    PaperSize PaperSize,
    HeaderFooterMode HeaderFooterMode,
    bool DoctorSignatureEnabled,
    HistorySortMode HistorySortMode,
    bool HistoryAutoDisplayEnabled);

public sealed record ReceiptSettingsDto(
    decimal TopMarginCm,
    string Currency,
    TimeOnly? PickupTimeDefault,
    bool PrintOnce,
    TestDetailDisplayMode TestDetailDisplayMode,
    bool CashierPrinterEnabled,
    HeaderFooterMode HeaderFooterMode);

public sealed record EnvelopePrintItemPositionDto(
    string ItemName,
    bool IsEnabled,
    decimal LeftOffsetCm,
    decimal TopOffsetCm);

public sealed record EnvelopeSettingsDto(
    decimal TopMarginCm,
    HeaderFooterMode HeaderFooterMode,
    bool SuppressCaptions,
    IReadOnlyList<EnvelopePrintItemPositionDto> Positions);

public sealed record PrinterAssignmentDto(
    PrinterOutputType OutputType,
    string PrinterName);

public sealed record LabPrintTextDto(
    string LabName,
    string Address,
    string Phone,
    string FontFamily,
    int FontSizePt);

public sealed record DatabaseServerSettingsDto(
    string ServerName,
    string DatabaseName,
    bool IntegratedSecurity,
    string Login);