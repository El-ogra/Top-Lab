# R1 Round 5 — Evidence

Generated 2026-10-25/26. Static evidence only: no WPF screen was executed, rendered or visually verified.
HEAD at start and end: `efc909c688ccd3375b6bf3bc15af617f3739c647` (unchanged).

## (a) Complete git diff of all tracked modified files

```diff
diff --git a/src/TopLab.Application/Features/CultureResults/Common/CultureResultDtos.cs b/src/TopLab.Application/Features/CultureResults/Common/CultureResultDtos.cs
index f8b707b..12856aa 100644
--- a/src/TopLab.Application/Features/CultureResults/Common/CultureResultDtos.cs
+++ b/src/TopLab.Application/Features/CultureResults/Common/CultureResultDtos.cs
@@ -17,7 +17,8 @@ public sealed record CultureMicroscopyDto(
 public sealed record CultureEntryGridDto(int PatientTestId, int PatientId, string TestName,
     string TestCode, string? Sample, string? OrganismA, string? OrganismB, string? OrganismC,
     string? CultureCondition, string? ColonyCount, IReadOnlyList<CultureSensitivityRowDto> Rows,
-    bool ParentIsReviewed, bool ParentIsPrinted, CultureMicroscopyDto? Microscopy = null);
+    bool ParentIsReviewed, bool ParentIsPrinted, CultureMicroscopyDto? Microscopy = null,
+    string? PatientNotes = null);
 
 public sealed record CultureReportDto(int PatientTestId, int PatientId, string PatientFullName,
     string? LabId, string PatientSex, int PatientAgeValue, string PatientAgeUnit,
diff --git a/src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs b/src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs
index dd0749e..46347e5 100644
--- a/src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs
+++ b/src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs
@@ -38,6 +38,7 @@ public sealed class GetCultureEntryGridQueryHandler : IRequestHandler<GetCulture
             .OrderBy(x => x.Id.Value).Select(x => { saved.TryGetValue(x.Id.Value, out var item); return new CultureSensitivityRowDto(item?.Id.Value, x.Id.Value, x.Name, item is null ? null : (int?)item.SensitivityCategory, x.IsPregnancyFlagged, x.IsChildrenFlagged, item?.InhibitionZoneMm); }).ToList();
         var header = _db.Set<CultureResult>().FirstOrDefault(x => x.PatientTestId.Value == pt.Id.Value);
         var micro = _db.Set<CultureMicroscopy>().FirstOrDefault(x => x.PatientTestId.Value == pt.Id.Value);
-        return Task.FromResult(Result<CultureEntryGridDto>.Success(new CultureEntryGridDto(pt.Id.Value, patient.Id.Value, test.Name, test.TestCode, header?.Sample, header?.OrganismA, header?.OrganismB, header?.OrganismC, header?.CultureCondition, header?.ColonyCount, rows, pt.IsReviewed, pt.IsPrinted, micro is null ? null : new CultureMicroscopyDto(micro.PusCells, micro.RedBloodCells, micro.EpithelialCells, micro.Crystals, micro.Fungi, micro.OthersOne, micro.OthersTwo, micro.OthersThree, micro.IsDirect))));
+        var dto = new CultureEntryGridDto(pt.Id.Value, patient.Id.Value, test.Name, test.TestCode, header?.Sample, header?.OrganismA, header?.OrganismB, header?.OrganismC, header?.CultureCondition, header?.ColonyCount, rows, pt.IsReviewed, pt.IsPrinted, micro is null ? null : new CultureMicroscopyDto(micro.PusCells, micro.RedBloodCells, micro.EpithelialCells, micro.Crystals, micro.Fungi, micro.OthersOne, micro.OthersTwo, micro.OthersThree, micro.IsDirect), patient.Notes);
+        return Task.FromResult(Result<CultureEntryGridDto>.Success(dto));
     }
 }
diff --git a/src/TopLab.Application/Features/ProfileResults/Common/ProfileResultDtos.cs b/src/TopLab.Application/Features/ProfileResults/Common/ProfileResultDtos.cs
index 014cfe0..15c3ee5 100644
--- a/src/TopLab.Application/Features/ProfileResults/Common/ProfileResultDtos.cs
+++ b/src/TopLab.Application/Features/ProfileResults/Common/ProfileResultDtos.cs
@@ -40,7 +40,8 @@ public sealed record ProfileEntryGridDto(
     string PatientFullName,
     string ProfileName,
     IReadOnlyList<ProfileEntryItemDto> Items,
-    int TestId = 0);
+    int TestId = 0,
+    string? PatientNotes = null);
 
 public sealed record ProfileReportLineDto(
     int ProfileResultItemId,
diff --git a/src/TopLab.Application/Features/ProfileResults/Queries/GetProfileEntryGrid/GetProfileEntryGridQueryHandler.cs b/src/TopLab.Application/Features/ProfileResults/Queries/GetProfileEntryGrid/GetProfileEntryGridQueryHandler.cs
index d17c27a..510f191 100644
--- a/src/TopLab.Application/Features/ProfileResults/Queries/GetProfileEntryGrid/GetProfileEntryGridQueryHandler.cs
+++ b/src/TopLab.Application/Features/ProfileResults/Queries/GetProfileEntryGrid/GetProfileEntryGridQueryHandler.cs
@@ -70,7 +70,8 @@ public sealed class GetProfileEntryGridQueryHandler
             patient.FullName,
             profile.Name,
             dtos,
-            pt.TestId.Value);
+            pt.TestId.Value,
+            patient.Notes);
 
         return Task.FromResult(Result<ProfileEntryGridDto>.Success(dto));
     }
diff --git a/src/TopLab.Application/Features/ResultDelivery/Common/ResultDeliveryDtos.cs b/src/TopLab.Application/Features/ResultDelivery/Common/ResultDeliveryDtos.cs
index 7938f20..688d905 100644
--- a/src/TopLab.Application/Features/ResultDelivery/Common/ResultDeliveryDtos.cs
+++ b/src/TopLab.Application/Features/ResultDelivery/Common/ResultDeliveryDtos.cs
@@ -50,4 +50,6 @@ public sealed record DeliveryAccountDto(
     decimal TotalPaid,
     decimal Balance,
     decimal RemainingToLab,
-    decimal RemainingToPatient);
+    decimal RemainingToPatient,
+    // STAGE R1 / GAP-04: the patient's own notes reach the delivery handover.
+    string? PatientNotes = null);
diff --git a/src/TopLab.Application/Features/ResultDelivery/Queries/GetDeliveryAccount/GetDeliveryAccountQueryHandler.cs b/src/TopLab.Application/Features/ResultDelivery/Queries/GetDeliveryAccount/GetDeliveryAccountQueryHandler.cs
index 57390d5..b6658bf 100644
--- a/src/TopLab.Application/Features/ResultDelivery/Queries/GetDeliveryAccount/GetDeliveryAccountQueryHandler.cs
+++ b/src/TopLab.Application/Features/ResultDelivery/Queries/GetDeliveryAccount/GetDeliveryAccountQueryHandler.cs
@@ -37,6 +37,7 @@ public sealed class GetDeliveryAccountQueryHandler
             account.TotalPaid,
             account.Balance,
             account.Balance > 0 ? account.Balance : 0m,
-            account.Balance < 0 ? -account.Balance : 0m)));
+            account.Balance < 0 ? -account.Balance : 0m,
+            patient.Notes)));
     }
 }
diff --git a/src/TopLab.Domain/Billing/PatientAccountCalculator.cs b/src/TopLab.Domain/Billing/PatientAccountCalculator.cs
index b2c15ce..476875e 100644
--- a/src/TopLab.Domain/Billing/PatientAccountCalculator.cs
+++ b/src/TopLab.Domain/Billing/PatientAccountCalculator.cs
@@ -1,3 +1,4 @@
+using TopLab.Domain.Common.Enums;
 using TopLab.Domain.Billing;
 
 namespace TopLab.Domain.Billing;
@@ -27,9 +28,43 @@ public static class PatientAccountCalculator
 
     public static decimal TotalPaid(IReadOnlyList<PaymentOperation> operations)
     {
-        return operations
-            .Where(o => !o.IsVoided && !o.IsExtraCharge)
+        var payments = operations
+            .Where(o => !o.IsVoided && !o.IsExtraCharge && o.OperationType != OperationType.Refund)
             .Sum(o => o.Amount + (o.DiscountAmount ?? 0m));
+        var refunds = operations
+            .Where(o => !o.IsVoided && o.OperationType == OperationType.Refund)
+            .Sum(o => o.RefundAmount ?? 0m);
+        return payments - refunds;
+    }
+
+    /// <summary>
+    /// The cash the patient actually handed over, and therefore the only money that can be
+    /// refunded (owner decision, R1 Round 5 Part B).
+    /// <para>
+    /// This is deliberately NOT <see cref="TotalPaid"/>: <c>TotalPaid</c> adds
+    /// <c>DiscountAmount</c> because a discount is money the patient no longer owes, which is
+    /// correct for the balance but wrong for a refund — a discount was never cash. Here the
+    /// discount is excluded, and so are extra charges (money the patient owes, not money they
+    /// paid) and refunds themselves.
+    /// </para>
+    /// <para>
+    /// Correction operations are treated exactly as <see cref="TotalPaid"/> treats them: a
+    /// correction is a non-voided, non-extra-charge, non-refund row, so its <c>Amount</c> counts
+    /// as paid cash and is therefore refundable. FullSettlement rows likewise count as ordinary
+    /// payments. Voided rows of any type contribute nothing. The result is floored at zero.
+    /// </para>
+    /// </summary>
+    public static decimal CashRefundable(IReadOnlyList<PaymentOperation> operations)
+    {
+        var cashPaid = operations
+            .Where(o => !o.IsVoided && !o.IsExtraCharge && o.OperationType != OperationType.Refund)
+            .Sum(o => o.Amount);
+        var refunds = operations
+            .Where(o => !o.IsVoided && o.OperationType == OperationType.Refund)
+            .Sum(o => o.RefundAmount ?? 0m);
+
+        var net = cashPaid - refunds;
+        return net > 0m ? net : 0m;
     }
 
     public static decimal Balance(
diff --git a/src/TopLab.Domain/Billing/PaymentOperation.cs b/src/TopLab.Domain/Billing/PaymentOperation.cs
index 3884360..678c493 100644
--- a/src/TopLab.Domain/Billing/PaymentOperation.cs
+++ b/src/TopLab.Domain/Billing/PaymentOperation.cs
@@ -12,6 +12,9 @@ public sealed class PaymentOperation : AuditableEntity<PaymentOperationId>
 
     public decimal? DiscountAmount { get; private set; }
 
+    /// <summary>Amount returned to the patient (refund). Null/0 for non-refund operations.</summary>
+    public decimal? RefundAmount { get; private set; }
+
     public bool IsExtraCharge { get; private set; }
 
     public OperationType OperationType { get; private set; }
@@ -40,7 +43,8 @@ public sealed class PaymentOperation : AuditableEntity<PaymentOperationId>
         bool isExtraCharge,
         OperationType operationType,
         int receivedByUserId,
-        DateTime operationAtUtc)
+        DateTime operationAtUtc,
+        decimal? refundAmount = null)
         : base(id)
     {
         PatientId = patientId;
@@ -50,6 +54,7 @@ public sealed class PaymentOperation : AuditableEntity<PaymentOperationId>
         OperationType = operationType;
         ReceivedByUserId = receivedByUserId;
         OperationAtUtc = operationAtUtc;
+        RefundAmount = refundAmount;
     }
 
     public static PaymentOperation Create(
@@ -60,7 +65,8 @@ public sealed class PaymentOperation : AuditableEntity<PaymentOperationId>
         DateTime operationAtUtc,
         decimal? discountAmount = null,
         bool isExtraCharge = false,
-        OperationType operationType = OperationType.Payment)
+        OperationType operationType = OperationType.Payment,
+        decimal? refundAmount = null)
     {
         if (amount < 0)
             throw new ArgumentException("Amount must be >= 0.", nameof(amount));
@@ -72,7 +78,19 @@ public sealed class PaymentOperation : AuditableEntity<PaymentOperationId>
             throw new ArgumentException("An extra charge cannot carry a discount.", nameof(discountAmount));
         if (operationType == OperationType.FullSettlement && amount <= 0)
             throw new ArgumentException("Settlement amount must be > 0.", nameof(amount));
-        return new PaymentOperation(id, patientId, amount, discountAmount, isExtraCharge, operationType, receivedByUserId, operationAtUtc);
+        if (operationType == OperationType.Refund)
+        {
+            if (refundAmount is null || refundAmount <= 0)
+                throw new ArgumentException("A refund operation requires RefundAmount > 0.", nameof(refundAmount));
+            if (amount != 0)
+                throw new ArgumentException("A refund operation must have Amount = 0.", nameof(amount));
+        }
+        else
+        {
+            if (refundAmount is not null)
+                throw new ArgumentException("Non-refund operations must not carry a RefundAmount.", nameof(refundAmount));
+        }
+        return new PaymentOperation(id, patientId, amount, discountAmount, isExtraCharge, operationType, receivedByUserId, operationAtUtc, refundAmount);
     }
 
     public void Void()
diff --git a/src/TopLab.Domain/Common/Enums/OperationType.cs b/src/TopLab.Domain/Common/Enums/OperationType.cs
index 8f9db40..8370156 100644
--- a/src/TopLab.Domain/Common/Enums/OperationType.cs
+++ b/src/TopLab.Domain/Common/Enums/OperationType.cs
@@ -4,5 +4,6 @@ public enum OperationType
 {
     Payment = 0,
     Correction = 1,
-    FullSettlement = 2
+    FullSettlement = 2,
+    Refund = 3
 }
diff --git a/src/TopLab.Infrastructure/DependencyInjection.cs b/src/TopLab.Infrastructure/DependencyInjection.cs
index d02e46f..545f257 100644
--- a/src/TopLab.Infrastructure/DependencyInjection.cs
+++ b/src/TopLab.Infrastructure/DependencyInjection.cs
@@ -111,6 +111,7 @@ public static class DependencyInjection
         services.AddScoped<IPdfPrinterDispatcher, ShellPdfPrinterDispatcher>();
         // WP-01: Arabic report document + on-screen preview (never prints).
         services.AddScoped<TopLab.Application.Features.ReportProduction.Common.IPdfPreviewService, PdfPreviewService>();
+        services.AddScoped<TopLab.Application.Features.ResultsEntry.Common.ISimpleResultPreviewBuilder, TopLab.Infrastructure.Printing.SimpleResultPreviewBuilder>();
         services.AddScoped<TopLab.Application.Features.ReportProduction.Common.ITestDisplayNameResolver, TestDisplayNameResolver>();
 
         // M-01 redacted connection descriptor: stateless, depends only on
diff --git a/src/TopLab.Infrastructure/Persistence/Configurations/PaymentOperationConfiguration.cs b/src/TopLab.Infrastructure/Persistence/Configurations/PaymentOperationConfiguration.cs
index 4d95a47..a083aac 100644
--- a/src/TopLab.Infrastructure/Persistence/Configurations/PaymentOperationConfiguration.cs
+++ b/src/TopLab.Infrastructure/Persistence/Configurations/PaymentOperationConfiguration.cs
@@ -14,6 +14,7 @@ public sealed class PaymentOperationConfiguration : IEntityTypeConfiguration<Pay
         b.Property(e => e.PatientId).HasConversion(v => v.Value, v => PatientId.Create(v)).IsRequired();
         b.Property(e => e.Amount).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
         b.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired(false);
+        b.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired(false);
         b.Property(e => e.IsExtraCharge).IsRequired();
         b.Property(e => e.OperationType).HasConversion<int>().HasColumnType("tinyint").IsRequired();
         b.Property(e => e.ReceivedByUserId).IsRequired();
diff --git a/src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs b/src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs
index 0eda02d..f23658d 100644
--- a/src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs
+++ b/src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs
@@ -200,6 +200,10 @@ namespace TopLab.Infrastructure.Persistence.Migrations
                     b.Property<int>("ReceivedByUserId")
                         .HasColumnType("int");
 
+                    b.Property<decimal?>("RefundAmount")
+                        .HasPrecision(18, 2)
+                        .HasColumnType("decimal(18,2)");
+
                     b.HasKey("Id");
 
                     b.HasIndex("PatientId");
diff --git a/src/TopLab.Presentation/App.xaml b/src/TopLab.Presentation/App.xaml
index 734dd78..c4694c0 100644
--- a/src/TopLab.Presentation/App.xaml
+++ b/src/TopLab.Presentation/App.xaml
@@ -1,8 +1,10 @@
 ﻿<Application x:Class="TopLab.Presentation.App"
              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
-             xmlns:local="clr-namespace:TopLab.Presentation">
+             xmlns:local="clr-namespace:TopLab.Presentation"
+             xmlns:converters="clr-namespace:TopLab.Presentation.Common.Converters">
     <Application.Resources>
         <BooleanToVisibilityConverter x:Key="BoolToVis" />
+        <converters:NullToVisibilityConverter x:Key="NullToVis" />
     </Application.Resources>
 </Application>
diff --git a/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs b/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs
index dc3f48c..2201cba 100644
--- a/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs
+++ b/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs
@@ -8,7 +8,9 @@ public sealed class NullToVisibilityConverter : IValueConverter
 {
     public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
     {
-        return value is null ? Visibility.Collapsed : Visibility.Visible;
+        if (value is null) return Visibility.Collapsed;
+        if (value is string s && string.IsNullOrWhiteSpace(s)) return Visibility.Collapsed;
+        return Visibility.Visible;
     }
 
     public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
diff --git a/src/TopLab.Presentation/DependencyInjection.cs b/src/TopLab.Presentation/DependencyInjection.cs
index 95eb6be..42213f6 100644
--- a/src/TopLab.Presentation/DependencyInjection.cs
+++ b/src/TopLab.Presentation/DependencyInjection.cs
@@ -1,6 +1,7 @@
 using Microsoft.Extensions.DependencyInjection;
 using TopLab.Application.Common.Interfaces;
 using TopLab.Presentation.Common.Navigation;
+using TopLab.Presentation.Common.Clipboard;
 using TopLab.Presentation.Common.Dialogs;
 using TopLab.Presentation.Common.ErrorPresentation;
 using TopLab.Presentation.Services;
@@ -28,6 +29,7 @@ public static class DependencyInjection
         // Services
         services.AddSingleton<INavigationService, NavigationService>();
         services.AddSingleton<IDialogService, DialogService>();
+        services.AddSingleton<IClipboardService, WpfClipboardService>();
         services.AddSingleton<ResultErrorPresenter>();
         services.AddSingleton<IPrinterCatalogService, PrinterCatalogService>();
         services.AddSingleton<TopLab.Presentation.Services.Configuration.ConfigurationFileService>();
diff --git a/src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
index 2e23ed2..4e712f4 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
@@ -97,6 +97,7 @@ public sealed class CultureEntryViewModel : ViewModelBase
     private bool _isBusy;
     private string _errorMessage = string.Empty;
     private string _statusMessage = string.Empty;
+    private string _patientNotes = string.Empty;
     private string? _pickedCommentPreview;
 
     public CultureEntryViewModel(
@@ -204,6 +205,12 @@ public sealed class CultureEntryViewModel : ViewModelBase
         private set => SetProperty(ref _statusMessage, value);
     }
 
+    public string PatientNotes
+    {
+        get => _patientNotes;
+        private set => SetProperty(ref _patientNotes, value);
+    }
+
     public AsyncRelayCommand SaveCommand { get; }
     public AsyncRelayCommand VerifyCommand { get; }
     public AsyncRelayCommand UnverifyCommand { get; }
@@ -259,6 +266,7 @@ public sealed class CultureEntryViewModel : ViewModelBase
                 IsDirectMicroscopy = grid.Microscopy?.IsDirect ?? false;
                 ParentIsReviewed = grid.ParentIsReviewed;
                 ParentIsPrinted = grid.ParentIsPrinted;
+                PatientNotes = grid.PatientNotes ?? string.Empty;
                 OnPropertyChanged(nameof(IsLocked));
 
                 var rows = new ObservableCollection<CultureSensitivityRow>();
diff --git a/src/TopLab.Presentation/ViewModels/Patients/DeliveryHandoverViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/DeliveryHandoverViewModel.cs
index 3dc6283..5fcbd86 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/DeliveryHandoverViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/DeliveryHandoverViewModel.cs
@@ -58,6 +58,7 @@ public sealed class DeliveryHandoverViewModel : ViewModelBase
     private bool _isBusy;
     private string _errorMessage = string.Empty;
     private string _statusMessage = string.Empty;
+    private string _patientNotes = string.Empty;
 
     public DeliveryHandoverViewModel(
         ISender mediator,
@@ -138,6 +139,12 @@ public sealed class DeliveryHandoverViewModel : ViewModelBase
         private set => SetProperty(ref _statusMessage, value);
     }
 
+    public string PatientNotes
+    {
+        get => _patientNotes;
+        private set => SetProperty(ref _patientNotes, value);
+    }
+
     public AsyncRelayCommand DeliverCommand { get; }
     public RelayCommand BackCommand { get; }
 
@@ -156,6 +163,7 @@ public sealed class DeliveryHandoverViewModel : ViewModelBase
             if (accountResult.IsSuccess && accountResult.Value is not null)
             {
                 Account = accountResult.Value;
+                PatientNotes = accountResult.Value.PatientNotes ?? string.Empty;
             }
             else if (accountResult.Error is not null)
             {
diff --git a/src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs
index 83cdd47..bbf97b7 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs
@@ -3,15 +3,19 @@ using System.Globalization;
 using MediatR;
 using Microsoft.Extensions.DependencyInjection;
 using TopLab.Application.Common.Results;
+using TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;
 using TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;
 using TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;
+using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
 using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
+using TopLab.Application.Features.PatientBilling.Commands.RecordRefund;
 using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
 using TopLab.Application.Features.PatientBilling.Commands.VoidPaymentOperation;
 using TopLab.Application.Features.PatientBilling.Common;
 using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
 using TopLab.Application.Features.PatientBilling.Queries.GetPatientInvoice;
 using TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;
+using TopLab.Domain.Billing;
 using TopLab.Presentation.Common;
 using TopLab.Presentation.Common.Dialogs;
 using TopLab.Presentation.Common.ErrorPresentation;
@@ -50,6 +54,9 @@ public sealed class PatientAccountViewModel : ViewModelBase
     private bool _showInvoicePreview;
     private string _paymentAmountText = string.Empty;
     private string? _paymentDiscountText;
+    private string? _paymentPercentageText;
+    private string _refundAmountText = string.Empty;
+    private readonly AccountEntrySession _accountEntry = new();
     private bool _isBusy;
     private string _errorMessage = string.Empty;
     private string _statusMessage = string.Empty;
@@ -72,6 +79,9 @@ public sealed class PatientAccountViewModel : ViewModelBase
         PrevOperationsPageCommand = new AsyncRelayCommand(async (_, ct) => await LoadOperationsPageAsync(nextPage: false, ct));
         RecordPaymentCommand = new AsyncRelayCommand(async (_, ct) => await RecordPaymentAsync(ct));
         SettleCommand = new AsyncRelayCommand(async (_, ct) => await SettleAsync(ct));
+        AccountEntryEnterCommand = new AsyncRelayCommand(async (_, ct) => await AccountEntryEnterAsync(ct));
+        RefundCommand = new AsyncRelayCommand(async (_, ct) => await RefundAsync(ct));
+        ClearAccountCommand = new AsyncRelayCommand(async (_, ct) => await ClearAccountAsync(ct));
         PrintReceiptCommand = new AsyncRelayCommand(async (_, ct) => await PrintReceiptAsync(ct));
         OpenCorrectionCommand = new AsyncRelayCommand(async (_, ct) => await OpenCorrectionAsync(ct));
         OpenExtraChargeCommand = new AsyncRelayCommand(async (_, ct) => await OpenExtraChargeAsync(ct));
@@ -121,9 +131,46 @@ public sealed class PatientAccountViewModel : ViewModelBase
     /// <summary>S-03 Slice 6: null-number case displays «لم تُصدَر بعد».</summary>
     public string InvoiceNumberDisplay => InvoicePreview?.InvoiceNumber?.ToString(CultureInfo.InvariantCulture) ?? "لم تُصدَر بعد";
 
-    public string PaymentAmountText { get => _paymentAmountText; set => SetProperty(ref _paymentAmountText, value); }
+    public string PaymentAmountText
+    {
+        get => _paymentAmountText;
+        set
+        {
+            if (SetProperty(ref _paymentAmountText, value))
+            {
+                _accountEntry.AmountText = value;
+            }
+        }
+    }
+
+    public string? PaymentDiscountText
+    {
+        get => _paymentDiscountText;
+        set
+        {
+            if (SetProperty(ref _paymentDiscountText, value))
+            {
+                _accountEntry.DiscountText = value;
+                PaymentPercentageText = null;
+            }
+        }
+    }
+
+    /// <summary>1.3.1.3 «%»: converted to an amount against the patient's TOTAL account amount.</summary>
+    public string? PaymentPercentageText
+    {
+        get => _paymentPercentageText;
+        set
+        {
+            if (SetProperty(ref _paymentPercentageText, value))
+            {
+                _accountEntry.PercentageText = value;
+            }
+        }
+    }
 
-    public string? PaymentDiscountText { get => _paymentDiscountText; set => SetProperty(ref _paymentDiscountText, value); }
+    /// <summary>1.3.2.5 «استرداد» amount entry.</summary>
+    public string RefundAmountText { get => _refundAmountText; set => SetProperty(ref _refundAmountText, value); }
 
     public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
 
@@ -141,6 +188,15 @@ public sealed class PatientAccountViewModel : ViewModelBase
 
     public AsyncRelayCommand SettleCommand { get; }
 
+    /// <summary>1.3.1.4.2: <c>Enter</c> in the amount / discount / percentage fields.</summary>
+    public AsyncRelayCommand AccountEntryEnterCommand { get; }
+
+    /// <summary>1.3.2.5 «استرداد».</summary>
+    public AsyncRelayCommand RefundCommand { get; }
+
+    /// <summary>1.3.2.7 «تصفير الحساب».</summary>
+    public AsyncRelayCommand ClearAccountCommand { get; }
+
     public AsyncRelayCommand PrintReceiptCommand { get; }
 
     public AsyncRelayCommand OpenCorrectionCommand { get; }
@@ -245,44 +301,177 @@ public sealed class PatientAccountViewModel : ViewModelBase
         OnPropertyChanged(nameof(ShowOperationsEmpty));
     }
 
-    /// <summary>Reuses the editor's record-payment flow (same texts).</summary>
+    /// <summary>
+    /// Reuses the editor's record-payment flow through the shared
+    /// <see cref="AccountEntryParser"/> (same texts, same surcharge convention, same
+    /// percentage conversion), so the two payment-entry screens cannot diverge.
+    /// </summary>
     private async Task RecordPaymentAsync(CancellationToken cancellationToken)
     {
         ErrorMessage = string.Empty;
         StatusMessage = string.Empty;
 
-        if (!decimal.TryParse(PaymentAmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
+        var amount = AccountEntryParser.ParsePaidAmount(PaymentAmountText);
+        if (!amount.IsValid)
         {
-            ErrorMessage = "قيمة المدفوع غير صالحة.";
+            ErrorMessage = amount.ErrorMessage!;
             return;
         }
 
-        decimal? discount = null;
-        if (!string.IsNullOrWhiteSpace(PaymentDiscountText))
+        var discount = AccountEntryParser.ParseDiscount(PaymentDiscountText);
+        if (!discount.IsValid)
         {
-            if (!decimal.TryParse(PaymentDiscountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) || d < 0)
+            ErrorMessage = discount.ErrorMessage!;
+            return;
+        }
+
+        var percentage = AccountEntryParser.ParsePercentage(PaymentPercentageText);
+        if (!percentage.IsValid)
+        {
+            ErrorMessage = percentage.ErrorMessage!;
+            return;
+        }
+
+        if (amount.Kind == AccountEntryKind.Surcharge && discount.Value > 0m)
+        {
+            ErrorMessage = AccountEntryParser.SurchargeWithDiscountMessage;
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
+            var result = amount.Kind == AccountEntryKind.Surcharge
+                ? await _mediator.Send(new RecordExtraChargeCommand(_patientId, amount.Value), cancellationToken)
+                : await _mediator.Send(
+                    new RecordPaymentCommand(_patientId, amount.Value, discount.Value > 0m ? discount.Value : null),
+                    cancellationToken);
+            if (!result.IsSuccess)
             {
-                ErrorMessage = "قيمة الخصم غير صالحة.";
+                ErrorMessage = _presenter.Present(result.Error!);
                 return;
             }
 
-            discount = d;
+            _accountEntry.MarkPersisted();
+            PaymentAmountText = string.Empty;
+            PaymentDiscountText = null;
+            PaymentPercentageText = null;
+            await RefreshAsync(cancellationToken);
+            StatusMessage = amount.Kind == AccountEntryKind.Surcharge ? "تم تسجيل المبلغ الإضافي." : "تم تسجيل المدفوع.";
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
+    /// <summary>
+    /// 1.3.1.4.2: one <c>Enter</c> press. The first press shows the resulting figures and
+    /// never reaches the mediator; the second press with unchanged inputs records through the
+    /// same command the record-payment button uses, then resets.
+    /// </summary>
+    private async Task AccountEntryEnterAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+
+        // 1.3.1.3: a percentage is converted against the patient's TOTAL account amount.
+        if (!string.IsNullOrWhiteSpace(PaymentPercentageText))
+        {
+            var conversionError = _accountEntry.ApplyPercentage(TotalCharged);
+            if (conversionError is not null)
+            {
+                ErrorMessage = conversionError;
+                return;
+            }
+
+            // Round 4 fix: push the converted amount into the field through the backing
+            // field, not through the PaymentDiscountText setter. That setter exists to clear
+            // the percentage on a USER edit, and routing the conversion through it would
+            // clear the percentage (and, by the session's rule, the amount it just produced).
+            _paymentDiscountText = _accountEntry.DiscountText;
+            _paymentPercentageText = null;
+            OnPropertyChanged(nameof(PaymentDiscountText));
+            OnPropertyChanged(nameof(PaymentPercentageText));
+        }
+
+        var step = _accountEntry.PressEnter(TotalCharged, TotalPaid);
+        if (!step.IsValid)
+        {
+            ErrorMessage = step.ErrorMessage!;
+            return;
+        }
+
+        if (!step.ShouldPersist)
+        {
+            StatusMessage = step.StatusMessage!;
+            return;
+        }
+
+        await RecordPaymentAsync(cancellationToken);
+    }
+
+    /// <summary>1.3.2.5 «استرداد»: records a refund of the entered amount.</summary>
+    private async Task RefundAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+
+        var parsed = AccountEntryParser.ParseRefundAmount(RefundAmountText);
+        if (!parsed.IsValid)
+        {
+            ErrorMessage = parsed.ErrorMessage!;
+            return;
         }
 
         IsBusy = true;
         try
         {
-            var result = await _mediator.Send(new RecordPaymentCommand(_patientId, amount, discount), cancellationToken);
+            var result = await _mediator.Send(new RecordRefundCommand(_patientId, parsed.Value), cancellationToken);
+            if (!result.IsSuccess)
+            {
+                ErrorMessage = _presenter.Present(result.Error!);
+                return;
+            }
+
+            RefundAmountText = string.Empty;
+            await RefreshAsync(cancellationToken);
+            StatusMessage = "تم تسجيل الاسترداد.";
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
+    /// <summary>1.3.2.7 «تصفير الحساب»: voids every operation after a confirmation.</summary>
+    private async Task ClearAccountAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+
+        var confirm = await _dialogs.ShowConfirmationAsync(
+            "تصفير الحساب",
+            "سيتم إلغاء جميع عمليات حساب المريض (تبقى ظاهرة بعلامة ملغاة). هل تريد المتابعة؟");
+        if (!confirm)
+        {
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
+            var result = await _mediator.Send(new ClearPatientAccountCommand(_patientId), cancellationToken);
             if (!result.IsSuccess)
             {
                 ErrorMessage = _presenter.Present(result.Error!);
                 return;
             }
 
-            PaymentAmountText = string.Empty;
-            PaymentDiscountText = null;
             await RefreshAsync(cancellationToken);
-            StatusMessage = "تم تسجيل المدفوع.";
+            StatusMessage = result.Value > 0
+                ? $"تم تصفير الحساب ({result.Value} عملية)."
+                : "لا توجد عمليات لتصفيرها.";
         }
         finally
         {
diff --git a/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs
index 196c0c4..007356a 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs
@@ -5,11 +5,15 @@ using Microsoft.Extensions.DependencyInjection;
 using TopLab.Application.Common.Results;
 using TopLab.Application.Features.AnalyteProfiles.Common;
 using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
+using TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;
 using TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;
 using TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;
+using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
 using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
+using TopLab.Application.Features.PatientBilling.Commands.RecordRefund;
 using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
 using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
+using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
 using TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;
 using TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;
 using TopLab.Application.Features.PatientRegistration.Commands.ApplyConditionDeltas;
@@ -31,12 +35,14 @@ using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
 using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
 using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
 using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
+using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;
 using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
 using TopLab.Application.Features.PatientRegistration.Queries.SearchPatients;
 using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintResultPassword;
 using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
 using TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;
 using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
+using TopLab.Domain.Billing;
 using TopLab.Domain.Common.Enums;
 using TopLab.Domain.Patients;
 using TopLab.Presentation.Common;
@@ -102,6 +108,12 @@ public sealed class PatientEditorViewModel : ViewModelBase
     private string? _notes;
     private string _paymentAmountText = string.Empty;
     private string? _paymentDiscountText;
+    private string? _paymentPercentageText;
+    private string _refundAmountText = string.Empty;
+    private readonly AccountEntrySession _accountEntry = new();
+    private string _catalogFilterText = string.Empty;
+    private CatalogFilterMode _catalogFilterMode = CatalogFilterMode.Tg;
+    private HashSet<int> _customGroupTestIds = new();
     private decimal _totalCharged;
     private decimal _totalDiscount;
     private decimal _totalPaid;
@@ -144,11 +156,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
         UndoCommand = new AsyncRelayCommand(async (_, ct) => await UndoAsync());
         RecordPaymentCommand = new AsyncRelayCommand(async (_, ct) => await RecordPaymentAsync(ct));
         SettleCommand = new AsyncRelayCommand(async (_, ct) => await SettleAsync(ct));
+        AccountEntryEnterCommand = new AsyncRelayCommand(async (_, ct) => await AccountEntryEnterAsync(ct));
+        RefundCommand = new AsyncRelayCommand(async (_, ct) => await RefundAsync(ct));
+        ClearAccountCommand = new AsyncRelayCommand(async (_, ct) => await ClearAccountAsync(ct));
         PrintBarcodeCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "barcode"));
         PrintWorkSheetCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "worksheet"));
         PrintReceiptCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "receipt"));
         PrintInvoiceCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "invoice"));
         PrintLabOrderCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "laborder"));
+        PrintEnvelopeCommand = new AsyncRelayCommand(async (_, ct) => await PrintEnvelopeAsync(ct));
         AddTestToSelectionCommand = new RelayCommand(param =>
         {
             if (param is int testId)
@@ -172,8 +188,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
         NextSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct, nextPage: true));
         PrevSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct));
         ClearSearchCommand = new RelayCommand(ClearSearch);
+        ClearCatalogFilterCommand = new RelayCommand(_ => CatalogFilterText = string.Empty);
+        SetTmModeCommand = new RelayCommand(_ => CatalogFilterMode = CatalogFilterMode.Tm);
+        SetTgModeCommand = new RelayCommand(_ => CatalogFilterMode = CatalogFilterMode.Tg);
+        SetCgModeCommand = new RelayCommand(_ => CatalogFilterMode = CatalogFilterMode.Cg);
         OpenSearchResultCommand = new AsyncRelayCommand(async (param, ct) => await OpenSearchResultAsync(param as PatientSummaryDto, ct));
         OpenAccountCommand = new AsyncRelayCommand(async (_, ct) => await OpenAccountAsync(ct));
+        PatientWindowCommand = new AsyncRelayCommand((_, ct) => PatientWindowAsync(ct));
+        LabResultsCommand = new AsyncRelayCommand(async (_, ct) => await LabResultsAsync(ct));
+        FollowUpCommand = new AsyncRelayCommand(async (_, ct) => await FollowUpAsync(ct));
         AddProfileCommand = new AsyncRelayCommand(async (_, ct) => await AddProfileAsync(ct));
         AddCustomGroupCommand = new AsyncRelayCommand(async (_, ct) => await AddCustomGroupAsync(ct));
         ClearAllVisitTestsCommand = new AsyncRelayCommand(async (_, ct) => await ClearAllVisitTestsAsync(ct));
@@ -244,8 +267,115 @@ public sealed class PatientEditorViewModel : ViewModelBase
         public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
     }
 
+    /// <summary>
+    /// The full catalogue as loaded (1.2.2.5.1.1 — the master test list). The filtered view
+    /// below is what the ListView binds to; this collection is never narrowed so clearing the
+    /// filter restores every test without a round trip.
+    /// </summary>
     public ObservableCollection<TestSummaryDto> CatalogTests { get; } = new();
 
+    /// <summary>
+    /// 1.2.2.5.1.1 «start» — the test filter field of the master test list. A non-empty value
+    /// narrows <see cref="FilteredCatalogTests"/> by test name OR test code, case-insensitively.
+    /// </summary>
+    public string CatalogFilterText
+    {
+        get => _catalogFilterText;
+        set
+        {
+            if (SetProperty(ref _catalogFilterText, value))
+            {
+                ApplyCatalogFilter();
+            }
+        }
+    }
+
+    /// <summary>The catalogue rows the screen shows: all tests, or those matching the filter.</summary>
+    public ObservableCollection<TestSummaryDto> FilteredCatalogTests { get; } = new();
+
+    /// <summary>True when a filter is active and it matched nothing (distinct from an empty catalogue).</summary>
+    public bool ShowCatalogFilterNoMatch
+        => _catalogFilterMode == CatalogFilterMode.Tg
+           && !string.IsNullOrWhiteSpace(_catalogFilterText)
+           && FilteredCatalogTests.Count == 0;
+
+    /// <summary>
+    /// D-02: the active catalogue mode. Exactly one mode is active at any time, and an undefined
+    /// value is REJECTED with the property left unchanged (GAP-11 acceptance criterion:
+    /// "mode options are mutually exclusive; invalid mode is rejected").
+    /// </summary>
+    public CatalogFilterMode CatalogFilterMode
+    {
+        get => _catalogFilterMode;
+        set
+        {
+            if (!Enum.IsDefined(typeof(CatalogFilterMode), value))
+            {
+                // Rejected: state unchanged. The setter re-raises so a bound control snaps back.
+                OnPropertyChanged(nameof(CatalogFilterMode));
+                OnPropertyChanged(nameof(IsTmMode));
+                OnPropertyChanged(nameof(IsTgMode));
+                OnPropertyChanged(nameof(IsCgMode));
+                return;
+            }
+
+            if (_catalogFilterMode == value)
+            {
+                return;
+            }
+
+            var leavingTg = _catalogFilterMode == CatalogFilterMode.Tg;
+            _catalogFilterMode = value;
+
+            // Design detail (D-02): the search box is active in TG only, so leaving TG clears the
+            // text — no hidden state survives the switch.
+            if (leavingTg)
+            {
+                _catalogFilterText = string.Empty;
+                OnPropertyChanged(nameof(CatalogFilterText));
+            }
+
+            OnPropertyChanged(nameof(CatalogFilterMode));
+            OnPropertyChanged(nameof(IsTmMode));
+            OnPropertyChanged(nameof(IsTgMode));
+            OnPropertyChanged(nameof(IsCgMode));
+            OnPropertyChanged(nameof(IsCatalogSearchEnabled));
+
+            ApplyCatalogFilter();
+        }
+    }
+
+    /// <summary>TM «كل التحاليل».</summary>
+    public bool IsTmMode
+    {
+        get => _catalogFilterMode == CatalogFilterMode.Tm;
+        set { if (value) CatalogFilterMode = CatalogFilterMode.Tm; }
+    }
+
+    /// <summary>TG «بحث بالاسم أو الكود».</summary>
+    public bool IsTgMode
+    {
+        get => _catalogFilterMode == CatalogFilterMode.Tg;
+        set { if (value) CatalogFilterMode = CatalogFilterMode.Tg; }
+    }
+
+    /// <summary>CG «مجموعاتي المخصصة».</summary>
+    public bool IsCgMode
+    {
+        get => _catalogFilterMode == CatalogFilterMode.Cg;
+        set { if (value) CatalogFilterMode = CatalogFilterMode.Cg; }
+    }
+
+    /// <summary>D-02: the search box is enabled in TG only; its text is ignored in TM and CG.</summary>
+    public bool IsCatalogSearchEnabled => _catalogFilterMode == CatalogFilterMode.Tg;
+
+    /// <summary>TM / TG / CG commands (bound to the three buttons; no code-behind).</summary>
+    public RelayCommand SetTmModeCommand { get; private set; } = null!;
+
+    public RelayCommand SetTgModeCommand { get; private set; } = null!;
+
+    public RelayCommand SetCgModeCommand { get; private set; } = null!;
+
     public ObservableCollection<TestGroupDto> TestGroups { get; } = new();
 
     public ObservableCollection<string> Titles { get; } = new();
@@ -537,9 +667,47 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }
 
-    public string PaymentAmountText { get => _paymentAmountText; set => SetProperty(ref _paymentAmountText, value); }
+    public string PaymentAmountText
+    {
+        get => _paymentAmountText;
+        set
+        {
+            if (SetProperty(ref _paymentAmountText, value))
+            {
+                _accountEntry.AmountText = value;
+            }
+        }
+    }
 
-    public string? PaymentDiscountText { get => _paymentDiscountText; set => SetProperty(ref _paymentDiscountText, value); }
+    public string? PaymentDiscountText
+    {
+        get => _paymentDiscountText;
+        set
+        {
+            if (SetProperty(ref _paymentDiscountText, value))
+            {
+                // 1.3.1.2 / 1.3.1.3: the value and the percentage are never applied together.
+                _accountEntry.DiscountText = value;
+                PaymentPercentageText = null;
+            }
+        }
+    }
+
+    /// <summary>1.3.1.3 «%»: converted to an amount against the patient's TOTAL account amount.</summary>
+    public string? PaymentPercentageText
+    {
+        get => _paymentPercentageText;
+        set
+        {
+            if (SetProperty(ref _paymentPercentageText, value))
+            {
+                _accountEntry.PercentageText = value;
+            }
+        }
+    }
+
+    /// <summary>1.3.2.5 «استرداد» amount entry.</summary>
+    public string RefundAmountText { get => _refundAmountText; set => SetProperty(ref _refundAmountText, value); }
 
     public decimal TotalCharged { get => _totalCharged; private set => SetProperty(ref _totalCharged, value); }
 
@@ -554,15 +722,67 @@ public sealed class PatientEditorViewModel : ViewModelBase
     /// <summary>S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).</summary>
     public bool ShowSelectedTestsEmpty => SelectedTests.Count == 0;
 
-    /// <summary>S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).</summary>
+    /// <summary>
+    /// S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).
+    /// D-02: this means "the catalogue itself is empty" — it is deliberately based on the FULL
+    /// set, so a mode that hides rows (CG with no groups, TG with no match) does not claim the
+    /// catalogue is empty. Each mode has its own message for that.
+    /// </summary>
     public bool ShowCatalogEmpty => CatalogTests.Count == 0;
 
     private void RefreshEmptyStates()
     {
         OnPropertyChanged(nameof(ShowSelectedTestsEmpty));
         OnPropertyChanged(nameof(ShowCatalogEmpty));
+        OnPropertyChanged(nameof(ShowCatalogFilterNoMatch));
+    }
+
+    /// <summary>
+    /// 1.2.2.5.1.1: rebuilds <see cref="FilteredCatalogTests"/> from
+    /// <see cref="CatalogTests"/> using the current filter text. Matching is on the test name
+    /// OR the test code, case-insensitively, so both "CBC" and "صورة دم" find the same test.
+    /// An empty or whitespace filter shows the whole catalogue.
+    /// </summary>
+    private void ApplyCatalogFilter()
+    {
+        // D-02: the search text only takes effect in TG. In TM and CG it is ignored, so the
+        // modes stay mutually exclusive even if a stale text survived somewhere.
+        var needle = _catalogFilterMode == CatalogFilterMode.Tg
+            ? (_catalogFilterText ?? string.Empty).Trim()
+            : string.Empty;
+
+        FilteredCatalogTests.Clear();
+        foreach (var test in CatalogTests)
+        {
+            var visible = _catalogFilterMode switch
+            {
+                // TM: every test the catalogue already shows — the same base set as before.
+                CatalogFilterMode.Tm => true,
+                // TG: the search box; an empty box means all tests.
+                CatalogFilterMode.Tg => needle.Length == 0 || MatchesCatalogFilter(test, needle),
+                // CG: only tests belonging to at least one custom group.
+                CatalogFilterMode.Cg => _customGroupTestIds.Contains(test.Id),
+                _ => true
+            };
+
+            if (visible)
+            {
+                FilteredCatalogTests.Add(test);
+            }
+        }
+
+        OnPropertyChanged(nameof(ShowCatalogEmpty));
+        OnPropertyChanged(nameof(ShowCatalogFilterNoMatch));
     }
 
+    /// <summary>Name OR code, case-insensitive, substring match.</summary>
+    private static bool MatchesCatalogFilter(TestSummaryDto test, string needle)
+        => test.Name.Contains(needle, StringComparison.OrdinalIgnoreCase)
+           || test.TestCode.Contains(needle, StringComparison.OrdinalIgnoreCase);
+
+    /// <summary>Clears the filter (the «مسح» button next to the filter field).</summary>
+    public RelayCommand ClearCatalogFilterCommand { get; }
+
     public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
 
     public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
@@ -626,6 +846,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     public AsyncRelayCommand SettleCommand { get; }
 
+    /// <summary>1.3.1.4.2: the account panel's <c>Enter</c> key (first press computes, second saves).</summary>
+    public AsyncRelayCommand AccountEntryEnterCommand { get; }
+
+    /// <summary>1.3.2.5 «استرداد».</summary>
+    public AsyncRelayCommand RefundCommand { get; }
+
+    /// <summary>1.3.2.7 «تصفير الحساب».</summary>
+    public AsyncRelayCommand ClearAccountCommand { get; }
+
     public AsyncRelayCommand PrintBarcodeCommand { get; }
 
     public AsyncRelayCommand PrintWorkSheetCommand { get; }
@@ -636,6 +865,7 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     /// <summary>Phase 1 REF-068: laboratory-order slip print action.</summary>
     public AsyncRelayCommand PrintLabOrderCommand { get; }
+    public AsyncRelayCommand PrintEnvelopeCommand { get; }
 
     public RelayCommand AddTestToSelectionCommand { get; }
 
@@ -653,6 +883,16 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     /// <summary>S-03 Slice 4: totals-row entry into the patient account screen.</summary>
     public AsyncRelayCommand OpenAccountCommand { get; }
+    public AsyncRelayCommand PatientWindowCommand { get; }
+    public AsyncRelayCommand LabResultsCommand { get; }
+
+    /// <summary>
+    /// 1.2.2.6.10 «Follow-up»: opens the case follow-up screen — reference section 1.12
+    /// "Case Follow-up and Audit Trail", screen 1.12.1 "Case History Screen", which this
+    /// codebase implements as <see cref="TopLab.Presentation.ViewModels.Audit.AuditViewModel"/>
+    /// (its P tab is the per-patient case history).
+    /// </summary>
+    public AsyncRelayCommand FollowUpCommand { get; }
 
     public AsyncRelayCommand AddProfileCommand { get; }
 
@@ -679,6 +919,8 @@ public sealed class PatientEditorViewModel : ViewModelBase
                 CatalogTests.Add(test);
             }
 
+            ApplyCatalogFilter();
+
             TestGroups.Clear();
             foreach (var group in catalog.Value.TestGroups)
             {
@@ -769,6 +1011,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
             }
         }
 
+        // D-02 mode CG: the registration catalogue does not carry custom-group membership, so the
+        // union of member test ids is read once here. Read-only, no schema change; a failure is
+        // non-fatal (CG simply shows nothing) exactly like the other picker sources.
+        var memberIds = await _mediator.Send(new GetCustomGroupTestIdsQuery(), cancellationToken);
+        if (memberIds.IsSuccess && memberIds.Value is not null)
+        {
+            _customGroupTestIds = new HashSet<int>(memberIds.Value);
+        }
+
         var groups = await _mediator.Send(new GetCustomGroupsQuery(), cancellationToken);
         if (!groups.IsSuccess)
         {
@@ -1488,6 +1739,59 @@ public sealed class PatientEditorViewModel : ViewModelBase
         }
     }
 
+    /// <summary>
+    /// GAP-10 / R1 Round 3 A1: the patient list opens in add mode as well as edit mode,
+    /// so the unsaved-patient guard is deliberately absent here (it stays on
+    /// <see cref="LabResultsAsync"/>). No unsaved-data confirmation exists elsewhere in the
+    /// editor's navigation actions, so none is invented.
+    /// </summary>
+    private Task PatientWindowAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        _navigation.NavigateTo<PatientSearchViewModel>();
+        return Task.CompletedTask;
+    }
+
+    private async Task LabResultsAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!IsEditMode || !_patientId.HasValue)
+        {
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل عرض النتائج.";
+            return;
+        }
+
+        _navigation.NavigateTo<PatientResultSheetViewModel>();
+        if (_navigation.CurrentViewModel is PatientResultSheetViewModel sheet)
+        {
+            await sheet.LoadAsync(_patientId.Value, cancellationToken);
+        }
+    }
+
+    /// <summary>
+    /// 1.2.2.6.10: the case follow-up screen for the loaded patient. Same guard and the same
+    /// navigate-then-load idiom as the account and lab-results actions, so an unsaved patient
+    /// gets the editor's standard "save first" message rather than a half-loaded history.
+    /// </summary>
+    private async Task FollowUpAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!IsEditMode || !_patientId.HasValue)
+        {
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل عرض المتابعة.";
+            return;
+        }
+
+        _navigation.NavigateTo<TopLab.Presentation.ViewModels.Audit.AuditViewModel>();
+        if (_navigation.CurrentViewModel is TopLab.Presentation.ViewModels.Audit.AuditViewModel audit)
+        {
+            await audit.LoadWithPatientIdAsync(_patientId.Value, cancellationToken);
+        }
+    }
+
     /// <summary>S-03 Slice 3: wires the orphaned AddProfileToVisitCommand (edit mode only).</summary>
     private async Task AddProfileAsync(CancellationToken cancellationToken)
     {
@@ -1617,6 +1921,13 @@ public sealed class PatientEditorViewModel : ViewModelBase
         }
     }
 
+    /// <summary>
+    /// R1 Round 3 B1/B2: the record-payment button. Parsing and the percentage conversion run
+    /// through the shared <see cref="AccountEntryParser"/> / <see cref="AccountEntryCalculator"/>
+    /// so this screen and the patient-account screen behave identically. A leading <c>+</c>
+    /// routes to the existing extra-charge mechanism (1.3.1.4.1 / OD-11); the existing separate
+    /// extra-charge action is kept.
+    /// </summary>
     private async Task RecordPaymentAsync(CancellationToken cancellationToken)
     {
         ErrorMessage = string.Empty;
@@ -1627,38 +1938,184 @@ public sealed class PatientEditorViewModel : ViewModelBase
             return;
         }
 
-        if (!decimal.TryParse(PaymentAmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
+        var amount = AccountEntryParser.ParsePaidAmount(PaymentAmountText);
+        if (!amount.IsValid)
+        {
+            ErrorMessage = amount.ErrorMessage!;
+            return;
+        }
+
+        var discount = AccountEntryParser.ParseDiscount(PaymentDiscountText);
+        if (!discount.IsValid)
+        {
+            ErrorMessage = discount.ErrorMessage!;
+            return;
+        }
+
+        var percentage = AccountEntryParser.ParsePercentage(PaymentPercentageText);
+        if (!percentage.IsValid)
+        {
+            ErrorMessage = percentage.ErrorMessage!;
+            return;
+        }
+
+        if (amount.Kind == AccountEntryKind.Surcharge && discount.Value > 0m)
+        {
+            ErrorMessage = AccountEntryParser.SurchargeWithDiscountMessage;
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
+            var result = amount.Kind == AccountEntryKind.Surcharge
+                ? await _mediator.Send(new RecordExtraChargeCommand(_patientId.Value, amount.Value), cancellationToken)
+                : await _mediator.Send(
+                    new RecordPaymentCommand(_patientId.Value, amount.Value, discount.Value > 0m ? discount.Value : null),
+                    cancellationToken);
+            if (!result.IsSuccess)
+            {
+                ErrorMessage = _presenter.Present(result.Error!);
+                return;
+            }
+
+            _accountEntry.MarkPersisted();
+            PaymentAmountText = string.Empty;
+            PaymentDiscountText = null;
+            PaymentPercentageText = null;
+            await RefreshBillingAsync(cancellationToken);
+            StatusMessage = amount.Kind == AccountEntryKind.Surcharge ? "تم تسجيل المبلغ الإضافي." : "تم تسجيل المدفوع.";
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
+    /// <summary>
+    /// 1.3.1.4.2: one <c>Enter</c> press in the amount / discount / percentage fields.
+    /// The first press only displays the resulting figures and never reaches the mediator;
+    /// the second press with unchanged inputs routes to the same record-payment path the
+    /// button uses, then resets.
+    /// </summary>
+    private async Task AccountEntryEnterAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!IsEditMode || !_patientId.HasValue)
+        {
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل تسجيل مدفوعات.";
+            return;
+        }
+
+        // 1.3.1.3: a percentage is converted against the patient's TOTAL account amount.
+        if (!string.IsNullOrWhiteSpace(PaymentPercentageText))
+        {
+            var conversionError = _accountEntry.ApplyPercentage(TotalCharged);
+            if (conversionError is not null)
+            {
+                ErrorMessage = conversionError;
+                return;
+            }
+
+            // Round 4 fix: push the converted amount into the field through the backing
+            // field, not through the PaymentDiscountText setter. That setter exists to clear
+            // the percentage on a USER edit, and routing the conversion through it would
+            // clear the percentage (and, by the session's rule, the amount it just produced).
+            _paymentDiscountText = _accountEntry.DiscountText;
+            _paymentPercentageText = null;
+            OnPropertyChanged(nameof(PaymentDiscountText));
+            OnPropertyChanged(nameof(PaymentPercentageText));
+        }
+
+        var step = _accountEntry.PressEnter(TotalCharged, TotalPaid);
+        if (!step.IsValid)
+        {
+            ErrorMessage = step.ErrorMessage!;
+            return;
+        }
+
+        if (!step.ShouldPersist)
+        {
+            // First press: figures only, nothing recorded.
+            StatusMessage = step.StatusMessage!;
+            return;
+        }
+
+        await RecordPaymentAsync(cancellationToken);
+    }
+
+    /// <summary>1.3.2.5 «استرداد»: records a refund of the entered amount (never overwrites).</summary>
+    private async Task RefundAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!IsEditMode || !_patientId.HasValue)
+        {
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل تسجيل استرداد.";
+            return;
+        }
+
+        var parsed = AccountEntryParser.ParseRefundAmount(RefundAmountText);
+        if (!parsed.IsValid)
         {
-            ErrorMessage = "قيمة المدفوع غير صالحة.";
+            ErrorMessage = parsed.ErrorMessage!;
             return;
         }
 
-        decimal? discount = null;
-        if (!string.IsNullOrWhiteSpace(PaymentDiscountText))
+        IsBusy = true;
+        try
         {
-            if (!decimal.TryParse(PaymentDiscountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) || d < 0)
+            var result = await _mediator.Send(new RecordRefundCommand(_patientId.Value, parsed.Value), cancellationToken);
+            if (!result.IsSuccess)
             {
-                ErrorMessage = "قيمة الخصم غير صالحة.";
+                ErrorMessage = _presenter.Present(result.Error!);
                 return;
             }
 
-            discount = d;
+            RefundAmountText = string.Empty;
+            await RefreshBillingAsync(cancellationToken);
+            StatusMessage = "تم تسجيل الاسترداد.";
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
+    /// <summary>1.3.2.7 «تصفير الحساب»: voids every operation after a confirmation.</summary>
+    private async Task ClearAccountAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!IsEditMode || !_patientId.HasValue)
+        {
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل تصفير الحساب.";
+            return;
+        }
+
+        var confirm = await _dialogs.ShowConfirmationAsync(
+            "تصفير الحساب",
+            "سيتم إلغاء جميع عمليات حساب المريض (تبقى ظاهرة بعلامة ملغاة). هل تريد المتابعة؟");
+        if (!confirm)
+        {
+            return;
         }
 
         IsBusy = true;
         try
         {
-            var result = await _mediator.Send(new RecordPaymentCommand(_patientId.Value, amount, discount), cancellationToken);
+            var result = await _mediator.Send(new ClearPatientAccountCommand(_patientId.Value), cancellationToken);
             if (!result.IsSuccess)
             {
                 ErrorMessage = _presenter.Present(result.Error!);
                 return;
             }
 
-            PaymentAmountText = string.Empty;
-            PaymentDiscountText = null;
             await RefreshBillingAsync(cancellationToken);
-            StatusMessage = "تم تسجيل المدفوع.";
+            StatusMessage = result.Value > 0
+                ? $"تم تصفير الحساب ({result.Value} عملية)."
+                : "لا توجد عمليات لتصفيرها.";
         }
         finally
         {
@@ -1695,6 +2152,36 @@ public sealed class PatientEditorViewModel : ViewModelBase
         }
     }
 
+    /// <summary>GAP-10: bind the existing envelope-print feature on the patient editor action band.</summary>
+    private async Task PrintEnvelopeAsync(CancellationToken cancellationToken)
+    {
+        if (!IsEditMode || !_patientId.HasValue)
+        {
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل الطباعة.";
+            return;
+        }
+
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        IsBusy = true;
+        try
+        {
+            var result = await _mediator.Send(new PrintEnvelopeCommand(_patientId.Value), cancellationToken);
+            if (result.IsSuccess)
+            {
+                StatusMessage = "تمت طباعة المغلّف.";
+            }
+            else if (result.Error is not null)
+            {
+                ErrorMessage = _presenter.Present(result.Error);
+            }
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
     private async Task PrintAsync(CancellationToken cancellationToken, string document)
     {
         ErrorMessage = string.Empty;
diff --git a/src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs
index facc15c..3e5bc46 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs
@@ -94,6 +94,7 @@ public sealed class ProfileEntryViewModel : ViewModelBase
     private bool _isBusy;
     private string _errorMessage = string.Empty;
     private string _statusMessage = string.Empty;
+    private string _patientNotes = string.Empty;
     private ProfileEntryRow? _selectedRow;
 
     public ProfileEntryViewModel(
@@ -191,6 +192,12 @@ public sealed class ProfileEntryViewModel : ViewModelBase
         private set => SetProperty(ref _statusMessage, value);
     }
 
+    public string PatientNotes
+    {
+        get => _patientNotes;
+        private set => SetProperty(ref _patientNotes, value);
+    }
+
     public AsyncRelayCommand SaveCommand { get; }
     public AsyncRelayCommand VerifyCommand { get; }
     public AsyncRelayCommand UnverifyCommand { get; }
@@ -216,6 +223,7 @@ public sealed class ProfileEntryViewModel : ViewModelBase
                 PatientFullName = result.Value.PatientFullName;
                 ProfileName = result.Value.ProfileName;
                 Comment = string.Empty;
+                PatientNotes = result.Value.PatientNotes ?? string.Empty;
                 _testId = result.Value.TestId;
 
                 var rows = new ObservableCollection<ProfileEntryRow>();
diff --git a/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs
index 38b2efc..95384c8 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs
@@ -2,7 +2,9 @@ using System.Collections.ObjectModel;
 using MediatR;
 using TopLab.Application.Features.ResultDelivery.Common;
 using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
+using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
 using TopLab.Presentation.Common;
+using TopLab.Presentation.Common.Clipboard;
 using TopLab.Presentation.Common.ErrorPresentation;
 using TopLab.Presentation.Common.Navigation;
 
@@ -17,6 +19,7 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
     private readonly ISender _mediator;
     private readonly ResultErrorPresenter _presenter;
     private readonly INavigationService _navigation;
+    private readonly IClipboardService _clipboard;
 
     private DateOnly? _from = DateOnly.FromDateTime(DateTime.UtcNow);
     private DateOnly? _to = DateOnly.FromDateTime(DateTime.UtcNow);
@@ -27,15 +30,26 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
     private bool _isBusy;
     private string _errorMessage = string.Empty;
 
-    public ResultDeliveryViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
+    public ResultDeliveryViewModel(
+        ISender mediator,
+        ResultErrorPresenter presenter,
+        INavigationService navigation,
+        IClipboardService clipboard)
     {
         _mediator = mediator;
         _presenter = presenter;
         _navigation = navigation;
+        _clipboard = clipboard;
 
         LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
         OpenHandoverCommand = new RelayCommand(param => OpenHandover(param as UndeliveredPatientRowDto));
         OpenGroupSlipsCommand = new RelayCommand(_ => OpenGroupSlips());
+        PatientAccountCommand = new RelayCommand(param => OpenPatientAccount(param as UndeliveredPatientRowDto), HasRow);
+        CombinedReportCommand = new RelayCommand(param => OpenCombinedReport(param as UndeliveredPatientRowDto), HasRow);
+        EmptyReportCommand = new RelayCommand(param => OpenEmptyReport(param as UndeliveredPatientRowDto), HasRow);
+        CopyCommand = new RelayCommand(param => CopyRow(param as UndeliveredPatientRowDto), HasRow);
+        PatientDataCommand = new RelayCommand(param => OpenPatientData(param as UndeliveredPatientRowDto), HasRow);
+        PrintEnvelopeCommand = new AsyncRelayCommand(async (param, ct) => await PrintEnvelopeAsync(param as UndeliveredPatientRowDto, ct), HasRow);
         BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientsHubViewModel>());
     }
 
@@ -54,12 +68,33 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
     public int Page { get => _page; set => SetProperty(ref _page, value); }
     public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }
 
+    /// <summary>
+    /// The grid's real selection (ResultDeliveryView.xaml binds <c>DataGrid.SelectedItem</c>
+    /// to this property; the three account/report buttons pass it as
+    /// <c>CommandParameter</c>). The setter re-evaluates the row-gated commands so the
+    /// buttons disable while no row is selected (R1 Round 3 A2).
+    /// </summary>
     public UndeliveredPatientRowDto? SelectedItem
     {
         get => _selectedItem;
-        set => SetProperty(ref _selectedItem, value);
+        set
+        {
+            if (SetProperty(ref _selectedItem, value))
+            {
+                PatientAccountCommand.RaiseCanExecuteChanged();
+                CombinedReportCommand.RaiseCanExecuteChanged();
+                EmptyReportCommand.RaiseCanExecuteChanged();
+                CopyCommand.RaiseCanExecuteChanged();
+                PatientDataCommand.RaiseCanExecuteChanged();
+                PrintEnvelopeCommand.RaiseCanExecuteChanged();
+            }
+        }
     }
 
+    /// <summary>Row gate for the three account/report buttons: a selectable row must exist.</summary>
+    private static bool HasRow(object? parameter)
+        => parameter is UndeliveredPatientRowDto row && row.PatientId > 0;
+
     public ObservableCollection<UndeliveredPatientRowDto> Items
     {
         get => _items;
@@ -91,8 +126,26 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
     public AsyncRelayCommand LoadCommand { get; }
     public RelayCommand OpenHandoverCommand { get; }
     public RelayCommand OpenGroupSlipsCommand { get; }
+    public RelayCommand PatientAccountCommand { get; }
+    public RelayCommand CombinedReportCommand { get; }
+    public RelayCommand EmptyReportCommand { get; }
+    /// <summary>1.4.1.4.5 «Copy»: copies the selected row's textual summary to the clipboard.</summary>
+    public RelayCommand CopyCommand { get; }
+
+    /// <summary>1.4.1.4.7 «بيانات المريض»: opens the patient's data from the delivery screen.</summary>
+    public RelayCommand PatientDataCommand { get; }
+
+    /// <summary>
+    /// 1.4.1.4.11 «طباعة ظرف»: prints the result envelope for the selected patient, reusing the
+    /// existing <c>PrintEnvelopeCommand</c> the patient editor and result sheet already use.
+    /// </summary>
+    public AsyncRelayCommand PrintEnvelopeCommand { get; }
+
     public RelayCommand BackCommand { get; }
 
+    /// <summary>1.4.1.4.2 «Refresh»: reloads the delivery list for the current period.</summary>
+    public AsyncRelayCommand RefreshCommand => LoadCommand;
+
     public async Task LoadAsync(CancellationToken cancellationToken = default)
     {
         IsBusy = true;
@@ -119,6 +172,83 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
         }
     }
 
+    /// <summary>
+    /// 1.4.1.4.5 «Copy»: copies a plain-text summary of the selected delivery entry.
+    /// The summary is built here (not in the view) so a test can assert its exact content.
+    /// </summary>
+    private void CopyRow(UndeliveredPatientRowDto? row)
+    {
+        if (row is null || row.PatientId <= 0)
+        {
+            return;
+        }
+
+        _ = _clipboard.SetTextAsync(BuildCopySummary(row));
+    }
+
+    /// <summary>
+    /// The exact text the copy action puts on the clipboard: patient, lab id, registration
+    /// date, visit status and the number of undelivered results, plus the note when present.
+    /// </summary>
+    public static string BuildCopySummary(UndeliveredPatientRowDto row)
+    {
+        var builder = new System.Text.StringBuilder();
+        builder.Append("المريض: ").Append(row.PatientFullName);
+        builder.Append(" — Lab ID: ").Append(row.LabId ?? "—");
+        builder.Append(" — التاريخ: ").Append(row.RegistrationDateUtc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
+        builder.Append(" — الحالة: ").Append(row.VisitStatus);
+        builder.Append(" — غير المسلَّم: ").Append(row.UndeliveredCount);
+
+        if (!string.IsNullOrWhiteSpace(row.PatientNotes))
+        {
+            builder.Append(" — ملاحظات: ").Append(row.PatientNotes);
+        }
+
+        return builder.ToString();
+    }
+
+    /// <summary>
+    /// 1.4.1.4.7 «بيانات المريض»: opens the patient editor for the selected patient, carrying
+    /// the patient id the same way the account action does.
+    /// </summary>
+    private void OpenPatientData(UndeliveredPatientRowDto? row)
+    {
+        if (row is null || row.PatientId <= 0)
+        {
+            return;
+        }
+
+        _navigation.NavigateTo<PatientEditorViewModel>();
+        if (_navigation.CurrentViewModel is PatientEditorViewModel vm)
+        {
+            _ = vm.LoadPatientAsync(row.PatientId);
+        }
+    }
+
+    /// <summary>1.4.1.4.11: the result envelope for one patient, through the existing handler.</summary>
+    private async Task PrintEnvelopeAsync(UndeliveredPatientRowDto? row, CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        if (row is null || row.PatientId <= 0)
+        {
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
+            var result = await _mediator.Send(new PrintEnvelopeCommand(row.PatientId), cancellationToken);
+            if (!result.IsSuccess)
+            {
+                ErrorMessage = _presenter.Present(result.Error!);
+            }
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
     private void OpenHandover(UndeliveredPatientRowDto? row)
     {
         if (row is null || row.PatientId <= 0)
@@ -148,4 +278,34 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
             _ = vm.LoadAsync();
         }
     }
+
+    private void OpenPatientAccount(UndeliveredPatientRowDto? row)
+    {
+        if (row is null || row.PatientId <= 0) return;
+        _navigation.NavigateTo<PatientAccountViewModel>();
+        if (_navigation.CurrentViewModel is PatientAccountViewModel vm)
+        {
+            _ = vm.LoadAsync(row.PatientId);
+        }
+    }
+
+    private void OpenCombinedReport(UndeliveredPatientRowDto? row)
+    {
+        if (row is null || row.PatientId <= 0) return;
+        _navigation.NavigateTo<CombinedReportViewModel>();
+        if (_navigation.CurrentViewModel is CombinedReportViewModel vm)
+        {
+            _ = vm.LoadAsync(row.PatientId);
+        }
+    }
+
+    private void OpenEmptyReport(UndeliveredPatientRowDto? row)
+    {
+        if (row is null || row.PatientId <= 0) return;
+        _navigation.NavigateTo<BlankReportViewModel>();
+        if (_navigation.CurrentViewModel is BlankReportViewModel vm)
+        {
+            _ = vm.LoadAsync(row.PatientId);
+        }
+    }
 }
diff --git a/src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs
index be715ac..1a7abca 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs
@@ -1,6 +1,8 @@
 using System.Collections.ObjectModel;
 using MediatR;
 using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
+using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
+using TopLab.Application.Features.ReportProduction.Common;
 using TopLab.Application.Features.ResultsEntry.Common;
 using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
 using TopLab.Presentation.Common;
@@ -19,23 +21,42 @@ public sealed class SimpleResultEntryViewModel : ViewModelBase
     private readonly ISender _mediator;
     private readonly ResultErrorPresenter _presenter;
     private readonly IDialogService _dialogs;
+    private readonly IResultPrintCoordinator _printCoordinator;
+    private readonly IPrintedStateRecorder _printedState;
+    private readonly IPdfPreviewService _pdfPreview;
+    private readonly ISimpleResultPreviewBuilder _previewBuilder;
 
     private ResultWorklistItemDto? _item;
     private string _resultValue = string.Empty;
     private int? _resultFlag;
     private string _notes = string.Empty;
     private string _errorMessage = string.Empty;
+    private string _statusMessage = string.Empty;
     private bool _isBusy;
 
-    public SimpleResultEntryViewModel(ISender mediator, ResultErrorPresenter presenter, IDialogService dialogs)
+    public SimpleResultEntryViewModel(
+        ISender mediator,
+        ResultErrorPresenter presenter,
+        IDialogService dialogs,
+        IResultPrintCoordinator printCoordinator,
+        IPrintedStateRecorder printedState,
+        IPdfPreviewService pdfPreview,
+        ISimpleResultPreviewBuilder previewBuilder)
     {
         _mediator = mediator;
         _presenter = presenter;
         _dialogs = dialogs;
+        _printCoordinator = printCoordinator;
+        _printedState = printedState;
+        _pdfPreview = pdfPreview;
+        _previewBuilder = previewBuilder;
 
         SaveCommand = new AsyncRelayCommand(async _ => await SaveAsync());
         ClearCommand = new AsyncRelayCommand(async _ => await ClearAsync());
         OpenCommentPickerCommand = new AsyncRelayCommand(async _ => await OpenCommentPickerAsync());
+        PrintCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct));
+        PreviewCommand = new AsyncRelayCommand(async (_, ct) => await PreviewAsync(ct));
+        ReviewCommand = new AsyncRelayCommand(async (_, ct) => await ReviewAsync(ct));
     }
 
     public ResultWorklistItemDto? Item
@@ -87,6 +108,24 @@ public sealed class SimpleResultEntryViewModel : ViewModelBase
     public AsyncRelayCommand ClearCommand { get; }
     public AsyncRelayCommand OpenCommentPickerCommand { get; }
 
+    /// <summary>1.22.12.2 «طباعة»: prints the report of the test through the shared coordinator.</summary>
+    public AsyncRelayCommand PrintCommand { get; }
+
+    /// <summary>1.22.12.3 «معاينة الطباعة»: shows the report before it is printed.</summary>
+    public AsyncRelayCommand PreviewCommand { get; }
+
+    /// <summary>1.22.12.4 «مراجعة»: opens the result for review.</summary>
+    public AsyncRelayCommand ReviewCommand { get; }
+
+    /// <summary>True once a result row is loaded, i.e. the three actions have something to act on.</summary>
+    public bool HasResult => PatientTestId > 0;
+
+    public string StatusMessage
+    {
+        get => _statusMessage;
+        private set => SetProperty(ref _statusMessage, value);
+    }
+
     public async Task LoadAsync(ResultWorklistItemDto item)
     {
         // S-04 Slice 3: load reference ranges for this test
@@ -159,6 +198,123 @@ public sealed class SimpleResultEntryViewModel : ViewModelBase
         }
     }
 
+    /// <summary>
+    /// 1.22.12.2 «طباعة»: the SAME shared print path the profile and culture screens use
+    /// (<see cref="IResultPrintCoordinator"/> with <see cref="ResultPrintKind.SimpleResult"/>,
+    /// then <see cref="IPrintedStateRecorder"/>). No second implementation.
+    /// </summary>
+    private async Task PrintAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!HasResult)
+        {
+            ErrorMessage = "اختر نتيجة محفوظة أولًا قبل الطباعة.";
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
+            var outcome = await _printCoordinator.PrintAsync(PatientTestId, ResultPrintKind.SimpleResult, cancellationToken);
+            if (!outcome.Printed)
+            {
+                ErrorMessage = "تعذّرت الطباعة: " + outcome.ErrorMessage;
+                return;
+            }
+
+            var recorded = await _printedState.RecordForPatientTestAsync(PatientTestId, cancellationToken);
+            if (!recorded.IsSuccess)
+            {
+                ErrorMessage = recorded.Error!.Message;
+                return;
+            }
+
+            StatusMessage = "تمت الطباعة.";
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
+    /// <summary>
+    /// 1.22.12.3 «معاينة الطباعة»: builds the same simple-result report and opens it for
+    /// review through <see cref="IPdfPreviewService"/> — never sends it to a printer.
+    /// </summary>
+    private async Task PreviewAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!HasResult)
+        {
+            ErrorMessage = "اختر نتيجة محفوظة أولًا قبل المعاينة.";
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
+            var built = await _previewBuilder.BuildAsync(PatientTestId, cancellationToken);
+            if (!built.IsSuccess)
+            {
+                ErrorMessage = built.Error?.Message ?? "تعذّر بناء تقرير التحليل.";
+                return;
+            }
+
+            var preview = await _pdfPreview.PreviewAsync(built.Value!, cancellationToken);
+            if (!preview.IsSuccess)
+            {
+                ErrorMessage = preview.Error?.Message ?? "تعذّر فتح معاينة التقرير.";
+                return;
+            }
+
+            StatusMessage = "تم فتح المعاينة.";
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
+    /// <summary>
+    /// 1.22.12.4 «مراجعة»: dispatches the existing <c>ReviewResultCommand</c> — the same
+    /// Application handler the worklist uses. The result must be saved first.
+    /// </summary>
+    private async Task ReviewAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!HasResult)
+        {
+            ErrorMessage = "اختر نتيجة محفوظة أولًا قبل المراجعة.";
+            return;
+        }
+
+        if (string.IsNullOrWhiteSpace(ResultValue))
+        {
+            ErrorMessage = "لا يمكن اعتماد نتيجة غير مدخلة.";
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
+            var result = await _mediator.Send(new ReviewResultCommand(PatientTestId), cancellationToken);
+            if (!result.IsSuccess)
+            {
+                ErrorMessage = _presenter.Present(result.Error!);
+                return;
+            }
+
+            StatusMessage = "تم اعتماد النتيجة.";
+        }
+        finally
+        {
+            IsBusy = false;
+        }
+    }
+
     private async Task ClearAsync()
     {
         var confirm = await _dialogs.ShowConfirmationAsync("مسح النتيجة", "هل أنت متأكد من مسح هذه النتيجة؟");
diff --git a/src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml b/src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml
index 77b2396..9e2f1eb 100644
--- a/src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml
@@ -7,6 +7,7 @@
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
+            <RowDefinition Height="Auto"/>
             <RowDefinition Height="*"/>
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
@@ -22,8 +23,12 @@
             </StackPanel>
         </Border>
 
+        <!-- Patient notes (GAP-05) -->
+        <TextBlock Grid.Row="1" Text="{Binding PatientNotes}" TextWrapping="Wrap" Foreground="#555" Margin="0,0,0,8"
+                   Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}"/>
+
         <!-- Culture header fields -->
-        <Grid Grid.Row="1" Margin="0,0,0,8">
+        <Grid Grid.Row="2" Margin="0,0,0,8">
             <Grid.ColumnDefinitions>
                 <ColumnDefinition Width="*"/>
                 <ColumnDefinition Width="*"/>
@@ -60,7 +65,7 @@
         </Grid>
 
         <!-- Loading -->
-        <ProgressBar Grid.Row="2" Height="6" IsIndeterminate="True" Margin="0,0,0,8">
+        <ProgressBar Grid.Row="3" Height="6" IsIndeterminate="True" Margin="0,0,0,8">
             <ProgressBar.Style>
                 <Style TargetType="ProgressBar">
                     <Setter Property="Visibility" Value="Collapsed"/>
@@ -74,7 +79,7 @@
         </ProgressBar>
 
         <!-- Empty state -->
-        <TextBlock Grid.Row="3" Text="لا مضادات حيوية مرفقة بهذه المزرعة" Foreground="#666666"
+        <TextBlock Grid.Row="4" Text="لا مضادات حيوية مرفقة بهذه المزرعة" Foreground="#666666"
                    HorizontalAlignment="Center" VerticalAlignment="Center">
             <TextBlock.Style>
                 <Style TargetType="TextBlock">
@@ -89,7 +94,7 @@
         </TextBlock>
 
         <!-- Sensitivity Grid -->
-        <DataGrid Grid.Row="3"
+        <DataGrid Grid.Row="4"
                   ItemsSource="{Binding SensitivityRows}"
                   AutoGenerateColumns="False"
                   CanUserAddRows="False"
@@ -123,7 +128,7 @@
         </DataGrid>
 
         <!-- Microscopy -->
-        <GroupBox Grid.Row="4" Header="الفحص المجهري" Margin="0,8,0,0">
+        <GroupBox Grid.Row="5" Header="الفحص المجهري" Margin="0,8,0,0">
             <Grid>
                 <Grid.ColumnDefinitions>
                     <ColumnDefinition Width="*"/>
@@ -176,7 +181,7 @@
         </GroupBox>
 
         <!-- Report preview -->
-        <Border Grid.Row="5" Background="#FAFAFA" Padding="8" Margin="0,8,0,0" BorderBrush="#DDD" BorderThickness="1">
+        <Border Grid.Row="6" Background="#FAFAFA" Padding="8" Margin="0,8,0,0" BorderBrush="#DDD" BorderThickness="1">
             <Border.Style>
                 <Style TargetType="Border">
                     <Setter Property="Visibility" Value="Collapsed"/>
@@ -200,11 +205,11 @@
         </Border>
 
         <!-- Messages -->
-        <TextBlock Grid.Row="6" Text="{Binding StatusMessage}" Foreground="Green" Margin="0,4,0,0"/>
-        <TextBlock Grid.Row="6" Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="0,20,0,0"/>
+        <TextBlock Grid.Row="7" Text="{Binding StatusMessage}" Foreground="Green" Margin="0,4,0,0"/>
+        <TextBlock Grid.Row="7" Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="0,20,0,0"/>
 
         <!-- Action Buttons -->
-        <StackPanel Grid.Row="7" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,8,0,0">
+        <StackPanel Grid.Row="8" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,8,0,0">
             <Button Content="حفظ" Margin="0,0,6,0" Padding="12,6" Command="{Binding SaveCommand}"/>
             <Button Content="اعتماد" Margin="0,0,6,0" Padding="12,6" Command="{Binding VerifyCommand}"/>
             <Button Content="إلغاء اعتماد" Margin="0,0,6,0" Padding="12,6" Command="{Binding UnverifyCommand}"/>
@@ -212,7 +217,7 @@
             <Button Content="تقرير" Margin="0,0,6,0" Padding="12,6" Command="{Binding ShowReportCommand}"/>
             <Button Content="تعليق" Margin="0,0,6,0" Padding="12,6" Command="{Binding OpenCommentPickerCommand}"/>
         </StackPanel>
-        <TextBlock Grid.Row="7" Text="{Binding PickedCommentPreview}" Foreground="Gray" TextWrapping="Wrap" Margin="0,34,0,0"
+        <TextBlock Grid.Row="8" Text="{Binding PickedCommentPreview}" Foreground="Gray" TextWrapping="Wrap" Margin="0,34,0,0"
                    Visibility="{Binding HasPickedComment, Converter={StaticResource BoolToVis}}"/>
     </Grid>
-</UserControl>
+</UserControl>
\ No newline at end of file
diff --git a/src/TopLab.Presentation/Views/Patients/DeliveryHandoverView.xaml b/src/TopLab.Presentation/Views/Patients/DeliveryHandoverView.xaml
index f2c99c9..803d358 100644
--- a/src/TopLab.Presentation/Views/Patients/DeliveryHandoverView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/DeliveryHandoverView.xaml
@@ -7,8 +7,8 @@
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
-            <RowDefinition Height="*"/>
             <RowDefinition Height="Auto"/>
+            <RowDefinition Height="*"/>
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
         </Grid.RowDefinitions>
@@ -25,8 +25,17 @@
             </StackPanel>
         </Border>
 
+        <!-- Patient notes (GAP-04) -->
+        <Border Grid.Row="1" Background="#FFF8E1" Padding="8" Margin="0,0,0,8"
+                Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}">
+            <StackPanel>
+                <TextBlock Text="ملاحظات المريض:" FontWeight="SemiBold"/>
+                <TextBlock Text="{Binding PatientNotes}" TextWrapping="Wrap" Margin="0,4,0,0"/>
+            </StackPanel>
+        </Border>
+
         <!-- Settlement controls -->
-        <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="0,0,0,8">
+        <StackPanel Grid.Row="2" Orientation="Horizontal" Margin="0,0,0,8">
             <TextBlock Text="مبلغ التسوية:" VerticalAlignment="Center" Margin="0,0,6,0"/>
             <TextBox Text="{Binding SettleAmountText, UpdateSourceTrigger=PropertyChanged}" Width="100" Margin="0,0,8,0"/>
             <CheckBox Content="تسوية كاملة (خلاص)" IsChecked="{Binding SettleInFull}" VerticalAlignment="Center" Margin="0,0,12,0"/>
@@ -36,7 +45,7 @@
         </StackPanel>
 
         <!-- Loading -->
-        <ProgressBar Grid.Row="2" Height="6" IsIndeterminate="True" Margin="0,0,0,8">
+        <ProgressBar Grid.Row="3" Height="6" IsIndeterminate="True" Margin="0,0,0,8">
             <ProgressBar.Style>
                 <Style TargetType="ProgressBar">
                     <Setter Property="Visibility" Value="Collapsed"/>
@@ -50,7 +59,7 @@
         </ProgressBar>
 
         <!-- Empty state -->
-        <TextBlock Grid.Row="3" Text="لا توجد تحاليل لهذا المريض." Foreground="#666666"
+        <TextBlock Grid.Row="4" Text="لا توجد تحاليل لهذا المريض." Foreground="#666666"
                    HorizontalAlignment="Center" VerticalAlignment="Center">
             <TextBlock.Style>
                 <Style TargetType="TextBlock">
@@ -65,7 +74,7 @@
         </TextBlock>
 
         <!-- Delivery Grid -->
-        <DataGrid Grid.Row="3"
+        <DataGrid Grid.Row="4"
                   ItemsSource="{Binding GridRows}"
                   AutoGenerateColumns="False"
                   CanUserAddRows="False"
@@ -101,7 +110,7 @@
         </DataGrid>
 
         <!-- Messages -->
-        <TextBlock Grid.Row="4" Text="{Binding StatusMessage}" Foreground="Green" Margin="0,4,0,0"/>
-        <TextBlock Grid.Row="5" Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="0,4,0,0"/>
+        <TextBlock Grid.Row="5" Text="{Binding StatusMessage}" Foreground="Green" Margin="0,4,0,0"/>
+        <TextBlock Grid.Row="6" Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="0,4,0,0"/>
     </Grid>
 </UserControl>
diff --git a/src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml b/src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml
index 1edadc0..3c3fdc7 100644
--- a/src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml
@@ -67,11 +67,29 @@
                 <!-- Payment + print actions -->
                 <WrapPanel>
                     <TextBlock Text="دفعة:" Margin="2" VerticalAlignment="Center" />
-                    <TextBox Text="{Binding PaymentAmountText, UpdateSourceTrigger=PropertyChanged}" Width="80" Margin="2" />
+                    <TextBox Text="{Binding PaymentAmountText, UpdateSourceTrigger=PropertyChanged}" Width="80" Margin="2">
+                        <TextBox.InputBindings>
+                            <KeyBinding Key="Enter" Command="{Binding AccountEntryEnterCommand}" />
+                        </TextBox.InputBindings>
+                    </TextBox>
                     <TextBlock Text="خصم:" Margin="8,2,2,2" VerticalAlignment="Center" />
-                    <TextBox Text="{Binding PaymentDiscountText, UpdateSourceTrigger=PropertyChanged}" Width="60" Margin="2" />
+                    <TextBox Text="{Binding PaymentDiscountText, UpdateSourceTrigger=PropertyChanged}" Width="60" Margin="2">
+                        <TextBox.InputBindings>
+                            <KeyBinding Key="Enter" Command="{Binding AccountEntryEnterCommand}" />
+                        </TextBox.InputBindings>
+                    </TextBox>
+                    <TextBlock Text="%" Margin="8,2,2,2" VerticalAlignment="Center" />
+                    <TextBox Text="{Binding PaymentPercentageText, UpdateSourceTrigger=PropertyChanged}" Width="50" Margin="2">
+                        <TextBox.InputBindings>
+                            <KeyBinding Key="Enter" Command="{Binding AccountEntryEnterCommand}" />
+                        </TextBox.InputBindings>
+                    </TextBox>
                     <Button Content="تسجيل مدفوع" Margin="8,2,2,2" Padding="10,2" Command="{Binding RecordPaymentCommand}" />
                     <Button Content="تسوية كاملة" Margin="2" Padding="10,2" Command="{Binding SettleCommand}" />
+                    <TextBlock Text="استرداد:" Margin="8,2,2,2" VerticalAlignment="Center" />
+                    <TextBox Text="{Binding RefundAmountText, UpdateSourceTrigger=PropertyChanged}" Width="70" Margin="2" />
+                    <Button Content="استرداد" Margin="2" Padding="10,2" Command="{Binding RefundCommand}" />
+                    <Button Content="تصفير الحساب" Margin="2" Padding="10,2" Command="{Binding ClearAccountCommand}" />
                     <Button Content="الإيصال" Margin="8,2,2,2" Padding="10,2" Command="{Binding PrintReceiptCommand}" />
                 </WrapPanel>
 
diff --git a/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml b/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml
index a0f52f8..a86120b 100644
--- a/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml
@@ -30,11 +30,29 @@
                 <TextBlock Text="{Binding Balance}" Margin="0,4,8,4" VerticalAlignment="Center" FontWeight="Bold" />
                 <Button Content="عرض الحساب" Margin="8,4,4,4" Padding="10,2" Command="{Binding OpenAccountCommand}" />
                 <TextBlock Text="دفعة:" Margin="8,4,4,4" VerticalAlignment="Center" />
-                <TextBox Text="{Binding PaymentAmountText, UpdateSourceTrigger=PropertyChanged}" Width="80" Margin="0,4" />
+                <TextBox Text="{Binding PaymentAmountText, UpdateSourceTrigger=PropertyChanged}" Width="80" Margin="0,4">
+                    <TextBox.InputBindings>
+                        <KeyBinding Key="Enter" Command="{Binding AccountEntryEnterCommand}" />
+                    </TextBox.InputBindings>
+                </TextBox>
                 <TextBlock Text="خصم:" Margin="8,4,4,4" VerticalAlignment="Center" />
-                <TextBox Text="{Binding PaymentDiscountText, UpdateSourceTrigger=PropertyChanged}" Width="60" Margin="0,4" />
+                <TextBox Text="{Binding PaymentDiscountText, UpdateSourceTrigger=PropertyChanged}" Width="60" Margin="0,4">
+                    <TextBox.InputBindings>
+                        <KeyBinding Key="Enter" Command="{Binding AccountEntryEnterCommand}" />
+                    </TextBox.InputBindings>
+                </TextBox>
+                <TextBlock Text="%" Margin="8,4,4,4" VerticalAlignment="Center" />
+                <TextBox Text="{Binding PaymentPercentageText, UpdateSourceTrigger=PropertyChanged}" Width="50" Margin="0,4">
+                    <TextBox.InputBindings>
+                        <KeyBinding Key="Enter" Command="{Binding AccountEntryEnterCommand}" />
+                    </TextBox.InputBindings>
+                </TextBox>
                 <Button Content="تسجيل مدفوع" Margin="8,4,4,4" Padding="10,2" Command="{Binding RecordPaymentCommand}" />
                 <Button Content="تسوية كاملة" Margin="4" Padding="10,2" Command="{Binding SettleCommand}" />
+                <TextBlock Text="استرداد:" Margin="8,4,4,4" VerticalAlignment="Center" />
+                <TextBox Text="{Binding RefundAmountText, UpdateSourceTrigger=PropertyChanged}" Width="70" Margin="0,4" />
+                <Button Content="استرداد" Margin="4" Padding="10,2" Command="{Binding RefundCommand}" />
+                <Button Content="تصفير الحساب" Margin="4" Padding="10,2" Command="{Binding ClearAccountCommand}" />
             </WrapPanel>
             <WrapPanel>
                 <Button Content="جديد" Margin="4" Padding="14,2" Command="{Binding NewPatientCommand}" />
@@ -47,6 +65,10 @@
                 <Button Content="الإيصال" Margin="4" Padding="14,2" Command="{Binding PrintReceiptCommand}" />
                 <Button Content="الفاتورة" Margin="4" Padding="14,2" Command="{Binding PrintInvoiceCommand}" />
                 <Button Content="طلب تحاليل" Margin="4" Padding="14,2" Command="{Binding PrintLabOrderCommand}" />
+<Button Content="مغلف" Margin="4" Padding="14,2" Command="{Binding PrintEnvelopeCommand}" IsEnabled="{Binding IsEditMode}" />
+  <Button Content="نافذة المرضى" Margin="4" Padding="14,2" Command="{Binding PatientWindowCommand}" />
+  <Button Content="نتائج التحاليل" Margin="4" Padding="14,2" Command="{Binding LabResultsCommand}" IsEnabled="{Binding IsEditMode}" />
+  <Button Content="متابعة" Margin="4" Padding="14,2" Command="{Binding FollowUpCommand}" IsEnabled="{Binding IsEditMode}" />
             </WrapPanel>
             <TextBlock Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="4,2" />
             <TextBlock Text="{Binding StatusMessage}" Foreground="#1E3A5F" TextWrapping="Wrap" Margin="4,0" />
@@ -508,6 +530,40 @@
                     </ListView.ItemTemplate>
                 </ListView>
                 <TextBlock DockPanel.Dock="Top" Text="كتالوج التحاليل:" FontWeight="Bold" Margin="0,4" />
+                <!-- 1.2.2.5.1.1 «start»: the master test list's filter field. Narrows by name OR code. -->
+                <!-- 1.2.2.5.1.2 «TM / TG / CG»: three mutually exclusive view-filter modes (D-02).
+                     Radio buttons, so exactly one is active at any time; no code-behind. -->
+                <WrapPanel DockPanel.Dock="Top" Margin="0,0,0,4">
+                    <RadioButton Content="TM" GroupName="CatalogMode" Margin="2,0,10,0" VerticalAlignment="Center"
+                                 IsChecked="{Binding IsTmMode, Mode=TwoWay}"
+                                 ToolTip="كل التحاليل" />
+                    <RadioButton Content="TG" GroupName="CatalogMode" Margin="0,0,10,0" VerticalAlignment="Center"
+                                 IsChecked="{Binding IsTgMode, Mode=TwoWay}"
+                                 ToolTip="بحث بالاسم أو الكود" />
+                    <RadioButton Content="CG" GroupName="CatalogMode" Margin="0,0,10,0" VerticalAlignment="Center"
+                                 IsChecked="{Binding IsCgMode, Mode=TwoWay}"
+                                 ToolTip="مجموعاتي المخصصة" />
+                </WrapPanel>
+                <WrapPanel DockPanel.Dock="Top" Margin="0,0,0,4">
+                    <TextBlock Text="بحث:" Margin="2" VerticalAlignment="Center" />
+                    <TextBox Text="{Binding CatalogFilterText, UpdateSourceTrigger=PropertyChanged}"
+                             Width="180" Margin="2"
+                             IsEnabled="{Binding IsCatalogSearchEnabled}" />
+                    <Button Content="مسح" Margin="2" Padding="8,0" Command="{Binding ClearCatalogFilterCommand}" />
+                    <TextBlock Text="لا توجد تحاليل مطابقة للبحث." Foreground="#666666" Margin="6,2,2,2"
+                               VerticalAlignment="Center">
+                        <TextBlock.Style>
+                            <Style TargetType="TextBlock">
+                                <Setter Property="Visibility" Value="Collapsed" />
+                                <Style.Triggers>
+                                    <DataTrigger Binding="{Binding ShowCatalogFilterNoMatch}" Value="True">
+                                        <Setter Property="Visibility" Value="Visible" />
+                                    </DataTrigger>
+                                </Style.Triggers>
+                            </Style>
+                        </TextBlock.Style>
+                    </TextBlock>
+                </WrapPanel>
                 <TextBlock DockPanel.Dock="Top" Text="لا توجد تحاليل في الكتالوج." Foreground="#666666" Margin="0,0,0,4">
                     <TextBlock.Style>
                         <Style TargetType="TextBlock">
@@ -520,7 +576,7 @@
                         </Style>
                     </TextBlock.Style>
                 </TextBlock>
-                <ListView ItemsSource="{Binding CatalogTests}" Margin="0,0,0,4">
+                <ListView ItemsSource="{Binding FilteredCatalogTests}" Margin="0,0,0,4">
                     <ListView.ItemTemplate>
                         <DataTemplate>
                             <StackPanel Orientation="Horizontal">
diff --git a/src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml b/src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml
index f79fd1f..894c9ff 100644
--- a/src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml
@@ -7,6 +7,7 @@
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
+            <RowDefinition Height="Auto"/>
             <RowDefinition Height="*"/>
             <RowDefinition Height="Auto"/>
             <RowDefinition Height="Auto"/>
@@ -21,14 +22,19 @@
             </StackPanel>
         </Border>
 
+        <!-- Patient notes (GAP-05) -->
+        <TextBlock Grid.Row="1" Text="{Binding PatientNotes}" TextWrapping="Wrap"
+                   Foreground="#555" Margin="0,0,0,8"
+                   Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}"/>
+
         <!-- Comment -->
-        <DockPanel Grid.Row="1" Margin="0,0,0,8">
+        <DockPanel Grid.Row="2" Margin="0,0,0,8">
             <TextBlock Text="ملاحظة:" VerticalAlignment="Center" Margin="0,0,8,0"/>
             <TextBox Text="{Binding Comment, UpdateSourceTrigger=PropertyChanged}" MaxLength="500"/>
         </DockPanel>
 
         <!-- Loading -->
-        <ProgressBar Grid.Row="2" Height="6" IsIndeterminate="True" Margin="0,0,0,8">
+        <ProgressBar Grid.Row="3" Height="6" IsIndeterminate="True" Margin="0,0,0,8">
             <ProgressBar.Style>
                 <Style TargetType="ProgressBar">
                     <Setter Property="Visibility" Value="Collapsed"/>
@@ -42,7 +48,7 @@
         </ProgressBar>
 
         <!-- Empty state -->
-        <TextBlock Grid.Row="3" Text="لا توجد مواد في هذا البروفايل" Foreground="#666666"
+        <TextBlock Grid.Row="4" Text="لا توجد مواد في هذا البروفايل" Foreground="#666666"
                    HorizontalAlignment="Center" VerticalAlignment="Center">
             <TextBlock.Style>
                 <Style TargetType="TextBlock">
@@ -57,7 +63,7 @@
         </TextBlock>
 
         <!-- Entry Grid -->
-        <DataGrid Grid.Row="3"
+        <DataGrid Grid.Row="4"
                   ItemsSource="{Binding Rows}"
                   SelectedItem="{Binding SelectedRow}"
                   AutoGenerateColumns="False"
@@ -94,7 +100,7 @@
         </DataGrid>
 
         <!-- Report preview -->
-        <Border Grid.Row="4" Background="#FAFAFA" Padding="8" Margin="0,8,0,0" BorderBrush="#DDD" BorderThickness="1">
+        <Border Grid.Row="5" Background="#FAFAFA" Padding="8" Margin="0,8,0,0" BorderBrush="#DDD" BorderThickness="1">
             <Border.Style>
                 <Style TargetType="Border">
                     <Setter Property="Visibility" Value="Collapsed"/>
@@ -124,11 +130,11 @@
         </Border>
 
         <!-- Messages -->
-        <TextBlock Grid.Row="5" Text="{Binding StatusMessage}" Foreground="Green" Margin="0,4,0,0"/>
-        <TextBlock Grid.Row="5" Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="0,20,0,0"/>
+        <TextBlock Grid.Row="6" Text="{Binding StatusMessage}" Foreground="Green" Margin="0,4,0,0"/>
+        <TextBlock Grid.Row="6" Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="0,20,0,0"/>
 
         <!-- Action Buttons -->
-        <StackPanel Grid.Row="6" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,8,0,0">
+        <StackPanel Grid.Row="7" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,8,0,0">
             <Button Content="حفظ" Margin="0,0,6,0" Padding="12,6" Command="{Binding SaveCommand}"/>
             <Button Content="اعتماد" Margin="0,0,6,0" Padding="12,6" Command="{Binding VerifyCommand}"/>
             <Button Content="إلغاء اعتماد" Margin="0,0,6,0" Padding="12,6" Command="{Binding UnverifyCommand}"/>
@@ -139,4 +145,4 @@
             <Button Content="تعليق" Margin="0,0,6,0" Padding="12,6" Command="{Binding OpenCommentPickerCommand}"/>
         </StackPanel>
     </Grid>
-</UserControl>
+</UserControl>
\ No newline at end of file
diff --git a/src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml b/src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml
index ae0a7c3..30f27b9 100644
--- a/src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml
@@ -9,7 +9,18 @@
             <DatePicker SelectedDate="{Binding From, UpdateSourceTrigger=PropertyChanged}" Width="140" Margin="0,0,8,0"/>
             <TextBlock Text="إلى:" VerticalAlignment="Center" Margin="0,0,6,0"/>
             <DatePicker SelectedDate="{Binding To, UpdateSourceTrigger=PropertyChanged}" Width="140" Margin="0,0,8,0"/>
-            <Button Content="عرض" Width="70" Margin="0,0,12,0" Command="{Binding LoadCommand}"/>
+            <Button Content="عرض" Width="70" Margin="0,0,6,0" Command="{Binding LoadCommand}"/>
+            <!-- 1.4.1.4.2 «Refresh»: reloads the delivery list for the current period. -->
+            <Button Content="تحديث" Width="70" Margin="0,0,12,0" Command="{Binding RefreshCommand}"/>
+            <!-- 1.4.1.4.5 «Copy»: copies the selected entry's summary. -->
+            <Button Content="نسخ" Width="60" Margin="0,0,8,0" Command="{Binding CopyCommand}" CommandParameter="{Binding SelectedItem}"/>
+            <!-- 1.4.1.4.7 «بيانات المريض»: opens the patient's data from the delivery screen. -->
+            <Button Content="بيانات المريض" Width="100" Margin="0,0,12,0" Command="{Binding PatientDataCommand}" CommandParameter="{Binding SelectedItem}"/>
+            <!-- 1.4.1.4.11 «طباعة ظرف»: the result envelope for the selected patient. -->
+            <Button Content="طباعة ظرف" Width="90" Margin="0,0,12,0" Command="{Binding PrintEnvelopeCommand}" CommandParameter="{Binding SelectedItem}"/>
+            <Button Content="حساب المريض" Width="90" Margin="0,0,8,0" Command="{Binding PatientAccountCommand}" CommandParameter="{Binding SelectedItem}"/>
+            <Button Content="تقرير مجمع" Width="90" Margin="0,0,8,0" Command="{Binding CombinedReportCommand}" CommandParameter="{Binding SelectedItem}"/>
+            <Button Content="تقرير فارغ" Width="90" Margin="0,0,8,0" Command="{Binding EmptyReportCommand}" CommandParameter="{Binding SelectedItem}"/>
             <Button Content="قصاصة تقرير جماعية" Width="150" Margin="0,0,12,0" Command="{Binding OpenGroupSlipsCommand}"/>
             <Button Content="رجوع" Width="70" Command="{Binding BackCommand}"/>
         </StackPanel>
@@ -58,6 +69,7 @@
                 <DataGridTextColumn Header="التاريخ" Binding="{Binding RegistrationDateUtc, StringFormat={}{0:yyyy-MM-dd}}" Width="100"/>
                 <DataGridTextColumn Header="الحالة" Binding="{Binding VisitStatus}" Width="60"/>
                 <DataGridTextColumn Header="غير المسلَّم" Binding="{Binding UndeliveredCount}" Width="80"/>
+                <DataGridTextColumn Header="ملاحظات" Binding="{Binding PatientNotes}" Width="120"/>
                 <DataGridTemplateColumn Header="تسليم" Width="70">
                     <DataGridTemplateColumn.CellTemplate>
                         <DataTemplate>
diff --git a/src/TopLab.Presentation/Views/Patients/SimpleResultEntryView.xaml b/src/TopLab.Presentation/Views/Patients/SimpleResultEntryView.xaml
index b1f5210..3aaa330 100644
--- a/src/TopLab.Presentation/Views/Patients/SimpleResultEntryView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/SimpleResultEntryView.xaml
@@ -46,12 +46,21 @@
         
         <!-- Error Message -->
         <TextBlock Grid.Row="4" Text="{Binding ErrorMessage}" Foreground="Red" FontWeight="SemiBold" />
-        
+
+        <!-- Status Message (1.22.12.x feedback: printed / previewed / reviewed) -->
+        <TextBlock Grid.Row="5" Text="{Binding StatusMessage}" Foreground="#1E3A5F" FontWeight="SemiBold" />
+
         <!-- Action Buttons -->
         <StackPanel Grid.Row="7" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
             <Button Content="مسح" Command="{Binding ClearCommand}" Margin="0,0,8,0"/>
             <Button Content="تعليق" Command="{Binding OpenCommentPickerCommand}" Margin="0,0,8,0"/>
-            <Button Content="حفظ" Command="{Binding SaveCommand}" IsDefault="True"/>
+            <Button Content="حفظ" Command="{Binding SaveCommand}" IsDefault="True" Margin="0,0,8,0"/>
+            <!-- 1.22.12.2 «طباعة» / 1.22.12.3 «معاينة الطباعة» / 1.22.12.4 «مراجعة»:
+                 the same shared Application handlers the profile and culture screens use. -->
+            <Separator Margin="0,0,8,0"/>
+            <Button Content="طباعة" Command="{Binding PrintCommand}" Margin="0,0,8,0"/>
+            <Button Content="معاينة" Command="{Binding PreviewCommand}" Margin="0,0,8,0"/>
+            <Button Content="مراجعة" Command="{Binding ReviewCommand}"/>
         </StackPanel>
     </Grid>
 </UserControl>
\ No newline at end of file
diff --git a/tests/TopLab.Domain.Tests/Billing/PatientAccountCalculatorTests.cs b/tests/TopLab.Domain.Tests/Billing/PatientAccountCalculatorTests.cs
index 6105a1a..8f29414 100644
--- a/tests/TopLab.Domain.Tests/Billing/PatientAccountCalculatorTests.cs
+++ b/tests/TopLab.Domain.Tests/Billing/PatientAccountCalculatorTests.cs
@@ -14,7 +14,8 @@ public class PatientAccountCalculatorTests
         decimal? discountAmount = null,
         bool isExtraCharge = false,
         OperationType operationType = OperationType.Payment,
-        bool voided = false)
+        bool voided = false,
+        decimal? refundAmount = null)
     {
         var op = PaymentOperation.Create(
             PaymentOperationId.Create(_nextId++),
@@ -24,7 +25,8 @@ public class PatientAccountCalculatorTests
             DateTime.UtcNow,
             discountAmount,
             isExtraCharge,
-            operationType);
+            operationType,
+            refundAmount);
         if (voided)
             op.Void();
         return op;
@@ -186,4 +188,66 @@ public class PatientAccountCalculatorTests
 
         Assert.Equal(350m, PatientAccountCalculator.Balance(prices, Array.Empty<PaymentOperation>()));
     }
+
+    [Fact]
+    public void Refund_ReducesTotalPaid()
+    {
+        var prices = new List<decimal> { 100m };
+        var ops = new List<PaymentOperation>
+        {
+            Op(80m),
+            Op(0m, operationType: OperationType.Refund, refundAmount: 20m),
+        };
+
+        Assert.Equal(100m, PatientAccountCalculator.TotalCharged(prices, ops));
+        Assert.Equal(60m, PatientAccountCalculator.TotalPaid(ops));
+        Assert.Equal(40m, PatientAccountCalculator.Balance(prices, ops));
+    }
+
+    [Fact]
+    public void VoidedRefund_IsIgnored()
+    {
+        var prices = new List<decimal> { 100m };
+        var ops = new List<PaymentOperation>
+        {
+            Op(80m),
+            Op(0m, operationType: OperationType.Refund, refundAmount: 20m, voided: true),
+        };
+
+        Assert.Equal(100m, PatientAccountCalculator.TotalCharged(prices, ops));
+        Assert.Equal(80m, PatientAccountCalculator.TotalPaid(ops));
+        Assert.Equal(20m, PatientAccountCalculator.Balance(prices, ops));
+    }
+
+    [Fact]
+    public void Mixed_PaymentsDiscountsExtraChargesRefunds()
+    {
+        var prices = new List<decimal> { 100m, 50m };
+        var ops = new List<PaymentOperation>
+        {
+            Op(20m, isExtraCharge: true),
+            Op(80m, discountAmount: 10m),
+            Op(0m, operationType: OperationType.Refund, refundAmount: 5m),
+            Op(999m, voided: true),
+        };
+
+        Assert.Equal(170m, PatientAccountCalculator.TotalCharged(prices, ops));
+        Assert.Equal(85m, PatientAccountCalculator.TotalPaid(ops));
+        Assert.Equal(85m, PatientAccountCalculator.Balance(prices, ops));
+    }
+
+    [Fact]
+    public void NoRefunds_UnchangedBehaviour()
+    {
+        var prices = new List<decimal> { 100m, 50m };
+        var ops = new List<PaymentOperation>
+        {
+            Op(20m, isExtraCharge: true),
+            Op(80m, discountAmount: 10m),
+        };
+
+        Assert.Equal(170m, PatientAccountCalculator.TotalCharged(prices, ops));
+        Assert.Equal(90m, PatientAccountCalculator.TotalPaid(ops));
+        Assert.Equal(80m, PatientAccountCalculator.Balance(prices, ops));
+    }
 }
diff --git a/tests/TopLab.Presentation.Tests/Common/Fakes.cs b/tests/TopLab.Presentation.Tests/Common/Fakes.cs
index 8bf1149..4c3ac0d 100644
--- a/tests/TopLab.Presentation.Tests/Common/Fakes.cs
+++ b/tests/TopLab.Presentation.Tests/Common/Fakes.cs
@@ -49,6 +49,23 @@ public sealed class FakeCurrentUserService : ICurrentUserService
     }
 }
 
+/// <summary>
+/// Records what a ViewModel asked the clipboard to hold, so a copy action can be asserted
+/// without touching the real OS clipboard (which is unavailable in a unit test).
+/// </summary>
+public sealed class FakeClipboardService : TopLab.Presentation.Common.Clipboard.IClipboardService
+{
+    public List<string> CopiedTexts { get; } = new();
+
+    public string? LastText => CopiedTexts.Count == 0 ? null : CopiedTexts[^1];
+
+    public Task SetTextAsync(string text, CancellationToken cancellationToken = default)
+    {
+        CopiedTexts.Add(text);
+        return Task.CompletedTask;
+    }
+}
+
 public sealed class FakeDialogService : IDialogService
 {
     public List<string> Errors { get; } = new();
diff --git a/tests/TopLab.Presentation.Tests/Patients/PatientEditorMedicalHistoryTests.cs b/tests/TopLab.Presentation.Tests/Patients/PatientEditorMedicalHistoryTests.cs
index 7ada236..58178fd 100644
--- a/tests/TopLab.Presentation.Tests/Patients/PatientEditorMedicalHistoryTests.cs
+++ b/tests/TopLab.Presentation.Tests/Patients/PatientEditorMedicalHistoryTests.cs
@@ -96,6 +96,12 @@ public class PatientEditorMedicalHistoryTests
                     Array.Empty<CustomGroupSummaryDto>()));
             }
 
+            if (request is TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds.GetCustomGroupTestIdsQuery)
+            {
+                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<int>>.Success(
+                    Array.Empty<int>()));
+            }
+
             if (request is GetNextLabIdQuery)
             {
                 return Task.FromResult((TResponse)(object)Result<string>.Success("1"));

```

## (b) Complete text of every new source/test file created this round


### src/TopLab.Domain/Billing/AccountEntrySession.cs

```csharp
using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Billing;

/// <summary>
/// What the current <c>Enter</c> press did (canonical reference 1.3.1.4.2: "the first press
/// performs the accounting operation for the patient; the second press saves the operation").
/// </summary>
public enum AccountEntryStage
{
    /// <summary>Nothing entered yet, or an edit reset the sequence.</summary>
    Idle = 0,

    /// <summary>The first press computed and displayed the figures; nothing was recorded.</summary>
    Previewed = 1,

    /// <summary>The second press may persist; the caller performs the write.</summary>
    ReadyToPersist = 2
}

/// <summary>
/// Outcome of one <c>Enter</c> press. <see cref="ShouldPersist"/> is the single gate the
/// ViewModels use to decide whether the record-payment command may be sent — the first press
/// can never persist, by construction.
/// </summary>
public sealed record AccountEntryStep(
    AccountEntryStage Stage,
    bool ShouldPersist,
    bool IsValid,
    string? ErrorMessage,
    string? StatusMessage,
    AccountEntryPreview? Preview,
    AccountEntryKind Kind,
    decimal Amount,
    decimal Discount);

/// <summary>
/// The account panel's entry state machine (canonical reference 1.3.1.2, 1.3.1.3, 1.3.1.4,
/// 1.3.1.4.1, 1.3.1.4.2; owner decisions OD-11, OD-12 and the Round 3 percentage decision).
/// <para>
/// Pure and unit-testable: it performs no I/O and knows nothing about WPF. Both payment-entry
/// screens drive one instance, so they cannot diverge. Behaviour:
/// </para>
/// <list type="bullet">
/// <item>the first <c>Enter</c> validates the inputs and returns the resulting figures, and
/// <see cref="ShouldPersist"/> is false;</item>
/// <item>a second <c>Enter</c> with unchanged inputs returns <see cref="ShouldPersist"/> true;</item>
/// <item>any edit to an input resets the sequence to the first-press state;</item>
/// <item>invalid input reports an error and stays in the first-press state.</item>
/// </list>
/// </summary>
public sealed class AccountEntrySession
{
    private string? _amountText;
    private string? _discountText;
    private string? _percentageText;

    private string? _validatedAmountText;
    private string? _validatedDiscountText;
    private string? _validatedPercentageText;

    /// <summary>
    /// The percentage actually converted into the discount field by <see cref="ApplyPercentage"/>.
    /// The percentage text is cleared once it has been converted, so this value is what the D-01
    /// availability check reads on that same Enter press and on the second (persisting) press.
    /// </summary>
    private decimal _appliedPercentage;

    /// <summary>Paid-amount text. Editing it resets the two-press sequence.</summary>
    public string? AmountText
    {
        get => _amountText;
        set => SetField(ref _amountText, value);
    }

    /// <summary>Value-discount text. Editing it resets the sequence and clears the percentage.</summary>
    public string? DiscountText
    {
        get => _discountText;
        set
        {
            if (SetField(ref _discountText, value))
            {
                // 1.3.1.2 / 1.3.1.3: the value and the percentage are never applied together.
                _percentageText = null;
                _appliedPercentage = 0m;
            }
        }
    }

    /// <summary>Percentage-discount text. Editing it resets the sequence and clears the value.</summary>
    public string? PercentageText
    {
        get => _percentageText;
        set
        {
            if (SetField(ref _percentageText, value))
            {
                _discountText = null;
            }
        }
    }

    /// <summary>Current stage of the two-press sequence.</summary>
    public AccountEntryStage Stage { get; private set; } = AccountEntryStage.Idle;

    /// <summary>Resets everything, including the stage (called after a successful save).</summary>
    public void Reset()
    {
        _amountText = null;
        _discountText = null;
        _percentageText = null;
        _validatedAmountText = null;
        _validatedDiscountText = null;
        _validatedPercentageText = null;
        _appliedPercentage = 0m;
        Stage = AccountEntryStage.Idle;
    }

    /// <summary>
    /// Applies a percentage to the patient's TOTAL account amount and writes the resulting
    /// amount into the value field, as the Round 3 decision requires. Returns the Arabic error
    /// message when the percentage is out of range.
    /// </summary>
    public string? ApplyPercentage(decimal totalAccountAmount)
    {
        var parsed = AccountEntryParser.ParsePercentage(_percentageText);
        if (!parsed.IsValid)
        {
            return parsed.ErrorMessage;
        }

        if (parsed.Value <= 0m)
        {
            return null;
        }

        var amount = AccountEntryCalculator.DiscountAmountFromPercentage(totalAccountAmount, parsed.Value);

        // Round 4 fix: the converted amount is written first, then the percentage field is
        // cleared through the BACKING FIELD. Going through the PercentageText setter would
        // clear _discountText as well (the mutual-exclusion rule for USER edits) and silently
        // throw the converted amount away — which is exactly what the Round-3 build did.
        _discountText = amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        _percentageText = null;

        // The field is now empty, so PressEnter cannot read the percentage from the text any
        // more. Keep the value that was actually applied so the D-01 availability check still
        // runs on the same Enter press (and on the second press, which must persist).
        _appliedPercentage = parsed.Value;
        Stage = AccountEntryStage.Idle;
        return null;
    }

    /// <summary>
    /// One <c>Enter</c> press. <paramref name="totalCharged"/> and
    /// <paramref name="alreadyPaid"/> come from the loaded account (never from client-side
    /// arithmetic on other figures) and are used only to display the resulting figures.
    /// </summary>
    public AccountEntryStep PressEnter(decimal totalCharged, decimal alreadyPaid)
    {
        var amount = AccountEntryParser.ParsePaidAmount(_amountText);
        if (!amount.IsValid)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(amount.ErrorMessage!, AccountEntryKind.Payment, 0m, 0m);
        }

        var discount = AccountEntryParser.ParseDiscount(_discountText);
        if (!discount.IsValid)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(discount.ErrorMessage!, amount.Kind, amount.Value, 0m);
        }

        var percentage = AccountEntryParser.ParsePercentage(_percentageText);
        if (!percentage.IsValid)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(percentage.ErrorMessage!, amount.Kind, amount.Value, 0m);
        }

        // A percentage that was already converted into the discount field no longer has any text
        // to parse, so the effective percentage is whichever of the two is set.
        var effectivePercentage = percentage.Value > 0m ? percentage.Value : _appliedPercentage;

        // 1.3.1.4.1: a surcharge carries no discount.
        if (amount.Kind == AccountEntryKind.Surcharge && discount.Value > 0m)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(AccountEntryParser.SurchargeWithDiscountMessage, amount.Kind, amount.Value, 0m);
        }

        // D-01 (owner decision, option 4 of card D-01): the percentage discount is available
        // ONLY when the payment settles the whole remaining balance — that is, when
        // amount + percentage-discount EQUALS (TotalCharged - net TotalPaid). This availability
        // check runs BEFORE the Domain rule below, so a percentage that fails it is reported as
        // unavailable rather than as an over-discount.
        if (effectivePercentage > 0m)
        {
            var remaining = AccountEntryCalculator.Round2(totalCharged - alreadyPaid);
            if (remaining <= 0m)
            {
                Stage = AccountEntryStage.Idle;
                return Invalid(
                    AccountEntryParser.PercentageWithoutRemainingBalanceMessage,
                    amount.Kind, amount.Value, discount.Value);
            }

            var settled = AccountEntryCalculator.Round2(amount.Value + discount.Value);
            if (settled != remaining)
            {
                Stage = AccountEntryStage.Idle;
                return Invalid(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        AccountEntryParser.PercentageRequiresFullSettlementMessage,
                        remaining.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)),
                    amount.Kind, amount.Value, discount.Value);
            }
        }

        // The domain rule stays untouched: a discount can never exceed the operation amount.
        // It is the SECOND check, exactly as the owner required.
        if (discount.Value > amount.Value)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", amount.Kind, amount.Value, discount.Value);
        }

        var preview = AccountEntryCalculator.Preview(
            totalCharged, alreadyPaid, amount.Value, discount.Value, amount.Kind);

        var isSecondPress = Stage == AccountEntryStage.Previewed
            && _validatedAmountText == _amountText
            && _validatedDiscountText == _discountText
            && _validatedPercentageText == _percentageText;

        if (isSecondPress)
        {
            Stage = AccountEntryStage.ReadyToPersist;
            return new AccountEntryStep(
                AccountEntryStage.ReadyToPersist,
                ShouldPersist: true,
                IsValid: true,
                ErrorMessage: null,
                StatusMessage: null,
                Preview: preview,
                Kind: amount.Kind,
                Amount: amount.Value,
                Discount: discount.Value);
        }

        _validatedAmountText = _amountText;
        _validatedDiscountText = _discountText;
        _validatedPercentageText = _percentageText;
        Stage = AccountEntryStage.Previewed;

        var status = amount.Kind == AccountEntryKind.Surcharge
            ? $"مبلغ إضافي: {preview.Amount:0.##} — إجمالي التحاليل بعد الإضافة: {preview.TotalChargedAfter:0.##} — الباقي: {preview.BalanceAfter:0.##}. اضغط Enter مرة أخرى للحفظ."
            : $"المدفوع: {preview.Amount:0.##} — الخصم: {preview.Discount:0.##} — الباقي بعد العملية: {preview.BalanceAfter:0.##}. اضغط Enter مرة أخرى للحفظ.";

        return new AccountEntryStep(
            AccountEntryStage.Previewed,
            ShouldPersist: false,
            IsValid: true,
            ErrorMessage: null,
            StatusMessage: status,
            Preview: preview,
            Kind: amount.Kind,
            Amount: amount.Value,
            Discount: discount.Value);
    }

    /// <summary>After a successful save the panel returns to its idle state.</summary>
    public void MarkPersisted() => Reset();

    private static AccountEntryStep Invalid(string message, AccountEntryKind kind, decimal amount, decimal discount)
        => new(
            AccountEntryStage.Idle,
            ShouldPersist: false,
            IsValid: false,
            ErrorMessage: message,
            StatusMessage: null,
            Preview: null,
            Kind: kind,
            Amount: amount,
            Discount: discount);

    private bool SetField(ref string? field, string? value)
    {
        if (string.Equals(field, value, StringComparison.Ordinal))
        {
            return false;
        }

        field = value;
        // Any edit returns the sequence to its first-press state (1.3.1.4.2).
        Stage = AccountEntryStage.Idle;
        _validatedAmountText = null;
        _validatedDiscountText = null;
        _validatedPercentageText = null;
        return true;
    }
}

```


### src/TopLab.Presentation/ViewModels/Patients/CatalogFilterMode.cs

```csharp
namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// The three mutually exclusive modes of the registration screen's test-catalogue filter bar
/// (canonical reference 1.2.2.5.1.2, owner decision D-02 / interpretation A of D-03).
/// <para>
/// This is a VIEW filter only: it selects which of the already-loaded catalogue rows are shown.
/// It never writes to the database, never changes the catalogue query, and never hides a test
/// that the unfiltered catalogue shows (mode <see cref="Tm"/> is exactly that set).
/// </para>
/// </summary>
public enum CatalogFilterMode
{
    /// <summary>TM «كل التحاليل»: every test the catalogue already shows (the base set).</summary>
    Tm = 0,

    /// <summary>TG «بحث بالاسم أو الكود»: only the rows matching the search box (all rows when empty).</summary>
    Tg = 1,

    /// <summary>CG «مجموعاتي المخصصة»: only tests belonging to at least one custom group.</summary>
    Cg = 2
}

```


### src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Queries/GetCustomGroupTestIds/GetCustomGroupTestIdsQuery.cs

```csharp
using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;

/// <summary>
/// The union of the test ids that belong to at least one custom group (R1 Round 5, D-02 mode CG).
/// <para>
/// Read-only, no schema change: membership already exists as <c>CustomGroupItem</c>
/// (<c>CustomGroupId</c> + <c>TestId</c>), and the registration catalogue does not carry it.
/// The result is distinct, so a test that belongs to several groups appears once — which is what
/// the catalogue list needs, since it shows one row per test.
/// </para>
/// </summary>
public sealed record GetCustomGroupTestIdsQuery() : IRequest<Result<IReadOnlyList<int>>>;

```


### src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Queries/GetCustomGroupTestIds/GetCustomGroupTestIdsQueryHandler.cs

```csharp
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;

/// <summary>
/// Returns the distinct test ids that appear in at least one custom group.
/// Read-only projection over the existing <c>CustomGroupItem</c> rows; no write, no schema change.
/// </summary>
public sealed class GetCustomGroupTestIdsQueryHandler
    : IRequestHandler<GetCustomGroupTestIdsQuery, Result<IReadOnlyList<int>>>
{
    private readonly IApplicationDbContext _db;

    public GetCustomGroupTestIdsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<int>>> Handle(
        GetCustomGroupTestIdsQuery request, CancellationToken cancellationToken)
    {
        // Distinct: a test in several groups must appear once, because the catalogue shows one
        // row per test.
        var testIds = _db.Set<CustomGroupItem>()
            .Select(i => i.TestId.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<int>>.Success(testIds));
    }
}

```


### tests/TopLab.Domain.Tests/Billing/RefundCeilingTests.cs

```csharp
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 5 Part B — the refund ceiling is the net CASH the patient actually paid.
/// <para>
/// Before the fix, <c>MaxRefundable</c> delegated to <c>PatientAccountCalculator.TotalPaid</c>,
/// which sums <c>Amount + DiscountAmount</c>. A discount is money the patient no longer OWES, not
/// money they PAID, so a discounted payment made the laboratory refund cash it never received.
/// </para>
/// <para>
/// <c>TotalPaid</c> and <c>Balance</c> are deliberately unchanged: a discount still reduces the
/// balance. Only the refund ceiling changed.
/// </para>
/// </summary>
public class RefundCeilingTests
{
    private static PaymentOperation Payment(decimal amount, decimal? discount = null, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), amount, 7, DateTime.UtcNow, discount);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    private static PaymentOperation Surcharge(decimal amount, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), amount, 7, DateTime.UtcNow, null, true);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    private static PaymentOperation Refund(decimal amount, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), 0m, 7, DateTime.UtcNow,
            null, false, OperationType.Refund, amount);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    private static PaymentOperation Correction(decimal amount, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), amount, 7, DateTime.UtcNow,
            null, false, OperationType.Correction);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    // ================================================================
    // The defect itself
    // ================================================================

    [Fact]
    public void Discount_IsExcludedFromTheCeiling_ButStillCountsInTotalPaid()
    {
        // Payment of 800 cash with a 200 discount: the patient handed over 800.
        var operations = new[] { Payment(800m, 200m) };

        Assert.Equal(1000m, PatientAccountCalculator.TotalPaid(operations));  // unchanged
        Assert.Equal(800m, AccountEntryCalculator.MaxRefundable(operations)); // cash only
    }

    [Fact]
    public void DiscountedPayment_OneUnitAboveTheCashPaid_IsAboveTheCeiling()
    {
        var operations = new[] { Payment(800m, 200m) };

        Assert.Equal(800m, AccountEntryCalculator.MaxRefundable(operations));
        Assert.True(800.01m > AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void Balance_IsUnchangedByTheFix()
    {
        var operations = new[] { Payment(800m, 200m) };
        var prices = new[] { 1000m };

        // The discount still reduces the balance: 1000 - (800 + 200) = 0.
        Assert.Equal(0m, PatientAccountCalculator.Balance(prices, operations));
        // ...while only 800 is refundable.
        Assert.Equal(800m, AccountEntryCalculator.MaxRefundable(operations));
    }

    // ================================================================
    // The other operation types
    // ================================================================

    [Fact]
    public void Surcharge_IsNotRefundable_ItIsMoneyOwedNotMoneyPaid()
    {
        var operations = new[] { Payment(500m), Surcharge(300m) };

        Assert.Equal(500m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void Correction_CountsAsPaidCash_ExactlyAsTotalPaidTreatsIt()
    {
        var operations = new[] { Payment(500m), Correction(50m) };

        Assert.Equal(550m, PatientAccountCalculator.TotalPaid(operations));
        Assert.Equal(550m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void VoidedOperations_ContributeNothing_OfAnyType()
    {
        var operations = new[]
        {
            Payment(500m),
            Payment(400m, voided: true),
            Surcharge(300m, voided: true),
            Correction(70m, voided: true),
            Refund(60m, voided: true)
        };

        Assert.Equal(500m, AccountEntryCalculator.MaxRefundable(operations));
    }

    // ================================================================
    // Mixed scenarios
    // ================================================================

    [Fact]
    public void Mixed_DiscountsVoidsEarlierRefundsAndCorrections()
    {
        // Cash: 800 (disc 200) + 300 (disc 50) + correction 100 = 1200.
        // Surcharge 400 is not cash. Voided payment 999 is ignored.
        // Earlier refund 250 is subtracted.  => 950 refundable.
        var operations = new[]
        {
            Payment(800m, 200m),
            Payment(300m, 50m),
            Correction(100m),
            Surcharge(400m),
            Payment(999m, voided: true),
            Refund(250m)
        };

        Assert.Equal(950m, AccountEntryCalculator.MaxRefundable(operations));
        // TotalPaid still includes both discounts: 800+200+300+50+100 - 250 = 1200.
        Assert.Equal(1200m, PatientAccountCalculator.TotalPaid(operations));
    }

    [Fact]
    public void RefundEqualToTheCashPaid_LeavesZero()
    {
        var operations = new[] { Payment(800m, 200m), Refund(800m) };

        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void NeverNegative_WhenRefundsExceedCash()
    {
        var operations = new[] { Payment(100m), Refund(150m) };

        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void NoOperationsAtAll_YieldsZero()
    {
        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(Array.Empty<PaymentOperation>()));
    }

    [Fact]
    public void FullSettlement_CountsAsPaidCash()
    {
        var settlement = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), 1000m, 7, DateTime.UtcNow,
            null, false, OperationType.FullSettlement);

        Assert.Equal(1000m, AccountEntryCalculator.MaxRefundable(new[] { settlement }));
    }
}

```


### tests/TopLab.Presentation.Tests/Patients/PatientAccountPercentageTests.cs

```csharp
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 5 — D-01 on the PATIENT ACCOUNT screen (<see cref="PatientAccountViewModel"/>).
/// The same eight required cases as the editor tests, driven through
/// <c>AccountEntryEnterCommand</c> exactly as the Enter key drives it, so both payment-entry
/// screens are proved to behave identically.
/// </summary>
public class PatientAccountPercentageTests
{
    private sealed class AccountSender : ISender
    {
        public decimal TotalCharged { get; set; } = 1000m;
        public decimal TotalPaid { get; set; }
        public int PaymentCalls { get; private set; }
        public List<RecordPaymentCommand> Payments { get; } = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetPatientAccountQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(new PatientAccountDto(
                        PatientId: 7,
                        PatientFullName: "أحمد محمد علي",
                        LabId: "LAB-1",
                        AccountType: "Individual",
                        TotalCharged: TotalCharged,
                        TotalPaid: TotalPaid,
                        TotalDiscount: 0m,
                        Balance: TotalCharged - TotalPaid,
                        ChargedTests: Array.Empty<ChargedTestDto>(),
                        Operations: Array.Empty<PaymentOperationDto>())));
                case ListPatientPaymentsQuery:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<PaymentOperationDto>>.Success(
                        Array.Empty<PaymentOperationDto>()));
                case RecordPaymentCommand payment:
                    PaymentCalls++;
                    Payments.Add(payment);
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                default:
                    throw new NotSupportedException($"AccountSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class SilentNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;
        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { }
    }

    private sealed class StubServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task<(PatientAccountViewModel Vm, AccountSender Sender)> LoadedAsync(
        decimal totalCharged, decimal totalPaid)
    {
        var sender = new AccountSender { TotalCharged = totalCharged, TotalPaid = totalPaid };
        var vm = new PatientAccountViewModel(
            sender, new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadAsync(7);
        return (vm, sender);
    }

    private static async Task PressEnterAsync(PatientAccountViewModel vm)
    {
        vm.AccountEntryEnterCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    // Case 1: TC 1000, nothing paid, amount 900, 10% -> accepted; first previews, second persists.
    [Fact]
    public async Task D01_Case1_FullSettlementByPercentage_IsAcceptedThenPersists()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "900";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(vm.StatusMessage));
        Assert.Equal(0, sender.PaymentCalls);

        await PressEnterAsync(vm);

        Assert.Equal(1, sender.PaymentCalls);
        Assert.Equal(900m, sender.Payments[0].Amount);
        Assert.Equal(100m, sender.Payments[0].DiscountAmount);
    }

    // Case 2: TC 1000, nothing paid, amount 100, 50% -> refused as unavailable.
    [Fact]
    public async Task D01_Case2_PartialPaymentWithPercentage_IsRefused()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "50";

        await PressEnterAsync(vm);

        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // Case 3: TC 1000, nothing paid, amount 1000, 10% -> would credit 100, refused.
    [Fact]
    public async Task D01_Case3_OverSettlementWithPercentage_IsRefused()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "1000";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // Case 4: TC 1000, already paid 400 (remaining 600), amount 500, 10% -> accepted.
    [Fact]
    public async Task D01_Case4_RemainingAfterAPartPayment_IsAccepted()
    {
        var (vm, sender) = await LoadedAsync(1000m, 400m);
        vm.PaymentAmountText = "500";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // Case 5: TC 1000, nothing paid, amount 400, 60% -> availability passes, Domain rule refuses.
    [Fact]
    public async Task D01_Case5_AvailabilityPassesThenTheDomainRuleRefuses()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "400";
        vm.PaymentPercentageText = "60";

        await PressEnterAsync(vm);

        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // Case 6: nothing remaining -> its own message.
    [Fact]
    public async Task D01_Case6_NoRemainingBalance_IsRefused()
    {
        var (vm, sender) = await LoadedAsync(1000m, 1000m);
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("لا يوجد مبلغ متبقٍ في الحساب لتطبيق نسبة عليه.", vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    // Case 7: value discount unchanged.
    [Fact]
    public async Task D01_Case7_ValueDiscount_IsUnchanged()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "100";
        vm.PaymentDiscountText = "20";

        await PressEnterAsync(vm);

        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);

        await PressEnterAsync(vm);

        Assert.Equal(1, sender.PaymentCalls);
        Assert.Equal(20m, sender.Payments[0].DiscountAmount);
    }

    // Case 8: the Round-4 fix is preserved — the percentage is never silently cleared.
    [Fact]
    public async Task D01_Case8_PercentageIsNeverSilentlyCleared()
    {
        var (vm, _) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "900";
        vm.PaymentPercentageText = "10";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Null(vm.PaymentPercentageText);
        Assert.NotEqual("", vm.PaymentDiscountText);
    }
}

```


### tests/TopLab.Presentation.Tests/Patients/PatientEditorCatalogModeTests.cs

```csharp
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 5 — D-02: the three mutually exclusive catalogue modes TM / TG / CG.
/// <para>
/// A view filter only: no database write, no schema change, and TM shows exactly the base set
/// the unfiltered catalogue shows today. The search box is active in TG only and its text is
/// cleared when the user leaves TG.
/// </para>
/// </summary>
public class PatientEditorCatalogModeTests
{
    private sealed class ModeSender : ISender
    {
        public IReadOnlyList<int> MemberTestIds { get; set; } = Array.Empty<int>();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetRegistrationCatalogQuery)
            {
                return Task.FromResult((TResponse)(object)Result<RegistrationCatalogDto>.Success(Catalog()));
            }

            if (request is GetCustomGroupTestIdsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<int>>.Success(MemberTestIds));
            }

            if (request is GetProfileDefinitionsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ProfileDefinitionDto>>.Success(
                    Array.Empty<ProfileDefinitionDto>()));
            }

            if (request is GetCustomGroupsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>>.Success(
                    Array.Empty<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>()));
            }

            if (request is GetNextLabIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<string>.Success("LAB-1"));
            }

            throw new NotSupportedException($"ModeSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private static RegistrationCatalogDto Catalog() => new(
            Tests: new[]
            {
                Test(1, "CBC", "صورة دم كاملة"),
                Test(2, "FBS", "سكر صائم"),
                Test(3, "LFT", "وظائف كبد"),
                Test(4, "CULT", "مزرعة")
            },
            TestGroups: Array.Empty<TestGroupDto>(),
            PatientTitles: Array.Empty<PatientTitleDto>(),
            MedicalConditionTypes: Array.Empty<MedicalConditionTypeDto>(),
            DefaultAccountType: AccountType.Individual,
            DisableAutoTitleInsertion: false,
            ReferralPlaceholderMale: string.Empty,
            ReferralPlaceholderFemale: string.Empty);

        private static TestSummaryDto Test(int id, string code, string name) => new(
            Id: id, TestCode: code, Name: name, ReportName: name, TestGroupId: null,
            TestGroupName: null, Barcode: null, CompletionDurationMinutes: 0, IsSentOut: false,
            PatientPrice: 100m, LabToLabPrice: null, IsActive: true);
    }

    private sealed class SilentNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel => null;
        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { }
    }

    private sealed class StubServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task<PatientEditorViewModel> LoadedAsync(params int[] memberTestIds)
    {
        var sender = new ModeSender { MemberTestIds = memberTestIds };
        var vm = new PatientEditorViewModel(
            sender, new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadCatalogAsync();
        return vm;
    }

    // ================================================================
    // Default mode and mutual exclusivity
    // ================================================================

    [Fact]
    public async Task DefaultMode_IsTg_AndItsEmptySearchEqualsTodaysList()
    {
        var vm = await LoadedAsync();

        Assert.Equal(CatalogFilterMode.Tg, vm.CatalogFilterMode);
        Assert.True(vm.IsTgMode);
        Assert.False(vm.IsTmMode);
        Assert.False(vm.IsCgMode);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
        Assert.True(vm.IsCatalogSearchEnabled);
    }

    [Fact]
    public async Task ExactlyOneModeIsActive_AtAnyTime()
    {
        var vm = await LoadedAsync(1);

        foreach (var mode in new[] { CatalogFilterMode.Tm, CatalogFilterMode.Tg, CatalogFilterMode.Cg })
        {
            vm.CatalogFilterMode = mode;

            Assert.Equal(mode, vm.CatalogFilterMode);
            Assert.Equal(mode == CatalogFilterMode.Tm, vm.IsTmMode);
            Assert.Equal(mode == CatalogFilterMode.Tg, vm.IsTgMode);
            Assert.Equal(mode == CatalogFilterMode.Cg, vm.IsCgMode);

            var active = new[] { vm.IsTmMode, vm.IsTgMode, vm.IsCgMode }.Count(x => x);
            Assert.Equal(1, active);
        }
    }

    [Fact]
    public async Task UndefinedMode_IsRejected_AndTheStateIsUnchanged()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        vm.CatalogFilterMode = (CatalogFilterMode)99;

        Assert.Equal(CatalogFilterMode.Tm, vm.CatalogFilterMode);
        Assert.True(vm.IsTmMode);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task TheThreeCommandsSetTheirMode()
    {
        var vm = await LoadedAsync();

        vm.SetTmModeCommand.Execute(null);
        Assert.Equal(CatalogFilterMode.Tm, vm.CatalogFilterMode);

        vm.SetTgModeCommand.Execute(null);
        Assert.Equal(CatalogFilterMode.Tg, vm.CatalogFilterMode);

        vm.SetCgModeCommand.Execute(null);
        Assert.Equal(CatalogFilterMode.Cg, vm.CatalogFilterMode);
    }

    // ================================================================
    // TM — every test
    // ================================================================

    [Fact]
    public async Task Tm_ShowsAllTests_EvenWithTextInTheBox()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterText = "CBC";     // in TG this narrows
        Assert.Single(vm.FilteredCatalogTests);

        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task Tm_ShowsTheSameBaseSetAsTheUnfilteredCatalogue()
    {
        var vm = await LoadedAsync();
        var baseSet = vm.CatalogTests.Select(t => t.Id).OrderBy(x => x).ToList();

        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(baseSet, vm.FilteredCatalogTests.Select(t => t.Id).OrderBy(x => x).ToList());
    }

    // ================================================================
    // TG — name or code
    // ================================================================

    [Theory]
    [InlineData("CBC", 1)]      // code match
    [InlineData("cbc", 1)]      // case-insensitive
    [InlineData("سكر", 1)]       // name match
    [InlineData("F", 2)]        // partial code: FBS + LFT
    [InlineData("ZZZ", 0)]      // no match
    [InlineData("", 4)]         // empty -> all
    [InlineData("   ", 4)]      // whitespace -> all
    public async Task Tg_MatchesByNameOrCode(string text, int expected)
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tg;

        vm.CatalogFilterText = text;

        Assert.Equal(expected, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task Tg_WithNoMatch_RaisesTheNoMatchFlag()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tg;

        vm.CatalogFilterText = "ZZZ";

        Assert.True(vm.ShowCatalogFilterNoMatch);
        Assert.False(vm.ShowCatalogEmpty);
    }

    // ================================================================
    // CG — custom-group members
    // ================================================================

    [Fact]
    public async Task Cg_WithNoGroups_ShowsNothing()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        Assert.Empty(vm.FilteredCatalogTests);
        Assert.False(vm.ShowCatalogEmpty);       // the catalogue itself is not empty
        Assert.False(vm.ShowCatalogFilterNoMatch); // that flag belongs to TG
    }

    [Fact]
    public async Task Cg_WithOneGroup_ShowsOnlyItsMembers()
    {
        var vm = await LoadedAsync(2);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal("FBS", only.TestCode);
    }

    [Fact]
    public async Task Cg_WithSeveralGroups_ShowsTheUnion()
    {
        var vm = await LoadedAsync(1, 3, 4);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        Assert.Equal(3, vm.FilteredCatalogTests.Count);
        Assert.Equal(
            new[] { 1, 3, 4 },
            vm.FilteredCatalogTests.Select(t => t.Id).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Cg_ATestInSeveralGroupsAppearsOnce()
    {
        // The Application query is DISTINCT, so the same id arriving twice must not duplicate
        // the row; the ViewModel stores the ids in a HashSet for the same reason.
        var vm = await LoadedAsync(1, 1, 1);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal(1, only.Id);
    }

    [Fact]
    public async Task Cg_IgnoresTheSearchText()
    {
        var vm = await LoadedAsync(1, 2);
        vm.CatalogFilterMode = CatalogFilterMode.Tg;
        vm.CatalogFilterText = "سكر";     // would narrow TG to FBS

        vm.CatalogFilterMode = CatalogFilterMode.Cg;

        Assert.Equal(2, vm.FilteredCatalogTests.Count);
    }

    // ================================================================
    // The search box's enabled state and the cleared text
    // ================================================================

    [Fact]
    public async Task SearchBox_IsEnabledInTgOnly()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterMode = CatalogFilterMode.Tg;
        Assert.True(vm.IsCatalogSearchEnabled);

        vm.CatalogFilterMode = CatalogFilterMode.Tm;
        Assert.False(vm.IsCatalogSearchEnabled);

        vm.CatalogFilterMode = CatalogFilterMode.Cg;
        Assert.False(vm.IsCatalogSearchEnabled);
    }

    [Fact]
    public async Task LeavingTg_ClearsTheSearchText()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterMode = CatalogFilterMode.Tg;
        vm.CatalogFilterText = "CBC";

        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(string.Empty, vm.CatalogFilterText);
    }

    [Fact]
    public async Task SwitchingBetweenTmAndCg_KeepsTheTextEmpty()
    {
        var vm = await LoadedAsync(1);

        vm.CatalogFilterMode = CatalogFilterMode.Tm;
        vm.CatalogFilterMode = CatalogFilterMode.Cg;
        vm.CatalogFilterMode = CatalogFilterMode.Tm;

        Assert.Equal(string.Empty, vm.CatalogFilterText);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    // ================================================================
    // XAML wiring (read as text — no WPF element is instantiated)
    // ================================================================

    [Fact]
    public void View_HasTheThreeRadioButtonsInOneGroupWithTooltips()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Content=\"TM\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"TG\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"CG\"", xaml, StringComparison.Ordinal);
        Assert.Contains("GroupName=\"CatalogMode\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"كل التحاليل\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"بحث بالاسم أو الكود\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ToolTip=\"مجموعاتي المخصصة\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsEnabled=\"{Binding IsCatalogSearchEnabled}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    private static string ReadPresentationFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(dir!.FullName, "src", "TopLab.Presentation", relativePath));
    }
}

```


### tests/TopLab.Domain.Tests/Billing/AccountEntryPercentageProbeTests.cs

```csharp
using TopLab.Domain.Billing;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 5 — D-01 (owner decision, option 4 of card D-01).
/// <para>
/// The percentage discount is available ONLY when the payment settles the whole remaining
/// balance: <c>amount + percentage-discount == TotalCharged - net TotalPaid</c>. The percentage
/// keeps its base (the patient's TOTAL account amount, rounded to 2 decimals away from zero).
/// The Domain rule "discount cannot exceed the operation amount" stays untouched and is applied
/// as the SECOND check.
/// </para>
/// <para>
/// This file replaces the Round-4 characterization probe of the same name, which asserted the
/// pre-decision behaviour (percentage computed from the total and then rejected on every partial
/// payment). The Round-4 file stated in its own header that it "must be rewritten to assert the
/// chosen behaviour" once the owner decided. That is what this is.
/// </para>
/// </summary>
public class AccountEntryPercentageProbeTests
{
    private static AccountEntrySession Session(string amount, string? percentage = null, string? discount = null)
        => new() { AmountText = amount, PercentageText = percentage, DiscountText = discount };

    // ================================================================
    // The five required cases
    // ================================================================

    /// <summary>Case 1: TC 1000, nothing paid, amount 900, 10% → discount 100, accepted.</summary>
    [Fact]
    public void D01_Case1_FullSettlementByPercentage_IsAccepted()
    {
        var session = Session("900", "10");

        var conversionError = session.ApplyPercentage(totalAccountAmount: 1000m);
        var first = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.Null(conversionError);
        Assert.Equal("100", session.DiscountText);   // 10% of 1000
        Assert.True(first.IsValid);
        Assert.False(first.ShouldPersist);           // first Enter previews only
        Assert.Equal(100m, first.Discount);
        Assert.Equal(900m, first.Amount);

        // second Enter with unchanged inputs persists
        var second = session.PressEnter(1000m, 0m);
        Assert.True(second.ShouldPersist);
    }

    /// <summary>Case 2: TC 1000, nothing paid, amount 100, 50% → refused as unavailable.</summary>
    [Fact]
    public void D01_Case2_PartialPaymentWithPercentage_IsRefusedAsUnavailable()
    {
        var session = Session("100", "50");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(1000m, 0m);

        Assert.False(step.IsValid);
        Assert.False(step.ShouldPersist);
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            step.ErrorMessage);
    }

    /// <summary>Case 3: TC 1000, nothing paid, amount 1000, 10% → would create a 100 credit, refused.</summary>
    [Fact]
    public void D01_Case3_OverSettlementWithPercentage_IsRefused()
    {
        var session = Session("1000", "10");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(1000m, 0m);

        Assert.False(step.IsValid);
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            step.ErrorMessage);
    }

    /// <summary>Case 4: TC 1000, already paid 400 (remaining 600), amount 500, 10% → accepted.</summary>
    [Fact]
    public void D01_Case4_RemainingBalanceAfterAPartPayment_IsAccepted()
    {
        var session = Session("500", "10");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 400m);

        Assert.True(step.IsValid);
        Assert.Null(step.ErrorMessage);
        Assert.Equal(100m, step.Discount);            // 10% of the TOTAL (1000), not of the 600
        Assert.Equal(500m, step.Amount);
        Assert.Equal(1000m - (400m + 500m + 100m), step.Preview!.BalanceAfter);
    }

    /// <summary>
    /// Case 5: TC 1000, nothing paid, amount 400, 60% → 400 + 600 = 1000 passes availability,
    /// then the Domain rule refuses because 600 &gt; 400. Proves the ORDER of the two checks.
    /// </summary>
    [Fact]
    public void D01_Case5_AvailabilityPassesThenTheDomainRuleRefuses()
    {
        var session = Session("400", "60");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(1000m, 0m);

        Assert.False(step.IsValid);
        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", step.ErrorMessage);
    }

    /// <summary>Case 6: nothing remaining (or a credit) → refused with its own message.</summary>
    [Theory]
    [InlineData(1000, 1000)]   // remaining 0
    [InlineData(1000, 1200)]   // remaining negative (credit)
    public void D01_Case6_NoRemainingBalance_IsRefused(decimal totalCharged, decimal alreadyPaid)
    {
        var session = Session("100", "10");

        session.ApplyPercentage(totalCharged);
        var step = session.PressEnter(totalCharged, alreadyPaid);

        Assert.False(step.IsValid);
        Assert.Equal("لا يوجد مبلغ متبقٍ في الحساب لتطبيق نسبة عليه.", step.ErrorMessage);
    }

    /// <summary>Case 7: a value discount is untouched by D-01 — same results as before.</summary>
    [Fact]
    public void D01_Case7_ValueDiscount_IsUnchanged()
    {
        var session = Session("900", discount: "100");

        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.True(step.IsValid);
        Assert.Equal(100m, step.Discount);
        Assert.Equal(900m, step.Amount);
        Assert.False(step.ShouldPersist);
    }

    /// <summary>A value discount on a partial payment still works, as it did before D-01.</summary>
    [Fact]
    public void D01_Case7b_ValueDiscountOnAPartialPayment_StillWorks()
    {
        var session = Session("100", discount: "20");

        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.True(step.IsValid);
        Assert.Equal(20m, step.Discount);
    }

    /// <summary>
    /// Case 8: the Round-4 fix is preserved — converting a percentage never silently discards
    /// the discount it produced.
    /// </summary>
    [Fact]
    public void D01_Case8_PercentageIsNeverSilentlyCleared()
    {
        var session = Session("900", "10");

        var error = session.ApplyPercentage(1000m);

        Assert.Null(error);
        Assert.Equal("100", session.DiscountText);   // the amount survives the conversion
        Assert.Null(session.PercentageText);         // the percentage field is cleared (never both)
    }

    // ================================================================
    // The boundary and the ordering
    // ================================================================

    /// <summary>The availability rule is an equality, so both sides of it are refused.</summary>
    [Theory]
    [InlineData(1000, 0, 900, 9.99, false)]    // 900 + 99.9 = 999.9 -> refused
    [InlineData(1000, 0, 900, 10, true)]       // 900 + 100 = 1000 -> accepted
    [InlineData(1000, 0, 900, 10.01, false)]   // 900 + 100.1 = 1000.1 -> refused
    public void D01_TheAvailabilityRuleIsAnEquality(
        decimal totalCharged, decimal alreadyPaid, decimal amount, decimal percentage, bool accepted)
    {
        var session = Session(
            amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            percentage.ToString(System.Globalization.CultureInfo.InvariantCulture));

        session.ApplyPercentage(totalCharged);
        var step = session.PressEnter(totalCharged, alreadyPaid);

        Assert.Equal(accepted, step.IsValid);
    }

    /// <summary>
    /// With no percentage at all the availability rule never runs, so a partial payment is
    /// unaffected — the value-discount path is exactly as it was.
    /// </summary>
    [Fact]
    public void D01_WithoutAPercentage_TheAvailabilityRuleDoesNotRun()
    {
        var session = Session("100");

        var step = session.PressEnter(1000m, 0m);

        Assert.True(step.IsValid);
        Assert.Equal(100m, step.Amount);
    }

    /// <summary>Rounding follows the panel convention: 2 decimals, away from zero.</summary>
    [Fact]
    public void D01_PercentageIsRoundedToTwoDecimalsAwayFromZero()
    {
        // 33.333% of 100 = 33.333 -> 33.33, so 66.67 + 33.33 = 100 exactly.
        var session = Session("66.67", "33.333");

        session.ApplyPercentage(100m);
        var step = session.PressEnter(100m, 0m);

        Assert.Equal("33.33", session.DiscountText);
        Assert.True(step.IsValid);
    }
}

```


---

## (c) Reference quotes with line numbers

Source: `Docs/Audit/00-combined-laboratory-system-STAGE1-READY-RECONCILED-20261021-R1.md`.

### D-01 — percentage discount
- L975: `| 1.3.1.2 | Discount Value Field | قيمة الخصم | Field / UI element | The discount applied to the patient's account as an absolute amount. | [VERIFIED] | In scope |`
- L976: `| 1.3.1.3 | Discount Percentage Field | % | Field / UI element | The discount applied to the patient's account as a percentage. | [VERIFIED] | In scope |`
- L977: `| 1.3.1.4 | Paid Amount Field | المدفوع | Field / UI element | The amount paid by the patient; entered by the user. | [VERIFIED] | In scope |`
- L979: `| 1.3.1.4.2 | Enter Key Account Operation | — | Rule / Rule | The first `Enter` press performs the accounting operation for the patient; the second press saves the operation. | [VERIFIED] | In scope |`
- L1633: `| 1.3.1.4.2 | The two `Enter` presses are not interchangeable: the first computes, the second saves. | [VERIFIED] |`

**The reference is silent on the availability of the percentage on a partial payment.** It never
states the base of the percentage (the owner fixed it: the patient's total account amount) and never
states whether a percentage may be used when the payment does not settle the balance. That silence is
what card D-01 asked about, and what the owner has now decided.

### D-02 — TM / TG / CG
- L936: `| 1.2.2.5.1.2 | Test Filter Mode Options | TM / TG / CG | Field group / Container | Three mutually exclusive mode options marked with the short codes TM, TG and CG. The meaning of the codes is not established. | [UNVERIFIED] | In scope |`
- L1715: `| 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | [UNVERIFIED] |`
- L4796: `| Q-003 | B — element visible, its behaviour not established | 1 | 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | The element is visible in the source. | The meaning of the three test-filter mode codes is not established. | showcase p. 36 | The behaviour of the element is not established; the element itself is. |`
- L935: `| 1.2.2.5.1.1 | Test Filter Field | start | Field / UI element | The filter control of the master test list. | [VERIFIED] | In scope |`
- L279–L283 (tree): `1.2.2.5.1 Test Filter Bar` containing `1.2.2.5.1.1 Test Filter Field`, `1.2.2.5.1.2 Test Filter Mode Options`, `1.2.2.5.1.3 Complete Test Menu Action`, `1.2.2.5.1.4 Test Group Menu Action`.

The reference marks the mode options `[UNVERIFIED]` and states three times that the meaning of the
codes is not established. The owner supplied the meaning (D-02), which is why the feature could be
built at all.

### D-03 — the two "menu" identifiers
- L937: `| 1.2.2.5.1.3 | Complete Test Menu Action | complete test menu | Action / Function | Opens the complete test menu of the master test list. | [VERIFIED] | In scope |`
- L938: `| 1.2.2.5.1.4 | Test Group Menu Action | test group menu | Action / Function | Opens the test-group menu of the master test list. | [VERIFIED] | In scope |`

### Part B — refund
- L1000: `| 1.3.2.5 | Refund Action | استرداد | Action / Function | Records a refund against the account. | [VERIFIED] | In scope |`
- L1002: `| 1.3.2.7 | Clear Account Action | تصفير الحساب | Action / Function | Clears the account of the patient. | [VERIFIED] | In scope |`

The reference describes neither the refund ceiling nor what "clears" means for individual operation
types — which is exactly why Part B was a defect fix (the owner's rule was already final) and D-08 is
a decision.

### Other identifiers touched for completeness
- L978: `| 1.3.1.4.1 | Surcharge Entry Convention | — | Rule / Rule | An additional amount that is not linked to a test — an extra service or an additional cost — is entered in the paid field preceded by a `+` sign, and is then added to the total of the tests. | [VERIFIED] | In scope |`
- L1632: `| 1.3.1.4.1 | An amount prefixed with `+` in the paid field is treated as an addition to the total of the tests; an unprefixed amount is the payment itself. | [VERIFIED] |`
- L1078: `| 1.6.1.3.2 | Print Preview Action | معاينة | Action / Function | Shows the report preview before printing. | [VERIFIED] | In scope |`
- L1440–L1443: the four composite-screen actions (`1.22.12.2`–`1.22.12.5`), unchanged from Round 4.

---

## (d) The owner decisions, in the owner's wording

### D-01 (option 4 of card D-01)
> the percentage discount is available only when the payment settles the whole remaining balance. The
> owner delegated the exact rule; it is: the percentage is accepted only when amount + percentage-discount
> EQUALS the remaining balance, where remaining balance = TotalCharged - net TotalPaid as the loaded
> account shows it. The percentage keeps its base: the patient's TOTAL account amount (TotalCharged),
> rounded to 2 decimals (MidpointRounding.AwayFromZero). Otherwise the entry is refused with a clear
> Arabic message, for example: "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = {remaining}."
> The Domain rule "discount cannot exceed the operation amount" stays untouched and keeps working as a
> second check, applied after the availability check. The value-discount field keeps working exactly as
> today. No change to the balance formula. No migration, no schema, no permission code.

### D-02 (option 1 of card D-02)
> three mutually exclusive mode buttons above the catalogue list on the registration screen (the
> existing test-catalogue UI of PatientEditorView). Labels: TM, TG, CG (with a short static Arabic
> tooltip each: TM "كل التحاليل", TG "بحث بالاسم أو الكود", CG "مجموعاتي المخصصة"). They are a view
> filter only: no database write, no schema. … TM: shows ALL tests registered in the system. Use exactly
> the same base set the unfiltered catalogue shows today. … TG: shows only the tests that match the text
> in the search box (by name OR by code, same matching rule as the existing Round 4 filter). With an
> EMPTY search box it shows all tests. … CG: shows only the tests that belong to at least one CustomGroup
> (the union over all custom groups). No group picker. … The search box is ACTIVE in TG only. … When the
> user leaves TG, clear the text so no hidden state remains. … Default mode when the screen opens, and
> after a new or reset patient: TG … The mode is an enum; setting a value that is not a defined member is
> rejected (state unchanged); exactly one mode is active at any time.

### D-03 (interpretation A)
> there are NO separate "complete test menu" and "test group menu" buttons. Identifiers 1.2.2.5.1.3 and
> 1.2.2.5.1.4 describe the two lists that already exist on the screen (the catalogue list and the
> patient's selected-tests list); 1.2.2.5.1.3 is covered by mode TM and 1.2.2.5.1.4 by mode CG of the
> catalogue list. Do NOT add buttons, popups, lists or any extra action.

---

## "Owner interpretation of 1.2.2.5.1.3 and 1.2.2.5.1.4"

The owner reads the two identifiers as a description of the two lists that already exist on the
registration screen — the catalogue list and the patient's selected-tests list — and not as two
additional actions. Under that reading:

- **1.2.2.5.1.3 "Complete Test Menu Action"** is covered by mode **TM** of the catalogue list: TM shows
  the complete test menu (every test the catalogue already shows).
- **1.2.2.5.1.4 "Test Group Menu Action"** is covered by mode **CG** of the catalogue list: CG shows the
  test-group menu (the tests that belong to the custom groups).

The reference itself classifies both as **"Action / Function"** and describes them as "Opens the complete
test menu of the master test list" (L937) and "Opens the test-group menu of the master test list" (L938).
The owner reads them instead as a description of the existing lists. No button, popup, list or extra
action was added; the two identifiers are satisfied by TM and CG.

Reference quotes:
- L937: `| 1.2.2.5.1.3 | Complete Test Menu Action | complete test menu | Action / Function | Opens the complete test menu of the master test list. | [VERIFIED] | In scope |`
- L938: `| 1.2.2.5.1.4 | Test Group Menu Action | test group menu | Action / Function | Opens the test-group menu of the master test list. | [VERIFIED] | In scope |`

Note on the classification difference: the reference calls them **Action / Function**; the owner reads
them as a description of the existing lists. This is recorded as an interpretation, not as a
contradiction, because the reference describes *what the operator reaches* (the complete list, the group
list) and the owner's reading supplies *how* it is reached (a mode of the one catalogue list).


---

## (e) V6 — the refund ceiling, BEFORE and AFTER the fix

Measured by executing the code, with a temporary probe test that was then deleted
(`TempRefundProbe.cs` in `TopLab.Domain.Tests`, `TempRefundHandlerProbe.cs` in
`TopLab.Application.Tests`).

### Setup
One payment operation: `Amount = 800`, `DiscountAmount = 200`, no voids, no earlier refunds.
(`PaymentOperation.Create(id, patientId, 800m, userId, now, 200m)`.)

### BEFORE the fix

| Figure | Value | Source |
|---|---|---|
| `PatientAccountCalculator.TotalPaid` | **1000** | sums `Amount + DiscountAmount` |
| `AccountEntryCalculator.MaxRefundable` | **1000** | delegated to `TotalPaid`, floored at 0 |
| `PatientAccountCalculator.Balance` (prices 1000) | **0** | 1000 − 1000 |
| `RecordRefundCommandHandler` with a refund of **1000** | **accepted** (`IsSuccess = true`, no error) | the handler compares against `MaxRefundable` |
| Same handler with a refund of **1000.01** | **refused**: `مبلغ الاسترداد يتجاوز إجمالي المدفوع الصافي (0).` | the ceiling had already been consumed by the first refund |

With a correction of 50 added to the same account, the probe measured
`TotalPaid = 1050` and `MaxRefundable = 1050` — i.e. the correction's `Amount` already counted as
refundable cash, which is the treatment Part B keeps.

**Conclusion before the fix:** the laboratory would hand back 1000 in cash for a patient who paid 800
and received a 200 discount. The 200 was never money.

### Does any existing test cover a discounted payment in the refund ceiling?

**No.** Searching the whole test tree for `MaxRefundable` returns exactly two assertions, both in
`tests/TopLab.Domain.Tests/Billing/AccountEntryParserTests.cs`:
`MaxRefundable_IsNetPaidAfterEarlierRefundsAndVoids` (L298, asserts 120 from two plain payments of
100 and 50 minus a 30 refund, with a voided 40) and `MaxRefundable_NeverNegative` (L319, asserts 0).
Neither constructs a payment with a `DiscountAmount`. `RecordRefundCommandHandlerTests` had no
discounted payment either (its only discount mention was a comment in the balance-reconciliation test).
So the defect was uncovered by the suite.

### AFTER the fix

| Figure | Value | Source |
|---|---|---|
| `PatientAccountCalculator.TotalPaid` | **1000** (unchanged) | untouched |
| `PatientAccountCalculator.CashRefundable` | **800** | new: sums only the cash `Amount` |
| `AccountEntryCalculator.MaxRefundable` | **800** | now delegates to `CashRefundable` |
| `Balance` (prices 1000) | **0** (unchanged) | untouched |
| Refund of **800** | accepted | equals the cash paid |
| Refund of **800.01** | refused: `مبلغ الاسترداد يتجاوز إجمالي المدفوع النقدي الصافي (800).` | one unit above |
| Refund of **1000** | refused | the exact case that used to succeed |

Covered by 11 new Domain tests (`RefundCeilingTests`) and 5 new Application tests
(`RecordRefundCommandHandlerTests`), listed in §(f).

### How corrections are treated (reported, as required)

`PatientAccountCalculator.TotalPaid` treats a Correction as an ordinary paid row: its filter is
`!IsVoided && !IsExtraCharge && OperationType != Refund`, so `Correction` passes and its `Amount` is
summed. `CashRefundable` uses **the same filter** and sums only `Amount`, so a correction's cash value
counts as refundable. Measured: payment 500 + correction 50 → `TotalPaid = 550`, `MaxRefundable = 550`.
A voided correction counts in neither. `FullSettlement` likewise counts as paid cash in both.

### Message and screen text

The handler's message changed from `…إجمالي المدفوع الصافي ({maxRefundable}).` to
`…إجمالي المدفوع النقدي الصافي ({maxRefundable}).` — the word «النقدي» was added so the figure quoted
is unambiguous. A search of `src/` for `المدفوع الصافي`, `MaxRefundable` and any refundable figure in
XAML found **no screen that displays a refundable figure**: the refund controls are a text box
(`RefundAmountText`) and a button (`RefundCommand`) on `PatientEditorView.xaml:53-54` and
`PatientAccountView.xaml:90-91`, and the ceiling surfaces only through the handler's error message.
No screen text needed changing.

---

## (f) Tests whose expected values changed, and why

| Test | Was | Now | Why |
|---|---|---|---|
| `AccountEntryPercentageProbeTests` (whole file, 8 tests) | asserted the pre-decision behaviour: percentage computed from the total and then rejected on every partial payment by the Domain over-discount rule | rewritten as 17 tests asserting D-01: availability equality, the five required cases, the two extra cases, the boundary, rounding | The file's own header said it "must be rewritten to assert the chosen behaviour" once the owner decided. The owner decided. |
| `PatientEditorPercentageConflictTests.V1_PartialPayment_WithPercentage_ShowsTheDiscountExceedsMessage` | expected `"الخصم لا يمكن أن يتجاوز مبلغ الدفع."` | renamed `…ShowsTheAvailabilityMessage`, expects `"النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000."` | The availability check now runs first, so that is the message the operator sees. The assertions were not weakened: same exact-message requirement, same converted-discount requirement, same "no status line", same "no mediator call". |
| `PatientEditorPercentageConflictTests.V2_FullPayment_FivePercent_ShowsFiguresAndRecordsNothing` | expected accepted + status line | renamed `…IsRefusedAsOverSettlement`, expects the availability message | With nothing previously paid, 1000 + 50 over-settles by 50, which D-01 refuses. |
| `PatientEditorPercentageConflictTests.V2_FullPayment_HundredPercent_IsAccepted` | expected accepted | renamed `…IsRefusedAsOverSettlement`, expects the availability message | 1000 + 1000 would credit 1000. |
| `PatientEditorPercentageConflictTests.V2_PartialPayment_HundredPercent_IsRejected` | expected the Domain over-discount message | expects the availability message (`…= 100.`) | Availability runs first. |
| `PatientEditorCatalogFilterTests` (10 tests) | — | **unchanged, all pass** | The default mode is TG and its empty-search result equals today's list, exactly as the owner specified. |
| `PatientEditorMedicalHistoryTests` (2 tests) | — | test fixture only: added a canned response for the new `GetCustomGroupTestIdsQuery` | The editor now issues that query while loading picker sources; without a canned response the fake throws `NotSupportedException`. No assertion changed. |
| `RecordRefundCommandHandlerTests.OverRefund_IsRejected_WithTheNetPaidInTheMessage` | — | **unchanged and still passing** | Its account has no discount, so the cash ceiling equals the old ceiling. |
| `AccountEntryParserTests.MaxRefundable_*` (2 tests) | — | **unchanged and still passing** | Same reason: no discounts in those fixtures. |
| `AccountEntrySessionTests` (17 tests) | — | **unchanged and still passing** | No percentage-availability case among them. |

No assertion was weakened anywhere. The only test file deleted-and-rewritten is
`AccountEntryPercentageProbeTests`, which was a characterization probe by design.


---

## (g) Part C — the two new Decision Cards

Cards D-04, D-05 and D-06 are **DEFERRED by the owner** and keep their Round 4 text verbatim in
`Docs/Audit/R1-ROUND4-EVIDENCE-20261025.md` §(f). They are not repeated here and nothing was implemented
for them.

---

### D-07: Any signed-in user can refund money and void a whole patient account

**Question.** Should refunding money and clearing a patient's whole account keep working for every
signed-in user, or should each of them require its own permission code?

**Reference evidence.**
- L1000: `| 1.3.2.5 | Refund Action | استرداد | Action / Function | Records a refund against the account. | [VERIFIED] | In scope |`
- L1002: `| 1.3.2.7 | Clear Account Action | تصفير الحساب | Action / Function | Clears the account of the patient. | [VERIFIED] | In scope |`
- Stage 3 §3.5 GAP-13 states the criterion: *"Security/access: refund and clear require cashier permission."*

**The reference is silent on the permission.** It names the two actions and nothing about who may use
them. The only statement about access is the roadmap's own acceptance criterion, which says "cashier
permission" without saying whether that is a new code or an existing one.

**Why only the owner can decide.** This is a separation-of-duties question with money attached, and the
codebase currently answers it in the weakest possible way. The permission system is a table of thirteen
seeded codes (`PermissionConfiguration.cs` L17–L31, ids 1–13: `ADD_EDIT_PATIENT`, `EDIT_RESULTS`,
`REVIEW_RESULTS`, `PRINT_RESULTS`, `BLOCK_PRINT_ON_BALANCE`, `DELIVER_RESULTS`, `DISCOUNT_LIMIT`,
`PRINT_WORKSHEET`, `DELETE_PATIENT`, `EDIT_SYSTEM_SETTINGS`, `CASH_DISBURSE_DEPOSIT`, `STATISTICS`,
`PT_AUDIT_ACCESS`), enforced by a MediatR behaviour that only runs for commands implementing
`IAuthorizedRequest`. `RecordRefundCommand` and `ClearPatientAccountCommand` deliberately do **not**
implement it — they are plain `IRequest<Result<int>>` records, exactly like `RecordPaymentCommand`.
The consequence, verified in the code: a registrar who can register a patient can also hand back cash
and can void every financial row on that patient's account, and the only trace is the void flag.
Meanwhile the *reads* of the same account (`GetPatientAccountQuery`, `ListPatientPaymentsQuery`,
`GetPatientReceiptQuery`, `GetPatientInvoiceQuery`) all require `CASH_DISBURSE_DEPOSIT` — so the system
already treats the cash desk as a privileged area for reading, but not for refunding. That asymmetry is
a business rule, not a technical one: it depends on whether this laboratory wants a second person to
approve money leaving the till. Only the owner knows how the counter actually works.

**Affected.** GAP-13 (the §3.5 security criterion); identifiers 1.3.2.5, 1.3.2.7;
`RecordRefundCommand`, `ClearPatientAccountCommand` and their handlers;
`PatientBillingAccessPolicy`; `PermissionConfiguration.cs` (the seed); `UserManagementViewModel`
(its permission list, L52–L53); both account-panel ViewModels; tests
`PatientBillingAuthorizationTests`, `RecordRefundCommandHandlerTests`,
`ClearPatientAccountCommandHandlerTests`.

**Options.**

1. **Keep both ungated (today's behaviour).**
   - What changes for the user: nothing — anyone signed in can refund and clear.
   - Data / audit / financial correctness: a refund and a clear both leave an audit trail (a refund row;
     void flags on every cleared row, with `ReceivedByUserId` on the original rows unchanged), but there
     is no record of *who* pressed refund or clear beyond the new row's receiver. No financial figure
     changes.
   - Migration / schema / permission: **no** to all three.
   - Effort: none. Risk: **high** — the exact scenario the roadmap's criterion warns about.
2. **Two new permission codes: `REFUND_PAYMENT` and `CLEAR_ACCOUNT`.**
   - What changes for the user: a user without the code sees the refund and clear buttons refused with
     the standard forbidden message; a supervisor grants the codes per user.
   - Data / audit / financial correctness: the same rows and the same figures; the difference is who is
     allowed to create them. Audit trail unchanged in shape, but the actor becomes meaningful.
   - Migration / schema / permission: **yes to all three** — two rows in `PermissionConfiguration`
     `HasData` (ids 14 and 15), a new migration to apply the seed, and two `IAuthorizedRequest`
     implementations plus a `PatientBillingAccessPolicy` constant each. Note the seed is *data*, not
     schema, but it is delivered by a migration.
   - Effort: **medium**. Risk: **low** once the codes exist — the enforcement mechanism is already built
     and already tested for `RecordCorrectionCommand`.
3. **Reuse the existing `CASH_DISBURSE_DEPOSIT` code for both.**
   - What changes for the user: only users who already have the cash-desk permission can refund or clear;
     no new code is granted to anyone.
   - Data / audit / financial correctness: unchanged rows; enforcement matches the account *reads*,
     which already require this code — so whoever can see the account can also act on it.
   - Migration / schema / permission: **no** — the code is already seeded (id 11) and already enforced.
   - Effort: **low** — two `IAuthorizedRequest` implementations and one constant, no seed, no migration.
   - Risk: **low** for correctness, **medium** for operations: every cashier must already hold the code,
     and anyone who holds it for reading gains refund rights too.
4. **Reuse `CASH_DISBURSE_DEPOSIT` for refund, and require the existing `DELETE_PATIENT` for clear.**
   - What changes for the user: refunding needs the cash permission; clearing an account needs the
     destructive permission a supervisor already grants.
   - Data / audit / financial correctness: unchanged rows; "clear" becomes a supervisor-level action,
     which matches how destructive it is.
   - Migration / schema / permission: **no** — both codes are already seeded (11 and 9).
   - Effort: **low**. Risk: **medium** — `DELETE_PATIENT` is about deleting patients, so reusing it for
     account clearing overloads its meaning and makes the permission screen harder to explain.

**Recommendation: option 3 for the refund, and option 3 for clear-account as an interim — i.e. gate both
on the already-seeded `CASH_DISBURSE_DEPOSIT`, and raise option 2 as the follow-up if the owner wants a
separate refund right.** The reason is in the code: the six financial *reads* on this exact account
already require `CASH_DISBURSE_DEPOSIT` (`GetPatientAccountQuery` L11, `ListPatientPaymentsQuery` L13,
`GetPatientReceiptQuery` L11, `GetPatientInvoiceQuery` L15), so a user who cannot see the account cannot
reach the refund button anyway — gating the writes on the same code closes the only real hole (a
registrar who is not at the cash desk) with no seed, no migration and no new permission, which is
exactly what this round is allowed to do. Option 2 is the cleaner long-term answer and the enforcement
machinery already exists, but it needs two seeded rows and a migration, which this round forbids — so it
belongs to the round that is allowed to touch the seed. Options 1 and 4 are rejected: 1 leaves the hole
open, and 4 overloads a permission about deleting patients.

**If deferred.** GAP-13 keeps its functional completeness (refund and clear work and are tested) but its
§3.5 security criterion stays unmet. R1 can still ship, because the exposure is a policy choice the
owner has been running with since Round 3, not a defect introduced by it.

---

### D-08: Which operation types the clear-account action should void

**Question.** Clear-account voids every non-voided operation of the patient — should it keep doing that,
or should it leave surcharges (and corrections) standing?

**Reference evidence.**
- L1002: `| 1.3.2.7 | Clear Account Action | تصفير الحساب | Action / Function | Clears the account of the patient. | [VERIFIED] | In scope |`
- Stage 3 §3.5 GAP-13: *"Observable result: refund appears as a new operation; clear removes all operations (or marks them cleared, per OD-2)."*

**The reference is silent on operation types.** It says "Clears the account of the patient" and nothing
about which rows that covers. The roadmap's criterion says "all operations", which is what the code does
today — but the roadmap also flags the mechanism as OD-2-dependent, and the owner has since decided
(OD-12 b) that corrections are never overwritten, which leaves the question of whether a *void* of a
correction is the same thing as an *overwrite*.

**Why only the owner can decide.** "Clear the account" is three different financial events depending on
what it voids, and the numbers differ materially — which is why each option below carries a worked
example. Voids are irreversible in practice (the row stays, flagged, and nothing un-voids it), so a
wrong choice destroys the record of money the patient actually owes. A surcharge is money the patient
owes the laboratory for a service already rendered; voiding it forgives that debt. A correction is an
accounting adjustment a supervisor made on purpose; voiding it silently reverses a deliberate act. The
reference cannot break the tie because it never discusses operation types at all.

**Affected.** GAP-13; identifiers 1.3.2.7; `ClearPatientAccountCommandHandler` (its filter is
`o.PatientId.Value == request.PatientId && !o.IsVoided`); `PatientBillingReader` (the totals);
`PatientAccountCalculator.TotalCharged` / `TotalPaid`; both account-panel ViewModels; tests
`ClearPatientAccountCommandHandlerTests` (10 tests).

**Worked example, used by every option.** A patient has: tests totalling **1000**, a **surcharge of
300**, a **payment of 700**, a **correction of 100**, and an **earlier refund of 50**. Before clearing:

| Figure | Value | Why |
|---|---|---|
| `TotalCharged` | **1300** | 1000 tests + 300 surcharge |
| `TotalPaid` | **750** | 700 payment + 100 correction − 50 refund |
| `Balance` | **550** | 1300 − 750 |

**Options.**

1. **Void every non-voided operation (today's behaviour).**
   - What changes for the user: after pressing تصفير الحساب the whole account reads as unpaid again — the
     surcharge included.
   - Data / audit / financial correctness: every row survives, flagged voided; nothing is deleted. The
     surcharge debt of 300 is forgiven, and the supervisor's correction is reversed.
     **After:** `TotalCharged = 1000` (the surcharge no longer counts), `TotalPaid = 0`, `Balance = 1000`.
   - Migration / schema / permission: **no** to all three.
   - Effort: none. Risk: **high** — it forgives real debt and silently reverses a deliberate correction.
2. **Void payments, discounts and refunds only; keep surcharges and corrections.**
   - What changes for the user: the payments disappear from the account but the extra service charge and
     the supervisor's adjustment stay, so the patient still owes them.
   - Data / audit / financial correctness: the surcharge debt of 300 and the correction of 100 survive;
     the money actually collected is written off. **After:** `TotalCharged = 1300`,
     `TotalPaid = 100` (correction only; the refund was voided with the payments), `Balance = 1200`.
   - Migration / schema / permission: **no** to all three.
   - Effort: **low** — one extra clause in the handler's filter. Risk: **medium** — the balance grows,
     which may surprise the operator who expected "clear" to mean zero.
3. **Void payments only; keep surcharges, corrections and refunds.**
   - What changes for the user: only the cash payments are cancelled; the surcharge, the adjustment and
     the earlier refund stay exactly as they are.
   - Data / audit / financial correctness: the most conservative reading — it cancels the least.
     **After:** `TotalCharged = 1300`, `TotalPaid = 50` (100 correction − 50 refund, both still live),
     `Balance = 1250`.
   - Migration / schema / permission: **no** to all three.
   - Effort: **low**. Risk: **medium** — "clear" that leaves a 50 paid and a 300 charge on the account
     may not read as "cleared" to the operator.
4. **Void everything except corrections (corrections are never reversed, per OD-12 b).**
   - What changes for the user: everything the account was built from is cancelled, but the supervisor's
     accounting adjustment stands.
   - Data / audit / financial correctness: honours OD-12's "never overwrite a stored amount" in spirit —
     a correction is a deliberate accounting act and is left alone. **After:** `TotalCharged = 1000`,
     `TotalPaid = 100` (correction only), `Balance = 900`.
   - Migration / schema / permission: **no** to all three.
   - Effort: **low**. Risk: **low** — it changes the least while still reading as "cleared".

**Recommendation: option 4.** Two pieces of the owner's own record point at it. First, OD-12 (b) says
corrections are offsets that must never be overwritten — and a void is the harshest possible overwrite of
a correction's effect, so leaving corrections alone is the reading consistent with that decision. Second,
§3.5 says "clear removes all operations (or marks them cleared, per OD-2)" and ties the mechanism to OD-2,
so the owner already accepted that the *set* is a decision rather than a given. Option 4 still clears
everything the account was built from (tests-driven charges, surcharges, payments, refunds) so the
operator sees a genuinely cleared account, and it preserves the one row a supervisor created on purpose.
Option 1 is rejected because it forgives the 300 surcharge debt and reverses the correction; options 2
and 3 are rejected because they leave a balance that does not read as "cleared" while also keeping less
of the account than option 4.

**If deferred.** Nothing changes: the current behaviour (option 1) stays and GAP-13 remains functionally
complete. This is a refinement of an action that already works and is already tested; R1 ships either way.


---

## (h) Updated R1 status table

| Gap | Status | What remains | Blocked by |
|---|---|---|---|
| **GAP-04** | **partial** | the note is visible on both screens (`DeliveryHandoverView.xaml:30`, `ResultDeliveryView.xaml` ملاحظات column) and is tested; the acceptance criterion "an action opens the stored patient note" is not met | **D-05** (DEFERRED by the owner) |
| **GAP-05** | **complete** | nothing | — |
| **GAP-10** | **complete** | envelope print, follow-up, patient-window, lab-results all wired, guarded and tested; `IsEnabled` per §3.5 with the owner-decided patient-window exception | — |
| **GAP-11** | **complete** | filter field (Round 4), TM/TG/CG modes with mutual exclusivity and invalid-mode rejection (Round 5), "menus open" covered by TM and CG per the owner's interpretation of 1.2.2.5.1.3/.4 | — |
| **GAP-12** | **partial** | done: patient account, refresh, copy, patient data, combined report, empty report, envelope print. **Remaining: print of the delivery list (1.4.1.4.3), preview (1.4.1.4.4), notes action (1.4.1.4.6), medical history (1.4.1.4.9)** | **D-04** (print/preview) and **D-05** (notes), both DEFERRED; medical history was never carded because no target screen exists |
| **GAP-13** | **complete functionally / partial on security** | refund and clear-account work, are capped correctly (Part B) and are tested; the §3.5 security criterion "refund and clear require cashier permission" is not met | **D-07** |
| **GAP-14** | **complete** | percentage availability rule (D-01), `+` surcharge, two-press Enter, value discount — all tested on both screens | — (D-08 concerns clear-account, not this gap) |
| **GAP-22** | **partial** | print, preview and review are done and tested on the single-test screen; "open main result" is not | **D-06** (DEFERRED by the owner) |
| **GAP-30** | **not started** | the whole action | **D-06** (DEFERRED by the owner) |

---

## (i) Changed screen areas — row tables and ASCII drawings

**STATIC EVIDENCE ONLY.** Read from the XAML text. No screen was executed, rendered or visually verified.

### I1. `PatientEditorView.xaml` — the catalogue column, after D-02

The `DockPanel Grid.Column="1"` (L473) now has two new docked rows before the catalogue list.

| Order | Element | Dock | Content |
|---|---|---|---|
| 1 | TextBlock | Top | «التحاليل» |
| 2 | StackPanel | Top | «إجراءات الزيارة» (profile picker, sample types, group picker, «مسح الكل») |
| 3 | TextBlock | Top | «تحاليل المريض:» |
| 4 | TextBlock | Top | «لا توجد تحاليل مختارة.» |
| 5 | ListView | Top | `SelectedTests` (height 180) |
| 6 | TextBlock | Top | «كتالوج التحاليل:» |
| **7** | **WrapPanel** | **Top** | **NEW — three RadioButtons: TM / TG / CG, `GroupName="CatalogMode"`, tooltips «كل التحاليل» / «بحث بالاسم أو الكود» / «مجموعاتي المخصصة»** |
| 8 | WrapPanel | Top | «بحث:» + TextBox(`CatalogFilterText`, `IsEnabled="{Binding IsCatalogSearchEnabled}"`) + «مسح» + «لا توجد تحاليل مطابقة للبحث.» |
| 9 | TextBlock | Top | «لا توجد تحاليل في الكتالوج.» (only when the catalogue itself is empty) |
| 10 | ListView | Fill | `FilteredCatalogTests` |

```
DockPanel (Grid.Column=1, RTL)
 ┌──────────────────────────────────────────────────┐
 │ التحاليل                                          │  [1]
 │ إجراءات الزيارة: بروفايل [▾] إضافة                │  [2]
 │ تحاليل المريض:                                    │  [3]
 │ [ ListView: SelectedTests                 ]       │  [5]
 │ كتالوج التحاليل:                                  │  [6]
 │ (•) TM   ( ) TG   ( ) CG                          │  [7]  <-- NEW (default TG)
 │ بحث: [__________] [مسح]   (لا توجد مطابقة)        │  [8]  (disabled unless TG)
 │ [ ListView: FilteredCatalogTests           ]      │  [10]
 └──────────────────────────────────────────────────┘
```

Mode → what the list shows:

| Mode | List contents | Search box |
|---|---|---|
| **TM** | every row of `CatalogTests` (the same base set as the pre-D-02 unfiltered list) | disabled, text ignored |
| **TG** (default) | rows whose name OR code contains the search text; all rows when the text is empty | enabled |
| **CG** | rows whose id is in the union of custom-group member ids | disabled, text ignored |

Leaving TG clears the text (design detail reported per the owner's instruction).

### I2. `PatientEditorView.xaml` / `PatientAccountView.xaml` — the account strip

Unchanged in layout; only the percentage rule behind the existing fields changed (D-01). The refund
amount box and the refund / clear buttons are unchanged from Round 3.

```
دفعة: [____]  خصم: [____]  % [____]  [تسجيل مدفوع] [تسوية كاملة]  استرداد: [____] [استرداد] [تصفير الحساب]
```

---

## (j) Assumptions I was forced to make

| # | Assumption | Alternatives | Why |
|---|---|---|---|
| 1 | The D-01 availability rule compares `Round2(amount + discount)` against `Round2(TotalCharged - TotalPaid)`. | (a) compare unrounded; (b) compare with a tolerance. | The owner specified 2-decimal rounding for the percentage and "EQUALS" for the rule. Rounding both sides before comparing is the only reading that makes 66.67 + 33.33 = 100 pass exactly; a tolerance would let 999.99 settle 1000. |
| 2 | "net TotalPaid as the loaded account shows it" is the ViewModel's `TotalPaid` (from `GetPatientAccountQuery`), not a fresh query. | (a) re-read the account inside `PressEnter`. | `PressEnter` is a pure Domain method with no I/O by design; the ViewModel already passes `TotalCharged` and `TotalPaid` from the loaded account. |
| 3 | A percentage on an account with **zero or negative** remaining is refused with its own message rather than the equality message. | (a) use the equality message with `{remaining} = 0`. | A message saying "must equal 0" invites the operator to type a 0 payment, which the paid-amount parser already rejects. A distinct message states the actual situation. |
| 4 | The applied percentage is remembered in `_appliedPercentage` because `ApplyPercentage` clears the text field. | (a) do not clear the text until after the Enter press; (b) pass the percentage as a parameter to `PressEnter`. | The owner requires the two fields never to be applied together, and Round 4's fix requires the amount to survive. Remembering the value is the smallest change that keeps both invariants; (b) would change `PressEnter`'s signature and both ViewModels' call sites. |
| 5 | CG membership is read through a **new read-only query** (`GetCustomGroupTestIdsQuery`) rather than extending `CustomGroupSummaryDto`. | (a) add `TestIds` to the summary DTO; (b) load every group's detail. | The summary DTO is consumed by the group picker, which needs only id/name/count; adding ids there would change a shared DTO for one screen. A distinct query returns exactly the distinct id list CG needs. No schema change. |
| 6 | CG shows nothing when the patient has no custom groups, and does **not** raise the "no match" message. | (a) reuse the TG no-match message. | The two states mean different things ("your groups are empty" vs "your search matched nothing"); merging them would mislead. `ShowCatalogFilterNoMatch` is therefore TG-only. |
| 7 | `ShowCatalogEmpty` is based on the FULL `CatalogTests` count, not the filtered list. | (a) base it on `FilteredCatalogTests`. | If it were based on the filtered list, CG with no groups would claim "لا توجد تحاليل في الكتالوج" — false, and it would hide the real reason. |
| 8 | The mode is a Presentation enum (`CatalogFilterMode`), not a Domain type. | (a) Domain enum. | It is a view filter with no domain meaning; the Domain must not learn about screen modes. |
| 9 | An undefined enum value is rejected by re-raising the property notifications, so a bound RadioButton snaps back. | (a) throw. | Throwing from a property setter would crash the binding; GAP-11 asks that the mode be "rejected (state unchanged)", which a silent no-op with notifications achieves. |
| 10 | Corrections count as refundable cash (same filter as `TotalPaid`). | (a) exclude corrections from the ceiling. | The owner said "the net amount the patient ACTUALLY PAID, in cash" and told me to treat corrections the same way `TotalPaid` does. `TotalPaid` includes them. Reported explicitly in §(e). |
| 11 | The refund message now says «المدفوع النقدي الصافي». | (a) keep «المدفوع الصافي». | The old wording described the old (wrong) figure. The new wording names the cash ceiling so the number in the message cannot be misread. |
| 12 | `AccountEntryPercentageProbeTests` was rewritten rather than deleted. | (a) delete it and rely on `AccountEntrySessionTests`. | It carries the five owner-mandated cases in one readable place, and its old header explicitly said it must be rewritten once the decision was made. |

---

## (k) Final verification

**Baseline**, taken BEFORE the first edit: build started **2026-10-25T23:47:18**, finished
**2026-10-25T23:53:49**. Build **0 errors, 0 warnings**.
Domain 711, Application 1907, Infrastructure 527, Persistence 24 (+2 skipped), Presentation 336.

> Caveat, stated plainly: the Domain figure is 711 rather than 710 because I had already written one
> temporary V6 probe file into `TopLab.Domain.Tests` before launching the run. Subtracting that single
> probe gives **3504 passed, 2 skipped** — which matches Round 4's closing figure exactly. The probe was
> deleted immediately after the measurement and contributes nothing to the final numbers.

**Final**, after all edits: build started **2026-10-26T01:03:42**, finished **2026-10-26T01:08:04**.

| Suite | Passed | Skipped | Failed |
|---|---|---|---|
| TopLab.Domain.Tests | 728 | 0 | 0 |
| TopLab.Application.Tests | 1912 | 0 | 0 |
| TopLab.Infrastructure.Tests | 527 | 0 | 0 |
| TopLab.Persistence.Tests | 24 | 2 | 0 |
| TopLab.Presentation.Tests | 373 | 0 | 0 |
| **Total** | **3564** | **2** | **0** |

Build: `dotnet build TopLab.sln --no-incremental` → **0 errors, 0 warnings**.

Delta: **+60 tests**, 0 failures, 0 warnings before and after.

### Safety rules
- No `git commit` / `push` / `add` / `stash` / `reset` / `checkout` / `clean`. `HEAD` is
  `efc909c688ccd3375b6bf3bc15af617f3739c647` at start and end; nothing is staged.
- No migration created or applied. `20261025123435_AddRefundAmountToPaymentOperation` untouched.
  No database was touched.
- No permission code, seed row, or schema change was added. (D-07 explains what adding one would take
  and was deliberately not implemented.)
- Every pre-existing file in `Docs/Audit/` is byte-identical: SHA-256 of all 22 files recorded before
  and after; zero changed. This evidence file is the only new file there.
- No WPF screen was executed or visually verified; every layout statement in §(i) is static evidence.
- D-04, D-05 and D-06 were not touched and are recorded as DEFERRED by the owner.
