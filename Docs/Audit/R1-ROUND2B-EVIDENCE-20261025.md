# R1 Round 2b Evidence

Generated: 2026-10-25 (static evidence; no DB, no commit).

HEAD: efc909c688ccd3375b6bf3bc15af617f3739c647


## (a) git diff (tracked modified files)

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
diff --git a/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs b/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs
index 196c0c4..30a5e41 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs
@@ -10,6 +10,7 @@ using TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;
 using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
 using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
 using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
+using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
 using TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;
 using TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;
 using TopLab.Application.Features.PatientRegistration.Commands.ApplyConditionDeltas;
@@ -149,6 +150,7 @@ public sealed class PatientEditorViewModel : ViewModelBase
         PrintReceiptCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "receipt"));
         PrintInvoiceCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "invoice"));
         PrintLabOrderCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "laborder"));
+        PrintEnvelopeCommand = new AsyncRelayCommand(async (_, ct) => await PrintEnvelopeAsync(ct));
         AddTestToSelectionCommand = new RelayCommand(param =>
         {
             if (param is int testId)
@@ -174,6 +176,8 @@ public sealed class PatientEditorViewModel : ViewModelBase
         ClearSearchCommand = new RelayCommand(ClearSearch);
         OpenSearchResultCommand = new AsyncRelayCommand(async (param, ct) => await OpenSearchResultAsync(param as PatientSummaryDto, ct));
         OpenAccountCommand = new AsyncRelayCommand(async (_, ct) => await OpenAccountAsync(ct));
+        PatientWindowCommand = new AsyncRelayCommand(async (_, ct) => await PatientWindowAsync(ct));
+        LabResultsCommand = new AsyncRelayCommand(async (_, ct) => await LabResultsAsync(ct));
         AddProfileCommand = new AsyncRelayCommand(async (_, ct) => await AddProfileAsync(ct));
         AddCustomGroupCommand = new AsyncRelayCommand(async (_, ct) => await AddCustomGroupAsync(ct));
         ClearAllVisitTestsCommand = new AsyncRelayCommand(async (_, ct) => await ClearAllVisitTestsAsync(ct));
@@ -636,6 +640,7 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     /// <summary>Phase 1 REF-068: laboratory-order slip print action.</summary>
     public AsyncRelayCommand PrintLabOrderCommand { get; }
+    public AsyncRelayCommand PrintEnvelopeCommand { get; }
 
     public RelayCommand AddTestToSelectionCommand { get; }
 
@@ -653,6 +658,8 @@ public sealed class PatientEditorViewModel : ViewModelBase
 
     /// <summary>S-03 Slice 4: totals-row entry into the patient account screen.</summary>
     public AsyncRelayCommand OpenAccountCommand { get; }
+    public AsyncRelayCommand PatientWindowCommand { get; }
+    public AsyncRelayCommand LabResultsCommand { get; }
 
     public AsyncRelayCommand AddProfileCommand { get; }
 
@@ -1488,6 +1495,36 @@ public sealed class PatientEditorViewModel : ViewModelBase
         }
     }
 
+    private async Task PatientWindowAsync(CancellationToken cancellationToken)
+    {
+        ErrorMessage = string.Empty;
+        StatusMessage = string.Empty;
+        if (!IsEditMode || !_patientId.HasValue)
+        {
+            ErrorMessage = "احفظ بيانات المريض أولًا قبل عرض قائمة المرضى.";
+            return;
+        }
+
+        _navigation.NavigateTo<PatientSearchViewModel>();
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
@@ -1695,6 +1732,36 @@ public sealed class PatientEditorViewModel : ViewModelBase
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
index 38b2efc..a6eb574 100644
--- a/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs
+++ b/src/TopLab.Presentation/ViewModels/Patients/ResultDeliveryViewModel.cs
@@ -36,6 +36,9 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
         LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
         OpenHandoverCommand = new RelayCommand(param => OpenHandover(param as UndeliveredPatientRowDto));
         OpenGroupSlipsCommand = new RelayCommand(_ => OpenGroupSlips());
+        PatientAccountCommand = new RelayCommand(param => OpenPatientAccount(param as UndeliveredPatientRowDto));
+        CombinedReportCommand = new RelayCommand(param => OpenCombinedReport(param as UndeliveredPatientRowDto));
+        EmptyReportCommand = new RelayCommand(param => OpenEmptyReport(param as UndeliveredPatientRowDto));
         BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientsHubViewModel>());
     }
 
@@ -91,6 +94,9 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
     public AsyncRelayCommand LoadCommand { get; }
     public RelayCommand OpenHandoverCommand { get; }
     public RelayCommand OpenGroupSlipsCommand { get; }
+    public RelayCommand PatientAccountCommand { get; }
+    public RelayCommand CombinedReportCommand { get; }
+    public RelayCommand EmptyReportCommand { get; }
     public RelayCommand BackCommand { get; }
 
     public async Task LoadAsync(CancellationToken cancellationToken = default)
@@ -148,4 +154,34 @@ public sealed class ResultDeliveryViewModel : ViewModelBase
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
diff --git a/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml b/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml
index a0f52f8..121eb42 100644
--- a/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml
+++ b/src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml
@@ -47,6 +47,9 @@
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


## (b) Untracked test/converter/source files added in R1/Round2/Round2b


### tests/TopLab.Application.Tests/Features/ResultDelivery/GetDeliveryAccountNotesTests.cs

```csharp
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class GetDeliveryAccountNotesTests
{
    [Fact]
    public async Task CarriesNotes_WhenPresent()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, notes: "note text");
        db.Patients.Add(patient);
        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("note text", result.Value!.PatientNotes);
    }

    [Fact]
    public async Task CarriesNull_WhenAbsent()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(patient);
        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PatientNotes);
    }
}

```


### tests/TopLab.Application.Tests/Features/ProfileResults/GetProfileEntryGridNotesTests.cs

```csharp
using TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Application.Tests.Features.ProfileResults;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class GetProfileEntryGridNotesTests
{
    [Fact]
    public async Task CarriesNotes_WhenPresent()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P1", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, notes: "profile note"));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);

        var handler = new GetProfileEntryGridQueryHandler(db);
        var result = await handler.Handle(new GetProfileEntryGridQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("profile note", result.Value!.PatientNotes);
    }

    [Fact]
    public async Task CarriesNull_WhenAbsent()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);

        var handler = new GetProfileEntryGridQueryHandler(db);
        var result = await handler.Handle(new GetProfileEntryGridQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PatientNotes);
    }
}

```


### tests/TopLab.Application.Tests/Features/CultureResults/GetCultureEntryGridNotesTests.cs

```csharp
using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

public class GetCultureEntryGridNotesTests
{
    private static FakeApplicationDbContext Seed(bool withNotes)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Patient", Sex.Female, 30, AgeUnit.Year, DateTime.UtcNow, notes: withNotes ? "culture note" : null));
        db.Tests.Add(Test.Create(TestId.Create(1), "Culture", "Culture report", "Culture", "CULT", 1, 100m, ResultKind.Culture, isCultureType: true));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10), PatientId.Create(1), TestId.Create(1), 100m));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(1), "Normal"));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(1)));
        return db;
    }

    [Fact]
    public async Task CarriesNotes_WhenPresent()
    {
        var db = Seed(true);
        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("culture note", result.Value!.PatientNotes);
    }

    [Fact]
    public async Task CarriesNull_WhenAbsent()
    {
        var db = Seed(false);
        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PatientNotes);
    }
}

```


### tests/TopLab.Domain.Tests/Billing/PaymentOperationRefundTests.cs

```csharp
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

public class PaymentOperationRefundTests
{
    private static PaymentOperation Make(
        decimal amount = 0m,
        OperationType operationType = OperationType.Payment,
        decimal? refundAmount = null,
        decimal? discountAmount = null,
        bool isExtraCharge = false) =>
        PaymentOperation.Create(
            PaymentOperationId.Create(1),
            PatientId.Create(1),
            amount,
            1,
            DateTime.UtcNow,
            discountAmount,
            isExtraCharge,
            operationType,
            refundAmount);

    [Fact]
    public void Refund_StoresRefundAmount()
    {
        var op = Make(operationType: OperationType.Refund, refundAmount: 25m);

        Assert.Equal(OperationType.Refund, op.OperationType);
        Assert.Equal(25m, op.RefundAmount);
        Assert.Equal(0m, op.Amount);
    }

    [Fact]
    public void Refund_RequiresPositiveRefundAmount()
    {
        Assert.Throws<ArgumentException>(() => Make(operationType: OperationType.Refund, refundAmount: null));
        Assert.Throws<ArgumentException>(() => Make(operationType: OperationType.Refund, refundAmount: 0m));
        Assert.Throws<ArgumentException>(() => Make(operationType: OperationType.Refund, refundAmount: -1m));
    }

    [Fact]
    public void Refund_RequiresZeroAmount()
    {
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.Refund, refundAmount: 5m));
    }

    [Fact]
    public void NonRefund_RejectsRefundAmount()
    {
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.Payment, refundAmount: 5m));
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.Correction, refundAmount: 5m));
        Assert.Throws<ArgumentException>(() => Make(amount: 10m, operationType: OperationType.FullSettlement, refundAmount: 5m));
    }

    [Fact]
    public void RefundAmount_NullForNonRefund()
    {
        var op = Make(amount: 10m, operationType: OperationType.Payment);
        Assert.Null(op.RefundAmount);
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
        public event Action<ViewModelBase?>? Navigated;
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


## (c) Final text of PaymentOperation.Create

```csharp
public static PaymentOperation Create(
        PaymentOperationId id,
        PatientId patientId,
        decimal amount,
        int receivedByUserId,
        DateTime operationAtUtc,
        decimal? discountAmount = null,
        bool isExtraCharge = false,
        OperationType operationType = OperationType.Payment,
        decimal? refundAmount = null)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be >= 0.", nameof(amount));
        if (discountAmount < 0)
            throw new ArgumentException("Discount must be >= 0.", nameof(discountAmount));
        if (discountAmount > amount)
            throw new ArgumentException("Discount cannot exceed the operation amount.", nameof(discountAmount));
        if (isExtraCharge && discountAmount is > 0)
            throw new ArgumentException("An extra charge cannot carry a discount.", nameof(discountAmount));
        if (operationType == OperationType.FullSettlement && amount <= 0)
            throw new ArgumentException("Settlement amount must be > 0.", nameof(amount));
        if (operationType == OperationType.Refund)
        {
            if (refundAmount is null || refundAmount <= 0)
                throw new ArgumentException("A refund operation requires RefundAmount > 0.", nameof(refundAmount));
            if (amount != 0)
                throw new ArgumentException("A refund operation must have Amount = 0.", nameof(amount));
        }
        else
        {
            if (refundAmount is not null)
                throw new ArgumentException("Non-refund operations must not carry a RefundAmount.", nameof(refundAmount));
        }
        return new PaymentOperation(id, patientId, amount, discountAmount, isExtraCharge, operationType, receivedByUserId, operationAtUtc, refundAmount);
    }
```


## (d) New row tables and ASCII drawings

### DeliveryHandoverView (outer Grid, 7 rows)
| Row | Height | Direct child |
|---|---|---|
| 0 | Auto | Border (account panel) |
| 1 | Auto | Border (notes, NullToVis) |
| 2 | Auto | StackPanel (settlement) |
| 3 | Auto | ProgressBar (loading) |
| 4 | * | TextBlock (empty) OR DataGrid |
| 5 | Auto | TextBlock (status) |
| 6 | Auto | TextBlock (error) |

```
[0] Account panel
[1] Notes (yellow, visible only when non-empty)
[2] Settlement
[3] Loading
[4] Empty/Grid
[5] Status
[6] Error
```

### ProfileEntryView (8 rows)
| Row | Height | Direct child |
|---|---|---|
| 0 | Auto | Border (header) |
| 1 | Auto | TextBlock (notes, NullToVis) |
| 2 | Auto | DockPanel (comment) |
| 3 | Auto | ProgressBar (loading) |
| 4 | * | TextBlock (empty) OR DataGrid |
| 5 | Auto | Border (report preview) |
| 6 | Auto | TextBlock (status) / TextBlock (error) |
| 7 | Auto | StackPanel (actions) |

```
[0] Header
[1] Notes (visible only when non-empty)
[2] Comment
[3] Loading
[4] Empty/Grid
[5] Report preview
[6] Status/Error
[7] Actions
```

### CultureEntryView (9 rows)
| Row | Height | Direct child |
|---|---|---|
| 0 | Auto | Border (header) |
| 1 | Auto | TextBlock (notes, NullToVis) |
| 2 | Auto | Grid (culture fields) |
| 3 | Auto | ProgressBar (loading) |
| 4 | * | TextBlock (empty) OR DataGrid |
| 5 | Auto | GroupBox (microscopy) |
| 6 | Auto | Border (report preview) |
| 7 | Auto | TextBlock (status) / TextBlock (error) |
| 8 | Auto | StackPanel (actions) + comment preview |

```
[0] Header
[1] Notes (visible only when non-empty)
[2] Culture fields
[3] Loading
[4] Empty/Grid
[5] Microscopy
[6] Report preview
[7] Status/Error
[8] Actions + comment
```

### PatientEditorView action band
New buttons added after "مغلف": "نافذة المرضى" (PatientWindowCommand), "نتائج التحاليل" (LabResultsCommand).

### ResultDeliveryView filter bar
New buttons: "حساب المريض" (PatientAccountCommand), "تقرير مجمع" (CombinedReportCommand), "تقرير فارغ" (EmptyReportCommand), each with CommandParameter=SelectedItem; safe when no row selected (null guard).


## (e) Converter usage audit (NullToVisibilityConverter / NullToVis)

Usages found (XAML): DeliveryHandoverView.xaml, ProfileEntryView.xaml, CultureEntryView.xaml (notes elements only).
Code (C#): definition + App.xaml registration.
No other screen uses this converter. Therefore changing its behaviour to also collapse empty/whitespace affects ONLY the notes screens — no regression elsewhere. Converter tests added (null, empty, whitespace, text).


## (f) GAP-22 / GAP-30 acceptance criteria table

### GAP-22 (PARTIAL) — Single-test result-entry print/preview/review/main-result (ref 1.22.12.2, 1.22.12.3, 1.22.12.4, 1.6.1.3.2)
| Criterion | Met? | Evidence |
|---|---|---|
| Precondition: single-test result entry open with saved result | n/a | SimpleResultEntryViewModel.cs (no print/preview/review/main-result commands) |
| Action: print | NOT MET | SimpleResultEntryViewModel has no PrintCommand |
| Action: preview | NOT MET | no PreviewCommand |
| Action: review | NOT MET | no ReviewCommand |
| Action: open main result | NOT MET | no OpenMainResult command |
| Validation: disabled when no result saved | n/a | no commands |
| Persistence: no schema change | MET | none added |
| Security: respects permission | n/a | no commands |
| Regression | MET | existing tests pass |
| OD-8 parity (profile/culture) | PARTIAL | ProfileEntryViewModel + CultureEntryViewModel have PrintCommand + ShowReportCommand (preview) but no Review/OpenMainResult; added notes parity in R1/R2 |

### GAP-30 (MISSING) — Composite result screen: open main result (ref 1.22.12.5)
| Criterion | Met? | Evidence |
|---|---|---|
| Precondition: composite result screen open | n/a | CombinedReportViewModel exists |
| Action: open main result | NOT MET | CombinedReportViewModel has no OpenMainResult command (only BuildPreview/Print/InsertFromHistory/AutoInsert/MoveUp/MoveDown) |
| Validation: disabled when no main result | n/a | no command |
| Persistence: no schema change | MET | none added |
| Security: respects permission | n/a | no command |
| Regression | MET | existing tests pass |

Implementation decision: Only actions with existing backing handlers/navigation targets were wired (none new for GAP-22/30 beyond already-present Print/ShowReport). Missing actions (review, open main result, preview on single-test) have no backing handler or navigation target — left unchanged, reported.
