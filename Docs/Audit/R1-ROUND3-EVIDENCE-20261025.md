# R1 Round 3 — Evidence

Generated 2026-10-25. Static evidence only: the WPF screens were NOT executed or visually verified.
HEAD at start and end: `efc909c688ccd3375b6bf3bc15af617f3739c647` (unchanged).

## (a) git diff — all tracked modified files

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
index 83cdd47..50daabc 100644
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
@@ -245,44 +301,171 @@ public sealed class PatientAccountViewModel : ViewModelBase
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
+            PaymentDiscountText = _accountEntry.DiscountText;
+            OnPropertyChanged(nameof(PaymentDiscountText));
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
index 196c0c4..b87b2ef 100644
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
@@ -102,6 +107,9 @@ public sealed class PatientEditorViewModel : ViewModelBase
     private string? _notes;
     private string _paymentAmountText = string.Empty;
     private string? _paymentDiscountText;
+    private string? _paymentPercentageText;
+    private string _refundAmountText = string.Empty;
+    private readonly AccountEntrySession _accountEntry = new();
     private decimal _totalCharged;
     private decimal _totalDiscount;
     private decimal _totalPaid;
@@ -144,11 +152,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
@@ -174,6 +186,8 @@ public sealed class PatientEditorViewModel : ViewModelBase
         ClearSearchCommand = new RelayCommand(ClearSearch);
         OpenSearchResultCommand = new AsyncRelayCommand(async (param, ct) => await OpenSearchResultAsync(param as PatientSummaryDto, ct));
         OpenAccountCommand = new AsyncRelayCommand(async (_, ct) => await OpenAccountAsync(ct));
+        PatientWindowCommand = new AsyncRelayCommand((_, ct) => PatientWindowAsync(ct));
+        LabResultsCommand = new AsyncRelayCommand(async (_, ct) => await LabResultsAsync(ct));
         AddProfileCommand = new AsyncRelayCommand(async (_, ct) => await AddProfileAsync(ct));
         AddCustomGroupCommand = new AsyncRelayCommand(async (_, ct) => await AddCustomGroupAsync(ct));
         ClearAllVisitTestsCommand = new AsyncRelayCommand(async (_, ct) => await ClearAllVisitTestsAsync(ct));
@@ -537,9 +551,47 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
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
+
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
 
-    public string? PaymentDiscountText { get => _paymentDiscountText; set => SetProperty(ref _paymentDiscountText, value); }
+    /// <summary>1.3.2.5 «استرداد» amount entry.</summary>
+    public string RefundAmountText { get => _refundAmountText; set => SetProperty(ref _refundAmountText, value); }
 
     public decimal TotalCharged { get => _totalCharged; private set => SetProperty(ref _totalCharged, value); }
 
@@ -626,6 +678,15 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
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
@@ -636,6 +697,7 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     /// <summary>Phase 1 REF-068: laboratory-order slip print action.</summary>
     public AsyncRelayCommand PrintLabOrderCommand { get; }
+    public AsyncRelayCommand PrintEnvelopeCommand { get; }
 
     public RelayCommand AddTestToSelectionCommand { get; }
 
@@ -653,6 +715,8 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     /// <summary>S-03 Slice 4: totals-row entry into the patient account screen.</summary>
     public AsyncRelayCommand OpenAccountCommand { get; }
+    public AsyncRelayCommand PatientWindowCommand { get; }
+    public AsyncRelayCommand LabResultsCommand { get; }
 
     public AsyncRelayCommand AddProfileCommand { get; }
 
@@ -1488,6 +1552,37 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
     /// <summary>S-03 Slice 3: wires the orphaned AddProfileToVisitCommand (edit mode only).</summary>
     private async Task AddProfileAsync(CancellationToken cancellationToken)
     {
@@ -1617,6 +1712,13 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
@@ -1627,38 +1729,178 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
+            PaymentDiscountText = _accountEntry.DiscountText;
+            OnPropertyChanged(nameof(PaymentDiscountText));
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
         {
-            ErrorMessage = "قيمة المدفوع غير صالحة.";
+            // First press: figures only, nothing recorded.
+            StatusMessage = step.StatusMessage!;
             return;
         }
 
-        decimal? discount = null;
-        if (!string.IsNullOrWhiteSpace(PaymentDiscountText))
+        await RecordPaymentAsync(cancellationToken);
+    }
+
+    /// <summary>1.3.2.5 «استرداد»: records a refund of the entered amount (never overwrites).</summary>
+    private async Task RefundAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!IsEditMode || !_patientId.HasValue)
         {
-            if (!decimal.TryParse(PaymentDiscountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) || d < 0)
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل تسجيل استرداد.";
+            return;
+        }
+
+        var parsed = AccountEntryParser.ParseRefundAmount(RefundAmountText);
+        if (!parsed.IsValid)
+        {
+            ErrorMessage = parsed.ErrorMessage!;
+            return;
+        }
+
+        IsBusy = true;
+        try
+        {
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
@@ -1695,6 +1937,36 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
index 38b2efc..cc992f6 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs
@@ -36,6 +36,9 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
         LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
         OpenHandoverCommand = new RelayCommand(param => OpenHandover(param as UndeliveredPatientRowDto));
         OpenGroupSlipsCommand = new RelayCommand(_ => OpenGroupSlips());
+        PatientAccountCommand = new RelayCommand(param => OpenPatientAccount(param as UndeliveredPatientRowDto), HasRow);
+        CombinedReportCommand = new RelayCommand(param => OpenCombinedReport(param as UndeliveredPatientRowDto), HasRow);
+        EmptyReportCommand = new RelayCommand(param => OpenEmptyReport(param as UndeliveredPatientRowDto), HasRow);
         BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientsHubViewModel>());
     }
 
@@ -54,12 +57,30 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
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
@@ -91,6 +112,9 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
     public AsyncRelayCommand LoadCommand { get; }
     public RelayCommand OpenHandoverCommand { get; }
     public RelayCommand OpenGroupSlipsCommand { get; }
+    public RelayCommand PatientAccountCommand { get; }
+    public RelayCommand CombinedReportCommand { get; }
+    public RelayCommand EmptyReportCommand { get; }
     public RelayCommand BackCommand { get; }
 
     public async Task LoadAsync(CancellationToken cancellationToken = default)
@@ -148,4 +172,34 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
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
index a0f52f8..907825d 100644
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
@@ -47,6 +65,9 @@
                 <Button Content="الإيصال" Margin="4" Padding="14,2" Command="{Binding PrintReceiptCommand}" />
                 <Button Content="الفاتورة" Margin="4" Padding="14,2" Command="{Binding PrintInvoiceCommand}" />
                 <Button Content="طلب تحاليل" Margin="4" Padding="14,2" Command="{Binding PrintLabOrderCommand}" />
+<Button Content="مغلف" Margin="4" Padding="14,2" Command="{Binding PrintEnvelopeCommand}" />
+  <Button Content="نافذة المرضى" Margin="4" Padding="14,2" Command="{Binding PatientWindowCommand}" />
+  <Button Content="نتائج التحاليل" Margin="4" Padding="14,2" Command="{Binding LabResultsCommand}" />
             </WrapPanel>
             <TextBlock Text="{Binding ErrorMessage}" Foreground="Red" TextWrapping="Wrap" Margin="4,2" />
             <TextBlock Text="{Binding StatusMessage}" Foreground="#1E3A5F" TextWrapping="Wrap" Margin="4,0" />
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
index ae0a7c3..d0aca5d 100644
--- a/src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml
@@ -10,6 +10,9 @@
             <TextBlock Text="إلى:" VerticalAlignment="Center" Margin="0,0,6,0"/>
             <DatePicker SelectedDate="{Binding To, UpdateSourceTrigger=PropertyChanged}" Width="140" Margin="0,0,8,0"/>
             <Button Content="عرض" Width="70" Margin="0,0,12,0" Command="{Binding LoadCommand}"/>
+            <Button Content="حساب المريض" Width="90" Margin="0,0,8,0" Command="{Binding PatientAccountCommand}" CommandParameter="{Binding SelectedItem}"/>
+            <Button Content="تقرير مجمع" Width="90" Margin="0,0,8,0" Command="{Binding CombinedReportCommand}" CommandParameter="{Binding SelectedItem}"/>
+            <Button Content="تقرير فارغ" Width="90" Margin="0,0,8,0" Command="{Binding EmptyReportCommand}" CommandParameter="{Binding SelectedItem}"/>
             <Button Content="قصاصة تقرير جماعية" Width="150" Margin="0,0,12,0" Command="{Binding OpenGroupSlipsCommand}"/>
             <Button Content="رجوع" Width="70" Command="{Binding BackCommand}"/>
         </StackPanel>
@@ -58,6 +61,7 @@
                 <DataGridTextColumn Header="التاريخ" Binding="{Binding RegistrationDateUtc, StringFormat={}{0:yyyy-MM-dd}}" Width="100"/>
                 <DataGridTextColumn Header="الحالة" Binding="{Binding VisitStatus}" Width="60"/>
                 <DataGridTextColumn Header="غير المسلَّم" Binding="{Binding UndeliveredCount}" Width="80"/>
+                <DataGridTextColumn Header="ملاحظات" Binding="{Binding PatientNotes}" Width="120"/>
                 <DataGridTemplateColumn Header="تسليم" Width="70">
                     <DataGridTemplateColumn.CellTemplate>
                         <DataTemplate>
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

```

## (b) Untracked source and test files created in this round (and the Round-2b test files still untracked)


### src/TopLab.Domain/Billing/AccountEntryParser.cs

```csharp
using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Billing;

/// <summary>
/// What a parsed paid-amount entry means (canonical reference 1.3.1.4.1).
/// </summary>
public enum AccountEntryKind
{
    /// <summary>An unprefixed amount is the payment itself.</summary>
    Payment = 0,

    /// <summary>An amount prefixed with <c>+</c> is an addition to the total of the tests.</summary>
    Surcharge = 1
}

/// <summary>
/// Result of parsing one account-entry field. A failure carries the Arabic message the
/// screens show, so the wording lives in exactly one place (SD-17).
/// </summary>
public sealed record AccountEntryParseResult(bool IsValid, decimal Value, AccountEntryKind Kind, string? ErrorMessage)
{
    public static AccountEntryParseResult Ok(decimal value, AccountEntryKind kind = AccountEntryKind.Payment)
        => new(true, value, kind, null);

    public static AccountEntryParseResult Fail(string message)
        => new(false, 0m, AccountEntryKind.Payment, message);
}

/// <summary>
/// Pure parsing for the account-panel entry fields (canonical reference 1.3.1.2, 1.3.1.3,
/// 1.3.1.4, 1.3.1.4.1; owner decisions OD-11 and the Round 3 percentage decision).
/// <para>
/// This type exists so that every payment-entry screen parses identically. It performs no
/// I/O, holds no state, and introduces no schema change and no stored percentage.
/// </para>
/// </summary>
public static class AccountEntryParser
{
    /// <summary>Arabic message shown when a paid amount cannot be read as a number.</summary>
    public const string InvalidAmountMessage = "قيمة المدفوع غير صالحة.";

    /// <summary>Arabic message for a bare <c>+</c>, <c>+0</c> or a negative paid amount.</summary>
    public const string InvalidSurchargeMessage = "قيمة المبلغ الإضافي غير صالحة.";

    /// <summary>Arabic message for a <c>+</c> amount combined with a discount (1.3.1.4.1).</summary>
    public const string SurchargeWithDiscountMessage = "لا يمكن إضافة مبلغ إضافي مع خصم.";

    /// <summary>Arabic message for a discount value that cannot be read or is negative.</summary>
    public const string InvalidDiscountMessage = "قيمة الخصم غير صالحة.";

    /// <summary>Arabic message for a percentage outside the open range (0, 100].</summary>
    public const string InvalidPercentageMessage = "نسبة الخصم يجب أن تكون أكبر من صفر ولا تتجاوز 100.";

    private const char SurchargePrefix = '+';
    private const char MinusSign = '-';
    private const char ArabicDecimalSeparator = '\u066B';

    /// <summary>
    /// Parses the paid-amount field. A leading <c>+</c> (after trimming) selects the surcharge
    /// convention (1.3.1.4.1 / OD-11); anything else is an ordinary payment.
    /// </summary>
    public static AccountEntryParseResult ParsePaidAmount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Fail(InvalidAmountMessage);
        }

        var trimmed = text.Trim();
        var kind = AccountEntryKind.Payment;
        if (trimmed.Length > 0 && trimmed[0] == SurchargePrefix)
        {
            kind = AccountEntryKind.Surcharge;
            trimmed = trimmed[1..].Trim();
        }

        if (!TryParseDecimal(trimmed, out var value))
        {
            return AccountEntryParseResult.Fail(kind == AccountEntryKind.Surcharge ? InvalidSurchargeMessage : InvalidAmountMessage);
        }

        if (value <= 0)
        {
            return AccountEntryParseResult.Fail(kind == AccountEntryKind.Surcharge ? InvalidSurchargeMessage : InvalidAmountMessage);
        }

        return AccountEntryParseResult.Ok(value, kind);
    }

    /// <summary>
    /// Parses the value-discount field. An empty or whitespace entry means "no discount"
    /// and is reported as a valid zero.
    /// </summary>
    public static AccountEntryParseResult ParseDiscount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Ok(0m);
        }

        if (!TryParseDecimal(text.Trim(), out var value) || value < 0)
        {
            return AccountEntryParseResult.Fail(InvalidDiscountMessage);
        }

        return AccountEntryParseResult.Ok(value);
    }

    /// <summary>
    /// Parses the percentage-discount field (1.3.1.3). An empty entry means "no percentage".
    /// The valid range is greater than 0 and at most 100.
    /// </summary>
    public static AccountEntryParseResult ParsePercentage(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Ok(0m);
        }

        if (!TryParseDecimal(text.Trim().TrimEnd('%'), out var value) || value <= 0 || value > 100m)
        {
            return AccountEntryParseResult.Fail(InvalidPercentageMessage);
        }

        return AccountEntryParseResult.Ok(value);
    }

    /// <summary>
    /// Parses a refund amount (1.3.2.5). Must be greater than zero; the upper bound against
    /// the patient's net paid total is enforced by the refund handler, not here.
    /// </summary>
    public static AccountEntryParseResult ParseRefundAmount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Fail(InvalidAmountMessage);
        }

        if (!TryParseDecimal(text.Trim(), out var value) || value <= 0)
        {
            return AccountEntryParseResult.Fail(InvalidAmountMessage);
        }

        return AccountEntryParseResult.Ok(value);
    }

    /// <summary>
    /// Reads a decimal from user text. Latin digits, Arabic-Indic digits (U+0660–U+0669) and
    /// their extended form (U+06F0–U+06F9) are all accepted, as is the Arabic decimal
    /// separator, because the operator may be typing on an Arabic keyboard.
    /// </summary>
    public static bool TryParseDecimal(string? text, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = NormalizeDigits(text.Trim()).Replace(ArabicDecimalSeparator, '.');
        if (normalized.Length == 0)
        {
            return false;
        }

        return decimal.TryParse(
            normalized,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }

    /// <summary>
    /// Maps Arabic-Indic and extended Arabic-Indic digits onto ASCII digits so a single
    /// invariant parser can read every keyboard.
    /// </summary>
    public static string NormalizeDigits(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var buffer = new char[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            buffer[i] = c switch
            {
                >= '\u0660' and <= '\u0669' => (char)(c - '\u0660' + '0'),
                >= '\u06F0' and <= '\u06F9' => (char)(c - '\u06F0' + '0'),
                _ => c
            };
        }

        return new string(buffer);
    }

    /// <summary>True when the entry carries the surcharge prefix and nothing else.</summary>
    public static bool IsSurchargePrefixOnly(string? text)
        => !string.IsNullOrWhiteSpace(text)
           && text.Trim().Length == 1
           && text.Trim()[0] == SurchargePrefix;

    /// <summary>True when the entry starts with the surcharge prefix.</summary>
    public static bool HasSurchargePrefix(string? text)
        => !string.IsNullOrWhiteSpace(text) && text.Trim()[0] == SurchargePrefix;

    /// <summary>True when the entry starts with a minus sign (always rejected).</summary>
    public static bool HasMinusSign(string? text)
        => !string.IsNullOrWhiteSpace(text) && text.Trim()[0] == MinusSign;
}

/// <summary>
/// Pure calculations behind the account panel's first <c>Enter</c> press (1.3.1.4.2) and the
/// percentage field (1.3.1.3). No I/O, no state, no schema change.
/// </summary>
public static class AccountEntryCalculator
{
    /// <summary>
    /// Converts a discount percentage into an amount. The percentage is always calculated
    /// from the patient's TOTAL account amount (the total charged value of all the patient's
    /// tests), per the owner's Round 3 decision, and rounded to two decimals away from zero.
    /// </summary>
    public static decimal DiscountAmountFromPercentage(decimal totalAccountAmount, decimal percentage)
    {
        if (percentage <= 0m || percentage > 100m)
        {
            throw new ArgumentException("Percentage must be greater than 0 and at most 100.", nameof(percentage));
        }

        if (totalAccountAmount <= 0m)
        {
            return 0m;
        }

        return Round2(totalAccountAmount * percentage / 100m);
    }

    /// <summary>Rounds to two decimals away from zero (the money convention of the panel).</summary>
    public static decimal Round2(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The figures the first <c>Enter</c> press shows in the status line, before anything is
    /// recorded: the amount about to be paid, the discount about to be applied, and the
    /// balance that would remain.
    /// </summary>
    public static AccountEntryPreview Preview(
        decimal totalCharged,
        decimal alreadyPaid,
        decimal amount,
        decimal discount,
        AccountEntryKind kind)
    {
        if (kind == AccountEntryKind.Surcharge)
        {
            // A surcharge raises the charged side (1.3.1.4.1) and carries no discount.
            var chargedAfter = totalCharged + amount;
            return new AccountEntryPreview(0m, 0m, chargedAfter - alreadyPaid, chargedAfter);
        }

        var paidAfter = alreadyPaid + amount + discount;
        return new AccountEntryPreview(amount, discount, totalCharged - paidAfter, totalCharged);
    }

    /// <summary>
    /// The highest refund the patient may still receive: the net total actually paid after
    /// earlier refunds and voided operations (never negative).
    /// </summary>
    public static decimal MaxRefundable(IReadOnlyList<PaymentOperation> operations)
    {
        var net = PatientAccountCalculator.TotalPaid(operations);
        return net > 0m ? net : 0m;
    }

    /// <summary>Human-readable type name used by the confirmation/status lines.</summary>
    public static string Describe(OperationType operationType) => operationType switch
    {
        OperationType.Refund => "استرداد",
        OperationType.Correction => "تعديل",
        OperationType.FullSettlement => "تسوية كاملة",
        _ => "دفع"
    };
}

/// <summary>Figures shown by the first <c>Enter</c> press; nothing is persisted to produce them.</summary>
public sealed record AccountEntryPreview(
    decimal Amount,
    decimal Discount,
    decimal BalanceAfter,
    decimal TotalChargedAfter);

```


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
        _discountText = amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
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


### src/TopLab.Application/Features/PatientBilling/Commands/RecordRefund/RecordRefundCommand.cs

```csharp
using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordRefund;

/// <summary>
/// Records a refund against the patient's account (canonical reference 1.3.2.5).
/// <para>
/// Ungated exactly like <see cref="Commands.RecordPayment.RecordPaymentCommand"/>: the
/// reference establishes the action, not a new permission, and the round forbids adding
/// permission codes. The refund is a new operation of <c>OperationType.Refund</c> carrying
/// <c>RefundAmount</c> with <c>Amount = 0</c>, as the Domain enforces — a stored amount is
/// never overwritten (OD-12 option b).
/// </para>
/// </summary>
public sealed record RecordRefundCommand(
    int PatientId,
    decimal RefundAmount) : IRequest<Result<int>>;

```


### src/TopLab.Application/Features/PatientBilling/Commands/RecordRefund/RecordRefundCommandValidator.cs

```csharp
using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordRefund;

public sealed class RecordRefundCommandValidator : AbstractValidator<RecordRefundCommand>
{
    public RecordRefundCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.RefundAmount).GreaterThan(0).WithMessage("مبلغ الاسترداد يجب أن يكون أكبر من صفر.");
    }
}

```


### src/TopLab.Application/Features/PatientBilling/Commands/RecordRefund/RecordRefundCommandHandler.cs

```csharp
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordRefund;

/// <summary>
/// Records a refund (canonical reference 1.3.2.5, «استرداد»). The operation is a new
/// <c>OperationType.Refund</c> row carrying <c>RefundAmount</c> with <c>Amount = 0</c>
/// (the Domain's rule), so nothing stored is overwritten (OD-12 option b).
/// <para>
/// The refund may not exceed the patient's net total actually paid — that is
/// <c>PatientAccountCalculator.TotalPaid</c> after earlier refunds and voided operations —
/// so repeated refunds can never drive the account below zero.
/// </para>
/// </summary>
public sealed class RecordRefundCommandHandler : IRequestHandler<RecordRefundCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RecordRefundCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(RecordRefundCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        var operations = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == request.PatientId)
            .ToList();

        var maxRefundable = AccountEntryCalculator.MaxRefundable(operations);
        if (request.RefundAmount > maxRefundable)
        {
            return Result<int>.Failure(Error.Validation(
                $"مبلغ الاسترداد يتجاوز إجمالي المدفوع الصافي ({maxRefundable})."));
        }

        PaymentOperation refund;
        try
        {
            refund = PaymentOperation.Create(
                PaymentOperationId.Create(0),
                PatientId.Create(request.PatientId),
                amount: 0m,
                receivedByUserId: _currentUser.UserId,
                operationAtUtc: _clock.UtcNow,
                discountAmount: null,
                isExtraCharge: false,
                operationType: OperationType.Refund,
                refundAmount: request.RefundAmount);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(refund);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(refund.Id.Value);
    }
}

```


### src/TopLab.Application/Features/PatientBilling/Commands/ClearPatientAccount/ClearPatientAccountCommand.cs

```csharp
using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;

/// <summary>
/// Clears the patient's account (canonical reference 1.3.2.7, «تصفير الحساب»).
/// <para>
/// Voids every non-voided operation of that patient through the existing
/// <c>IsVoided</c>/<c>Void()</c> mechanism. Nothing is hard-deleted, the rows and the audit
/// trail stay, and other patients are untouched. Idempotent: when nothing remains to void the
/// command still succeeds. Ungated like <c>RecordPaymentCommand</c> (no new permission code).
/// </para>
/// </summary>
public sealed record ClearPatientAccountCommand(
    int PatientId) : IRequest<Result<int>>;

```


### src/TopLab.Application/Features/PatientBilling/Commands/ClearPatientAccount/ClearPatientAccountCommandValidator.cs

```csharp
using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;

public sealed class ClearPatientAccountCommandValidator : AbstractValidator<ClearPatientAccountCommand>
{
    public ClearPatientAccountCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}

```


### src/TopLab.Application/Features/PatientBilling/Commands/ClearPatientAccount/ClearPatientAccountCommandHandler.cs

```csharp
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;

/// <summary>
/// Clears the account (canonical reference 1.3.2.7, «تصفير الحساب») by voiding every
/// non-voided operation of the patient.
/// <para>
/// Settled pins honoured: no hard delete (rows and audit trail survive), only this patient's
/// operations are touched, already-voided rows are left alone, and the command is idempotent —
/// an account with nothing left to void returns success with a count of zero. The returned
/// value is the number of operations actually voided, which the screen reports.
/// </para>
/// </summary>
public sealed class ClearPatientAccountCommandHandler : IRequestHandler<ClearPatientAccountCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public ClearPatientAccountCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(ClearPatientAccountCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        var operations = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == request.PatientId && !o.IsVoided)
            .ToList();

        foreach (var operation in operations)
        {
            operation.Void();
        }

        if (operations.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(operations.Count);
    }
}

```


### tests/TopLab.Domain.Tests/Billing/AccountEntryParserTests.cs

```csharp
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 3 B1/B2/B3: the shared account-entry parsing and calculation code
/// (canonical reference 1.3.1.2, 1.3.1.3, 1.3.1.4, 1.3.1.4.1, 1.3.1.4.2).
/// Pure Domain tests — no I/O, no screen.
/// </summary>
public class AccountEntryParserTests
{
    // ============================================================
    // Paid amount and the "+" surcharge convention (1.3.1.4.1)
    // ============================================================

    [Theory]
    [InlineData("100", 100)]
    [InlineData(" 100 ", 100)]
    [InlineData("100.50", 100.50)]
    [InlineData("0.01", 0.01)]
    public void ParsePaidAmount_PlainAmount_IsAPayment(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
        Assert.Equal(AccountEntryKind.Payment, result.Kind);
    }

    [Theory]
    [InlineData("+100", 100)]
    [InlineData(" + 100 ", 100)]
    [InlineData("+0.5", 0.5)]
    public void ParsePaidAmount_PlusPrefix_IsASurcharge(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
        Assert.Equal(AccountEntryKind.Surcharge, result.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("+")]
    [InlineData("+0")]
    [InlineData("+abc")]
    public void ParsePaidAmount_Invalid_IsRejectedWithArabicMessage(string? text)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.False(result.IsValid);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void ParsePaidAmount_BarePlus_UsesTheSurchargeMessage()
    {
        var result = AccountEntryParser.ParsePaidAmount("+");

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidSurchargeMessage, result.ErrorMessage);
    }

    [Fact]
    public void ParsePaidAmount_ZeroSurcharge_UsesTheSurchargeMessage()
    {
        var result = AccountEntryParser.ParsePaidAmount("+0");

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidSurchargeMessage, result.ErrorMessage);
    }

    [Fact]
    public void ParsePaidAmount_Negative_UsesThePaymentMessage()
    {
        var result = AccountEntryParser.ParsePaidAmount("-5");

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidAmountMessage, result.ErrorMessage);
    }

    // ============================================================
    // Arabic-Indic digits
    // ============================================================

    [Theory]
    [InlineData("\u0661\u0660\u0660", 100)]          // ١٠٠
    [InlineData("\u0660\u066B\u0665", 0.5)]          // ٠٫٥
    [InlineData("\u06F1\u06F2\u06F3", 123)]          // ۱۲۳ (extended)
    public void ParsePaidAmount_ArabicIndicDigits_AreAccepted(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void NormalizeDigits_MapsBothArabicDigitRanges()
    {
        Assert.Equal("0123456789", AccountEntryParser.NormalizeDigits("\u0660\u0661\u0662\u0663\u0664\u0665\u0666\u0667\u0668\u0669"));
        Assert.Equal("0123456789", AccountEntryParser.NormalizeDigits("\u06F0\u06F1\u06F2\u06F3\u06F4\u06F5\u06F6\u06F7\u06F8\u06F9"));
    }

    // ============================================================
    // Discount value (1.3.1.2)
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ParseDiscount_Empty_MeansNoDiscount(string? text)
    {
        var result = AccountEntryParser.ParseDiscount(text);

        Assert.True(result.IsValid);
        Assert.Equal(0m, result.Value);
    }

    [Fact]
    public void ParseDiscount_Value_IsAccepted()
    {
        Assert.Equal(12.5m, AccountEntryParser.ParseDiscount("12.5").Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    public void ParseDiscount_Invalid_IsRejectedWithArabicMessage(string text)
    {
        var result = AccountEntryParser.ParseDiscount(text);

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidDiscountMessage, result.ErrorMessage);
    }

    // ============================================================
    // Discount percentage (1.3.1.3)
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParsePercentage_Empty_MeansNoPercentage(string? text)
    {
        var result = AccountEntryParser.ParsePercentage(text);

        Assert.True(result.IsValid);
        Assert.Equal(0m, result.Value);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("10", 10)]
    [InlineData("100", 100)]
    [InlineData("12.5", 12.5)]
    [InlineData("25%", 25)]
    public void ParsePercentage_InRange_IsAccepted(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePercentage(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("100.01")]
    [InlineData("101")]
    [InlineData("abc")]
    public void ParsePercentage_OutOfRange_IsRejectedWithArabicMessage(string text)
    {
        var result = AccountEntryParser.ParsePercentage(text);

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidPercentageMessage, result.ErrorMessage);
    }

    // ============================================================
    // Refund amount (1.3.2.5)
    // ============================================================

    [Theory]
    [InlineData("50", 50)]
    [InlineData("0.01", 0.01)]
    public void ParseRefundAmount_Positive_IsAccepted(string text, decimal expected)
    {
        var result = AccountEntryParser.ParseRefundAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData(null)]
    public void ParseRefundAmount_ZeroOrNegativeOrUnreadable_IsRejected(string? text)
    {
        var result = AccountEntryParser.ParseRefundAmount(text);

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidAmountMessage, result.ErrorMessage);
    }

    // ============================================================
    // Prefix helpers
    // ============================================================

    [Theory]
    [InlineData("+", true)]
    [InlineData("+5", false)]
    [InlineData("5", false)]
    [InlineData(null, false)]
    public void IsSurchargePrefixOnly_DetectsABarePlus(string? text, bool expected)
        => Assert.Equal(expected, AccountEntryParser.IsSurchargePrefixOnly(text));

    [Theory]
    [InlineData("+5", true)]
    [InlineData("+ 5", true)]
    [InlineData("5", false)]
    [InlineData(null, false)]
    public void HasSurchargePrefix_DetectsThePrefix(string? text, bool expected)
        => Assert.Equal(expected, AccountEntryParser.HasSurchargePrefix(text));
}

/// <summary>R1 Round 3 B1: the percentage-to-amount conversion and the first-press figures.</summary>
public class AccountEntryCalculatorTests
{
    [Fact]
    public void DiscountAmountFromPercentage_IsOfTheTotalAccountAmount()
    {
        // 10% of the patient's TOTAL account amount (200) is 20.
        Assert.Equal(20m, AccountEntryCalculator.DiscountAmountFromPercentage(200m, 10m));
    }

    [Fact]
    public void DiscountAmountFromPercentage_RoundsAwayFromZeroToTwoDecimals()
    {
        // 33.333% of 100 = 33.333 -> 33.33; 1% of 0.005 = 0.00005 -> 0.00
        Assert.Equal(33.33m, AccountEntryCalculator.DiscountAmountFromPercentage(100m, 33.333m));
        // 12.5% of 10.01 = 1.25125 -> 1.25
        Assert.Equal(1.25m, AccountEntryCalculator.DiscountAmountFromPercentage(10.01m, 12.5m));
        // 50% of 0.05 = 0.025 -> 0.03 (away from zero, not banker's rounding)
        Assert.Equal(0.03m, AccountEntryCalculator.DiscountAmountFromPercentage(0.05m, 50m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void DiscountAmountFromPercentage_OutOfRange_Throws(decimal percentage)
    {
        Assert.Throws<ArgumentException>(() => AccountEntryCalculator.DiscountAmountFromPercentage(100m, percentage));
    }

    [Fact]
    public void DiscountAmountFromPercentage_NoChargedAmount_YieldsZero()
    {
        Assert.Equal(0m, AccountEntryCalculator.DiscountAmountFromPercentage(0m, 10m));
    }

    [Fact]
    public void Preview_Payment_ShowsAmountDiscountAndRemainingBalance()
    {
        var preview = AccountEntryCalculator.Preview(200m, 50m, 100m, 20m, AccountEntryKind.Payment);

        Assert.Equal(100m, preview.Amount);
        Assert.Equal(20m, preview.Discount);
        Assert.Equal(200m - (50m + 100m + 20m), preview.BalanceAfter);
    }

    [Fact]
    public void Preview_Surcharge_RaisesTheChargedSideAndCarriesNoDiscount()
    {
        var preview = AccountEntryCalculator.Preview(200m, 50m, 30m, 0m, AccountEntryKind.Surcharge);

        Assert.Equal(0m, preview.Amount);
        Assert.Equal(0m, preview.Discount);
        Assert.Equal(230m, preview.TotalChargedAfter);
        Assert.Equal(180m, preview.BalanceAfter);
    }

    [Fact]
    public void MaxRefundable_IsNetPaidAfterEarlierRefundsAndVoids()
    {
        var operations = new List<PaymentOperation>
        {
            PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 100m, 7, DateTime.UtcNow),
            PaymentOperation.Create(PaymentOperationId.Create(2), PatientId.Create(1), 50m, 7, DateTime.UtcNow)
        };
        var refund = PaymentOperation.Create(
            PaymentOperationId.Create(3), PatientId.Create(1), 0m, 7, DateTime.UtcNow,
            null, false, OperationType.Refund, 30m);
        operations.Add(refund);

        var voided = PaymentOperation.Create(PaymentOperationId.Create(4), PatientId.Create(1), 40m, 7, DateTime.UtcNow);
        voided.Void();
        operations.Add(voided);

        // Paid = 100 + 50 - 30 (refund) = 120; the voided 40 is ignored.
        Assert.Equal(120m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void MaxRefundable_NeverNegative()
    {
        var operations = new List<PaymentOperation>
        {
            PaymentOperation.Create(
                PaymentOperationId.Create(1), PatientId.Create(1), 0m, 7, DateTime.UtcNow,
                null, false, OperationType.Refund, 100m)
        };

        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(operations));
    }
}

```


### tests/TopLab.Domain.Tests/Billing/AccountEntrySessionTests.cs

```csharp
using TopLab.Domain.Billing;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 3 B3: the two-press <c>Enter</c> state machine (canonical reference 1.3.1.4.2:
/// "the first press performs the accounting operation for the patient; the second press saves
/// the operation"). <c>ShouldPersist</c> is the only gate a ViewModel may use, so proving it
/// false on the first press proves the first press cannot write to the database.
/// </summary>
public class AccountEntrySessionTests
{
    [Fact]
    public void FirstPress_ComputesAndDisplays_ButNeverPersists()
    {
        var session = new AccountEntrySession { AmountText = "100" };

        var step = session.PressEnter(totalCharged: 200m, alreadyPaid: 0m);

        Assert.True(step.IsValid);
        Assert.False(step.ShouldPersist);
        Assert.Equal(AccountEntryStage.Previewed, step.Stage);
        Assert.NotNull(step.Preview);
        Assert.Equal(100m, step.Preview!.Amount);
        Assert.False(string.IsNullOrWhiteSpace(step.StatusMessage));
    }

    [Fact]
    public void SecondPress_WithUnchangedInputs_Persists()
    {
        var session = new AccountEntrySession { AmountText = "100" };

        var first = session.PressEnter(200m, 0m);
        var second = session.PressEnter(200m, 0m);

        Assert.False(first.ShouldPersist);
        Assert.True(second.ShouldPersist);
        Assert.Equal(AccountEntryStage.ReadyToPersist, second.Stage);
    }

    [Fact]
    public void SecondPress_AfterAnEdit_ReturnsToTheFirstPressState()
    {
        var session = new AccountEntrySession { AmountText = "100" };
        session.PressEnter(200m, 0m);

        session.AmountText = "120";
        var afterEdit = session.PressEnter(200m, 0m);

        Assert.False(afterEdit.ShouldPersist);
        Assert.Equal(AccountEntryStage.Previewed, afterEdit.Stage);
    }

    [Fact]
    public void SecondPress_AfterADiscountEdit_ReturnsToTheFirstPressState()
    {
        var session = new AccountEntrySession { AmountText = "100" };
        session.PressEnter(200m, 0m);

        session.DiscountText = "10";
        var afterEdit = session.PressEnter(200m, 0m);

        Assert.False(afterEdit.ShouldPersist);
    }

    [Fact]
    public void ThirdPress_AfterPersisting_IsAFirstPressAgain()
    {
        var session = new AccountEntrySession { AmountText = "100" };
        session.PressEnter(200m, 0m);
        Assert.True(session.PressEnter(200m, 0m).ShouldPersist);

        session.MarkPersisted();
        var next = session.PressEnter(200m, 0m);

        Assert.False(next.ShouldPersist);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("+")]
    public void InvalidInput_ReportsAnErrorAndStaysInTheFirstPressState(string text)
    {
        var session = new AccountEntrySession { AmountText = text };

        var step = session.PressEnter(200m, 0m);

        Assert.False(step.IsValid);
        Assert.False(step.ShouldPersist);
        Assert.False(string.IsNullOrWhiteSpace(step.ErrorMessage));
        Assert.Equal(AccountEntryStage.Idle, step.Stage);
    }

    [Fact]
    public void InvalidInput_ThenValidInput_StillRequiresTwoPresses()
    {
        var session = new AccountEntrySession { AmountText = "abc" };
        session.PressEnter(200m, 0m);

        session.AmountText = "100";
        var first = session.PressEnter(200m, 0m);
        var second = session.PressEnter(200m, 0m);

        Assert.False(first.ShouldPersist);
        Assert.True(second.ShouldPersist);
    }

    // ============================================================
    // Surcharge (1.3.1.4.1 / OD-11)
    // ============================================================

    [Fact]
    public void PlusPrefix_IsReportedAsASurcharge()
    {
        var session = new AccountEntrySession { AmountText = "+30" };

        var step = session.PressEnter(200m, 50m);

        Assert.True(step.IsValid);
        Assert.Equal(AccountEntryKind.Surcharge, step.Kind);
        Assert.Equal(30m, step.Amount);
        Assert.Equal(230m, step.Preview!.TotalChargedAfter);
    }

    [Fact]
    public void SurchargeWithDiscount_IsRejected()
    {
        var session = new AccountEntrySession { AmountText = "+30", DiscountText = "5" };

        var step = session.PressEnter(200m, 50m);

        Assert.False(step.IsValid);
        Assert.Equal(AccountEntryParser.SurchargeWithDiscountMessage, step.ErrorMessage);
    }

    // ============================================================
    // Discount rules preserved
    // ============================================================

    [Fact]
    public void DiscountGreaterThanTheAmount_IsRejected_TheDomainRuleIsUnchanged()
    {
        var session = new AccountEntrySession { AmountText = "100", DiscountText = "150" };

        var step = session.PressEnter(200m, 0m);

        Assert.False(step.IsValid);
    }

    // ============================================================
    // Percentage (1.3.1.3)
    // ============================================================

    [Fact]
    public void ApplyPercentage_WritesTheConvertedAmountIntoTheValueField()
    {
        var session = new AccountEntrySession { PercentageText = "10" };

        var error = session.ApplyPercentage(totalAccountAmount: 200m);

        Assert.Null(error);
        Assert.Equal("20", session.DiscountText);
    }

    [Fact]
    public void ApplyPercentage_OutOfRange_ReturnsTheArabicMessage()
    {
        var session = new AccountEntrySession { PercentageText = "101" };

        var error = session.ApplyPercentage(200m);

        Assert.Equal(AccountEntryParser.InvalidPercentageMessage, error);
        Assert.Null(session.DiscountText);
    }

    [Fact]
    public void EditingTheValueField_ClearsThePercentage()
    {
        var session = new AccountEntrySession { PercentageText = "10" };

        session.DiscountText = "20";

        Assert.Null(session.PercentageText);
    }

    [Fact]
    public void EditingThePercentageField_ClearsTheValue()
    {
        var session = new AccountEntrySession { DiscountText = "20" };

        session.PercentageText = "10";

        Assert.Null(session.DiscountText);
    }
}

```


### tests/TopLab.Application.Tests/Features/PatientBilling/RecordRefundCommandHandlerTests.cs

```csharp
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordRefund;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

/// <summary>
/// R1 Round 3 B4: the refund handler (canonical reference 1.3.2.5, «استرداد»).
/// The refund is a new <c>OperationType.Refund</c> row carrying <c>RefundAmount</c> with
/// <c>Amount = 0</c>; nothing stored is overwritten (OD-12 option b).
/// </summary>
public class RecordRefundCommandHandlerTests
{
    private const int PatientIdValue = 1;
    private const int UserIdValue = 7;

    private static FakeApplicationDbContext SeedPatient(int id = PatientIdValue, bool deleted = false)
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(id), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        if (deleted)
        {
            patient.SoftDelete();
        }

        db.Patients.Add(patient);
        return db;
    }

    private static void SeedPayment(FakeApplicationDbContext db, decimal amount, int patientId = PatientIdValue, bool voided = false)
    {
        var operation = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(patientId), amount, UserIdValue, DateTime.UtcNow);
        if (voided)
        {
            operation.Void();
        }

        db.PaymentOperations.Add(operation);
    }

    private static void SeedRefund(FakeApplicationDbContext db, decimal amount, int patientId = PatientIdValue)
    {
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(patientId), 0m, UserIdValue, DateTime.UtcNow,
            null, false, OperationType.Refund, amount));
    }

    private static RecordRefundCommandHandler Handler(FakeApplicationDbContext db)
        => new(db, new FakeCurrentUserService { UserId = UserIdValue }, new FakeDateTimeProvider());

    [Fact]
    public async Task Success_RecordsARefundWithAmountZeroAndTheRefundAmount()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 40m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var refund = Assert.Single(db.PaymentOperations, o => o.OperationType == OperationType.Refund);
        Assert.Equal(0m, refund.Amount);
        Assert.Equal(40m, refund.RefundAmount);
        Assert.Equal(OperationType.Refund, refund.OperationType);
        Assert.Equal(UserIdValue, refund.ReceivedByUserId);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task RefundExactlyEqualToNetPaid_IsAccepted()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 100m), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OverRefund_IsRejected_WithTheNetPaidInTheMessage()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 100.01m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Contains("100", result.Error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(db.PaymentOperations, o => o.OperationType == OperationType.Refund);
    }

    [Fact]
    public async Task EarlierRefunds_ReduceWhatMayStillBeRefunded()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);
        SeedRefund(db, 60m);

        // Net paid is 40, so 41 must be refused and 40 accepted.
        Assert.False((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 41m), CancellationToken.None)).IsSuccess);
        Assert.True((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 40m), CancellationToken.None)).IsSuccess);
    }

    [Fact]
    public async Task VoidedOperations_AreIgnoredWhenComputingTheCeiling()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);
        SeedPayment(db, 500m, voided: true);

        // Only the 100 survives, so 150 is refused.
        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 150m), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RefundWithNothingPaid_IsRejected()
    {
        var db = SeedPatient();

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 1m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ZeroOrNegativeAmount_IsRejectedByTheDomain(decimal amount)
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, amount), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task UnknownPatient_IsNotFound()
    {
        var db = SeedPatient();

        var result = await Handler(db).Handle(new RecordRefundCommand(999, 10m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_IsNotFound()
    {
        var db = SeedPatient(deleted: true);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 10m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task OtherPatientsOperations_DoNotRaiseThisPatientsCeiling()
    {
        var db = SeedPatient();
        SeedPayment(db, 50m);
        SeedPayment(db, 1000m, patientId: 2);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 60m), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RecordRefund_IsNotGated_TheReferenceAddsNoPermissionCode()
    {
        Assert.IsNotAssignableFrom<TopLab.Application.Common.Authorization.IAuthorizedRequest>(
            new RecordRefundCommand(1, 10m));
    }

    [Fact]
    public async Task BalanceFormula_ReconcilesWithRefundsVoidsDiscountsAndSurchargesTogether()
    {
        var db = SeedPatient();
        var prices = new List<decimal> { 200m };

        // Payment of 100 with a 20 discount, a surcharge of 30, a voided payment of 500,
        // and a refund of 40.
        var payment = PaymentOperation.Create(
            PaymentOperationId.Create(1), PatientId.Create(PatientIdValue), 100m, UserIdValue, DateTime.UtcNow, 20m);
        var surcharge = PaymentOperation.Create(
            PaymentOperationId.Create(2), PatientId.Create(PatientIdValue), 30m, UserIdValue, DateTime.UtcNow, null, true);
        var voided = PaymentOperation.Create(
            PaymentOperationId.Create(3), PatientId.Create(PatientIdValue), 500m, UserIdValue, DateTime.UtcNow);
        voided.Void();
        var refund = PaymentOperation.Create(
            PaymentOperationId.Create(4), PatientId.Create(PatientIdValue), 0m, UserIdValue, DateTime.UtcNow,
            null, false, OperationType.Refund, 40m);

        var operations = new List<PaymentOperation> { payment, surcharge, voided, refund };

        // Charged = 200 + 30 = 230; Paid = (100 + 20) - 40 = 80; Balance = 150.
        Assert.Equal(230m, PatientAccountCalculator.TotalCharged(prices, operations));
        Assert.Equal(80m, PatientAccountCalculator.TotalPaid(operations));
        Assert.Equal(150m, PatientAccountCalculator.Balance(prices, operations));

        // The same figures come out of the read model the screens use.
        var account = TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount.PatientBillingReader.ReadAccount(db, db.Patients[0]);
        Assert.Equal(0m, account.TotalPaid);

        db.PaymentOperations.AddRange(operations);
        db.PatientTests.Add(TopLab.Domain.Results.PatientTest.Create(
            TopLab.Domain.Common.Ids.PatientTestId.Create(1), PatientId.Create(PatientIdValue),
            TopLab.Domain.Common.Ids.TestId.Create(1), 200m));

        var loaded = TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount.PatientBillingReader.ReadAccount(db, db.Patients[0]);
        Assert.Equal(230m, loaded.TotalCharged);
        Assert.Equal(80m, loaded.TotalPaid);
        Assert.Equal(150m, loaded.Balance);
    }
}

```


### tests/TopLab.Application.Tests/Features/PatientBilling/ClearPatientAccountCommandHandlerTests.cs

```csharp
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

/// <summary>
/// R1 Round 3 B5: clear-account (canonical reference 1.3.2.7, «تصفير الحساب»).
/// Every non-voided operation of the patient is voided through the existing mechanism:
/// nothing is hard-deleted, other patients are untouched, already-voided rows stay as they
/// are, and the command is idempotent.
/// </summary>
public class ClearPatientAccountCommandHandlerTests
{
    private const int PatientIdValue = 1;

    private static FakeApplicationDbContext SeedPatient(int id = PatientIdValue, bool deleted = false)
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(id), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        if (deleted)
        {
            patient.SoftDelete();
        }

        db.Patients.Add(patient);
        return db;
    }

    private static PaymentOperation AddOperation(
        FakeApplicationDbContext db, decimal amount, int patientId = PatientIdValue, bool voided = false)
    {
        var operation = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(patientId), amount, 7, DateTime.UtcNow);
        if (voided)
        {
            operation.Void();
        }

        db.PaymentOperations.Add(operation);
        return operation;
    }

    [Fact]
    public async Task Success_VoidsEveryNonVoidedOperationOfThePatient()
    {
        var db = SeedPatient();
        var first = AddOperation(db, 100m);
        var second = AddOperation(db, 50m);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.True(first.IsVoided);
        Assert.True(second.IsVoided);
    }

    [Fact]
    public async Task RowsSurvive_TheAuditTrailIsNotDeleted()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);

        await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Single(db.PaymentOperations);
    }

    [Fact]
    public async Task AlreadyVoidedOperations_AreLeftAloneAndNotCounted()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);
        var alreadyVoided = AddOperation(db, 50m, voided: true);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Equal(1, result.Value);
        Assert.True(alreadyVoided.IsVoided);
    }

    [Fact]
    public async Task OtherPatientsOperations_AreUntouched()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);
        var other = AddOperation(db, 500m, patientId: 2);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Equal(1, result.Value);
        Assert.False(other.IsVoided);
    }

    [Fact]
    public async Task Idempotent_WhenNothingRemains_StillSucceedsWithZero()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);
        var handler = new ClearPatientAccountCommandHandler(db);

        var first = await handler.Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);
        var second = await handler.Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value);
        Assert.True(second.IsSuccess);
        Assert.Equal(0, second.Value);
    }

    [Fact]
    public async Task EmptyAccount_SucceedsWithoutSaving()
    {
        var db = SeedPatient();

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownPatient_IsNotFound()
    {
        var db = SeedPatient();

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task SoftDeletedPatient_IsNotFound()
    {
        var db = SeedPatient(deleted: true);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public void ClearPatientAccount_IsNotGated_TheReferenceAddsNoPermissionCode()
    {
        Assert.IsNotAssignableFrom<TopLab.Application.Common.Authorization.IAuthorizedRequest>(
            new ClearPatientAccountCommand(1));
    }

    [Fact]
    public async Task AfterClearing_TheBalanceIsTheFullChargedAmount()
    {
        var db = SeedPatient();
        var prices = new List<decimal> { 200m };
        var operation = AddOperation(db, 100m);

        await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Equal(200m, PatientAccountCalculator.Balance(prices, new[] { operation }));
    }
}

```


### tests/TopLab.Presentation.Tests/Patients/ProfileCultureEntryNotesTests.cs

```csharp
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 3 A3 (GAP-05): <c>PatientNotes</c> is loaded on the profile and culture
/// result-entry screens and is empty when the DTO carries null.
///
/// Round 2b claimed these tests were impossible. They are not: <see cref="ProfileEntryViewModel"/>
/// and <see cref="CultureEntryViewModel"/> take several ports, but the ones
/// <c>LoadAsync</c> does not touch (<c>IApplicationDbContext</c>, <c>IPdfPreviewService</c>,
/// <c>IResultPrintCoordinator</c>, <c>IPrintedStateRecorder</c>) are passed as <c>null!</c>
/// deliberately — <c>LoadAsync</c> reads only the mediator and sets the properties asserted
/// here, so a null dependency can never be dereferenced on this path. Every dependency the
/// path does use is a real fake (<see cref="FakeSender"/>, <see cref="FakeDialogService"/>).
/// </summary>
public class ProfileCultureEntryNotesTests
{
    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private const int PatientTestId = 42;

    /// <summary>
    /// The three ports <c>LoadAsync</c> never touches on this path. They are supplied as
    /// explicit no-op fakes (never <c>null</c>) so the test cannot pass because of a missing
    /// dependency: if a future <c>LoadAsync</c> starts using one, the fake throws loudly.
    /// </summary>
    private sealed class UnusedDbContext : IApplicationDbContext
    {
        public IQueryable<TEntity> Set<TEntity>() where TEntity : class
            => throw new NotSupportedException("LoadAsync must not touch the database on this path.");
        public void Add<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Update<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class UnusedPdfPreview : IPdfPreviewService
    {
        public Task<Result<string>> PreviewAsync(string absolutePdfPath, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("LoadAsync must not preview on this path.");
    }

    private sealed class UnusedPrintCoordinator : IResultPrintCoordinator
    {
        public Task<ResultPrintOutcome> PrintAsync(int patientTestId, ResultPrintKind kind, CancellationToken ct = default)
            => throw new NotSupportedException("LoadAsync must not print on this path.");
    }

    private sealed class UnusedPrintedState : IPrintedStateRecorder
    {
        public Task<Result> RecordAsync(IReadOnlyCollection<int> patientTestIds, IReadOnlyCollection<int> profileResultItemIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result> RecordForPatientTestAsync(int patientTestId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static ProfileEntryGridDto ProfileGrid(string? notes) => new(
        PatientTestId: PatientTestId,
        PatientId: 7,
        PatientFullName: "أحمد محمد علي",
        ProfileName: "صورة دم كاملة",
        Items: Array.Empty<ProfileEntryItemDto>(),
        TestId: 3,
        PatientNotes: notes);

    private static CultureEntryGridDto CultureGrid(string? notes) => new(
        PatientTestId: PatientTestId,
        PatientId: 7,
        TestName: "مزرعة",
        TestCode: "CUL",
        Sample: null,
        OrganismA: null,
        OrganismB: null,
        OrganismC: null,
        CultureCondition: null,
        ColonyCount: null,
        Rows: Array.Empty<CultureSensitivityRowDto>(),
        ParentIsReviewed: false,
        ParentIsPrinted: false,
        Microscopy: null,
        PatientNotes: notes);

    private static ProfileEntryViewModel ProfileVm(string? notes)
    {
        var sender = new FakeSender();
        sender.WithResponse(
            new GetProfileEntryGridQuery(PatientTestId),
            Result<ProfileEntryGridDto>.Success(ProfileGrid(notes)));

        return new ProfileEntryViewModel(
            sender,
            new ResultErrorPresenter(),
            new FakeDialogService(),
            services: new StubServiceProvider(),
            pdfPreview: new UnusedPdfPreview(),
            db: new UnusedDbContext(),
            printCoordinator: new UnusedPrintCoordinator(),
            printedState: new UnusedPrintedState());
    }

    private static CultureEntryViewModel CultureVm(string? notes)
    {
        var sender = new FakeSender();
        sender.WithResponse(
            new GetCultureEntryGridQuery(PatientTestId),
            Result<CultureEntryGridDto>.Success(CultureGrid(notes)));

        return new CultureEntryViewModel(
            sender,
            new ResultErrorPresenter(),
            new FakeDialogService(),
            printCoordinator: new UnusedPrintCoordinator(),
            printedState: new UnusedPrintedState());
    }

    [Fact]
    public async Task ProfileEntry_PatientNotes_Loaded_WhenPresent()
    {
        var vm = ProfileVm("ملاحظات المريض");

        await vm.LoadAsync(PatientTestId);

        Assert.Equal("ملاحظات المريض", vm.PatientNotes);
    }

    [Fact]
    public async Task ProfileEntry_PatientNotes_Empty_WhenNull()
    {
        var vm = ProfileVm(null);

        await vm.LoadAsync(PatientTestId);

        Assert.Equal(string.Empty, vm.PatientNotes);
    }

    [Fact]
    public async Task CultureEntry_PatientNotes_Loaded_WhenPresent()
    {
        var vm = CultureVm("ملاحظات المريض");

        await vm.LoadAsync(PatientTestId);

        Assert.Equal("ملاحظات المريض", vm.PatientNotes);
    }

    [Fact]
    public async Task CultureEntry_PatientNotes_Empty_WhenNull()
    {
        var vm = CultureVm(null);

        await vm.LoadAsync(PatientTestId);

        Assert.Equal(string.Empty, vm.PatientNotes);
    }
}

```


### tests/TopLab.Presentation.Tests/Patients/PatientEditorRoundThreeTests.cs

```csharp
using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;
using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Commands.RecordRefund;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
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
/// R1 Round 3 A1/A3: the patient editor's navigation actions and its account-panel work.
/// <para>
/// The editor is driven through its real load path with a hand-rolled <see cref="ISender"/>
/// shim (the pattern <c>PatientEditorLabOrderTests</c> and
/// <c>PatientEditorClearAllGuardTests</c> already use), so nothing here instantiates a WPF
/// element (SD-19) and no private method is invoked directly.
/// </para>
/// </summary>
public class PatientEditorRoundThreeTests
{
    private const int PatientIdValue = 7;
    private static readonly DateTime Clock = new(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc);

    // ================================================================
    // Sender shim
    // ================================================================

    private sealed class EditorSender : ISender
    {
        public int EnvelopeCalls { get; private set; }
        public int PaymentCalls { get; private set; }
        public int ExtraChargeCalls { get; private set; }
        public int RefundCalls { get; private set; }
        public int ClearCalls { get; private set; }
        public List<RecordPaymentCommand> Payments { get; } = new();
        public Result? EnvelopeResult { get; set; }
        public Result<int>? RefundResult { get; set; }
        public Result<int>? ClearResult { get; set; }
        public decimal TotalCharged { get; set; } = 200m;
        public decimal TotalPaid { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetPatientByIdQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientDetailDto>.Success(Detail()));
                case GetPatientVisitHistoryQuery:
                    return Task.FromResult((TResponse)(object)Result<IReadOnlyList<VisitHistoryDto>>.Success(
                        new[] { Visit() }));
                case GetVisitWorkSheetQuery:
                    return Task.FromResult((TResponse)(object)Result<VisitWorkSheetDto>.Success(Sheet()));
                case GetPatientAccountQuery:
                    return Task.FromResult((TResponse)(object)Result<PatientAccountDto>.Success(Account()));
                case PrintEnvelopeCommand:
                    EnvelopeCalls++;
                    return Task.FromResult((TResponse)(object)(EnvelopeResult ?? Result.Success()));
                case RecordPaymentCommand payment:
                    PaymentCalls++;
                    Payments.Add(payment);
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                case RecordExtraChargeCommand:
                    ExtraChargeCalls++;
                    return Task.FromResult((TResponse)(object)Result<int>.Success(1));
                case RecordRefundCommand:
                    RefundCalls++;
                    return Task.FromResult((TResponse)(object)(RefundResult ?? Result<int>.Success(1)));
                case ClearPatientAccountCommand:
                    ClearCalls++;
                    return Task.FromResult((TResponse)(object)(ClearResult ?? Result<int>.Success(2)));
                default:
                    throw new NotSupportedException($"EditorSender has no canned response for {request.GetType().Name}.");
            }
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static PatientDetailDto Detail() => new(
            PatientId: PatientIdValue, LabId: "LAB-A", Title: null, FullName: "Patient 7",
            Sex: Sex.Male, AgeValue: 30, AgeUnit: AgeUnit.Year, NationalId: null, Address: null,
            AccountType: AccountType.Individual, IsVip: false, RegistrationDateUtc: Clock.AddHours(-2),
            PickupDateUtc: null, IsFastingIndicated: false, FastingHours: null, RecentContrastImaging: false,
            Notes: null, TreatingDoctorId: null, TreatingDoctorName: null, ReferralEntityId: null,
            ReferralEntityName: null, PhoneNumbers: Array.Empty<PatientPhoneNumberDto>(),
            MedicalConditions: Array.Empty<PatientMedicalConditionDto>(), IsDeleted: false);

        private static VisitHistoryDto Visit() => new(
            PatientId: PatientIdValue, LabId: "LAB-A", RegistrationDateUtc: Clock.AddHours(-2),
            Tests: Array.Empty<PatientTestSummaryDto>());

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

    /// <summary>Records which ViewModel the editor navigated to, including the generic overload.</summary>
    private sealed class RecordingNavigation : INavigationService
    {
        public List<Type> Targets { get; } = new();
        public ViewModelBase? CurrentViewModel { get; private set; }

        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            Targets.Add(typeof(TViewModel));
            CurrentViewModel = null;
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            Targets.Add(viewModel.GetType());
            CurrentViewModel = viewModel;
        }
    }

    private sealed class DecliningDialogService : IDialogService
    {
        public int ConfirmationCalls { get; private set; }
        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            ConfirmationCalls++;
            return Task.FromResult(false);
        }
        public Task ShowErrorAsync(string message) => Task.CompletedTask;
        public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(true);
        public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);
        public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null) => Task.FromResult<string?>(null);
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task<(PatientEditorViewModel Vm, EditorSender Sender, RecordingNavigation Nav)> LoadedAsync(
        IDialogService? dialogs = null, EditorSender? sender = null)
    {
        sender ??= new EditorSender();
        var nav = new RecordingNavigation();
        var vm = new PatientEditorViewModel(
            sender, nav, new ResultErrorPresenter(), dialogs ?? new FakeDialogService(), new StubServiceProvider());
        await vm.LoadPatientAsync(PatientIdValue);
        return (vm, sender, nav);
    }

    private static PatientEditorViewModel Unsaved(IDialogService? dialogs = null, EditorSender? sender = null)
        => new(sender ?? new EditorSender(), new RecordingNavigation(), new ResultErrorPresenter(),
            dialogs ?? new FakeDialogService(), new StubServiceProvider());

    /// <summary>AsyncRelayCommand.Execute is async void, so the command is awaited by yielding.</summary>
    private static async Task RunAsync(AsyncRelayCommand command)
    {
        command.Execute(null);
        await Task.Yield();
        await Task.Delay(80);
    }

    // ================================================================
    // A1 — the patient list opens in add mode and in edit mode
    // ================================================================

    [Fact]
    public async Task PatientWindow_OpensTheList_WhenThePatientIsSaved()
    {
        var (vm, _, nav) = await LoadedAsync();

        await RunAsync(vm.PatientWindowCommand);

        Assert.Contains(typeof(PatientSearchViewModel), nav.Targets);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public void PatientWindow_OpensTheList_WhenThePatientIsNotSaved()
    {
        var vm = Unsaved();
        var nav = (RecordingNavigation)typeof(PatientEditorViewModel)
            .GetField("_navigation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(vm)!;

        vm.PatientWindowCommand.Execute(null);

        Assert.Contains(typeof(PatientSearchViewModel), nav.Targets);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public void PatientWindow_DoesNotAskForAConfirmation_NoSuchPatternExistsInTheEditor()
    {
        var dialogs = new DecliningDialogService();
        var vm = Unsaved(dialogs);

        vm.PatientWindowCommand.Execute(null);

        Assert.Equal(0, dialogs.ConfirmationCalls);
    }

    // ================================================================
    // A1 — the lab-results guard stays
    // ================================================================

    [Fact]
    public async Task LabResults_OnAnUnsavedPatient_ShowsTheSaveFirstMessageAndNavigatesNowhere()
    {
        var vm = Unsaved();
        var nav = (RecordingNavigation)typeof(PatientEditorViewModel)
            .GetField("_navigation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(vm)!;

        await RunAsync(vm.LabResultsCommand);

        Assert.Equal("احفظ بيانات المريض أولًا قبل عرض النتائج.", vm.ErrorMessage);
        Assert.Empty(nav.Targets);
    }

    [Fact]
    public async Task LabResults_OnASavedPatient_NavigatesToTheResultSheet()
    {
        var (vm, _, nav) = await LoadedAsync();

        await RunAsync(vm.LabResultsCommand);

        Assert.Contains(typeof(PatientResultSheetViewModel), nav.Targets);
    }

    // ================================================================
    // A3 (2) — the envelope command
    // ================================================================

    [Fact]
    public async Task PrintEnvelope_OnASavedPatient_SendsTheCommand()
    {
        var (vm, sender, _) = await LoadedAsync();

        await RunAsync(vm.PrintEnvelopeCommand);

        Assert.Equal(1, sender.EnvelopeCalls);
        Assert.Empty(vm.ErrorMessage);
        Assert.Equal("تمت طباعة المغلّف.", vm.StatusMessage);
    }

    [Fact]
    public async Task PrintEnvelope_OnAnUnsavedPatient_ShowsTheSaveFirstMessageAndSendsNothing()
    {
        var vm = Unsaved();
        var sender = new EditorSender();
        var vm2 = Unsaved(sender: sender);

        await RunAsync(vm2.PrintEnvelopeCommand);

        Assert.Equal(0, sender.EnvelopeCalls);
        Assert.Equal("احفظ بيانات المريض أولًا قبل الطباعة.", vm2.ErrorMessage);
        Assert.NotNull(vm);
    }

    // ================================================================
    // A3 (4) / B3 — the two-press Enter state machine never writes on the first press
    // ================================================================

    [Fact]
    public async Task FirstEnterPress_DisplaysFiguresAndWritesNothing()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "50";
        vm.PaymentDiscountText = "10";

        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal(0, sender.ExtraChargeCalls);
        Assert.False(string.IsNullOrWhiteSpace(vm.StatusMessage));
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public async Task SecondEnterPress_WithUnchangedInputs_RecordsThroughTheSameCommandAsTheButton()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "50";
        vm.PaymentDiscountText = "10";

        await RunAsync(vm.AccountEntryEnterCommand);
        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(1, sender.PaymentCalls);
        Assert.Equal(50m, sender.Payments[0].Amount);
        Assert.Equal(10m, sender.Payments[0].DiscountAmount);
    }

    [Fact]
    public async Task EditingAnInputAfterTheFirstPress_ResetsToTheFirstPressState()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "50";
        await RunAsync(vm.AccountEntryEnterCommand);

        vm.PaymentAmountText = "60";
        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(0, sender.PaymentCalls);
    }

    [Fact]
    public async Task InvalidInput_ShowsAnErrorAndWritesNothing()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "abc";

        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal("قيمة المدفوع غير صالحة.", vm.ErrorMessage);
    }

    // ================================================================
    // B1 — the percentage field
    // ================================================================

    [Fact]
    public async Task PercentageEnter_ConvertsAgainstTheTotalAccountAmountIntoTheValueField()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "10";

        await RunAsync(vm.AccountEntryEnterCommand);

        // 10% of the patient's TOTAL account amount (200) = 20, written into the value field.
        Assert.Equal("20", vm.PaymentDiscountText);
        Assert.Equal(0, sender.PaymentCalls);
    }

    [Fact]
    public async Task PercentageOutOfRange_ShowsTheArabicMessageAndWritesNothing()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "100";
        vm.PaymentPercentageText = "101";

        await RunAsync(vm.AccountEntryEnterCommand);

        Assert.Equal("نسبة الخصم يجب أن تكون أكبر من صفر ولا تتجاوز 100.", vm.ErrorMessage);
        Assert.Equal(0, sender.PaymentCalls);
    }

    [Fact]
    public async Task EditingTheValueField_ClearsThePercentage()
    {
        var (vm, _, _) = await LoadedAsync();
        vm.PaymentPercentageText = "10";

        vm.PaymentDiscountText = "5";

        Assert.Null(vm.PaymentPercentageText);
    }

    // ================================================================
    // B2 — the "+" surcharge convention routes to the existing extra-charge path
    // ================================================================

    [Fact]
    public async Task PlusPrefix_RoutesToTheExistingExtraChargeCommand()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "+30";

        await RunAsync(vm.RecordPaymentCommand);

        Assert.Equal(1, sender.ExtraChargeCalls);
        Assert.Equal(0, sender.PaymentCalls);
    }

    [Theory]
    [InlineData("+")]
    [InlineData("+0")]
    [InlineData("-5")]
    public async Task InvalidSurchargeEntries_AreRejectedWithAnArabicMessage(string text)
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = text;

        await RunAsync(vm.RecordPaymentCommand);

        Assert.Equal(0, sender.ExtraChargeCalls);
        Assert.Equal(0, sender.PaymentCalls);
        Assert.False(string.IsNullOrWhiteSpace(vm.ErrorMessage));
    }

    [Fact]
    public async Task PlusPrefixWithADiscount_IsRejected()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.PaymentAmountText = "+30";
        vm.PaymentDiscountText = "5";

        await RunAsync(vm.RecordPaymentCommand);

        Assert.Equal(0, sender.ExtraChargeCalls);
        Assert.Equal(0, sender.PaymentCalls);
        Assert.Equal("لا يمكن إضافة مبلغ إضافي مع خصم.", vm.ErrorMessage);
    }

    // ================================================================
    // B4 — refund
    // ================================================================

    [Fact]
    public async Task Refund_OnAnUnsavedPatient_ShowsTheSaveFirstMessage()
    {
        var vm = Unsaved(sender: new EditorSender());

        await RunAsync(vm.RefundCommand);

        Assert.Equal("احفظ بيانات المريض أولًا قبل تسجيل استرداد.", vm.ErrorMessage);
    }

    [Fact]
    public async Task Refund_WithAnInvalidAmount_IsRejectedBeforeTheMediator()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.RefundAmountText = "0";

        await RunAsync(vm.RefundCommand);

        Assert.Equal(0, sender.RefundCalls);
        Assert.Equal("قيمة المدفوع غير صالحة.", vm.ErrorMessage);
    }

    [Fact]
    public async Task Refund_WithAValidAmount_SendsTheCommandAndRefreshes()
    {
        var (vm, sender, _) = await LoadedAsync();
        vm.RefundAmountText = "25";

        await RunAsync(vm.RefundCommand);

        Assert.Equal(1, sender.RefundCalls);
        Assert.Equal("تم تسجيل الاسترداد.", vm.StatusMessage);
        Assert.Equal(string.Empty, vm.RefundAmountText);
    }

    [Fact]
    public async Task Refund_SurfacesTheHandlersValidationMessage()
    {
        var sender = new EditorSender { RefundResult = Result<int>.Failure(Error.Validation("مبلغ الاسترداد يتجاوز إجمالي المدفوع الصافي (10).")) };
        var (vm, _, _) = await LoadedAsync(sender: sender);
        vm.RefundAmountText = "50";

        await RunAsync(vm.RefundCommand);

        Assert.Contains("يتجاوز", vm.ErrorMessage, StringComparison.Ordinal);
    }

    // ================================================================
    // B5 — clear-account
    // ================================================================

    [Fact]
    public async Task ClearAccount_WhenTheConfirmationIsDeclined_DoesNothing()
    {
        var dialogs = new DecliningDialogService();
        var sender = new EditorSender();
        var (vm, _, _) = await LoadedAsync(dialogs, sender);

        await RunAsync(vm.ClearAccountCommand);

        Assert.Equal(1, dialogs.ConfirmationCalls);
        Assert.Equal(0, sender.ClearCalls);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public async Task ClearAccount_WhenConfirmed_SendsTheCommandAndRefreshes()
    {
        var (vm, sender, _) = await LoadedAsync();

        await RunAsync(vm.ClearAccountCommand);

        Assert.Equal(1, sender.ClearCalls);
        Assert.Equal("تم تصفير الحساب (2 عملية).", vm.StatusMessage);
    }

    [Fact]
    public async Task ClearAccount_OnAnUnsavedPatient_ShowsTheSaveFirstMessage()
    {
        var dialogs = new DecliningDialogService();
        var vm = Unsaved(dialogs);

        await RunAsync(vm.ClearAccountCommand);

        Assert.Equal("احفظ بيانات المريض أولًا قبل تصفير الحساب.", vm.ErrorMessage);
        Assert.Equal(0, dialogs.ConfirmationCalls);
    }

    // ================================================================
    // XAML wiring (read as text — no WPF element is instantiated)
    // ================================================================

    [Fact]
    public void View_BindsTheEnterKeyAndTheNewAccountActions()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientEditorView.xaml"));

        Assert.Contains("Key=\"Enter\" Command=\"{Binding AccountEntryEnterCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding RefundCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ClearAccountCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Binding PaymentPercentageText", xaml, StringComparison.Ordinal);
        Assert.Contains("Binding RefundAmountText", xaml, StringComparison.Ordinal);
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountView_BindsTheEnterKeyAndTheNewAccountActions()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "PatientAccountView.xaml"));

        Assert.Contains("Key=\"Enter\" Command=\"{Binding AccountEntryEnterCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding RefundCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ClearAccountCommand}\"", xaml, StringComparison.Ordinal);
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


### tests/TopLab.Presentation.Tests/Patients/ResultDeliveryRoundThreeTests.cs

```csharp
using System.Collections.ObjectModel;
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
/// R1 Round 3 A2: the delivery list's three new account/report buttons.
/// <para>
/// The grid's real selection binds <c>DataGrid.SelectedItem</c> to
/// <see cref="ResultDeliveryViewModel.SelectedItem"/> (verified in
/// <c>ResultDeliveryView.xaml</c>); each button passes it as <c>CommandParameter</c>.
/// With a selected row the command navigates to the right ViewModel with the patient id;
/// with no row selected the command is disabled and does nothing.
/// </para>
/// </summary>
public class ResultDeliveryRoundThreeTests
{
    private sealed class StubSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetUndeliveredResultsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<UndeliveredPatientRowDto>>
                    .Success(Array.Empty<UndeliveredPatientRowDto>()));
            }

            throw new NotSupportedException($"StubSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    /// <summary>Records the navigation target and hands back a real instance of it.</summary>
    private sealed class LiveNavigation : INavigationService
    {
        private readonly Func<ViewModelBase>? _factory;
        public List<Type> Targets { get; } = new();
        public ViewModelBase? CurrentViewModel { get; private set; }

        public LiveNavigation(Func<ViewModelBase>? factory = null) => _factory = factory;

        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            Targets.Add(typeof(TViewModel));
            CurrentViewModel = _factory?.Invoke();
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            Targets.Add(viewModel.GetType());
            CurrentViewModel = viewModel;
        }
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static UndeliveredPatientRowDto Row(int patientId) => new(
        PatientId: patientId,
        PatientFullName: $"Patient {patientId}",
        LabId: "LAB-A",
        RegistrationDateUtc: new DateTime(2026, 10, 25, 9, 0, 0, DateTimeKind.Utc),
        VisitStatus: 1,
        UndeliveredCount: 2);

    private static ResultDeliveryViewModel Vm(INavigationService navigation)
        => new(new StubSender(), new ResultErrorPresenter(), navigation);

    // ================================================================
    // SelectedItem is the real grid selection
    // ================================================================

    [Fact]
    public void View_BindsTheGridSelectionToSelectedItem()
    {
        var xaml = ReadPresentationFile(Path.Combine("Views", "Patients", "ResultDeliveryView.xaml"));

        Assert.Contains("SelectedItem=\"{Binding SelectedItem}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("CommandParameter=\"{Binding SelectedItem}\"", xaml, StringComparison.Ordinal);
    }

    // ================================================================
    // No row selected: disabled and inert
    // ================================================================

    [Fact]
    public void WithNoRowSelected_TheThreeCommandsCannotExecute()
    {
        var vm = Vm(new LiveNavigation());

        Assert.False(vm.PatientAccountCommand.CanExecute(null));
        Assert.False(vm.CombinedReportCommand.CanExecute(null));
        Assert.False(vm.EmptyReportCommand.CanExecute(null));
    }

    [Fact]
    public void WithNoRowSelected_ExecutingDoesNothing()
    {
        var nav = new LiveNavigation();
        var vm = Vm(nav);

        vm.PatientAccountCommand.Execute(null);
        vm.CombinedReportCommand.Execute(null);
        vm.EmptyReportCommand.Execute(null);

        Assert.Empty(nav.Targets);
    }

    [Fact]
    public void WithARowSelected_TheThreeCommandsCanExecute()
    {
        var vm = Vm(new LiveNavigation());
        vm.SelectedItem = Row(7);

        Assert.True(vm.PatientAccountCommand.CanExecute(vm.SelectedItem));
        Assert.True(vm.CombinedReportCommand.CanExecute(vm.SelectedItem));
        Assert.True(vm.EmptyReportCommand.CanExecute(vm.SelectedItem));
    }

    [Fact]
    public void ARowWithNoPatientId_DoesNotEnableTheCommands()
    {
        var vm = Vm(new LiveNavigation());
        vm.SelectedItem = Row(0);

        Assert.False(vm.PatientAccountCommand.CanExecute(vm.SelectedItem));
    }

    // ================================================================
    // A selected row navigates to the right ViewModel with the patient id
    // ================================================================

    [Fact]
    public void PatientAccount_WithASelectedRow_NavigatesToTheAccountScreen()
    {
        var nav = new LiveNavigation(() => new PatientAccountViewModel(
            new StubSender(), new LiveNavigation(), new ResultErrorPresenter(), new FakeDialogService(), new StubServiceProvider()));
        var vm = Vm(nav);
        var row = Row(7);
        vm.SelectedItem = row;

        vm.PatientAccountCommand.Execute(row);

        Assert.Contains(typeof(PatientAccountViewModel), nav.Targets);
        Assert.IsType<PatientAccountViewModel>(nav.CurrentViewModel);
    }

    [Fact]
    public void CombinedReport_WithASelectedRow_NavigatesToTheCombinedReportScreen()
    {
        var nav = new LiveNavigation(() => new CombinedReportViewModel(
            new StubSender(), new ResultErrorPresenter(), new FakeDialogService(), new StubServiceProvider(),
            new LiveNavigation(), new UnusedPdfPreview(), new UnusedDbContext()));
        var vm = Vm(nav);
        var row = Row(9);
        vm.SelectedItem = row;

        vm.CombinedReportCommand.Execute(row);

        Assert.Contains(typeof(CombinedReportViewModel), nav.Targets);
    }

    [Fact]
    public void EmptyReport_WithASelectedRow_NavigatesToTheBlankReportScreen()
    {
        var nav = new LiveNavigation(() => new BlankReportViewModel(
            new StubSender(), new ResultErrorPresenter(), new LiveNavigation()));
        var vm = Vm(nav);
        var row = Row(11);
        vm.SelectedItem = row;

        vm.EmptyReportCommand.Execute(row);

        Assert.Contains(typeof(BlankReportViewModel), nav.Targets);
    }

    [Fact]
    public void Commands_IgnoreAParameterThatIsNotARow()
    {
        var nav = new LiveNavigation();
        var vm = Vm(nav);

        vm.PatientAccountCommand.Execute("not a row");
        vm.CombinedReportCommand.Execute(42);

        Assert.Empty(nav.Targets);
    }

    // ================================================================
    // Helpers
    // ================================================================

    private sealed class UnusedPdfPreview : TopLab.Application.Features.ReportProduction.Common.IPdfPreviewService
    {
        public Task<Result<string>> PreviewAsync(string absolutePdfPath, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("The account/report buttons do not preview on this path.");
    }

    private sealed class UnusedDbContext : TopLab.Application.Common.Interfaces.IApplicationDbContext
    {
        public IQueryable<TEntity> Set<TEntity>() where TEntity : class => throw new NotSupportedException();
        public void Add<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Update<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
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


### tests/TopLab.Presentation.Tests/Converters/NullToVisibilityConverterTests.cs

```csharp
using System.Windows;
using TopLab.Presentation.Common.Converters;
using Xunit;

namespace TopLab.Presentation.Tests.Converters;

public class NullToVisibilityConverterTests
{
    private readonly NullToVisibilityConverter _c = new();

    [Fact]
    public void Null_Collapsed()
    {
        Assert.Equal(Visibility.Collapsed, _c.Convert(null!, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void EmptyString_Collapsed()
    {
        Assert.Equal(Visibility.Collapsed, _c.Convert(string.Empty, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Whitespace_Collapsed()
    {
        Assert.Equal(Visibility.Collapsed, _c.Convert("   ", typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Text_Visible()
    {
        Assert.Equal(Visibility.Visible, _c.Convert("note", typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture));
    }
}

```


### tests/TopLab.Presentation.Tests/Layout/XamlGridOverlapGuardTests.cs

```csharp
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace TopLab.Presentation.Tests.Layout;

public class XamlGridOverlapGuardTests
{
    [Theory]
    [InlineData("DeliveryHandoverView.xaml")]
    [InlineData("ProfileEntryView.xaml")]
    [InlineData("CultureEntryView.xaml")]
    public void NoTwoDirectChildrenShareSameGridCell(string fileName)
    {
        var root = FindSolutionRoot();
        var path = Path.Combine(root, "src", "TopLab.Presentation", "Views", "Patients", fileName);
        var doc = XDocument.Load(path);
        var grid = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Grid");
        Assert.NotNull(grid);
        var children = grid!.Elements().ToList();
        var cells = new HashSet<(int row, int col)>();
        foreach (var c in children)
        {
            var tag = c.Name.LocalName;
            if (tag == "RowDefinition" || tag == "ColumnDefinition" || tag.EndsWith("RowDefinitions") || tag.EndsWith("ColumnDefinitions")) continue;
            var subtree = c.ToString();
            // Mutually-exclusive empty-state vs grid
            if (subtree.Contains("ShowEmpty")) continue;
            // Mutually-exclusive report preview
            if (subtree.Contains("HasReport")) continue;
            // Status/error message pair shares a row by design (pre-existing)
            if (subtree.Contains("StatusMessage") || subtree.Contains("ErrorMessage")) continue;
            // Action buttons + trailing comment preview share a row by design (pre-existing)
            if (tag == "StackPanel" && subtree.Contains("Orientation=\"Horizontal\"")) continue;
            if (subtree.Contains("PickedCommentPreview")) continue;

            var rowAttr = c.Attributes().FirstOrDefault(a => a.Name.LocalName == "Grid.Row");
            var colAttr = c.Attributes().FirstOrDefault(a => a.Name.LocalName == "Grid.Column");
            var row = rowAttr != null && int.TryParse(rowAttr.Value, out var rr) ? rr : 0;
            var col = colAttr != null && int.TryParse(colAttr.Value, out var cc) ? cc : 0;
            var key = (row, col);
            Assert.False(cells.Contains(key), $"{fileName}: two direct children share Grid.Row={row}, Grid.Column={col}");
            cells.Add(key);
        }
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }
        Assert.NotNull(dir);
        return dir!.FullName;
    }
}

```


### tests/TopLab.Presentation.Tests/Patients/DeliveryHandoverNotesTests.cs

```csharp
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

public class DeliveryHandoverNotesTests
{
    private sealed class FakeSender : ISender
    {
        public Result<DeliveryAccountDto>? AccountResult { get; set; }
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetDeliveryAccountQuery q)
            {
                var r = AccountResult ?? Result<DeliveryAccountDto>.Success(new DeliveryAccountDto(1, 100m, 80m, 20m, 20m, 0m, "note text"));
                return Task.FromResult((TResponse)(object)r);
            }
            if (request is GetDeliveryGridQuery q2)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<DeliveryGridRowDto>>.Success(Array.Empty<DeliveryGridRowDto>()));
            }
            throw new NotSupportedException(request.GetType().Name);
        }
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeNav : INavigationService
    {
        public ViewModelBase? CurrentViewModel { get; set; }
        public event Action<ViewModelBase?>? Navigated
        {
            add { }
            remove { }
        }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
        public void NavigateTo(ViewModelBase viewModel) { CurrentViewModel = viewModel; }
    }

    [Fact]
    public async Task PatientNotes_Loaded_WhenPresent()
    {
        var sender = new FakeSender { AccountResult = Result<DeliveryAccountDto>.Success(new DeliveryAccountDto(1, 100m, 80m, 20m, 20m, 0m, "note text")) };
        var vm = new DeliveryHandoverViewModel(sender, new ResultErrorPresenter(), new FakeDialogService(), new FakeNav());
        await vm.LoadAsync(1);
        Assert.Equal("note text", vm.PatientNotes);
    }

    [Fact]
    public async Task PatientNotes_Empty_WhenNull()
    {
        var sender = new FakeSender { AccountResult = Result<DeliveryAccountDto>.Success(new DeliveryAccountDto(1, 100m, 80m, 20m, 20m, 0m, null)) };
        var vm = new DeliveryHandoverViewModel(sender, new ResultErrorPresenter(), new FakeDialogService(), new FakeNav());
        await vm.LoadAsync(1);
        Assert.Equal(string.Empty, vm.PatientNotes);
    }
}

```


## (c) Reference text used, with line numbers (canonical reference)

### 1.3.1.2 / 1.3.1.3 / 1.3.1.4 / 1.3.1.4.1 / 1.3.1.4.2

L975: | 1.3.1.2 | Discount Value Field | قيمة الخصم | Field / UI element | The discount applied to the patient's account as an absolute amount. | [VERIFIED] | In scope |
L976: | 1.3.1.3 | Discount Percentage Field | % | Field / UI element | The discount applied to the patient's account as a percentage. | [VERIFIED] | In scope |
L977: | 1.3.1.4 | Paid Amount Field | المدفوع | Field / UI element | The amount paid by the patient; entered by the user. | [VERIFIED] | In scope |
L978: | 1.3.1.4.1 | Surcharge Entry Convention | — | Rule / Rule | An additional amount that is not linked to a test — an extra service or an additional cost — is entered in the paid field preceded by a `+` sign, and is then added to the total of the tests. | [VERIFIED] | In scope |
L979: | 1.3.1.4.2 | Enter Key Account Operation | — | Rule / Rule | The first `Enter` press performs the accounting operation for the patient; the second press saves the operation. | [VERIFIED] | In scope |

### 1.3.2.5 / 1.3.2.7

L1000: | 1.3.2.5 | Refund Action | استرداد | Action / Function | Records a refund against the account. | [VERIFIED] | In scope |
L1002: | 1.3.2.7 | Clear Account Action | تصفير الحساب | Action / Function | Clears the account of the patient. | [VERIFIED] | In scope |

### account walkthrough (§ Compute and settle the patient account)

L1512: 1. The account figures appear in the account panel; the total of the tests is computed from the prices entered in the program. → 1.3.1.1
L1513: 2. If a discount exists, enter its value or its percentage. → 1.3.1.2, 1.3.1.3
L1514: 3. Enter the amount already paid by the patient in the paid field. → 1.3.1.4
L1515: 4. Press `Enter`: the first press performs the accounting operation, the second press saves it. → 1.3.1.4.2
L1516: 5. **Branch — the patient pays in full:** press the net-settlement action, then confirm in the dialog that follows. → 1.3.1.10
L1517: 6. **Branch — an additional amount linked to a test:** enter the amount in the paid field prefixed with `+` and press the confirm-payment action; the amount is added to the total of the tests. → 1.3.1.4.1, 1.3.1.9
L1518: 7. **Branch — correcting an amount:** press the account-summary action; on the patient-account screen select the amount and press the modify action, or press the delete action to remove the transaction. → 1.3.1.11, 1.3.2.5, 1.3.2.6

### key-rules table

L1632: | 1.3.1.4.1 | An amount prefixed with `+` in the paid field is treated as an addition to the total of the tests; an unprefixed amount is the payment itself. | [VERIFIED] |
L1633: | 1.3.1.4.2 | The two `Enter` presses are not interchangeable: the first computes, the second saves. | [VERIFIED] |

### 1.3.1.12 unverified note (context)

L1718: | 1.3.1.12 | The custody action is present on the account panel, but its behaviour is not established. | [UNVERIFIED] |


### Contradictions between the reference text and the owner decisions

None found.

- 1.3.1.4.1 (L978, L1632) says an amount prefixed with `+` "is entered in the paid field preceded by a `+` sign, and is then added to the total of the tests". OD-11 (a) adopts exactly this, in the paid-amount field, through the existing `IsExtraCharge` capability. No contradiction.
- 1.3.1.4.2 (L979, L1633) says "The first `Enter` press performs the accounting operation for the patient; the second press saves the operation" and "the two `Enter` presses are not interchangeable: the first computes, the second saves". The owner decision matches the second clause literally. The first clause ("performs the accounting operation") is worded ambiguously — it could be read as "records". The owner decision resolves it as compute-and-display, and that reading is the one supported by L1633. Recorded as an assumption (see (f)).
- 1.3.1.3 (L976) names the field `%` and says only "The discount applied to the patient's account as a percentage". The base for the percentage is NOT stated in the reference. The owner decision fixes it as the patient's TOTAL account amount. Recorded as an assumption (see (f)).
- 1.3.2.5 (L1000) says only "Records a refund against the account". The ceiling and the row shape come from the owner decision plus the existing Domain rule. No contradiction.
- 1.3.2.7 (L1002) says only "Clears the account of the patient". The reference does not say whether rows are deleted; the owner decision (void, keep rows) is compatible. No contradiction.
- 1.3.1.2 vs 1.3.1.3 (L975/L976) describe two separate fields. Nothing in the reference says they are mutually exclusive; the owner decision makes them exclusive (editing one clears the other). This is a stricter behaviour than the text requires, not a contradiction.


## (d) Every payment-entry screen found in Part B


| # | Screen / entry point | ViewModel | View | Command used today |
|---|---|---|---|---|
| 1 | Patient editor — account strip (bottom bar) | `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` (L146, L554, L1732) | `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` (L33, L50) | `RecordPaymentCommand` → `RecordPaymentCommand`; separate extra-charge action `OpenExtraChargeCommand` → dialog |
| 2 | Patient account screen — payment strip | `src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs` (L80, L134, L314) | `src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml` (L70, L87) | `RecordPaymentCommand` → `RecordPaymentCommand`; `OpenExtraChargeCommand` → `ExtraChargeDialogViewModel` (L64) → `RecordExtraChargeCommand` |
| 3 | Sent-out lab account screen | `src/TopLab.Presentation/ViewModels/Patients/SentOutLabAccountViewModel.cs` (L42, L65, L101) | `src/TopLab.Presentation/Views/Patients/SentOutLabAccountView.xaml` (L31, L32) | `RecordSentOutPaymentCommand` — a DIFFERENT feature (`SentOutSamples`), not `PatientBilling`. Left untouched: the reference's account panel (1.3.1) is the patient account, and the round's scope is the patient-account panel. |

Both patient-billing entry screens (1 and 2) now drive the same shared
`AccountEntryParser` / `AccountEntryCalculator` / `AccountEntrySession`, so they cannot diverge.
Screen 3 is a different feature with its own command and its own amount semantics.


## (e) A4 — every occurrence of NullToVis / NullToVisibilityConverter (actual search output)

```
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:231: +        <converters:NullToVisibilityConverter x:Key="NullToVis" />
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:234: diff --git a/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs b/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:236: --- a/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:237: +++ b/src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:238: @@ -8,7 +8,9 @@ public sealed class NullToVisibilityConverter : IValueConverter
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:545: +                   Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}"/>
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:644: +                Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}">
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:727: +                   Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}"/>
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:1202: ### tests/TopLab.Presentation.Tests/Converters/NullToVisibilityConverterTests.cs
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:1211: public class NullToVisibilityConverterTests
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:1213: private readonly NullToVisibilityConverter _c = new();
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:1354: | 1 | Auto | Border (notes, NullToVis) |
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:1375: | 1 | Auto | TextBlock (notes, NullToVis) |
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:1398: | 1 | Auto | TextBlock (notes, NullToVis) |
Docs/Audit/R1-ROUND2B-EVIDENCE-20261025.md:1426: ## (e) Converter usage audit (NullToVisibilityConverter / NullToVis)
Docs/Investigations/تقرير الوكيل المحلي.md:286: 4. **لا `NotSupportedException` إنتاجية** عدا محولي `ConvertBack` بالتصميم (`NullToVisibilityConverter.cs:14`، `InverseBooleanConverter.cs:13`) — `VERIFIED-BY-EXECUTION`.
Docs/Remaining Tasks Folder/رد الوكيل الفرعي الاول.md:450: NullToVisibilityConverter	Common/Converters/NullToVisibilityConverter.cs:7	null→Collapsed else Visible	throw new NotSupportedException() (:14)
Docs/Remaining Tasks Folder/رد الوكيل الفرعي الاول.md:564: throw new NotSupportedException()	2, both intentional one-way IValueConverter.ConvertBack: Common/Converters/InverseBooleanConverter.cs:13, Common/Converters/NullToVisibilityConverter.cs:14
src/TopLab.Presentation/App.xaml:8: <converters:NullToVisibilityConverter x:Key="NullToVis" />
src/TopLab.Presentation/Common/Converters/NullToVisibilityConverter.cs:7: public sealed class NullToVisibilityConverter : IValueConverter
src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml:28: Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}"/>
src/TopLab.Presentation/Views/Patients/DeliveryHandoverView.xaml:30: Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}">
src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml:28: Visibility="{Binding PatientNotes, Converter={StaticResource NullToVis}}"/>
tests/TopLab.Presentation.Tests/Converters/NullToVisibilityConverterTests.cs:7: public class NullToVisibilityConverterTests
tests/TopLab.Presentation.Tests/Converters/NullToVisibilityConverterTests.cs:9: private readonly NullToVisibilityConverter _c = new();
```


The converter is registered once in `App.xaml` and referenced by exactly three views — the three
notes elements. No other screen consumes it, so the Round-2b behaviour change (also collapsing
empty/whitespace strings) affects only those three notes elements.


## (f) Other assumptions I was forced to make

| # | Assumption | Alternatives | Why this one |
|---|---|---|---|
| 1 | 1.3.1.4.2 "the first press performs the accounting operation" = compute and DISPLAY only; nothing is written. | (a) the first press records and the second confirms it; (b) the first press computes and the second writes. | L1633 states the presses "are not interchangeable: the first computes, the second saves", and the owner decision is explicit. (b) chosen; a test proves no mediator call on the first press. |
| 2 | The percentage base is the patient's TOTAL account amount (`TotalCharged`). | (a) the remaining balance; (b) the entered paid amount. | The reference does not state the base (L976). The owner decision names the total account amount. |
| 3 | "the patient's TOTAL account amount" is read as `PatientAccountDto.TotalCharged` (tests + surcharges) — the figure the panel labels "إجمالي التحاليل". | (a) tests only, excluding surcharges. | `TotalCharged` is the only total the read model exposes and is what the panel displays as the total; excluding surcharges would need a new projection. |
| 4 | A surcharge entered with `+` routes to the existing `RecordExtraChargeCommand` rather than to `RecordPaymentCommand`. | (a) record it as a `Payment` with a negative discount; (b) add a new command. | OD-11 says to use the existing extra-charge mechanism and keep the separate extra-charge action; the existing command already creates `IsExtraCharge = true`. |
| 5 | The refund ceiling is `PatientAccountCalculator.TotalPaid(operations)` (already net of refunds and voids, floored at zero). | (a) `TotalCharged − TotalPaid`; (b) a new "net paid" projection. | `TotalPaid` already subtracts refunds and ignores voided rows, so it IS the net paid after earlier refunds and voids. |
| 6 | Clear-account voids ALL non-voided operations, including extra charges and corrections, not only payments. | (a) only `OperationType.Payment`. | 1.3.2.7 says "Clears the account of the patient"; clearing an account while leaving surcharges standing would leave a non-zero balance, which is not "cleared". |
| 7 | Arabic-Indic digits are accepted on input. | (a) reject them; (b) leave the existing invariant parser alone. | The reference does not discuss input digits; the screens are RTL Arabic, so accepting both keyboards is the safer default. No existing payment parsing handled them (verified: no `NormalizeDigits`/Arabic-digit code existed anywhere in the repo). |
| 8 | `SentOutLabAccountViewModel` (the sent-out lab account) is NOT part of "the account panel". | (a) also refactor it. | 1.3.1 is the patient account panel; the sent-out lab account is a different feature with its own command and no `+`/percentage semantics in the reference. Out of the round's stated scope. |
| 9 | The Enter key binding lives in the ViewModel command; the XAML only binds `KeyBinding`. | (a) handle it in code-behind. | The task requires the logic in the ViewModel and no logic in code-behind. |


## (g) Changed screens — row tables and ASCII drawings (STATIC EVIDENCE ONLY)

These are read from the XAML text. No screen was executed, rendered or visually verified.

### PatientEditorView.xaml — bottom account strip (WrapPanel #1)

| Position | Element | Content |
|---|---|---|
| 1 | TextBlock | إجمالي التحاليل: `{Binding TotalCharged}` |
| 2 | TextBlock | الخصم: `{Binding TotalDiscount}` |
| 3 | TextBlock | المدفوع: `{Binding TotalPaid}` |
| 4 | TextBlock | الباقي: `{Binding Balance}` |
| 5 | Button | عرض الحساب → `OpenAccountCommand` |
| 6 | TextBlock | دفعة: |
| 7 | TextBox | `PaymentAmountText` + **KeyBinding Enter → `AccountEntryEnterCommand`** (NEW) |
| 8 | TextBlock | خصم: |
| 9 | TextBox | `PaymentDiscountText` + **KeyBinding Enter → `AccountEntryEnterCommand`** (NEW) |
| 10 | TextBlock | % (NEW) |
| 11 | TextBox | `PaymentPercentageText` + **KeyBinding Enter → `AccountEntryEnterCommand`** (NEW) |
| 12 | Button | تسجيل مدفوع → `RecordPaymentCommand` |
| 13 | Button | تسوية كاملة → `SettleCommand` |
| 14 | TextBlock | استرداد: (NEW) |
| 15 | TextBox | `RefundAmountText` (NEW) |
| 16 | Button | استرداد → `RefundCommand` (NEW) |
| 17 | Button | تصفير الحساب → `ClearAccountCommand` (NEW) |

```
WrapPanel #1 (account strip, RTL — first item is right-most)
[1] إجمالي التحاليل  [2] الخصم  [3] المدفوع  [4] الباقي  [5] عرض الحساب
[6] دفعة: [7][TextBox amount]   [8] خصم: [9][TextBox discount]   [10] % [11][TextBox percent]
[12] تسجيل مدفوع   [13] تسوية كاملة
[14] استرداد: [15][TextBox refund]  [16] استرداد   [17] تصفير الحساب
WrapPanel #2 (actions): جديد حفظ حذف تراجع | باركود ورقة العمل الإيصال الفاتورة طلب تحاليل مغلف نافذة المرضى نتائج التحاليل
[ErrorMessage TextBlock]
[StatusMessage TextBlock]
```

### PatientAccountView.xaml — payment strip

| Position | Element | Content |
|---|---|---|
| 1 | TextBlock | دفعة: |
| 2 | TextBox | `PaymentAmountText` + **KeyBinding Enter → `AccountEntryEnterCommand`** (NEW) |
| 3 | TextBlock | خصم: |
| 4 | TextBox | `PaymentDiscountText` + **KeyBinding Enter → `AccountEntryEnterCommand`** (NEW) |
| 5 | TextBlock | % (NEW) |
| 6 | TextBox | `PaymentPercentageText` + **KeyBinding Enter → `AccountEntryEnterCommand`** (NEW) |
| 7 | Button | تسجيل مدفوع → `RecordPaymentCommand` |
| 8 | Button | تسوية كاملة → `SettleCommand` |
| 9 | TextBlock | استرداد: (NEW) |
| 10 | TextBox | `RefundAmountText` (NEW) |
| 11 | Button | استرداد → `RefundCommand` (NEW) |
| 12 | Button | تصفير الحساب → `ClearAccountCommand` (NEW) |

```
[1] دفعة: [2][TextBox amount]  [3] خصم: [4][TextBox discount]  [5] % [6][TextBox percent]
[7] تسجيل مدفوع  [8] تسوية كاملة  [9] استرداد: [10][TextBox refund]  [11] استرداد  [12] تصفير الحساب
```


---

## (h) Final verification run (this machine, this round)

Baseline (start of round, HEAD `efc909c`): 3310 tests — 3308 passed, 2 skipped (Docker);
build 0 errors, **6 warning lines / 2 distinct warning kinds**
(`CS1998` at `PatientEditorViewModel.cs(1498,24)` — the `PatientWindowAsync` async-without-await
warning this round was asked to remove, and `CS0067` in `DeliveryHandoverNotesTests.cs(43,46)`).

Final (end of round, HEAD unchanged):

| Suite | Passed | Skipped | Failed |
|---|---|---|---|
| TopLab.Domain.Tests | 702 | 0 | 0 |
| TopLab.Application.Tests | 1907 | 0 | 0 |
| TopLab.Infrastructure.Tests | 527 | 0 | 0 |
| TopLab.Persistence.Tests | 24 | 2 | 0 |
| TopLab.Presentation.Tests | 294 | 0 | 0 |
| **Total** | **3454** | **2** | **0** |

Build: `dotnet build TopLab.sln --no-incremental` → **0 errors, 0 warnings**.

Delta: +146 tests, −6 warning lines (both warning kinds removed), 0 failures.

### Safety rules
- No `git commit` / `push` / `add` / `stash` / `reset` / `checkout` / `clean` was run; `HEAD` is
  `efc909c688ccd3375b6bf3bc15af617f3739c647` at both start and end and the index is untouched.
- No migration was created or applied; the applied migration
  `20261025123435_AddRefundAmountToPaymentOperation` was not touched. No database was touched.
- Every pre-existing file in `Docs/Audit/` is byte-identical: the SHA-256 of all 20 files was
  recorded before and after, and zero changed. This evidence file is the only new file there.
- No permission code, seed row, migration or schema change was added; both new commands are
  ungated exactly like `RecordPaymentCommand`.
- No WPF screen was executed or visually verified; every layout statement above is static evidence
  read from the XAML text.
