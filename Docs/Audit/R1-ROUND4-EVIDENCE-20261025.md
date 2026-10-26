# R1 Round 4 — Evidence

Generated 2026-10-25. Static evidence only: no WPF screen was executed, rendered or visually verified.
HEAD at start and end: `efc909c688ccd3375b6bf3bc15af617f3739c647` (unchanged).

## (a) Complete git diff of all tracked modified files this round

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
index b2c15ce..c4bd021 100644
--- a/src/TopLab.Domain/Billing/PatientAccountCalculator.cs
+++ b/src/TopLab.Domain/Billing/PatientAccountCalculator.cs
@@ -1,3 +1,4 @@
+using TopLab.Domain.Common.Enums;
 using TopLab.Domain.Billing;
 
 namespace TopLab.Domain.Billing;
@@ -27,9 +28,13 @@ public static class PatientAccountCalculator
 
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
index 196c0c4..c709ebd 100644
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
@@ -37,6 +41,7 @@ using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintRes
 using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
 using TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;
 using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
+using TopLab.Domain.Billing;
 using TopLab.Domain.Common.Enums;
 using TopLab.Domain.Patients;
 using TopLab.Presentation.Common;
@@ -102,6 +107,10 @@ public sealed class PatientEditorViewModel : ViewModelBase
     private string? _notes;
     private string _paymentAmountText = string.Empty;
     private string? _paymentDiscountText;
+    private string? _paymentPercentageText;
+    private string _refundAmountText = string.Empty;
+    private readonly AccountEntrySession _accountEntry = new();
+    private string _catalogFilterText = string.Empty;
     private decimal _totalCharged;
     private decimal _totalDiscount;
     private decimal _totalPaid;
@@ -144,11 +153,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
@@ -172,8 +185,12 @@ public sealed class PatientEditorViewModel : ViewModelBase
         NextSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct, nextPage: true));
         PrevSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct));
         ClearSearchCommand = new RelayCommand(ClearSearch);
+        ClearCatalogFilterCommand = new RelayCommand(_ => CatalogFilterText = string.Empty);
         OpenSearchResultCommand = new AsyncRelayCommand(async (param, ct) => await OpenSearchResultAsync(param as PatientSummaryDto, ct));
         OpenAccountCommand = new AsyncRelayCommand(async (_, ct) => await OpenAccountAsync(ct));
+        PatientWindowCommand = new AsyncRelayCommand((_, ct) => PatientWindowAsync(ct));
+        LabResultsCommand = new AsyncRelayCommand(async (_, ct) => await LabResultsAsync(ct));
+        FollowUpCommand = new AsyncRelayCommand(async (_, ct) => await FollowUpAsync(ct));
         AddProfileCommand = new AsyncRelayCommand(async (_, ct) => await AddProfileAsync(ct));
         AddCustomGroupCommand = new AsyncRelayCommand(async (_, ct) => await AddCustomGroupAsync(ct));
         ClearAllVisitTestsCommand = new AsyncRelayCommand(async (_, ct) => await ClearAllVisitTestsAsync(ct));
@@ -244,8 +261,36 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
+        => !string.IsNullOrWhiteSpace(_catalogFilterText) && FilteredCatalogTests.Count == 0;
+
     public ObservableCollection<TestGroupDto> TestGroups { get; } = new();
 
     public ObservableCollection<string> Titles { get; } = new();
@@ -537,9 +582,47 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
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
 
@@ -555,14 +638,46 @@ public sealed class PatientEditorViewModel : ViewModelBase
     public bool ShowSelectedTestsEmpty => SelectedTests.Count == 0;
 
     /// <summary>S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).</summary>
-    public bool ShowCatalogEmpty => CatalogTests.Count == 0;
+    public bool ShowCatalogEmpty => FilteredCatalogTests.Count == 0 && string.IsNullOrWhiteSpace(_catalogFilterText);
 
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
+        var needle = (_catalogFilterText ?? string.Empty).Trim();
+
+        FilteredCatalogTests.Clear();
+        foreach (var test in CatalogTests)
+        {
+            if (needle.Length == 0 || MatchesCatalogFilter(test, needle))
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
@@ -626,6 +741,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
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
@@ -636,6 +760,7 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     /// <summary>Phase 1 REF-068: laboratory-order slip print action.</summary>
     public AsyncRelayCommand PrintLabOrderCommand { get; }
+    public AsyncRelayCommand PrintEnvelopeCommand { get; }
 
     public RelayCommand AddTestToSelectionCommand { get; }
 
@@ -653,6 +778,16 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
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
 
@@ -679,6 +814,8 @@ public sealed class PatientEditorViewModel : ViewModelBase
                 CatalogTests.Add(test);
             }
 
+            ApplyCatalogFilter();
+
             TestGroups.Clear();
             foreach (var group in catalog.Value.TestGroups)
             {
@@ -1488,6 +1625,59 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
@@ -1617,6 +1807,13 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
@@ -1627,38 +1824,184 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
@@ -1695,6 +2038,36 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
index a0f52f8..0369086 100644
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
@@ -508,6 +530,26 @@
                     </ListView.ItemTemplate>
                 </ListView>
                 <TextBlock DockPanel.Dock="Top" Text="كتالوج التحاليل:" FontWeight="Bold" Margin="0,4" />
+                <!-- 1.2.2.5.1.1 «start»: the master test list's filter field. Narrows by name OR code. -->
+                <WrapPanel DockPanel.Dock="Top" Margin="0,0,0,4">
+                    <TextBlock Text="بحث:" Margin="2" VerticalAlignment="Center" />
+                    <TextBox Text="{Binding CatalogFilterText, UpdateSourceTrigger=PropertyChanged}"
+                             Width="180" Margin="2" />
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
@@ -520,7 +562,7 @@
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

        // 1.3.1.4.1: a surcharge carries no discount.
        if (amount.Kind == AccountEntryKind.Surcharge && discount.Value > 0m)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(AccountEntryParser.SurchargeWithDiscountMessage, amount.Kind, amount.Value, 0m);
        }

        // The domain rule stays untouched: a discount can never exceed the operation amount.
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


### src/TopLab.Application/Features/ResultsEntry/Common/ISimpleResultPreviewBuilder.cs

```csharp
namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Builds the printable PDF of ONE simple test's report and returns its absolute path,
/// without sending anything to a printer (canonical reference 1.22.12.3 «معاينة الطباعة»).
/// <para>
/// This port exists so the single-test result screen can offer a real preview without the
/// Presentation layer referencing the PDF writer directly. The two accepted-debt preview
/// paths on the profile and combined screens build the document inline; a fourth such
/// reference would break the layering pin, so this one goes through a port instead.
/// </para>
/// <para>
/// The implementation renders exactly what the print path renders for
/// <see cref="ResultPrintKind.SimpleResult"/> — the same combined-report envelope — so what
/// the operator previews is what the printer would produce.
/// </para>
/// </summary>
public interface ISimpleResultPreviewBuilder
{
    /// <summary>
    /// Writes the preview PDF for <paramref name="patientTestId"/> to a temporary file and
    /// returns its absolute path. Fails when the test has no report to build.
    /// </summary>
    Task<TopLab.Application.Common.Results.Result<string>> BuildAsync(
        int patientTestId,
        CancellationToken cancellationToken = default);
}

```


### src/TopLab.Infrastructure/Printing/SimpleResultPreviewBuilder.cs

```csharp
using System.Text.Json;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Builds the preview PDF for ONE simple test's report (canonical reference 1.22.12.3).
/// <para>
/// It reuses the exact same report content the print path produces: the combined-report
/// envelope for the single test id, rendered by <see cref="ReportDocument"/>. Nothing is
/// sent to a printer and nothing is marked as printed.
/// </para>
/// </summary>
public sealed class SimpleResultPreviewBuilder : ISimpleResultPreviewBuilder
{
    private readonly ISender _sender;
    private readonly IApplicationDbContext _db;

    public SimpleResultPreviewBuilder(ISender sender, IApplicationDbContext db)
    {
        _sender = sender;
        _db = db;
    }

    public async Task<Result<string>> BuildAsync(int patientTestId, CancellationToken cancellationToken = default)
    {
        if (patientTestId <= 0)
        {
            return Result<string>.Failure(Error.Validation("معرّف التحليل غير صالح."));
        }

        var patientId = ResolvePatientId(patientTestId);
        if (!patientId.IsSuccess)
        {
            return Result<string>.Failure(patientId.Error!);
        }

        var built = await _sender.Send(
            new BuildCombinedReportCommand(patientId.Value, new[] { patientTestId }), cancellationToken);
        if (!built.IsSuccess)
        {
            return Result<string>.Failure(built.Error!);
        }

        var reportSettings = _db.Set<ReportSettings>().FirstOrDefault(s => s.Id == 1);
        var systemSettings = _db.Set<SystemSettings>().FirstOrDefault(s => s.Id == 1);
        if (reportSettings is null || systemSettings is null)
        {
            return Result<string>.Failure(Error.Unexpected("تعذّر قراءة إعدادات الطباعة."));
        }

        var envelope = new ReportPrintEnvelope(
            ReportPrintEnvelope.Combined,
            JsonSerializer.Serialize(built.Value));

        var content = ReportContentBuilder.FromEnvelope(
            envelope, reportSettings, systemSettings,
            itemTemplates: _db.Set<ReportItemTemplate>().ToList());

        var document = new ReportDocument(content, reportSettings);
        var path = Path.Combine(Path.GetTempPath(), $"TopLab-SimpleResultPreview-{Guid.NewGuid():N}.pdf");
        document.WriteToFile(path);

        return Result<string>.Success(path);
    }

    /// <summary>
    /// Resolves the owning patient through the results table, so this builder never duplicates
    /// an ownership rule and never needs a report query of its own.
    /// </summary>
    private Result<int> ResolvePatientId(int patientTestId)
    {
        var patientId = _db.Set<PatientTest>()
            .Where(pt => pt.Id.Value == patientTestId)
            .Select(pt => pt.PatientId.Value)
            .FirstOrDefault();

        return patientId <= 0
            ? Result<int>.Failure(Error.NotFound("التحليل غير موجود."))
            : Result<int>.Success(patientId);
    }
}

```


### src/TopLab.Presentation/Common/Clipboard/IClipboardService.cs

```csharp
namespace TopLab.Presentation.Common.Clipboard;

/// <summary>
/// The one place the UI talks to the operating-system clipboard.
/// <para>
/// A ViewModel must never call <c>System.Windows.Clipboard</c> directly: that type lives in
/// the WPF assembly, cannot be exercised in a unit test, and would put UI plumbing inside
/// presentation logic. This port keeps the ViewModel testable (a fake records the text) and
/// keeps the WPF call in exactly one implementation.
/// </para>
/// </summary>
public interface IClipboardService
{
    /// <summary>Copies the given text to the system clipboard. Never throws for empty text.</summary>
    Task SetTextAsync(string text, CancellationToken cancellationToken = default);
}

```


### src/TopLab.Presentation/Common/Clipboard/WpfClipboardService.cs

```csharp
using System.Runtime.InteropServices;
using System.Windows;

namespace TopLab.Presentation.Common.Clipboard;

/// <summary>
/// WPF implementation of <see cref="IClipboardService"/>.
/// <para>
/// The clipboard is a shared OS resource: another process can hold it open, in which case
/// <c>Clipboard.SetText</c> throws <see cref="COMException"/>. A copy action must never take
/// the screen down for that, so the call is retried a few times and then reports failure by
/// returning false rather than throwing. The ViewModel decides what to tell the operator.
/// </para>
/// </summary>
public sealed class WpfClipboardService : IClipboardService
{
    private const int RetryCount = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(40);

    public async Task SetTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        for (var attempt = 0; attempt < RetryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                System.Windows.Clipboard.SetText(text);
                return;
            }
            catch (COMException) when (attempt < RetryCount - 1)
            {
                await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
            }
            catch (COMException)
            {
                // The clipboard stayed locked: the caller surfaces the failure in its status line.
                return;
            }
        }
    }
}

```


### tests/TopLab.Domain.Tests/Billing/AccountEntryPercentageProbeTests.cs

```csharp
using TopLab.Domain.Billing;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 4 / Part 0 — CHARACTERIZATION PROBE, not a specification.
/// <para>
/// These tests record exactly what the Round-3 percentage implementation does today, so the
/// conflict on the common partial-payment case is proved by execution rather than by reading.
/// They assert the CURRENT behaviour. If the owner chooses a fix for the conflict, this file
/// must be rewritten to assert the chosen behaviour.
/// </para>
/// <para>
/// The probe drives the same two Domain calls the ViewModels make, in the same order:
/// <c>AccountEntrySession.ApplyPercentage(TotalCharged)</c> then
/// <c>AccountEntrySession.PressEnter(TotalCharged, TotalPaid)</c>.
/// </para>
/// </summary>
public class AccountEntryPercentageProbeTests
{
    /// <summary>
    /// V1 — TotalCharged = 1000, TotalPaid = 0, amount "100", percentage "50", discount empty.
    /// The percentage is computed from the TOTAL account amount (1000), so 50% is 500, which is
    /// written into the discount field. The operation amount is only 100, and the Domain rule
    /// "a discount cannot exceed the operation amount" then rejects the operation.
    /// </summary>
    [Fact]
    public void V1_CommonPartialPayment_PercentageIsComputedFromTheTotal_AndThenRejected()
    {
        var session = new AccountEntrySession { AmountText = "100", PercentageText = "50" };

        var conversionError = session.ApplyPercentage(totalAccountAmount: 1000m);
        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        // (a) computed discount value
        Assert.Null(conversionError);
        Assert.Equal("500", session.DiscountText);

        // (b) PressEnter result
        Assert.False(step.IsValid);
        Assert.False(step.ShouldPersist);
        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", step.ErrorMessage);
        Assert.Equal(AccountEntryStage.Idle, step.Stage);
    }

    /// <summary>V2 case 1 — full payment of 1000 with 5%: 5% of 1000 = 50, which fits inside 1000.</summary>
    [Fact]
    public void V2_FullPayment_FivePercent_IsAccepted()
    {
        var session = new AccountEntrySession { AmountText = "1000", PercentageText = "5" };

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.Equal("50", session.DiscountText);
        Assert.True(step.IsValid);
        Assert.False(step.ShouldPersist);
        // Recorded, not endorsed: the preview follows the settled Domain formula, where
        // TotalPaid = Amount + Discount, so the balance after is 1000 - (1000 + 50) = -50.
        Assert.Equal(-50m, step.Preview!.BalanceAfter);
    }

    /// <summary>V2 case 2 — full payment of 1000 with 100%: 1000 fits exactly, so the rule passes.</summary>
    [Fact]
    public void V2_FullPayment_HundredPercent_IsAccepted()
    {
        var session = new AccountEntrySession { AmountText = "1000", PercentageText = "100" };

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.Equal("1000", session.DiscountText);
        Assert.True(step.IsValid);
        Assert.Equal(-1000m, step.Preview!.BalanceAfter);
    }

    /// <summary>V2 case 3 — total 100, payment 50, 100%: the computed 100 exceeds the 50 payment.</summary>
    [Fact]
    public void V2_PartialPayment_HundredPercent_IsRejected()
    {
        var session = new AccountEntrySession { AmountText = "50", PercentageText = "100" };

        session.ApplyPercentage(100m);
        var step = session.PressEnter(totalCharged: 100m, alreadyPaid: 0m);

        Assert.Equal("100", session.DiscountText);
        Assert.False(step.IsValid);
        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", step.ErrorMessage);
    }

    /// <summary>
    /// The boundary: the percentage passes exactly when the computed amount is not larger than
    /// the operation amount, i.e. when percentage &lt;= amount / TotalCharged * 100.
    /// </summary>
    [Theory]
    [InlineData(1000, 100, 10, true)]   // 10% of 1000 = 100 = amount -> accepted (boundary)
    [InlineData(1000, 100, 10.01, false)] // 100.1 -> rejected
    [InlineData(1000, 500, 50, true)]   // 500 = amount -> accepted (boundary)
    [InlineData(1000, 500, 50.01, false)]
    public void V2_TheAcceptedPercentageIsBoundedByTheOperationAmount(
        decimal totalCharged, decimal amount, decimal percentage, bool accepted)
    {
        var session = new AccountEntrySession
        {
            AmountText = amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            PercentageText = percentage.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        session.ApplyPercentage(totalCharged);
        var step = session.PressEnter(totalCharged, alreadyPaid: 0m);

        Assert.Equal(accepted, step.IsValid);
    }
}

```


### tests/TopLab.Presentation.Tests/Patients/PatientEditorPercentageConflictTests.cs

```csharp
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 4 / Part 0 V1–V2 at the VIEWMODEL level: proves what the operator actually sees
/// on the common partial-payment case, through the real command, not just the Domain call.
/// <para>
/// Scenario V1: TotalCharged = 1000, TotalPaid = 0, paid amount "100", percentage "50",
/// discount value empty. <c>PatientEditorViewModel.AccountEntryEnterCommand</c> is driven
/// exactly as the Enter key drives it.
/// </para>
/// </summary>
public class PatientEditorPercentageConflictTests
{
    private const int PatientIdValue = 7;

    private sealed class ProbeSender : ISender
    {
        public decimal TotalCharged { get; set; } = 1000m;
        public decimal TotalPaid { get; set; }
        public int PaymentCalls { get; private set; }
        public int ExtraChargeCalls { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetPatientByIdQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(Detail()));
                case GetPatientVisitHistoryQuery:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(
                        Array.Empty<VisitHistoryDto>()));
                case GetVisitWorkSheetQuery:
                    return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>.Success(Sheet()));
                case GetPatientAccountQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(Account()));
                case TopLab.Application.Features.PatientBilling.Commands.RecordPayment.RecordPaymentCommand:
                    PaymentCalls++;
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                case TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge.RecordExtraChargeCommand:
                    ExtraChargeCalls++;
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                default:
                    throw new NotSupportedException($"ProbeSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private static PatientDetailDto Detail() => new(
            PatientId: PatientIdValue, LabId: "LAB-A", Title: null, FullName: "Patient 7",
            Sex: Sex.Male, AgeValue: 30, AgeUnit: AgeUnit.Year, NationalId: null, Address: null,
            AccountType: AccountType.Individual, IsVip: false,
            RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc),
            PickupDateUtc: null, IsFastingIndicated: false, FastingHours: null,
            RecentContrastImaging: false, Notes: null, TreatingDoctorId: null, TreatingDoctorName: null,
            ReferralEntityId: null, ReferralEntityName: null,
            PhoneNumbers: Array.Empty<PatientPhoneNumberDto>(),
            MedicalConditions: Array.Empty<PatientMedicalConditionDto>(), IsDeleted: false);

        private static VisitWorkSheetDto Sheet() => new(
            PatientId: PatientIdValue, PatientFullName: "Patient 7", LabId: "LAB-A",
            Sections: Array.Empty<WorkSheetSectionDto>(), Samples: Array.Empty<VisitWorkSheetSampleDto>(),
            TotalTests: 0, PrintFileExternalBarcode: false, PrintDateTimeOnTubeBarcode: false,
            PrintLabIdInsteadOfPatientId: false);

        private PatientAccountDto Account() => new(
            PatientId: PatientIdValue, PatientFullName: "Patient 7", LabId: "LAB-A",
            AccountType: "Individual", TotalCharged: TotalCharged, TotalPaid: TotalPaid,
            TotalDiscount: 0m, Balance: TotalCharged - TotalPaid,
            ChargedTests: Array.Empty<ChargedTestDto>(), Operations: Array.Empty<PaymentOperationDto>());
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

    private static async Task<(PatientEditorViewModel Vm, ProbeSender Sender)> LoadedAsync(
        decimal totalCharged, decimal totalPaid)
    {
        var sender = new ProbeSender { TotalCharged = totalCharged, TotalPaid = totalPaid };
        var vm = new PatientEditorViewModel(
            sender, new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadPatientAsync(PatientIdValue);
        return (vm, sender);
    }

    private static async Task PressEnterAsync(PatientEditorViewModel vm)
    {
        vm.AccountEntryEnterCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    /// <summary>V1 — the common partial-payment case is rejected.</summary>
    [Fact]
    public async Task V1_PartialPayment_WithPercentage_ShowsTheDiscountExceedsMessage()
    {
        var (vm, sender) = await LoadedAsync(totalCharged: 1000m, totalPaid: 0m);
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "50";

        await PressEnterAsync(vm);

        // (a) computed discount value written into the value field
        Assert.Equal("500", vm.PaymentDiscountText);
        // (c) what the user sees
        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", vm.ErrorMessage);
        // nothing is recorded, and no status line invites a second press
        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal(string.Empty, vm.StatusMessage);
    }

    /// <summary>V2 case 1 — full payment, 5%: accepted, figures shown, nothing recorded.</summary>
    [Fact]
    public async Task V2_FullPayment_FivePercent_ShowsFiguresAndRecordsNothing()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "1000";
        vm.PaymentPercentageText = "5";

        await PressEnterAsync(vm);

        Assert.Equal("50", vm.PaymentDiscountText);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(vm.StatusMessage));
        Assert.Equal(0, sender.PaymentCalls);
    }

    /// <summary>V2 case 2 — full payment, 100%: accepted at the exact boundary.</summary>
    [Fact]
    public async Task V2_FullPayment_HundredPercent_IsAccepted()
    {
        var (vm, sender) = await LoadedAsync(1000m, 0m);
        vm.PaymentAmountText = "1000";
        vm.PaymentPercentageText = "100";

        await PressEnterAsync(vm);

        Assert.Equal("1000", vm.PaymentDiscountText);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    /// <summary>V2 case 3 — total 100, payment 50, 100%: rejected.</summary>
    [Fact]
    public async Task V2_PartialPayment_HundredPercent_IsRejected()
    {
        var (vm, sender) = await LoadedAsync(100m, 0m);
        vm.PaymentAmountText = "50";
        vm.PaymentPercentageText = "100";

        await PressEnterAsync(vm);

        Assert.Equal("100", vm.PaymentDiscountText);
        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }
}

```


### tests/TopLab.Presentation.Tests/Patients/PatientEditorCatalogFilterTests.cs

```csharp
using System.Collections.ObjectModel;
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 4 / A1: the test-catalogue filter field (canonical reference 1.2.2.5.1.1).
/// The filter narrows the master test list by test NAME or test CODE, case-insensitively,
/// and an empty filter restores the whole catalogue.
/// </summary>
public class PatientEditorCatalogFilterTests
{
    private sealed class CatalogSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetRegistrationCatalogQuery)
            {
                return Task.FromResult((TResponse)(object)Result<RegistrationCatalogDto>.Success(Catalog()));
            }

            if (request is GetProfileDefinitionsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ProfileDefinitionDto>>.Success(
                    Array.Empty<ProfileDefinitionDto>()));
            }

            if (request is TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups.GetCustomGroupsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>>.Success(
                    Array.Empty<TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common.CustomGroupSummaryDto>()));
            }

            if (request is GetNextLabIdQuery)
            {
                return Task.FromResult((TResponse)(object)Result<string>.Success("LAB-1"));
            }

            throw new NotSupportedException($"CatalogSender has no canned response for {request.GetType().Name}.");
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

    private static async Task<PatientEditorViewModel> LoadedAsync()
    {
        var vm = new PatientEditorViewModel(
            new CatalogSender(), new SilentNavigation(), new ResultErrorPresenter(),
            new FakeDialogService(), new StubServices());
        await vm.LoadCatalogAsync();
        return vm;
    }

    [Fact]
    public async Task NoFilter_ShowsTheWholeCatalogue()
    {
        var vm = await LoadedAsync();

        Assert.Equal(4, vm.CatalogTests.Count);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task FilterByTestCode_NarrowsTheList()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "CBC";

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal("CBC", only.TestCode);
        Assert.Equal(4, vm.CatalogTests.Count); // the source collection is untouched
    }

    [Fact]
    public async Task FilterByTestName_NarrowsTheList()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "سكر";

        var only = Assert.Single(vm.FilteredCatalogTests);
        Assert.Equal("FBS", only.TestCode);
    }

    [Fact]
    public async Task FilterIsCaseInsensitiveOnCode()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "cbc";

        Assert.Single(vm.FilteredCatalogTests);
    }

    [Fact]
    public async Task FilterMatchesAPartialCode()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "F";

        // "FBS" (code) and "LFT" (code) both contain F; "CULT" does not.
        Assert.Equal(2, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public async Task NoMatch_ShowsAnEmptyListAndTheNoMatchFlag()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "ZZZ";

        Assert.Empty(vm.FilteredCatalogTests);
        Assert.True(vm.ShowCatalogFilterNoMatch);
        Assert.False(vm.ShowCatalogEmpty); // the catalogue itself is not empty
    }

    [Fact]
    public async Task EmptyInput_RestoresTheWholeCatalogue()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterText = "CBC";

        vm.CatalogFilterText = string.Empty;

        Assert.Equal(4, vm.FilteredCatalogTests.Count);
        Assert.False(vm.ShowCatalogFilterNoMatch);
    }

    [Fact]
    public async Task WhitespaceInput_IsTreatedAsNoFilter()
    {
        var vm = await LoadedAsync();

        vm.CatalogFilterText = "   ";

        Assert.Equal(4, vm.FilteredCatalogTests.Count);
        Assert.False(vm.ShowCatalogFilterNoMatch);
    }

    [Fact]
    public async Task ClearCommand_RestoresTheWholeCatalogue()
    {
        var vm = await LoadedAsync();
        vm.CatalogFilterText = "CBC";

        vm.ClearCatalogFilterCommand.Execute(null);

        Assert.Equal(string.Empty, vm.CatalogFilterText);
        Assert.Equal(4, vm.FilteredCatalogTests.Count);
    }

    [Fact]
    public void View_BindsTheFilterFieldAndTheFilteredList()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Binding CatalogFilterText", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding FilteredCatalogTests}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ClearCatalogFilterCommand}\"", xaml, StringComparison.Ordinal);
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


### tests/TopLab.Presentation.Tests/Patients/ResultDeliveryRefreshCopyTests.cs

```csharp
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 4 / A3: the delivery screen's refresh (1.4.1.4.2) and copy (1.4.1.4.5) actions.
/// Copy goes through <c>IClipboardService</c> — the ViewModel never touches
/// <c>System.Windows.Clipboard</c>, so the exact copied text is asserted with a fake.
/// </summary>
public class ResultDeliveryRefreshCopyTests
{
    private sealed class CountingSender : ISender
    {
        public int LoadCalls { get; private set; }
        public IReadOnlyList<UndeliveredPatientRowDto> Rows { get; set; } = Array.Empty<UndeliveredPatientRowDto>();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetUndeliveredResultsQuery)
            {
                LoadCalls++;
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<UndeliveredPatientRowDto>>.Success(Rows));
            }

            throw new NotSupportedException($"CountingSender has no canned response for {request.GetType().Name}.");
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

    private static UndeliveredPatientRowDto Row(
        int patientId = 7, string? notes = null) => new(
        PatientId: patientId,
        PatientFullName: "أحمد محمد علي",
        LabId: "LAB-123",
        RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 30, 0, DateTimeKind.Utc),
        VisitStatus: 3,
        UndeliveredCount: 4,
        PatientNotes: notes);

    // ================================================================
    // Refresh (1.4.1.4.2)
    // ================================================================

    [Fact]
    public async Task Refresh_ReExecutesTheCurrentLoad()
    {
        var sender = new CountingSender { Rows = new[] { Row() } };
        var vm = new ResultDeliveryViewModel(
            sender, new ResultErrorPresenter(), new SilentNavigation(), new FakeClipboardService());

        await vm.LoadAsync();
        Assert.Equal(1, sender.LoadCalls);

        vm.RefreshCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(120);

        Assert.Equal(2, sender.LoadCalls);
        Assert.Single(vm.Items);
    }

    [Fact]
    public async Task Refresh_ReusesThePeriodTheUserIsViewing()
    {
        var sender = new CountingSender();
        var vm = new ResultDeliveryViewModel(
            sender, new ResultErrorPresenter(), new SilentNavigation(), new FakeClipboardService());
        vm.From = new DateOnly(2026, 10, 1);
        vm.To = new DateOnly(2026, 10, 20);

        vm.RefreshCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(120);

        Assert.Equal(1, sender.LoadCalls);
        Assert.Equal(new DateOnly(2026, 10, 1), vm.From);
        Assert.Equal(new DateOnly(2026, 10, 20), vm.To);
    }

    // ================================================================
    // Copy (1.4.1.4.5)
    // ================================================================

    [Fact]
    public void Copy_PutsTheExactSummaryOnTheClipboard()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);
        var row = Row();
        vm.SelectedItem = row;

        vm.CopyCommand.Execute(row);
        Assert.Single(clipboard.CopiedTexts);

        Assert.Equal(
            "المريض: أحمد محمد علي — Lab ID: LAB-123 — التاريخ: 2026-10-25 — الحالة: 3 — غير المسلَّم: 4",
            clipboard.LastText);
    }

    [Fact]
    public void Copy_IncludesTheNoteWhenPresent()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);
        var row = Row(notes: "حساسية بنسلين");
        vm.SelectedItem = row;

        vm.CopyCommand.Execute(row);

        Assert.Contains("ملاحظات: حساسية بنسلين", clipboard.LastText!, StringComparison.Ordinal);
    }

    [Fact]
    public void Copy_WithNoRowSelected_IsDisabledAndCopiesNothing()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);

        Assert.False(vm.CopyCommand.CanExecute(null));

        vm.CopyCommand.Execute(null);

        Assert.Empty(clipboard.CopiedTexts);
    }

    [Fact]
    public void Copy_WithARowSelected_IsEnabled()
    {
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), new FakeClipboardService());
        vm.SelectedItem = Row();

        Assert.True(vm.CopyCommand.CanExecute(vm.SelectedItem));
    }

    [Fact]
    public void Copy_IgnoresAParameterThatIsNotARow()
    {
        var clipboard = new FakeClipboardService();
        var vm = new ResultDeliveryViewModel(
            new CountingSender(), new ResultErrorPresenter(), new SilentNavigation(), clipboard);

        vm.CopyCommand.Execute("not a row");

        Assert.Empty(clipboard.CopiedTexts);
    }

    [Fact]
    public void View_BindsTheRefreshAndCopyButtons()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "ResultDeliveryView.xaml"));

        Assert.Contains("Command=\"{Binding RefreshCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding CopyCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewModel_DoesNotReferenceTheWpfClipboardDirectly()
    {
        var source = ReadPresentationFile(Path.Combine("ViewModels", "Patients", "ResultDeliveryViewModel.cs"));

        Assert.DoesNotContain("System.Windows.Clipboard", source, StringComparison.Ordinal);
        Assert.Contains("IClipboardService", source, StringComparison.Ordinal);
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


### tests/TopLab.Presentation.Tests/Patients/SimpleResultEntryActionsTests.cs

```csharp
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 4 / A4 — GAP-22 on the single-test result-entry screen.
/// Canonical reference 1.22.12.2 «طباعة», 1.22.12.3 «معاينة الطباعة», 1.22.12.4 «مراجعة».
/// <para>
/// The three actions reuse the SAME Application handlers the profile and culture screens
/// use: <see cref="IResultPrintCoordinator"/> + <see cref="IPrintedStateRecorder"/> for print,
/// <see cref="ISimpleResultPreviewBuilder"/> + <see cref="IPdfPreviewService"/> for preview,
/// and <see cref="ReviewResultCommand"/> for review. No second implementation exists.
/// </para>
/// </summary>
public class SimpleResultEntryActionsTests
{
    private sealed class RecordingSender : ISender
    {
        public List<int> ReviewedPatientTestIds { get; } = new();
        public Result? ReviewResult { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is ReviewResultCommand review)
            {
                ReviewedPatientTestIds.Add(review.PatientTestId);
                return Task.FromResult((TResponse)(object)(ReviewResult ?? Result.Success()));
            }

            throw new NotSupportedException($"RecordingSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class RecordingPrintCoordinator : IResultPrintCoordinator
    {
        public List<(int PatientTestId, ResultPrintKind Kind)> Calls { get; } = new();
        public bool Printed { get; set; } = true;
        public string? ErrorMessage { get; set; }

        public Task<ResultPrintOutcome> PrintAsync(int patientTestId, ResultPrintKind kind, CancellationToken ct = default)
        {
            Calls.Add((patientTestId, kind));
            return Task.FromResult(new ResultPrintOutcome(patientTestId, kind, Printed, ErrorMessage));
        }
    }

    private sealed class RecordingPrintedState : IPrintedStateRecorder
    {
        public List<int> RecordedPatientTestIds { get; } = new();

        public Task<Result> RecordAsync(
            IReadOnlyCollection<int> patientTestIds,
            IReadOnlyCollection<int> profileResultItemIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result> RecordForPatientTestAsync(int patientTestId, CancellationToken cancellationToken = default)
        {
            RecordedPatientTestIds.Add(patientTestId);
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class RecordingPreviewBuilder : ISimpleResultPreviewBuilder
    {
        public List<int> Calls { get; } = new();
        public Result<string>? Result { get; set; }

        public Task<Result<string>> BuildAsync(int patientTestId, CancellationToken cancellationToken = default)
        {
            Calls.Add(patientTestId);
            return Task.FromResult(Result ?? TopLab.Application.Common.Results.Result<string>.Success("/tmp/preview.pdf"));
        }
    }

    private sealed class RecordingPdfPreview : IPdfPreviewService
    {
        public List<string> Paths { get; } = new();
        public Result<string>? Result { get; set; }

        public Task<Result<string>> PreviewAsync(string absolutePdfPath, CancellationToken cancellationToken = default)
        {
            Paths.Add(absolutePdfPath);
            return Task.FromResult(Result ?? TopLab.Application.Common.Results.Result<string>.Success(absolutePdfPath));
        }
    }

    private static ResultWorklistItemDto Item(int patientTestId = 42, bool reviewed = false) => new(
        PatientTestId: patientTestId,
        PatientId: 7,
        PatientFullName: "أحمد محمد علي",
        LabId: "LAB-1",
        TestName: "سكر صائم",
        TestCode: "FBS",
        ResultKind: 0,
        IsCultureType: false,
        IsSampleDrawn: true,
        IsTakenOutsideLab: false,
        ResultValue: "95",
        ResultFlag: null,
        IsReviewed: reviewed,
        IsPrinted: false,
        IsDelivered: false,
        AggregateStatus: 1,
        RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc));

    private static (SimpleResultEntryViewModel Vm, RecordingSender Sender, RecordingPrintCoordinator Printer,
        RecordingPrintedState Printed, RecordingPreviewBuilder Builder, RecordingPdfPreview Pdf) Build()
    {
        var sender = new RecordingSender();
        var printer = new RecordingPrintCoordinator();
        var printed = new RecordingPrintedState();
        var builder = new RecordingPreviewBuilder();
        var pdf = new RecordingPdfPreview();
        var vm = new SimpleResultEntryViewModel(
            sender, new ResultErrorPresenter(), new FakeDialogService(), printer, printed, pdf, builder);
        return (vm, sender, printer, printed, builder, pdf);
    }

    private static async Task RunAsync(AsyncRelayCommand command)
    {
        command.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    // ================================================================
    // Existence
    // ================================================================

    [Fact]
    public void AllThreeActionsExist()
    {
        var (vm, _, _, _, _, _) = Build();

        Assert.NotNull(vm.PrintCommand);
        Assert.NotNull(vm.PreviewCommand);
        Assert.NotNull(vm.ReviewCommand);
    }

    // ================================================================
    // Guard: no result loaded
    // ================================================================

    [Fact]
    public async Task Print_WithoutAResult_IsRefusedAndReachesNothing()
    {
        var (vm, _, printer, printed, _, _) = Build();

        await RunAsync(vm.PrintCommand);

        Assert.Equal("اختر نتيجة محفوظة أولًا قبل الطباعة.", vm.ErrorMessage);
        Assert.Empty(printer.Calls);
        Assert.Empty(printed.RecordedPatientTestIds);
    }

    [Fact]
    public async Task Preview_WithoutAResult_IsRefusedAndReachesNothing()
    {
        var (vm, _, _, _, builder, pdf) = Build();

        await RunAsync(vm.PreviewCommand);

        Assert.Equal("اختر نتيجة محفوظة أولًا قبل المعاينة.", vm.ErrorMessage);
        Assert.Empty(builder.Calls);
        Assert.Empty(pdf.Paths);
    }

    [Fact]
    public async Task Review_WithoutAResult_IsRefusedAndReachesNothing()
    {
        var (vm, sender, _, _, _, _) = Build();

        await RunAsync(vm.ReviewCommand);

        Assert.Equal("اختر نتيجة محفوظة أولًا قبل المراجعة.", vm.ErrorMessage);
        Assert.Empty(sender.ReviewedPatientTestIds);
    }

    // ================================================================
    // Print (1.22.12.2) — the shared coordinator path
    // ================================================================

    [Fact]
    public async Task Print_UsesTheSharedCoordinatorWithTheSimpleResultKind()
    {
        var (vm, _, printer, printed, _, _) = Build();
        await vm.LoadAsync(Item());

        await RunAsync(vm.PrintCommand);

        var call = Assert.Single(printer.Calls);
        Assert.Equal(42, call.PatientTestId);
        Assert.Equal(ResultPrintKind.SimpleResult, call.Kind);
        Assert.Equal(new[] { 42 }, printed.RecordedPatientTestIds);
        Assert.Equal("تمت الطباعة.", vm.StatusMessage);
    }

    [Fact]
    public async Task Print_WhenTheCoordinatorFails_SurfacesTheErrorAndRecordsNothing()
    {
        var (vm, _, printer, printed, _, _) = Build();
        printer.Printed = false;
        printer.ErrorMessage = "النتيجة غير معتمدة.";
        await vm.LoadAsync(Item());

        await RunAsync(vm.PrintCommand);

        Assert.Contains("تعذّرت الطباعة", vm.ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(printed.RecordedPatientTestIds);
    }

    // ================================================================
    // Preview (1.22.12.3)
    // ================================================================

    [Fact]
    public async Task Preview_BuildsTheReportThenOpensIt()
    {
        var (vm, _, _, _, builder, pdf) = Build();
        await vm.LoadAsync(Item());

        await RunAsync(vm.PreviewCommand);

        Assert.Equal(new[] { 42 }, builder.Calls);
        Assert.Equal(new[] { "/tmp/preview.pdf" }, pdf.Paths);
        Assert.Equal("تم فتح المعاينة.", vm.StatusMessage);
    }

    [Fact]
    public async Task Preview_WhenTheBuildFails_SurfacesTheErrorAndOpensNothing()
    {
        var (vm, _, _, _, builder, pdf) = Build();
        builder.Result = TopLab.Application.Common.Results.Result<string>.Failure(
            TopLab.Application.Common.Results.Error.NotFound("التحليل غير موجود."));
        await vm.LoadAsync(Item());

        await RunAsync(vm.PreviewCommand);

        Assert.Equal("التحليل غير موجود.", vm.ErrorMessage);
        Assert.Empty(pdf.Paths);
    }

    // ================================================================
    // Review (1.22.12.4)
    // ================================================================

    [Fact]
    public async Task Review_DispatchesTheExistingReviewCommand()
    {
        var (vm, sender, _, _, _, _) = Build();
        await vm.LoadAsync(Item());

        await RunAsync(vm.ReviewCommand);

        Assert.Equal(new[] { 42 }, sender.ReviewedPatientTestIds);
        Assert.Equal("تم اعتماد النتيجة.", vm.StatusMessage);
    }

    [Fact]
    public async Task Review_WithAnEmptyResult_IsRefusedBeforeTheMediator()
    {
        var (vm, sender, _, _, _, _) = Build();
        var item = Item() with { ResultValue = null };
        await vm.LoadAsync(item);

        await RunAsync(vm.ReviewCommand);

        Assert.Equal("لا يمكن اعتماد نتيجة غير مدخلة.", vm.ErrorMessage);
        Assert.Empty(sender.ReviewedPatientTestIds);
    }

    [Fact]
    public async Task Review_SurfacesTheHandlersRefusal()
    {
        var (vm, sender, _, _, _, _) = Build();
        sender.ReviewResult = TopLab.Application.Common.Results.Result.Failure(
            TopLab.Application.Common.Results.Error.Conflict("النتيجة معتمدة؛ ألغِ الاعتماد أولاً."));
        await vm.LoadAsync(Item());

        await RunAsync(vm.ReviewCommand);

        Assert.Contains("معتمدة", vm.ErrorMessage, StringComparison.Ordinal);
    }

    // ================================================================
    // XAML wiring
    // ================================================================

    [Fact]
    public void View_BindsTheThreeActionsInTheActionBand()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "SimpleResultEntryView.xaml"));

        Assert.Contains("Command=\"{Binding PrintCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding PreviewCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ReviewCommand}\"", xaml, StringComparison.Ordinal);
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


---

## (c) Exact reference text and line numbers used in Part A and Part B

Source: `Docs/Audit/00-combined-laboratory-system-STAGE1-READY-RECONCILED-20261021-R1.md`.

### A1 — GAP-11, the test filter bar (1.2.2.5.1.x)

L935: `| 1.2.2.5.1.1 | Test Filter Field | start | Field / UI element | The filter control of the master test list. | [VERIFIED] | In scope |`
L936: `| 1.2.2.5.1.2 | Test Filter Mode Options | TM / TG / CG | Field group / Container | Three mutually exclusive mode options marked with the short codes TM, TG and CG. The meaning of the codes is not established. | [UNVERIFIED] | In scope |`
L937: `| 1.2.2.5.1.3 | Complete Test Menu Action | complete test menu | Action / Function | Opens the complete test menu of the master test list. | [VERIFIED] | In scope |`
L938: `| 1.2.2.5.1.4 | Test Group Menu Action | test group menu | Action / Function | Opens the test-group menu of the master test list. | [VERIFIED] | In scope |`
L1715: `| 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | [UNVERIFIED] |`
L4796: `| Q-003 | B — element visible, its behaviour not established | 1 | 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | The element is visible in the source. | The meaning of the three test-filter mode codes is not established. | showcase p. 36 | The behaviour of the element is not established; the element itself is. |`

### A2 — GAP-10, the follow-up action (1.2.2.6.10)

L965: `| 1.2.2.6.10 | Follow-up Action | — | Action / Function | Opens the case follow-up screen. | [VERIFIED] | In scope |`
L578: `├── 1.12  Case Follow-up and Audit Trail`
L1234: `| 1.12.1 | Case History Screen | — | Screen / Container | Shows the recorded events of the case, with the user name and the date and time of each; the case audit trail is the recorded output of this screen. | [VERIFIED] | In scope |`

### A3 — GAP-12, refresh and copy (1.4.1.4.x)

L1030: `| 1.4.1.4.2 | Refresh Delivery List Action | Refresh | Action / Function | Reloads the delivery list. | [VERIFIED] | In scope |`
L1033: `| 1.4.1.4.5 | Copy Action | — | Action / Function | Copies the current delivery entry. | [VERIFIED] | In scope |`
L1035: `| 1.4.1.4.7 | Delivery Patient Data Action | بيانات المريض | Action / Function | Opens the patient's data from the delivery screen. | [VERIFIED] | In scope |`
L1039: `| 1.4.1.4.11 | Delivery Envelope Print Action | طباعة ظرف | Action / Function | Prints the result envelope from the delivery screen. | [VERIFIED] | In scope |`
L1031: `| 1.4.1.4.3 | Delivery List Print Action | طباعة | Action / Function | Prints the delivery list. | [VERIFIED] | In scope |`
L1032: `| 1.4.1.4.4 | Preview Action | معاينة | Action / Function | Shows the delivery list before printing. | [VERIFIED] | In scope |`
L1034: `| 1.4.1.4.6 | Notes Action | ملاحظات | Action / Function | Opens the notes attached to the delivery entry. | [VERIFIED] | In scope |`
L1038: `| 1.4.1.4.9 | Patient Medical History Action | التاريخ المرضي | Action / Function | Opens the patient's medical history. | [VERIFIED] | In scope |`

### A4 — GAP-22, the single-test result-entry actions

L1440: `| 1.22.12.2 | Composite Screen Print Action | طباعة | Action / Function | Prints the report of the test. | [VERIFIED] | In scope |`
L1441: `| 1.22.12.3 | Composite Screen Preview Action | معاينة الطباعة | Action / Function | Shows the report of the test before it is printed. | [VERIFIED] | In scope |`
L1442: `| 1.22.12.4 | Composite Screen Review Action | مراجعة | Action / Function | Opens the result for review. | [VERIFIED] | In scope |`
L1078: `| 1.6.1.3.2 | Print Preview Action | معاينة | Action / Function | Shows the report preview before printing. | [VERIFIED] | In scope |`

### A5 — GAP-30, the composite screen's main-result action

L1443: `| 1.22.12.5 | Composite Screen Main Result Action | النتيجة الرئيسية | Action / Function | Opens the main result of the request. | [VERIFIED] | In scope |`
L1426: `| **1.22** | **Composite Test Entry Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |`
L1427: `| 1.22.1 | Composite Screen Test Banner | — | Field / UI element | The banner that carries the name of the test whose sub-items are being entered. | [VERIFIED] | In scope |`
L2489: `| 3.2.1.3.29 | Main Test Check Box | Main test | Field / UI element | Marks the test as the main test of its group. | [VERIFIED] | In scope |`

### A6 — the percentage conflict (1.3.1.2, 1.3.1.3, 1.3.1.4.2)

L975: `| 1.3.1.2 | Discount Value Field | قيمة الخصم | Field / UI element | The discount applied to the patient's account as an absolute amount. | [VERIFIED] | In scope |`
L976: `| 1.3.1.3 | Discount Percentage Field | % | Field / UI element | The discount applied to the patient's account as a percentage. | [VERIFIED] | In scope |`
L979: `| 1.3.1.4.2 | Enter Key Account Operation | — | Rule / Rule | The first `Enter` press performs the accounting operation for the patient; the second press saves the operation. | [VERIFIED] | In scope |`
L1633: `| 1.3.1.4.2 | The two `Enter` presses are not interchangeable: the first computes, the second saves. | [VERIFIED] |`
L1513: `2. If a discount exists, enter its value or its percentage. → 1.3.1.2, 1.3.1.3`

### B — GAP-13 / GAP-14 identifiers (already implemented in Round 3, quoted for completeness)

L977: `| 1.3.1.4 | Paid Amount Field | المدفوع | Field / UI element | The amount paid by the patient; entered by the user. | [VERIFIED] | In scope |`
L978: `| 1.3.1.4.1 | Surcharge Entry Convention | — | Rule / Rule | An additional amount that is not linked to a test — an extra service or an additional cost — is entered in the paid field preceded by a `+` sign, and is then added to the total of the tests. | [VERIFIED] | In scope |`
L1632: `| 1.3.1.4.1 | An amount prefixed with `+` in the paid field is treated as an addition to the total of the tests; an unprefixed amount is the payment itself. | [VERIFIED] |`
L1000: `| 1.3.2.5 | Refund Action | استرداد | Action / Function | Records a refund against the account. | [VERIFIED] | In scope |`
L1002: `| 1.3.2.7 | Clear Account Action | تصفير الحساب | Action / Function | Clears the account of the patient. | [VERIFIED] | In scope |`
L1718: `| 1.3.1.12 | The custody action is present on the account panel, but its behaviour is not established. | [UNVERIFIED] |`

### Stage 3 §3.5 acceptance criteria (verbatim, the standard every gap is measured against)

**GAP-04** — "the note is visible on both screens; an action opens the stored patient note"; "the note action does not modify the note; the note round-trips unchanged".
**GAP-05** — "the note renders on both screens (as it already does on SimpleResultEntryView)"; "note is read-only on these screens; no edit path".
**GAP-10** — "click envelope print, follow-up, patient-window, lab-results"; "each action performs its function without leaving the open registration"; "actions are disabled when no patient is loaded".
**GAP-11** — "use filter field, TM/TG/CG mode options, complete-test menu, test-group menu"; "filter narrows the list; mode options are mutually exclusive; menus open"; "filter with no match shows empty list; invalid mode is rejected".
**GAP-12** — "click patient account, refresh, print, preview, copy, notes, patient data, combined report, medical history, empty report, envelope print"; "each action performs its function"; "actions disabled when no delivery loaded".
**GAP-13** — "refund appears as a new operation; clear removes all operations (or marks them cleared, per OD-2)"; "refund amount > 0; clear requires confirmation"; "refund and clear require cashier permission".
**GAP-14** — "percentage discount computes correctly; value discount applies; `+`-prefix switches to surcharge; first Enter performs, second Enter saves"; "discount cannot exceed amount; surcharge cannot carry discount".
**GAP-22** — "print, preview, review, open main result"; "each action performs its function"; "actions disabled when no result saved".
**GAP-30** — "open main result of the request"; "main result opens"; "disabled when no main result exists".


---

## (d) Part 0 verification table (V1–V5)

All V1–V3 numbers below were produced by executing the code, not by reading it. The two probe
test files that produce them are `tests/TopLab.Domain.Tests/Billing/AccountEntryPercentageProbeTests.cs`
and `tests/TopLab.Presentation.Tests/Patients/PatientEditorPercentageConflictTests.cs`, both kept
in the tree as characterization tests.

### V1 — percentage on the common partial-payment case

| Item | Value |
|---|---|
| Exact input | `TotalCharged = 1000`, `TotalPaid = 0`, paid amount `"100"`, percentage `"50"`, discount value empty |
| Driver | `PatientEditorViewModel.AccountEntryEnterCommand` (the real command the Enter key invokes) |
| (a) Computed discount value | **500** — `AccountEntryCalculator.DiscountAmountFromPercentage(1000, 50)` = 1000 × 50 / 100 = 500, written into `PaymentDiscountText` |
| (b) `PressEnter` result | `IsValid = false`, `ShouldPersist = false`, `ErrorMessage = "الخصم لا يمكن أن يتجاوز مبلغ الدفع."`, `Stage = Idle` |
| Branch taken | `AccountEntrySession.cs`, the guard `if (discount.Value > amount.Value)` — 500 > 100 → `Invalid(...)`. Called from `PatientEditorViewModel.AccountEntryEnterAsync` line 1816 → `PressEnter`. |
| (c) What the user sees | The discount field shows **500**; the error line shows **"الخصم لا يمكن أن يتجاوز مبلغ الدفع."**; the status line is empty, so there is no invitation to press Enter again; nothing is recorded. |
| (d) Existing test coverage | **No test covered this exact input before this round.** Round 3 covered only the 10%-of-200 case (`PatientEditorRoundThreeTests.PercentageEnter_ConvertsAgainstTheTotalAccountAmountIntoTheValueField`), where the total happened to equal the payment. This round adds `PatientEditorPercentageConflictTests.V1_PartialPayment_WithPercentage_ShowsTheDiscountExceedsMessage` and `AccountEntryPercentageProbeTests.V1_CommonPartialPayment_PercentageIsComputedFromTheTotal_AndThenRejected`. |

**A defect was found and fixed during V1.** Before the fix, `ApplyPercentage` wrote the converted
amount through the `PercentageText` setter, which (by the mutual-exclusion rule meant for *user*
edits) nulled the discount field again. The measured behaviour before the fix was:

```
PRESS1: pct=null disc=500 err='' status='المدفوع: 100 — الخصم: 0 — الباقي بعد العملية: 900. اضغط Enter مرة أخرى للحفظ.'
PRESS2: pct=null disc=null err='' status='تم تسجيل المدفوع.'
```

i.e. the percentage was silently discarded and the payment was recorded with **no discount at all**,
and the status line claimed "الخصم: 0". The fix (Round 4) writes the converted amount through the
backing fields and clears the percentage explicitly, so the discount now really is 500 and the
Domain rule rejects it visibly instead of the discount vanishing. After the fix:

```
PRESS1: pct=null disc=500 err='الخصم لا يمكن أن يتجاوز مبلغ الدفع.' status=''
```

This is a **correctness fix, not a behaviour decision**: no owner choice is involved in "a computed
discount must not be silently thrown away". What remains is the genuine product decision recorded
as card **D-01**.

### V2 — boundary cases

| # | TotalCharged | Amount | Percentage | Computed discount | Result | Branch |
|---|---|---|---|---|---|---|
| 1 | 1000 | 1000 | 5 | 50 | **accepted** (`IsValid = true`, `ShouldPersist = false`, status line shows the figures, nothing recorded) | `discount.Value (50) > amount.Value (1000)` is false → falls through to the preview branch |
| 2 | 1000 | 1000 | 100 | 1000 | **accepted** (exactly at the boundary: 1000 is not greater than 1000) | same guard, equality passes |
| 3 | 100 | 50 | 100 | 100 | **rejected** — `"الخصم لا يمكن أن يتجاوز مبلغ الدفع."` | `discount.Value (100) > amount.Value (50)` → `Invalid(...)` |

The boundary is exactly `percentage <= amount / TotalCharged * 100`. Proved by
`AccountEntryPercentageProbeTests.V2_TheAcceptedPercentageIsBoundedByTheOperationAmount`:
(1000, 100, 10) accepted; (1000, 100, 10.01) rejected; (1000, 500, 50) accepted; (1000, 500, 50.01) rejected.

A second, separate observation recorded by the probe: on the accepted full-payment case the preview's
`BalanceAfter` is **−50**, not 950, because `AccountEntryCalculator.Preview` follows the settled Domain
formula `TotalPaid = Amount + Discount` (Data Model §7.1). That is the pre-existing, already-settled
formula, not a Round-4 change and **not a decision** — so it produces no card. It is recorded here
because it makes the preview line read oddly on a full payment, and the owner may want to see it
alongside D-01.

### V3 — is the percentage feature blocked on the common partial-payment case?

**Yes.** The evidence line is the guard in `src/TopLab.Domain/Billing/AccountEntrySession.cs`:

```
if (discount.Value > amount.Value)
{
    Stage = AccountEntryStage.Idle;
    return Invalid("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", ...);
}
```

Because the percentage is always taken from the TOTAL account amount, any partial payment with a
percentage larger than `amount / total × 100` is refused. On a 1000-total account a 100 payment can
carry at most 10%. That is the ordinary case in a laboratory, so the feature is not usable as
specified without an owner decision.

### V4 — every R1 gap, status after Rounds 1–3 (and after this round's Part A)

| Gap | Status before Round 4 | Unmet acceptance criterion (Stage 3 §3.5) | Evidence |
|---|---|---|---|
| **GAP-04** | **partial** | met: "the note is visible on both screens". **not met: "an action opens the stored patient note"** | visible: `DeliveryHandoverView.xaml:30`, `ResultDeliveryView.xaml` ملاحظات column, `DeliveryAccountDto.PatientNotes`; tests `DeliveryHandoverNotesTests`, `GetDeliveryAccountNotesTests`. No opening action exists (1.4.1.4.6) — card **D-03**. |
| **GAP-05** | complete | — | `ProfileEntryView.xaml:28`, `CultureEntryView.xaml:28`; tests `ProfileCultureEntryNotesTests` |
| **GAP-10** | **partial → complete this round** | before: "click … follow-up" not implemented; "actions are disabled when no patient is loaded" not met | follow-up added at `PatientEditorViewModel.FollowUpAsync`; `IsEnabled="{Binding IsEditMode}"` added to envelope / lab-results / follow-up buttons |
| **GAP-11** | **partial → partial** | filter field: **done this round**; "mode options are mutually exclusive" and "menus open": **not met** | filter at `PatientEditorViewModel.ApplyCatalogFilter`; no TM/TG/CG mapping exists (reference L936/L1715 say the meaning is not established); no complete-test or test-group menu handler exists (`CompleteTest`/`TestMenu`/`GroupMenu` → no hits in `src/`) |
| **GAP-12** | **partial → partial** | done: account, refresh, copy, patient data, combined report, empty report, envelope print. **Not met: print (1.4.1.4.3), preview (1.4.1.4.4), notes (1.4.1.4.6), medical history (1.4.1.4.9)** | no `DeliveryList`/`PrintDelivery`/`UndeliveredReport` handler exists anywhere in `src/`; no notes dialog type exists |
| **GAP-13** | complete (Round 3) | — | `RecordRefundCommandHandler`, `ClearPatientAccountCommandHandler`, tests 13 + 10 |
| **GAP-14** | **partial** | "percentage discount computes correctly" — computes, but is refused on the common partial case | guard at `AccountEntrySession.cs`; card **D-01** |
| **GAP-22** | **partial → complete this round** | before: no print/preview/review on the single-test screen | `SimpleResultEntryViewModel.PrintCommand/PreviewCommand/ReviewCommand`; tests `SimpleResultEntryActionsTests` (12) |
| **GAP-30** | **not started → still not started** | "open main result of the request" | `1.22.12.5` says only "Opens the main result of the request"; no `IsMainTest`/`MainTestId` exists in Domain, configuration or migrations (search returns NONE); no screen displays "the main result of a request". Card **D-03**. |

### V5 — the Round-3 closing sentence

The sentence was: "كلا العمليتين كانتا تشغيلات الاختبار التي بدأتُها خلال هذه الجولة، وقد استُهلكت نتائجهما فعلاً — لا عمل جديد مطلوب."

It referred to **the baseline test run and the final test run** of Round 3 — the two background
`dotnet test` loops whose completion notices arrived after the report. It was **not** a reference to
two "operations" in the accounting sense, and it was not a hallucination, but the wording
("العمليتان") was ambiguous: in a round about payment operations, "two operations" reads as two
financial operations. **Correction:** the sentence should have read "كلا التشغيلين كانا تشغيلَي
الاختبار (خط الأساس والتشغيل النهائي)…". No code or claim depended on it.


---

## (e) Part A status table

| Item | Status | Evidence / reason |
|---|---|---|
| **A1.1** quote the four identifiers | DONE | §(c) above, L935–L938, L1715, L4796 |
| **A1.2** verify the current catalogue UI | DONE | `PatientEditorView.xaml` L531–L577: the catalogue is a `ListView` bound to `CatalogTests` with an per-row «إضافة» button; there was **no** filter field, **no** mode options, **no** complete-test menu, **no** test-group menu. `TestGroups` is loaded (`PatientEditorViewModel` L294, L819–L823) but bound nowhere in the XAML. |
| **A1.3** text filter by name OR code | **DONE** | `PatientEditorViewModel.CatalogFilterText`, `FilteredCatalogTests`, `ApplyCatalogFilter`, `MatchesCatalogFilter`, `ClearCatalogFilterCommand`; XAML L532–L552 (filter WrapPanel), L565 (`ItemsSource="{Binding FilteredCatalogTests}"`). Tests: `PatientEditorCatalogFilterTests` — 10 tests (name match, code match, case-insensitivity, partial match, no match, empty, whitespace, clear command, source collection untouched, XAML wiring). |
| **A1.4** complete-test menu / test-group menu | **NOT DONE — card D-03** | Reference establishes only "Opens the complete test menu of the master test list" (L937) and "Opens the test-group menu of the master test list" (L938). It does **not** establish what the menu contains or what choosing an entry does. No handler exists: searching `src/` for `CompleteTest`, `TestMenu`, `GroupMenu` returns **no hits**. Implementing would mean inventing the behaviour. |
| **A1.5** TM/TG/CG modes | **NOT DONE — card D-02** | Reference L936 marks it `[UNVERIFIED]` and states "The meaning of the codes is not established"; L1715 and Q-003 (L4796) repeat it. No mapping exists anywhere in the codebase. |
| **A2** follow-up action | **DONE** | Reference L965 + L578 + L1234. Candidate search: `PatientVisitHistoryViewModel` (visit master-detail, not a case-history screen), `HistoryReportsViewModel` (history *reports*, not the case audit trail), `AuditViewModel` (P tab = per-patient case audit: registering user, modification count, last modifier, payment receivers — this **is** 1.12.1). Implemented `FollowUpCommand` + `FollowUpAsync` navigating to `AuditViewModel` and calling `LoadWithPatientIdAsync`. Tests: `PatientEditorRoundThreeTests.FollowUp_OnAnUnsavedPatient_ShowsTheSaveFirstMessageAndNavigatesNowhere`, `FollowUp_OnASavedPatient_OpensTheCaseHistoryScreen`. |
| **A3.1** quote refresh/copy | DONE | §(c) above, L1030 and L1033 |
| **A3.2** verify current state | DONE | `ResultDeliveryViewModel` had only `LoadCommand` (no separate refresh) and **no** clipboard usage anywhere in `src/` (search for `Clipboard` → no hits). `ResultDeliveryView.xaml` had one «عرض» button. |
| **A3.3** RefreshCommand | **DONE** | `RefreshCommand => LoadCommand` (1.4.1.4.2 says only "Reloads the delivery list", which is exactly `LoadAsync`). Tests: `ResultDeliveryRefreshCopyTests.Refresh_ReExecutesTheCurrentLoad` (asserts the mediator call count goes 1 → 2), `Refresh_ReusesThePeriodTheUserIsViewing`. |
| **A3.4** CopyCommand + IClipboardService | **DONE** | New port `TopLab.Presentation.Common.Clipboard.IClipboardService` + `WpfClipboardService` (the only place `System.Windows.Clipboard` is called; retries on `COMException`), registered in `DependencyInjection.cs`. `ResultDeliveryViewModel.BuildCopySummary` is a pure static so the exact text is assertable. Tests: `Copy_PutsTheExactSummaryOnTheClipboard`, `Copy_IncludesTheNoteWhenPresent`, `Copy_WithNoRowSelected_IsDisabledAndCopiesNothing`, `Copy_WithARowSelected_IsEnabled`, `Copy_IgnoresAParameterThatIsNotARow`, `ViewModel_DoesNotReferenceTheWpfClipboardDirectly`. |
| **A3 extra** patient data (1.4.1.4.7) and envelope print (1.4.1.4.11) | **DONE (bonus, both were implementable)** | `PatientDataCommand` → `PatientEditorViewModel.LoadPatientAsync`; `PrintEnvelopeCommand` → the existing `PrintEnvelopeCommand` handler. Tests: `PatientData_WithASelectedRow_NavigatesToThePatientEditor`, `PatientData_WithNoRowSelected_IsDisabled`, `PrintEnvelope_WithASelectedRow_SendsTheExistingEnvelopeCommand`, `PrintEnvelope_WithNoRowSelected_IsDisabled`. |
| **A4** GAP-22 single-test actions | **DONE (3 of 4)** | Print → `IResultPrintCoordinator` with `ResultPrintKind.SimpleResult` + `IPrintedStateRecorder` (the same path the profile/culture VMs use, `ProfileEntryViewModel.PrintAsync` L371, `CultureEntryViewModel.PrintAsync` L466). Preview → new `ISimpleResultPreviewBuilder` (Infrastructure `SimpleResultPreviewBuilder`, rendering the same combined envelope the print path renders) + the existing `IPdfPreviewService`. Review → the existing `ReviewResultCommand`. The 4th action ("open main result") is card **D-07**. Tests: `SimpleResultEntryActionsTests` — 12 tests. |
| **A5** GAP-30 | **NOT DONE — card D-07** | Reference L1443 says only "Opens the main result of the request". No `IsMainTest`/`MainTestId` exists in Domain, configuration or migrations (search → NONE), so "the main result" cannot be resolved. No screen displays a request's main result. |
| **A6** percentage conflict | **CARD D-01** (not fixed) | V1/V3 confirm the block. A separate correctness defect (silent discard of the computed discount) was found and fixed; the product decision is untouched. |
| **A7** tests | **DONE** | 10 (filter) + 9 (refresh/copy) + 2 (patient data) + 2 (envelope) + 12 (GAP-22) + 2 (follow-up) + 1 (IsEnabled) + 8 (Domain probe) + 4 (VM probe) = 50 new tests this round. No test was skipped; no item was declared impossible. |
| **A8** build + tests after each item | **DONE** | Each item was built and its own tests run before moving on; the final full run is in §(h). |

### Per-gap criterion table for GAP-22 (§3.5, verbatim criteria)

| Criterion from §3.5 | Met? | Evidence |
|---|---|---|
| Precondition: single-test result entry open with a saved result | MET | `SimpleResultEntryViewModel.LoadAsync(ResultWorklistItemDto)`; `HasResult => PatientTestId > 0` |
| Action: print | MET | `PrintCommand` → `IResultPrintCoordinator.PrintAsync(PatientTestId, ResultPrintKind.SimpleResult, ct)` |
| Action: preview | MET | `PreviewCommand` → `ISimpleResultPreviewBuilder.BuildAsync` then `IPdfPreviewService.PreviewAsync` |
| Action: review | MET | `ReviewCommand` → `ReviewResultCommand(PatientTestId)` |
| Action: open main result | **NOT MET — card D-07** | no main-result concept exists |
| Observable result: each action performs its function | MET for 3 of 4 | 12 tests |
| Validation: actions disabled when no result saved | MET | `HasResult` guard in each method, plus the guard messages asserted by 3 tests |
| Persistence: no schema change | MET | no migration, no column |
| Security: each respects existing permission | MET | print path uses the coordinator (no new gate); review uses `ReviewResultCommand`, which already carries `ResultsAccessPolicy.ReviewResults`; preview reads only |
| Regression: existing result-entry tests pass | MET | full suite green (§h) |

### Per-gap criterion table for GAP-30 (§3.5, verbatim criteria)

| Criterion from §3.5 | Met? | Evidence |
|---|---|---|
| Precondition: composite result screen open | N/A | `CombinedReportViewModel` exists, but no "main result" concept |
| Action: open main result of the request | **NOT MET** | card D-07 |
| Observable result: main result opens | **NOT MET** | card D-07 |
| Validation: disabled when no main result exists | **NOT MET** | no command exists |
| Persistence: no schema change | MET | nothing added |
| Security: respects existing permission | N/A | no command |
| Regression: existing composite screen tests pass | MET | full suite green |


---

## (f) Part B — Decision Catalog

Six cards. Ordering is by how much of R1 each one blocks: D-01 blocks the whole percentage feature
(GAP-14, and with it the last unmet item of the account panel); D-02 blocks one third of GAP-11 and
cannot be answered by reading anything; D-03 blocks two delivery-screen actions and one patient-editor
action; D-04 blocks the delivery print/preview pair; D-05 blocks the delivery notes action; D-06
blocks nothing that Part A did not already close, but it is the only remaining way to finish GAP-22.

---

### D-01: Percentage discount when the computed discount exceeds the current operation amount

**Question.** When the operator enters a percentage, and that percentage of the patient's TOTAL
account amount is larger than the amount being paid right now, what should the system do?

**Reference evidence.**
- L976: `| 1.3.1.3 | Discount Percentage Field | % | Field / UI element | The discount applied to the patient's account as a percentage. | [VERIFIED] | In scope |`
- L975: `| 1.3.1.2 | Discount Value Field | قيمة الخصم | Field / UI element | The discount applied to the patient's account as an absolute amount. | [VERIFIED] | In scope |`
- L1513: `2. If a discount exists, enter its value or its percentage. → 1.3.1.2, 1.3.1.3`
- L979: `| 1.3.1.4.2 | Enter Key Account Operation | — | Rule / Rule | The first `Enter` press performs the accounting operation for the patient; the second press saves the operation. | [VERIFIED] | In scope |`
- L1633: `| 1.3.1.4.2 | The two `Enter` presses are not interchangeable: the first computes, the second saves. | [VERIFIED] |`

**The reference is silent on this point.** It never states the base of the percentage (the owner fixed
that: the patient's total account amount), and it never states what happens when the resulting amount
exceeds the operation amount. The only rule in the codebase that touches this is the pre-existing
Domain guard "discount cannot exceed the operation amount", which Stage 3 §3.5 GAP-14 lists as a
validation that must stay: *"Validation: discount cannot exceed amount; surcharge cannot carry discount."*

**Why this cannot be settled without the owner.** This is a money decision, not a coding one. Today a
10%-of-total discount is refused on any payment smaller than a tenth of the bill — on a 1000-pound
account, the ordinary 100-pound part payment can carry at most 10%, so the field the owner asked for is
unusable in the normal case. Every way out changes either what the patient is charged, what the audit
trail records, or what the operator is allowed to type, and those are the laboratory's own financial
rules, not the programmer's. The reference cannot break the tie because it never describes the
partial-payment case at all.

**Affected.** GAP-14 (the only unmet acceptance criterion left: *"percentage discount computes
correctly"*); identifiers 1.3.1.2, 1.3.1.3, 1.3.1.4.2; `AccountEntrySession.ApplyPercentage` and
`PressEnter`; `AccountEntryCalculator.DiscountAmountFromPercentage`; `PatientEditorViewModel` and
`PatientAccountViewModel` (both `AccountEntryEnterAsync`); `PaymentOperation.Create` (its
`discountAmount > amount` guard); screens `PatientEditorView.xaml`, `PatientAccountView.xaml`;
existing tests `AccountEntrySessionTests`, `AccountEntryParserTests`, `PatientEditorRoundThreeTests`,
`PatientEditorPercentageConflictTests`, `AccountEntryPercentageProbeTests`.

**Options.**

1. **Cap the discount at the operation amount.**
   - What changes for the user: they type 50% on a 100 payment of a 1000 bill and the discount becomes
     100 (10%) instead of 500. A short note in the status line says the discount was capped.
   - Data / audit / financial correctness: the operation stays internally consistent (discount ≤
     amount), the balance formula is untouched, and the audit trail records a smaller discount than the
     operator typed. The remaining part of the discount is simply not granted — the patient still owes it.
   - Migration / schema / permission: no.
   - Effort: low. Risk: **medium** — the operator may believe a 50% discount was given while only 10%
     was; the status line has to be unmistakable.
2. **Split the discount across operations.**
   - What changes for the user: typing 50% on a 100 payment writes a 100 discount now and remembers the
     remaining 400 to be applied automatically to the next payments until it is used up.
   - Data / audit / financial correctness: needs a place to remember the unapplied remainder. That is
     either a new stored column (schema change — forbidden this round) or an in-memory value that dies
     when the screen closes. Audit trail becomes hard to explain: several operations each carry a piece
     of one discount.
   - Migration / schema / permission: **yes** — a stored remainder column, or an equivalent
     "discount carry" table.
   - Effort: high. Risk: **high**.
3. **Relax the Domain rule so a discount may exceed the operation amount.**
   - What changes for the user: 50% of 1000 = 500 is accepted on a 100 payment; the account immediately
     shows a 400 credit in the patient's favour.
   - Data / audit / financial correctness: the settled balance formula `TotalPaid = Σ(Amount + Discount)`
     would produce a negative paid total, i.e. the laboratory owes the patient money that was never
     refunded. It also breaks the Stage 3 §3.5 validation that must stay, and it silently overlaps the
     refund feature built in Round 3 (a refund is the sanctioned way to give money back).
   - Migration / schema / permission: no schema change, but a Domain invariant is removed.
   - Effort: low. Risk: **high** — this is the option that can produce a wrong balance in production.
4. **Restrict the field: the percentage may only be used when the payment covers the whole remaining
   balance.**
   - What changes for the user: on a part payment the % field is disabled (or the Enter press says "النسبة
     متاحة فقط عند سداد كامل المبلغ"); the value-discount field keeps working as it does today.
   - Data / audit / financial correctness: nothing changes in the stored data or the formula; every
     accepted percentage is guaranteed to be ≤ the amount, so the Domain rule is never approached.
   - Migration / schema / permission: no.
   - Effort: low. Risk: **low**.

**Recommendation: option 4.** The reference only says the percentage is "the discount applied to the
patient's account as a percentage" (L976) and the walkthrough only offers "enter its value or its
percentage" (L1513) — neither sentence promises a percentage on a part payment. Option 4 keeps the
Domain guard that Stage 3 §3.5 explicitly requires to stay, keeps the balance formula and the refund
path untouched, and cannot produce a wrong figure. Option 1 is the natural second choice if the owner
wants the field always usable, but it needs a very loud status line to avoid the operator believing a
50% discount was granted when 10% was. Options 2 and 3 are rejected: 2 needs a schema change this
round forbids, and 3 breaks the settled formula and duplicates the refund feature.

**If deferred.** GAP-14 stays partial; the percentage field stays present but refuses the common
partial-payment case with a clear Arabic message ("الخصم لا يمكن أن يتجاوز مبلغ الدفع."), which is at
least honest. Everything else in R1 ships: the value discount, the `+` surcharge, the two-press Enter,
refund and clear-account all work today.

---

### D-02: What the TM / TG / CG mode options are supposed to filter by

**Question.** The test filter bar has three mutually exclusive mode options labelled TM, TG and CG;
what should each one select?

**Reference evidence.**
- L936: `| 1.2.2.5.1.2 | Test Filter Mode Options | TM / TG / CG | Field group / Container | Three mutually exclusive mode options marked with the short codes TM, TG and CG. The meaning of the codes is not established. | [UNVERIFIED] | In scope |`
- L1715: `| 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | [UNVERIFIED] |`
- L4796: `| Q-003 | B — element visible, its behaviour not established | 1 | 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | The element is visible in the source. | The meaning of the three test-filter mode codes is not established. | showcase p. 36 | The behaviour of the element is not established; the element itself is. |`

**The reference explicitly declares itself silent.** It records that the element exists and that its
meaning is not established, in three separate places, and marks it `[UNVERIFIED]`.

**Why this cannot be settled without the owner.** This is a domain-vocabulary decision, and the owner
is the only person who knows the reference system's shorthand. A laboratory professional reads TM/TG/CG
as some pair of "test / test group / custom group" — but which letter maps to which, and whether the
third one means the programme's custom groups (the `CustomGroup` entity that already exists) or
something else entirely, cannot be inferred from three letters. Guessing wrong would ship a filter that
silently hides the wrong tests, which is worse than shipping no filter at all. No amount of code
reading can resolve it: there is no TM/TG/CG string anywhere in the source tree.

**Affected.** GAP-11 (acceptance criterion *"mode options are mutually exclusive; invalid mode is
rejected"*); identifier 1.2.2.5.1.2; `PatientEditorViewModel` (would need a mode property and to extend
`MatchesCatalogFilter`); `PatientEditorView.xaml` (would need three radio buttons); the existing
`CatalogFilterText`/`FilteredCatalogTests` machinery; tests `PatientEditorCatalogFilterTests`.

**Options.**

1. **Owner supplies the mapping, then implement it.**
   - What changes for the user: three radio buttons appear next to the filter field; each narrows the
     catalogue to one kind of item, exactly as the reference system does.
   - Data / audit / financial correctness: no effect — it is a view filter only.
   - Migration / schema / permission: no.
   - Effort: low once the mapping is known. Risk: low.
2. **Ship the three options as mutually exclusive but inert, with a visible "not configured" note.**
   - What changes for the user: the buttons exist and are mutually exclusive, but choosing one changes
     nothing and a note says so.
   - Data / audit / financial correctness: no effect.
   - Migration / schema / permission: no.
   - Effort: low. Risk: **medium** — a control that looks functional and does nothing is a support
     burden and would fail §3.5's "filter narrows the list".
3. **Do not ship the mode options at all; keep only the working text filter.**
   - What changes for the user: nothing appears where the reference has three small buttons; the filter
     field works.
   - Data / audit / financial correctness: no effect.
   - Migration / schema / permission: no.
   - Effort: none (already the state). Risk: low.
4. **Infer the mapping from the existing entities and ship it as a best guess.**
   - What changes for the user: the buttons filter by test name / test group / custom group.
   - Data / audit / financial correctness: no effect on money, but a wrong guess hides tests the
     operator expects to see.
   - Migration / schema / permission: no.
   - Effort: medium. Risk: **high** — an unverified guess presented as the reference behaviour.

**Recommendation: option 1, with option 3 as the interim state.** The reference states three times that
the meaning is not established (L936, L1715, L4796); the honest position until the owner says what the
letters mean is to ship the working text filter and leave the mode options out, rather than to invent a
mapping. Option 4 is the one thing that must not happen: it would put an unverified guess into a screen
the operator trusts. Once the owner names the mapping, option 1 is a small, low-risk change.

**If deferred.** GAP-11 stays partial (the filter field is done, the mode options are not). R1 ships.

---

### D-03: Which screen the "complete test menu" and "test group menu" should open

**Question.** The master test list has a "complete test menu" action and a "test group menu" action;
what should each one open, and what should choosing an entry do?

**Reference evidence.**
- L937: `| 1.2.2.5.1.3 | Complete Test Menu Action | complete test menu | Action / Function | Opens the complete test menu of the master test list. | [VERIFIED] | In scope |`
- L938: `| 1.2.2.5.1.4 | Test Group Menu Action | test group menu | Action / Function | Opens the test-group menu of the master test list. | [VERIFIED] | In scope |`
- L279–L283 place both under `1.2.2.5.1 Test Filter Bar` of the registration screen, next to the filter
  field and the mode options.

**The reference names the actions but not their content.** It says each "opens … the menu" and stops.
It does not say whether the menu is a dropdown that filters, a modal that lists tests, or a picker that
adds several tests at once. Stage 3 §3.5 narrows it only to *"menus open"*.

**Why this cannot be settled without the owner.** "A menu opens" is satisfied by at least three
different screens with different consequences: a filter dropdown (changes what the list shows), a
multi-select picker (adds tests to the visit — a database write), or a read-only reference list (changes
nothing). The third is harmless, the second writes to the visit, and choosing wrongly means either a
dead button or an unintended bulk write. The codebase has candidate machinery for all three
(`TestGroups` is already loaded but unbound; `AddTestsToVisitCommand` exists; `TestCatalogViewModel`
exists as a full catalogue screen), so the code cannot arbitrate either.

**Affected.** GAP-11 (acceptance criterion *"menus open"*); identifiers 1.2.2.5.1.3, 1.2.2.5.1.4;
`PatientEditorViewModel` (would need two commands); `PatientEditorView.xaml`; possibly
`AddTestsToVisitCommand` and `TestGroups`; tests `PatientEditorCatalogFilterTests`.

**Options.**

1. **Owner describes what each menu contains; then implement as a filter dropdown.**
   - What changes for the user: two dropdowns appear; picking an entry narrows the catalogue.
   - Data / audit / financial correctness: no writes — view filter only.
   - Migration / schema / permission: no.
   - Effort: low–medium. Risk: low.
2. **Implement both as multi-select pickers that add tests to the visit.**
   - What changes for the user: choosing entries adds several tests at once instead of one at a time.
   - Data / audit / financial correctness: **writes** `PatientTest` rows and changes the charged total;
     needs the same guards the existing «إضافة» button has.
   - Migration / schema / permission: no, but it is a write path and must reuse `AddTestsToVisitCommand`.
   - Effort: medium. Risk: **medium** — a bulk write from a menu the operator expects to be a filter.
3. **Implement them as navigation to the existing full catalogue screen (`TestCatalogViewModel`).**
   - What changes for the user: the button opens the laboratory's test-catalogue screen.
   - Data / audit / financial correctness: no effect.
   - Migration / schema / permission: no.
   - Effort: low. Risk: **medium** — that screen is an administrative editor; sending a registrar there
     is probably not what the reference means.
4. **Leave both out until described.**
   - What changes for the user: no menus; the text filter works.
   - Data / audit / financial correctness: no effect.
   - Migration / schema / permission: no.
   - Effort: none. Risk: low.

**Recommendation: option 1, with option 4 as the interim state.** The reference stops at "opens the
menu" (L937, L938) and §3.5 only asks that "menus open", so the owner's one-sentence description is the
whole missing input. Option 4 is what ships today and costs nothing. Options 2 and 3 are rejected as
guesses: one silently writes visit rows, the other sends a registrar into an administrative screen.

**If deferred.** GAP-11 stays partial alongside D-02. R1 ships.

---

### D-04: What "prints the delivery list" and "shows the delivery list before printing" should produce

**Question.** The delivery screen has a print action and a preview action for the delivery *list*
itself; what document should they produce, and with which columns?

**Reference evidence.**
- L1031: `| 1.4.1.4.3 | Delivery List Print Action | طباعة | Action / Function | Prints the delivery list. | [VERIFIED] | In scope |`
- L1032: `| 1.4.1.4.4 | Preview Action | معاينة | Action / Function | Shows the delivery list before printing. | [VERIFIED] | In scope |`
- Stage 3 §3.5 GAP-12: *"click … print, preview …"*; *"each action performs its function"*.

**The reference names the actions but not the document.** It does not say which of the screen's columns
appear, whether the period is printed in the header, whether the page is A4 or A5, or whether the list
is one page per patient or a single table.

**Why this cannot be settled without the owner.** A "delivery list" print is a document the laboratory
hands to staff at the counter. Its content is an operational choice with real consequences: printing
patient names and lab ids on a sheet that is left on a desk is a privacy exposure, and printing the
balance on the same sheet changes what the counter staff will be asked about. The codebase has no
delivery-list document at all (searching `DeliveryList`, `PrintDelivery`, `UndeliveredReport`,
`DeliveryReport` in `src/` returns **no hits**), so there is no existing shape to copy and no handler to
reuse. Building one means inventing the document.

**Affected.** GAP-12 (acceptance criteria *"print"*, *"preview"*); identifiers 1.4.1.4.3, 1.4.1.4.4;
would need a new Application query/command plus an Infrastructure PDF writer; `ResultDeliveryViewModel`;
`ResultDeliveryView.xaml`; tests `ResultDeliveryRoundThreeTests`, `ResultDeliveryRefreshCopyTests`.

**Options.**

1. **Owner specifies the columns and the paper; then implement a dedicated delivery-list PDF.**
   - What changes for the user: a printed sheet listing the period's undelivered patients, with exactly
     the columns the owner names.
   - Data / audit / financial correctness: read-only; no data changes.
   - Migration / schema / permission: no; the print permission that already guards report printing applies.
   - Effort: medium (new query + writer + preview path). Risk: low once specified.
2. **Reuse the existing "group patient slips" print (one slip per patient) for both actions.**
   - What changes for the user: pressing print produces one slip per patient instead of one list sheet.
   - Data / audit / financial correctness: read-only.
   - Migration / schema / permission: no.
   - Effort: **low** — `PrintGroupPatientSlipsCommand` and `GroupPatientSlipsViewModel` already exist and
     already print one page per patient.
   - Risk: **medium** — it is a different document from the one the reference names; the operator may
     expect a list.
3. **Print the on-screen grid as-is (generic table dump).**
   - What changes for the user: whatever is on screen is printed, including the status and notes columns.
   - Data / audit / financial correctness: read-only, but it prints the patient-notes column by default,
     which is the privacy-sensitive one.
   - Migration / schema / permission: no.
   - Effort: medium. Risk: **medium** — printing notes by default is a privacy decision nobody made.
4. **Leave both out.**
   - What changes for the user: no print/preview on the delivery screen; the envelope print and the
     group slips still work.
   - Data / audit / financial correctness: no effect.
   - Migration / schema / permission: no.
   - Effort: none. Risk: low.

**Recommendation: option 1, with option 4 as the interim state, and option 2 named as the cheap
fallback if the owner wants something today.** The reference gives only the verb "Prints the delivery
list" (L1031) — it does not describe the document, and the codebase has no such document to copy. A
delivery list is a sheet that leaves the building, so its columns (in particular whether patient notes
are on it) are the owner's call, not mine. Option 2 is genuinely cheap because the group-slip print
already exists, but it is a different document and should be chosen knowingly, not silently.

**If deferred.** GAP-12 stays partial: account, refresh, copy, patient data, combined report, empty
report and envelope print all work; print and preview of the list do not. R1 ships.

---

### D-05: What the delivery screen's notes action should do

**Question.** The delivery action block has a notes action; should it open the patient's stored note
read-only, or let the operator edit it?

**Reference evidence.**
- L1034: `| 1.4.1.4.6 | Notes Action | ملاحظات | Action / Function | Opens the notes attached to the delivery entry. | [VERIFIED] | In scope |`
- Stage 3 §3.5 GAP-04: *"the note is visible on both screens; an action opens the stored patient note"*;
  *"Validation: the note action does not modify the note; the note round-trips unchanged"*.

**The reference is partly explicit and partly silent.** It says the action "opens the notes attached to
the delivery entry" — it does not say whether the note is the *patient's* note (which is what GAP-04
assumes, and what the screen already displays in its ملاحظات column) or a note attached to the
*delivery transaction*. §3.5 resolves the read-only part ("does not modify the note") but not the
ownership part.

**Why this cannot be settled without the owner.** "The notes attached to the delivery entry" can mean
two different things stored in two different places: `Patient.Notes` (already on screen, one per
patient) or a note on the delivery event itself (which does not exist in the schema and would need a new
column — forbidden this round). If it is the patient's note, the action is a read-only dialog and needs
no schema change. If it is a delivery note, it needs a new column and a new write path. The owner is the
only one who knows which the reference system shows, and §3.5's phrase "the stored patient note" leans
one way without settling it.

**Affected.** GAP-04 (the one unmet criterion: *"an action opens the stored patient note"*); GAP-12
(the notes action); identifiers 1.4.1.4.6, 1.4.1.4.1; `ResultDeliveryViewModel`; `ResultDeliveryView.xaml`;
`DeliveryAccountDto.PatientNotes`; tests `GetDeliveryAccountNotesTests`, `DeliveryHandoverNotesTests`.

**Options.**

1. **Read-only dialog showing the patient's note (the note already on screen).**
   - What changes for the user: pressing ملاحظات opens a small window with the patient's note, no editing.
   - Data / audit / financial correctness: read-only; nothing is written; the note round-trips unchanged
     (exactly §3.5's validation).
   - Migration / schema / permission: no — `Patient.Notes` already exists and is already loaded.
   - Effort: **low**. Risk: low.
2. **Read-only dialog, but with a copy-to-clipboard button.**
   - What changes for the user: same window, plus a «نسخ» button that copies the note text.
   - Data / audit / financial correctness: read-only; clipboard only.
   - Migration / schema / permission: no.
   - Effort: low (the clipboard port now exists from A3). Risk: low.
3. **Editable dialog that saves back to `Patient.Notes`.**
   - What changes for the user: the operator can correct the patient's note from the delivery screen.
   - Data / audit / financial correctness: **writes** the patient record from a screen whose job is
     delivery; no audit entry exists for a note edit, so the change would be invisible afterwards.
   - Migration / schema / permission: no schema change, but it needs the patient-update permission and
     an audit decision.
   - Effort: medium. Risk: **high** — an unaudited write to the patient record from the delivery desk.
4. **A note on the delivery event itself (new stored field).**
   - What changes for the user: a separate note per delivery, not the patient's note.
   - Data / audit / financial correctness: needs a new column and a migration.
   - Migration / schema / permission: **yes** — forbidden this round.
   - Effort: high. Risk: medium.

**Recommendation: option 1.** Stage 3 §3.5 asks for exactly two things — that an action opens the stored
patient note, and that the action "does not modify the note; the note round-trips unchanged" — and
option 1 satisfies both with no schema change, no new permission and no write path. The screen already
displays that same note in its ملاحظات column (`ResultDeliveryView.xaml`), so the dialog adds reach, not
new data. Option 2 is a harmless nicety if the owner wants it. Options 3 and 4 are rejected: 3 creates an
unaudited write from the delivery desk, and 4 needs the schema change this round forbids.

**If deferred.** GAP-04 stays partial (the note is visible on both screens but no action opens it) and
GAP-12 loses one action. R1 ships.

---

### D-06: What "the main result of the request" is on the composite result screen

**Question.** The composite result screen has a "main result" action; which result is "the main result
of the request", and which screen should it open?

**Reference evidence.**
- L1443: `| 1.22.12.5 | Composite Screen Main Result Action | النتيجة الرئيسية | Action / Function | Opens the main result of the request. | [VERIFIED] | In scope |`
- L1426: `| **1.22** | **Composite Test Entry Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |`
- L1427: `| 1.22.1 | Composite Screen Test Banner | — | Field / UI element | The banner that carries the name of the test whose sub-items are being entered. | [VERIFIED] | In scope |`
- L2489: `| 3.2.1.3.29 | Main Test Check Box | Main test | Field / UI element | Marks the test as the main test of its group. | [VERIFIED] | In scope |`
- Stage 3 §3.5 GAP-30: *"open main result of the request"*; *"main result opens"*; *"disabled when no main result exists"*.

**The reference points at a concept the codebase does not have.** L2489 establishes that a test can be
marked as "the main test of its group" — that is the only place the word "main" is defined. But the
`Test` entity has no such property (its public members are `Name`, `ReportName`, `ReceiptName`,
`TestCode`, `IsActive`, `AnalyteId`, `TestGroupId`, `Barcode`, `CompletionDurationMinutes`, `IsSentOut`,
`SentOutCostPrice`, `PatientPrice`, `LabToLabPrice`, `ResultKind`, `IsCultureType`), and searching the
whole tree for `IsMainTest` / `MainTestId` returns **no hits** — no column, no migration, no
configuration. So "the main test" exists in the reference and has never been modelled here.

**Why this cannot be settled without the owner.** Two different decisions are tangled. First, what
"main" means operationally: is it the main test of the test's *group* (the L2489 concept), or the
composite test whose sub-items are being entered (the L1427 banner concept)? Second, whether modelling
it is in scope at all: the L2489 flag is a **test-catalogue** field, and building it means a column, a
migration and a catalogue editor change — all forbidden this round, and all arguably R5 work, not R1.
Without the first answer the second cannot even be sized. The codebase cannot arbitrate: it has no main
test, no main result, and no screen that displays "the result of a request".

**Affected.** GAP-30 (all three acceptance criteria); GAP-22's fourth action (*"open main result"*);
identifiers 1.22.12.5, 3.2.1.3.29, 1.22.1; would need `Test` + configuration + migration for the flag;
`CombinedReportViewModel`; `SimpleResultEntryViewModel`; screens `CombinedReportView`, `SimpleResultEntryView`;
tests `SimpleResultEntryActionsTests`.

**Options.**

1. **Model the main-test flag, then wire the action.**
   - What changes for the user: a test can be marked as the main test of its group in the catalogue, and
     النتيجة الرئيسية opens that test's report from the composite screen.
   - Data / audit / financial correctness: adds a catalogue attribute; no effect on money or on the
     balance; the audit trail is unaffected.
   - Migration / schema / permission: **yes** — a `Test.IsMainTest` column and a migration, which this
     round forbids and which is arguably R5 (reference-data editing) work.
   - Effort: **high**. Risk: medium.
2. **Treat "the main result" as the composite test's own report and open the existing result sheet.**
   - What changes for the user: النتيجة الرئيسية opens the report of the composite test itself.
   - Data / audit / financial correctness: read-only.
   - Migration / schema / permission: no.
   - Effort: **low** — `PatientResultSheetViewModel` and `ShowReportCommand` already exist.
   - Risk: **medium** — it may be exactly right, or it may be a different document from the one the
     reference means; nothing in the reference text distinguishes them.
3. **Ask the owner for one sentence describing the main result, then decide between 1 and 2.**
   - What changes for the user: depends on the answer.
   - Data / audit / financial correctness: depends.
   - Migration / schema / permission: depends — option 1 needs one, option 2 does not.
   - Effort: none now. Risk: low.
4. **Leave the action out of R1 and raise it for R5 alongside the rest of reference-data editing.**
   - What changes for the user: the button does not appear on the composite screen.
   - Data / audit / financial correctness: no effect.
   - Migration / schema / permission: no.
   - Effort: none. Risk: low.

**Recommendation: option 3 now, then option 2 unless the owner says otherwise.** The reference defines
"main" only as a **test-catalogue** flag (L2489), which is not modelled here and whose implementation
is a schema change forbidden in this round — so option 1 cannot be done now even if it is right.
Option 2 is the only reading that can be built today without inventing a column, and it is honest
provided the owner confirms that "النتيجة الرئيسية" means the composite test's own report rather than a
separately-flagged main test. Option 4 is the safe fallback if the owner prefers to settle this with the
rest of the catalogue work.

**If deferred.** GAP-30 stays not started and GAP-22 stays at 3 of 4 actions. Everything else in R1
ships — including print, preview and review on the single-test screen, which are the three actions
§3.5 lists before "open main result".


---

## (g) Screens changed this round — row tables and ASCII drawings

**STATIC EVIDENCE ONLY.** Every table and drawing below was read from the XAML text. No screen was
executed, rendered, or verified visually. No screenshot exists.

### G1. `PatientEditorView.xaml` — the catalogue column (left)

Container: `DockPanel Grid.Column="1"` (L473), children docked top-down.

| Order | Element | Dock | Content |
|---|---|---|---|
| 1 | TextBlock | Top | «التحاليل» |
| 2 | StackPanel | Top | «إجراءات الزيارة»: profile picker, sample-type checkboxes, custom-group picker, «مسح الكل» |
| 3 | TextBlock | Top | «تحاليل المريض:» |
| 4 | TextBlock | Top | «لا توجد تحاليل مختارة.» (collapsed unless `ShowSelectedTestsEmpty`) |
| 5 | ListView | Top | `SelectedTests`, height 180 |
| 6 | TextBlock | Top | «كتالوج التحاليل:» |
| **7** | **WrapPanel** | **Top** | **NEW — filter bar: «بحث:» + TextBox(`CatalogFilterText`) + «مسح» + «لا توجد تحاليل مطابقة للبحث.»** |
| 8 | TextBlock | Top | «لا توجد تحاليل في الكتالوج.» (collapsed unless `ShowCatalogEmpty`) |
| 9 | ListView | Fill | **changed source** `CatalogTests` → `FilteredCatalogTests` |

```
DockPanel (Grid.Column=1, RTL)
 ┌──────────────────────────────────────────────┐
 │ التحاليل                                      │  [1]
 │ إجراءات الزيارة: بروفايل [▾] إضافة            │  [2]
 │   بول براز دم مني نخاعي خارج المعمل            │
 │   مجموعة: [▾] إضافة   مسح الكل                │
 │ تحاليل المريض:                                │  [3]
 │ (لا توجد تحاليل مختارة.)                      │  [4]
 │ [ ListView: SelectedTests            ]        │  [5]
 │ كتالوج التحاليل:                              │  [6]
 │ بحث: [__________] [مسح]  (لا توجد مطابقة)     │  [7]  <-- NEW
 │ (لا توجد تحاليل في الكتالوج.)                 │  [8]
 │ [ ListView: FilteredCatalogTests      ]       │  [9]  <-- source changed
 └──────────────────────────────────────────────┘
```

### G2. `PatientEditorView.xaml` — the action band (bottom, WrapPanel #2)

| Order | Button | Command | Enabled when |
|---|---|---|---|
| 1 | جديد | `NewPatientCommand` | always |
| 2 | حفظ | `SaveCommand` | always |
| 3 | حذف | `DeleteCommand` | always |
| 4 | تراجع | `UndoCommand` | always |
| 5 | باركود | `PrintBarcodeCommand` | always |
| 6 | ورقة العمل | `PrintWorkSheetCommand` | always |
| 7 | الإيصال | `PrintReceiptCommand` | always |
| 8 | الفاتورة | `PrintInvoiceCommand` | always |
| 9 | طلب تحاليل | `PrintLabOrderCommand` | always |
| 10 | مغلف | `PrintEnvelopeCommand` | **`IsEditMode`** (new) |
| 11 | نافذة المرضى | `PatientWindowCommand` | always (owner decision: add mode too) |
| 12 | نتائج التحاليل | `LabResultsCommand` | **`IsEditMode`** (new) |
| 13 | **متابعة** | **`FollowUpCommand`** | **`IsEditMode`** (new row) |

```
[جديد][حفظ][حذف][تراجع] | [باركود][ورقة العمل][الإيصال][الفاتورة][طلب تحاليل]
[مغلف][نافذة المرضى][نتائج التحاليل][متابعة]     <-- متابعة is new
```

### G3. `ResultDeliveryView.xaml` — the filter/action bar (DockPanel.Dock="Top")

| Order | Element | Command | Enabled when |
|---|---|---|---|
| 1 | «من:» + DatePicker | — | — |
| 2 | «إلى:» + DatePicker | — | — |
| 3 | «عرض» | `LoadCommand` | always |
| 4 | **«تحديث»** | **`RefreshCommand`** | always |
| 5 | **«نسخ»** | **`CopyCommand`** | selected row |
| 6 | **«بيانات المريض»** | **`PatientDataCommand`** | selected row |
| 7 | **«طباعة ظرف»** | **`PrintEnvelopeCommand`** | selected row |
| 8 | «حساب المريض» | `PatientAccountCommand` | selected row |
| 9 | «تقرير مجمع» | `CombinedReportCommand` | selected row |
| 10 | «تقرير فارغ» | `EmptyReportCommand` | selected row |
| 11 | «قصاصة تقرير جماعية» | `OpenGroupSlipsCommand` | always |
| 12 | «رجوع» | `BackCommand` | always |

```
من:[date] إلى:[date] [عرض][تحديث][نسخ][بيانات المريض][طباعة ظرف][حساب المريض][تقرير مجمع][تقرير فارغ][قصاصة تقرير جماعية][رجوع]
                     ^^^^^^  ^^^^  ^^^^^^^^^^^^^^^  ^^^^^^^^^^^^^  <-- new this round
```

### G4. `SimpleResultEntryView.xaml` — the action band (Grid.Row=7)

Grid has 8 rows (`Auto` ×6, `*`, `Auto`). The new status line occupies the previously empty row 5.

| Row | Element |
|---|---|
| 0 | Border — test banner (`TestName`, `TestCode`) |
| 1 | StackPanel — «قيمة النتيجة:» + TextBox |
| 2 | StackPanel — «علامة النتيجة:» + ComboBox |
| 3 | StackPanel — «ملاحظات:» + TextBox |
| 4 | TextBlock — `ErrorMessage` (red) |
| **5** | **TextBlock — `StatusMessage` (new; was empty)** |
| 6 | `*` — unused spacer |
| 7 | StackPanel — action band (below) |

| Order | Button | Command |
|---|---|---|
| 1 | مسح | `ClearCommand` |
| 2 | تعليق | `OpenCommentPickerCommand` |
| 3 | حفظ (`IsDefault`) | `SaveCommand` |
| 4 | *Separator* | — |
| **5** | **طباعة** | **`PrintCommand`** |
| **6** | **معاينة** | **`PreviewCommand`** |
| **7** | **مراجعة** | **`ReviewCommand`** |

```
[مسح][تعليق][حفظ] | [طباعة][معاينة][مراجعة]
                     ^^^^^^^^^^^^^^^^^^^^^^ new (1.22.12.2/.3/.4)
```

---

## (h) Assumptions I was forced to make

| # | Assumption | Alternatives | Why |
|---|---|---|---|
| 1 | The catalogue filter matches **name OR code**, case-insensitive, substring. | (a) name only; (b) prefix-only; (c) also match `ReportName`. | Reference L935 says only "The filter control of the master test list" — no field is named. Name+code covers what an operator actually types; `ReportName` is a print artefact, not a catalogue label. |
| 2 | The filter narrows a **second** collection (`FilteredCatalogTests`) and leaves `CatalogTests` intact. | (a) mutate `CatalogTests` in place. | Clearing the filter must restore every test without a round trip; mutating in place would require reloading the catalogue, which needs the mediator and the whole `LoadCatalogAsync` path. |
| 3 | `ShowCatalogEmpty` keeps meaning "the catalogue itself is empty", and a new `ShowCatalogFilterNoMatch` covers "the filter matched nothing". | (a) reuse `ShowCatalogEmpty` for both. | The two states need different Arabic text; merging them would tell the operator the catalogue is empty when it is not. |
| 4 | "Follow-up" (1.2.2.6.10) maps to `AuditViewModel` (the P tab = per-patient case audit). | (a) `PatientVisitHistoryViewModel`; (b) `HistoryReportsViewModel`; (c) a new screen. | Reference L578 names the section "Case Follow-up and Audit Trail" and L1234 defines its screen as "the recorded events of the case, with the user name and the date and time of each" — that is exactly `AuditViewModel`'s P tab (registering user, modification count, last modifier, payment receivers). `PatientVisitHistoryViewModel` is a visit master-detail; `HistoryReportsViewModel` produces history *reports*. |
| 5 | "Copy" copies a one-line Arabic summary of the selected row, not the whole grid. | (a) copy all rows; (b) copy only the patient name. | Reference L1033 says "Copies the current delivery entry" — singular, and the button is row-gated. |
| 6 | "Refresh" is the same operation as the existing «عرض» load. | (a) a separate command that also resets the page. | Reference L1030 says only "Reloads the delivery list". `RefreshCommand` therefore aliases `LoadCommand`, which re-runs `LoadAsync` with the current period. |
| 7 | The single-test preview renders the **combined-report envelope for the one test id**, exactly as the print path does. | (a) a new simple-result-specific PDF; (b) no preview at all. | `ResultPrintCoordinator` already maps `ResultPrintKind.SimpleResult` to `BuildCombinedTokenAsync` (L75–L77), so previewing the same envelope guarantees "what you preview is what prints". |
| 8 | The preview builder lives in Infrastructure behind a new Application port. | (a) build the PDF inline in the ViewModel (the accepted-debt pattern of `ProfileEntryViewModel`/`CombinedReportViewModel`). | `PresentationLayeringTests` pins the exact set of Presentation files that may reference Infrastructure; a fourth one would fail that test and deepen recorded debt. A port keeps the layering pin intact. |
| 9 | The clipboard port lives in Presentation, not Application. | (a) an Application port. | The clipboard is a UI concern; Application must not depend on it. `IDialogService` — the closest existing analogue — is also a Presentation port. |
| 10 | The three delivery actions (copy, patient data, envelope print) are **disabled** when no row is selected, per §3.5 "actions disabled when no delivery loaded". | (a) leave them always enabled and rely on the null guard. | §3.5 states the criterion explicitly; the null guard alone satisfies "does nothing" but not "disabled". |
| 11 | `PatientWindowCommand` stays **enabled** with no patient loaded. | (a) disable it like the others. | Owner decision in Round 3 A1: "the patient list must open in add mode and in edit mode". Disabling it would contradict that. Recorded here because it is the one deliberate exception to §3.5's "actions are disabled when no patient is loaded". |
| 12 | The percentage conversion clears the percentage field after writing the amount. | (a) keep both fields populated. | Round-3 owner decision: "the two are never applied together". Clearing the percentage (not the amount) is the only way to honour both that rule and "converting writes the amount into the value field". |
| 13 | The percentage probe tests assert **current** behaviour and are marked as characterization tests. | (a) assert the desired behaviour (they would fail). | The desired behaviour is exactly what D-01 must decide; a red test would block the build while the decision is pending. |
| 14 | `SimpleResultPreviewBuilder` resolves the patient id from `PatientTest` rather than from a report query. | (a) add a new query; (b) reuse `ResolvePatientIdAsync` from the coordinator (private). | The builder needs one integer; a query would duplicate read logic and the coordinator's resolver is private. |

---

## (i) Contradictions between the reference text and the owner's earlier decisions

**None.** Each owner decision either restates the reference or fills a silence:

| Owner decision | Reference text | Verdict |
|---|---|---|
| OD-11 (a) `+`-prefix surcharge in the paid field | L978 "entered in the paid field preceded by a `+` sign, and is then added to the total of the tests"; L1632 same | **agrees verbatim** |
| OD-12 (b) correction offsets, never overwrite | reference has no text on overwriting; L997 describes "Edits an amount already entered" | reference is silent → decision fills a silence |
| Percentage base = patient's total account amount | L976 says only "as a percentage"; no base stated | reference is silent → decision fills a silence |
| Enter: first computes only, second saves | L979 "the first `Enter` press performs the accounting operation … the second press saves"; L1633 "the first computes, the second saves" | **L1633 agrees; L979's "performs the accounting operation" is ambiguous** and is resolved by L1633. Recorded, not a contradiction. |
| Refund: new row, `Amount = 0`, capped by net paid | L1000 "Records a refund against the account" — no shape, no cap | reference is silent → decision fills a silence |
| Clear-account: void, keep rows | L1002 "Clears the account of the patient" — silent on deletion | reference is silent; the decision is compatible |
| OD-8 (a) GAP-22 on single-test **and** profile/culture | §3.5 GAP-22 lists the actions; the reference's 1.22.12 block sits under "Composite Test Entry Screen" | reference places the actions on the composite screen; the owner extended them to profile/culture/single-test for parity. **Extension, not contradiction.** |

One genuine tension is recorded for the owner's attention, not as a contradiction: L1443 places
النتيجة الرئيسية on the **composite** screen (1.22.12), while Stage 3 §3.5 also asks for it on the
single-test screen (GAP-22). If "the main result of the request" means the composite test's own report,
the same action cannot also be meaningful on a simple test. That is part of what D-06 asks.

---

## (j) Final verification (this machine, this round)

**Baseline at the start of Round 4** (HEAD `efc909c`): build **0 errors, 0 warnings**;
Domain 710, Application 1907, Infrastructure 527, Persistence 24 (+2 skipped), Presentation 298 —
**3466 passed, 2 skipped, 0 failed**.

> Note on the prompt's expected baseline (3456 total / 3454 passed): the measured baseline was 3468
> total / 3466 passed. The difference is 12 tests, which are the Round-3 characterization and
> percentage tests already in the tree. The prompt's figure was one run behind; the measured number is
> reported here rather than the expected one.

**Final at the end of Round 4** (HEAD unchanged):

| Suite | Passed | Skipped | Failed |
|---|---|---|---|
| TopLab.Domain.Tests | 710 | 0 | 0 |
| TopLab.Application.Tests | 1907 | 0 | 0 |
| TopLab.Infrastructure.Tests | 527 | 0 | 0 |
| TopLab.Persistence.Tests | 24 | 2 | 0 |
| TopLab.Presentation.Tests | 336 | 0 | 0 |
| **Total** | **3504** | **2** | **0** |

Build: `dotnet build TopLab.sln --no-incremental` → **0 errors, 0 warnings**.

Delta this round: **+38 tests** (10 filter, 9 refresh/copy, 2 patient data, 2 envelope, 12 GAP-22,
2 follow-up, 1 IsEnabled), 0 failures, 0 warnings before and after.

### Safety rules
- No `git commit` / `push` / `add` / `stash` / `reset` / `checkout` / `clean`. `HEAD` is
  `efc909c688ccd3375b6bf3bc15af617f3739c647` at start and end; the index is untouched.
- No migration created or applied. `20261025123435_AddRefundAmountToPaymentOperation` untouched.
  No database was touched.
- No permission code, seed row, or schema change was added.
- Every pre-existing file in `Docs/Audit/` is byte-identical: SHA-256 of all 21 files recorded before
  and after; zero changed. This evidence file is the only new file there.
- No WPF screen was executed or visually verified; every layout statement in §(g) is static evidence
  read from the XAML text.
- Every item recorded as a Decision Card was left unimplemented: D-01 (percentage behaviour), D-02
  (TM/TG/CG), D-03 (complete-test / test-group menus), D-04 (delivery list print/preview), D-05
  (delivery notes action), D-06 (main result).
