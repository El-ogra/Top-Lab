# R1 Layout Evidence (static analysis; WPF not executable)
Repo: C:\Users\LAP LINK\source\repos\Top-Lab
HEAD: efc909c688ccd3375b6bf3bc15af617f3739c647
Static evidence only. No view was executed.

## Screen A — DeliveryHandoverView.xaml (GAP-04 notes)

File: `src/TopLab.Presentation/Views/Patients/DeliveryHandoverView.xaml` (115 lines). Root: `UserControl` FlowDirection=RightToLeft (L4). Outer container: `Grid Margin="16"` (L5).

Grid.RowDefinitions (L6-L14): 7 rows: Auto, Auto, Auto, *, Auto, Auto, Auto.

| # | Grid.Row | Grid.Column | Type | Notes |
|---|---|---|---|---|
| 1 | 0 | (default 0) | Border (account panel) | L17-L26. StackPanel inside. |
| 2 | **6** | (default 0) | Border (patient notes, GAP-04) | L29-L34. **NEW**. StackPanel with 2 TextBlocks. |
| 3 | 1 | (default 0) | StackPanel (settlement controls) | L37-L44. |
| 4 | 2 | (default 0) | ProgressBar (loading) | L47-L58. Visibility Collapsed unless IsBusy. |
| 5 | 3 | (default 0) | TextBlock (empty state) | L61-L73. Collapsed unless ShowEmpty. |
| 6 | 3 | (default 0) | DataGrid (delivery grid) | L76-L109. **Same cell as #5** (mutually exclusive via Visibility). |
| 7 | 4 | (default 0) | TextBlock (status) | L112. |
| 8 | 5 | (default 0) | TextBlock (error) | L113. |

**Overlap check:** The new notes Border sits in Grid.Row=6. Rows 4,5 (Auto) are occupied by settlement/loading/empty+grid; row 6 is the last Auto row (L13). No other element shares row 6. No overlap.

**Row height:** Row 6 = Auto (L13). A 10-line note: the TextBlock has TextWrapping=Wrap (L32) but NO MaxHeight/ScrollViewer. Auto row grows to fit content, so 10 lines are fully visible (pushes subsequent rows down; there are none after row 6 except none — row 6 is last). No clipping, no scroll.

**Empty note:** When PatientNotes is null/empty, the TextBlock (L32) renders empty; the Border (L29) still renders a light-yellow box (#FFF8E1) with the heading 'ملاحظات المريض:' (L31). So an empty coloured box with a heading and no text is shown.

**Binding validity:** DataContext = DeliveryHandoverViewModel. `PatientNotes` property added in R1 (DeliveryHandoverViewModel.cs). Valid.

**Flow/alignment:** UserControl FlowDirection=RightToLeft (L4). Border has no HorizontalAlignment (defaults Stretch). TextBlock TextWrapping=Wrap. RTL applies.

ASCII layout (top to bottom):
```
[Row0] Account panel (Border) - حساب التسليم
[Row1] Settlement controls (StackPanel horizontal)
[Row2] Loading (ProgressBar) [collapsed unless busy]
[Row3] Empty state (TextBlock) OR Delivery DataGrid (mutually exclusive)
[Row4] Status message (green)
[Row5] Error message (red)
[Row6] *** Patient notes (Border, #FFF8E1) ***  <-- NEW (GAP-04)
```

## Screen B — ProfileEntryView.xaml (GAP-05 notes)

File: `src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml` (146 lines). Root UserControl FlowDirection=RightToLeft (L4). Outer: `Grid Margin="16"` (L5). RowDefinitions (L6-L14): 7 rows Auto, Auto, Auto, *, Auto, Auto, Auto.

| # | Grid.Row | Type | Notes |
|---|---|---|---|
| 1 | 0 | Border (header) | L17-L22. PatientFullName + ProfileName. |
| 2 | 1 | DockPanel (comment) | L25-L28. 'ملاحظة:' + TextBox. |
| 3 | **1** | TextBlock (patient notes, GAP-05) | L31-L32. **NEW**. **Same Grid.Row as #2**. |
| 4 | 2 | ProgressBar (loading) | L35-L46. |
| 5 | 3 | TextBlock (empty state) | L49-L61. Collapsed unless ShowEmpty. |
| 6 | 3 | DataGrid (entry grid) | L64-L98. Same cell as #5. |
| 7 | 4 | Border (report preview) | L101-L128. Collapsed unless HasReport. |
| 8 | 5 | TextBlock (status) | L131. |
| 9 | 5 | TextBlock (error) | L132. |
| 10 | 6 | StackPanel (action buttons) | L135-L144. |

**Overlap check:** The new notes TextBlock (L31) sets Grid.Row="1" (L31) — **same cell as the comment DockPanel (L25, Grid.Row="1")**. In WPF, two elements in the same Grid cell render stacked in document order: DockPanel first, then TextBlock on top. They will NOT overlap visually only if the first leaves room; but since both occupy the full cell, the TextBlock will be drawn over the DockPanel's bottom area (or vice versa depending on z-order/document order). **Potential visual overlap / occlusion** in row 1. The notes TextBlock has Margin="0,4,0,8" (L32) which offsets it 4px from top, but the DockPanel above takes the full row. This is a real layout defect to verify manually.

**Row height:** Row 1 = Auto (L8). The DockPanel (comment) + TextBlock (notes) both in Auto row; row grows to tallest. 10-line note: TextBlock TextWrapping=Wrap (L31) with no MaxHeight/ScrollViewer → fully visible, pushes rows below down. No clipping.

**Empty note:** Binding {Binding PatientNotes} (L31) with no fallback → empty TextBlock renders nothing (no box, no heading). Unlike Screen A, there is no visible container when empty. No empty coloured box.

**Binding validity:** DataContext = ProfileEntryViewModel. `PatientNotes` property added in R1 (ProfileEntryViewModel.cs). Valid.

**Flow/alignment:** FlowDirection=RightToLeft (L4). TextBlock no HorizontalAlignment (defaults Stretch). Foreground=#555 (L32). Margin 0,4,0,8.

ASCII layout:
```
[Row0] Header (Border) - PatientFullName / ProfileName
[Row1] Comment (DockPanel)  +  *** Patient notes TextBlock (GAP-05) ***  <-- SAME ROW, OVERLAP RISK
[Row2] Loading (ProgressBar)
[Row3] Empty state OR Entry DataGrid
[Row4] Report preview (Border, collapsed unless HasReport)
[Row5] Status (green) / Error (red)
[Row6] Action buttons (StackPanel horizontal)
```

## Screen C — CultureEntryView.xaml (GAP-05 notes)

File: `src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml` (221 lines). Root UserControl FlowDirection=RightToLeft (L4). Outer: `Grid Margin="16"` (L5). RowDefinitions (L6-L15): 8 rows: Auto, Auto, Auto, *, Auto, Auto, Auto, Auto.

| # | Grid.Row | Type | Notes |
|---|---|---|---|
| 1 | 0 | Border (header) | L18-L23. TestName + TestCode. |
| 2 | **(default 0)** | TextBlock (patient notes, GAP-05) | L26. **NEW**. **No Grid.Row attribute** → defaults to 0. |
| 3 | 1 | Grid (culture header fields) | L29-L63. 3 cols x 2 rows. |
| 4 | 2 | ProgressBar (loading) | L66-L77. |
| 5 | 3 | TextBlock (empty state) | L80-L92. |
| 6 | 3 | DataGrid (sensitivity grid) | L95-L126. Same cell as #5. |
| 7 | 4 | GroupBox (microscopy) | L129-L179. |
| 8 | 5 | Border (report preview) | L182-L203. |
| 9 | 6 | TextBlock (status) | L206. |
| 10 | 6 | TextBlock (error) | L207. |
| 11 | 7 | StackPanel (action buttons) | L210-L217. |
| 12 | 7 | TextBlock (picked comment preview) | L218-L219. |

**Overlap check:** The new notes TextBlock (L26) has **no `Grid.Row` attribute**. In a Grid, an element without Grid.Row defaults to 0. Therefore it shares **Grid.Row=0** with the header Border (L18). Both occupy the same cell → the TextBlock is drawn on top of the header (or behind, depending on document order: Border first, then TextBlock). **Visual overlap in row 0.** This is a real layout defect.

**Row height:** Row 0 = Auto (L7). Header Border + notes TextBlock both in Auto row. 10-line note: TextBlock TextWrapping=Wrap (L26), no MaxHeight/ScrollViewer → fully visible, pushes header content; row grows. No clipping.

**Empty note:** Binding {Binding PatientNotes} (L26) with no fallback → empty TextBlock renders nothing (no box, no heading). No visible container when empty.

**Binding validity:** DataContext = CultureEntryViewModel. `PatientNotes` property added in R1 (CultureEntryViewModel.cs). Valid.

**Flow/alignment:** FlowDirection=RightToLeft (L4). TextBlock Foreground=#555, Margin="0,0,0,8". No HorizontalAlignment (defaults Stretch).

ASCII layout:
```
[Row0] Header (Border)  +  *** Patient notes TextBlock (GAP-05) ***  <-- NO Grid.Row => defaults 0, OVERLAP
[Row1] Culture header fields (Grid, 3x2)
[Row2] Loading (ProgressBar)
[Row3] Empty state OR Sensitivity DataGrid
[Row4] Microscopy (GroupBox)
[Row5] Report preview (Border)
[Row6] Status (green) / Error (red)
[Row7] Action buttons + picked comment preview
```

## Screen D — PatientEditorView.xaml (GAP-10 envelope button)

File: `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` (540 lines). Root: `UserControl` FlowDirection=RightToLeft (L4). Outer container: `DockPanel LastChildFill=True` (L5).

The new envelope button is at **L50**: `<Button Content="مغلف" ... Command="{Binding PrintEnvelopeCommand}"/>`.
It sits inside a **WrapPanel** (L39-L51) which is inside a **StackPanel** docked Bottom (L20, DockPanel.Dock="Bottom").
The WrapPanel contains (L40-L50): جديد، حفظ، حذف، تراجع، Separator، باركود، ورقة العمل، الإيصال، الفاتورة، طلب تحاليل، **مغلف (NEW)**.

**No Grid involved** for this button — it is in a WrapPanel, so no Grid.Row/Column overlap concern. Flow is RTL (L4), WrapPanel wraps left-to-right visually under RTL.

### PrintEnvelopeCommand / PrintEnvelopeAsync signature & requirements

- Binding: `PrintEnvelopeCommand = new AsyncRelayCommand(async (_, ct) => await PrintEnvelopeAsync(ct));` (VM L153). **No CanExecute predicate** — same as neighbouring print commands (PrintBarcode/Receipt/Invoice/LabOrder all use the same single-arg constructor, VM L148-L152). So the button is always enabled (no CanExecute gating).
- `PrintEnvelopeAsync(CancellationToken)` (VM L1702-L1728): guards `if (!_patientId.HasValue || _patientId.Value <= 0) return;` (L1704-L1707) — silently does nothing if no patient saved (no error message). Sets IsBusy, sends `PrintEnvelopeCommand(_patientId.Value)`, on success sets StatusMessage='تمت طباعة المغلّف.', on failure presents error.
- Handler: `PrintEnvelopeCommandHandler` (src/.../PatientEnvelope/Commands/PrintEnvelope/PrintEnvelopeCommandHandler.cs). Needs: patient exists & not soft-deleted (returns NotFound otherwise), system settings (GetSystemSettingsQuery), IEnvelopePrintingService. Prints an envelope with PatientId, FullName, identifier (barcode payload from lab id or patient id per settings), referral display name (if referral entity exists), registration date, lab id.
- If patient not yet saved: VM guard returns silently (no message) — **inconsistent with PrintAsync which shows 'احفظ بيانات المريض أولًا قبل الطباعة.'** (VM L1734-L1737). This is a UX inconsistency.
- Permission: `PRINT_RESULTS` (PrintEnvelopeCommand implements IAuthorizedRequest, RequiredPermissionCode=ResultsEntryAccessPolicy.PrintResults).
- Validation: `PrintEnvelopeCommandValidator` requires PatientId > 0.

Comparison with neighbouring buttons:
| Button | Command | Guard when unsaved | CanExecute | Permission |
|---|---|---|---|---|
| باركود | PrintBarcodeCommand | shows 'احفظ...' (PrintAsync) | none | (via handler) |
| الإيصال | PrintReceiptCommand | shows 'احفظ...' | none | (via handler) |
| الفاتورة | PrintInvoiceCommand | shows 'احفظ...' | none | (via handler) |
| طلب تحاليل | PrintLabOrderCommand | shows 'احفظ...' | none | (via handler) |
| **مغلف (NEW)** | **PrintEnvelopeCommand** | **silent return, no message** | **none** | **PRINT_RESULTS** |

ASCII layout (PatientEditorView bottom strip):
```
[DockPanel Top] Loading bar (ProgressBar, collapsed unless busy)
[DockPanel Bottom] StackPanel:
   WrapPanel: [جديد][حفظ][حذف][تراجع] | [باركود][ورقة العمل][الإيصال][الفاتورة][طلب تحاليل][مغلف*]
   ErrorMessage (red) / StatusMessage (blue)
[DockPanel Fill] Grid: form (right) + tests (left)
```
* = newly added (GAP-10)

## Screen E — ResultDeliveryView.xaml (GAP-04 Notes column)

File: `src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml` (74 lines). Root: `UserControl` FlowDirection=RightToLeft (L4). Outer: `DockPanel Margin="16"` (L5).

The new column is at **L61**: `<DataGridTextColumn Header="ملاحظات" Binding="{Binding PatientNotes}" Width="120"/>`.

DataGrid ItemsSource = `{Binding Items}` (L49). Item type = the DTO returned by GetDeliveryAccount/GetUndeliveredResults. The DTO must expose `PatientNotes`.
From R1 work: `ResultDeliveryDtos.cs` added `PatientNotes` (default null) to the delivery row DTO; `GetDeliveryAccountQueryHandler.cs` passes `patient.Notes`. Binding path valid IF the ItemsSource item type is that DTO. **Verify:** the test `PatientNotesPropagationTests.cs` confirms `GetUndeliveredResultsQueryHandler` carries `PatientNotes` (line ~60-93). For `ResultDeliveryView` (Items = undelivered list), binding is valid.

Overlap: DataGrid column, no overlap concern. Width=120 fixed.

Empty note: cell shows empty string (no placeholder). RTL alignment inherited.

ASCII layout:
```
[Dock Top] Filters: من/إلى DatePicker + عرض + قصاصة تقرير جماعية + رجوع
[Dock Top] Loading (ProgressBar)
[Dock Bottom] Error (red)
[Fill] DataGrid:
  columns: المريض | Lab ID | التاريخ | الحالة | غير المسلَّم | *** ملاحظات (NEW, 120px) *** | تسليم
```

## Inventory for next round (existing code only; nothing implemented here)

### Navigation targets for unimplemented reference actions

| Reference action | Identifier | Exists? | File path | How to navigate |
|---|---|---|---|---|
| Follow-up | 1.2.2.6.10 | **No dedicated VM** | (none) | No `FollowUpViewModel` exists. Closest: `PatientVisitHistoryViewModel` (src/.../Patients/PatientVisitHistoryViewModel.cs) via `_navigation.NavigateTo<PatientVisitHistoryViewModel>()`. |
| Patient Window / patient list | 1.2.2.6.15 | **Yes** | `src/.../Patients/PatientSearchViewModel.cs` | `_navigation.NavigateTo<PatientSearchViewModel>()` (used in BackCommand of several VMs). |
| Lab Results | 1.2.2.6.16 | **Yes** | `src/.../Patients/PatientResultSheetViewModel.cs` | `_navigation.NavigateTo<PatientResultSheetViewModel>()`. Also `_navigation.NavigateTo<SimpleResultEntryViewModel>()`. |
| Combined Report | 1.4.1.4.8 | **Yes (VM)** | `src/.../Patients/CombinedReportViewModel.cs` | `_navigation.NavigateTo<CombinedReportViewModel>()` (called from PatientVisitHistoryViewModel). |
| Patient Medical History | 1.4.1.4.9 | **No dedicated VM** | (none) | No `PatientMedicalHistoryViewModel`. Closest: `PatientVisitHistoryViewModel` (history reports) via `_navigation.NavigateTo<PatientVisitHistoryViewModel>()`. |
| Delivery Empty Report | 1.4.1.4.10 | **No dedicated VM** | (none) | No `EmptyReportViewModel`. Closest: `BlankReportViewModel` (src/.../Patients/BlankReportViewModel.cs) via `_navigation.NavigateTo<BlankReportViewModel>()` (called from PatientsHubViewModel). |
| Patient Account (on delivery screen) | 1.4.1.4.1 | **Yes** | `src/.../Patients/PatientAccountViewModel.cs` | `_navigation.NavigateTo<PatientAccountViewModel>()` (called from PatientEditorViewModel.OpenAccountAsync and PatientVisitHistoryViewModel). |

### Account panel (payment/discount/extra-charge) implementation

Location: `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` L19-L38 (bottom strip WrapPanel) + `PatientEditorViewModel.cs`.

Fields:
- `PaymentAmountText` (string) — bound to TextBox L33 (Width=80).
- `PaymentDiscountText` (string?) — bound to TextBox L35 (Width=60). Parsed as absolute amount (decimal), validated >= 0.
- `TotalCharged`, `TotalDiscount`, `TotalPaid`, `Balance` (decimal) — display TextBlocks L24-L30.
- `OpenAccountCommand`, `RecordPaymentCommand`, `SettleCommand`.

Commands:
- `RecordPaymentCommand` (VM L146): calls `RecordPaymentAsync` which validates IsEditMode+patientId, parses amount (>0) and optional discount (>=0), sends `RecordPaymentCommand(patientId, amount, discount)`, then `RefreshBillingAsync`. Discount is **absolute value only** — no percentage field, no +-prefix, no Enter-key handling.
- `SettleCommand` (VM L147): `SettleAsync` (not shown in detail here).
- `OpenAccountCommand` (VM L178): navigates to PatientAccountViewModel.

Key handling: **none** — no KeyDown/Enter handling found in PatientEditorViewModel for the payment fields. The reference requires two Enter presses (1.3.1.4.2, L979/L1633): first computes, second saves. Not implemented.
Discount: absolute value field only (1.3.1.2, L975). Percentage field (1.3.1.3, L976) not present in XAML.
Surcharge (+-prefix, 1.3.1.4.1, L978/L1632): not implemented — PaymentAmountText is parsed as plain decimal; no '+' prefix handling.
Clear account (1.3.2.7, L1002): no command in VM (no ClearAccount).

### Anything that cannot be determined statically
- Actual rendered pixel positions / clipping of the notes TextBlocks (requires runtime layout).
- Whether the ProfileEntry row-1 overlap actually occludes the comment box at runtime (z-order/document order suggests yes, but runtime may differ).
- CanExecute gating: AsyncRelayCommand in this codebase apparently has no CanExecute overload used here (all print commands use single-arg constructor). Whether the button visually disables when no patient is loaded cannot be determined from XAML alone; VM has no CanExecute predicate.
