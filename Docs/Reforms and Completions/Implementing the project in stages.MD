# Implementing the project in stages

**خارطة تنفيذ مُفصَّلة Bringing TopLab إلى التكافؤ الوظيفي مع RealLab**

| | |
|---|---|
| **المشروع** | TopLab (`https://github.com/El-ogra/Top-Lab.git`) |
| **الـ commit المُدقَّق** | `9b912c1a0ec51fb9786831a7d8b3362141e78e18` |
| **حالة HEAD** | **لم يتقدّم** — تحقّق عبر `git ls-remote origin HEAD` (الفرع الافتراضي البعيد = نفس الـ hash) |
| **نظام المرجع** | RealLab — `RLS_Learn.pdf` (212 ص) + `RL_Show.pdf` (77 ص)، في `Docs/Reference system files/` |
| **نطاق التغطية** | 84 من 87 متطلباً · 3 مستبعَدة (Natigh) |
| **عدد حزم العمل** | **30** حزمة (WP-01 … WP-30) |
| **عدد الهجرات** | **20** هجرة EF + 1 تعديل تعداد + إضافات `HasData` مدمجة فيها (ملحق ج §ج.1/§ج.2/§ج.3) |
| **حالة المستودع** | للقراءة فقط — `git status --porcelain` فارغ قبل وبعد التحقق |
| **تاريخ الخطة** | 2026-09-30 |

---

## المصادر المستخدمة

| # | المرفق | الدور في هذه الخطة |
|---|---|---|
| A | `Project_completion_status.md` | تدقيق 87 متطلباً (R-01..R-87)، مصفوفة تتبّع، سجل عيوب D-01..D-34 |
| B | `الوكيل الفرعي الأول.MD` | تدقيق Cap 1..12 (السجل، الطلب، المحاسبة، النتائج، التقرير المركّب، الفارغ، المزرعة، التاريخ، التعليقات، التدقيق، الباركود، السحب) + D1..D9 العابرة |
| C | `الوكيل الفرعي الثاني.MD` | تدقيق A..G (الكتالوج، الجهات، المزرعة، المستخدمون، الإحصائيات، الحسابات، الإعدادات) + عيوب #1..#14 |
| — | **ملفَّا PDF للمرجع** | **لم يُفتحا في هذه الخطة.** كل استشهاد `p.NN` في هذا الملف **موروث حرفياً** من `Project_completion_status.md` §4/§5 ولم يُعد التحقق منه. أي تنقيح يجب أن يعود إلى ذلك التقرير. |
| D | ناتج وكيل تخطيط سابق (جزئي) | التحقق المستقل لسبعة عيوب P0 — أُعيد التحقق منها ولم تُفتح للنقاش |

### تحقق مستقل لسبعة عيوب P0

أُعيد فتح المستودع على نفس الـ commit وأُعيد التحقق من كل عيب بنفسه:

| # | العيب | الدليل المُعاد التحقق منه |
|---|---|---|
| 1 | إدخال النتائج غير قابل للوصول | `ResultsWorklistViewModel.cs:42,146` (الأمر مُنشأ) · `ResultsWorklistView.xaml:47` `<DataGrid>` بلا `Command=`/`SelectedItem=`/`MouseBinding` · `grep OpenDetailCommand` لا يُرجع مرجعاً خارج الـ VM |
| 2 | إزاحة فئة الحساسية عند الحفظ | `SensitivityCategory.cs:3-7` = `HighlyFor=0 … ResistantFor=3` مقابل `CultureEntryView.xaml:111` `SelectedIndex` على 4 عناصر |
| 3 | `ReportPdfWriter` يكسر العربي ويتجاهل الإعدادات | `ReportPdfWriter.cs:237-251` `ToAscii` · `:163` `/MediaBox [0 0 595 842]` · `:142` موضع خط ثابت · `ArabicFontResolver` مستخدَم في `InvoicePdfWriter:54` و`ReceiptPdfWriter:53` فقط |
| 4 | تعذّر إنشاء جهة إحالة/تعاقد | `ExternalEntityEditorViewModel.cs:157-161` يمرّر `null, null` · `ExternalEntity.cs:159,168` يرمي `ReferralOrContract requires PriceListId.` · النافذة بلا أداة اختيار |
| 5 | طباعة وهمية تُفسد سجل التدقيق | `ProfileEntryViewModel.cs:331-358` · `CultureEntryViewModel.cs:321,333,336` · `ExecuteBulkPrintCommandHandler.cs:15-17` اعتماداته `_db, _currentUser, _clock` فقط |
| 6 | تسرّب بيانات بين المرضى | `BuildCombinedReportCommandHandler.cs:34-38` بلا `pt.PatientId == request.PatientId` · المدقّق قاعدتان فقط |
| 7 | غياب محدّدات تبويب جرد العناصر | `AccountsHubView.xaml:106-137` · `grep "ElementKind\|ReportType\|ElementId\|AccountTypeFilter\|CommissionsAndShares"` = **لا نتائج** |

**تصحيح تسمية:** تقرير الوكيل الفرعي الثاني يشير إلى `ExternalEntityEditorView.xaml`؛ الملف الفعلي هو `ExternalEntityEditorWindow.xaml` (لا يوجد ملف بهذا الاسم).

**بنية المجلدات المُتحقَّق منها** (تُستخدم أدناه في قوائم الملفات المتأثرة):

```
src/TopLab.Application/Features/   →  AccessAndNavigation · AnalyteProfiles · Attendance
                                       AuditAndTraceability · CultureAndAntibiotics
                                       CultureResults · ExternalEntities
                                       InventoryAndAccounting · PatientBilling
                                       PatientRegistration · PatientSearch
                                       PriceListsCommentsAndCustomGroups · ProfileResults
                                       ReportProduction · ResultDelivery · ResultsEntry
                                       SampleCollection · SentOutSamples · Statistics
                                       SystemAndPrintSettings
                                       TestCatalogAndReferenceRanges
                                       UsersAndPermissions · Utilities · WorkSheets
src/TopLab.Domain/                  →  Accounting · Attendance · Billing · Common
                                       ExternalEntities · PatientStatus · Patients
                                       Properties · Reports · Results · SentOutSamples
                                       Settings · Tests · Users · Utilities
src/TopLab.Infrastructure/Printing/ →  ArabicFontResolver · IPdfPrinterDispatcher
                                       IReportPdfWriter · InvoicePdfWriter
                                       InvoicePrintingService · ReceiptPdfWriter
                                       ReceiptPrintingService · ReportPdfWriter
                                       ReportPrintingService · ShellPdfPrinterDispatcher
                                       WorkSheetPdfWriter · WorkSheetPrintingService
                                       (لا يوجد Envelope* ولا Card* ولا PriceList*)
tests/                              →  TopLab.Domain.Tests (474) · Application.Tests (1,484)
                                       Infrastructure.Tests (198) · Persistence.Tests
                                       Presentation.Tests
```

**الهجرات القائمة (8):** `BaselineDataModel` · `RenamePkColumns` · `AddTestCodeAndLifecycleColumns` · `AddPatientIsDeletedAndPatientTestSampleDrawnIndex` · `AddPatientTestReferenceRangeSnapshots` · `AddAnalyteProfileDomain` · `AddPregnancyMedicalConditionTypeSeed` · `AddInvoiceIssues`

---

## 1) أهداف المالك ونطاق الاستثناء

**الهدف:** أن يصبح TopLab مكافئاً وظيفياً لـ RealLab في عمل المختبر اليومي: التسجيل، الطلب، المحاسبة، إدخال النتائج، المراجعة، التسليم، أوراق العمل، المزارع، المستخدمون والصلاحيات، الحضور، الحسابات، الإحصائيات، الإعدادات، والطباعة — مع محدّدات واجهة ومخرجات مطبوعة مطابقة لسلوك المرجع.

**الاستثناء الملزم:** كل ما يخص Natigh / نتيجة.كوم / أي موقع أو بوابة ويب لمشاركة نتائج المرضى — رفع النتائج، حسابات المريض/الطبيب/المختبر، تصدير Word/PDF من الويب، تطبيق أندرويد، عدّادات Natigh، الحظر من الموقع، وطباعة بيانات الموقع على الإيصال. **خارج النطاق كلياً**. لا يُقترح بديل أصغر، ولا واجهة رفع، ولا API رفع، ولا «إدارة حسابات على سطح المكتب» كبديل.

**المتطلبات المستبعَدة:** R-29 · R-30 · R-31 (والعيب D-14) — تُدرَج في **ملحق أ** ليُرى أنها أُخذت في الحسبان واستُبعدت بقصد. أي ميزة لها شظيّة Natigh تُخطَّط **بدون** تلك الشظيّة، مع التصريح بذلك في متن الحزمة.

---

## 2) مبادئ المعمارية الملزمة

| # | المبدأ | التطبيق العملي |
|---|---|---|
| 1 | Clean Architecture محفوظة | `Domain → Application → Infrastructure → Presentation`. لا إعادة كتابة، لا تغيير إطار، لا منطق أعمال في `code-behind` |
| 2 | CQRS كما هو | MediatR + سلوكيات `Logging → Validation → Authorization`. `IAuthorizedRequest` + رموز صلاحيات مزروعة، مع إبقاء تجاوز `IsAbsolutePermission` |
| 3 | EF Core 8 + SQL Server | معرّفات قوية النوع، التهيئة تحت `Infrastructure/Persistence/Configurations`، **كل** تغيير مخطط = هجرة صريحة مُسمّاة |
| 4 | WPF + MVVM | `RelayCommand`/`AsyncRelayCommand`، واجهة عربية RTL. ممنوع المنطق في `Code-Behind` |
| 5 | الطباعة على QuestPDF | نمط `I*PrintingService` / `IPdfPrinterDispatcher` نفسه + `ArabicFontResolver`. **يُشجَّع** على هجرة كاتب التقرير إلى QuestPDF |
| 6 | الاختبارات | xUnit + بدائل hand-rolled. **ممنوع** إدخال Moq / NSubstitute / أي مكتبة mocking |
| 7 | صفر الشيفرة الوهمية | لا `NotImplementedException`، لا `TODO`، لا معالج `return Task.CompletedTask` خلف خاصية معلنة |
| 8 | الصدق في المخرجات | إما مستند حقيقي أو خطأ صريح. لا «تمت الطباعة» بلا ورقة |
| 9 | تسمية متسقة | أسماء الملفات/الأوامر/الهوّيات تتبع أسماء المجلدات المُثبتة أعلاه |
| 10 | تتبّع كامل | كل `R-xx` مقصود مرتبط بـ `WP-xx` في ملحق ب |

---

## 3) ملخص التنفيذ بالترتيب

| WP | العنوان | الأولوية | الاعتماديات | الحجم | أثر الهجرات |
|---|---|---|---|---|---|
| **WP-01** | كتابة التقرير على QuestPDF + معاينة الطباعة | **P0** | — | **XL** | none |
| **WP-02** | إحياء إدخال النتائج والوصول للشاشات اليتيمة | **P0** | — | S | none |
| **WP-03** | تصحيح ترميز فئة الحساسية + ترميم البيانات | **P0** | — | S | **migration + seed** |
| **WP-04** | محرر جهة الإحالة/التعاقد (قائمة أسعار + عمولة + بريد) | **P0** | — | M | none |
| **WP-05** | حارس ملكية التقرير المركّب | **P0** | — | S | none |
| **WP-06** | منسّق طباعة صادق: مستند حقيقي أو خطأ صريح | **P0** | WP-01 | M | none |
| **WP-07** | تعليقاه المدى الأدنى/الأعلى حتى المخرج | **P0** | WP-01 | S | none |
| **WP-08** | الوحدات وأنواع الباركود ووحدة الصنف | P1 | — | M | **migration** |
| **WP-09** | مركز الحسابات: 5×4 + عمولات + تنزيل + فترات جاهزة | P1 | WP-04 | M | none |
| **WP-10** | تقارير التاريخ: الوصول + صحّتها + مصفوفة CBC | P1 | WP-01, WP-02 | L | none |
| **WP-11** | زر «تحديث القيمة المرجعية» | P1 | WP-02 | S | none |
| **WP-12** | اكتمال شبكة إدخال النتائج وشاشة الإدخال البسيط | P1 | WP-02, WP-08 | M | none |
| **WP-13** | خيارات التقرير المركّب + ملاحظة «خارج المعمل» + تعليقات الاختبار | P1 | WP-01, WP-02 | M | **migration** |
| **WP-14** | عمق المزرعة: الفحص المجهري، منطقة التثبيط، الأسماء التجارية | P1 | WP-01, WP-03, WP-06 | L | **migration ×2** |
| **WP-15** | مفهوم الفرع / تعدّد الفروع | P1 | — | L | **migration** |
| **WP-16** | فلاتر البحث الـ13 عن المرضى | P1 | WP-15, WP-18 | M | none |
| **WP-17** | اكتمال كتالوج الاختبارات | P1 | WP-08 | M | **migration** |
| **WP-18** | تكافؤ التسجيل والمحاسبة | P1 | — | L | **migration ×2** |
| **WP-19** | الإحصائيات: تجميعات + كلمة المرور الثانية + تقرير المتابعة | P1 | WP-15 | M | none |
| **WP-20** | وفاء إعدادات التقرير (رأس/تذييل، مواضع، ألوان، غامق) | P2 | WP-01 | L | **migration** |
| **WP-21** | وفاء إعدادات الإيصال | P2 | — | M | **migration** |
| **WP-22** | كاتب المظروف + إعدادات الكارنيه + طابعة Card | P2 | WP-01 | M | **migration ×2** |
| **WP-23** | وفاء الباركود: المقاس، الاتجاه، الرمز، ملصقات الأنابيب | P2 | WP-08, WP-17 | M | **migration** |
| **WP-24** | إكمال إعدادات النظام | P2 | WP-15, WP-22, WP-04 | M | **migration** |
| **WP-25** | فصل وسحب العينات: كيان الأنبوب + «أنبوب آخر» | P2 | WP-01 | L | **migration** |
| **WP-26** | كاشف تسجيل الدخول/الخروج | P2 | — | M | **migration** |
| **WP-27** | طباعة قائمة الأسعار والمضادات + لمسات الكتالوج والفوترة | P2 | WP-01, WP-14 | M | **migration** |
| **WP-28** | صقل التكافؤ منخفض الشدة | P3 | WP-12, WP-18 | M | **migration + seed** |
| **WP-29** | سلامة البيانات وقابلية التشخيص | P1 | WP-01, WP-06, WP-12 | M | none |
| **WP-30** | مسار تعديل للنتائج البسيطة والمزارع + صقل أمان التعديل | **P1** | WP-02, WP-12 | M | **migration جديدة** |

### الخريطة حسب الموجة

> **[C19] حُذفت هذه الخريطة من هنا عمداً.** كانت تناقض §5.2/§5.3 في خمس مواضع
> (WP-08 · WP-13/14/29 · WP-23 · غياب WP-30)، وكانت الجملة أسفلها تدّعي أن الموجة 1
> «خالية من الهجرات» وهي **كاذبة**: الموجة 1 فيها `WP-03` و`WP-04` و`WP-15`،
> وثلاثتها **هجرات** (`FixCultureSensitivityCategoryOffByOne` · `AddExternalEntityEmail` ·
> `AddBranchNumber`).
>
> **المصدر الوحيد للموجات الآن: §5.2 (جدول الباينة) و§5.3.** أي عدد هنا أو هناك
> يجب أن يُشتق من ذلك الجدول؛ وأي تعارض يُحلّه §5.2،لأنه مُولَّد منه.

**نقطة جوهرية مُصحَّحة:** الموجة 1 فيها **ثلاث** هجرات لا واحدة. إحياء إدخال النتائج
(WP-02) ومنع تسرّب المرضى (WP-05) **بلا** هجرة. أما تصحيح الحساسية (WP-03)
وبريد جهة الإحالة (WP-04) ورقم الفرع (WP-15) فتحتاج هجرات صراحةً، فلا يصحّ عدّ
الموجة رخيصةً من حيث المخطط.

---

## 4) حزم العمل بالتفصيل

قالب كل حزمة يتبع: الأولوية · المرجع · الحالة الحالية · الحالة المستهدفة · المكونات (Domain / Application / Infrastructure / Presentation / Tests) · خطوات التنفيذ · الاعتماديات · المخاطر · معايير القبول · الحجم · أثر الهجرات.

### قالب موسّع مستخدم في هذا الملف

إضافةً للقالب المطلوب، كل حزمة تُدرج:
- **الملفات المتأثرة** — قائمة صريحة بمسارات الملفات التي ستُعدَّل أو تُضاف.
- **التراجع (Rollback)** — كيف تُنهى الحزمة إن فشلت.

---

### WP-01 — كتابة التقرير على QuestPDF + معاينة الطباعة

| | |
|---|---|
| **الأولوية** | **P0** |
| **سبب الأولوية** | المخرج السريري الأساسي غير مقروء اليوم. ست متطلبات من الطبقة الأولى موصوفة «مكتملة» تنتهي عند هذا الكاتب — أي أن صفراً منها قابل للتسليم عملياً. |
| **المرجع** | R-17, R-20, R-32, R-33, R-35, R-47, R-81 · D-01, D-02, D-08, D-11, D-12 |
| **الحجم** | **XL** (5–8 أيام عمل) |
| **الهجرات** | **none** — البنية التحتية فقط |
| **الاعتماديات** | لا شيء. **هذا هو WP الجذر** الذي يفكّ كل اعتماديات الطباعة اللاحقة. |

### الحالة الحالية — PROVED DEFECT

| الملاحظة | الدليل |
|---|---|
| كل محرف غير ASCII يُستبدل بـ `?` | `ReportPdfWriter.cs:237-251` (`ToAscii`) |
| **كاتب ثانٍ يكسر العربية — خارج هذه الحزمة** | `Infrastructure/Services/PatientReportPdfExporter.cs:38-57` — `:38` يُصدر `"TopLab Patient Report - Patient …"`، `:39` `ToAscii(data.PatientFullName)`، `:44,:49,:52,:57` تسميات إنجليزية. يُستدعى عبر `ExportPatientReportPdfCommandHandler.cs:14,163` ويُسجَّل في `DependencyInjection.cs:63` |
| **تسميات إنجليزية داخل التقرير العربي** | `ReportPdfWriter.cs:47-49` `LabId:`/`PatientId:`/`Name:` · `:50` `Paper:` · `:51` `TopSpace:` · `:55` `HeaderFooter:` · `:60` السلسلة الحرفية `"Doctor Signature: Yes"` |
| حجم الصفحة مثبّت A4 ⇒ `PaperSize.A5` بلا أثر | `ReportPdfWriter.cs:163` `/MediaBox [0 0 595 842]` |
| الموضع مثبّت ⇒ الهوامش لا تُقرأ | `ReportPdfWriter.cs:142` `BT /F1 12 Tf 50 800 Td` |
| المسافة العلوية تُطبع كنص لا تُستعمل تموضعاً | `ReportPdfWriter.cs:51` |
| وضع الرأس/التذييل يُطبع كنص ولا يُرسم شيء | `ReportPdfWriter.cs:53-61` |
| لا ترقيم صفحات | تيار محتوى واحد |
| النمط الصحيح موجود في المشروع بالفعل | `ArabicFontResolver.cs:15` مستخدَم في `InvoicePdfWriter.cs:54` و`ReceiptPdfWriter.cs:53` و`WorkSheetPdfWriter` |
| عقد الواجهة سليم — الخلل في التنفيذ | `IReportPdfWriter.cs:18` `(absolutePath, envelope, reportSettings, systemSettings, ct)` |
| نسخة ثانية من `ToAscii` مكرَّرة | `BarcodeService.cs:207-221` |

### الحالة المستهدفة

يُطبع اسم المريض واسم الاختبار واسم الطبيب واسم جهة الإحالة وتعليق النطاق بالعربية بشكل مقروء؛ ويُحترم `PaperSize` (A4/A5) و`PageMarginLeftCm`/`PageMarginBottomCm` و`ReportTopSpaceCm` ووضع الرأس/التذييل وتوقيع الطبيب **فعلياً على الصفحة**؛ ويقسّم التقرير الطويل صفحات؛ ويعرض المستخدم معاينة PDF حقيقية قبل الإرسال للطابعة.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Infrastructure/Printing/ReportPdfWriter.cs          ← إعادة كتابة كاملة
  src/TopLab.Infrastructure/Printing/ReportPrintingService.cs   ← استدعاء المعاينة + تسجيل
  src/TopLab.Application/Features/ReportProduction/Common/ReportDtos.cs  ← إضافة حقول (WP-07)
  src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/CombinedReportViewModel.cs
  src/TopLab.Presentation/ViewModels/WorkSheets/WorkSheetsViewModel.cs

يُعدَّل (C3 — موجودة، تُعاد كتابتها لا تُنشأ):
  src/TopLab.Infrastructure/Services/PatientReportPdfExporter.cs  ← حذف BuildMinimalPdf ونسخة ToAscii
  src/TopLab.Application/Features/ResultsEntry/Commands/ExportPatientReportPdf/ExportPatientReportPdfCommandHandler.cs

يُضاف:
  src/TopLab.Infrastructure/Printing/ReportDocument.cs           ← مستند QuestPDF
  src/TopLab.Infrastructure/Printing/ReportPageComposer.cs       ← بناء رأس/جسم/تذييل
  src/TopLab.Infrastructure/Printing/ReportCultureSection.cs     ← عقد مستقر لكتلة المزرعة
  src/TopLab.Application/Features/ReportProduction/Common/IPdfPreviewService.cs
  src/TopLab.Infrastructure/Printing/PdfPreviewService.cs
  src/TopLab.Application/Features/ReportProduction/Common/ITestDisplayNameResolver.cs
  src/TopLab.Infrastructure/Printing/TestDisplayNameResolver.cs
  tests/TopLab.Infrastructure.Tests/Printing/ReportPdfWriterGoldenTests.cs
  tests/TopLab.Infrastructure.Tests/Printing/PdfPreviewServiceTests.cs
```

### المكونات

**Domain** — لا تغيير. العقد القائم `ReportSettings` و`SystemSettings` كافٍ.

**Application**
- `IPdfPreviewService` في `Features/ReportProduction/Common`:
  ```csharp
  public interface IPdfPreviewService
  {
      Task<Result<string>> PreviewAsync(
          string absolutePdfPath, CancellationToken cancellationToken = default);
      // يُرجع المسار عند النجاح، و Error.Unexpected برسالة عربية عند الفشل
  }
  ```
- `ITestDisplayNameResolver` — يفصل قراءة أسماء الاختبارات عن الكاتب (كسر دائرية مع WP-17):
  ```csharp
  // [C21(a)] المعامل هو CombinedReportLineDto — وهو DTO مسار التقرير الفعلي.
  // لا يوجد نوع اسمه TestSummaryRow؛ وTestSummaryDto (TestCatalogAndReferenceRanges)
  // هو DTO الكتالوج ولا يصل إلى الكاتب. التحويل من Catalog → Report سطر في WP-17.
  public interface ITestDisplayNameResolver
  {
      string ResolveReportName(CombinedReportLineDto line);   // WP-01: TestName فقط
      string ResolveHistoryName(CombinedReportLineDto line);  // WP-01: TestName فقط
  }
  ```
- تعديل `BuildCombinedReportCommandHandler` لتغذية `CombinedReportLineDto` ببقايا المدى المجمّدة (تنفيذ WP-07 ضمن هذه الحزمة لأنه يُنفَّذ ضمن هذه الحزمة مع الكاتب).

**Infrastructure**
- `ReportDocument : IDocument` على QuestPDF، ملتزم بنمط `ReceiptPdfWriter.cs:53`:
  - `ArabicFontResolver.Resolve(...)` لاختيار الخط.
  - `DirectionFromRightToLeft` على المستند والجداول.
  - `page.Size(...)` من `ReportSettings.PaperSize` (A4 = 595×842pt، A5 = 420×595pt).
  - `page.MarginTop(ReportTopSpaceCm.ToPoints())` و`MarginLeft/MarginRight` و`MarginBottom` من الإعدادات.
  - كتلة رأس وتذييل حقيقية بحسب `HeaderFooterMode`.
  - سطر توقيع الطبيب عند تفعيله.
  - ترقيم «صفحة X من Y» في التذييل.
- فروع المحتوى الأربعة: سطر اختبار بسيط · سطور صنف متعدد (`ProfileLines`) · سطر مزرعة · كتلة تاريخ.
- `ReportCultureSection` — **عقد مستقر يتحمّل الحقول الناقصة (null-safe)**. هذه هي آلية كسر الدائرة مع WP-14: الكاتب يُبنى أولاً بعقد يفترض غياب البيانات، وWP-14 يملأ الحقول لاحقاً دون تعديل الكاتب.
- `PdfPreviewService` — يكتب الملف في مجلد مؤقت مخصّص، يفتحه في العارض المهيّأ، ويُرجع المسار.
- توحيد مسار التنظيف والتسجيل مع `BarcodeService` (تفصيله في WP-29).

**Presentation**
- زر «معاينة الطباعة» في الشاشات الثلاث يستدعي `IPdfPreviewService` بدل لوحة `TextBlock` الحالية.
- المعاينة **لا ترسل** إلى الطابعة.
- عند فشل المعاينة: رسالة عربية + تسجيل الاستثناء.

**Tests** (الأسماء مقبولة حرفياً)
```
ReportPdfWriter_ContainsArabicPatientName
ReportPdfWriter_ContainsArabicTestName
ReportPdfWriter_HonoursPaperSizeA5
ReportPdfWriter_HonoursPaperSizeA4
ReportPdfWriter_AppliesLeftAndBottomMargins
ReportPdfWriter_AppliesTopSpaceSetting
ReportPdfWriter_DrawsHeaderWhenModeIsWords
ReportPdfWriter_DrawsFooterWhenModeIsWords
ReportPdfWriter_DrawsDoctorSignatureWhenEnabled
ReportPdfWriter_PaginatesContentBeyondOnePage
ReportPdfWriter_RendersCultureSectionWhenPresent
ReportPdfWriter_ToleratesMissingCultureFields
ReportPdfWriter_HistorySection_PrintsDates
ReportPdfWriter_BlankReport_RendersPatientBlock
PdfPreviewService_CreatesFileBeforeReturning
PdfPreviewService_ReturnsErrorOnFailure
Printing_ContainsNoToAsciiHelper          ← [C21b] يُقيَّم على ReportDocument + ReportPdfExporter فقط.
                                              فحص BarcodeService المؤجَّل إلى WP-23؛
                                              سجّل ذلك صراحةً في WP-23 لا هنا.
ReportPdf_ContainsNoEnglishLabels
ReportPdfExporter_ContainsArabicPatientName
```

### خطوات التنفيذ

1. **قراءة النمط المرجعي.** اقرأ `ReceiptPdfWriter.cs` كاملاً واستخرج: تهيئة المستند، حلّ الخط، الاتجاه RTL، حجم الصفحة، الهوامش، بناء التذييل. هذا هو القالب الحرفي.
2. **إنشاء `ReportDocument`.** صنف واحد يحوي `ComposeHeader` / `ComposeBody` / `ComposeFooter` / `ComposePageFooter`.
3. **الخط والاتجاه.** نداء `ArabicFontResolver.Resolve` مرة واحدة في `ConfigureDocument` وتخزين النتيجة. **اختبار مبكر:** ورقة اختبار ذهبية باسم عربي كثيف + اسم طبيب + اسم إحالة قبل كتابة أي شيء آخر. إن فشل هذا، أوقف WP-01 وأصلح `ArabicFontResolver` أولاً.
4. **الهندسة من الإعدادات.** `PageSizeMapper.ToPoints(PaperSize)` و`CmToPoints(decimal)` كدالتين صريحتين في `Infrastructure/Printing` (لا حسابات مباشرة في الكود).
5. **كتلة الرأس.** اسم المعمل، شريط «Computer Software / real lab system» إن وُجد في `SystemSettings`، وحقول الرأس المتاحة حالياً.
6. **كتلة الجسم.** حسب `ReportPrintEnvelope.ReportKind`:
   - `Combined` → شبكة أعمدة: اسم التحليل · النتيجة · الوحدة · المدى · العلم · التعليق.
   - `Blank` → كتلة بيانات المريض فقط.
   - `History` → جدول بالتاريخ كصفوط وصنف كأعمدة.
7. **كتلة التذييل.** رقم التقرير، التاريخ، «صفحة X من Y».
8. **سطر التوقيع.** عند `DoctorSignatureEnabled`.
9. **الترقيم.** `.Pagination()` في التذييل.
10. **حذف `ToAscii`.** احذفه من مسار التقرير كلياً. إن بقي في `BarcodeService.cs:207-221` فادمج الدالة في مكان واحد بدل تكرارها (تُغلق في WP-23).
11. **`IPdfPreviewService`.** الواجهة في Application، التنفيذ في Infrastructure، التسجيل في `DependencyInjection`.
12. **الربط.** أزرار المعاينة الثلاثة. تأكد أن `ShowReportAsync:360-382` لم تعد تبني لوحة نصّية.
13. **WP-07 المدمج.** أضف `LowComment`/`HighComment` إلى `CombinedReportLineDto` واملأهما من `PatientTestReferenceRangeSnapshot` المجمّد، وارسمهما شرطياً.
14. **تنظيف `IReportPdfWriter`.** أبقي على نفس التوقيع — هو سليم.
15. **[C3] استيعاب `PatientReportPdfExporter`.** يعيد بناء البايتات بـ `BuildMinimalPdf` (`:34-57`). اجعله يجمع نفس `ReportDocument` الذي يخدم `IReportPdfWriter` ويكتب نفس الملف — ثم احذف `BuildMinimalPdf` ونسخة `ToAscii` من هذا الملف. **لا** يحتفظ بمسار تصدير ثانٍ بكاتب ASCII.
16. **[C15] احذف كل التسميات الإنجليزية.** `LabId:` · `PatientId:` · `Name:` · `Paper:` · `TopSpace:` · `HeaderFooter:` · `"Doctor Signature: Yes"` — كلها تُستبدل بعناوين عربية أو برسم حقيقي للمواضع. **اختبار `ReportPdf_ContainsNoEnglishLabels` إلزامي.**

### الاعتماديات
لا شيء. **مطلوب قبل:** WP-06, WP-07, WP-10, WP-13, WP-14, WP-20, WP-22, WP-25, WP-27, WP-29.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تقنية | QuestPDF قد لا يخطّط RTL بنفس جودة الإيصال | اختبار ذهبي مبكر (خطوة 3) قبل بقية العمل |
| تقنية | الأعمدة العريضة قد تلتفّ أو تُقصّ | `ShrinkWrap` + `PageBreak` قبل الجدول |
| تجربة مستخدم | شكل التقرير يتغيّر بصرياً عن الوضع الحالي (المكسور) | اعرض نموذجاً على المالك قبل الترحيل النهائي |
| بيانات | لا شيء | — |

### معايير القبول
- [ ] `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` ⇒ 0/0
- [ ] كل الاختبارات الـ17 المذكورة خضراء
- [ ] فحص يدوي: تقرير باسم عربي ومقاس A5 يُخرج صفحة A5 مطبوعاً بالعربية
- [ ] فحص يدوي: تقرير من 3 صفحات ينتج 3 صفحات مرقّمة
- [ ] `grep -rn "ToAscii" src/TopLab.Infrastructure/Printing/` ⇒ **صفر نتيجة**
- [ ] `grep -c "MediaBox" ReportPdfWriter.cs` ⇒ صفر (أصبح `page.Size`)
- [ ] المعاينة تُنشئ ملفاً **قبل** أي إرسال للطابعة

### التراجع (Rollback)
الكاتب القديم ملف واحد. احتفظ بنسخة `.bak` خارج المشروع في فرع مؤقت، أو اعتمد على Git: `git revert <merge-commit>` يعيد نظاماً يعمل (مكسوراً لكنه قابل للبناء). **لا تعتمد** على تراجع جزئي داخل الملف.

---

### WP-02 — إحياء إدخال النتائج والوصول للشاشات اليتيمة

| | |
|---|---|
| **الأولوية** | **P0** |
| **سبب الأولوية** | ثلاث شاشات إدخال نتائج مسجّلة في DI ومُنمَّطة في `MainWindow.xaml` ولا يصلها أي مسار حيّ. إ Britain's المراجع لا يستطيع إدخال نتيجة أصلاً. |
| **المرجع** | R-01, R-16, R-20, R-33 · تقرير الوكيل الفرعي الأول: Critical Finding |
| **الحجم** | S (1–2 يوم) |
| **الهجرات** | **none** |
| **الاعتماديات** | لا شيء. **مطلوب قبل:** WP-10, WP-11, WP-12, WP-13. |

### الحالة الحالية — PROVED DEFECT

| الملاحظة | الدليل |
|---|---|
| الأمر مُنشأ ومعلَن ولا يُستدعى | `ResultsWorklistViewModel.cs:42` الإنشاء · `:146` الإعلان |
| الشبكة بلا أي ربط تنقّل | `ResultsWorklistView.xaml:47` `<DataGrid ItemsSource="{Binding Items}">` — بلا `Command=` ولا `SelectedItem=` ولا `MouseBinding` |
| `OpenDetailAsync` هو المُلاحِ الوحيد | `ResultsWorklistViewModel.cs:172-208` (`:184` بسيط، `:193` صنف، `:202` مزرعة) |
| `BlankReportViewModel` يتيم | `grep NavigateTo<BlankReportViewModel>` ⇒ صفر. مسجّل `DependencyInjection.cs:61`، مُنمَّط `MainWindow.xaml:140` |
| `HistoryReportsViewModel` يتيم | `grep NavigateTo<HistoryReportsViewModel>` ⇒ صفر. مسجّل `:62`، مُنمَّط `:144` |
| الـ Hub أربعة أزرار فقط | `PatientsHubView.xaml:10-17` |
| شيفرة وهمية | `BlankReportViewModel.cs:202` `await Task.CompletedTask` |

### الحالة المستهدفة
نقرة (مزدوجة أو زر «فتح») في سطر قائمة العمل تفتح الشاشة الصحيحة حسب `ResultKind`؛ ويصبح زر «تقرير فارغ» وزر «تقارير التاريخ» في `PatientsHubView`؛ ولا يبقى أي ViewModel مُسجَّل بلا متصل.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Presentation/Views/Patients/ResultsWorklistView.xaml
  src/TopLab.Presentation/ViewModels/Patients/ResultsWorklistViewModel.cs
  src/TopLab.Presentation/Views/Patients/PatientsHubView.xaml
  src/TopLab.Presentation/ViewModels/Patients/PatientsHubViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/BlankReportViewModel.cs
  tests/TopLab.Presentation.Tests/PresentationStructuralTests.cs

يُضاف:
  src/TopLab.Presentation/Views/Patients/Converters/RowCommandConverter.cs
  tests/TopLab.Presentation.Tests/Navigation/OrphanedViewModelTests.cs
```

### المكونات

**Domain** — لا تغيير.

**Application** — لا تغيير. المعاملات `SimpleResultEntryQuery` و`GetResultEntryQuery` و`GetCultureEntryGridQuery` قائمة.

**Infrastructure** — لا تغيير.

**Presentation**
- `ResultsWorklistView.xaml`:
  - `SelectedItem="{Binding SelectedItem}"` على الـ `DataGrid`.
  - عمود `<DataGridTemplateColumn Header="فتح">` فيه زر:
    ```xml
    <Button Command="{Binding DataContext.OpenDetailCommand,
                        RelativeSource={RelativeSource AncestorType=DataGrid}}"
            CommandParameter="{Binding}"/>
    ```
  - `MouseDoubleClick` على الصف يستدعي نفس الأمر.
  - `RowStyle` بمؤشّر يد عند التمرير.
- `ResultsWorklistViewModel.cs`:
  - خاصية `SelectedItem` من نوع `ResultWorklistItemDto?` ترفع `OnPropertyChanged`.
  - حارس في `OpenDetailAsync`: إن كان المعامل `null` ⇒ `StatusMessage = "اختر مريضاً من القائمة أولاً."` بلا رمي استثناء.
  - تأكيد `ResultKind`Routing مقابل القيم الثلاث قبل الملاحة.
- `PatientsHubView.xaml` — زرّان جديدان beneath الصف الموجود:
  - «تقرير فارغ» → `BlankReportViewModel`
  - «تقارير التاريخ» → `HistoryReportsViewModel`
- `PatientsHubViewModel.cs` — أمران `AsyncRelayCommand` بنفس نمط `PatientsHubViewModel.cs:24,35,46,57` (المLocations الموجودة).
- `BlankReportViewModel.cs:202` — استبدال `Task.CompletedTask` بتنفيذ: جلب بيانات المريض عبر `BuildBlankReportCommand`، ملء النموذج، تفعيل الزر.

**Tests**
```
OrphanedViewModel_BlankReport_HasNavigationCaller
OrphanedViewModel_HistoryReports_HasNavigationCaller
OrphanedViewModel_ResultEntryScreens_ReachableFromWorklist
ResultsWorklist_OpenDetail_RoutesByResultKind
ResultsWorklist_OpenDetail_NullParameter_ShowsMessageNotCrash
ResultsWorklist_RowCommand_ResolvesThroughDataContext
PatientsHub_ShowsFourPlusTwoEntryButtons
BlankReport_LoadsPatientData
```

### خطوات التنفيذ

1. أضف `SelectedItem` إلى `ResultsWorklistViewModel` مع الإشعار الصحيح.
2. أضف عمود الزر + `RelativeSource AncestorType=DataGrid` — **هذا هو الربط المفقود أصلاً**.
3. أضف `MouseDoubleClick` على الصف (يُنفَّذ داخلياً في `ResultsWorklistView.xaml.cs` بنداء واحد إلى `OpenDetailCommand`، بلا منطق أعمال في الـ code-behind).
4. أضف الحارس والحالات الثلاث في `OpenDetailAsync`.
5. أضف زرَّي الـ Hub وأمرَيهما.
6. نفّذ `BlankReportViewModel` الناقص.
7. **أضف اختبار البنية** — هذا هو الفحص الذي كان سيلتقط العيب فوراً:
   ```csharp
   [Fact]
   public void OrphanedViewModel_EachRegisteredEntryScreen_HasNavigationCaller()
   {
       // لكل ViewModel مُسجَّل في DependencyInjection ضمن نطاق العمل:
       //   assert SourceContains($"NavigateTo<{vm.Name}>")
       //   أو NavigationRequested event
   }
   ```
   قائمة الاستثناءات الوحيدة المسموحة: `ShellViewModel` (جذر) و`LoginViewModel` (جذر).
8. شغّل `grep -rn "NavigateTo<" src/TopLab.Presentation | sort` واحفظ المخرجات كخط أساس.

### الاعتماديات
لا شيء.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تجربة مستخدم | موضع الزرَّين الجديدين في الـ Hub | اعرض مخططاً على المالك قبل الدمج |
| تقنية | زر داخل صف `DataGrid` يحتاج `RelativeSource` صحيح | استخدم `AncestorType=DataGrid` لا `Self` |
| — | اختبار البنية قد يُنتج إيجابيات كاذبة على VMs الجذر | قائمة استثناءات صريحة |

### معايير القبول
- [ ] فتح أي مريض من قائمة العمل ينقلك للشاشة الصحيحة
- [ ] الشاشات الثلاث تُفتح وتُغلق بلا تسريب أو استثناء
- [ ] «تقرير فارغ» يُنتج تقريراً فعلياً
- [ ] `OrphanedViewModel_*` الثلاثة خضراء
- [ ] `grep NavigateTo<` يُظهر مدخلاً لكل ViewModel مسجّل غير جذر

### التراجع
كل التغييرات في XAML + VM. `git revert` كامل للحزمة يعيد bunny. **لا** تدمج نصف الحزمة: الربط بلا الاختبار البنيوي يعيد العيب صامتاً.

---

### WP-03 — تصحيح ترميز فئة الحساسية + ترميم البيانات المحفوظة

| | |
|---|---|
| **الأولوية** | **P0** |
| **سبب الأولوية** | يفسد بيانات سريرية مخزّنة: كل حساسية تُحفظ درجة واحدة خاطئة، و`Resistant` غير قابل للوصول أصلاً. |
| **المرجع** | R-23 · D-10 · تقرير الوكيل الفرعي الأول Cap 7 |
| **الحجم** | S (1–2 يوم) |
| **الهجرات** | **migration جديدة** — `FixCultureSensitivityCategoryOffByOne` |
| **الاعتماديات** | لا شيء. **مطلوب قبل:** WP-14. |

### الحالة الحالية — PROVED DEFECT + بيانات قائمة تالفة

```
SensitivityCategory.cs:3-7
  HighlyFor = 0 · ModerateFor = 1 · LowFor = 2 · ResistantFor = 3

CultureEntryView.xaml:111
  <ComboBox SelectedIndex="{Binding SensitivityCategory, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
      <ComboBoxItem Content="غير محدد"/>        ← الفهرس 0
      <ComboBoxItem Content="حساسية عالية"/>     ← الفهرس 1
      <ComboBoxItem Content="حساسية متوسطة"/>    ← الفهرس 2
      <ComboBoxItem Content="حساسية منخفضة"/>    ← الفهرس 3
```

| المظهر | القيمة المخزّنة | النتيجة الفعلية |
|---|---|---|
| «غير محدد» | `0` | `HighlyFor` |
| «حساسية عالية» | `1` | `ModerateFor` |
| «حساسية متوسطة» | `2` | `LowFor` |
| «حساسية منخفضة» | `3` | `ResistantFor` |
| `ResistantFor` (3) | **غير قابل للوصول** | لا يوجد عنصر يعرضه |

`SaveCultureResultsCommandValidator.cs:24` يقبل `0..3` فلا يلتقط شيئاً.

### الحالة المستهدفة
ما يراه المستخدم هو ما يُحفظ تماماً؛ «غير محدد» يُحفظ `null`؛ `Resistant` قابل للاختيار؛ والصفوف الموجودة تُراجَع وفق قاعدة معلنة.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml
  src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
  src/TopLab.Application/Features/CultureResults/Commands/SaveCultureResults/SaveCultureResultsCommandValidator.cs
  src/TopLab.Infrastructure/Persistence/Configurations/CultureAntibioticResultConfiguration.cs

يُضاف:
  src/TopLab.Domain/Results/SensitivityOption.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_FixCultureSensitivityCategoryOffByOne.cs
  tests/TopLab.Application.Tests/Features/CultureResults/SaveCultureResultsMappingTests.cs
  tests/TopLab.Persistence.Tests/Migrations/SensitivityCategoryRepairTests.cs
```

### المكونات

**Domain**
- `SensitivityOption(SensitivityCategory? Value, string Label)` — غلاف nullable لربط WPF.
- **لا تغيير** في `SensitivityCategory`.

**Application**
- استبدال الربط بالفهرس بربط بالقيمة:
  ```xml
  <ComboBox ItemsSource="{Binding SensitivityOptions}"
            DisplayMemberPath="Label"
            SelectedValuePath="Value"
            SelectedValue="{Binding SensitivityCategory, Mode=TwoWay}"/>
  ```
- `CultureEntryViewModel.SensitivityOptions` = قائم ثابت:
  ```csharp
  new(null, "غير محدد")   // القيمة nullable ⇒ تُحفظ null لا 0
  new(SensitivityCategory.HighlyFor,     "حساسية عالية")
  new(SensitivityCategory.ModerateFor,   "حساسية متوسطة")
  new(SensitivityCategory.LowFor,        "حساسية منخفضة")
  new(SensitivityCategory.ResistantFor,  "مقاومة")
  // + HighlyFor "حساسية عالية" · ModerateFor "حساسية متوسطة"
  // + LowFor "حساسية منخفضة" · ResistantFor "مقاومة"
  ```
- تثبيت التحقق في المدقّق: `RuleFor(x => x.SensitivityCategory).InclusiveBetween(0, 3).When(x => x.SensitivityCategory.HasValue)`.

**Infrastructure**
- هجرة **قراءة فقط أولاً**: `UP` مقسّم على `CASE` + جدول تقرير، **بلا** `UPDATE`.
- ثم هجرة `UPDATE` بالقاعدة المعتمدة في **القرار 1**:
  | القيمة المخزّنة | القيمة الصحيحة | السبب |
  |---|---|---|
  | `0` | `NULL` | الفهرس 0 = «غير محدد» ⇒ لا قيمة |
  | `1` | `0` | كان `HighlyFor` |
  | `2` | `1` | كان `ModerateFor` |
  | `3` | `2` | كان `LowFor` |
- توثيق قبل/بعد في `Migration` XML doc.

**Presentation**
- `CultureEntryView.xaml:108-117` إعادة بناء الـ `ComboBox`.
- إدراج `OrganismC` في لوحة المعاينة الداخلية (`CultureEntryView.xaml:140-144` يتجاهلها رغم قابلية تحريرها في `:49`).

**Tests**
```
SaveCulture_High_Persists_Highly
SaveCulture_Moderate_Persists_Moderate
SaveCulture_Low_Persists_Low
SaveCulture_Resistant_Persists_Resistant
SaveCulture_Unspecified_Persists_Null
SaveCulture_OutOfRangeValue_IsRejected
SensitivityOptions_ListContainsExactlyFiveEntries
SensitivityRepair_MapsStoredOne_ToZero
SensitivityRepair_MapsStoredTwo_ToOne
SensitivityRepair_MapsStoredThree_ToTwo
SensitivityRepair_MapsStoredZero_ToNull
```

### خطوات التنفيذ

1. **تقرير قراءة فقط.** نفّذ `SELECT SensitivityCategory, COUNT(*) … GROUP BY` واكتب النتيجة في ملف. **لا تكتب شيئاً بعد.**
> **القاعدة المعتمدة (تُطبَّق حرفياً — لا نسخة ثانية في هذا الملف):**
> | المخزَّن | يُستبدل بـ |
> |---|---|
> | `0` | `NULL` |
> | `1` | `0` |
> | `2` | `1` |
> | `3` | `2` |
>
> `ResistantFor` (3) كان غير قابل للوصول في الواجهة أصلاً، ولذلك كل صف بـ `3` يعني «منخفضة» ⇒ `2`.

2. اعرض التقرير على المالك (القرار 1).
3. أضف `SensitivityOption` و`SensitivityOptions` في الـ VM.
4. استبدل `SelectedIndex` بـ `SelectedValue`.
5. ثبّت التحقق في المدقّق.
6. أنشئ هجرة الإصلاح **كتابة فقط** بالقاعدة المعتمدة.
7. أضف تقرير التحقق بعد الكتابة، وأرفقه في وصف الـ PR.
8. أضف `OrganismC` إلى لوحة المعاينة.
9. شغّل الاختبارات الستّة.

### الاعتماديات
لا شيء. **مطلوب قبل WP-14** لأن WP-14 يبني على الفئات الصحيحة.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | القيمة `0` ملتبسة: قد تكون اختيار المستخدم «غير محدد» أو نتيجة الإزاحة نفسها | **القاعدة نفسها في متن الحزمة** (`0→NULL` …). البوابة: تقرير قراءة-فقط + موافقة المالك (القرار 1) |
| بيانات | بعض الصفوف قد تكون لامراض أُعيدت قراءتها يدوياً فصارت سليمة | قِس الأعداد لكل قيمة قبل وبعد |
| تقنية | `ResistantFor` كان غير قابل للوصول ⇒ صفوف بـ `3` تعني «منخفضة»، وقد استُخدمت الواجهة قبل تصحيحها | معالَج في قاعدة الترميم |
| تجربة مستند | «مقاومة» بدل «حساسية منخفضة» تحتاج تأكيد التسمية العربية | اسأل المالك عن التسمية |

### معايير القبول
- [ ] تقرير القراءة-فقط مُراجَع من المالك قبل أي كتابة
- [ ] هجرة الإصلاح مُطبَّقة **بعد** موافقة المالك
- [ ] الاختبارات الخمسة الأولى خضراء
- [ ] `SensitivityOptions` تحوي 5 مدخلات بالضبط
- [ ] لا صف واحد بقي خارج [0..3] بعد الترميم

### التراجع
الهجرة **لا يمكن التراجع عنها** بمجرد `Down`. احتفظ بجدول نسخ احتياطي في نفس الهجرة:
```sql
IF OBJECT_ID('CultureSensitivityBackup') IS NULL
    CREATE TABLE CultureSensitivityBackup AS SELECT * FROM CultureAntibioticResults;
```
هذا يجعل التراجع ممكناً يدوياً دائماً. **لا تحذف الجدول.**

---

### WP-04 — محرر جهة الإحالة/التعاقد (قائمة أسعار + عمولة + بريد)

| | |
|---|---|
| **الأولوية** | **P0** |
| **سبب الأولوية** | نوع كيان إلزامي في النموذج لا يمكن إنشاؤه من الواجهة. هذا يحجب حسابات إحالة المرضى كلّها وحساب «نسبة المختبر» بشكل دائم. |
| **المرجع** | R-55, R-78 · D-03, D-33 |
| **الحجم** | M (3–4 أيام) |
| **الهجرات** | **migration جديدة** — `AddExternalEntityEmail` |
| **الاعتماديات** | لا شيء. **مطلوب قبل:** WP-09. **يُنسَّق مع** WP-24 (نفس الجدول). |

### الحالة الحالية — PROVED DEFECT

```
ExternalEntity.cs:159-170   ValidatePriceListRule
  :168  throw new ArgumentException("ReferralOrContract requires PriceListId.", ...)

ExternalEntityEditorViewModel.cs:157-161   (مسار الإنشاء)
  new CreateExternalEntityCommand(
      EntityType, Name.Trim(), BlankToNull(City), BlankToNull(Address),
      BlankToNull(Phone), BlankToNull(Fax),
      BlankToNull(ResponsiblePersonName), BlankToNull(ResponsiblePersonPhone),
      null, null);        ← PriceListId = null, DiscountOrCommissionPercent = null

ExternalEntityEditorWindow.xaml   ← لا ComboBox ولا NumericUpBox لقائمة الأسعار
GetCashDrawerInventoryQueryHandler.cs:143   يشترط is > 0 لنسبة العمولة
```

### التتابع
`CreateExternalEntityCommandValidator.cs:41-47` يشترطه أيضاً ⇒ حتى لو أُصلح الـ VM، لا بد من إصلاح المدقّق أو تمرير قيمة صحيحة. **المطلوب هو تمرير القيمة** لا إزالة القاعدة.

### الحالة المستهدفة
يُنشأ طلب إحالة من النافذة باختيار قائمة أسعاره ونسبة عمولته وبريده، ويُحفظ؛ ويظهر أثره فوراً في تسعير المريض وفي حساب «نسبة المختبر» في جرد الدرج.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/ExternalEntities/ExternalEntity.cs          ← + Email
  src/TopLab.Presentation/Views/External/ExternalEntityEditorWindow.xaml
  src/TopLab.Presentation/ViewModels/External/ExternalEntityEditorViewModel.cs
  src/TopLab.Application/Features/ExternalEntities/Commands/CreateExternalEntity/CreateExternalEntityCommand.cs
  src/TopLab.Application/Features/ExternalEntities/Commands/UpdateExternalEntity/UpdateExternalEntityCommand.cs
  src/TopLab.Infrastructure/Persistence/Configurations/ExternalEntityConfiguration.cs

يُضاف:
  src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Queries/GetPriceListsForPick/
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddExternalEntityEmail.cs
  tests/TopLab.Presentation.Tests/ExternalEntities/ExternalEntityEditorViewModelTests.cs
```

### المكونات

**Domain**
- `ExternalEntity.Email` (string?, ≤ 200) — الغائب عن `ExternalEntity.cs:12-32` وDTO والأمر.
- **لا** تلمس `ValidatePriceListRule` — القاعدة صحيحة.

**Application**
- `GetPriceListsForPickQuery` → `IReadOnlyList<PriceListOptionDto>(PriceListId Id, string Name, int ItemCount)` بمُصنَّفة + `IAuthorizedRequest(Permissions.PriceLists)` + ترتيب أبجدي.
- `Email` في `Create/UpdateExternalEntityCommand` + مدقّق `EmailAddress().When(!string.IsNullOrWhiteSpace(x))`.
- `DiscountOrCommissionPercent` مدقّق: `InclusiveBetween(0, 100)`.

**Infrastructure** — هجرة `AddExternalEntityEmail` بعمود `nvarchar(200) null`.

**Presentation**
```
ExternalEntityEditorWindow.xaml — لوحة "قائمة الأسعار والعمولة" (أظهرها فقط عند
EntityType == ReferralOrContract):
  <TextBlock Text="قائمة الأسعار *" />
  <ComboBox ItemsSource="{Binding PriceListOptions}"
            DisplayMemberPath="Display"
            SelectedValuePath="Id"
            SelectedValue="{Binding PriceListId}"/>
  <TextBlock Text="نسبة العمولة %" />
  <TextBox Text="{Binding DiscountOrCommissionPercent, StringFormat='0.##'}"/>
  <TextBlock Text="البريد الإلكتروني" />
  <TextBox Text="{Binding Email}"/>
```
- `ExternalEntityEditorViewModel`:
  - `LoadPriceListsAsync()` عند `Initialize` و`InitializeEdit`.
  - الخصائص `_priceListId` و`_discountOrCommissionPercent` **موجودة أصلاً** (`:35, :99, :134-135`) — فقط أضِف التحميل والمُعرِّفات في XAML.
  - `IsPriceListRequired => EntityType == EntityType.ReferralOrContract`.
  - `CanSave` يمنع الحفظ بلا `PriceListId` عند `IsPriceListRequired` — **تحقّق مبكر بدل انتظار خطأ الخادم**.

**Tests**
```
CreateReferralEntity_ViaViewModel_Succeeds          ← الاختبار الحاسم
CreateReferralEntity_WithoutPriceList_FailsEarly    ← CanSave = false
CreateTreatingDoctor_WithPriceList_IsRejected      ← القاعدة سليمة
UpdateReferralEntity_RoundTripsPriceListAndCommission
UpdateReferralEntity_PersistsEmail
GetPriceListsForPick_ReturnsAllSorted
GetPriceListsForPick_RequiresPermission
CommissionFlow_ReferralEntity_Percent_FlowsToDrawer
```

### خطوات التنفيذ

1. أضف `Email` للنموذج + الأمرين + المدقّق + DTO + التهيئة + **الهجرة**.
2. أنشئ `GetPriceListsForPickQuery` بنفس نسيج استعلامات `PriceLists` القائمة.
3. أضف `LoadPriceListsAsync` إلى الـ VM عند التهيئة والتعديل.
4. أضف اللوحة الثلاثية إلى النافذة مع `Visibility` مرتبطة بـ `IsPriceListRequired`.
5. **غيّر `:157-161`** — مرّر `_priceListId` و`_discountOrCommissionPercent` و`Email` بدل `null, null`.
6. أضف `CanSave` و`IsPriceListRequired`.
7. اكتب اختبار الانحدار **عبر الـ ViewModel** — هذا هو الفحص الذي كان سيلتقط العيب أصلاً. لا تكتفِ باختبار الأمر.
8. تحقّق: أنشئ جهة إحالة بنسبة 10%، سجّل مريضاً بإحالة إليها، ثم افتح جرد الدرج وتأكد أن جدول العمولات يعرض رقماً ≠ 0.

### الاعتماديات
لا شيء.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| بيانات | تعديل مخطط على جدول جهات خارجية | هجرة صغيرة، بلا فهرس |
| تجربة مستخدم | حقول إلزامية على نموذج كان يرفض الحفظ أصلاً | لا يكسر أحداً — لم يكن قابلاً للحفظ من قبل |
| بنيوي |Coordinator مع WP-24 على نفس الجدول | **نفّذ WP-04 أولاً** ثم أضف `EnglishName` في WP-24 بهجرة منفصلة، أو ادمجهما في هجرة واحدة موقّعة |

### معايير القبول
- [ ] إنشاء جهة إحالة من الواجهة ينجح ويظهر في `ExternalEntitiesView`
- [ ]_validator_ يمرّ دون تعديل — القاعدة سليمة
- [ ] نسبة العمولة تظهر في جرد الدرج (تحقّق يدوي)
- [ ] `CreateReferralEntity_ViaViewModel_Succeeds` أخضر
- [ ] البريد يُحفظ ويُقرأ

### التراجع
حقل nullable + combo box. `git revert` كامل يعيد الواجهة كما كانت (والمعطّل يبقى). آمن.

---

### WP-05 — حارس ملكية التقرير المركّب

| | |
|---|---|
| **الأولوية** | **P0** |
| **سبب الأولوية** | تسرّب سريري بين المرضى. تقرير لمريض واحد يمكن أن يحمل اسم مريض آخر ونتائجه ومرجعه. |
| **المرجع** | R-18 · D-04 في سجل الوكيل الفرعي الأول (بند «تسريب بين المرضى») |
| **الحجم** | S (نصف يوم) |
| **الهجرات** | **none** |
| **الاعتماديات** | لا شيء. |

### الحالة الحالية — PROVED DEFECT (reachability: crafted request)

```
BuildCombinedReportCommandHandler.cs:34-38
  var distinctIds = request.OrderedPatientTestIds.Distinct().ToList();
  var pts = _db.Set<PatientTest>()
      .Where(pt => distinctIds.Contains(pt.Id.Value))     ← لا يوجد pt.PatientId == request.PatientId
      .ToList();

BuildCombinedReportCommandValidator.cs   ← قاعدتان فقط:
  RuleFor(x => x.PatientId).GreaterThan(0);
  RuleFor(x => x.OrderedPatientTestIds).NotEmpty();
```

`GetCombinableTestsQueryHandler.cs:197` يقيّد المنتقي بشكل صحيح ⇒ **الثغرة تتطلب طلباً مصنوعاً**، لا واجهة. لكن غياب الحارس على الخادم ثغرة أمنية حتى لو لم تكنثغرة أمنية حتى لو لم تكن قابلة للوصول من الشاشات. من الشاشات.

### الحالة المستهدفة
أي `PatientTestId` لا يخص المريض المطلوب يُرفض برسالة عربية واضحة، ولا يُبنى أي تقرير، ولا يُسرَب أي حقل هوية.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Application/Features/ReportProduction/Commands/BuildCombinedReport/BuildCombinedReportCommandHandler.cs
  src/TopLab.Application/Features/ReportProduction/Commands/BuildCombinedReport/BuildCombinedReportCommandValidator.cs
  src/TopLab.Application/Features/ReportProduction/Common/DomainFailureTranslator.cs
  src/TopLab.Application/Features/ReportProduction/Commands/PrintCombinedReport/PrintCombinedReportCommandHandler.cs
```

### المكونات

**Domain** — لا تغيير.

**Application**
```csharp
var distinctIds = request.OrderedPatientTestIds.Distinct().ToList();
var pts = await _db.Set<PatientTest>()
    .Where(pt => distinctIds.Contains(pt.Id.Value)
              && pt.PatientId == request.PatientId)          // ← الحارس
    .Select(pt => new { pt.Id, pt.PatientId, pt.IsDeleted })
    .ToListAsync(ct);

if (pts.Any(p => p.IsDeleted))
    return Error.Conflict("أحد التحاليل محذوف ولا يمكن تضمينه في التقرير.");

if (pts.Count != distinctIds.Count)
    return Error.Forbidden("أحد التحاليل المحددة لا يخص هذا المريض.");
```
- مُترجَم جديد في `DomainFailureTranslator` لكل رسالة جديدة.
- المدقّق: `RuleFor(x => x.OrderedPatientTestIds).Must(ids => ids.Distinct().Count() == ids.Count)`
  بـ `"لا يمكن تكرار التحاليل في التقرير الواحد."`
- **افحص** `PrintCombinedReportCommandHandler` — إن كان يبني مسار DTO منفصلاً، طبّق نفس الحارس. التوازي بين البناء والطباعة يجب أن يكون متطابقاً.

**Infrastructure** — لا تغيير.

**Presentation** — لا تغيير. (الرسالة الجديدة تظهر في `ErrorMessage` الموجود.)

**Tests**
```
BuildCombinedReport_RejectsForeignPatientTestId
BuildCombinedReport_RejectsDeletedPatientTest
BuildCombinedReport_RejectsDuplicateIds
BuildCombinedReport_AcceptsOwnReviewedTests
BuildCombinedReport_ErrorMessage_DoesNotLeakOtherPatientName
PrintCombinedReport_RejectsForeignPatientTestId
```

### خطوات التنفيذ

1. أضف `pt.PatientId == request.PatientId` إلى الاستعلام في `BuildCombinedReportCommandHandler.cs:34-38`.
2. **ميّز** بين ثلاث حالات برسائل عربية مختلفة: «غير موجود» · «محذوف» · «لا يخص هذا المريض». الرسالة الثالثة **لا** تذكر اسم مريض آخر إطلاقاً.
3. أضف فحص التفرّد في المدقّق.
4. **افحص** `PrintCombinedReportCommandHandler` وطبّق نفس الحارس إن لزم.
5. أضف `IsDeleted` للاستعلام.
6. اكتب الاختبارات الستّة. الاختبار الحاسم هو `DoesNotLeakOtherPatientName`: ابنِ التقرير بمعرّف اختبار مريض آخر وافحص **كل** حقول `Result`returned للتأكد من عدم وجود أي اسم.

### الاعتماديات
لا شيء.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تقنية | طلبات قائمة قد تعتمد على ids مكرّرة كانت تُقبل | سلوك مقصود التصحيح —-note في الـ PR |
| — | لا شيء آخر | — |

### معايير القبول
- [ ] الاختبارات الستّة خضراء
- [ ] بناء بمعرّف مريض آخر ⇒ `Error.Forbidden` برسالة لا تذكر أي هوية
- [ ] `grep -rn "OrderedPatientTestIds" src/` — كل مسار يمرّ بالحارس

### التراجع
سطر واحد في الاستعلام. آمن تماماً.

---

### WP-06 — منسّق طباعة صادق: مستند حقيقي أو خطأ صريح

| | |
|---|---|
| **الأولوية** | **P0** |
| **سبب الأولوية** | أزرار «طباعة» تكتب `PrintCount++` بلا ورقة. هذا يفسد سجل التدقيق الذي وُجد ليجيب على سؤال «من طبع ومتى». |
| **المرجع** | R-16, R-17, R-28, R-42 · D-08 · D2 في تقرير الوكيل الفرعي الأول |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **none** |
| **الاعتماديات** | **WP-01** (يحتاج كاتباً ينتج عربياً). **مطلوب قبل:** WP-14, WP-29. |

### الحالة الحالية — PROVED DEFECT

| الشاشة | الدليل | السلوك |
|---|---|---|
| إدخال الصنف | `ProfileEntryViewModel.cs:331-358` | `MarkProfilePrintedCommand` فقط ثم `StatusMessage = "تم الطباعة."` |
| إدخال المزرعة | `CultureEntryViewModel.cs:321,333,336` | `MarkCultureReportPrintedCommand` فقط ثم `"تمت الطباعة."` |
| الطباعة المجمّعة | `ExecuteBulkPrintCommandHandler.cs:15-17` | اعتماداته `_db, _currentUser, _clock` فقط — لا خدمة طباعة — ثم `pt.MarkPrinted()` |

`MarkProfilePrintedCommandHandler` لا يحقن أي خدمة طباعة. **7 معالجات فقط** في كامل طبقة
طبقة Application تحقن منفذ طباعة: `PrintInvoiceCommandHandler` · `PrintReceiptCommandHandler` ·
`PrintBarcodeCommandHandler` · `PrintBlankReportCommandHandler` · `PrintCombinedReportCommandHandler` ·
`PrintHistoryReportCommandHandler` · `PrintWorkSheetCommandHandler`.

### الحالة المستهدفة
أي مسار طباعة يمرّ عبر منسّق واحد: يبني التقرير → ينشئ PDF → يسلّمه للطابعة → **وعند النجاح فقط** يُرسل أمر الوسم. عند الفشل: رسالة خطأ عربية + سطر تشخيص في السجل + **صفر** صف مطبوع.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs
  src/TopLab.Application/Features/ResultsEntry/Commands/BulkPrint/ExecuteBulkPrintCommandHandler.cs
  src/TopLab.Infrastructure/Printing/ReportPrintingService.cs
  src/TopLab.Infrastructure/Barcode/BarcodeService.cs
  src/TopLab.Presentation/DependencyInjection.cs

يُضاف:
  src/TopLab.Application/Features/ResultsEntry/Common/ResultPrintCoordinator.cs
  src/TopLab.Application/Features/ResultsEntry/Common/IResultPrintCoordinator.cs
  src/TopLab.Infrastructure/Logging/PrintingDiagnostics.cs
  tests/TopLab.Application.Tests/Features/ResultsEntry/ResultPrintCoordinatorTests.cs
  tests/TopLab.Application.Tests/Features/ResultsEntry/BulkPrintHonestyTests.cs
```

### المكونات

**Domain** — لا تغيير.

**Application**
```csharp
public interface IResultPrintCoordinator
{
    Task<Result<ResultPrintOutcome>> PrintAsync(
        int patientTestId, ResultPrintKind kind, CancellationToken ct = default);
}

public sealed record ResultPrintOutcome(
    int PatientTestId, string PdfPath, DateTime PrintedAtUtc, int PrintCount);
```
- ترتيب الخطوات صارم: **بناء → طباعة → وسم**.
- عند نجاح البناء وفشل الطابعة: **لا وسم**، ويُعاد نفس نص الخطأ العربي الذي يعيده `ReportPrintingService` (حتى لا يتغيّر سلوك الواجهة).
- `ExecuteBulkPrintCommandHandler` يستدعي المنسّق لكل مريض ويعيد:
  `BulkPrintOutcomes.Printed` فقط عند النجاح، وإلا `BulkPrintOutcomes.Failed` مع سبب.

**Infrastructure**
- تسجيل الاستثناءات المبلَعة عبر `IAppLogger` — وهو **موصول بالفعل** في `Infrastructure/DependencyInjection.cs:102`
  (`services.AddSingleton<IAppLogger, Logging.FileAppLogger>()`). النقص أن خدمات الطباعة **لا تحقنه**.
  أضِف `IAppLogger` إلى:
  - `ReportPrintingService.cs:73` (الملف 77 سطراً — سطر الالتقاط الحقيقي)
  - `BarcodeService.cs:80-84`
  - `ExportPatientReportPdfCommandHandler.cs:161-165`
- استثناء `OperationCanceledException` يجب أن يُعاد رميه ولا يُسجَّل كخطأ (وهو سلوك صحيح حالياً في `BarcodeService.cs:76-79` — حافظ عليه).

**Presentation**
- النصوص:
  - نجاح: `«تمت الطباعة (ملف: <اسم الملف>).»`
  - فشل: `«تعذّرت الطباعة: <السبب>»` — **لا** نص نجاح.
- `IsBusy` يُدار في كل المسارات (بعضها الحالي يُنهي `finally` صحيحاً — حافظ).

**Tests**
```
ResultPrintCoordinator_Success_MarksPrinted
ResultPrintCoordinator_PrintFails_DoesNotMarkPrinted    ← الاختبار الحاسم
ResultPrintCoordinator_BuildFails_DoesNotCallPrinting
ResultPrintCoordinator_ReturnsArabicErrorMessage
BulkPrint_PrintFails_ReturnsFailedOutcome
BulkPrint_MixedBatch_ReportsPerPatientOutcome
PrintingService_CaughtException_IsLogged
NoViewModel_HardcodesPrintedMessage                    ← فحص بنيوي
```
اختبار `DoesNotCallPrinting` يحتاج بديلاً hand-rolled لـ `IReportPrintingService` يُرجع `Result.Failure` — **لا** تستخدم مكتبة mocking.

### خطوات التنفيذ

1. أنشئ `IResultPrintCoordinator` و`ResultPrintCoordinator` في `Features/ResultsEntry/Common`.
2. احقن `IReportPrintingService` + `IMediator` + `ICurrentUserService` + `ILogger`.
3. اكتب `PrintAsync` بترتيب البناء ثم الطباعة ثم الوسم.
4. **اختبر أولاً**: `DoesNotMarkPrinted` أخضر قبل استبدال أي شاشة.
5. استبدل `PrintAsync` في `ProfileEntryViewModel.cs:331-358` بالمنسّق.
6. استبدل `PrintAsync` في `CultureEntryViewModel.cs:321-348` بالمنسّق. (استخدم نفس المنسّق بلا تكرار —WP-14 سيضيف التفاصيل لاحقاً.)
7. أصلح `ExecuteBulkPrintCommandHandler` ليحقن المنسّق.
8. سجّل الاستثناءات المبلَعة في الثلاث مواضع.
9. سجّل المنسّق في `DependencyInjection`.
10. أضف الفحص البنيوي `NoViewModel_HardcodesPrintedMessage`:
    ```csharp
    // امسح كل ViewModels عن "تم الطباعة" / "تمت الطباعة"
    // واشترط أن تكون ناتجة عن Successful result فقط
    ```
11. تحقّق يدوياً: أوقف الطابعة ⇒ اضغط طباعة ⇒ يجب أن ترى خطأً وأن يبقى `IsPrinted = false`.

### الاعتماديات
**WP-01**. (لا يمكن إرسال مستند حقيقي قبل وجود كاتب يُنتج عربياً.)

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | سجلات طباعة وهمية قائمة لا يمكن إصلاحها | **القرار 2** — لا تمسّ بيانات الإنتاج دون استشارة فني قواعد البيانات |
| تجربة مستخدم | رسائل فشل بدل نجاح كاذب | هذا هو المطلوب — لكن أبلغ المستخدم في التدريب |
| تقنية | `MarkCultureReportPrinted` و`MarkProfilePrinted`أمران منفصلان | المنسّق يقرّر بالـ `ResultPrintKind` |

### معايير القبول
- [ ] اختبار فشل الطابعة ⇒ `IsPrinted` يبقى `false` و`PrintCount` ثابتاً
- [ ] `grep -rn "تم الطباعة" src/TopLab.Presentation/` ⇒ **لا نتيجة** خارج مسار نجاح
- [ ] الطباعة المجمّعة تعيد `Failed` لكل مريض فشل
- [ ] السجل يحتوي أثر كل استثناء مبلَع
- [ ] فحص يدوي بإيقاف الطابعة: خطأ ظاهر، وصفر وسم

### التراجع
الـ VMs قابلة للتراجع. **لكن** إن نُفِّذ القرار 2 (تفريغ السجلات) فهو **غير قابل للتراجع** — خذ نسخة احتياطية قبله.

---

### WP-07 — تعليقاه المدى الأدنى/الأعلى حتى المخرج

| | |
|---|---|
| **الأولوية** | **P0** |
| **سبب الأولوية** | البيانات تُخزَّن صحيحاً ثم تُهمَل عند حدّ المخرج. تفقد معلومة سريرية موجودة في قاعدة البيانات ولا تصل الورقة. |
| **المرجع** | R-47 · D-02 |
| **الحجم** | S (نصف يوم) — **مُدمج في WP-01** |
| **الهجرات** | **none** |
| **الاعتماديات** | **WP-01** |

### الحالة الحالية — PROVED DEFECT

```
BuildCombinedReportCommandHandler.cs:160-163
  FormatRange(...) => $"{MinValue} - {MaxValue}"          ← لا تعليقات

ReportDtos.cs:40-50   CombinedReportLineDto   ← لا LowComment ولا HighComment
ReportDtos.cs:19-20   ProfileReportLineDto.FrozenRange  ← يحمل LowComment/HighComment ✓
ReportPdfWriter.cs:81-89   ← لا يرسم profile.FrozenRange أصلاً
```

البيانات **موجودة**: `ReferenceRange` و`PatientTestReferenceRangeSnapshot` يحملان التعليقين. العطل عند حدّ المخرج فقط.

### الحالة المستهدفة
عند نتيجة أقل من الحد الأدنى يظهر تعليق الحد الأدنى، وعند تجاوز الحد الأعلى يظهر تعليق الحد الأعلى، في التقرير المطبوع وفي المعاينة. النتيجة داخل المدى لا تُخرج تعليقاً.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Application/Features/ReportProduction/Common/ReportDtos.cs
  src/TopLab.Application/Features/ReportProduction/Commands/BuildCombinedReport/BuildCombinedReportCommandHandler.cs
  src/TopLab.Application/Features/ReportProduction/Queries/GetPatientTestHistory/GetPatientTestHistoryQueryHandler.cs
  src/TopLab.Application/Features/ReportProduction/Queries/GetSeparateHistoryReport/GetSeparateHistoryReportQueryHandler.cs
  src/TopLab.Infrastructure/Printing/ReportDocument.cs   ← من WP-01، يُوسَّع

يُضاف:
  tests/TopLab.Application.Tests/Features/ReportProduction/RangeCommentPropagationTests.cs
```

### المكونات

**Domain** — لا تغيير.

**Application**
```csharp
// ReportDtos.cs
public sealed record CombinedReportLineDto(
    /* ...الحقول الحالية... */
    string? LowComment,
    string? HighComment);
```
- **مصدر القيم: `PatientTestReferenceRangeSnapshot` المجمّد، لا `ReferenceRange` الحيّ.** هذه ليست تفصيلاً أسلوبياً: التجميد هو ما يحمي ضمانة `R-48` (النتائج القديمة تحتفظ بنطاقها الأصلي).
- قاعدة الشرط:
  ```csharp
  string? low  = flag == ResultFlag.Low  ? frozen.LowComment  : null;
  string? high = flag == ResultFlag.High ? frozen.HighComment : null;
  ```
- نفس الحقلين في `HistoryEntryDto` و`ProfileReportLineDto.FrozenRange` (موجود — فقط ارسمه).

**Infrastructure** — في `ReportDocument`، سطر مستقل بخط أصغر واتجاه RTL تحت صف النتيجة.

**Presentation** — لا تغيير.

**Tests**
```
BuildCombinedReport_LowResult_CarriesLowComment
BuildCombinedReport_HighResult_CarriesHighComment
BuildCombinedReport_NormalResult_CarriesNoComment
BuildCombinedReport_UsesFrozenRangeNotLiveRange       ← حرج
BuildCombinedReport_ProfileLine_CarriesFrozenComments
ReportPdf_RendersLowComment_WhenFlagIsLow
ReportPdf_OmitsComment_WhenFlagIsNormal
GetPatientTestHistory_CarriesRangeComments
```

### خطوات التنفيذ

1. أضف `LowComment`/`HighComment` إلى `CombinedReportLineDto` و`HistoryEntryDto`.
2. املأهما في `BuildCombinedReportCommandHandler` من النطاق المجمّد.
3. **اكتب `UsesFrozenRangeNotLiveRange` أولاً** — يجب أن يفشل قبل الخطوة 2 إن قرأتَ من النطاق الحيّ.
4. ابدأ بناء النطاق المجمّد داخل `CombinedReportSelection` أو في المعالج، أيهما أقرب للسلوك القائم.
5. ارسم التعليق شرطياً في `ReportDocument`.
6. ارسم `ProfileReportLineDto.FrozenRange` (التعليقان) — لا يُرسم اليوم إطلاقاً.
6b. طبّق نفس الشيء على `GetPatientTestHistoryQueryHandler` و`GetSeparateHistoryReportQueryHandler`.
7. احذف `FormatRange` النصّي القديم `:160-163` واستبدله بالشكل المنسّق.

### الاعتماديات
**WP-01** (العرض النهائي). لا شيء يعتمد على WP-07.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **تقنية** | القراءة من النطاق الحيّ تكسر تجميد النتائج | اختبار `UsesFrozenRangeNotLiveRange` إلزامي |
| تجربة مستند | سطر إضافي يغيّر ارتفاع الجدول | سطر بخط أصغر + التفاف |
| بيانات | لا شيء | — |

### معايير القبول
- [ ] الاختبارات الثمانية خضراء
- [ ] نتيجة خارج المدى تُخرج تعليقها في الـ PDF
- [ ] نتيجة داخل المدى لا تُخرج تعليقاً
- [ ] تعديل نطاق الاختبار **بعد** إدخال نتيجة **لا** يغيّر تعليقها (تجميد)

### التراجع
حقول nullable على record. إضافة سطر في `ReportDocument`. آمن.

---

### WP-08 — الوحدات وأنواع الباركود ووحدة الصنف

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | مفهوم كامل غائب يبطله WP-17 وWP-23. لا فائدة من إضافة حقول لشيء غير موجود. |
| **المرجع** | R-46, R-50 · D-17, D-19 |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **migration جديدة** — `AddUnitsAndBarcodeTypes` + seed |
| **الاعتماديات** | لا شيء. **مطلوب قبل:** WP-12, WP-17, WP-23. |

### الحالة الحالية — MISSING FEATURE
لا كيان `Unit` ولا `BarcodeType` ولا DbSet ولا Command ولا View. `BarcodeLabelRenderer.cs:18` `BarcodeFormat.CODE_128` مكتوب صلباً. `LabHubView.xaml:7-41` فيه 11 تبويباً لا واحد منها للوحدات. `ReferenceRange` و`AnalyteReferenceRangeBand` بلا `Unit`. `ProfileResultItem.cs:22` `Unit` نص حر على مسار الصنف فقط.

### الحالة المستهدفة
نوافذ صيانة للوحدات وأنواع الباركود؛ وحدة تُختار في محرر النطاقات المرجعية فتظهر في التقرير ونتيجة الاختبار؛ ونوع الباركود قابل للاختيار بدل الصلب.

### الملفات المتأثرة

```
يُضاف (Domain):
  src/TopLab.Domain/Tests/Unit.cs
  src/TopLab.Domain/Tests/BarcodeType.cs
  src/TopLab.Domain/Tests/BarcodeSymbology.cs          ← غلاف لـ ZXing.BarcodeFormat

يُضاف (Application):
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/{Create,Update,Deactivate}Unit/
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/{GetUnits,GetBarcodeTypes}/
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/{Create,Update,Deactivate}BarcodeType/
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Common/TestCatalogAccessPolicy.cs

يُضاف (Infrastructure):
  src/TopLab.Infrastructure/Persistence/Configurations/UnitConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Configurations/BarcodeTypeConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddUnitsAndBarcodeTypes.cs

يُعدَّل (Presentation):
  src/TopLab.Presentation/Views/Lab/LabHubView.xaml                 ← تبويبان
  src/TopLab.Presentation/ViewModels/Lab/LabHubViewModel.cs
  src/TopLab.Presentation/Views/Lab/TestEditorWindow.xaml            ← عمود الوحدة في النطاقات
  src/TopLab.Presentation/ViewModels/Lab/TestEditorViewModel.cs
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/{Create,Update}ReferenceRange/*.cs
  src/TopLab.Domain/Tests/Analyte.cs                                ← + UnitId
  src/TopLab.Domain/Tests/ReferenceRange.cs                         ← + UnitId

يُضاف (Tests):
  tests/TopLab.Domain.Tests/Tests/UnitTests.cs
  tests/TopLab.Application.Tests/Features/TestCatalogAndReferenceRanges/UnitCrudTests.cs
  tests/TopLab.Presentation.Tests/Settings/SettingsCoverageTests.cs  (يُوسَّع في WP-20)
```

### المكونات

**Domain**
- `Unit(Guid Id, string Name, string Symbol, bool IsActive, int SortOrder)` — قواعد: الاسم non-empty، لا تكرار amongst النشط.
- `BarcodeType(Guid Id, string Name, BarcodeSymbology Symbology, decimal DefaultWidthMm, decimal DefaultHeightMm, bool IsActive)`.
- `BarcodeSymbology` — **غلاف داخلي** يحوّل من/إلى `ZXing.BarcodeFormat`. **لا تخزّن تعداد مكتبة خارجية في المخطط** — خزّن `byte` عبر التحويل.
- `Analyte.UnitId` (Guid?, FK → `Units`) و`ReferenceRange.UnitId` (Guid?, FK).

**Application**
- CRUD كامل لـ `Unit` و`BarcodeType` بنفس نسيج `TestComment` المرجعي (`Domain/Tests/TestComment.cs` + `Create/Update/DeleteTestComment`).
- `GetUnitsQuery(bool activeOnly = true)` و`GetBarcodeTypesQuery(bool activeOnly = true)`.
- **كل Command يحوي `IAuthorizedRequest`** — ملاحظة أمنية مستقلة: `SearchTestCatalogQuery` حالياً هو الاستعلام الوحيد بلا بوابة (يُصلَح في WP-17).
- إضافة `UnitId` إلى `Create/UpdateReferenceRangeCommand` + مدقّق.

**Infrastructure**
- إعدادان في `Configurations/`، فهرس فريد جزئي على `Units.Name` حيث `IsActive`.
- هجرة `AddUnitsAndBarcodeTypes` + **بذر**:
  - `BarcodeTypes`: `Code 128` افتراضي نشط.
  - `Units`: **لا** تبذر وحدات افتراضية (قرار المالك) — تركها تُنشأ من الواجهة. لكن **بذر** وحدة مرجعية واحدة مسمّاة «—» لتكون خيار «بلا وحدة» قابلاً للاختيار.
- ربط `DbSet<Unit>` و`DbSet<BarcodeType>` في `ApplicationDbContext`.

**Presentation**
- تبويبان في `LabHubView`: «الوحدات» و«أنواع الباركود» بنمط تبويب التعليقات القائم.
- عمود `الوحدة` في شبكة النطاقات `TestEditorWindow.xaml:75-133` (ComboBox من `GetUnitsQuery`).
- عمود `الوحدة` في نتيجة الاختبار البسيط — يغذّي WP-12.

**Tests**
```
CreateUnit_Persists
CreateUnit_DuplicateActiveName_IsRejected
DeactivateUnit_BlockedWhenReferencedByAnalyte
GetUnits_ReturnsActiveOnly_ByDefault
CreateBarcodeType_PersistsSymbologyAsByte
GetBarcodeTypes_ReturnsActiveOnly
AssignUnitToAnalyte_AppearsInRangeEditor
AssignUnitToReferenceRange_AppearsInReport
UnitCrud_RequiresPermission
```

### خطوات التنفيذ

1. أنشئ `Unit` و`BarcodeType` و`BarcodeSymbology` مع قواعد المجال.
2. أنشئ الإعدادين + DbSets.
3. أنشئ الـ CRUD والاستعلامات + `TestCatalogAccessPolicy`.
4. أنشئ الهجرة + البذر.
5. أضف `UnitId` إلى `Analyte` و`ReferenceRange` — **في نفس الهجرة** لتجنّب هجرتين متقاربتين.
6. وسّع `Create/UpdateReferenceRangeCommand`.
7. أضف عمود الوحدة في `TestEditorWindow.xaml:75-133`.
8. أنشئ التبويبين في `LabHubView`.
9. أضف عمود الوحدة في نتيجة الاختبار البسيط (WP-12 يتكفّل بالربط).
10. اكتب الاختبارات العشرة.

### الاعتماديات
لا شيء.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| بيانات | هجرة على `Analyte` و`ReferenceRange` (جداول متوسطة) | `UnitId` nullable ⇒ لا كسر |
| تقنية | تخزين `ZXing.BarcodeFormat` في المخطط | الغلاف `BarcodeSymbology` + `byte` |
| تجربة مستخدم | خيار «بلا وحدة» | بذر وحدة فارغة واحدة |

### معايير القبول
- [ ] الوحدات تظهر في محرر النطاقات وتُطبع مع النتيجة
- [ ] أنواع الباركود قابلة للإدارة من المعمل
- [ ] كل Commands محميّة بصلاحية (اختبار)
- [ ] لا كسر على `Analyte`/`ReferenceRange` القائمة

### التراجع
عمودان nullable + جدولان جديدان. الجدولان الجديدان يُتركان فارغين بعد التراجع (لا بيانات تُفقد). آمن.

---

### WP-09 — مركز الحسابات: 5×4 + عمولات + تنزيل + فترات جاهزة

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | أربع مقاييس محاسبية منفّذة ومُختبَرة بلا أي محدّد واجهة. التبويب يعرض شبكة فارغة دائماً. أعلى قيمة لكل وحدة جهد في المشروع كله. |
| **المرجع** | R-75, R-76, R-77, R-78 · D-04, D-33, D-34 |
| **الحجم** | M (3–4 أيام) |
| **الهجرات** | **none** |
| **الاعتماديات** | **WP-04** (العمولات تحتاج `DiscountOrCommissionPercent` قابلاً للتعبئة) |

### الحالة الحالية — PROVED DEFECT

```
AccountsHubView.xaml:106-137   ← التبويب: عنوان + لوحة أرقام + شبكة Lines فقط
grep "ElementKind|ReportType|ElementId|AccountTypeFilter|CommissionsAndShares"
  على AccountsHubView.xaml  ⇒  لا نتائج (تحقّق مباشر)

AccountsHubViewModel.cs:145-148   يعرّف الخصائص ويمرّرها للاستعلام ✓
GetElementInventoryQueryHandler.cs:206-224   switch حقيقي على 5×4 ✓
  :208   Summary ⇒ Lines فارغة
GetCashDrawerInventoryQueryHandler.cs:125-177   BuildCommissions ✓
InventoryDtos.cs:28   CommissionShareDto معرّف ✓  — ولا يربطه أحد
GetPatientSamplesDetail   موجود ✓ — ولا مشغّل له
AccountsHubView.xaml:26-29   DatePicker حرّان — لا فترات جاهزة
```

### الحالة المستهدفة
المستخدم يختار نوع العنصر (مستخدم / جهة إحالة / طبيب معالج / نوع حساب / عينات مرسلة) ونوع التقرير (ملخّص / تفصيلي / تفصيلي بالأسعار / تفصيلي بالنتائج) والعنصر نفسه؛ يرى الجدول والتقرير الصحيح؛ ويرى جدول عمولات ونسب مع صف إجمالي؛ وضغطة على إجمالي عينات المرضى تفتح تفصيل كل مريض؛ وأزرار فترات جاهزة.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Presentation/Views/Accounts/AccountsHubView.xaml          ← اللوحة الرئيسية
  src/TopLab.Presentation/ViewModels/Accounts/AccountsHubViewModel.cs
  src/TopLab.Application/Features/InventoryAndAccounting/Queries/GetElementInventory/GetElementInventoryQueryHandler.cs
  src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/GetPatientCountStatisticsQueryHandler.cs
  src/TopLab.Application/Features/Statistics/Queries/GetSentOutStatistics/GetSentOutStatisticsQueryHandler.cs

يُضاف:
  src/TopLab.Application/Features/InventoryAndAccounting/Queries/GetElementPicker/
  src/TopLab.Presentation/Views/Accounts/ElementInventorySelectorView.xaml
  src/TopLab.Presentation/ViewModels/Accounts/ElementInventorySelectorViewModel.cs
  src/TopLab.Presentation/Views/Accounts/PatientSamplesDrillDownWindow.xaml
  src/TopLab.Presentation/ViewModels/Accounts/PatientSamplesDrillDownViewModel.cs
  tests/TopLab.Application.Tests/Features/InventoryAndAccounting/ElementInventoryVariantTests.cs
  tests/TopLab.Presentation.Tests/Accounts/AccountsHubBindingTests.cs
```

### المكونات

**Domain** — لا تغيير.

**Application**
- `GetElementPickerQuery(InventoryElementKind kind)` → قائمة عناصر صالحة للنوع:
  | النوع | مصدر القائمة |
  |---|---|
  | `User` | كل المستخدمين النشطين |
  | `ReferralEntity` | جهات `ReferralOrContract` + `PartnerLab` |
  | `TreatingDoctor` | جهات `TreatingDoctor` |
  | `AccountType` | `SystemSettings.DefaultAccountType` + القيم المستخدمة فعلياً |
  | `SentOutSamples` | المختبرات الشريكة بالاسم |
- `GetElementInventoryQueryHandler.cs:208` — `Summary` يُعيد **صفاً تجميعياً واحداً على الأقل** بدل قائمة فارغة:
  ```csharp
  InventoryReportType.Summary => new[]
  {
      new InventoryLineDto(ElementName: "الإجمالي", Count: total.Count,
                            Amount: total.Amount, Notes: "جميع العناصر")
  };
  ```
- استغلال `GetPatientSamplesDetail` القائم للتفصيل.
- **دفع التجميع إلى SQL** — ينتمي رسمياً لـ WP-19، لكن `GetPatientCountStatisticsQueryHandler.cs:39` `ToList()` على كل المرضى يخدم هذا التبويب أيضاً ⇒ نفّذ سطرين هنا واكمل في WP-19.

**Infrastructure** — لا مخطط.

**Presentation**
```
AccountsHubView.xaml — التبويب «جرد العناصر» يُستبدل محتواه بـ:
  <Grid>
    <Grid.RowDefinitions>…</Grid.RowDefinitions>
    <!-- الصف 0: المحدّدات -->
    <StackPanel Orientation="Horizontal">
      <TextBlock Text="نوع العنصر"/>
      <ComboBox ItemsSource="{Binding ElementKinds}" DisplayMemberPath="Display"
                SelectedItem="{Binding ElementKind}"/>
      <TextBlock Text="العنصر"/>
      <ComboBox ItemsSource="{Binding ElementOptions}" DisplayMemberPath="Display"
                SelectedValuePath="Id" SelectedValue="{Binding ElementId}"/>
      <TextBlock Text="نوع الحساب"/>
      <ComboBox ItemsSource="{Binding AccountTypes}" SelectedItem="{Binding AccountTypeFilter}"/>
      <TextBlock Text="نوع التقرير"/>
      <ComboBox ItemsSource="{Binding ReportTypes}" DisplayMemberPath="Display"
                SelectedItem="{Binding ReportType}"/>
    </StackPanel>
    <!-- الصف 1: الفترات الجاهزة (WP-15 يضيف الفرع) -->
    <StackPanel Orientation="Horizontal">
      <Button Content="اليوم"   Command="{Binding ApplyTodayPeriodCommand}"/>
      <Button Content="أسبوع"   Command="{Binding ApplyWeekPeriodCommand}"/>
      <Button Content="شهر"    Command="{Binding ApplyMonthPeriodCommand}"/>
      <Button Content="سنة"    Command="{Binding ApplyYearPeriodCommand}"/>
      <Button Content="مخصص"   Command="{Binding ShowCustomPeriodCommand}"/>
    </StackPanel>
    <!-- الصف 2: لوحة الأرقام، مع صف قابل للنقر -->
    <Border MouseLeftButtonUp="{Binding DrillDownCommand}">
      <TextBlock Text="{Binding ElementStats.TotalSamplesCount, StringFormat='إجمالي عينات المرضى (عدد): {0}'}"/>
    </Border>
    <!-- الصف 3: شبكة العمولات -->
    <DataGrid ItemsSource="{Binding CommissionsAndShares}">
      <DataGridTextColumn Header="الجهة" Binding="{Binding EntityName}"/>
      <DataGridTextColumn Header="النسبة %" Binding="{Binding Percent}"/>
      <DataGridTextColumn Header="أساس الاحتساب" Binding="{Binding ChargeBase}"/>
      <DataGridTextColumn Header="العمولة" Binding="{Binding Commission}"/>
    </DataGrid>
    <!-- الصف 4: شبكة السطور -->
    <DataGrid ItemsSource="{Binding ElementStats.Lines}">…</DataGrid>
  </Grid>
```
- `AccountsHubViewModel` — `ObservableCollection<EnumOption<InventoryElementKind>>` + أمر `LoadElementOptionsAsync` عند تغيّر `ElementKind` + `DrillDownAsync`.
- `PatientSamplesDrillDownWindow` بالأعمدة: `تاريخ الطلب · كود المريض · اسم المريض · الإجمالي · قيمة الخصم · بعد الخصم · المدفوع · باقِ الحساب`.

**Tests**
```
GetElementInventory_Summary_ReturnsAggregateRow
GetElementInventory_ForEachOfFourReportTypes_ReturnsDistinctShape    [Theory ×4]
GetElementInventory_ForEachOfFiveElementKinds_ReturnsData            [Theory ×5]
GetElementInventory_UnknownElementId_ReturnsEmptyNotError
GetElementPicker_ForEachKind_ReturnsAppropriateList                   [Theory ×5]
DrillDown_ReturnsPerPatientRowsWithAllEightColumns
BuildCommissions_UsesEntityPercent
BuildCommissions_ZeroWhenNoReferralEntities
AccountsHub_BindsElementKindAndReportType                          ← فحص ربط
PeriodPresets_TodayWeekMonthYear_SetCorrectRange
```

### خطوات التنفيذ

1. **أولاً** اربط `ElementKind` و`ReportType` و`ElementId` و`AccountTypeFilter` بمكوّنات — هذا alone يُحيي أربع مقاييس كاملة.
2. أنشئ `GetElementPickerQuery` وخزّن القوائم في VM.
3. اجعل `ElementOptions` تُحمَّل عند تغيّر `ElementKind` (لا مرة واحدة عند الإقلاع).
4. أصلح `Summary` ليُخرج صفاً تجميعياً.
5. أضف شبكة `CommissionsAndShares` بصف `الإجمالي` بنسبة 100%.
6. أضف صف «إجمالي عينات المرضى» قابلاً للنقر + نافذة `PatientSamplesDrillDownWindow`.
7. أضف أزرار الفترات الجاهزة.
8. ادفع تجميع `GetPatientCountStatisticsQueryHandler` إلى SQL.
9. استبدل نمط `IN` في `GetSentOutStatisticsQueryHandler.cs:49-52` بـ join (حدّ 2100 معامل SQL Server).
10. أضف `ComboBox` الفرع **بعد** WP-15.
11. اكتب الاختبارات العشرة — وخصوصاً `AccountsHub_BindsElementKindAndReportType` الذي كان سيلتقط العيب.

### الاعتماديات
**WP-04**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تجربة مستخدم | أربعة محدّدات + فترات على تبويب واحد | صفوف أفقية + تمرير أفقي؛ نقرة على «جرد العناصر» تبقى كما هي |
| بيانات | جدول العمولات يعرض أصفاراً إن لم تُنشأ جهات إحالة بعد | سطر إرشادي «أنشئ جهة إحالة بنسبة أولاً» |
| تقنية | `Summary` behaviour تغيّر — أي عميل آخر يعتمد عليه؟ | `grep GetElementInventory` في `src/` قبل التعديل |

### معايير القبول
- [ ] كل تركيبة من 5×4 تُعيد بيانات غير فارغة (10 تركيبات)
- [ ] جدول العمولات يعرض قيماً ≠ 0 بعد إنشاء جهة إحالة بنسبة
- [ ] النقر على الإجمالي يفتح نافذة التفصيل بالأعمدة الثمانية
- [ ] الفترات الجاهزة تضبط `From`/`To` بدقة
- [ ] `AccountsHub_BindsElementKindAndReportType` أخضر

### التراجع
ربط XAML فقط + تعديل معالج `Summary`. آمن. **لكن** إرجاع `Summary` إلى قائمة فارغة يُعيد العيب — لا تفعل ذلك عند التراجع، بل أبقِ الصف التجميعي.

---

### WP-10 — تقارير التاريخ: الوصول + صحّتها + مصفوفة CBC

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | الشاشة يتيمة، ومسار متعدد المرضى ميت حتماً، ولا فلتر تاريخ ولا فلتر نفس الاختبار. |
| **المرجع** | R-32, R-33, R-34, R-35 · D-11 · تقرير الوكيل الفرعي الأول Cap 8 |
| **الحجم** | **L** (5 أيام) |
| **الهجرات** | **none** |
| **الاعتماديات** | **WP-01** (الرسم العربي) · **WP-02** (الوصول للشاشة) |

### الحالة الحالية — PROVED DEFECT / MISSING FEATURE

| الملاحظة | الدليل |
|---|---|
| الشاشة يتيمة | `grep NavigateTo<HistoryReportsViewModel>` ⇒ صفر |
| متعدد المرضى ميت دائماً | `HistoryReportsViewModel.cs:30` `private ObservableCollection<int> _multiPatientIds = new();` — بلا خاصية ولا setter ولا UI |
| | `LoadMultiAsync:157-160` ⇒ يُعيد دائماً `"قائمة المرضى مطلوبة."` |
| لا فلتر تاريخ | `GetPatientTestHistoryQuery(int PatientId)` · `GetMultiPatientHistoryQuery(IReadOnlyList<int>)` · `GetSeparateHistoryReportQuery(int PatientId)` — لا معاملات تاريخ إطلاقاً |
| فلتر نفس الاختبار **لم يُكتب** | `InsertHistoryDialogViewModel.cs:88-90` تعليق «Filter entries for the same test» ثم `Entries = new ObservableCollection<HistoryEntryDto>(allEntries);` |
| لا مصفوفة CBC | `HistoryEntryDto` سطر مسطّح · `PatientHistoryReader.BuildEntries:59-87` يقرأ `PatientTest` فقط · `HistoryInsertion.cs:105-107` يثبّت `FrozenRangeText: null, ProfileLines: Array.Empty<>()` |
| المعالج المكرر | `GetSeparateHistoryReportQueryHandler` نسخة حرفية من `GetPatientTestHistoryQueryHandler` (XML doc `:21-26` يصف نفسه بـ «fraternal twin») |
| التواريخ مفقودة في المخرج | `ReportPdfWriter.cs:124-135` يطبع `TestCode TestName: value` بلا تاريخ، مع أن `HistoryEntryDto` يحمل `EnteredAtUtc` |
| مسح جدول المرضى كاملاً | `PatientHistoryReader.cs:29-36` في وضع `ByPatientName` |

### الحالة المستهدفة
من شاشة التاريخ يمكن: اختيار مريض واحد أو عدة مرضى، تحديد مدى تاريخي، تصفية بنفس الاختبار (أو عرض مصفوفة CBC)، الفرز بالاسم أو Lab ID، والإدراج في التقرير نفسه أو منفصل — وكل ذلك ينتج تقريراً عربياً مقروءاً **بتواريخ**.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Application/Features/ReportProduction/Queries/GetPatientTestHistory/{GetPatientTestHistoryQuery.cs, GetPatientTestHistoryQueryHandler.cs}
  src/TopLab.Application/Features/ReportProduction/Queries/GetMultiPatientHistory/{...}
  src/TopLab.Application/Features/ReportProduction/Queries/GetSeparateHistoryReport/{...}
  src/TopLab.Application/Features/ReportProduction/Commands/InsertHistoryResult/InsertHistoryResultCommandHandler.cs
  src/TopLab.Application/Features/ReportProduction/Common/PatientHistoryReader.cs
  src/TopLab.Presentation/ViewModels/Patients/HistoryReportsViewModel.cs
  src/TopLab.Presentation/Views/Patients/HistoryReportsView.xaml
  src/TopLab.Presentation/ViewModels/Patients/InsertHistoryDialogViewModel.cs
  src/TopLab.Infrastructure/Printing/ReportDocument.cs   ← من WP-01، يُوسَّع

يُحذف:
  src/TopLab.Application/Features/ReportProduction/Queries/GetSeparateHistoryReport/GetSeparateHistoryReportQueryHandler.cs
                                                 (يُستبدل بإعادة استخدام)

يُضاف:
  src/TopLab.Application/Features/ReportProduction/Common/HistoryMatrixBuilder.cs
  src/TopLab.Application/Features/ReportProduction/Common/HistoryMatrixRow.cs
  tests/TopLab.Application.Tests/Features/ReportProduction/HistoryFilterTests.cs
  tests/TopLab.Application.Tests/Features/ReportProduction/HistoryMatrixTests.cs
```

### المكونات

**Domain** — لا تغيير (الكيانات قائمة).

**Application**
```csharp
public sealed record GetPatientTestHistoryQuery(
    int PatientId,
    DateOnly? FromUtc = null,
    DateOnly? ToUtc = null,
    int? TestId = null) : IAuthorizedRequest { … }

public sealed record HistoryMatrixRow(
    DateOnly SampleDate,
    IReadOnlyList<HistoryMatrixCell> Cells);
public sealed record HistoryMatrixCell(
    string AnalyteName, string? ResultValue, string? Unit, string? Flag, string? LowComment);
```
- `HistoryMatrixBuilder` — يحمّل `PatientTest` + `ProfileResultItem`، ثم **يحوّل (pivot)** إلى صفوف تواريخ × أعمدة صنف.
- `PatientHistoryReader` — انقل الفرز إلى SQL: `ByPatientName` يستعمل `EF.Functions.Collate` أو ترتيب SQL بدل `ToList()` ثم LINQ-to-Objects.
- `InsertHistoryDialogViewModel.cs:88-90` — **اكتب الفلتر فعلاً**:
  ```csharp
  var targetTestId = _targetPatientTestId is { } id
      ? _testLookup.GetTestId(id)
      : (int?)null;
  Entries = new ObservableCollection<HistoryEntryDto>(
      allEntries.Where(e => targetTestId is null || e.TestId == targetTestId));
  ```
- `HistoryEntryDto` — أضف `TestId` و`EnteredAtLocalDate` و`LowComment`/`HighComment` (من WP-07).
- **حذف** المعالج المكرر واستبدله باستدعاء `GetPatientTestHistoryQueryHandler` مع معاملات مختلفة.

**Infrastructure** — `ReportDocument` يرسم المصفوفة: ترويسة صنف ثابتة (أعمدة) + صف لكل تاريخ. إن تجاوز عمود العرض الصفحة ⇒ التفاف إلى جدولين متتاليين.

**Presentation**
- `HistoryReportsViewModel`:
  - `public ObservableCollection<int> MultiPatientIds { get; }` — **خاصية عامة** (هذا ما يُميّها).
  - `SelectedPatientIds` مرتبطة بـ `ListBox SelectionMode="Multi"`.
  - `FromDate` / `ToDate` / `SelectedTestId` / `PrintSeparately` / `SortMode`.
- `HistoryReportsView.xaml` — ListBox + DatePicker×2 + ComboBox اختبار + CheckBox تقرير منفصل.
- أسفل الشاشة: معاينة المصفوفة قبل الطباعة.

**Tests**
```
GetPatientTestHistory_FiltersByDateRange
GetPatientTestHistory_FiltersByTestId
GetPatientTestHistory_ExcludesOtherPatients
InsertHistoryDialog_ExcludesOtherTests        ← حرج
InsertHistoryDialog_IncludesSameTestAcrossVisits
GetMultiPatientHistory_ReturnsUnionOfVisits
GetMultiPatientHistory_OrdersByLabCode        [Theory: ByLabCode | ByPatientName]
HistoryMatrix_PivotsAnalytesByDate            ← حرج
HistoryMatrix_GroupsSameDateIntoOneRow
HistoryMatrix_HandlesMissingAnalyteCell
HistoryReports_MultiPatientIds_IsPublicObservable
GetSeparateHistoryReport_ReusesSameQueryShape
PatientHistoryReader_ByPatientName_DoesNotMaterializeAllPatients
Report_HistorySection_PrintsSampleDates
```

### خطوات التنفيذ

1. **أولاً** اجعل `MultiPatientIds` خاصية عامة واربطها بـ ListBox متعدد الاختيار — يُحيي المسار الميت.
2. أضف `FromUtc`/`ToUtc`/`TestId` إلى الاستعلامات الثلاثة **بمعاملات اختيارية** (لا كسر للاستدعاءات القائمة).
3. اكتب `InsertHistoryDialog_ExcludesOtherTests` أولاً — يجب أن يفشل قبل الفلتر.
4. اكتب الفلتر في `InsertHistoryDialogViewModel.cs:88-90`.
5. **احذف** `GetSeparateHistoryReportQueryHandler` واستبدله بإعادة استخدام. حدّث كل الاستدعاءات.
6. أنشئ `HistoryMatrixBuilder` + `HistoryMatrixRow`.
7. ارسم المصفوفة في `ReportDocument`.
8. أضف التواريخ لكتلة التاريخ في التقرير.
9. انقل فرز `PatientHistoryReader` إلى SQL.
10. اكتب الاختبارات الخمسة عشر.

### الاعتماديات
**WP-01** · **WP-02**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تقنية | أعمدة CBC متغيّرة العدد (14 صنفاً في المرجع) | عرض ثابت بأعمدة معرَّفة + التفاف لجدول ثانٍ |
| تجربة مستند | شاشة واحدة تجمع أربع مهام | أربع مجموعات بـ `GroupBox` |
| **تقنية** | حذف معالج مكرر قد يكسر استدعاءً | `grep GetSeparateHistoryReportQuery` قبل الحذف |
| بيانات | لا شيء | — |

### معايير القبول
- [ ] شاشة التاريخ تُفتح من `PatientsHubView` (WP-02)
- [ ] اختيار مريضين ومدة ⇒ تقرير واحد مرتّب
- [ ] الفلتر بالاختبار يمنع إدخال نتيجة جلوكوز في تقرير صورة دم
- [ ] التواريخ مطبوعة
- [ ] `GetSeparateHistoryReport` لم يعد له معالج خاص

### التراجع
**الحذر:** حذف المعالج المكرر يجب أن يكون **آخر** خطوة في الدمج، بعد نجاح الاختبارات. التراجع الجزئي ممكن بـ `git checkout` لملف واحد.

---

### WP-11 — زر «تحديث القيمة المرجعية»

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | معالج كامل ومختبَر بلا أي مُطلِب في الواجهة. نصف قاعدة المرجع (إعادة التطبيق بعد التغيير) غير قابل للوصول. |
| **المرجع** | R-49 · D-07 |
| **الحجم** | S (نصف يوم) |
| **الهجرات** | **none** |
| **الاعتماديات** | **WP-02** (الشاشة يجب أن تكون قابلة للوصول أولاً) |

### الحالة الحالية — PROVED DEFECT
`RefreshResultReferenceRangeCommandHandler` كامل (`:27-89`) يعيد حلّ النطاق ويعيد حساب العلم. `grep RefreshResultReferenceRangeCommand src/TopLab.Presentation` ⇒ **صفر نتيجة** (فقط الاختبارات). الوظيفة لا تُستدعى من أي مكان.

الجزء الأهم (**التجميد**) صحيح: النتائج القديمة تحتفظ بنطاقها عبر `PatientTestReferenceRangeSnapshot`. الناقص هو الجزء الثاني — إعادة التطبيق.

### الحالة المستهدفة
زر «تحديث القيمة المرجعية» على شاشة إدخال النتيجة، بتأكيد صريح يعرض المدى القديم والجديد، يعيد حساب المدى والعلم لهذا الاختبار ويعرض القيمة الجديدة. النتائج الأخرى لا تتأثر.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs
  src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml
  src/TopLab.Presentation/Views/Patients/SimpleResultEntryView.xaml

يُضاف:
  src/TopLab.Presentation/Views/Patients/RefreshRangeConfirmationWindow.xaml
  src/TopLab.Presentation/ViewModels/Patients/RefreshRangeConfirmationViewModel.cs
  tests/TopLab.Presentation.Tests/Patients/RefreshRangeCommandTests.cs
```

### المكونات

**Domain** — لا تغيير.

**Application** — لا تغيير. `RefreshResultReferenceRangeCommand` قائم.

**Infrastructure** — لا تغيير.

**Presentation**
- `ProfileEntryViewModel`:
  ```csharp
  RefreshRangeCommand = new AsyncRelayCommand(async (_, ct) => await RefreshRangeAsync(ct));
  public bool IsRefreshRangeAvailable { get; private set; }
  ```
- `IsRefreshRangeAvailable` يُحسب عند التحميل: `IsRangeStale` = النطاق الحالي للمريض **لا يساوي** النطاق المجمّد على النتيجة.
  (الاستعلام القائم `GetResultEntryQuery` يُعيد `RefRanges` و`FrozenRange` — قارن بينهما.)
- `RefreshRangeAsync`:
  1. افتح نافذة تأكيد تعرض: النطاق المجمّد الحالي + النطاق الجديد + العلم قبل/بعد.
  2. عند الموافقة ⇒ `_mediator.Send(new RefreshResultReferenceRangeCommand(...))`.
  3. أعد التحميل واظهر `StatusMessage = «تم تحديث القيمة المرجعية.»`.
  4. عند الرفض ⇒ لا شيء.
- الزر في شريط الأدوات إلى جانب «تحديث / Refresh» الموجود.

**Tests**
```
RefreshRange_IsHiddenWhenRangeIsCurrent
RefreshRange_IsVisibleWhenRangeIsStale
RefreshRange_RequiresConfirmation
RefreshRange_Declined_ChangesNothing
RefreshRange_Confirmed_RecomputesFlag
RefreshRange_AffectsOnlyTargetPatientTest
RefreshRange_DoesNotRecomputeOtherVisitsOfSamePatient
```

### الاعتماديات
**WP-02**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تجربة مستخدم | زر يغيّر علم نتيجة محفوظة | التأكيد إجباري ويعرض الفرق |
| — | لا شيء آخر | — |

### معايير القبول
- [ ] الزر يظهر فقط عند تغيّر النطاق
- [ ] التنفيذ يحدّث العلم
- [ ] نتائج الزيارات الأخرى لنفس المريض لا تتأثر
- [ ] الرفض لا يغيّر شيئاً

### التراجع
زر + أمر. آمن.

---

### WP-12 — اكتمال شبكة إدخال النتائج وشاشة الإدخال البسيط

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | الشاشة البسيطة تحوي `ComboBox` للعلم بلا `ItemsSource` — أي أن المستخدم **لا يستطيع ضبط H/L إطلاقاً**. وعمودا الوحدة والمدى غائبان. و«مسح» يوهم بحفظ ولا يحفظ. |
| **المرجع** | R-16, R-46, R-47 · تقرير الوكيل الفرعي الأول Cap 4 |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **none** |
| **الاعتماديات** | **WP-02** · **WP-08** (عمود الوحدة) |

### الحالة الحالية — PROVED DEFECT

```
SimpleResultEntryView.xaml:38
  <ComboBox Text="{Binding ResultFlag}" Margin="0,4"/>     ← بلا ItemsSource ⇒ غير وظيفي

SimpleResultEntryViewModel.cs:95-96
  // Reference ranges loaded client-side if needed
  await Task.CompletedTask;                ← ReferenceRanges و FrozenRange ميتتان خلف شفرة فارغة
  :12-13  التعليق ي claiming "Displays the ResultEntryDto fields" وهو غير صحيح

SimpleResultEntryViewModel.cs:133-145
  زر «مسح» يطلب «هل أنت متأكد من مسح هذه النتيجة؟» ثم يفرّغ مربع النص فقط
  ⇒ واجهة توهم بمحو محفوظ لا يحدث

SimpleResultEntryView.xaml:51-54
  زرّان فقط: «مسح» و«حفظ» — لا تحقّق، لا طباعة، لا تقرير

PatientResultSheetView.xaml:68-74   الأعمدة: TestName · TestCode · ResultValue · ResultFlag
                                     · Notes · IsReviewed · IsPrinted
                                     ⇒ لا وحدة، لا مدى مرجعي
ProfileEntryView.xaml:77-92         فيه «الوحدة» ✓ لكن لا عمود مدى رغم
                                     ProfileEntryRow.FrozenRange (:33) معبّأ

ResultsWorklistViewModel.cs:159
  TotalCount = result.Value.Count   ← حجم الصفحة مُسمّى إجمالياً
GetResultWorklistQueryHandler.cs:28-30
  DateOnly.FromDateTime(p.RegistrationDateUtc) == day   ← UTC مقابل DateOnly
```

### الحالة المستهدفة
الشاشة البسيطة تعرض النطاق المجمّد والوحدة، والعلم قابل للاختيار من قائمة، وأزرار «تحقّق» و«طباعة» و«تقرير» تعمل عبر منسّق WP-06، وزر «مسح» يمسح النتيجة المحفوظة فعلياً مع أثر تدقيق؛ والشبكتان تعرضان الوحدة والمدى المرجعي؛ والإجمالي صحيح.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Presentation/Views/Patients/SimpleResultEntryView.xaml
  src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs
  src/TopLab.Presentation/Views/Patients/PatientResultSheetView.xaml
  src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml
  src/TopLab.Presentation/ViewModels/Patients/ResultsWorklistViewModel.cs
  src/TopLab.Application/Features/ResultsEntry/Queries/GetResultWorklist/GetResultWorklistQuery.cs
  src/TopLab.Application/Features/ResultsEntry/Queries/GetResultWorklist/GetResultWorklistQueryHandler.cs
  src/TopLab.Application/Features/ResultsEntry/Queries/GetResultEntry/GetResultEntryQueryHandler.cs
  src/TopLab.Application/Features/ResultsEntry/Common/ResultsEntryDtos.cs

يُضاف:
  src/TopLab.Application/Features/ResultsEntry/Queries/GetResultEntry/ResultEntryLoadingService.cs
  tests/TopLab.Application.Tests/Features/ResultsEntry/SimpleResultEntryLoadingTests.cs
  tests/TopLab.Presentation.Tests/Patients/ResultSheetColumnTests.cs
```

### المكونات

**Domain** — لا تغيير. `ClearResultCommand` قائم.

**Application**
- **نفّذ `SimpleResultEntryViewModel.LoadAsync`**: استدعِ `GetResultEntryQuery` (أو `ResultEntryLoadingService` مُستخرج) واملأ `ReferenceRanges` و`FrozenRange` و`Unit`. **احذف التعليق المضلّل في `:12-13`.**
- `ResultSheetLineDto` — أضف `Unit` و`FrozenRange` و`LowComment`/`HighComment` (WP-07).
- `GetResultWorklistQuery` — أضف `int TotalCount` و`int PageNumber` و`int PageSize`، وأرجعهم في `PagedResult<ResultWorklistItemDto>`.
- `GetResultWorklistQueryHandler.cs:28-30` — استعمل تاريخ **محلي** للمقارنة:
  ```csharp
  DateOnly.FromDateTime(DateTime.SpecifyKind(p.RegistrationDateUtc, DateTimeKind.Utc)
                             .ToLocalTime()) == day
  ```
  أو، **الأفضل**: أضف `RegistrationDateLocal` محسوباً في الاستعلام وتجنّب التحويل في الذاكرة.

**Infrastructure** — لا مخطط.

**Presentation**
```xml
<!-- SimpleResultEntryView.xaml:38 — استبدل -->
<ComboBox ItemsSource="{Binding FlagOptions}"
          DisplayMemberPath="Display"
          SelectedValuePath="Value"
          SelectedValue="{Binding ResultFlag, Mode=TwoWay}"/>
<!-- FlagOptions: H (مرتفع) · L (منخفض) · N (طبيعي) · null (بدون) -->

<!-- SimpleResultEntryView.xaml:51-54 — أضف إلى شريط الأدوات -->
<Button Content="تحقّق" Command="{Binding VerifyCommand}"/>
<Button Content="طباعة" Command="{Binding PrintCommand}"/>     ← عبر IResultPrintCoordinator
<Button Content="تقرير" Command="{Binding ShowReportCommand}"/> ← عبر IPdfPreviewService
<Button Content="تحديث النطاق" Command="{Binding RefreshRangeCommand}"/>  ← WP-11
<Button Content="مسح"    Command="{Binding ClearCommand}"/>    ← ClearResultCommand الحقيقي

<!-- PatientResultSheetView.xaml — أعمدة جديدة -->
<DataGridTextColumn Header="الوحدة"     Binding="{Binding Unit}"/>
<DataGridTextColumn Header="المدى"      Binding="{Binding FrozenRange}"/>
<DataGridTextColumn Header="التعليق"    Binding="{Binding LowComment}"/>

<!-- ProfileEntryView.xaml — عمود واحد -->
<DataGridTextColumn Header="المدى المرجعي" Binding="{Binding FrozenRange}"/>
```
- `SimpleResultEntryViewModel.ClearAsync` — استدعِ `ClearResultCommand`.
  **حارس — مهم [C4]:** `PatientTest.ClearResult()` (`:131-133`) يرمي إن كانت `IsReviewed || IsPrinted || IsDelivered`.
  **ثلاث حالات، لا حالتان:**
  | الحالة | الرسالة الصحيحة |
  |---|---|
  | `IsReviewed` فقط | «ألغِ المراجعة أولاً (زر: إلغاء المراجعة)، ثم المسح.» — متاح فعلاً |
  | `IsPrinted` أو `IsDelivered` | «النتيجة مطبوعة/مسلَّمة ولا تُمسح. تُعدَّل عبر **التعديل** instead.» |
  > **تصحيح [C4]:** الرسالة الأولى في هذه الخطة كانت تقول «استخدم التعديل (Amend)» لكل الحالات —
  > وهو **غير متاح لنتيجتَي الاختبار البسيط والمزرعة**: `ProfileResultAmendment` مرتبط بـ
  > `ProfileResultItemId` وحده، ولا يوجد `AmendSimpleResult` ولا `AmendCultureResult`.
  > وبلا تحديد الحالة كان `ClearResult` يرمي بلا معالجة لحالة `IsReviewed`.
  > **الحل الصحيح في هذه الحزمة:** رسالة حسب الحالة + تحويل `IsReviewed` إلى إجراء متاح.
  > **العدّاد الدائم للتعديل على البسيط والمزرعة ⇒ WP-30.**
- `SimpleResultEntryViewModel` — اجعل `ReferenceRanges` و`FrozenRange` **مملوءين فعلاً** (هما مُعلَنان `:82-83`).

**اختبارات إضافية [C16]:**
```
WorkSheet_PrintTimestamp_UsesLocalDateNotUtc
WorkSheet_MatchesResultWorklistDayBoundary
```

**Tests**
```
SimpleEntry_LoadAsync_PopulatesFrozenRangeAndUnit
SimpleEntry_LoadAsync_PopulatesReferenceRanges
SimpleEntry_FlagComboBox_PersistsHigh
SimpleEntry_FlagComboBox_PersistsLow
SimpleEntry_FlagComboBox_PersistsNull
SimpleEntry_HasVerifyPrintReportButtons
ClearCommand_ClearsPersistedValue
ClearCommand_BlockedWhenPrinted
ClearCommand_BlockedWhenReviewed
ResultSheet_IncludesUnitAndRangeColumns
ProfileEntry_IncludesRangeColumn
Worklist_TotalCount_IsServerTotal_NotPageSize
Worklist_TodayFilter_UsesLocalDate
WorkSheet_PrintTimestamp_UsesLocalDateNotUtc      ← [C16] WorkSheetPdfWriter.cs:158
```

### الاعتماديات
**WP-02** · **WP-08**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تجربة مستخدم | زر «مسح» صار يخزّن — سلوك مختلف عمّا توقّعه المستخدم | نصّ التأكيد يذكر الحفظ صراحة: «سيتم حذف النتيجة المحفوظة. متابعة؟» |
| تقنية | حارس «مسح بعد الطباعة» قد يمنع ما اعتاده المستخدم | **رسالة حسب الحالة** (راجع C4 أعلاه)؛ `IsReviewed` → إلغاء المراجعة أولاً؛ المطبوع/المسلَّم → WP-30 |
| **تقنية** | تغيير التوقيت المحلي قد يغيّر «اليوم» لزر مرضى الليل | اختبار `Worklist_TodayFilter_UsesLocalDate` إلزامي |
| **تقنية** **[C16]** | `WorkSheetPdfWriter.cs:158` يكتب `$"تاريخ الطباعة: {DateTime.UtcNow…}"` — توقيت UTC معروض كمحلي | مرّر الوقت من `IDateTimeProvider` بدل `DateTime.UtcNow`؛ **عالَجها مع** `GetResultWorklistQueryHandler.cs:29` في نفس الحزمة — صنف العيب واحد |

### معايير القبول
- [ ] اختيار علم من الشاشة البسيطة يُحفظ ويعود بعد إعادة الفتح
- [ ] المدى والوحدة يظهران في الشاشة البسيطة والشبكتين
- [ ] المسح ينعكس على قاعدة البيانات والواجهة معاً
- [ ] المسح بعد الطباعة يُرفض برسالة موجّهة للتعديل
- [ ] الإجمالي في قائمة العمل = العدد الكل لا حجم الصفحة
- [ ] لا `await Task.CompletedTask` في VMs

### التراجع
آمن — تغييرات XAML + VM + DTO قابلة للإرجاع.

---

### WP-13 — خيارات التقرير المركّب + ملاحظة «خارج المعمل» + تعليقات الاختبار

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | صندوقا خيارات المواصفة غير موجودين، وراية موجودة في النموذج لا تصل أي تقرير، وقائمة تعليقات الاختبار معزولة تماماً. |
| **المرجع** | R-08, R-19, R-53 · D-01 · D-25 · تقرير الوكيل الفرعي الأول Caps 2, 5, 9 |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **migration جديدة** — `AddCombinedReportPrintOptions` |
| **الاعتماديات** | **WP-01** · **WP-02** |

### الحالة الحالية

| الملاحظة | الدليل | التصنيف |
|---|---|---|
| لا راية «طباعة عنوان فرعي» | `ReportSettings.cs:9-27` لا تحتويها · `grep` في `src/` ⇒ صفر | MISSING FEATURE |
| لا راية «طباعة الاختبار المطبوع دون رسالة» | `BulkPrintDtos.cs:5-6` `ReprintConfirmationMessage` حوار لكل مريض على مسار الطباعة المجمّعة فقط | MISSING FEATURE |
| `IsTakenOutsideLab` لا يصل أي تقرير | `PatientTest.cs:26` محفوظ · consumer الوحيد `WorkSheetPdfWriter.cs:218` · `CombinedReportLineDto`/`HistoryEntryDto`/`CultureReportDto` بلا العضو | PROVED DEFECT |
| `TestComment` معزول | CRUD كامل في `TestComment.cs:6-53` + `TestCommentsView.xaml` · `grep TestComment` في `Features/{ReportProduction,ResultsEntry,CultureResults,ProfileResults}` ⇒ **صفر** | PROVED DEFECT |
| «ملاحظة» في الإدخال = نص حر | `ProfileEntryView.xaml:24,27,111` ⇒ `PatientTest.Notes` عبر `SaveProfileResultsCommandHandler.cs:94` — ملاحظة لكل طلب، لا منتقي من القائمة | PROVED DEFECT |

### الحالة المستهدفة
صندوق «طباعة العنوان الفرعي (اسم المجموعة) في التقرير»؛ وصندوق «طباعة الاختبار المطبوع مرة أخرى دون رسالة»؛ وملاحظة في التقرير أن العينة أُخذت خارج المعمل؛ وزر «تعليق» في شاشات الإدخال الثلاثة يختار من `TestComment` ويظهر النص في التقرير.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Settings/ReportSettings.cs
  src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateReportSettings/UpdateReportSettingsCommand.cs
  src/TopLab.Application/Features/SystemAndPrintSettings/Queries/GetReportSettings/GetReportSettingsQueryHandler.cs
  src/TopLab.Application/Features/ReportProduction/Common/ReportDtos.cs
  src/TopLab.Application/Features/ReportProduction/Commands/BuildCombinedReport/BuildCombinedReportCommandHandler.cs
  src/TopLab.Application/Features/ResultsEntry/Commands/BulkPrint/ExecuteBulkPrintCommandHandler.cs
  src/TopLab.Infrastructure/Printing/ReportDocument.cs   ← من WP-01، يُوسَّع
  src/TopLab.Presentation/Views/Settings/ReportSettingsView.xaml
  src/TopLab.Presentation/Views/Patients/CombinedReportView.xaml
  src/TopLab.Presentation/ViewModels/Patients/CombinedReportViewModel.cs

يُضاف:
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetTestCommentsForResults/
  src/TopLab.Presentation/Views/Patients/TestCommentPickerWindow.xaml
  src/TopLab.Presentation/ViewModels/Patients/TestCommentPickerViewModel.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddCombinedReportPrintOptions.cs
  tests/TopLab.Application.Tests/Features/ReportProduction/CombinedReportOptionsTests.cs
```

### المكونات

**Domain**
- `ReportSettings.PrintGroupSubTitle` (bool, default `false`).
- `ReportSettings.SuppressReprintMessage` (bool, default `false` — **سلوك مطابق للحالي**).
- `ReportSettings.CreateDefaults` — أضف الحقلين.

**Application**
- الحقلان في `UpdateReportSettingsCommand` (`:8-16`) + `GetReportSettings` DTO + `UpdateReportSettingsCommandHandler` (`:25-29`).
- `CombinedReportLineDto.IsTakenOutsideLab` — يُملأ من `PatientTest.IsTakenOutsideLab`، ويُملأ لـ `HistoryEntryDto` أيضاً.
- `GetTestCommentsForResultsQuery(IReadOnlyList<int> testIds)` → قائمة تعليقات مجمّعة، مع التحقق من الصلاحية.
- `BuildCombinedReportCommandHandler` — يمرّر `TestComments` إلى DTO.
- `ExecuteBulkPrintCommandHandler:53-58` — يقرأ `SuppressReprintMessage`:
  ```csharp
  if (pt.IsPrinted && !settings.SuppressReprintMessage && !decision.ConfirmReprint)
      continue;   // السلوك الحالي
  ```
  مع الراية: عند `true` يتخطّى الحوار ويطبع مباشرة.

**Infrastructure**
- هجرة `AddCombinedReportPrintOptions` (عمودان `bit null` ⇒ backfill بـ `false`).
- `ReportDocument` يرسم: العنوان الفرعي قبل كل مجموعة · سطر ملاحظة «العينة أُخذت خارج المعمل» بلون مميّز · التعليقات بعد النتيجة.

**Presentation**
```xml
<!-- CombinedReportView.xaml — أضف -->
<CheckBox Content="طباعة العنوان الفرعي (اسم المجموعة) في التقرير"
          IsChecked="{Binding PrintGroupSubTitle}" IsEnabled="{Binding DataContext.PrintGroupSubTitle,
          RelativeSource={RelativeSource AncestorType=Window}}"/>
<CheckBox Content="طباعة الاختبار المطبوع مرة أخرى دون رسالة"
          IsChecked="{Binding SuppressReprintMessage}" .../>

<!-- ReportSettingsView.xaml — نفس الصندوقين بشكل دائم -->
```
- زر «تعليق» في `SimpleResultEntryView` و`ProfileEntryView` و`CultureEntryView` + `TestCommentPickerWindow` (قائمة + معاينة + «إدراج» + «تعليق جديد»).

**Tests**
```
UpdateReportSettings_PersistsPrintGroupSubTitle
UpdateReportSettings_PersistsSuppressReprintMessage
BuildCombinedReport_HonoursSubTitleSetting
BuildCombinedReport_HonoursSuppressReprintSetting
BuildCombinedReport_OutsideLab_CarriesNote
BuildCombinedReport_InLab_CarriesNoNote
GetTestCommentsForResults_ReturnsCommentsForEachTest
InsertedComment_AppearsInReportPayload
CommentPicker_DoesNotMutatePatientTest
BulkPrint_SuppressReprint_SkipsConfirmation
BulkPrint_DefaultBehaviour_ShowsConfirmation
```

### خطوات التنفيذ

1. أضف الرايتين للنموذج + الأمر + DTO + الواجهة + **الهجرة**.
2. اربط الصندوقين في `CombinedReportView.xaml` و`ReportSettingsView.xaml`.
3. اقرأ الرايتين في `CombinedReportViewModel` عند البناء والطباعة.
4. أضف `IsTakenOutsideLab` إلى DTOs.
5. أنشئ `GetTestCommentsForResultsQuery` بمجمّع واحد (تجنّب N+1).
6. أضف `TestComments` إلى DTO وارسمها.
7. أنشئ `TestCommentPickerWindow` واربطه بالأزرار الثلاثة.
8. عدّل `ExecuteBulkPrintCommandHandler` للراية.
9. ارسم العنوان الفرعي والملاحظة والتعليقات في `ReportDocument`.
10. اكتب الاختبارات الإحدى عشرة.

### الاعتماديات
**WP-01** · **WP-02**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| بيانات | عمودان `bit` على جدول إعدادات وحيد | backfill بـ `false` |
| تجربة مستند | زر «تعليق» رابع في ثلاث شاشات | اجعله في قائمة منسدلة لا شريط ممتلئ |
| — | الراية الافتراضية `false` تحفظ السلوك الحالي | قرار متعمّد |

### معايير القبول
- [ ] الرايتان تُحفظان وتؤثّران في الـ PDF
- [ ] تقرير عينة خارجية يحوي الملاحظة
- [ ] التعليق المُختار يظهر في الـ PDF
- [ ] الطباعة المجمّعة تحترم الراية

### التراجع
عمودان nullable + ربط. آمن.

---

### WP-14 — عمق المزرعة: الفحص المجهري، منطقة التثبيط، الأسماء التجارية

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | قسم كامل من علم الأحياء الدقيقة غير ممثَّل إطلاقاً: لا إدخال ولا تخزين ولا طباعة. وهو من أندر المخرجات في مختبر الميكروبيولوجيا. |
| **المرجع** | R-21, R-22, R-23, R-24, R-57, R-58 · D-09, D-10, D-23, D-27 |
| **الحجم** | **L** (5–6 أيام) |
| **الهجرات** | **migration جديدة ×2** — `AddCultureMicroscopyAndZone` · `AddAntibioticMasterFields` |
| **الاعتماديات** | **WP-01** · **WP-03** · **WP-06** |

### الحالة الحالية

| الملاحظة | الدليل | التصنيف |
|---|---|---|
| لا فحص مجهري إطلاقاً | `CultureResult.cs:10-20` ست حقول فقط · `grep "Microscop\|PusCell\|Epithelial\|Crystal\|Fungi"` ⇒ **صفر** في `src/` | MISSING FEATURE |
| لا منطقة تثبيط | `CultureAntibioticResult.cs:66-70` `AntibioticId` + `SensitivityCategory` فقط | MISSING FEATURE |
| لا اسم تجاري | `Antibiotic.cs:8-12` ثلاثة حقول فقط | MISSING FEATURE |
| شبكة مسطّحة بلا أعمدة | `CultureEntryView.xaml:107-121` | PROVED DEFECT |
| تقرير المزرعة سطر واحد | `ReportPdfWriter.cs:91-102` سطر مفصول بفواصل — بلا جدول حساسية | PROVED DEFECT |
| **اكتشاف الطفل معطوب** | `GetCultureEntryGridQueryHandler.cs:33` `patient.AgeUnit == AgeUnit.Year && patient.AgeValue < 12` ⇒ رضيع 11 شهراً **ليس** طفلاً | PROVED DEFECT |
| لا عتبة رقمية | `CultureAntibioticAttachment` بلا `SensitivityThresholdMm` | MISSING FEATURE |
| لا مفاتيح عرض | `grep "ShowSensitivity\|ShowReference\|ShowCommercial"` ⇒ **صفر** · `CultureReportDto` بلا رايات | MISSING FEATURE |
| `OrganismC` مفقود من المعاينة | `CultureEntryView.xaml:140-144` يتجاهله رغم أنه قابل للتحرير في `:49` | PROVED DEFECT |
| استعلام إعدادات غير متّسق | `GetCultureReportQueryHandler.cs:26` `FirstOrDefault()` بلا `Id == 1` بينما كل المعالجات الأخرى تستعمل `SingleOrDefault(s => s.Id == 1)` | POTENTIAL RISK |

### الحالة المستهدفة
تُدخل الفحوص المجهرية (خلايا صديدية / كريات حمراء / خلايا بطانية / بلورات / فطريات / أخرى×3 / مباشر؟)، ومنطقة التثبيط بالملم، والاسم التجاري للمضاد؛ ويظهر ذلك في تقرير الميكروبيولوجي **كمجدول حساسية حقيقي**؛ وقائمة الأطفال تُصحَّح للرضّع؛ ومفاتيح العرض الثلاثة تتحكم بأعمدة التقرير؛ وزر طباعة المزرعة ينتج PDF عبر منسّق WP-06.

### الملفات المتأثرة

```
يُضاف (Domain):
  src/TopLab.Domain/Results/CultureMicroscopy.cs
  src/TopLab.Domain/Tests/AntibioticCommercialName.cs        ← قرار 3: جدول ابن
  src/TopLab.Domain/Common/AgeRules.cs                       ← [C12] جديد — تطبيع العمر للطفل

يُعدَّل (Domain):
  src/TopLab.Domain/Results/CultureAntibioticResult.cs       ← + InhibitionZoneMm
  src/TopLab.Domain/Tests/CultureAntibioticAttachment.cs   ← + SensitivityThresholdMm
  src/TopLab.Domain/Tests/Antibiotic.cs                     ← + Symbol, ScientificName

يُضاف (Infrastructure):
  src/TopLab.Infrastructure/Persistence/Configurations/CultureMicroscopyConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Configurations/AntibioticCommercialNameConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddCultureMicroscopyAndZone.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddAntibioticMasterFields.cs

يُعدَّل (Application):
  src/TopLab.Application/Features/CultureResults/Commands/SaveCultureResults/*.cs
  src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs
  src/TopLab.Application/Features/CultureResults/Queries/GetCultureReport/GetCultureReportQueryHandler.cs
  src/TopLab.Application/Features/CultureResults/Common/CultureResultDtos.cs
  src/TopLab.Application/Features/CultureAndAntibiotics/Commands/SaveCultureAntibioticAttachment/*.cs
  src/TopLab.Application/Features/CultureAndAntibiotics/Queries/GetCultureAttachmentView/*.cs

يُعدَّل (Presentation):
  src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml
  src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
  src/TopLab.Presentation/Views/Lab/AntibioticsView.xaml
  src/TopLab.Presentation/Views/Lab/CultureAttachmentView.xaml
  src/TopLab.Infrastructure/Printing/ReportCultureSection.cs   ← من WP-01، يُوسَّع هنا

يُضاف (Tests):
  tests/TopLab.Domain.Tests/Results/AgeRulesTests.cs
  tests/TopLab.Application.Tests/Features/CultureResults/MicroscopyPersistenceTests.cs
  tests/TopLab.Application.Tests/Features/CultureResults/SensitivityZoneTests.cs
  tests/TopLab.Infrastructure.Tests/Printing/CultureReportSectionTests.cs
```

### المكونات

**Domain**

`CultureMicroscopy` — واحد إلى واحد مع `CultureResult`:
```csharp
public sealed class CultureMicroscopy : AuditableEntity
{
    public CultureResultId CultureResultId { get; }
    public string? PusCells { get; private set; }          // "+/++/+++"
    public string? RedBloodCells { get; private set; }
    public string? EpithelialCells { get; private set; }
    public string? Crystals { get; private set; }
    public string? Fungi { get; private set; }
    public string? OthersOne { get; private set; }
    public string? OthersTwo { get; private set; }
    public string? OthersThree { get; private set; }
    public bool IsDirect { get; private set; }               // «مباشر؟»
    // Create/Update بقواعد طول 20 حرفاً لكل حقل
}
```
> قيمDEPTH لـ«خلايا صديدية» عادة `+ / ++ / +++` أو `أعداد/فولووم حقل`. اختيار **نص حر قصير** بدل enum يسمح بأي اصطلاح مخبري. **قرار المالك مطلوب** إن كانChez مخبره enumerates.

`CultureAntibioticResult.InhibitionZoneMm` — `decimal?` (0.1 mm = 1mm).
`CultureAntibioticAttachment.SensitivityThresholdMm` — `decimal?` (مثل `> 18 mm`).
`Antibiotic.Symbol` (string?, ≤10) · `Antibiotic.ScientificName` (string?, ≤150) · `AntibioticCommercialName` (جدول ابن — القرار 3).
`AgeRules` في `Domain/Common`:
```csharp
public static class AgeRules
{
    public static bool IsUnderTwelve(int ageValue, AgeUnit ageUnit) => ageUnit switch
    {
        AgeUnit.Year  => ageValue < 12,
        AgeUnit.Month => ageValue < 144,   // 12 شهراً
        AgeUnit.Day   => ageValue < 365 * 5,
        _             => false
    };
}
```
> **مهم:** هذا **لا** يخالف قاعدة «بلا تحويل بين الوحدات» في `ReferenceRange.Matches` (`:97-110`). تلك قاعدة **مطابقة النطاق المرجعي**؛ هنا قاعدة **تصنيف عمري**. المعياران مختلفان. وثّق هذا التعليل في XML doc.

**Application**
- `SaveCultureResultsCommand` + `SaveCultureMicroscopy` في نفس `SaveChangesAsync` (ذرّية واحدة).
- `SensitivityCategory` من MM: عند `InhibitionZoneMm != null` والعتبة على الإرفاق ⇒ اقترح الفئة ولا تفرضها (القرار السريري للطبيب):
  ```csharp
  var suggested = ResolveCategory(zoneMm, attachment.ThresholdMm);  // للعرض فقط
  ```
- `GetCultureReportQueryHandler` — رايات العرض الثلاثة + تصحيح `SingleOrDefault(s => s.Id == 1)`.
- **كل مكان يستعمل `AgeUnit == Year && AgeValue < 12` استبدله بـ `AgeRules.IsUnderTwelve`.** ابحث أولاً: `grep -rn "AgeUnit == AgeUnit.Year" src/`.
- `SaveCultureResultsCommandValidator` — مدى `InhibitionZoneMm` بين 0 و 100.

**Infrastructure**
- هجرتان. الأولى: `CultureMicroscopy` + `InhibitionZoneMm` + `ThresholdMm`. الثانية: `Antibiotic` حقول + `AntibioticCommercialNames`.
- `ReportCultureSection` (الموجود من WP-01): أضف جدول الحساسية بصف لكل مضاد: `المضاد · الفئة · منطقة التثبيط (مم) · الاسم التجاري`، وكتلة الفحص المجهري تحته. الأعمدة المحكومة بالرايات.

**Presentation**
```xml
<!-- CultureEntryView.xaml — لوحتان -->
<GroupBox Header="الفحص المجهري">
  <Grid>  <!-- 9 صفوف: صديدية · كريات حمراء · خلايا بطانية · بلورات · فطريات · أخرى×3 -->
    <TextBox Text="{Binding Microscopy.PusCells}"/>
    …
    <CheckBox Content="مباشر؟" IsChecked="{Binding Microscopy.IsDirect}"/>
  </Grid>
</GroupBox>

<GroupBox Header="الحساسية">
  <DataGrid ItemsSource="{Binding AntibioticRows}">
    <DataGridTextColumn Header="المضاد الحيوي"  Binding="{Binding AntibioticName}" IsReadOnly="True"/>
    <DataGridTemplateColumn Header="الفئة">        <!-- SelectedValue لا SelectedIndex! -->
      <ComboBox ItemsSource="{Binding SensitivityOptions}" DisplayMemberPath="Display"
                SelectedValuePath="Value" SelectedValue="{Binding SensitivityCategory}"/>
    </DataGridTemplateColumn>
    <DataGridTextColumn Header="منطقة التثبيط (مم)" Binding="{Binding InhibitionZoneMm}"/>
    <DataGridTextColumn Header="الاسم التجاري"  Binding="{Binding CommercialName}" IsReadOnly="True"/>
  </DataGrid>
</GroupBox>
```
- `CultureAttachmentView` — عمود `العتبة (مم)` في شبكة الإرفاق.
- `AntibioticsView` — حقول الاسم العلمي/الرمز + شبكة الأسماء التجارية.
- مفاتيح العرض الثلاثة في شريط أدوات التقرير.
- `OrganismC` في لوحة المعاينة `:140-144`.
- **اربط زر طباعة المزرعة بمنسّق WP-06** — لا مسار وسم مباشر.

**Tests**
```
AgeRules_Year11_IsChild
AgeRules_Year13_IsNotChild
AgeRules_Month11_IsChild              ← حرج
AgeRules_Month13_IsNotChild
AgeRules_Day300_IsChild
AgeRules_Day2000_IsNotChild
AgeRules_UnknownUnit_IsNotChild
GetCultureEntryGrid_Infant11Months_ExcludesAdultOnlyAntibiotics
GetCultureEntryGrid_Child12_IncludesChildrenAntibiotics
SaveCulture_PersistsMicroscopyRows
SaveCulture_PersistsIsDirect
SaveCulture_PersistsInhibitionZoneMm
SaveCulture_OutOfRangeZone_IsRejected
SaveCulture_MicroscopyAndResults_AreAtomic
Report_PrintsSensitivityTableRows
Report_PrintsMicroscopyBlock
Report_ShowSensitivityFalse_HidesColumn
Report_ShowCommercialNameFalse_HidesColumn
CultureReport_UsesSystemSettingsIdOne
CultureEntry_PrintCommand_UsesResultPrintCoordinator
```

### خطوات التنفيذ

1. أنشئ `AgeRules` + اختباراتها **أولاً** — أرخص إصلاح عالي القيمة. استبدل كل موقع يستعمل الشرط القديم.
2. أنشئ `CultureMicroscopy` + الإعداد + الهجرة الأولى.
3. أضف `InhibitionZoneMm` و`SensitivityThresholdMm` + المدقّق.
4. أضف حقول `Antibiotic` + جدول الأسماء التجارية + الهجرة الثانية.
5. وسّع `SaveCultureResultsCommand` بالتزامنية (نفس `SaveChangesAsync`).
6. أعد بناء شبكة الحساسية بـ `SelectedValue` **لا** `SelectedIndex` — **نمط مُثبَّت في WP-03**. لا تعِد الخطأ.
7. أضف لوحة الفحص المجهري.
8. وسّع `CultureReportDto` بالرايات الثلاث + الإصلاح `SingleOrDefault`.
9. ارسم الجدول والكتلة في `ReportCultureSection`.
10. أضف حقول AntibioticsView + شبكة الأسماء التجارية.
11. اربط زر الطباعة بمنسّق WP-06.
12. أضف `OrganismC` للمعاينة.
13. اكتب الاختبارات العشرين.

### الاعتماديات
**WP-01** · **WP-03** · **WP-06**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | هجرتان على جداول نتائج ح existentة | العمودان nullable ⇒ لا كسر |
| بيانات | `SaveCultureResultsCommandHandler.cs:5` يحذف ويعيد إنشاء كل صفوف الحساسية في كل حفظ (معرّفات جديدة كل مرة) | قبل WP-14: جزّئ إلى `Update` لـ Existing و`Add` للجديد. يُنفَّذ ضمن WP-14 كخطوة 5b |
| **تقنية** | `AgeRules` يبدو أنه يخالف قاعدة «بلا تحويل» |-baseالقاعدة موثّقة في XML doc؛ الاختبارات الخمسة تثبت السلوك |
| تجربة مستند | لوحتان بدل واحدة | `GroupBox` بتبويبات، والقسم الدقيق مطوي افتراضياً |
| سريرية | اقتراح الفئة من MM قد يربك | **اقتراح فقط**، لا فرض |

### معايير القبول
- [ ] الفحص المجهري يُحفظ ويُطبع
- [ ] منطقة التثبيط بالملم تظهر في التقرير
- [ ] الرضيع 11 شهراً يُعامل كطفل في `GetCultureEntryGridQueryHandler`
- [ ] مفاتيح العرض الثلاثة تتحكم بالأعمدة
- [ ] زر طباعة المزرعة ينتج PDF عبر المنسّق (لا وسم فقط)
- [ ] تنصيب/إعادة التنصيب الحساسية لم يعد يحذف الصفوف الموجودة
- [ ] الاختبارات العشرون خضراء

### التراجع
جدولان جديدان + أعمدة nullable. الجدولان يُتركان بعد التراجع. آمن. **الاستثناء:** إذا نفّذت خطوة 5b (التحديث بدل الحذف) فاختبرها على بيانات حقيقية أولاً.

---

### WP-15 — مفهوم الفرع / تعدّد الفروع

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | مفهوم واحد غائب بأربعة مآخذ. توفّره يفتح أربعة متطلبات دفعة واحدة، وهو مورّد لثلاث حزم تالية. |
| **المرجع** | R-26, R-62, R-75, R-79 · D-13 |
| **الحجم** | **L** (4–5 أيام) |
| **الهجرات** | **migration جديدة** — `AddBranchNumber` + seed |
| **الاعتماديات** | لا شيء. **مطلوب قبل:** WP-16, WP-19, WP-24. |

### الحالة الحالية — MISSING FEATURE
`grep "BranchNumber\|BranchNo" src/TopLab.Domain/` ⇒ **صفر**. الضربة الوحيدة في المستودع كله تعليق في `TestPriceResolver.cs:16`. لا فرع على `User` ولا `SystemSettings` ولا معامل استعلام. `ShellViewModel` بلا مُنتقي فرع.

المرجع يذكر الفرع في أربعة مواضع: بحث المرضى (Learn p.42, p.45) · إسناد المستخدم (p.151) · فترة الدرج والحسابات (p.178-179) · إعدادات النظام (p.191).

### الحالة المستهدفة
رقم فرع على الإعدادات النظامية، ورقم فرع افتراضي لكل مستخدم، وفلتر فرع اختياري في بحث المرضى والحسابات والإحصائيات، ومُنتقي فرع في الدرج.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Settings/SystemSettings.cs
  src/TopLab.Domain/Users/User.cs
  src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateSystemSettings/*.cs
  src/TopLab.Application/Features/UsersAndPermissions/Commands/{Create,Update}User/*.cs
  src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/*.cs
  src/TopLab.Application/Features/InventoryAndAccounting/Queries/GetCashDrawerInventory/*.cs
  src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/*.cs
  src/TopLab.Application/Features/Statistics/Queries/GetTestCountStatistics/*.cs
  src/TopLab.Presentation/Views/Settings/SystemSettingsView.xaml
  src/TopLab.Presentation/Views/Users/UserManagementView.xaml
  src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs

يُضاف:
  src/TopLab.Domain/Common/BranchScope.cs
  src/TopLab.Application/Common/Interfaces/IBranchFilter.cs   ← امتداد الاستعلام الموحّد
  src/TopLab.Infrastructure/Persistence/Configurations/BranchScopeConfiguration.cs  (إن لزم جدول فروع)
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddBranchNumber.cs
  tests/TopLab.Application.Tests/Common/BranchFilterTests.cs
```

### المكونات

**Domain**
- `SystemSettings.BranchNumber` (int, default **1**).
- `User.BranchNumber` (int? — `null` = يرث فرع النظام).
- `BranchScope(int? Requested, int SystemDefault, int UserAssigned)` في `Domain/Common`:
  ```csharp
  public int Resolve() => Requested ?? UserAssigned ?? SystemDefault;
  public bool IsAllBranches => Requested is null && UserAssigned is null;
  ```

**Application**
- `IBranchFilter` — امتداد وحيد لتفادي تكرار الشروط في أربعة استعلامات:
  ```csharp
  public static IQueryable<Patient> WhereBranch(
      this IQueryable<Patient> q, BranchScope scope)
      => scope.IsAllBranches ? q : q.Where(p => p.BranchNumber == scope.Resolve());
  ```
  > **هذا يستلزم `Patient.BranchNumber`.** إن اخترت قرار 4 (أ) — فرع على الإعدادات والمستخدم فقط — فلا حاجة لحقل على المريض، ويكفي أن:
  > 1. `ShellViewModel` يحمّل الفرع الحالي في `CurrentBranchScope`.
  > 2. الحسابات والإحصائيات تمرّره.
  > 3. بحث المرضى — إن لم يكن للمريض فرع — يعرض **تنبيهاً**: «البحث بفرع غير متاح دون مبيعات موزّعة على الفروع». **هذا سلوك صادق** أفضل من واجهة معطوبة.
  > **قرار 4 في §7** يحدد هذا. **لا** تضف `Patient.BranchNumber` قبل القرار.
- `Create/UpdateUserCommand.BranchNumber`.
- `ShellViewModel` — `CurrentBranchScope` (من `SystemSettings.BranchNumber` و`User.BranchNumber`)، و`ChangeBranchCommand` للمستخدمين المُفوَّضين.

**Infrastructure** — هجرة `AddBranchNumber` (عمودان) + بذر `SystemSettings.BranchNumber = 1`.

**Presentation**
- `SystemSettingsView` — `NumericUpBox` «رقم الفرع» (min 1).
- `UserManagementView` — `NumericUpBox` «الفرع» (اختياري، مع تلميح «فارغ = فرع النظام»).
- `AccountsHubView` و`StatisticsView` و`PatientSearchView` — `ComboBox` «الفرع»: «كل الفروع» + قائمة الفروع المتاحة. (الـ Combo في Accounts يضاف في WP-09 كعنصر placeholder — **لا** تضف عنصراً معطّلاً، انتظر WP-15.)

**Tests**
```
SystemSettings_DefaultBranchNumberIsOne
SystemSettings_SaveAndReloadBranchNumber
BranchScope_NullRequest_UsesUserAssigned
BranchScope_NullUserAssigned_UsesSystemDefault
BranchScope_ExplicitRequest_Wins
BranchScope_IsAllBranches_WhenNothingAssigned
SearchPatients_FiltersByBranch
SearchPatients_AllBranches_ReturnsEverything
Accounting_FiltersByBranch
Statistics_FiltersByBranch
User_BranchNull_InheritsSystemBranch
User_SaveAndReloadBranchNumber
```

### خطوات التنفيذ

1. **اطلب قرار 4** (نموذج الفرع) قبل كتابة سطر واحد.
2. نفّذ `BranchScope` + اختباراتها الخمسة أولاً.
3. أضف `SystemSettings.BranchNumber` + `User.BranchNumber` + **الهجرة** + البذر.
4. أنشئ `WhereBranch` (أو `IQueryable` equivalent حسب القرار).
5. مرّر `BranchScope` في أربعة استعلامات: بحث · الدرج · إحصاء المرضى · إحصاء الاختبارات.
6. أضف `CurrentBranchScope` + أمر تبديل الفرع في `ShellViewModel`.
7. اربط الأرقام في `SystemSettingsView` و`UserManagementView`.
8. أضف `ComboBox` الفرع في `AccountsHubView` و`StatisticsView` و`PatientSearchView`.
9. **استبدل** العنصر placeholder إن كان قد أُضيف في WP-09.
10. اكتب الاختبارات الاثني عشر.

### الاعتماديات
لا شيء.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **تصميم** | نموذج الفرع سؤال مفتوح | **القرار 4**. الافتراضي (أ) أقل خطراً |
| تقنية | اتساق الفلتر في أربعة استعلامات | `WhereBranch` وحيدة موضعاً واحداً |
| تجربة مستخدم | «الفرع» في أربعة شاشات | توحيد التسمية + «كل الفروع» افتراضي |
| بيانات | عمودان على جدولين | nullable ⇒ آمن |

### معايير القبول
- [ ] رقم الفرع يُحفظ على الإعدادات والمستخدم ويُقرأ
- [ ] الفلتر يعمل في بحث المرضى والحسابات والإحصائيتين
- [ ] «كل الفروع» هو الافتراضي ويعيد كل شيء
- [ ] `BranchScope_*` الخمسة خضراء
- [ ] (حسب القرار 4) سلوك البحث بالفرع **موثّق ومُعلن للمستخدم**

### التراجع
عمودان nullable. آمن.

---

### WP-16 — فلاتر البحث الـ13 عن المرضى

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | نافذة بحث واحدة نص حر بدل شبكة أزرار بحث المواصفة (13 وضعاً). |
| **المرجع** | R-25, R-26 · D-26 |
| **الحجم** | M (3–4 أيام) |
| **الهجرات** | **none** (يعتمد على `Patient.CardNumber` من WP-18) |
| **الاعتماديات** | **WP-15** (الفرع) · **WP-18** (الكارنيه) |

### الحالة الحالية — PARTIAL
`PatientSearchView.xaml:8-16` مربع نص حر + جلب Lab ID + ترقيم صفحات. `SearchPatientsGlobalQueryHandler:25` يوجّه النص إلى الاسم/الرقم القومي/Lab ID/الهاتف فقط. لا مرشّحات حالة، ولا بحث بالاختبار، ولا بالتاريخ، ولا بالطبيب المعالج، ولا بالجنس/العمر/الكارنيه.

المواصفة (`RLS_Learn` pp.43, 45): اسم مطابق تماماً · اسم بعنصر بديل · اسم الطبيب المعالج · الجنس · العمر · الهاتف · رقم الكارنيه · حسب تحليل في فترة · بتاريخ الطلب · + أربعة مرشّحات حالة (لم تُدخل نتائجها / لم تُراجع / لم تُطبع / لم تُسلَّم).

### الحالة المستهدفة
شبكة بحث مطابقة للمواصفة بكل أوضاعها الثلاثة عشر.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs
  src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQueryHandler.cs
  src/TopLab.Application/Features/PatientSearch/Queries/GetPatientVisitHistory/*.cs   (إن لزم)
  src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml
  src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs

يُضاف:
  src/TopLab.Application/Features/PatientSearch/Common/PatientSearchMode.cs   ← enum بـ 13 قيمة
  src/TopLab.Application/Features/PatientSearch/Common/PendingStateFilter.cs  ← enum بـ 4 قيم
  tests/TopLab.Application.Tests/Features/PatientSearch/PatientSearchModesTests.cs
```

### المكونات

**Domain** — لا تغيير في هذه الحزمة. `Patient.CardNumber` من WP-18.

**Application**
```csharp
public enum PatientSearchMode
{
    ExactName, WildcardName, TreatingDoctorName, Sex, Age, Phone, CardNumber,
    ByTestInPeriod, ByRequestDate, Branch
}
public enum PendingStateFilter { None, ResultsNotEntered, NotReviewed, NotPrinted, NotDelivered }
```
```csharp
public sealed record SearchPatientsGlobalQuery(
    PatientSearchMode Mode,
    string? Text = null,
    Sex? SexFilter = null, int? AgeValue = null, AgeUnit? AgeUnit = null,
    int? TestId = null, DateOnly? FromDate = null, DateOnly? ToDate = null,
    PendingStateFilter Pending = PendingStateFilter.None,
    int? BranchNumber = null,
    int PageNumber = 1, int PageSize = 50) : IAuthorizedRequest { … }
```
- **`Where` ديناميكي** — لا 13 فرعاً مكرراً:
  ```csharp
  var q = _db.Set<Patient>().Where(p => !p.IsDeleted);
  q = query.Mode switch
  {
      PatientSearchMode.ExactName   => q.Where(p => p.Name == query.Text!.Trim()),
      PatientSearchMode.WildcardName=> q.Where(p => EF.Functions.Like(p.Name, $"%{query.Text!.Trim()}%")),
      PatientSearchMode.Sex         => q.Where(p => p.Sex == query.SexFilter!.Value),
      PatientSearchMode.Age         => q.Where(p => p.AgeValue == query.AgeValue
                                                 && p.AgeUnit == query.AgeUnit),
      PatientSearchMode.Phone       => q.Where(p => p.Phone == query.Text!.Trim()),
      PatientSearchMode.CardNumber  => q.Where(p => p.CardNumber == query.Text!.Trim()),
      PatientSearchMode.Branch      => WhereBranch(q, scope),
      _ => q
  };
  ```
- **مرشّح الحالة على مستوى المريض**: مجموّع على مستوى `PatientTest`:
  ```csharp
  PatientSearchMode مرشّح NotPrinted:
    q.Where(p => p.Tests.Any(t => !t.IsPrinted));
  PatientSearchMode مرشّح NotReviewed:
    q.Where(p => p.Tests.Any(t => !t.IsReviewed));
  PatientSearchMode مرشّح NotDelivered:
    q.Where(p => p.Tests.Any(t => !t.IsDelivered));
  PatientSearchMode مرشّح ResultsNotEntered:
    q.Where(p => p.Tests.Any(t => t.ResultValue == null || t.ResultValue == ""));
  ```
- `ByTestInPeriod`: `q.Where(p => p.Tests.Any(t => t.TestId == query.TestId
                                    && t.RegistrationDateUtc >= from && <= to))`
- `ByRequestDate`: على `p.RegistrationDateUtc`.
- **انقل كل الشرط إلى SQL.** ممنوع `ToList()` ثم LINQ-to-Objects.
- الترقيم: `Count()` + `Skip/Take` + `AsNoTracking`.

**Infrastructure** — لا مخطط.

**Presentation**
- `PatientSearchView.xaml` — **شبكة** (كما في المرجع) لا سطر أدوات:
  ```xml
  <Grid>
    <Grid.ColumnDefinitions>…</Grid.ColumnDefinitions>
    <GroupBox Header="البحث بالاسم" Grid.Row="0">
      <StackPanel Orientation="Horizontal">
        <RadioButton Content="مطابق"      IsChecked="{Binding IsExactNameMode}"/>
        <RadioButton Content="عنصر بديل"  IsChecked="{Binding IsWildcardNameMode}"/>
        <TextBox   Text="{Binding NameText}"/>
      </StackPanel>
    </GroupBox>
    <GroupBox Header="بحث بتفاصيل" Grid.Row="1">  <!-- الجنس · العمر+وحدة · الهاتف · الكارنيه -->
    <GroupBox Header="بحث بالتاريخ" Grid.Row="2">  <!-- من · إلى -->
    <GroupBox Header="بحث بالاختبار" Grid.Row="3"> <!-- ComboBox الاختبار + الفترة -->
    <GroupBox Header="بحث بالحالة" Grid.Row="4">
      <StackPanel Orientation="Horizontal">
        <RadioButton Content="الكل"                IsChecked="{Binding IsStateAll}"/>
        <RadioButton Content="لم تُدخل نتائجها"   IsChecked="{Binding IsNotEntered}"/>
        <RadioButton Content="لم تُراجع"          IsChecked="{Binding IsNotReviewed}"/>
        <RadioButton Content="لم تُطبع"           IsChecked="{Binding IsNotPrinted}"/>
        <RadioButton Content="لم تُسلَّم"         IsChecked="{Binding IsNotDelivered}"/>
      </StackPanel>
    </GroupBox>
    <GroupBox Header="الفرع" Grid.Row="5">  ← من WP-15
    <Button Content="بحث" Command="{Binding SearchCommand}" Grid.Row="6"/>
  </Grid>
  ```
- `PatientSearchViewModel` — `ObservableCollection<SearchResultRow>` + `SearchCommand` + `IsBusy`.

**Tests**
```
Search_ExactName_MatchesExactlyNotPartial        [حاسم]
Search_WildcardName_MatchesPartial
Search_ByTreatingDoctorName_ReturnsHisPatients
Search_BySex_Filters
Search_ByAge_MatchesValueAndUnit                  ← لا تحويل بين الوحدات
Search_ByPhone_Exact
Search_ByCardNumber_Exact
Search_ByTestInPeriod_FiltersBoth
Search_ByRequestDate_FiltersRegistration
Search_Pending_ResultsNotEntered
Search_Pending_NotReviewed
Search_Pending_NotPrinted
Search_Pending_NotDelivered
Search_ByBranch_Filters                           ← من WP-15
Search_ReturnsPagedResult_WithServerTotal
Search_DoesNotMaterializeAllPatients              ← فحص سلوكي
Search_ExcludesSoftDeletedPatients
```

### خطوات التنفيذ

1. أنشئ `PatientSearchMode` و`PendingStateFilter`.
2. وسّع `SearchPatientsGlobalQuery` بمعاملات اختيارية (لا كسر للاستدعاءات القائمة).
3. اكتب `Search_ExactName_MatchesExactlyNotPartial` أولاً — يجب أن يفشل قبل التنفيذ (الوضع الحالي يطابق جزئياً).
4. نفّذ `Where` الديناميكي.
5. نفّذ مرشّحات الحالة الأربعة.
6. نفّذ `ByTestInPeriod` و`ByRequestDate`.
7. انقل الترقيم إلى SQL (`Count` + `Skip/Take`).
8. صمّم الواجهة كشبكة.
9. اربط الفرع بعد WP-15.
10. اربط الكارنيه بعد WP-18.
11. اكتب الاختبارات السبعة عشر.

### الاعتماديات
**WP-15** · **WP-18** (كارنيه).

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تقنية | استعلام مركّب قد يخرج عن ترسيخ EF | اختبار مبكر لـ `ByTestInPeriod` (Contains + DateRange) |
| تجربة مستخدم | 13 خياراً تحتاج تنظيماً | `GroupBox` لكل فئة + شبكة عمودين |
| — | مرشّح الحالة يعيد مرضى بلا نتائج إطلاقاً | مقبول — «لم تُدخل نتائجها» = لا نتائج |

### معايير القبول
- [ ] 17 اختباراً أخضر
- [ ] كل وضع يُعيد مجموعة مختلفة ومتوقعة
- [ ] لا `ToList()` كامل في مسار البحث
- [ ] الأرقام المعروضة (عدد النتائج) من الخادم لا من حجم الصفحة

### التراجع
آمن — query + VM + XAML.

---

### WP-17 — اكتمال كتالوج الاختبارات

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | 6 حقول و5 أعمدة ناقصة. والـ DTO يحمل البيانات أصلاً ولا يعرضها —gap عرضي لا-data. |
| **المرجع** | R-43, R-44 · D-15 |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **migration جديدة** — `CompleteTestCatalogFields` (بلا `HasData` — لا يمسّ `PermissionConfiguration`) |
| **الاعتماديات** | **WP-08** (نوع الباركود والوحدة) |

### الحالة الحالية
`Test.cs:11-40` · `TestCatalogView.xaml:61-64` أربعة أعمدة فقط · `TestCatalogDtos.cs:5-17` يحمل `LabToLabPrice` و`IsSentOut` و`Barcode` بلا ربط · `grep "HistoryName|ArabicName|BarcodeName|ReferenceType|IsMainTest|PrintWithOther|AddWithGroup|SeeReport|PatientQuestion"` ⇒ **صفر** · `SearchTestCatalogQuery` بلا `IAuthorizedRequest` (الاستعلام الوحيد غير المبوَّب) · لا `ExternalEntityId` على `Test` (وجهة الإرسال تُختار لكل عينة وقت الإرسال فقط).

### الحالة المستهدفة
الكتالوج يعرض الأعمدة التسعة المواصفة؛ ومحرر الاختبار يحوي حقوله الثلاثة عشر؛ والبحث بالرقم يعمل؛ وكل قراءة للكتالوج مبوَّبة بصلاحية.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Tests/Test.cs                                    ← + 9 حقول
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/CreateTest/*.cs
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/UpdateTest/*.cs
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/SearchTestCatalog/*.cs
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Common/TestCatalogDtos.cs
  src/TopLab.Infrastructure/Persistence/Configurations/TestConfiguration.cs
  src/TopLab.Presentation/Views/Lab/TestCatalogView.xaml
  src/TopLab.Presentation/Views/Lab/TestEditorWindow.xaml
  src/TopLab.Presentation/ViewModels/Lab/TestEditorViewModel.cs

يُضاف:
  src/TopLab.Domain/Tests/ReferenceType.cs              ← enum
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Common/TestCatalogAccessPolicy.cs   ← [C12] جديد — بوابة صلاحية الكتالوج
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_CompleteTestCatalogFields.cs
  tests/TopLab.Application.Tests/Features/TestCatalogAndReferenceRanges/TestCatalogFieldsTests.cs
```

### المكونات

**Domain**
```csharp
public enum ReferenceType : byte { None = 0, ValueRange = 1, Textual = 2 }
```
`Test` — الحقول التسعة:
| الحقل | النوع | ملاحظة |
|---|---|---|
| `HistoryName` | `string?` | اسم التحليل في تقرير التاريخ |
| `ArabicName` | `string?` | الاسم العربي للطباعة |
| `BarcodeName` | `string?` | الاسم على ملصق الباركود |
| `ReferenceType` | `ReferenceType` | `None` افتراضي |
| `PatientQuestion` | `bool` | «هل يوجد سؤال من المريض؟» |
| `IsMainTest` | `bool` | اختبار رئيسي |
| `PrintWithOther` | `bool` | يُطبع مع غيره |
| `AddWithGroup` | `bool` | يُضاف مع المجموعة |
| `SeeReport` | `bool` | يُعرض التقرير |
| `ExternalEntityId` | `ExternalEntityId?` | جهة الإرسال الافتراضية |
| `BarcodeTypeId` | `Guid?` | من WP-08 |

> **كل الأسماء `string?`** — **لا** تخزّن نصاً مترجماً. الترجمة (مثل «مرتفع/منخفض») في كاتب التقرير لا في التخزين.

**Application**
- التسعة في `CreateTestCommand` / `UpdateTestCommand` (`:7-19`) + `Test.Update` (`:102-128`).
- `SearchTestCatalogQuery` — يضاف `TestId` **و** بوابة الصلاحية:
  ```csharp
  // التوقيع الحالي (لا معامل معرّف إطلاقاً):
  //   record SearchTestCatalogQuery(string? SearchTerm, int? TestGroupId, bool IncludeInactive = false)
  public sealed record SearchTestCatalogQuery(
      string? SearchTerm, int? TestGroupId, int? TestId = null, bool IncludeInactive = false)
      : IAuthorizedRequest(Permissions.ViewTests);
  ```
  - **مطلوب [C17(a)]**: التوقيع الحالي لا يملك أي معرّف، فلا يمكن تنفيذ «بحث برقم
    الاختبار» المنصوص عليه في `RLS_Learn` p.94/97. الرقم هو `Test.Id` لا `TestCode`.
  - **مطلوب أمنياً**: `SearchTestCatalogQuery` هو الاستعلام **الوحيد** في طبقة
    الكتالوج بلا `IAuthorizedRequest`. **إصلاح أمني مستقل عن التغطية** — لا يُؤجَّل.
  - منطق المعالج يفرّع على المعرّف أولاً ثم النص ثم المجموعة:
    ```csharp
    var q = query.TestId is { } id ? baseQ.Where(t => t.Id == id)
                                    : !string.IsNullOrWhiteSpace(query.SearchTerm)
                                        ? baseQ.Where(t => t.TestCode == query.SearchTerm.Trim())
                                        : baseQ;
    ```

**Infrastructure** — هجرة `CompleteTestCatalogFields` + بذر `ReferenceType = None` للاختبارات القائمة.

**Presentation**
- `TestCatalogView.xaml:61-64` — أعمدة إضافية: `الترتيب` · `سعر المختبر للمختبر` (`LabToLabPrice`) · `خارج المعمل؟` (`IsSentOut`) · `الباركود` (`Barcode`).
- `TestEditorWindow.xaml:39-70` — المجموعة الجديدة داخل `TabItem` جديد «اسماء وطرق العرض» حتى لا يتضخّم المحرر.
- **حقل «نوع المرجع» يقود الواجهة**: عند `ValueRange` تظهر شبكة النطاقات المرجعية (موجودة في `:75-133`)؛ عند `Textual` يظهر حقل نصي.

**Tests**
```
CreateTest_WithAllFields_RoundTrips
UpdateTest_ChangesAllFieldsAndPersists
SearchTestCatalog_ByTestId_ReturnsTest
SearchTestCatalog_ByTestCode_ReturnsTest
SearchTestCatalog_RequiresAuthorization           ← إصلاح أمني
UpdateTest_NullNames_AreAllowed
ReferenceType_ValueRange_ShowsRangeEditor
ReferenceType_Textual_HidesRangeEditor
TestCatalog_GridBindsAllNineColumns
TestDisplayName_Resolver_PrefersReportNameThenArabicThenName   ← من WP-01
```

### خطوات التنفيذ

1. أنشئ `ReferenceType` + الحقول التسعة + `BarcodeTypeId` + `ExternalEntityId`.
2. أنشئ `TestCatalogAccessPolicy` وأضف `IAuthorizedRequest` إلى `SearchTestCatalogQuery` — **أSTEP أول، وحده، مع اختباره**: إصلاح أمني مستقل.
3. وسّع `Create/UpdateTestCommand` + المدقّق.
4. أنشئ **الهجرة** + البذر.
5. اربط الأعمدة الخمسة الموجودة في DTO أصلاً — **لا عمل طباعي**.
6. أضف `TestItem` بالحقول الجديدة في المحرر داخل `TabItem`.
7. اجعل `ReferenceType` يقود ظهور شبكة النطاقات.
8. نفّذ `TestDisplayNameResolver` كاملاً (يستهلك `ReportName`/`ArabicName`/`TestName` بالترتيب) — يُغلق كسر الدائرة مع WP-01.
8b. **[C17(b)] زمن turnaround في الباركود.** `Test.CompletionDurationMinutes` مستهلك اليوم في
    `WorkSheetHelpers.cs:66,126` فقط؛ و`BarcodeService.cs:62-64` يطبع القيمة **بلا** أي حدّ turnaround.
    `RLS_Learn` p.95/98 يطلب «وقت الاختبار (أيام)». اجعل حمولة الباركود تحترم
    `CompletionDurationMinutes` عبر `Test.BarcodeTypeId` الجديد — **التنفيذ في WP-23** لأن الباركود هو
    موضعه الطبيعي، وهذه الحزمة توفّر الحقل فقط.
9. اكتب الاختبارات العشرة.

### الاعتماديات
**WP-08**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| بيانات | هجرة على جدول `Tests` (أكبر جدول نظام) | كل الأعمدة nullable؛ البذر `None` |
| تجربة مستند | محرر أطول | `TabItem` جديد |
| **بنيوي** | `ITestDisplayNameResolver` في WP-01 تنفيذيته مبدئية | **هذه هي آلية كسر الدائرة** — WP-01 يقرأ `TestName` فقط، وWP-17 يوسّع التنفيذ. `WP-17 → WP-01`، لا العكس |
| أمني | `SearchTestCatalogQuery` غير مبوّب | نفّذه في خطوة 2 منفصلاً مع اختباره |

### معايير القبول
- [ ] الحقول التسعة تدور كاملة (إنشاء + تعديل)
- [ ] البحث بالرقم (رمز ومعرّف) يعمل
- [ ] الاستعلام بلا صلاحية يُرفض — **اختبار أخضر**
- [ ] الأعمدة التسعة ظاهرة
- [ ] تقرير مطبوع باسم اختبار يُظهر `ReportName` ثم `ArabicName` ثم `TestName` بالترتيب

### التراجع
عمودان nullable على جدول كبير — **آمن**. عمود `BarcodeTypeId` FK اختياري ⇒ لا كسر.

### WP-18 — تكافؤ التسجيل والمحاسبة

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | لوح حالة سريرية كاملة موصوفة بالمُراجع (12 راية) غير مُبذورة إلا واحدة. وشاشة محاسبة تنقصها سيارة الدفع. وتعديل الدفعة غير موجود. |
| **المرجع** | R-02, R-03, R-05, R-10, R-11, R-12, R-13, R-15, R-25 · D-16, D-24 · تقرير الوكيل الفرعي الأول Caps 1, 3 |
| **الحجم** | **L** (5 أيام) |
| **الهجرات** | **migration جديدة ×2** — `AddPatientCardAndImaging` · `SeedClinicalHistoryConditionTypes` |
| **الاعتماديات** | لا شيء. **يغذّي:** WP-16 (كارنيه) · WP-28 (تأكيد التسوية). |

### الحالة الحالية

| الملاحظة | الدليل | التصنيف |
|---|---|---|
| بذرة حالة واحدة فقط | `MedicalConditionTypeConfiguration.cs:17` `HasData(new { Id = 1, Name = "حمل", Category = Pregnancy })` | PROVED DEFECT |
| `Medication` و`Condition` غير قابلتين للوصول | لا `AddMedicalConditionType` في `src/` إطلاقاً | MISSING FEATURE |
| لا رقم كارنيه | `grep -rni "cardnumber\|card_number\|PatientCard"` ⇒ صفر في Domain/Application/Infrastructure · `PatientDetailDto` بلا عضو | MISSING FEATURE |
| لا أشعة/سونار خلال شهر | `grep -rni "ultrasound\|xray\|أشعة\|سونار"` ⇒ **صفر** · `Patient.RecentContrastImaging` مفهوم مختلف بلا تاريخ | MISSING FEATURE |
| الرايات غير مُصفَّفة بالجنس | `PatientEditorView.xaml:217-223` ItemsControl عام | PROVED DEFECT |
| 4 بطاقات لا 3-way | `PatientAccountView.xaml:41-64` Total/Discount/Paid/Balance · الشطر «للمختبر/للمريض» **على شاشة التسليم فقط** (`GetDeliveryAccountQueryHandler.cs:39-40`) | PARTIAL |
| لا تعديل دفعة | `VoidPaymentOperationCommandHandler.cs:226-229` «Settled: no edit command ships» | MISSING BY DESIGN |
| لا `+` للإضافي | `ExtraChargeDialogViewModel.cs:50` `decimal` عادي | PROVED DEFECT |
| لا Enter مزدوج | `grep -rn "KeyBinding\|KeyDown\|PreviewKeyDown" src/TopLab.Presentation/Views` ⇒ **صفر** | PROVED DEFECT |
| التسوية بلا تأكيد في المحرر | `PatientEditorViewModel.cs:1298-1325` · مقابل `PatientAccountViewModel.cs:299` الذي يسأل | PROVED DEFECT |
| التصحيح السالب مستحيل | `CorrectionDialogViewModel.cs:51` يرفض `amount <= 0` بينما `RecordCorrectionCommandHandler.cs:12-15` يوثّق الموجب ائتمان · `PatientAccountCalculator.TotalPaid:31-32` يجمع التصحيحات في المدفوع | PROVED DEFECT |
| التصحيح بلا سبب | `CorrectionDialogViewModel.cs:12-13` «has no reason field (gap documented, not filled)» | PROVED DEFECT |
| تحديث لا يلمس 3 حقول | `UpdatePatientCommandHandler.cs:58` لا يحدّث `LabId`/`TreatingDoctorId`/`ReferralEntityId` | PROVED DEFECT |
| N+1 | `CreatePatientCommandHandler.cs:122-129` `Any()` داخل `foreach` · `:62, :72, :158` ثلاث استعلامات لنفس `ExternalEntity` | PROVED DEFECT |
| Lab ID بلا فهرس فريد | `CreatePatientCommandHandler.cs:53` تعليق صريح «No DB unique index» | POTENTIAL RISK |
| تسوية بلا قفل | `SettleAccountInFullCommandHandler.cs:117-123` قراءة-ثم-كتابة | POTENTIAL RISK |

### الحالة المستهدفة
لوحة تاريخ طبي كامل (6 أدوية + 5 أمراض + أشعة/سونار + حمل للإناث فقط) تُبذر وتُدار؛ ورقم كارنيه فريد على المريض ويُبحث به؛ وشاشة محاسبة بثلاثة أرقام (المدفوع / المتبقي للمختبر / المتبقي للمريض)؛ وتعديل دفعة بنموذج «إبطال + إصدار جديد»؛ وشحن إضافي بصيغة `+`؛ وEnter المزدوج يحسب ويحفظ؛ والتسوية من شاشة التسجيل تسأل قبل التنفيذ؛ والتصحيح السالب (استرداد) ممكن مع سبب إلزامي.

### الملفات المتأثرة

```
يُعدَّل (Domain):
  src/TopLab.Domain/Patients/Patient.cs                    ← + CardNumber, +RecentImaging*

يُضاف (Domain):
  src/TopLab.Domain/Common/PatientImagingWindow.cs          ← enum/قيمة
  src/TopLab.Domain/Billing/CorrectionDialogInput.cs        ← [C12] جديد — يسمح بمبلغ سالب

يُعدَّل (Application):
  src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs
  src/TopLab.Application/Features/PatientRegistration/Commands/UpdatePatient/UpdatePatientCommandHandler.cs
  src/TopLab.Application/Features/PatientBilling/Commands/*/…
  src/TopLab.Application/Features/PatientBilling/Queries/GetPatientAccount/GetPatientAccountQueryHandler.cs
  src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/*.cs   ← + CardNumber
  src/TopLab.Application/Features/PatientRegistration/Queries/GetMedicalConditionTypes/*.cs

يُضاف (Application):
  src/TopLab.Application/Features/PatientRegistration/Commands/{Create,Update,Delete}MedicalConditionType/
  src/TopLab.Application/Features/PatientBilling/Commands/UpdatePaymentOperation/

يُعدَّل (Infrastructure):
  src/TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Configurations/MedicalConditionTypeConfiguration.cs

يُضاف (Infrastructure):
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddPatientCardAndImaging.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_SeedClinicalHistoryConditionTypes.cs

يُعدَّل (Presentation):
  src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml
  src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs
  src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml
  src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/ExtraChargeDialogViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/CorrectionDialogViewModel.cs
  src/TopLab.Presentation/Views/Lab/LabHubView.xaml              ← تبويب أنواع الحالات

يُضاف (Tests):
  tests/TopLab.Application.Tests/Features/PatientRegistration/ClinicalHistorySeedTests.cs
  tests/TopLab.Application.Tests/Features/PatientBilling/PaymentEditTests.cs
  tests/TopLab.Application.Tests/Features/PatientRegistration/PatientUpdateCoverageTests.cs
```

### المكونات

**Domain**
- `Patient.CardNumber` (`string?`, ≤20) — **فهرس فريد جزئي**: `HasIndex(p => p.CardNumber).IsUnique().HasFilter("[CardNumber] IS NOT NULL")`.
- `Patient.RecentImagingType` (enum: `None | XRay | Ultrasound | Both`) + `Patient.RecentImagingDate` (`DateOnly?`).
  > `Patient.RecentContrastImaging` (`:45`) مفهوم مختلف **لا تحذفه** — مفيد سريرياً.
- `CorrectionDialogInput` — اسمح بـ `Amount < 0` (استرداد/Chargeback) مع التحقق `Amount != 0`، و`Reason` إلزامي (≥5 حروف).

**Application**

1. **بذر التاريخ الطبي** — 11 نوعاً موصوفاً، بهجرة بيانات:
   | الفئة | القيم |
   |---|---|
   | `Medication` | سكري · ضغط مرتفع · علاج فيروسي · مضاد حيوي · سيولة الدم · الكبد |
   | `Condition` | أنيميا · ذئبة · فشل كلوي · ضغط مرتفع · التهاب المفاصل |
   | الأشعة | (حقلان منفصلان أعلاه) |
   | `Pregnancy` | «حمل» — **موجود مسبقاً (Id=1)**، لا تُكرّره |
   السجلات التolate النشطة. أوجد أصلاً 11 + حمل = **12 خياراً** كما في المواصفة.

2. **CRUD أنواع الحالات** — `Create/Update/DeleteMedicalConditionTypeCommand` بنفس نسيج `TestComment`. `Delete` ⇒ `Deactivate` (لا حذف فعلي، لتفادي كسر التاريخ).

3. **`CreatePatientCommandHandler`**
   - إزالة N+1: `var types = await _db.Set<MedicalConditionType>().Where(t => ids.Contains(t.Id)).Select(t => t.Id).ToListAsync(ct);`
   - استعلام `ExternalEntity` **مرة واحدة** بدل ثلاث.
   - **فهرس فريد على `LabId`** — أضفه في هجرة `AddPatientCardAndImaging`، والتقط `DbUpdateException` ورسّله برسالة عربية: «رقم المختبر مستخدم لمريض آخر.»

4. **`UpdatePatientCommandHandler:58`** — أضف `LabId` و`TreatingDoctorId` و`ReferralEntityId` + `CardNumber` + حقول الأشعة إلى `Update`.

5. **`UpdatePaymentOperationCommand`** — **حافظ على نموذج «الإبطال + الإصدار الجديد»** (لا تحطّمه — هو أساس التدقيق):
   ```csharp
   // 1.Original.MarkVoided(byUserId, reason)
   // 2.ختم السطر القديم IsVoided = true, VoidedAtUtc = now
   // 3.إنشاء سطر جديد بالقيمة الصحيحة، نفس ExternalEntityId… لا، نفس VisitId
   // 4._db.SaveChangesAsync()  ← معاملة واحدة
   ```
   `Reason` إلزامي. يظهر في كشف الحساب كسطرين (ملغى + بديل) — هذا صحيح.

6. **الشاشة `PatientAccountView`** — أضف البطاقة الثالثة:
   - المدفوع ✓ (موجود)
   - **المتبقي للمختبر** — من `PatientBillingReader` (مُوحَّد مع `GetDeliveryAccountQueryHandler.cs:39-40` لتجنّب تكرار المنطق)
   - **المتبقي للمريض** (إن كان تجاوز الدفع) — من نفس المصدر

7. **`+` في الشحن الإضافي** — `ExtraChargeDialogViewModel.cs:50`:
   ```csharp
   private static decimal ParseAmount(string input)
   {
       var t = input.Trim();
       if (t.StartsWith("+")) t = t[1..];
       if (!decimal.TryParse(t, out var v)) throw new FormatException();
       return v;
   }
   ```
   واعرض Hint: «اكتب +٥٠ لإضافة 50 على الإجمالي».

8. **Enter المزدوج** — `InputBindings` على مستوى النافذة:
   ```xml
   <Window.InputBindings>
     <KeyBinding Key="Enter" Command="{Binding ComputeAndSaveCommand}"/>
   </Window.InputBindings>
   ```
   - على `PatientEditorView`: `ComputeAndSaveCommand` جديد = حساب الإجمالي ثم `SaveNewAsync` (أو `UpdatePatient` في وضع التعديل).
   - على `PatientAccountView`: يربط `SettleCommand`.

9. **تأكيد التسوية في المحرر** — `PatientEditorViewModel.cs:1298-1325`: أضف `if (!await _dialogs.ShowConfirmationAsync("تسوية الحساب كاملاً؟")) return;`

10. **`CardNumber` في البحث** — `SearchPatientsGlobalQuery.CardNumber`.

**Presentation**
- `PatientEditorView.xaml`:
  - حقل «رقم الكارنيه» بجوار `الرقم القومي`.
  - `GroupBox` «التاريخ الطبي» — نفس `ItemsControl` القائم لكن:
    - `ItemsSource` مجمّع بـ `ItemsControlStyle` يعرض `Category` كعنوان.
    - `Visibility` مرتبط بـ `IsApplicableToSex` (حمل للإناث فقط).
  - `GroupBox` «الأشعة»: ComboBox النوع + DatePicker التاريخ + «خلال شهر؟» (محسوب).
- تبويب «أنواع الحالات الطبية» في `LabHubView`.

**Tests**
```
ClinicalHistorySeed_SeedsElevenTypesPlusPregnancy
ClinicalHistorySeed_IsIdempotent
CreateMedicalConditionType_Persists
DeleteMedicalConditionType_DeactivatesNotRemoves
PatientEditor_FiltersPregnancyForFemalesOnly
PatientEditor_ShowsTwelveHistoryOptions
CreatePatient_WithCardNumber_Persists
CreatePatient_DuplicateCardNumber_IsRejected
CreatePatient_ConcurrentDuplicateLabId_IsRejected       ← فهرس فريد
CreatePatient_MedicalConditionTypes_LoadsInSingleQuery  ← إزالة N+1
CreatePatient_ExternalEntity_LoadsOnce
UpdatePatient_PersistsLabIdAndTreatingDoctorAndReferral
UpdatePatient_PersistsCardNumberAndImaging
UpdatePayment_PersistsNewAmountWithReason
UpdatePayment_KeepsOriginalAsVoided
UpdatePayment_OriginalAmountIsUnchanged
UpdatePayment_RequiresReason
Correction_NegativeAmount_IsAccepted                   ← استرداد
Correction_ZeroAmount_IsRejected
Correction_RequiresReason
ExtraCharge_ParsesPlusPrefix
ExtraCharge_WithoutPlus_StillWorks
Settle_FromEditor_RequiresConfirmation
AccountView_ShowsPaidAndRemainingToLabAndRemainingToPatient
DoubleEnter_ComputesThenSaves                         ← Enter المزدوج
```

### خطوات التنفيذ

1. **الهجرة الأولى** `AddPatientCardAndImaging`: `CardNumber` + `RecentImagingType` + `RecentImagingDate` + فهرس فريد جزئي على `CardNumber` + **فهرس فريد على `Patient.LabId`**.
2. **الهجرة الثانية** `SeedClinicalHistoryConditionTypes`: البذر الـ11.
3. أضف `CardNumber` وأ rame Imaging للنموذج + الإعداد.
4. أضف CRUD أنواع الحالات + تبويب المعمل.
5. صمّم `GroupBox` التاريخ الطبي مع تصفية الجنس.
6. أضف `CardNumber` و`TreatingDoctorId` و`ReferralEntityId` و`RecentImaging*` إلى `UpdatePatientCommandHandler`.
7. أنشئ `UpdatePaymentOperationCommand` (إبطال + إصدار).
8. اسمح بالتصحيح السالب واطلب السبب.
9. أضف البطاقة الثالثة لـ `PatientAccountView` وشارك المنطق مع `GetDeliveryAccountQueryHandler`.
10. نفّذ `+` في `ExtraChargeDialogViewModel`.
11. أضف `InputBindings` لـ Enter على النافذتين.
12. أضف تأكيد التسوية في `PatientEditorViewModel`.
13. حسّن `CreatePatientCommandHandler`: N+1 + استعلام `ExternalEntity` + التقاط `DbUpdateException`.
14. أضف `CardNumber` إلى `SearchPatientsGlobalQuery`.
15. اكتب الاختبارات الأربعة والعشرين.

### الاعتماديات
لا شيء.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | فهرس فريد على `Patient.LabId` قد يفشل إن كانت البيانات القائمة فيها تكرار | **قبل إنشاء الفهرس** شغّل تقرير تكرار. إن وُجد ⇒ `WHERE` على النتيجة أو نقص الفهرس. **هذا ليس اختيارياً** |
| بيانات | بذر 12 نوعاً على جدول مرجعي | هجرة بيانات idempotent (Upsert لا Insert) |
| تجربة مستخدم |Enter المزدوج قد يحفظ بالخطأ | اجعل الحقل الرقمي للتركيز يلتقط Enter أولاً — أو اقبل المخاطرة — هذا سلوك المواصفة |
| تقنية | نموذج «إبطال + إصدار» قد ينتج ازدواجاً في المجاميع | اختبار `OriginalAmountIsUnchanged` إلزامي |
| — | لوحة تاريخCHI أطول | `GroupBox` قابل للطي |

### معايير القبول
- [ ] لوحة التاريخ الطبي تعرض ≥12 خياراً مصنّفة بالعربية
- [ ] «حمل» يظهر للإناث فقط
- [ ] الكارنيه يُحفظ ويُفهرس ويُبحث به؛ والمكرر مرفوض
- [ ] Lab ID المكرر مرفوض على مستوى القاعدة (لا التطبيق فقط)
- [ ] Enter المزدوج يحسب ويحفظ
- [ ] التسوية من المحرر تطلب تأكيداً
- [ ] التصحيح السالب يعمل مع سبب إلزامي
- [ ] شاشة الحساب تعرض الأرقام الثلاثة
- [ ] الاختبارات الـ24 خضراء

### التراجع
عمودان nullable + بذر قابل للعكس. **الاستثناء الوحيد:** فهرس `LabId` الفريد — التراجع ممكن (`DROP INDEX`) لكن إن كان قد رُفض تسجيلات بسببه فستحتاج تصفية يدوية. **افحص التكرار قبل التطبيق.**

---

### WP-19 — الإحصائيات: تجميعات + كلمة المرور الثانية + تقرير المتابعة

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | شاشات تجارية حسّاسة بلا بوابة كلمة المرور الثانية (إصلاح بسطر واحد)، وثلاثة تجميعات ناقصة، وتقرير متابعة مفقود كلياً. |
| **المرجع** | R-68, R-69, R-71, R-72, R-73 · D-05, D-24, D-25 |
| **الحجم** | M (3–4 أيام) |
| **الهجرات** | **none** |
| **الاعتماديات** | **WP-15** (الفرع) · **WP-01** (كاتب تقرير المتابعة) |

### الحالة الحالية

| الملاحظة | الدليل | التصنيف |
|---|---|---|
| **لا كلمة المرور الثانية** | `ShellViewModel.cs:268-276` ينتقل إلى `StatisticsViewModel` **بلا** `ShowSecondaryPasswordDialogAsync()` — مقابل `:234` (المستخدمون) و`:256` (الحسابات) | **PROVED DEFECT** |
| بلا تجميع باليوم | `GetPatientCountStatisticsQuery.cs:8-14` | PROVED DEFECT |
| بلا جهة بعينها | نفس الملف | PROVED DEFECT |
| بلا تجميع بجهة الإحالة (اختبارات) | `GetTestCountStatisticsQuery.cs:26-33` | PROVED DEFECT |
| لا تقرير متابعة | `RL_Show` p.25 · لا معامل min/max في أي استعلام | MISSING FEATURE |
| تجميع في الذاكرة | `GetPatientCountStatisticsQueryHandler.cs:39` `ToList()` على كل المرضى ثم LINQ-to-Objects | POTENTIAL RISK |
| حد معاملات SQL | `GetSentOutStatisticsQueryHandler.cs:49-52` `IN` بقائمة معرّفات —-menIUZZ عند 2100 | POTENTIAL RISK |

### الحالة المستهدفة
الإحصائيات تطلب كلمة المرور الثانية؛ وتُجمَّع إحصاءات المرضى بالشهر/اليوم/الجنس/نوع الحساب/جهة الإحالة/جهة بعينها؛ وإحصاءات الاختبارات بالاختبار/المجموعة/جهة الإحالة؛ ويوجد تقرير متابعة يعرض النتائج الموجبة ضمن مدى رقمي بأعمدته الموصوفة؛ والتجميع كله في SQL.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs              ← :268-276
  src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/*.cs
  src/TopLab.Application/Features/Statistics/Queries/GetTestCountStatistics/*.cs
  src/TopLab.Application/Features/Statistics/Queries/GetSentOutStatistics/*.cs
  src/TopLab.Application/Features/Statistics/Common/StatisticsDtos.cs
  src/TopLab.Presentation/Views/Statistics/StatisticsView.xaml
  src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs

يُضاف:
  src/TopLab.Application/Features/Statistics/Queries/GetResultMonitoring/{GetResultMonitoringQuery.cs,
                                                                              GetResultMonitoringQueryValidator.cs,
                                                                              GetResultMonitoringQueryHandler.cs}
  src/TopLab.Application/Features/Statistics/Common/PatientStatisticsGrouping.cs   ← enum
  src/TopLab.Infrastructure/Printing/ResultMonitoringPdfWriter.cs
  src/TopLab.Application/Features/Statistics/Commands/PrintResultMonitoringReport/
  tests/TopLab.Presentation.Tests/Navigation/StatisticsSecondPasswordTests.cs
  tests/TopLab.Application.Tests/Features/Statistics/StatisticsGroupingTests.cs
  tests/TopLab.Application.Tests/Features/Statistics/ResultMonitoringTests.cs
```

### المكونات

**Domain** — لا تغيير.

**Application**

**1. البوابة (سطر واحد، نفّذه أولاً):**
```csharp
// ShellViewModel.cs:268-276
private async Task NavigateToStatisticsAsync()
{
    if (!await ShowSecondaryPasswordDialogAsync())   // نفس النمط تماماً كـ :234 و :256
        return;
    _navigation.NavigateTo<StatisticsViewModel>();
}
```

**2. التجميعات:**
```csharp
public enum PatientStatisticsGrouping
{ ByMonth, ByDayOfMonth, BySex, ByAccountType, ByReferralEntity, BySpecificReferralEntity }
```
- `GetPatientCountStatisticsQuery(FromUtc, ToUtc, Grouping, int? ReferralEntityId, int? BranchNumber)`.
- التجميع في **SQL**:
  ```csharp
  var grouped = await _db.Set<Patient>()
      .Where(p => !p.IsDeleted && range(p))
      .GroupBy(p => grouping == ByMonth   ? p.RegistrationDateUtc.Month
                  : grouping == ByDayOfMonth ? p.RegistrationDateUtc.Day
                  : grouping == BySex      ? (int?)p.Sex
                  : …)
      .Select(g => new { Key = g.Key, Count = g.Count() })
      .ToListAsync(ct);
  ```
- `GetTestCountStatisticsQuery(FromUtc, ToUtc, Grouping ∈ {ByTest, ByTestGroup, ByReferralEntity}, int? ReferralEntityId, int? BranchNumber)`.

**3. تقرير المتابعة:**
```csharp
public sealed record GetResultMonitoringQuery(
    int TestId, DateOnly From, DateOnly To,
    decimal? MinValue, decimal? MaxValue, bool PositivesOnly = true)
    : IAuthorizedRequest(Permissions.ViewStatistics);
```
- handler: `PatientTest` حيث `TestId` و`RegistrationDateUtc` في المدى و`ResultValue` ضمن [Min, Max].
- **التحويل من نص إلى رقم**: `ResultValue` نصّي. استخدم `decimal.TryParse` داخل `Where` غير ممكن ⇒ **حمّل النتائج الرقمية عبر `EF.Functions`** أو استعمل `AsEnumerable()` **للنطاق المحدد فقط**. الأحوّل:
  ```csharp
  // 1) استخرج المرشّحين بالنص (DateRange + TestId) — Hern
  // 2) صفِّ them بـ TryParse في الذاكرة  ← مقبول: النطاق محدود
  // 3) إن تجاوز العدد 5000 ⇒ أضف تنبيهاً للمستخدم
  ```
  **هذا استثناء مبرَّر**، ووثّقه في XML doc.Better: أضف عمود `ResultValueNumeric` nullable على `PatientTest` يُملأ عند الإدخال (هجرة صغيرة، WP-20 في نفس الموجة). **قرار المالك.**

**4. `GetSentOutStatisticsQueryHandler`** — استبدل `IN` بـ `Join`.

**Infrastructure**
- `ResultMonitoringPdfWriter` على QuestPDF بالأعمدة: `اليوم · اسم المريض · بيانات المريض · جهة الإحالة · التحليل · النتيجة · الحالة`.

**Presentation**
- `StatisticsView.xaml` — ComboBox لكل `Grouping` + ComboBox جهة بعينها + ComboBox الفرع (WP-15) + تبويب «متابعة النتائج» (ComboBox الاختبار + DatePicker×2 + Min/Max + CheckBox «الإيجابية فقط» + شبكة + زر طباعة).

**Tests**
```
Statistics_Navigation_RequiresSecondaryPassword       ← حرج
Statistics_Navigation_Cancelled_ByWrongPassword
PatientStats_GroupByMonth
PatientStats_GroupByDayOfMonth
PatientStats_GroupBySex
PatientStats_GroupByAccountType
PatientStats_GroupByReferralEntity
PatientStats_FilterBySpecificReferralEntity
PatientStats_AllGroupings_RunInSql                   ← فحص سلوكي
TestStats_GroupByTest
TestStats_GroupByTestGroup
TestStats_GroupByReferralEntity
TestStats_FilterBySpecificReferralEntity
SentOutStats_DoesNotUseInClause                      ← فحص سلوكي
ResultMonitoring_FiltersByDateRange
ResultMonitoring_FiltersByMinMax
ResultMonitoring_PositivesOnly
ResultMonitoring_ExcludesNonNumericResults
ResultMonitoring_RequiresPermission
ResultMonitoring_PrintsSevenColumns
```

### خطوات التنفيذ

1. **نفّذ البوابة أولاً** — سطر واحد، مع اختبارها. أرخص إصلاح عالي القيمة في المشروع.
2. أنشئ `PatientStatisticsGrouping`.
3. وسّع `GetPatientCountStatisticsQuery` + handler.
4. انقل التجميع من الذاكرة إلى SQL.
5. وسّع `GetTestCountStatisticsQuery` + handler بتجميع جهة الإحالة.
6. استبدل نمط `IN` بـ join في `GetSentOutStatisticsQueryHandler`.
7. أنشئ `GetResultMonitoringQuery` + validator + handler.
8. أنشئ `ResultMonitoringPdfWriter`.
9. اربط المحدّدات في `StatisticsView.xaml`.
10. أضف الفرع بعد WP-15.
11. اكتب الاختبارات العشرين.

### الاعتماديات
**WP-15** · **WP-01** (لكاتب تقرير المتابعة).

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **تقنية** | `ResultValue` نصي يمنع الفلترة في SQL | إما `ResultValueNumeric` (هجرة) أو فرز في الذاكرة مع تنبيه. **قرار المالك** |
| تقنية | تغيير `GroupBy` قد يغيّر أنواع الإرجاع | اختبارات لكل تجميع (6+3) |
| تجربة مستخدم | محدّدات كثيرة | `GroupBox` لكل بُعد |
| — | لا شيء | — |

### معايير القبول
- [ ] فتح الإحصائيات بلا كلمة المرور الثانية **مرفوض**
- [ ] 9 تجميعات تُعيد بيانات
- [ ] تقرير المتابعة يعمل ويطبع بأعمدته السبعة
- [ ] صفر `ToList()` على فترة كاملة في المسارات الإحصائية
- [ ] صفر نمط `IN` بمعرّفات في إحصاءات العينات

### التراجع
آمن. **الاستثناء:** إذا أضفت `ResultValueNumeric` فالهجرة تحتاج backfill من `ResultValue` النصي.

---

### WP-20 — وفاء إعدادات التقرير (رأس/تذييل، مواضع، ألوان، غامق)

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | صفحة الإعدادات تعرض حقولاً لا وجود لقارئها ولا كاتب لها — عمودان محفوظان في القاعدة، لا setter، لا أمر، لا DTO، لا واجهة. |
| **المرجع** | R-36, R-81, R-82 · D-12, D-28 |
| **الحجم** | **L** (4 أيام) |
| **الهجرات** | **migration جديدة** — `AddReportHeaderFooterItemSettings` |
| **الاعتماديات** | **WP-01** — **وممنوع** أن يعتمد WP-01 على WP-20 (كسر الدائرة مطبَّق في الاتجاه الواحد) |

### الحالة الحالية — PROVED DEFECT
`ReportSettings.cs:21,23,47-48` يعلن `HeaderColor` و`FooterColor` بلا setter · `ReportSettingsConfiguration.cs:20-21,25` يربطهما · `ReportSettingsTests.cs:92-93` **يؤكد أنهما `null`** · لا حقل في `UpdateReportSettingsCommand` (`:8-16`) · لا خاصية في DTO · لا حقل في `ReportSettingsView.xaml:27-44` · لا قارئ في أي كاتب.

**ملاحظة إضافية:** `ReportSettingsView.xaml:85-95` فيه معاينة تعرض **الإعدادات** (قائمة قيم) لا المستند. (المعاينة الحقيقية في WP-01.)

**الغائب تماماً:** صورة رأس كاملة، صورة تذييل كاملة، مواضع عناصر بالسنتيمتر، لون لكل عنصر، غامق لكل صنف في التاريخ، السطر العلوي/السفلي، التاريخ الطويل، عرض الوقت، ألوان مخصصة.

### الحالة المستهدفة
عناصر الرأس والتذييل تُموضَع بالسنتيمتر، وتُلوَّن بلون مخصص، وتُسمَّن بال Quartet، ويُتحكَّم في السطر العلوي والسفلي والتاريخ الطويل والوقت؛ ولكل صنف في تاريخ المريض خانة «غامق»؛ ومعاينة الإعدادات تعرض المستند الفعلي.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Settings/ReportSettings.cs               ← + HeaderColor/FooterColor setters
  src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateReportSettings/*.cs
  src/TopLab.Application/Features/SystemAndPrintSettings/Queries/GetReportSettings/*.cs
  src/TopLab.Infrastructure/Persistence/Configurations/ReportSettingsConfiguration.cs
  src/TopLab.Infrastructure/Printing/ReportPageComposer.cs   ← من WP-01، يُوسَّع لقراءة العناصر
  src/TopLab.Infrastructure/Printing/ReportDocument.cs   ← من WP-01، يُوسَّع
  src/TopLab.Presentation/Views/Settings/ReportSettingsView.xaml
  src/TopLab.Presentation/ViewModels/Settings/ReportSettingsViewModel.cs
  tests/TopLab.Domain.Tests/Settings/ReportSettingsTests.cs   ← :92-93

يُضاف:
  src/TopLab.Domain/Settings/ReportPrintItemSetting.cs
  src/TopLab.Domain/Settings/ReportPrintItemKind.cs          ← enum (رأس/تذييل)
  src/TopLab.Domain/Settings/ReportElementKind.cs            ← enum (اسم المريض، المعرّف، العمر/الجنس…)
  src/TopLab.Infrastructure/Persistence/Configurations/ReportPrintItemSettingConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddReportHeaderFooterItemSettings.cs
  tests/TopLab.Presentation.Tests/Settings/SettingsCoverageTests.cs
  tests/TopLab.Infrastructure.Tests/Printing/ReportPageComposerTests.cs
```

### المكونات

**Domain**

شكل التخزين المقترح — **جدول ابن**، تماشياً مع `EnvelopePrintItemPosition` القائم (لا نص JSON):
```csharp
public sealed class ReportPrintItemSetting : AuditableEntity
{
    public Guid Id { get; }
    public ReportSettingsId ReportSettingsId { get; }
    public ReportElementKind Element { get; private set; }   // PatientName, PatientId, AgeSex,
                                                            // RequestDate, ReferredBy, PrintedIn
    public ReportPrintItemKind Band { get; private set; }     // Header, Footer
    public decimal LeftCm { get; private set; }               // 0..30
    public decimal TopCm { get; private set; }                // 0..45
    public bool IsVisible { get; private set; }
    public bool IsBold { get; private set; }
    public string? ColorHex { get; private set; }             // "#RRGGBB" أو null = أسود
    public bool UseLongDate { get; private set; }
    public bool ShowTime { get; private set; }
    public bool ShowTopLine { get; private set; }
    public bool ShowBottomLine { get; private set; }
}
```
- `ReportSettings.HistoryBoldAnalytes` — قائمة نصوص (صنف في العمود الغامق). **مخزّنة كـ JSON في عمود `nvarchar` واحد** (كافية، لا تستحق جدولاً).
- **setters لـ `HeaderColor`/`FooterColor`** — أو **حذف العمودين** (القرار 5).

**Application**
- CRUD: `SaveReportPrintItemSettingsCommand(IReadOnlyList<ReportPrintItemSettingDto>)` — يحذف ويعيد البناء في `SaveChangesAsync` واحد (الإعداد قابل للاستبدال الكامل).
- `GetReportSettingsQuery` ⇒ يعيد `Items` + `HistoryBoldAnalytes`.
- `UpdateReportSettingsCommand` + `HeaderColor`/`FooterColor` إن اعتُمد التنفيذ.

**Infrastructure**
- هجرة `AddReportHeaderFooterItemSettings` + `ReportSettings.HistoryBoldAnalytes` (nvarchar) + عمودَي اللون إن اعتُمد.
- **بذر** 6 عناصر رأس (اسم المريض، المعرّف، العمر/الجنس، تاريخ الطلب، ب physician الإحالة، طُبع في) بمواضع افتراضية.
- `ReportPageComposer` — يقرأ `Items` ويرسم كل عنصر مرئياً at (Left, Top) باللون والغامق المطلوب.
- `ReportDocument` — `UseLongDate` ⇒ تنسيق `dd MMMM yyyy` بالعربية؛ `ShowTime` ⇒ `HH:mm`؛ `ShowTopLine`/`ShowBottomLine` ⇒ خط فاصل.

**Presentation**
```xml
<!-- ReportSettingsView.xaml — شبكة العناصر -->
<DataGrid ItemsSource="{Binding PrintItems}" AutoGenerateColumns="False">
  <DataGridComboBoxColumn Header="العنصر" SelectedItemBinding="{Binding Element}"
                          ItemsSource="{Binding ElementKinds}" DisplayMemberPath="Display"/>
  <DataGridComboBoxColumn Header="الشريط" SelectedItemBinding="{Binding Band}" …/>
  <DataGridCheckBoxColumn Header="ظاهر"   IsChecked="{Binding IsVisible}"/>
  <DataGridCheckBoxColumn Header="غامق"   IsChecked="{Binding IsBold}"/>
  <DataGridTextColumn    Header="يسار (سم)" Binding="{Binding LeftCm}"/>
  <DataGridTextColumn    Header="أعلى (سم)" Binding="{Binding TopCm}"/>
  <DataGridTemplateColumn Header="اللون">
    <DataGridTemplateColumn.CellTemplate>
      <DataTemplate><Button Content="اختر…" Command="{Binding DataContext.PickColorCommand, …}"/></DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
  </DataGridTemplateColumn>
  <DataGridCheckBoxColumn Header="تاريخ طويل" IsChecked="{Binding UseLongDate}"/>
  <DataGridCheckBoxColumn Header="عرض الوقت"  IsChecked="{Binding ShowTime}"/>
  <DataGridCheckBoxColumn Header="سطر علوي"   IsChecked="{Binding ShowTopLine}"/>
  <DataGridCheckBoxColumn Header="سطر سفلي"   IsChecked="{Binding ShowBottomLine}"/>
</DataGrid>

<!-- قائمة الصنف الغامق في إعدادات التاريخ -->
<ItemsControl ItemsSource="{Binding AvailableAnalytes}">
  <ItemsControl.ItemTemplate>
    <DataTemplate><CheckBox Content="{Binding Name}" IsChecked="{Binding IsBold}"/></DataTemplate>
  </ItemsControl.ItemTemplate>
</ItemsControl>

<!-- المعاينة: تستدعي IPdfPreviewService من WP-01 -->
<Button Content="معاينة المستند الفعلي" Command="{Binding PreviewActualDocumentCommand}"/>
```

**Tests**
```
ReportPrintItemSetting_Create_Persists
SaveReportPrintItems_ReplacesAllAtomically
ReportSettings_SaveAndReload_HeaderFooterItems
ReportPageComposer_AppliesItemPosition
ReportPageComposer_AppliesItemColor
ReportPageComposer_AppliesBold
ReportPageComposer_SkipsInvisibleItem
ReportPageComposer_RendersLongDate
ReportPageComposer_RendersTime
ReportDocument_ShowsTopAndBottomLines
HistoryMatrix_BoldsSelectedAnalytesOnly
HeaderColor_SetterPersists_AndWriterReadsIt     ← فقط إن اعتُمد القرار 5(a)
SettingsCoverage_ReportSettings_AllPropertiesReadOrRemoved   ← حاسم
```

### خطوات التنفيذ

1. **اطلب القرار 5.** إمّا تنفّذ عمودَي اللون كاملاً، أو تحذفهما وتحدّث `ReportSettingsTests.cs:92-93` صراحةً وتحدّث ادعاء التوافق. **لا تتركهما نصف منفذين.**
2. أنشئ `ReportElementKind` و`ReportPrintItemKind` و`ReportPrintItemSetting`.
3. أنشئ الإعداد + **الهجرة** + البذر.
4. أنشئ `SaveReportPrintItemsCommand` + DTO + `GetReportSettings`.
5. أضف `HistoryBoldAnalytes` + هجرته.
6. وسّع `ReportPageComposer` ليقرأ العناصر.
7. وسّع `ReportDocument` (تاريخ طويل، وقت، خطوط).
8. ارسم الصنف الغامق في مصفوفة التاريخ (WP-10).
9. اربط شبكة العناصر في الواجهة.
10. استبدل معاينة الإعدادات بمعاينة المستند الفعلي عبر `IPdfPreviewService`.
11. **اكتب `SettingsCoverage_ReportSettings_AllPropertiesReadOrRemoved`** — هذا هو الفحص الذي يمنع عودة D-27/D-28.
12. اكتب الاختبارات.

### الاعتماديات
**WP-01** فقط. **WP-01 لا يعتمد على WP-20** — الكاتب في WP-01 يقرأ الحقول الحالية، وهذه الحزمة تضيف القراءة. العلاقة أحادية.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| بيانات | جدول ابن جديد على إعداد وحيد | سهل |
| تجربة مستخدم | منتقي ألوان في واجهة عربية بالكامل | **القرار 5(b) يحذف الميزة كلياً** إن كانت الألوان غير مهمة |
| **بنيوي** | `ReportSettingsView` الاستدعاء للمعاينة | يعتمد على `IPdfPreviewService` من WP-01 — جاهز |
| تقنية | تخزين قائمة الأصناف كـ JSON | مقبول لحقل نصّي قصير |

### معايير القبول
- [ ] كل حقل يُحفظ ويعود بعد إعادة التشغيل
- [ ] تغيير موضع ولون عنصر ينعكس في الـ PDF
- [ ] التاريخ الطويل والوقت يظهران عند التفعيل
- [ ] الصنف الغامق يظهر غامقاً في المصفوفة
- [ ] `SettingsCoverage_ReportSettings` أخضر
- [ ] معاينة الإعدادات تعرض **مستنداً** لا قائمة قيم

### التراجع
جدول ابن + عمود JSON. آمن.

---

### WP-21 — وفاء إعدادات الإيصال

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | تذييل فارغ دائماً بلا شرط، و«طباعة لمرة واحدة» موثّقة في الشيفرة نفسها كزخرفة. |
| **مرجع** | R-83 · D-18 |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **migration جديدة** — `ExtendReceiptSettings` |
| **الاعتماديات** | لا شيء (WP-18 يوفّر `InvoiceIssue` كدليل طباعة سابقة) |

### الحالة الحالية — PROVED DEFECT
`ReceiptPdfWriter.cs:214` ⇒ `return new List<string>()` للتذييل **بلا شرط** على `HeaderFooterMode` · `ReceiptPrintingService.cs:20` تعليق صريح: *"a display-only setting: no reprint-guard"* · `HeaderFooterMode.Images` falls back to words (`:160-161`) · `ReceiptSettings.CashierPrinterEnabled` **بلا أي مستهلك** خارج الإعداد والواجهة · الورق A5 مكتوب صلباً (`:68`).

حيّة وتعمل: `TopMarginCm` (`:56`) · `Currency` (`GetPatientReceiptQueryHandler.cs:47`) · `PickupTimeDefault` (`:210`) · `TestDetailDisplayMode` (`:189-195`) · `HeaderFooterMode` (`:162`).

### الحالة المستهدفة
التذييل يُرسم من الإعدادات؛ و«عند الطباعة لمرة ثانية» يُنفَّذ (منع/تنبيه/سماح)؛ ووضع الصور يُطبَّق؛ والورق من الإعداد؛ وطابعة الكاشير لها معنى.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Settings/ReceiptSettings.cs
  src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateReceiptSettings/*.cs
  src/TopLab.Application/Features/SystemAndPrintSettings/Queries/GetReceiptSettings/*.cs
  src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs
  src/TopLab.Infrastructure/Printing/ReceiptPrintingService.cs
  src/TopLab.Application/Features/PatientBilling/Commands/PrintReceipt/*.cs
  src/TopLab.Presentation/Views/Settings/ReceiptSettingsView.xaml

يُضاف:
  src/TopLab.Domain/Settings/PrintOnceAction.cs        ← enum
  src/TopLab.Domain/Settings/PaperSizeSetting.cs        ← enum (A4 | A5)
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_ExtendReceiptSettings.cs
  tests/TopLab.Infrastructure.Tests/Printing/ReceiptPdfWriterSettingsTests.cs
  tests/TopLab.Application.Tests/Features/PatientBilling/PrintOnceGuardTests.cs
```

### المكونات

**Domain**
```csharp
public enum PrintOnceAction : byte { Allow = 0, Warn = 1, Block = 2 }
public enum PaperSizeSetting : byte { A4 = 0, A5 = 1 }
```
`ReceiptSettings` — الحقول الجديدة:
| الحقل | النوع | الافتراضي |
|---|---|---|
| `FooterText` | `string?` (≤200) | `null` |
| `HeaderImagePath` | `string?` | `null` |
| `FooterImagePath` | `string?` | `null` |
| `PaperSize` | `PaperSizeSetting` | `A5` (الوضع القائم) |
| `PrintOnceAction` | `PrintOnceAction` | **`Allow`** (سلوك مطابق للحالي) |
- **الافتراضي `Allow` مقصود**: التغيير لا يكسر شيئاً قبل أن يقرر المالك.

**Application**
- الحقول في `UpdateReceiptSettingsCommand` + DTO.
- حارس الطباعة لمرة — في `PrintReceiptCommandHandler` أو `GetPatientReceiptQueryHandler`:
  ```csharp
  if (settings.PrintOnceAction != PrintOnceAction.Allow)
  {
      var alreadyPrinted = await _db.Set<InvoiceIssue>()
          .AnyAsync(i => i.PatientTestId == request.PatientTestId, ct);
      if (alreadyPrinted)
          return settings.PrintOnceAction == PrintOnceAction.Block
              ? Result.Failure(Error.Conflict("سبق طباعة الإيصال لهذه الزيارة."))
              : Result.Failure(Error.Validation("سبق طباعة الإيصال. هل تريد الطباعة مرة أخرى؟"));
  }
  ```
- **القرار 6** على `CashierPrinterEnabled`: تنفيذ (وجّه إلى `PrinterOutputType.Receipt` إن اختُلف) أو حذف الحقل.

**Infrastructure**
- هجرة `ExtendReceiptSettings` + بذر `PaperSize = A5`, `PrintOnceAction = Allow`.
- `ReceiptPdfWriter`:
  - `:214` ⇒ يعيد أسطر `FooterText` بدل `new List<string>()`.
  - `:68` ⇒ `PageSize` من `PaperSize`.
  - `HeaderFooterMode.Images` ⇒ يرسم الصورة إن وُجد مسار صالح، وإلا يعود للكلمات **مع تحذير في السجل** (لا صمت).

**Presentation**
`ReceiptSettingsView.xaml:27-57` — `TextBox` للتذييل، `ComboBox` للورق، `TextBox` لمساري الصور، `RadioGroup` لـ `PrintOnceAction`، وحقل `CashierPrinterEnabled` (أو إزالته).

**Tests**
```
ReceiptSettings_SaveAndReload_NewFields
Receipt_PrintsFooterTextFromSettings
Receipt_FooterEmpty_WhenFooterTextNull
Receipt_UsesConfiguredPaperSizeA4
Receipt_UsesConfiguredPaperSizeA5
Receipt_HeaderImageMode_RendersImage
Receipt_HeaderImageMode_MissingFile_LogsWarningAndFallsBack
PrintOnce_Allow_AlwaysPrints                    ← السلوك الافتراضي
PrintOnce_Warn_BlocksSecondPrintWithQuestion
PrintOnce_Block_RejectsSecondPrint
PrintOnce_FirstPrint_AlwaysSucceeds
CashierPrinter_RoutesToCashierPrinter           ← فقط إن نُفّذ القرار 6(a)
```

### خطوات التنفيذ

1. أضف `PrintOnceAction` و`PaperSizeSetting` والحقول الخمسة.
2. أنشئ **الهجرة** + البذر.
3. وسّع الأمر والاستعلام والـ DTO.
4. أصلح `ReceiptPdfWriter.cs:214` — التذييل من الإعدادات.
5. اجعل الورق من الإعداد `:68`.
6. نفّذ مسار الصور مع تحذير مسجَّل.
7. نفّذ حارس `PrintOnceAction` (الافتراضي `Allow`).
8. احسم `CashierPrinterEnabled` (القرار 6).
9. اربط الواجهة.
10. اكتب الاختبارات الإحدى عشرة.

### الاعتماديات
لا شيء. WP-18 يوفّر `InvoiceIssue`.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تجربة مستخدم | `Block` يمنع الطباعة الثانية — سلوك جديد صادم | **الافتراضي `Allow`**، والمالك يفعّل `Warn` أولاً |
| بيانات | هجرة على جدول إعدادات | بسيطة |
| تقنية | مسار صورة على مسار ملف | تحقق من الوجود + fallback مسجَّل |

### معايير القبول
- [ ] التذييل يظهر عند ضبط نصه
- [ ] الورق المُعدّ يُستخدم
- [ ] الطباعة الثانية تُنبّه أو تُمنع بحسب الإعداد
- [ ] السلوك الافتراضي (Allow) لم يتغيّر
- [ ] لا راية ميتة جديدة في هذا المسار

### التراجع
عمودان nullable + enum. آمن.

---

### WP-22 — كاتب المظروف + إعدادات الكارنيه + طابعة Card

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | إعدادات مظروف كاملة قابلة للتحرير ولا راسمة — كل قيمة محفوظة ومقروءة من لا أحد. و«الكارنيه» مفهوم غائب. |
| **المرجع** | R-63, R-79, R-84, R-85 · D-06, D-27 |
| **الحجم** | M (3–4 أيام) |
| **الهجرات** | **migration جديدة ×2** — `AddCardSettings` · تعديل `PrinterOutputType` |
| **الاعتماديات** | **WP-01** (نمط الكاتب) |

### الحالة الحالية
`EnvelopeSettings` + `EnvelopePrintItemPosition` + `Update/GetEnvelopeSettings*` + `EnvelopeSettingsView.xaml:26-70` (بشبكة مواضع رباعية) **كلها حقيقية وتعمل وتُحفظ**. `ls Infrastructure/Printing/` ⇒ **لا `Envelope*` ولا `Card*`**. `PrinterOutputType` = `Reports=0, Barcode=1, Envelope=2, Receipt=3` — بلا `Card`. لا كيان `CardSettings`.

**دمج مقصود:** الكارنيه والمظروف في حزمة واحدة لأنهما يتشارسان جدول توزيع الطابعات وصفحة الإعدادات. تقسيمهما يفرض لمس جدولين مرتين بلا فائدة.

### الحالة المستهدفة
طباعة المظروف تنتج مستنداً يحترم الهوامش ووضع الرأس/التذييل ومواضع العناصر الأربعة، مربوطاً بـ `PrinterOutputType.Envelope`؛ وللكارنيه إعداداته وطابعته الخاصة.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Common/Enums/PrinterOutputType.cs        ← + Card = 4
  src/TopLab.Application/Features/SystemAndPrintSettings/Commands/SavePrinterAssignments/*.cs
  src/TopLab.Infrastructure/Printing/ShellPdfPrinterDispatcher.cs
  src/TopLab.Presentation/Views/Settings/SystemSettingsView.xaml
  src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml   ← زر طباعة المظروف

يُضاف:
  src/TopLab.Domain/Settings/CardSettings.cs
  src/TopLab.Domain/Settings/CardPrintItemPosition.cs
  src/TopLab.Application/Features/SystemAndPrintSettings/Commands/{Update,Get}CardSettings/
  src/TopLab.Infrastructure/Persistence/Configurations/CardSettingsConfiguration.cs
  src/TopLab.Infrastructure/Printing/EnvelopePdfWriter.cs
  src/TopLab.Infrastructure/Printing/EnvelopePrintingService.cs
  src/TopLab.Infrastructure/Printing/CardPdfWriter.cs
  src/TopLab.Infrastructure/Printing/CardPrintingService.cs
  src/TopLab.Application/Features/ResultsEntry/Commands/PrintEnvelope/
  src/TopLab.Application/Features/ResultsEntry/Commands/PrintCard/
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddCardSettings.cs
  tests/TopLab.Infrastructure.Tests/Printing/EnvelopePdfWriterTests.cs
  tests/TopLab.Infrastructure.Tests/Printing/CardPdfWriterTests.cs
```

### المكونات

**Domain**
- `PrinterOutputType.Card = 4` — **أضِف ولا تُعِد ترتيب** (القيم مخزّنة كـ `byte`).
- `CardSettings` — مخطط لـ `EnvelopeSettings`: `PaperSize`, `TopMarginCm`, `HeaderFooterMode`, `ImagePath`, `TextLines` (3 أسطر)، `IsActive`، `Id = 1`.
- `CardPrintItemPosition` — موضع عنصر واحد (اسم المريض / المعرّف / التاريخ).

**Application**
- `GetCardSettingsQuery` / `UpdateCardSettingsCommand` + validator — **انسخ نسيج** `GetEnvelopeSettings`/`UpdateEnvelopeSettings` حرفياً.
- `PrintEnvelopeCommand(int PatientTestId)` — `IAuthorizedRequest(Permissions.PrintResults)`.
- `PrintCardCommand(int PatientTestId)` — **صلاحية منفصلة** `PRINT_CARD`.
  > **[C6]** الرمز الجديد لا يكفي بكتابة الـ `HasData`: يجب (1) إضافته إلى
  > `PermissionConfiguration.cs:17-31`، و(2) زرع المنح القائمة في `UserPermissionGrant` من
  > `ADD_EDIT_PATIENT` (1) — وإلا كان الرمز بلا منحة و`AuthorizationBehavior` سيرفض كل شيء.
  > الترتيب: زرع أولاً، ثم تفعيل الفحص. نفس بروتوكول WP-28.
- استعلام بناء المظروف: `GetEnvelopePayloadQuery` (اسم المريض، الجنس، العمر، الطبيب، جهة الإحالة، التاريخ، الشريط).

**Infrastructure**
- `EnvelopePdfWriter` على QuestPDF:
  - مقاس المظروف (مثلاً 220×110 mm — **قرار المالك**، قياس حقيقي للحقيبة).
  - `TopMarginCm` ⇒ `page.MarginTop`.
  - `HeaderFooterMode` ⇒ رأس/تذييل/كلاهما.
  - `SuppressCaptions` ⇒ إخفاء النصوص التوضيحية.
  - `EnvelopePrintItemPosition` الأربعة ⇒ مواضع العناصر.
- `CardPdfWriter` على النمط نفسه.
- `ShellPdfPrinterDispatcher` — أضف فرع `PrinterOutputType.Card`.
- هجرتان: `AddCardSettings` (بذر `Id=1`) + تعديل التعداد (لا هجرة enum مطلوبة في EF لكن يلزم توثيقها).

**Presentation**
- تبويب «الكارنيه» في `SystemSettingsView` + **صفّ طابعة الكارنيه** في `:29-43`.
- زر «طباعة المظروف» في `ProfileEntryView.xaml:132-138` بصلاحيته.

**Tests**
```
GetCardSettings_ReturnsSeededSettings
UpdateCardSettings_PersistsAndReloads
Envelope_PrintsPerSettings                          ← حرج
Envelope_UsesTopMargin
Envelope_RespectsSuppressCaptions
Envelope_PlacesItemsAtConfiguredPositions
Envelope_DispatchesToEnvelopePrinter
Envelope_RequiresPrintPermission
Card_PrintsWithHeaderFooter
Card_PrintsPatientNameAndId
Card_PrintRequiresPrintCardPermission
Card_DispatchesToCardPrinter
PrinterOutputType_Card_IsStableAcrossMigration      ← القيم القديمة لم تتغيّر
```

### خطوات التنفيذ

1. أضف `Card = 4` إلى `PrinterOutputType` (ملف واحد، سطر واحد).
2. أنشئ `CardSettings` + `CardPrintItemPosition` + الإعداد + **الهجرة** + البذر.
3. أنشئ `Get/UpdateCardSettings` (نسخ حرفي من Envelope).
4. **اكتب `EnvelopePdfWriter` أولاً** — إعداداته موجودة بالفعل، فيخدم مثالاً لـ `CardPdfWriter`.
5. أنشئ `GetEnvelopePayloadQuery` + `PrintEnvelopeCommand`.
6. اربط `ShellPdfPrinterDispatcher` بـ `Envelope`.
7. أنشئ `CardPdfWriter` + `PrintCardCommand` + فرع الـ dispatcher.
8. أضف تبويب الكارنيه وصفّ الطابعة.
9. أضف زر المظروف بصلاحيته.
10. اكتب الاختبارات الثلاثة عشر.

### الاعتماديات
**WP-01** (نمط الكاتب + `IPdfPrinterDispatcher`).

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | إضافة قيمة للتعداد المخزّن | إلحاق بـ `4` فقط، **لا** إعادة ترتيب. اختبار `IsStableAcrossMigration` |
| تقنية | قياس حقيبة المظروف غير معروف | **قرار المالك** على المقاس؛ اجعله في `EnvelopeSettings` لا ثابتاً |
| تجربة مستند | تبويب جديدان | تابيبات قائمة |
| — | صلاحية `PrintCard` جديدة تحتاج زرع | زرع في WP-28 أو هنا مع مع تنبيه المستخدمين الحاليين |

### معايير القبول
- [ ] المظروف يُطبع ويصل للطابعة المخصّصة
- [ ] الكارنيه يُطبع من إعداداته
- [ ] القيم القديمة لـ `PrinterOutputType` لم تتغيّر
- [ ] كل مسار طباعة في هذه الحزمة ينتج مستنداً (لا وسم)

### التراجع
جدول جديد + قيمة تعداد زائدة (آمنة، لا تعني شيئاً). **آمن تماماً.**

---

### WP-23 — وفاء الباركود: المقاس، الاتجاه، الرمز، ملصقات الأنابيب

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | مقاس الملصق 2.7× المرجع، والرمز مكتوب صلباً، ولا ملصق أنبوب واحد — وعمود الباركود في ورقة العمل نصّ لا صورة. |
| **المرجع** | R-86, R-50 · D-19, D-29 · تقرير الوكيل الفرعي الأول Cap 11 |
| **الحجم** | M (3–4 أيام) |
| **الهجرات** | **migration جديدة** — `AddBarcodeSettings` |
| **الاعتماديات** | **WP-08** (أنواع الرموز) · **WP-17** (نوع باركود الاختبار) |

### الحالة الحالية — PROVED DEFECT
`BarcodeService.cs:88-89` `const int pageWidth = 288; const int pageHeight = 144;` ⇒ **101.6×50.8 مم** بدل **38×25 مم** الموصوف في `RL_Show` p.3 — أي 2.7× المساحة المطلوبة · `BarcodeLabelRenderer.cs:18` `BarcodeFormat.CODE_128` صلباً · `grep IBarcodeService` ⇒ المستهلك الوحيد `PrintBarcodeCommandHandler.cs:44` (ملصق مريض واحد) · `WorkSheetPdfWriter.cs:173` عمود الباركود **نصّ** لا صورة (`string.IsNullOrWhiteSpace(l.Barcode) ? "—" : l.Barcode`) · لا اتجاه، ولا ورق مطبوع مسبقاً، ولا إزاحة إطار · `BarcodeService.cs:207-221` نسخة ثانية من `ToAscii` تفسد السطر القابل للقراءة بالعربية.

### الحالة المستهدفة
ملصق 38×25 مم قابل للضبط، واتجاه عمودي/أفقي، ونوع رمز من إدارة WP-08، وورق مطبوع مسبقاً/أبيض مع إزاحة محاذاة، وملصق لكل أنبوب، وخيار «طباعة الباركود بعد قراءة المعرّف».

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Infrastructure/Barcode/BarcodeService.cs
  src/TopLab.Infrastructure/Barcode/BarcodeLabelRenderer.cs
  src/TopLab.Infrastructure/Printing/WorkSheetPdfWriter.cs     ← :158 تاريخ محلي، :173 صورة لا نص
  src/TopLab.Domain/Tests/Test.cs                                ← BarcodeTypeId (WP-17)
  src/TopLab.Application/Features/PatientRegistration/Commands/PrintBarcode/PrintBarcodeCommandHandler.cs
  src/TopLab.Application/Features/PatientRegistration/Commands/PrintBarcode/PrintBarcodeCommand.cs

يُضاف:
  src/TopLab.Domain/Settings/BarcodeSettings.cs
  src/TopLab.Domain/Settings/BarcodeOrientation.cs
  src/TopLab.Domain/Results/PatientTest.TubeBarcode.cs         ← أو حقل على PatientTest
  src/TopLab.Infrastructure/Persistence/Configurations/BarcodeSettingsConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddBarcodeSettings.cs
  src/TopLab.Application/Features/ResultsEntry/Commands/PrintTubeBarcodes/
  tests/TopLab.Infrastructure.Tests/Barcode/BarcodeGeometryTests.cs
  tests/TopLab.Infrastructure.Tests/Printing/WorkSheetBarcodeImageTests.cs
```

### المكونات

**Domain**
```csharp
public enum BarcodeOrientation : byte { Horizontal = 0, Vertical = 1 }
public sealed class BarcodeSettings  // Id = 1
{
    public decimal WidthMm  { get; private set; }   // افتراضي 38
    public decimal HeightMm { get; private set; }   // افتراضي 25
    public BarcodeOrientation Orientation { get; private set; }
    public bool UsePrePrintedStock { get; private set; }
    public decimal FrameOffsetMm { get; private set; }      // للورق المطبوع مسبقاً
    public bool PrintAfterReadId { get; private set; }
    public bool IsActive { get; private set; }
}
```
- `PatientTest.TubeBarcodeValue` (`string?`) — payload الأنبوب.

**Application**
- `PrintTubeBarcodesCommand(IReadOnlyList<int> patientTestIds, Guid? barcodeTypeId)` — من ورقة العمل أو من لوحة السحب (WP-25).
- `Test.BarcodeTypeId` (WP-17) ⇒ اختيار النوع لكل اختبار؛ الافتراضي من `BarcodeType` النشط.

**Infrastructure**
- هجرة `AddBarcodeSettings` + بذر `38 × 25`, أفقي, ورق أبيض.
- `BarcodeService.cs:88-89` — استبدل الثوابت:
  ```csharp
  var pageWidth  = (int)(settings.WidthMm  / 25.4 * 72);   // مم → نقطة
  var pageHeight = (int)(settings.HeightMm / 25.4 * 72);
  ```
  > ⚠ **الوحدة الحالية للنقطة (pageWidth = 288) ليست 72 نقطة/بوصة** — الفلتر يستخدم `pageWidth` و`pageHeight` بحساب مختلف. **اقرأ `BarcodeService.cs:99-106` كاملاً أولاً** واستنتج الوحدة الفعلية قبل التغيير، واختبر بالقياس (ملف PDF الناتج مقاسه 38×25 مم ⇒ افتحه في عارض وقيِس).
- `BarcodeLabelRenderer` — اجعل `BarcodeFormat` من `BarcodeType.Symbology`.
- `BarcodeService.cs:207-221` — **احذف `ToAscii` المنسوخ**، وارسم السطر القابل للقراءة بخط عربي مضمّن (نفس نهج `ArabicFontResolver`).
- `WorkSheetPdfWriter.cs:173` — أدرج صورة الباركود بدل النص.
- `FrameOffsetMm` ⇒ إزاحة موضع الرسم داخل حدود الملصق للورق المطبوع مسبقاً.
- `UsePrePrintedStock` ⇒ رسم حدود الملصق أم لا.

**Presentation**
- صفحة إعدادات «الباركود» في `SystemSettingsView`.
- عمود «طباعة ملصقات الأنابيب» في `WorkSheetsView.xaml` + اختيار النوع.
- منتقي نوع الباركود في `TestEditorWindow` (WP-17).

**Tests**
```
BarcodeSettings_DefaultsTo38x25mm
Barcode_UsesConfiguredStickerWidth
Barcode_UsesConfiguredStickerHeight
Barcode_Produces38x25mmPdf                       ← قياس فعلي لملف PDF
Barcode_RendersSelectedSymbology                 [Theory: Code128 | Code39 | EAN13]
Barcode_Vertical_RotatesOutput
Barcode_Horizontal_IsDefault
Barcode_PrePrintedStock_AppliesFrameOffset
Barcode_WhitePaper_DrawsBorder
Barcode_HumanReadableLine_IsArabic               ← لا ToAscii
Barcode_PrintsPerTubeLabels
WorkSheet_EmbedsBarcodeImages                    ← لا نص
TubeBarcode_UsesTestBarcodeType
PrintAfterReadId_TriggersBarcodeAfterRegistration ← من WP-25
```

### خطوات التنفيذ

1. **اقرأ `BarcodeService.cs` كاملاً** قبل أي تعديل — الوحدة الفعلية للنقطة ليست بديهية.
2. أضف `BarcodeSettings` + الإعداد + **الهجرة** + البذر.
3. استبدل الثوابت `:88-89` بالقيم من الإعدادات.
4. **اكتب `Barcode_Produces38x25mmPdf` أولاً** — يجب أن يفشل قبل الخطوة 3.
5. اجعل `BarcodeFormat` من `BarcodeType`.
6. نفّذ الدوران للاتجاه.
7. نفّذ `FrameOffsetMm` و`UsePrePrintedStock`.
8. **احذف `ToAscii` من `BarcodeService.cs:207-221`** وارسم بالعربية.
9. أنشئ `PrintTubeBarcodesCommand`.
10. أدرج الصور في `WorkSheetPdfWriter.cs:173`.
11. نفّذ `PrintAfterReadId` **بعد** WP-25.
12. اربط الواجهة.
13. اكتب الاختبارات الخمسة عشر.

### الاعتماديات
**WP-08** · **WP-17**. `PrintAfterReadId` بعد **WP-25**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **تقنية** | تغيير وحدة القياس قد ينتج ملصقاً بمقاس خاطئ | اختبار قياس فعلي لملف PDF إلزامي |
| تقنية | `ToAscii` في الباركود يفسد العربية | اختبار `HumanReadableLine_IsArabic` |
| تجربة مستخدم | الورق المطبوع مسبقاً يحتاج محاذاة يدوية | `FrameOffsetMm` قابل للضبط مع معاينة |
| — | `Test.BarcodeTypeId` من WP-17 | نفّذه بعد WP-17 أو اجعله اختيارياً أولاً |

### معايير القبول
- [ ] ملف PDF المنتَج مقاسه 38×25 مم فعلياً (مقاس بالملف لا بالشيفرة)
- [ ] الأنواع الثلاثة تُنتج رموزاً مختلفة
- [ ] الاتجاهان يعملان
- [ ] السطر القابل للقراءة عربي
- [ ] عمود الباركود في ورقة العمل صورة
- [ ] ملصق لكل أنبوب

### التراجع
جدول إعدادات + عمود nullable. آمن. **الاستثناء:** إن غيّرت وحدة القياس بأخطاء، التراجع هو `git revert` كامل للحزمة.

---

### WP-24 — إكمال إعدادات النظام

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | ثلاث رايات محفوظة في القاعدة ولا أحد يقرأها، وخمس قدرات موصوفة غائبة. |
| **المرجع** | R-79 · D-27 |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **migration جديدة** — `CompleteSystemSettings` — مسجّلة في ملحق ج §ج.1 (**البند 20**). **تُنسَّق مع WP-04** على `ExternalEntity`: ادمج `EnglishName` في `AddExternalEntityEmail` إن كان التوقيت يسمح، وإلا أبقِهما هجرتين متتاليتين بلا تعارض |
| **الاعتماديات** | **WP-15** (الفرع) · **WP-22** (الكارنيه) · **WP-04** (لتنسيق هجرة `ExternalEntity`) |

### الحالة الحالية — PROVED DEFECT

**حيّة ومقروءة** (لا تعمل عليها): `DefaultAccountType` → `GetRegistrationCatalogQueryHandler.cs:70` · `DisableAutoTitleInsertion` → `:71` · `EnablePatientNameSearchAssist` → `SearchPatientsGlobalQueryHandler.cs:25` · `PrintLabIdInsteadOfPatientId` → `PrintBarcodeCommandHandler.cs:40` و`ReportPdfWriter.cs:45` · `PrintDateTimeOnTubeBarcode` → `BarcodeService.cs:62` · `AutoReviewAndComplete` → `EnterResultCommandHandler.cs:116` · `DailyBackup*` → `DailyBackupHostedService.cs:57,62`

**ميتة** (محفوظة + تُقرأ في VM، لا أحد يستهلكها):
| الراية | الدليل |
|---|---|
| `SaveTreatingDoctorOnlyFromEntityWindow` | `SystemSettingsConfiguration.cs:18` + `SystemSettingsViewModel.cs:186` فقط |
| `PrintAccountInsteadOfDateOnReport` | `:23` + VM `:193` فقط |
| `ResultScreenAccountDisplayMode` | `:17` + VM `:194` فقط (شاشة النتائج تتجاهلها) |

**غائبة:** الفرع (→ WP-15) · الأسماء الإنجليزية على الإيصال (لا `EnglishName` في النموذج) · استبدال «نفسه» عند فراغ الإحالة · الإكمال التلقائي للاختبارات · طابعة الكارنيه (→ WP-22)

### الحالة المستهدفة
لا راية ميتة واحدة في `SystemSettings`؛ والخمس قدرات الموصوفة تعمل.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Settings/SystemSettings.cs
  src/TopLab.Domain/Patients/Patient.cs                      ← EnglishName
  src/TopLab.Domain/ExternalEntities/ExternalEntity.cs      ← EnglishName (WP-04 منسَّق)
  src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateSystemSettings/*.cs
  src/TopLab.Application/Features/PatientRegistration/Queries/GetRegistrationCatalog/GetRegistrationCatalogQueryHandler.cs
  src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs
  src/TopLab.Application/Features/PatientBilling/Queries/GetPatientReceipt/*.cs
  src/TopLab.Presentation/ViewModels/Settings/SystemSettingsViewModel.cs   ← :186, :193, :194
  src/TopLab.Presentation/Views/Settings/SystemSettingsView.xaml

يُضيف:
  src/TopLab.Domain/Settings/ReferralBlankSubstitution.cs    ← قيمة ثابتة أو قائمة
  src/TopLab.Application/Features/PatientRegistration/Queries/GetTestAutocomplete/
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_CompleteSystemSettings.cs
  tests/TopLab.Presentation.Tests/Settings/SystemSettingsCoverageTests.cs
  tests/TopLab.Application.Tests/Features/PatientRegistration/AutocompleteTests.cs
```

### المكونات

**Domain**
- `Patient.EnglishName` (`string?`, ≤100) و`ExternalEntity.EnglishName` (`string?`, ≤100).
- `SystemSettings.PrintEnglishNamesOnReceipt` (bool, **افتراضي `false`**).
- `SystemSettings.ReferralBlankSubstitution` (`string?`) — النص الذي يُوضع عند فراغ الإحالة. المواصفة تقول «نفسه/نفسها» ⇒ القيمة المقترحة `"نفسه"` مع **قائمة_dropdown** (نفسه · يذكر بنفسه · — اتركه فارغاً).
- `SystemSettings.AutoCompleteLabTests` (bool، **افتراضي `false`**).
- `SystemSettings.SaveDoctorOnlyFromEntityWindow` — **احسم** (القرار 7).
- `SystemSettings.PrintAccountInsteadOfDateOnReport` — **احسم** (القرار 7).
- `ResultScreenAccountDisplayMode` — **احذف** (قرار 7 المقترح).

**Application**
- **تنفيذ الراية الحية الأولى** `SaveTreatingDoctorOnlyFromEntityWindow`: في `PatientEditorView.xaml` اجعل حقل الطبيب قابلاً للتحرير **فقط** عند فتح نافذة الجهة الخارجية؛ غير ذلك يُعرض للقراءة فقط.
- `PrintAccountInsteadOfDateOnReport` — **قرار**: إن كانaci «إظهار رقم الحساب بدلاً من التاريخ في رأس التقرير» ⇒ `ReportDocument` يقرؤه. **المالك يقرر.**
- `PrintEnglishNamesOnReceipt` ⇒ `ReceiptPdfWriter` يرسم `EnglishName` تحت الاسم العربي.
- `ReferralBlankSubstitution` ⇒ في `CreatePatientCommandHandler`:
  ```csharp
  var referralId = request.ReferralEntityId ?? await ResolveBlankSubstitutionAsync(ct);
  ```
 Ev أي قيمة نصّية ⇒ ابحث عن جهة خارجية بهذا الاسم، أو احفظها كسلسلة بدل `Id`. **الأبسط:** احفظ النص في `Patient.ReferralFreeText` (عمود nullable) واستخدمه عند العرض والطباعة. **قرار صغير للمالك.**
- `GetTestAutocompleteQuery(string term, int take = 10)` — `t.Name.StartsWith(term) || EF.Functions.Like(t.Name, term + "%")` + `AsNoTracking` + `Take(10)`.
- **الأداء:** Throttle 250ms + `CancelPendingRequests` في VM.

**Infrastructure**
- **تنسيق مع WP-04:** WP-04 يضيف `ExternalEntity.Email`. اجمعها في هجرة واحدة `CompleteExternalAndSystemSettings` أو اجعلهما متتاليتين بلا تعارض. **قرار 9 في §7.**

**Presentation**
- `SystemSettingsView` — الحقول الخمسة + **اختفاء** حقول الرايات المحذوفة.
- `PatientEditorView` — قوائم الإكمال التلقائي، وحقل الإحالة الحرّ عند تفعيل الاستبدال.

**Tests**
```
SystemSettings_AllPropertiesAreConsumedOrRemoved        ← حاسم
SystemSettings_NoPropertyIsWriteOnly
SaveDoctorOnlyFromEntity_LocksFieldOutsideEntityWindow
PrintEnglishNamesOnReceipt_WhenEnabled
PrintEnglishNamesOnReceipt_DisabledByDefault
BlankReferral_UsesConfiguredSubstitution
BlankReferral_FreeTextAppearsInReport
Autocomplete_ReturnsMatchingTests
Autocomplete_LimitsToTen
Autocomplete_IsCaseInsensitive
Autocomplete_RequiresPermission
CardPrinterRow_AppearsInPrinterSettings             ← من WP-22
BranchRow_AppearsInSystemSettings                   ← من WP-15
```

### خطوات التنفيذ

1. **اطلب القرار 7** — حسم الرايات الثلاث.
2. نفّذ السالبات أولاً:
   - `ResultScreenAccountDisplayMode` ⇒ حذف العمود + تحديث الإعداد + `SystemSettingsViewModel.cs:194`.
   - `PrintAccountInsteadOfDateOnReport` ⇒ تنفيذ **أو** حذف حسب قرار المالك.
   - `SaveTreatingDoctorOnlyFromEntityWindow` ⇒ تنفيذ (موصوف بوضوح).
3. أضف `EnglishName` للمريض والطباعة.
4. أضف `PrintEnglishNamesOnReceipt` + `ReceiptPdfWriter`.
5. نفّذ `ReferralBlankSubstitution` (+ قرار «النص الحر»).
6. أنشئ `GetTestAutocompleteQuery` + Throttle في VM.
7. أضف الحقول للواجهة.
8. اجمع الفرع (WP-15) والكارنيه (WP-22).
9. اكتب الاختبارات.

### الاعتماديات
**WP-15** · **WP-22** · **WP-04** (تنسيق المخطط).

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| تجربة مستخدم | الإنجليزية قد تُربك واجهة عربية بالكامل | **افتراضي `false`** — تُفعَّل بإرادة المالك |
| **تصميم** | «نفسه» تتطلّب `ReferralFreeText` جديد | **قرار صغير للمالك** |
| بيانات | تعارض هجرة مع WP-04 على `ExternalEntity` | **قرار 9** — ادمج الهجرتين أو رتّبهم |
| بنيوي | اختبار التغطية قد يُنتج إيجابيات كاذبة | ابدأ بقائمة مخصّصة، ثم وسّع |

### معايير القبول
- [ ] `SystemSettings_AllPropertiesAreConsumedOrRemoved` أخضر
- [ ] صفر خاصية في `SystemSettings` بلا قارئ
- [ ] الأسماء الإنجليزية تظهر عند التفعيل فقط
- [ ] الإكمال التلقائي يعمل بحدّ 10 نتائج
- [ ] استبدال الإحالة الفارغة يعمل

### التراجع
حذف أعمدة ممكن؛ التراجع الجزئي هنا مُعقّد (الخصائص المحذوفة لا تعود). **نفّذ الحذف في خطوة منفصلة** عن الإضافات.

---

### WP-25 — فصل وسحب العينات: كيان الأنبوب + «أنبوب آخر»

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | نموذج الأنبوب غائب كلياً، فقلبُ «فصل وسحب العينات» بلا موضوع. اللوحة صفوفها اختبارات لا أنابيب. |
| **المرجع** | R-87, R-86 · تقرير الوكيل الفرعي الأول Cap 12 |
| **الحجم** | **L** (5 أيام) |
| **الهجرات** | **migration جديدة** — `AddTubes` + seed |
| **الاعتماديات** | **WP-01**. يستفيد من **WP-23** إن نُفِّذ بعده. |

### الحالة الحالية — MISSING FEATURE + POTENTIAL RISK
`grep -i "tube|أنبوب"` (خارج الهجرات) ⇒ `PrintDateTimeOnTubeBarcode` فقط · لا كيان أنبوب ولا `TubeId` · `SampleDrawBoardView.xaml:75-103` صف واحد لكل `PatientTest` باسم اختبار · `GetPatientTestsForDrawQueryHandler.cs:61-71` يقسّم صفوفاً لا أنابيب · لا مسار «حفظ بعد قراءة المعرّف» · `SampleDrawBoardViewModel.cs:200` `MarkSampleDrawnCommand(row.PatientTestId, DateTime.MinValue)` بينما المعالج **يتجاهل** `DrawnAtUtc` ويستخدم `_clock.UtcNow` — **فخّ تلوّث صامت** · `:203-213` يُهمل `Task` المُرَدّ · `:95,134` تسميتان إنجليزيتان `"semen"` و`"CSF"` وسط تسميات عربية.

### الحالة المستهدفة
اللوحة صفوفها أنابيب لا اختبارات؛ كل أنبوب يحمل اختباراته وعلاماته؛ يوجد «أنبوب آخر»؛ يمكن حفظ العينات بمسح باركود؛ والتسميات كلها عربية؛ و`DrawnAtUtc` محترم.

### الملفات المتأثرة

```
يُضاف (Domain):
  src/TopLab.Domain/Tests/Tube.cs
  src/TopLab.Domain/Results/SampleDrawBatch.cs

يُعدَّل (Domain):
  src/TopLab.Domain/Results/PatientTest.cs                 ← + TubeId
  src/TopLab.Application/Features/SampleCollection/Commands/MarkSampleDrawn/*.cs  ← احترام DrawnAtUtc

يُعدَّل (Application):
  src/TopLab.Application/Features/SampleCollection/Queries/GetPatientTestsForDraw/*.cs  ← إعادة كتابة
  src/TopLab.Application/Features/SampleCollection/Commands/MarkSampleDrawn/MarkSampleDrawnCommandHandler.cs
  src/TopLab.Application/Features/SampleCollection/Commands/MarkAllSamplesDrawnForPatient/*.cs

يُضاف (Application):
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/{Create,Update,Deactivate}Tube/
  src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetTubes/
  src/TopLab.Application/Features/SampleCollection/Commands/MarkTubeDrawn/
  src/TopLab.Application/Features/SampleCollection/Queries/GetPatientTubesForDraw/

يُعدَّل (Presentation):
  src/TopLab.Presentation/Views/Lab/SampleDrawBoardView.xaml
  src/TopLab.Presentation/ViewModels/Lab/SampleDrawBoardViewModel.cs
  src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml          ← عمود الأنبوب

يُعدَّل (Presentation) — إضافي:
  src/TopLab.Presentation/Views/Lab/LabHubView.xaml                  ← تبويب الأنابيب

يُضاف (Presentation):
  src/TopLab.Presentation/Views/SampleCollection/OtherTubeDialog.xaml
  src/TopLab.Presentation/ViewModels/SampleCollection/OtherTubeDialogViewModel.cs

يُضاف (Infrastructure):
  src/TopLab.Infrastructure/Persistence/Configurations/TubeConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddTubes.cs
  tests/TopLab.Application.Tests/Features/SampleCollection/TubeDrawTests.cs
```

### المكونات

**Domain**
```csharp
public sealed class Tube : AuditableEntity
{
    public Guid Id { get; }
    public string Name { get; private set; }        // "أنبوب محمر", "EDTA", …
    public int SortOrder { get; private set; }
    public string? ColorHex { get; private set; }
    public bool IsActive { get; private set; }
}
```
- `PatientTest.TubeId` (`TubeId?`, FK) — nullable: الاختبارات بلا أنبوب تبقى صالحة.
- `SampleDrawBatch` — اختياري لتجميع «حفظ بعد قراءة المعرّف» (متى سُحبت دفعة، من أي مسح).
- `MarkSampleDrawnCommand.PatientTestId, DateTime DrawnAtUtc` — **المعامل الميت يجب أن يُحترم** (أو يُحذف، انظر الخطوات).

**Application**
- CRUD `Tube` + `GetTubesQuery` في `TestCatalogAndReferenceRanges` (نفس النسيج).
- `GetPatientTubesForDrawQuery(int patientId)` ⇒ صفوف أنابيب:
  ```csharp
  new TubeDrawRowDto(
      TubeId, TubeName, SortOrder,
      IReadOnlyList<TubeTestRowDto> Tests,   // اسم + رايات النوع
      IsDrawn, DrawnAtLocal);
  ```
  **تجميع في الذاكرة على نتيجة مُقيَّدة بمريض واحد** — مقبول وموثّق.
- `MarkTubeDrawnCommand(int patientId, Guid tubeId, DateTime drawnAtUtc)`:
  - يعثر على كل `PatientTest` بـ `TubeId == tubeId` و`PatientId == patientId` و`!IsSampleDrawn` و**ليس** خارج المعمل (حارس `:41-44` القائم).
  - يوسمها كلها في **SaveChangesAsync واحد** ⇒ ذرّية.
  - يعيد `IReadOnlyList<int>` للمعرّفات الموسومة.
- `MarkSampleDrawnCommandHandler` — **احترم `DrawnAtUtc`** أو احذف المعامل:
  > **قرار: احذف المعامل.** هو مُغذّى بـ `DateTime.MinValue` من `:200`، وأي معالج لاحق يحترمه ⇒ كل عينة بتاريخ `0001-01-01`. الحذف يجعل `IClock` المصدر الوحيد للحقيقة. **هذا هو الخيار الآمن.**
- «أنبوب آخر» — لكل زيارة: `OtherTubeDialog` ينشئ سجل `Tube` بـ `IsActive=false` واسم حرّ، أو `PatientTest.TubeId` pointing لسجل مؤقت. **الأبسط:** `Tube` عادي يُنشأ بـ `IsActive=false` فلا يظهر في قائمة المعمل لكنه صالح للأنبوب الحرّ في هذه الزيارة.

**Infrastructure**
- هجرة `AddTubes` + **بذر قائمة أنابيب معقولة** (قرار المالك على الأسماء):
  `EDTA (بنفسجي)` · `سيروم (أحمر)` · `فلورايد (أصفر)` · `بول (أصفر)` · `براز (بني)` · `سائل منCSF (شفاف)` · ` cultures ( specialised)`.
- فهرس على `PatientTest.TubeId`.

**Presentation**
```
SampleDrawBoardView.xaml — DataGrid بصفوف الأنابيب:
  <DataGridTemplateColumn Header="مسحوبة">
    <DataGridCheckBoxColumn Header="سُحبت" Binding="{Binding IsDrawn}" IsReadOnly="False"/>
  </DataGridTemplateColumn>
  <DataGridTextColumn Header="الأنبوب"   Binding="{Binding TubeName}" IsReadOnly="True"/>
  <DataGridTextColumn Header="الاختبارات" Binding="{Binding TestsSummary}" IsReadOnly="True"/>
  <DataGridTextColumn Header="وقت السحب"  Binding="{Binding DrawnAtLocal}"/>
  <DataGridTemplateColumn Header="إجراءات">
    <Button Content="فتح" Command="{Binding OpenTubeCommand}"/>
  </DataGridTemplateColumn>
+ زر «أنبوب آخر» في شريط الأدوات
```
- داخل الأنبوب: شبكة الاختبارات (نفس رايات النوع الحالية، **بالعربية**).
- `SampleDrawBoardViewModel`:
  - **احترم `Task` المُرَدّ** (`:203-213`): `await MarkSampleAsync(dto)` أو `async void` مع `try/catch` وتسجيل.
  - `ReloadCommand` — مرّر `CancellationToken` كما تفعل بقية الدوال.
  - **اختر عبر DI** كسائر VMs (`SampleCollectionViewModel.cs:129` ينشئ `new SampleDrawBoardViewModel(...)` — استبدله بـ `IServiceProvider.GetRequiredService`).
- `PatientEditorView` — ComboBox «الأنبوب» لكل اختبار.

**Tests**
```
CreateTube_Persists
GetTubes_ReturnsActiveOnly_ByDefault
DeactivateTube_KeepsExistingPatientTestLinks
DrawBoard_GroupsTestsByTube
DrawBoard_ExcludesTestsWithoutTube
DrawBoard_HandlesEmptyPatient
MarkTubeDrawn_MarksAllItsTestsAtomically
MarkTubeDrawn_SkipsOutsideLabTests
MarkTubeDrawn_SkipsAlreadyDrawn
MarkTubeDrawn_UsesClockNotCommandParameter        ← يغلق فخ DateTime.MinValue
MarkTubeDrawn_MarksOnlyTargetPatient
OtherTube_CanBeCreatedPerVisit
OtherTube_DoesNotAppearInLabTubeList
SaveAfterReadId_MarksSamplesByBarcodeScan        ← من WP-23
DrawnAt_IsPersistedWithRealTimestamp
DrawBoard_AllLabelsAreArabic                     ← semen/CSF بالعربية
DrawBoard_CheckFailure_RevertsCheckboxVisual     ← الحالة البصرية تتبع القاعدة
```

### خطوات التنفيذ

1. **احذف `DrawnAtUtc` من `MarkSampleDrawnCommand`** + عدّل `:200`. (فخّ كsilently).
2. أنشئ `Tube` + الإعداد + **الهجرة** + البذر.
3. أضف `PatientTest.TubeId`.
4. أنشئ CRUD `Tube` + تبويب المعمل.
5. أنشئ `GetPatientTubesForDrawQuery` + `TubeDrawRowDto`.
6. أعد كتابة `SampleDrawBoardView.xaml` صفوف أنابيب.
7. أنشئ `MarkTubeDrawnCommand` + handler ذرّي.
8. أنشئ `OtherTubeDialog` + ربط زر «أنبوب آخر».
9. أضف ComboBox الأنبوب في `PatientEditorView`.
10. **أصلح** `SampleDrawBoardViewModel`: `await` المُرَدّ، `CancellationToken`، DI.
11. ترجم `"semen"` و`"CSF"` للعربية في اللوحة وفي `WorkSheetPdfWriter.cs:209,214`.
12. نفّذ «بعد قراءة المعرّف» بعد WP-23.
13. اكتب الاختبارات السبعة عشر.

### الاعتماديات
**WP-01**. (WP-23 اختيارية كمُستفِدة.)

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | `TubeId` على جدول `PatientTest` (الأكبر) | nullable ⇒ لا كسر؛ الفهرس يُضاف معًا |
| تجربة مستخدم | تغيير شكل اللوحة كلياً | **اعرض النموذج على المالك قبل الدمج** |
| **تقنية** | فخّ `DateTime.MinValue` | **احذف المعامل** في الخطوة 1 — لا تعتمد على انتباه مطوّر مستقبلي |
| تقنية | `IsChecked` setter يُهمل `Task` | `async void` مع `try/catch` + رجوع الحالة المرئية عند الفشل |
| تجربة مستخدم | أسماء الأنابيب في البذرة | **قرار المالك** على القائمة |
| — | تجميع في الذاكرة على مريض واحد | مقبول وموثّق |

### معايير القبول
- [ ] اللوحة تعرض أنابيب لا صفوف اختبار
- [ ] وسم الأنبوب يوسم كل اختباراته في معاملة واحدة
- [ ] «أنبوب آخر» يعمل ولا يظهر في قائمة أنابيب المعمل
- [ ] `DrawnAt` محترم بقيمة حقيقية
- [ ] كل التسميات عربية
- [ ] فشل الوسم **يعيد** الحالة المرئية للخلف

### التراجع
جدول جديد + عمود nullable. **آمن.** استثناء: حذف `DrawnAtUtc` من الأمر في public API — لكنه داخلي للتطبيق.

---

### WP-26 — كاشف تسجيل الدخول/الخروج

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | تقرير موصوف كامل غير موجود؛ المتبقي `LastLoginAtUtc` فقط — بلا اسم جهاز ولا وقت خروج. |
| **المرجع** | R-66 · D-30 |
| **الحجم** | M (2–3 أيام) |
| **الهجرات** | **migration جديدة** — `AddLoginSessions` |
| **الاعتماديات** | لا شيء. |

### الحالة الحالية — MISSING FEATURE
`User.cs:28` `LastLoginAtUtc` فقط (يضبطه `RecordLogin`) · لا جدول جلسات · لا اسم جهاز · لا وقت خروج · لا كشف مطبوع.

### الحالة المستهدفة
كشف مطبوع بأعمدة `اسم الجهاز · وقت الخروج · تاريخ الخروج · وقت الدخول · تاريخ الدخول` عن فترة محددة ومستخدم اختياري.

### الملفات المتأثرة

```
يُضاف:
  src/TopLab.Domain/Users/LoginSession.cs
  src/TopLab.Application/Features/UsersAndPermissions/Commands/RecordLogin/*.cs      ← تعديل
  src/TopLab.Application/Features/UsersAndPermissions/Commands/RecordLogout/         ← جديد
  src/TopLab.Application/Features/UsersAndPermissions/Queries/GetLoginSessions/      ← جديد
  src/TopLab.Application/Features/UsersAndPermissions/Commands/PrintLoginSessionsReport/
  src/TopLab.Infrastructure/Persistence/Configurations/LoginSessionConfiguration.cs
  src/TopLab.Infrastructure/Printing/LoginSessionsPdfWriter.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddLoginSessions.cs
  src/TopLab.Presentation/Views/Users/LoginSessionsView.xaml
  src/TopLab.Presentation/ViewModels/Users/LoginSessionsViewModel.cs
  tests/TopLab.Application.Tests/Features/UsersAndPermissions/LoginSessionTests.cs
```

### المكونات

**Domain**
```csharp
public sealed class LoginSession : Entity
{
    public Guid Id { get; }
    public UserId UserId { get; }
    public string MachineName { get; private set; }   // ≤128، من Environment.MachineName
    public DateTime LoginAtUtc { get; private set; }
    public DateTime? LogoutAtUtc { get; private set; }
    public string? IpAddress { get; private set; }
    public bool IsOpen => LogoutAtUtc is null;
    public void Close(DateTime atUtc) { … }   // يرمي إن كانت مغلقة
}
```

**Application**
- `RecordLoginCommandHandler` — إضافة:
  ```csharp
  // 1) أغلق أي جلسة مفتوحة لنفس المستخدم  (دخول جديد على جهاز آخر)
  var open = await _db.Set<LoginSession>().Where(s => s.UserId == userId && s.LogoutAtUtc == null).ToListAsync(ct);
  foreach (var s in open) s.Close(now);
  // 2) أنشئ جلسة جديدة
  _db.Add(LoginSession.Create(userId, Environment.MachineName, now));
  ```
- `RecordLogoutCommand(UserId?)` — يُستدعى عند «قفل المحطة» (المزايا الإضافية القائمة في `ShellViewModel`) وعند الخروج.
- `GetLoginSessionsQuery(DateOnly From, DateOnly To, UserId? UserId = null)` + validator.
- **صلاحية:** `VIEW_LOGIN_SESSIONS` — **صلاحية منفصلة** (بيانات دخول حسّاسة، لا تُشارك صلاحية إدارة المستخدمين).
  > **[C6]** تتطلّب، مثل `PRINT_CARD`، تعديل `PermissionConfiguration.HasData` **و** زرع المنح القائمة في
  > `UserPermissionGrant` من `ADD_EDIT_PATIENT` قبل تفعيل الفحص. هذه ليست هجرة مستقلة — بل صفّان داخل `AddLoginSessions`.

**Infrastructure**
- هجرة `AddLoginSessions` + فهرس مركّب على `(UserId, LoginAtUtc)` + فهرس على `LogoutAtUtc` (للاستعلامات «الجلسات المفتوحة»).
- **القرار 8** — اقتطاع: `HostedService` يومي يحذف ما قبل 24 شهراً.

**Presentation**
- `LoginSessionsView` **خلف كلمة المرور الثانية** (نفس نمط `ShellViewModel.cs:233-246`).
- أعمدة: `اسم الجهاز · تاريخ الدخول · وقت الدخول · تاريخ الخروج · وقت الخروج · مدة الجلسة`.
- `LoginSessionsPdfWriter` بالأعمدة الخمسة الموصوفة + إجمالي المدة.

**Tests**
```
RecordLogin_CreatesSessionWithMachineName
RecordLogin_ClosesPreviousOpenSession
RecordLogin_RecordsTimestamp
RecordLogout_StampsLogoutTime
Logout_WithoutOpenSession_IsNoOp_NotError
GetLoginSessions_FiltersByPeriod
GetLoginSessions_FiltersByUser
GetLoginSessions_IncludesOpenSessions
GetLoginSessions_RequiresDedicatedPermission
ReLogin_CreatesNewSessionAndClosesPrevious
LoginSessions_Print_ShowsFiveColumns
Retention_PrunesOlderThan24Months          ← فقط إن نُفّذ القرار 8(a)
```

### خطوات التنفيذ

1. أنشئ `LoginSession` + الإعداد + الفهارس + **الهجرة**.
2. أضف `RecordLoginCommandHandler` (إغلاق + إنشاء).
3. أضف `RecordLogoutCommand` واربطه بـ «قفل المحطة» و«خروج».
4. أضف صلاحية `ViewLoginSessions` **وزرعها** (لا تُفعّلها بلا زرع).
5. أنشئ `GetLoginSessionsQuery` + validator.
6. أنشئ `LoginSessionsPdfWriter` + أمر الطباعة.
7. أنشئ `LoginSessionsView` خلف كلمة المرور الثانية.
8. نفّذ الاقتطاع (القرار 8).
9. اكتب الاختبارات.

### الاعتماديات
لا شيء.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | نموّ غير محدود لجدول الجلسات | **القرار 8** — اقتطاع دوري |
| أمني | بيانات دخول حسّاسة | صلاحية منفصلة + خلف كلمة المرور الثانية |
| تقنية | `Environment.MachineName` في طبقة Application | مرّره من Infrastructure ⇒ `IPdfPrinterDispatcher`-نمط: `IEnvironmentInfo.MachineName` في `Application/Common/Interfaces` |
| — | — | — |

> ⚠ **تصحيح معماري مهم:** لا تستدعِ `Environment.MachineName` داخل معالج Application — ناقص اختباراً ومربوط بالبيئة. عرّف `IEnvironmentInfo` في `Application/Common/Interfaces` ونفّذه في `Infrastructure`.

### معايير القبول
- [ ] كل دخول يُنشئ جلسة باسم الجهاز
- [ ] الخروج يُختم الجلسة
- [ ] الدخول الجديد يُغلق الجلسة القديمة
- [ ] الكشف يطبع بالأعمدة الخمسة
- [ ] الصلاحية المنفصلة مُطبَّقة ومزروعة

### التراجع
جدول جديد بلا مراجع. آمن تماماً (البيانات القديمة في `User.LastLoginAtUtc` تبقى).

---

### WP-27 — طباعة قائمة الأسعار والمضادات + لمسات الكتالوج والفوترة

| | |
|---|---|
| **الأولوية** | P2 |
| **سبب الأولوية** | أمرا طباعة موصوفان غائبان، وفلتر مفقود، وسعر إجمالي مفقود. كلّها صغيرة ومحدّدة. |
| **المرجع** | R-52, R-54, R-57, R-60, R-42 · D-20, D-21, D-22, D-32, D-08 |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **migration جديدة** — `AddCustomGroupTotalPrice` |
| **الاعتماديات** | **WP-01** (نمط الكاتب) · **WP-14** (حقول المضاد للطباعة الكاملة) |

### الحالة الحالية — MISSING FEATURE
مجموعة أوامر الطباعة سبعة فقط (Invoice, Receipt, Barcode, BlankReport, CombinedReport, HistoryReport, WorkSheet) — لا `PrintPriceList` ولا `PrintAntibiotic` · `PriceListsView.xaml:13-85` بلا زر طباعة · `CustomGroup` بلا سعر إجمالي · `GetSentOutSamplesQuery` بلا حالة «مرسل/غير مرسل» · معاينة ورقة العمل غير موجودة.

### الحالة المستهدفة
تُطبع قائمة الأسعار مجمّعةً بالمجموعة مع عدّاد وملاحظات و«صفحة X من Y»؛ وتُطبع قائمة المضادات؛ والمجموعة المخصصة لها سعر إجمالي؛ وشاشة العينات المرسلة فيها مرشّح مرسل/غير مرسل؛ ومعاينة ورقة العمل قبل الطباعة.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Tests/CustomGroup.cs                     ← + TotalPrice
  src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Commands/{Create,Update}CustomGroup/*.cs
  src/TopLab.Application/Features/SentOutSamples/Queries/GetSentOutSamples/GetSentOutSamplesQuery.cs
  src/TopLab.Application/Features/SentOutSamples/Queries/GetSentOutSamples/GetSentOutSamplesQueryHandler.cs
  src/TopLab.Presentation/Views/Patients/SentOutSamplesView.xaml
  src/TopLab.Presentation/ViewModels/WorkSheets/WorkSheetsViewModel.cs     ← معاينة
  src/TopLab.Presentation/Views/Lab/PriceListsView.xaml
  src/TopLab.Presentation/Views/Lab/AntibioticsView.xaml

يُضاف (Application):
  src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Common/CustomGroupPriceResolver.cs   ← [C12] جديد
  src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Queries/BuildPriceListReport/
  src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Commands/PrintPriceList/
  src/TopLab.Application/Features/CultureAndAntibiotics/Queries/BuildAntibioticList/
  src/TopLab.Application/Features/CultureAndAntibiotics/Commands/PrintAntibioticList/

يُضاف (Infrastructure):
  src/TopLab.Infrastructure/Printing/PriceListPdfWriter.cs
  src/TopLab.Infrastructure/Printing/AntibioticListPdfWriter.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddCustomGroupTotalPrice.cs

يُضاف (Tests):
  tests/TopLab.Infrastructure.Tests/Printing/PriceListPdfWriterTests.cs
  tests/TopLab.Application.Tests/Features/SentOutSamples/SentOutStateFilterTests.cs
```

### المكونات

**Domain** — `CustomGroup.TotalPrice` (`decimal?`).

**Application**

1. **`CustomGroupPriceResolver`** — دالة واحدة تُستعمل في التسجيل وتطبيق المجموعة:
   ```csharp
   public static decimal Resolve(CustomGroup group)
   {
       if (group.TotalPrice is { } t && t > 0) return t;
       return group.Items.Sum(i => i.Price);
   }
   ```
   ⇒ اجعلها **مصدر الحقيقة الوحيد** (لا منطق مكرر في `TestPriceResolver`).

2. **`SentOutState`** enum = `All | NotSent`:
   ```csharp
   public enum SentOutState : byte { All = 0, NotSent = 1 }
   // في المعالج:
   query.SentState == SentOutState.NotSent
       ? q.Where(s => !s.IsSent)
       : q;
   ```

3. **`BuildPriceListReportQuery(int priceListId)`**:
   ```csharp
   new PriceListReportDto(
       PriceListName,
       IReadOnlyList<PriceListGroupBlock> Groups);   // GroupName + Items + Count
   // Item: TestName, TestCode, Price, Barcode, CollectionNotes
   ```
   تجميع في الذاكرة على قائمة أسعار واحدة (محدودة الحجم) — مقبول.

4. **`PrintPriceListCommand(int PriceListId)`** + مدقّق + صلاحية.
5. **`BuildAntibioticListQuery()`** + **`PrintAntibioticListCommand()`** — يستهلك حقول WP-14 (يعمل بأسماء فقط إن لم يُنفَّذ WP-14).

**Infrastructure**
- `PriceListPdfWriter` على QuestPDF بنمط `WorkSheetPdfWriter` (المجموعات والجداول العربية):
  - ترويسة: اسم القائمة + التاريخ.
  - لكل مجموعة: رأس المجموعة + جدول (`الاسم · الكود · السعر · الباركود · ملاحظات`) + سطر «عدد الاختبارات: N».
  - تذييل: `صفحة X من Y`.
- `AntibioticListPdfWriter` — جدول (`الاسم · الاسم العلمي · الرمز · الأسماء التجارية · حمل؟ · أطفال؟`).
- هجرة `AddCustomGroupTotalPrice` + بذر `NULL` للربط.

**Presentation**
- `PriceListsView.xaml:13-85` — زر «طباعة القائمة».
- `AntibioticsView.xaml:32-34` — زر «طباعة القائمة».
- `SentOutSamplesView.xaml:12-17` — `RadioGroup` «الكل / غير مرسل» (المواصفة في `RLS_Learn` p.186).
- `WorkSheetsViewModel` — زر «معاينة» عبر `IPdfPreviewService` (WP-01).

**Tests**
```
PrintPriceList_GroupsByTestGroup                        ← حرج
PrintPriceList_ShowsItemCountPerGroup
PrintPriceList_ShowsPageXOfY
PrintPriceList_IncludesCollectionNotes
PrintPriceList_RequiresPermission
PrintAntibioticList_ListsAllAntibiotics
PrintAntibioticList_IncludesScientificAndCommercialNames  ← فقط بعد WP-14
CustomGroup_TotalPrice_OverridesItemSum
CustomGroup_NullTotalPrice_UsesItemSum
CustomGroup_Resolver_IsSingleSourceOfTruth
SentOut_Filter_NotSent_ReturnsOnlyUnsent
SentOut_Filter_All_ReturnsEverything
WorkSheet_Preview_CreatesFileBeforeDispatch
```

### خطوات التنفيذ

1. أضف `SentOutState` + enum + فلتر + RadioGroup — **أرخص بند، نفّذه أولاً**.
2. أضف `CustomGroup.TotalPrice` + **الهجرة**.
3. أنشئ `CustomGroupPriceResolver` واجعله المصدر الوحيد.
4. أنشئ `BuildPriceListReportQuery` (تجميع بالمجموعة + عدّاد + ملاحظات).
5. أنشئ `PriceListPdfWriter` + `PrintPriceListCommand` + زر.
6. أنشئ `BuildAntibioticListQuery` + `AntibioticListPdfWriter` + أمر + زر.
7. أضف معاينة ورقة العمل عبر `IPdfPreviewService`.
8. اكتب الاختبارات الأربعة عشر.

### الاعتماديات
**WP-01** · **WP-14** (اختياري لحقول المضاد).

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| بيانات | عمود واحد nullable | آمن |
| **تقنية** | منطق السعر المكرر في `TestPriceResolver` | `CustomGroupPriceResolver` كمصدر وحيد + اختبار |
| تجربة مستند | زرّا طباعة جديدان | في شريط الأدوات القائم |
| — | تجميع في الذاكرة | حجم قائمة أسعار محدود — موثّق |

### معايير القبول
- [ ] قائمة الأسعار تُطبع مجمّعة مع عدّاد و«صفحة X من Y»
- [ ] مرشّح «غير مرسل» يعمل
- [ ] السعر الإجمالي للمجموعة يُحترم عند ضبطه
- [ ] معاينة ورقة العمل تُنشئ ملفاً قبل الإرسال
- [ ] الاختبارات الأربعة عشر خضراء

### التراجع
عمود nullable + أمرا طباعة جديدان. آمن.

---

### WP-28 — صقل التكافؤ منخفض الشدة

| | |
|---|---|
| **الأولوية** | P3 |
| **سبب الأولوية** | لا يُفسد بيانات ولا يحجب تدفّقاً. تحسينات مطابقة فقط. |
| **المرجع** | R-06, R-11, R-56, R-62, R-63 · D-31 |
| **الحجم** | M (2–3 أيام) |
| **الهجرات** | **migration + seed** — `AddBreakHoursAndPermissions` |
| **الاعتماديات** | **WP-12** (الشاشات) · **WP-18** (نمط التأكيد) |

### الحالة الحالية — PARTIAL
`grep -rn "MouseDoubleClick|DoubleClick|MouseBinding" src/TopLab.Presentation` ⇒ **صفر** في كامل طبقة العرض ⇒ لا نقر مزدوج لإضافة/إزالة اختبار · `PermissionConfiguration.cs:17-31` 13 صلاحية؛ **لا** صلاحية منفصلة لطباعة الوصل ولا لعرض الدرج/الحسابات (يُعاد استخدام `CASH_DISBURSE_DEPOSIT`) · `User.BreakDurationMinutes` (`User.cs:23`) دقائق فقط لا ساعات+دقائق · `Test.IsCultureType` موجودة لكن **لا بذرة** لنطاق 118-139 تحت «CULTURE AND SENSITIVITY».

### الحالة المستهدفة
نقر مزدوج يضيف/يزيل؛ فترة الراحة بالساعات والدقائق؛ صلاحيتان منفصلتان؛ بذرة مزارع بالنطاق الموصوف.

### الملفات المتأثرة

```
يُعدَّل:
  src/TopLab.Domain/Users/User.cs
  src/TopLab.Application/Features/UsersAndPermissions/Commands/{Create,Update}User/*.cs
  src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Configurations/UserConfiguration.cs
  src/TopLab.Presentation/Views/Users/UserManagementView.xaml          ← :25-66
  src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml       ← :457-469
  src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml.cs

يُضاف:
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddBreakHoursAndPermissions.cs
  tests/TopLab.Domain.Tests/Users/BreakPeriodTests.cs
  tests/TopLab.Application.Tests/Features/UsersAndPermissions/ReceiptPrintPermissionTests.cs
```

### المكونات

**Domain**
- `User.BreakDurationHours` (`int`, افتراضي `0`) — **يُقرأ مع `BreakDurationMinutes`** (لا تستبدل). `TotalMinutes => Hours * 60 + Minutes`.
- رموز صلاحية جديدة في `PermissionConfiguration`:
  ```
  PRINT_RECEIPT      "طباعة وصل استلام"
  VIEW_DRAWER        "عرض درج النقدية والحسابات"
  ```
  (الأسماء تتبع نمط `PermissionConfiguration.cs:17-31` القائم — **اقرأه وتحاكِ نمطه الحرفي**.)

**Application**
- `Create/UpdateUserCommand` + `BreakDurationHours`.
- `PrintReceiptCommand` — `IAuthorizedRequest(Permissions.PrintReceipt)` **بدلاً** من صلاحية تسجيل المريض.
- تبويب/مسار الدرج — `IAuthorizedRequest(Permissions.ViewDrawer)` في `GetCashDrawerInventoryQuery`.
- `IsAbsolutePermission` يكفي للأمن (المدير).

**Infrastructure**
- **الترتيب حاسم في الزرع:**
  1. الهجرة تنشئ الرمزين في جدول `Permissions`.
  2. **في نفس المعاملة** — *بصيغة صحيحة، انظر التصحيح أدناه*.
  3. **ثم** فقط يُفعَّل الفحص في `AuthorizationBehavior`.
  - ⇒ **لا انقطاع للمستخدمين الحاليين.**

> ⚠ **[C1] تصحيح جوهري — الصياغة السابقة كانت تُقفل كل المستخدمين.**
> جدول المنح هو `UserPermissionGrants` (**بالجمع**) — see `BaselineDataModel.cs:507`
> `name: "UserPermissionGrants"` و`FK_UserPermissionGrants_Users_UserId` (:517)
> و`ApplicationDbContextModelSnapshot.cs:1949` `b.ToTable("UserPermissionGrants")`.
> الصنف البرمجي `UserPermissionGrant` **بالمفرد** واسم الجدول **بالجمع**؛ لا يوجد أي
> جدول بالمفرد في أي هجرة. المفتاح مركّب `UserId + PermissionId`، و**لا يملك عمود
> `PermissionCode` أصلاً**؛ و`PRINT_PATIENT` **ليس** من الرموز الـ13 المزروعة في
> `PermissionConfiguration.cs:17-31` (الرمز الفعلي `ADD_EDIT_PATIENT`). أي أن جملة `INSERT`
> بصيغتها السابقة كانت سترمي `Invalid object name` فلا تُدرج **ولا صفاً واحداً**،
> ثم تُفعَّل الفحوص ⇒ فقدان وصول شامل لكل المستخدمين.

> ⚠ **[C6] أربعة رموز لا ثلاثة.** الرموز الجديدة المطلوبة هي:
> `PRINT_RECEIPT` و`VIEW_DRAWER` (هنا في WP-28)، و`PRINT_CARD` (يستهلكه WP-22)،
> و`VIEW_LOGIN_SESSIONS` (يستهلكه WP-26). ولأن **WP-22 وWP-26 يستهلكان رمزيْهما**،
> يجب أن يسبق هذا البروتوكول حزمتَيْهما؛ ولذلك نُقلت WP-28 إلى **الموجة 3**
> في §5.2/§5.3 (انظر هناك).

| # | المرحلة | التفصيل |
|---|---|---|
| 1 | إضافة الرموز | عدّل `PermissionConfiguration.HasData` ليضيف **أربعة** رموز: `Id=14 PRINT_RECEIPT` · `Id=15 VIEW_DRAWER` · `Id=16 PRINT_CARD` · `Id=17 VIEW_LOGIN_SESSIONS` (الحدّ 50 حرفاً للرمز، 300 للوصف). |
| 2 | زرع المنح | `INSERT INTO UserPermissionGrants (UserId, PermissionId) SELECT g.UserId, 14 FROM UserPermissionGrants g WHERE g.PermissionId = 1` — المصدر `ADD_EDIT_PATIENT`. ومثلها `PermissionId = 11` (`CASH_DISBURSE_DEPOSIT`) ← `VIEW_DRAWER` (15). ولـ`PRINT_CARD` (16) و`VIEW_LOGIN_SESSIONS` (17) زرعٌ من المصدر نفسه `ADD_EDIT_PATIENT`. |
| 3 | تفعيل الفحص | **فقط بعد** نجاح 1 و2 — وداخل الحزمة نفسها، لا في حزمة لاحقة. |

**تحقّق إلزامي قبل المتابعة:**
```sql
SELECT PermissionId, COUNT(*) FROM UserPermissionGrants
WHERE PermissionId IN (14, 15, 16, 17) GROUP BY PermissionId;   -- يجب ألا يعود أي صف فارغاً
```
كما يجب أن يترجم `AuthorizationBehavior` غياب المنحة إلى الرسالة العربية الموصوفة
(`AuthorizationBehavior.cs:34-50`) بدل رمي استثناء خام.
- `BreakDurationHours` عمود `int` + بذر `0`.
- **بذر أنواع المزارع** 118-139: يكفي **صف واحد** يُعلّم `IsCultureType` (الراية موجودة) — لا بذرة 22 اختباراً. **قرار المالك** إن أراد بذرة كاملة. seeder صغير في نفس الهجرة.

**Presentation**
- `UserManagementView.xaml:25-66` — خانتا الساعات والدقائق جنباً إلى جنب.
- `UserManagementView` — صندوقا `PRINT_RECEIPT` و`VIEW_DRAWER` في قائمة الصلاحيات.
- `PatientEditorView.xaml:457-469` — `MouseDoubleClick` على شبكتَي الاختبارات (إضافة/إزالة). **أبقِ الأزرار** — النقر المزدوج تحسين لا بديل.

**Tests**
```
BreakPeriod_TotalMinutes_CombinesHoursAndMinutes
BreakPeriod_ZeroHours_UsesMinutesOnly
BreakPeriod_SaveAndReload
CreateUser_WithReceiptPrintPermission_Persists
PrintReceipt_RequiresReceiptPrintPermission          ← حرج
PrintReceipt_DeniedUser_GetsArabicMessage
DrawerView_RequiresDrawerViewPermission
DrawerView_DeniedUser_GetsArabicMessage
AbsolutePermission_BypassesNewPermissions
PermissionSeed_ExistingUsersRetainAccess              ← حرج: لا انقطاع
PermissionSeed_GrantsRowsForNewCodes                  ← حرج: UserPermissionGrant لا صف فارغ
PermissionSeed_SourceCodes_AreAddEditPatientAndCashDisburse
Authorization_FallsBackToArabicMessageOnMissingGrant ← لا استثناء خام
DoubleClick_OnTestList_AddsTest
DoubleClick_OnSelectedTests_RemovesTest
Buttons_StillWork_ForUsersWithoutDoubleClick
CultureSeed_MarksRange118To139
```

### خطوات التنفيذ

1. **نفّذ الزرع أولاً** (خطوتان 1+2 في هجرة واحدة) — قبل أي فحص.
2. أضف `BreakDurationHours` + `TotalMinutes`.
3. أضف حقتي الساعات والدقائق في الواجهة.
4. **بعد نجاح الزرع** — فعّل `PrintReceipt` `IAuthorizedRequest`.
5. **بعد نجاح الزرع** — فعّل فحص الدرج.
6. أضف صندوقَي الصلاحيتين.
7. نفّذ النقر المزدوج في `PatientEditorView` (منطق بسيط في `.xaml.cs` بنداء الأمر — **لا** منطق أعمال).
8. بذر رايات المزارع.
9. اكتب الاختبارات الخمسة عشر.

### الاعتماديات
**WP-12** · **WP-18**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | تفعيل فحص قبل الزرع ⇒ **انقطاع كل المستخدمين** | ثلاث مراحل مرقّمة أعلاه، وزرع المنح عبر `UserPermissionGrant (UserId, PermissionId)` بمصدرين صحيحَين. **الترتيب غير قابل للتفاوض**، واستعلام التحقق إلزامي |
| تجربة مستخدم | نقر مزدوج قد يفتح نافذة تعديل بالخطأ | اختبر على ListBox/DataGrid المحدد |
| بيانات | بذر 22 اختباراً قد يتعارض مع اختبارات قائمة | افحص النطاق 118-139 أولاً؛ **إن كان مشغولاً ⇒ احذف البذر وسجّل** |

### معايير القبول
- [ ] النقر المزدوج يضيف ويزيل
- [ ] الأزرار تعمل أيضاً (لم تُحذف)
- [ ] القطع بدون صلاحية يُرفض بالرسالة العربية الموصوفة
- [ ] الأدمن الحالي لا يفقد شيئاً بعد الزرع
- [ ] فترة الراحة بالساعات تعمل

### التراجع
**التراجع الآمن: أزل الصندوقين من الواجهة أولاً، ثم أزل الفحص، ثم احذف الرموز.** التراجع العكسي (حذف الرموز أولاً) يترك فحصاً يشير إلى رمز غير موجود.

---

### WP-29 — سلامة البيانات وقابلية التشخيص

| | |
|---|---|
| **الأولوية** | P1 |
| **سبب الأولوية** | تدفّقات متعددة الأوامر بلا معاملة تُترك نصف مُطبَّقة عند الفشل. والاستثناءات المبلَعة تترك أثراً صفرياً عند الانهيار — وهو سبب بقاء D-01 مخفياً months. |
| **المرجع** | D6, D3, D5 في تقرير الوكيل الفرعي الأول · §8.2 من التدقيق |
| **الحجم** | M (3 أيام) |
| **الهجرات** | **none** (تأكيد الفهارس فقط) |
| **الاعتماديات** | **WP-01** · **WP-06** · **WP-12** |

### الحالة الحالية
| الملاحظة | الدليل | التصنيف |
|---|---|---|
| **[C14] استرجاع المعرّفات عبر `Take()` فوق كامل اختبارات المريض** | `AddTestsToVisitCommandHandler.cs:112` `createdIds.Add(0)` عنصر نائب، ثم `:117-122` استعلام ثانٍ بـ `OrderByDescending(pt.Id).Take(createdIds.Count)` على **كل** `PatientTest` للمريض. تحت أي إدراج متزامن تُعاد معرّفات من زيارة أخرى، و`PatientEditorViewModel.ApplyTestDeltasAsync` يطمس بها جيرانه | **PROVED DEFECT** |
| سلسلة تعديلات الزيارة بلا معاملة | `PatientEditorViewModel.cs:918-976` `ApplyTestDeltasAsync` — إزالة ثم إضافة ثم تحديث رايات، كلٌّ بـ `SaveChangesAsync` خاص | **PROVED DEFECT** |
| نفس المشكلة في الحالات الطبية | `:883-916` `ApplyConditionDeltasAsync` | PROVED DEFECT |
| تسوية بلا قفل | `SettleAccountInFullCommandHandler.cs:117-123` قراءة-ثم-كتابة ⇒ تساقطان متزامنان يُدخلان دفعتين | **POTENTIAL RISK** |
| استثناءات مبلَعة بلا تسجيل | `ReportPrintingService.cs:73` · `BarcodeService.cs:80-84` · `ExportPatientReportPdfCommandHandler.cs:161-165`. `IAppLogger` **موصول** في `DependencyInjection.cs:102` — النقص أن هذه الخدمات لا تحقنه | PROVED DEFECT |
| ملفات `%TEMP%` تتراكم | `ReportPrintingService.cs:69` · `BarcodeService.cs:76` «intentionally left in the OS temp directory» | PROVED DEFECT |
| جداول كاملة في الذاكرة | `PatientBillingReader.cs:141,158` · `GetResultEntryQueryHandler.cs:104` · `GetResultWorklistQueryHandler.cs:37` · `GetCombinableTestsQueryHandler.cs:194` · `BuildCombinedReportCommandHandler.cs:60,86` · `GetCultureReportQueryHandler.cs:28` · `GetPatientTestsForDrawQueryHandler.cs:45` · `GetPatientTestAuditQueryHandler.cs:61` | **POTENTIAL RISK** (الأثر يعتمد على حجم البيانات) |

### الحالة المستهدفة
سلسلة تعديلات الزيارة تُطبَّق كاملةً أو لا شيء؛ ولا يمكن إدخال دفعتين بتسوية واحدة؛ وكل فشل طباعة يترك أثراً في السجل؛ ولا ملفات PDF قديمة تتراكم؛ وقواعد القراءة الساخنة لا تحمّل جداول كاملة.

### الملفات المتأثرة

```
يُضاف:
  src/TopLab.Application/Common/Interfaces/IAppUnitOfWork.cs
  src/TopLab.Infrastructure/Persistence/AppUnitOfWork.cs
  src/TopLab.Application/Features/ResultsEntry/Commands/ApplyVisitDeltas/
  src/TopLab.Application/Features/PatientRegistration/Commands/ApplyConditionDeltas/
  src/TopLab.Infrastructure/Hosting/TempPdfCleanupService.cs
  src/TopLab.Infrastructure/Logging/PrintingDiagnostics.cs
  src/TopLab.Application/Common/Interfaces/IEnvironmentInfo.cs        ← (من WP-26)

يُعدَّل:
  src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs   ← :918-976, :883-916
  src/TopLab.Application/Features/PatientBilling/Commands/SettleAccountInFull/*.cs
  src/TopLab.Application/Common/Behaviors/*  أو Infrastructure DI
  src/TopLab.Infrastructure/Printing/ReportPrintingService.cs
  src/TopLab.Infrastructure/Barcode/BarcodeService.cs
  src/TopLab.Presentation/DependencyInjection.cs

يُضاف (Tests):
  tests/TopLab.Application.Tests/Common/VisitDeltasAtomicityTests.cs
  tests/TopLab.Application.Tests/Features/PatientBilling/ConcurrentSettleTests.cs
  tests/TopLab.Infrastructure.Tests/Services/TempPdfCleanupServiceTests.cs
```

### المكونات

**Domain** — لا تغيير.

**Application**
- `IAppUnitOfWork` في `Application/Common/Interfaces`:
  ```csharp
  public interface IAppUnitOfWork
  {
      Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
  }
  ```
  **لا** تسريب `DbContext` إلى طبقة Application.
- `ApplyVisitDeltasCommand` — **أمر واحد** يحلّ محل ثلاثة:
  ```csharp
  public sealed record ApplyVisitDeltasCommand(
      int VisitId,
      IReadOnlyList<int> RemovePatientTestIds,
      IReadOnlyList<TestOrderLine> AddLines,
      IReadOnlyList<TestFlagUpdate> FlagUpdates) : IAuthorizedRequest { … }
  ```
  المعالج: حذف الإضافات، ثم إضافة المحذوفات، ثم تحديث الرايات، **ثم `SaveChangesAsync` واحد داخل معاملة**.
- `ApplyConditionDeltasCommand` — نفس النمط.
- `SettleAccountInFullCommandHandler` — معاملة + قفل صفّي:
  ```csharp
  // خامس داخل معاملة، على VisitId
  var visit = await _db.Set<PatientTest>()
      .FromSqlInterpolated($"SELECT * FROM PatientTests WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}")
      .FirstOrDefaultAsync(ct);
  ```
  ⇒ **يتطلب فهرساً على `Id`** (موجود كمفتاح أساسي ⇒ كافٍ).

**Infrastructure**
- `AppUnitOfWork` — يفتح معاملة على `ApplicationDbContext` (لأن `IApplicationDbContext` لا يكشف `Database`).
- تسجيل `TempPdfCleanupService : BackgroundService` — يمسح الملفات الأقدم من 24 ساعة **من مجلد TopLab المؤقت الخاص** (وليس `%TEMP%` كله).
- توصيل `ILogger` إلى `ReportPrintingService` و`BarcodeService` و`ExportPatientReportPdfCommandHandler`.
- **استثناء مهم:** `OperationCanceledException` **لا يُسجَّل كخطأ** (سلوك صحيح حالياً في `BarcodeService.cs:76-79` — حافظ عليه).

**Presentation** — لا تغيير منطقي؛ الأخطاء تُعرض كما هي.

**Tests** — تحتاج fakes يدوية، **بلا مكتبة mocking**:
```
AddTestsToVisit_ReturnsIdsOfRowsItInsertedOnly        ← [C14] حرج
AddTestsToVisit_NoTakeQuery_NoConcurrentInsertRace    ← [C14] حرج
ApplyVisitDeltas_AppliesAllChangesInOneSave
ApplyVisitDeltas_AddFailure_RollsBackRemovals        ← حرج
ApplyVisitDeltas_UpdateFailure_RollsBackAdditions    ← حرج
ApplyConditionDeltas_RollsBackOnFailure
Settle_ConcurrentCalls_InsertOnePayment              ← اختبار مهم
Settle_ReadsBalanceUnderRowLock
PrintingService_CaughtException_IsLoggedWithTypeAndMessage
PrintingService_OperationCanceled_NotLoggedAsError
TempCleanup_RemovesOnlyFilesOlderThan24Hours
TempCleanup_LeavesRecentFiles
TempCleanup_IgnoresOtherDirectories
```

### خطوات التنفيذ

1. أنشئ `IAppUnitOfWork` + `AppUnitOfWork` + تسجيل DI.
2. **أولاً** لُفّ `ApplyTestDeltasAsync` و`ApplyConditionDeltasAsync` بمعاملة (تغيير أدنى، يحل 80% من المشكلة).
3. **ثم** استخرج `ApplyVisitDeltasCommand` كحلّ دائم — واستبدل التمحور.
3b. **[C14] أصلح استرجاع المعرّفات.** سجّل المعرّفات من `ChangeTracker` **قبل** `SaveChangesAsync`:
```csharp
var pending = new List<int>(toAdd.Count);
foreach (var pt in toAdd) { _db.Add(pt); pending.Add(pt.Id.Value); }   // EF يولّد القيمة
await _db.SaveChangesAsync(cancellationToken);
return Result<IReadOnlyList<int>>.Success(pending);   // لا استعلام ثانٍ، لا Take()
```
يزيل `createdIds.Add(0)` و`Take()` معاً، ويقضي على سباق الإدراج المتزامن في الوقت نفسه.
4. ضع `SettleAccountInFullCommandHandler` في معاملة مع `UPDLOCK, HOLDLOCK`.
5. **وصّل `ILogger`** إلى خدمات الطباعة الثلاث.
6. استثنِ `OperationCanceledException` من التسجيل كخطأ.
7. أنشئ `TempPdfCleanupService` بمجلد TopLab خاص.
8. حسّن القراءات الساخنة: `PatientBillingReader.cs:141,158` و`GetResultWorklistQueryHandler.cs:37` و`GetCombinableTestsQueryHandler.cs:194` و`BuildCombinedReportCommandHandler.cs:60,86` — أضِف `Where` بدل التحميل الكامل، أو استخدم join/project.
9. اكتب الاختبارات العشرة — **اختبارا التراجع هما الأهم**.

### الاعتماديات
**WP-01** · **WP-06** · **WP-12**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **بيانات** | تغيير سلوك المعاملات يكشف أخطاء كانت مخفية | اختبارات rollback إلزامية قبل الدمج |
| تقنية | `FromSqlInterpolated` على `PatientTests` قد يتعارض مع_tracking | `AsNoTracking` بعد التحميل الصريح |
| تقنية | خدمات طباعة «لا ترمي» بحسب عقدها | **حافظ على العقد** — سجّل وأعِد `Error` |
| بيانات | `TempPdfCleanupService` قد يحذف ملفاً قيد الفتح | افحص زمن آخر الكتابة (`LastWriteTimeUtc`) |
| — | تحسين القراءات قد يغيّر النتائج | اختبار لكل استعلام |

### معايير القبول
- [ ] اختبارا التراجع خضراء (فشل في المنتصف ⇒ صفر تغيير)
- [ ] اختبار التزامن يُدخل دفعة واحدة
- [ ] كل فشل طباعة يظهر في السجل بنوعه ورسالته
- [ ] الملفات الأقدم من 24 ساعة تُنظَّف، الحديثة لا تُمسّ
- [ ] لا تحميل جدول كامل في القراءات الساخنة المذكورة

### التراجع
`AppUnitOfWork` اختياري (لا يكسر شيئاً بإزالته). قفل الصفوف قد يحتاج تعطيلاً بتلميح. `TempPdfCleanupService` قابل للإيقاف بإزالة التسجيل. **آمن.**

---


### WP-30 — مسار تعديل للنتائج البسيطة والمزارع + صقل أمان التعديل

| | |
|---|---|
| **الأولوية** | **P1** |
| **سبب الأولوية** | نتيجة مطبوعة من نوع بسيط أو مزرعة **مقفولة نهائياً**: لا مسح، لا إلغاء مراجعة، لا تعديل، لا سجل. مسح خاطئ يُتلف السجل الطبي ولا يمكن التراجع عنه. |
| **المرجع** | R-16, R-28 (سجل التدقيق) · D-10 · Cap 10 في تقرير الوكيل الفرعي الأول |
| **الحجم** | M (3–4 أيام) |
| **الهجرات** | **migration جديدة** — `AddResultAmendments` (بند 19 في ملحق ج) |
| **الاعتماديات** | **WP-02** (الشاشات)، **WP-12** (شبكة النتائج). **مطلوب قبل** الإغلاق النهائي. |

### الحالة الحالية — PROVED DEFECT

سلسلة القفل في `Domain/Results/PatientTest.cs`:

| العملية | السطر | الحارس | الأثر |
|---|---|---|---|
| `EnterResult` | `:117-119` | يرمي إن `IsReviewed` | لا إدخال بعد المراجعة |
| `Unreview` | `:145-147` | يرمي إن `IsPrinted \|\| IsDelivered` | **لا رجعة بعد الطباعة** |
| `ClearResult` | `:131-133` | يرمي إن `IsReviewed \|\| IsPrinted \|\| IsDelivered` | لا مسح |

**والتعديل موجود لنوع واحد فقط:** `ProfileResultAmendment.cs:17` مرتبط بـ `ProfileResultItemId`، والأمر الوحيد
`Features/ProfileResults/Commands/AmendProfileResult/`. لا `AmendSimpleResult` ولا `AmendCultureResult`.

**النتيجة:** 2 من 3 أنواع نتائج — البسيط والمزرعة — **بلا أي مسار تصحيح بعد الطباعة**.
وكما نصوصت في WP-12، الرسالة التي كان يعرضها زر «مسح» كانت تشير إلى باب غير موجود لهذين النوعَين.

### الحالة المستهدفة
لكل نوع نتيجة (بسيط · صنف · مزرعة) مسار تعديل كامل: حفظ القيمة القديمة والجديدة، ومُنشئ التعديل ووقته وسببه، وظهوره في تقرير الطباعة، وسجل قابل للعرض.

### الملفات المتأثرة

```
يُضاف (Domain):
  src/TopLab.Domain/Results/ResultAmendment.cs
  src/TopLab.Domain/Results/AmendableResultKind.cs          ← Simple | Profile | Culture

يُعدَّل (Application) — نصوص الترجمة العربية:
  src/TopLab.Application/Features/ResultsEntry/Common/DomainFailureTranslator.cs

يُضاف (Application):
  src/TopLab.Application/Features/ResultsEntry/Commands/AmendResult/*.cs
  src/TopLab.Application/Features/ResultsEntry/Queries/GetResultAmendments/*.cs

يُضاف (Infrastructure):
  src/TopLab.Infrastructure/Persistence/Configurations/ResultAmendmentConfiguration.cs
  src/TopLab.Infrastructure/Persistence/Migrations/<ts>_AddResultAmendments.cs
  src/TopLab.Infrastructure/Printing/ReportDocument.cs     ← قسم التعديل في التقرير

يُضيف (Presentation):
  src/TopLab.Presentation/Views/Patients/AmendDialogView.xaml   ← [C12] جديد — نافذة التعديل، تستخدم AmendDialogViewModel القائم

يُعدَّل (Presentation):
  src/TopLab.Presentation/Views/Patients/SimpleResultEntryView.xaml
  src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml
  src/TopLab.Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs
  src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
```

### المكونات

**Domain**
```csharp
public sealed class ResultAmendment : Entity      // غير قابل للتحديث — مثل ProfileResultAmendment
{
    public PatientTestId PatientTestId { get; }
    public AmendableResultKind Kind { get; }      // Simple | Profile | Culture
    public string? OldResultValue { get; }
    public string? NewResultValue { get; }
    public string? OldUnit { get; }
    public string? NewUnit { get; }
    public int? OldFlag { get; }
    public int? NewFlag { get; }
    public int AmendedByUserId { get; }
    public DateTime AmendedAtUtc { get; }
    public string Reason { get; }                 // 500 حرف، إلزامي
    public static ResultAmendment Create(…);
}
```

**Application**
- `AmendResultCommand(PatientTestId, Kind, NewResultValue, NewUnit?, NewFlag?, Reason)`
  — `IAuthorizedRequest(EditResults)`.
- **صلاحية منفصلة [C4]:** `ProfileResultAmendment` يستعمل اليوم `EditResults` نفسها بلاصلاحية تعديل مميزة
  (`AmendProfileResultCommand.cs:16`). أي من يدخل نتيجة يستطيع إعادة كتابة التاريخ المطبوع.
  أضف `AMEND_RESULTS` واربطها، أو — إن كان الفصل مرفوضاً — وثّق القرار صراحةً في §6.4.
- التحقق من `NewFlag ∈ [0..2]` (الصنف لا يقبل 3) —closes the `AmendProfileResultCommandHandler.cs:50` gap.
- `PatientTest` يحتاج `Amend(resultValue, flag, …)` يسمح به بعد `IsPrinted` — **حارس جديد مستقل** عن `Unreview()`.
  **لا** تُرخِّ `Unreview()`: قفل التسليم يجب أن يبقى كما هو.
- `GetResultAmendmentsQuery(PatientTestId)` لعرض السجل.
- **ترجمة الأخطاء:** `AmendProfileResultCommandHandler.cs:76` يُعيد `ex.Message` الخام بالإنجليزية إلى
  واجهة عربية. مرِّره عبر `DomainFailureTranslator` (النمط المستخدم في `ProfileResults/Common/`).

**Infrastructure**
- هجرة `AddResultAmendments` (جدول جديد + فهرس على `PatientTestId`).
- `ReportDocument` — قسم «تصحيحات» يظهر فقط إن وُجدت تعديلات، يفصله خط.

**Presentation**
- زر «تعديل» + نافذة `AmendDialog` في `SimpleResultEntryView` و`CultureEntryView` (مُنمَّطة أصلاً).
- «سجل التعديلات» بجانب «سجل المراجعة» القائم في `ProfileEntryView.xaml`.
- **WP-12 يحدّث الرسالة** ليشير إلى هذا المسار بعد دمجه.

**Tests**
```
AmendResult_Simple_PersistsOldAndNew
AmendResult_Culture_PersistsOldAndNew
AmendResult_WorksAfterPrinted
AmendResult_RequiresReason
AmendResult_RejectsOutOfRangeFlag            ← يغلق فجوة :50
AmendResult_ReturnsArabicError_NotRawException   ← يغلق فجوة :76
AmendResult_RequiresDedicatedPermission
Unreview_StillThrowsAfterPrinted            ← القفل لم يُرخَّ
GetResultAmendments_ReturnsChronological
Report_ShowsAmendmentSectionOnlyWhenPresent
Report_AmendmentSection_ListsReason
Amendment_Immutable_NoUpdateNoDelete
ClearResult_StillThrowsAfterReviewed        ← لم يُرخَّ
```

### خطوات التنفيذ

1. أنشئ `ResultAmendment` + `AmendableResultKind` + الإعداد + **الهجرة**.
2. أضف `PatientTest.Amend(…)` بحارس مستقل — **اترك `Unreview()` و`ClearResult()` كما هما**.
3. أنشئ `AmendResultCommand` + validator + معالج (Amendment + تغيير النتيجة في `SaveChangesAsync` واحد).
4. أضف صلاحية `AMEND_RESULTS` **أو** وثّق رفض الفصل (ببروتوكول الزرع نفسه).
5. أغلق فجوة النطاق على `Flag` في `AmendProfileResultCommandHandler.cs:50`.
6. وجّه `:76` عبر `DomainFailureTranslator`.
7. أنشئ `GetResultAmendmentsQuery` + نافذة السجل.
8. اربط الزر في الشاشتين.
9. أضف قسم التصحيحات في التقرير.
10. **عدّل رسالة WP-12** لتشير إلى «تعديل (Amend)» بثقة.
11. اكتب الاختبارات الثلاثة عشر — وخصوصاً `Unreview_StillThrowsAfterPrinted` و`ClearResult_StillThrowsAfterReviewed`
    (**إثبات أن القفل لم يُفتح**).

### الاعتماديات
**WP-02** · **WP-12**.

### المخاطر
| النوع | المخاطرة | التخفيف |
|---|---|---|
| **سلامة سريرية** | فتح التعديل على نتيجة مسلَّمة تسمح بتغيير ما رآه المريض | اسمح على `IsPrinted`، **ممنوع** على `IsDelivered`؛ والتقرير المعاد طباعته يحمل قسم التصحيحات |
| **تدقيق** | تخفيف شرط التعديل يفتح باب إعادة الكتابة | `AMEND_RESULTS` منفصلة + `Reason` إلزامي + سجل غير قابل للتعديل |
| تجربة مستخدم | ثلاث شاشات بزر إضافي | الزر في شريط الأدوات مع «سجل التعديلات» |
| بيانات | جدول جديد | لا مراجع |

### معايير القبول
- [ ] نتيجة بسيطة مطبوعة قابلة للتعديل مع سبب
- [ ] نتيجة مزرعة مطبوعة قابلة للتعديل
- [ ] `Unreview` و`ClearResult` **ما زالا يرميان** — القفل لم يُفتح
- [ ] التعديل يظهر في التقرير المعاد طباعته
- [ ] `Reason` إلزامي، والنطاق المرجعي، والرسالة عربية

### التراجع
جدول جديد بلا مراجع. آمن. **الفصل في الصلاحية اختياري** ويسهل التراجع عنه.

---

## 5) تسلسل التنفيذ المقترح

> **[C19] هذا القسم مولَّد من جدول واحد (`D` في `§5.2`) ولا يُحرَّر يدوياً**
> — مُصحَّح بالكامل بعد جولة التحقق. الأرقام فيه متسقة بالضرورة مع بعضها.

### 5.1 المسار الحرج

```
WP-01 (ReportPdfWriter على QuestPDF + معاينة)          ← جذر كل طباعة
   │
   ├─► WP-06 (منسّق الطباعة الصادق)  ─┐
   ├─► WP-07 (تعليقات المدى)  [مُدمج في WP-01]
   ├─► WP-10 (تقارير التاريخ)        │
   ├─► WP-13 (خيارات التقرير المركّب)│
   ├─► WP-14 (عمق المزرعة)          │
   ├─► WP-20 (إعدادات التقرير)      │
   ├─► WP-22 (المظروف + الكارنيه)  │
   ├─► WP-27 (قائمة الأسعار)       │
   └─► WP-29 (سلامة البيانات) ◄────┘

WP-02 (إدخال النتائج) ─► WP-11, WP-12, WP-13, WP-30
```

### 5.2 جدول الباينة — المصدر الوحيد للأعداد

| WP | الأولوية | الحجم | أيام | الموجة | المجموعة |
|---|---|---|---|---|---|
| **WP-01** | P0 | XL | 8 | 0 | B |
| **WP-02** | P0 | S | 2 | 1 | A |
| **WP-03** | P0 | S | 2 | 1 | F |
| **WP-04** | P0 | M | 4 | 1 | D |
| **WP-05** | P0 | S | 1 | 1 | A |
| **WP-06** | P0 | M | 3 | 2 | B |
| **WP-07** | P0 | S | 1 | 2 | B |
| **WP-08** | P1 | M | 3 | 3 | C |
| **WP-09** | P1 | M | 4 | 3 | G |
| **WP-10** | P1 | L | 5 | 2 | B |
| **WP-11** | P1 | S | 1 | 3 | A |
| **WP-12** | P1 | M | 3 | 3 | A |
| **WP-13** | P1 | M | 3 | 2 | B |
| **WP-14** | P1 | L | 6 | 2 | B |
| **WP-15** | P1 | L | 5 | 1 | E |
| **WP-16** | P1 | M | 4 | 3 | E |
| **WP-17** | P1 | M | 3 | 3 | C |
| **WP-18** | P1 | L | 5 | 3 | D |
| **WP-19** | P1 | M | 4 | 3 | E |
| **WP-20** | P2 | L | 4 | 4 | B |
| **WP-21** | P2 | M | 3 | 4 | B |
| **WP-22** | P2 | M | 4 | 4 | G |
| **WP-23** | P2 | M | 4 | 4 | C |
| **WP-24** | P2 | M | 3 | 4 | D |
| **WP-25** | P2 | L | 5 | 4 | A |
| **WP-26** | P2 | M | 3 | 4 | G |
| **WP-27** | P2 | M | 3 | 4 | B |
| **WP-28** | P3 | M | 3 | 3 | A |
| **WP-29** | P1 | M | 3 | 2 | H |
| **WP-30** | P1 | M | 4 | 3 | A |

**مجموع أيام العمل: 106 يوم-عمل** (30 حزمة)

### 5.3 الموجات
**الموجة 0 — الأساس — مورد واحد يستهلكه كل من يطبع** — 8 يوم-عمل
`WP-01`

**الموجة 1 — إحياء المسارات المكسورة — أرخص موجة في المشروع** — 14 يوم-عمل
`WP-02 · WP-03 · WP-04 · WP-05 · WP-15`

**الموجة 2 — توصيل الطباعة بالمعنى — «يطبع» بدل «يضع علامة طباعة»** — 21 يوم-عمل
`WP-06 · WP-07 · WP-10 · WP-13 · WP-14 · WP-29`

**الموجة 3 — الاتساع — النموذج والواجهة والاستعلامات + زرع الصلاحيات** — 34 يوم-عمل
`WP-08 · WP-09 · WP-11 · WP-12 · WP-16 · WP-17 · WP-18 · WP-19 · WP-28 · WP-30`

**الموجة 4 — المخرجات والضبط** — 29 يوم-عمل
`WP-20 · WP-21 · WP-22 · WP-23 · WP-24 · WP-25 · WP-26 · WP-27`

> **[C6]** نُقلت **WP-28** من الموجة 5 إلى الموجة 3 لسبب أمني لا لجدولة: هي المالك
> الوحيد لزرع رموز `PRINT_RECEIPT` · `VIEW_DRAWER` · `PRINT_CARD` · `VIEW_LOGIN_SESSIONS`
> في `PermissionConfiguration.HasData`، وWP-22 (يستهلك `PRINT_CARD`) وWP-26
> (يستهلك `VIEW_LOGIN_SESSIONS`) في الموجة 4. لو نُفِّذ الزرع بعدهما لأُضيف الرمزان
> 16 و17 بلا صفحات مُزروعة، فيُمنع الوصول لجميع المستخدمين. لهذا سقطت **الموجة 5**
> بالكامل: 8 + 14 + 21 + 34 + 29 = **106** كما هي.

### 5.4 مجموعات التوازي

| المجموعة | الحزم | المجلد المشترك | ملاحظة |
|---|---|---|---|
| **A** | WP-02, WP-05, WP-11, WP-12, WP-25, WP-28, WP-30 | `Views/Patients/*` + `ViewModels/Patients/*` — **مملوكة لمطوّر واحد** | |
| **B** | WP-01, WP-06, WP-07, WP-10, WP-13, WP-14, WP-20, WP-21, WP-27 | `Infrastructure/Printing/*` — **مستحيل التوازي؛ كاتب واحد في الوقت** | |
| **C** | WP-08, WP-17, WP-23 | `Domain/Tests/*` + هجرات الكتالوج والباركود | |
| **D** | WP-04, WP-18, WP-24 | هجرات `ExternalEntity` / `Patient` / `SystemSettings` — **قرار 9** | |
| **E** | WP-15, WP-16, WP-19 | `SystemSettings.BranchNumber` + استعلامات الفروع | |
| **F** | WP-03 | `CultureEntryView.xaml` — بعد WP-03 | |
| **G** | WP-09, WP-22, WP-26 | الحسابات + المظروف/الكارنيه + كاشف الدخول | |
| **H** | WP-29 | المعاملات والتسجيل + استعلامات النتائج | |

> **[C19] التصحيح:** الإصدار السابق أسقط ست حزم (05، 06، 09، 21، 26، 29) من أي مجموعة،
> وكرّر WP-26 في جدول المطورين، وذكر WP-07 كحزمة مستقلة رغم أنه مُدمج في WP-01.
> الجدول أعلاه يغطّي **30/30** حزمة بلا استثناء، وWP-07 مُعلَّم `[مُدمج في WP-01]` فلا يأخذ مساراً مستقلاً.

### 5.5 جدول أزموني لثلاثة مطوّرين

| الأسبوع | مطوّر 1 (المجموعة B — الطباعة) | مطوّر 2 (A — شاشات المرضى) | مطوّر 3 (C/D/E — النموذج) |
|---|---|---|---|
| 1–2 | WP-01 | WP-02, WP-05 | WP-03, WP-04 |
| 3–4 | WP-06, WP-07 | WP-11, WP-12 | WP-08 |
| 5–6 | WP-10, WP-14 | WP-30, WP-28 | WP-15, WP-18 |
| 7–8 | WP-13, WP-20 | WP-25 | WP-09, WP-17 |
| 9–10 | WP-21, WP-22 | WP-16 | WP-19, WP-24 |
| 11–12 | WP-27, WP-29 | — | WP-23, WP-26 |

> **[C19] تصحيحان:** (1) WP-26 لم يعد مكرَّراً (كان في الأسابيع 5-6 و11-12 معاً).
> (2) WP-12 صار بعد WP-08 في الجدول — كان يزامنان في الأسابيع 3-4 مع أن WP-12 يعتمد على WP-08.
>
> **[C6]** أضيف **WP-28** إلى الأسابيع 5–6 (مع WP-30 على المطوّر 2) لتسبق الموجة 4 التي
> تستهلك رموزها المزروعة. WP-22 وWP-26 يبقيان في الأسابيع 9–10 و11–12 — أي بعد الزرع.
>
> **تنبيه خفيف:** الجدول أزموني إرشادي وم-grouped حسب §5.4، ومخالفته له مقصودة في
> هذه الحالة وحدها لأن القيد أمني (ترتيب الزرع) يفوق تفضيل المطوّر. الباقي يطابق.

### 5.6 التقدير

**مجموع أيام العمل: 106 يوم-عمل.**

| المطورون | المدة التقريبية |
|---|---|
| 1 | 15–19 أسبوعاً |
| 2 | 8–10 أسابيع |
| **3** | **6–8 أسابيع** |
| 3 (مع توازن مثالي) | 5–6 أسابيع |

> **[C19] تصحيح حسابي صريح.** الإصدار السابق كتب «67–91 يوم عمل» ثم قسّمها على ثلاثة
> وذكر «9–13 أسبوعاً» — وهو رقم **لا ينتج** عن القسمة (67÷3 ≈ 4.4 أسابيع).
> الآن: مجموع الباينة أعلاه **106 يوم-عمل**، مقسوماً على ثلاثة ⇒ ~35 أيام عمل لكل مطوّر
> ⇒ **6–8 أسابيع** (لا 9–13). القيود التي ترفعه فوق الحد النظري:
> سلسلة الطباعة (المجموعة B) تسلسلية بالكامل ≈ 34 يوم عمل ⇒ 7 أسابيع لمطوّر واحد؛
> وWP-04/15/18 تعطيذّ WP-16/19/24؛ **وWP-28 (الموجة 3) يجب أن تُنجَز قبل WP-22 وWP-26
> (الموجة 4) وإلا مُنع الوصول لجميع المستخدمين**؛ وحاجز القرارات 1–9.


---

*الجدول تقديري ويعتمد على توفّر المالك للقرارات 1–9 في §6.1.*
## 6) مخاطر المشروع والقرارات المطلوبة من المالك

### 6.1 القرارات (مطلوبة قبل بدء التنفيذ)

| # | القرار | الخيارات | توصياتي | متى لازم |
|---|---|---|---|---|
| **1** | **ترميم بيانات الحساسية** (WP-03) | القاعدة **واحدة**: `0→NULL`، `1→0`، `2→1`، `3→2` — تطابق متن WP-03 §WP-03. بوابة إجرائية: تقرير قراءة-فقط **ثم** موافقة المالك **ثم** الكتابة. | **(أ)** مع تقرير جدول أوّل. الصفوف بـ `0` ملتبسة (قد تكون «غير محدد») ولا يمكن تمييزها ⇒ تُصفَّر إلى `NULL` لا إلى `HighlyFor`. | قبل WP-03 |
| **2** | **سجلات الطباعة الوهمية** (WP-06) | (أ) تجاهلها · (ب) تصفير `IsPrinted`/`PrintCount`/`LastPrintedAtUtc` لكل ما لم يُطبع فعلاً | **(ب)** على قاعدة إنتاج، **بشرط** التحقق من فترات التسليم مع المالك | بعد WP-06 |
| **3** | **الأسماء التجارية للمضادات** (WP-14) | (أ) جدول ابن `AntibioticCommercialName` · (ب) عمود نصي مفصول بفواصل | **(أ)** — Antipiotics تُدار وتُطبع اجمعاً | قبل WP-14 |
| **4** | **الفرع: ما الذي ينتمي لفرع؟** (WP-15) | (أ) النظام + المستخدم فقط · (ب) المستخدم + الزيارة · (ج) مبيعات منفصلة | **(أ)** إن لم يكن المختبر متعدد الفروع فعلياً. | قبل WP-15 |
| **5** | **عمودا لون الرأس/التذييل** (WP-20) | (أ) تنفيذ المسار كاملاً (setter→command→DTO→UI→writer) · (ب) **حذف العمودين** وتحديث `ReportSettingsTests.cs:92-93` | **(ب)** إن لم تكن الألوان مطلباً تشغيلياً. عمود بلا قارئ = كذب تنظيمي. | قبل WP-20 |
| **6** | **`CashierPrinterEnabled`** (WP-21) | (أ) تنفيذ (طابعة إيصال منفصلة) · (ب) حذف | **(أ)** إن كان للمعمل طابعة كاشير؛ **(ب)** غير ذلك | قبل WP-21 |
| **7** | **الرايات الميتة الثلاث** (WP-24) | تنفيذ أم حذف، لكل واحدة | **حذف `ResultScreenAccountDisplayMode`** (سطح UI فقط). تنفيذ `SaveTreatingDoctorOnlyFromEntityWindow` (سلوك حقيقي). **قرار مطلوب** لـ `PrintAccountInsteadOfDateOnReport`. | قبل WP-24 |
| **8** | **نمو جدول جلسات الدخول** (WP-26) | (أ) بلا اقتطاع · (ب) اقتطاع أقدم من N شهراً | **(ب)** مع `N = 24` | قبل WP-26 |
| **9** | **تسمية الهجرات** (كل الحزم) | هل تُدمج الهجرات المتقاربة؟ | **ادمج** `WP-04.Email` + `WP-24.EnglishName` في `ExternalEntity` واحدة. **افصل** `WP-18.CardNumber` عن `WP-24.SystemSettings`. | قبل Wave 3 |

### 6.2 المخاطر التقنية

| # | المخاطرة | الاحتمال | الأثر | التخفيف |
|---|---|---|---|---|
| R-1 | **الطباعة لا تعمل على أجهزة المختبر** (طابعات غير موجودة في بيئة التطوير) | عالٍ | عالٍ | محاكاة `IPdfPrinterDispatcher` في CI. اختبار «فشل ⇒ لا وسم» WP-06 إلزامي. |
| R-2 | **الاختبارات الثلاثة الفاشلة** (`Receipt`/`Invoice`/`WorkSheetPrintingServiceTests`) تبقى غير محسومة | عالٍ | متوسط | هي بيئة Linux (Skia مفقود). أنشئ **CI على Windows** قبل Wave 2. |
| R-3 | **الخط العربي غير متوفر على أجهزة مستهدفة** | متوسط | **حرج** | قائمة fallback في `ArabicFontResolver.cs:18-27` (Arial، Traditional Arabic، Simplified Arabic، Segoe UI، Tahoma، Times New Roman، Calibri). **افحص** التوفر. اجعل الحزمة تقسّم الخط **مع التقرير** لتفادي المشكلة. |
| R-4 | **هجرة ترميم WP-03 تُفسد بيانات** | منخفض | **حرج** | تقرير جدول + نسخة احتياطية + **لا ت.Commit** بلا موافقة المالك. |
| R-5 | **تساقنان على التسوية** يُدخلان دفعتين | منخفض | متوسط | اختبار تزامن + `UPDLOCK` (WP-29) |
| R-6 | **النتائج القديمة تتغيّر عند تعديل النطاق** (كسر التجميد) | منخفض | **حرج** | اختبار `UsesFrozenRangeNotLiveRange` (WP-07) + تجميد `PatientTestReferenceRangeSnapshot` قائم |
| R-7 | **استعلام WP-16 يخرج عن ترسيخ EF** | متوسط | متوسط | اختبار مبكر لـ `ByTestInPeriod` |
| R-8 | **تعارضات في المجموعات A, B, D** | **عالٍ** | متوسط | ترتيب صارم + لا تُشغّل مجموعتين تكتبان نفس الملف. |
| R-9 | **مقاس المظروف غير معروف** | عالٍ | منخفض | اجعله إعداداً قابلاً للضبط (WP-22) |
| R-10 | **WP-23 يغيّر وحدة القياس** | متوسط | منخفض | اختبار قياس PDF فعلي |

### 6.3 مخاطر المشروع

| # | المخاطرة | التخفيف |
|---|---|---|
| P-1 | **الحزم تتوقف على قرار من المالك** (1–9) | ضع جدول «القرارات» في أول يوم. القرارات 1، 2، 4 تحجب Wave 1–3. |
| P-2 | **«صنف P0» يتضخّم** — كل حزمة P0 تفتح أسئلة جديدة | التزم بالقائمة. الجديد ⇒ سجّله كـ WP-30 وما بعد. |
| P-3 | **R-83 جزء منه Natigh** | نفّذ WP-21 **دون** بند «بيانات الموقع على الإيصال» — مُصرَّح في الملحق أ. |
| P-4 | **تقليص النطاق عند ضغط الجدول** | **لا تحذف** WP-01/02/03/05/06/07 (حجر أساس). اختبر أربعة. المسموح بالتقليص: WP-26, WP-28. |
| P-5 | **اختبارات QuestPDF تفشل على Linux** | CI على Windows. أو اقبل الاختبار كـ `Skipped` على Linux. |

### 6.4 نقاط يجب أن يعرفها المالك

1. **`SearchTestCatalogQuery` غير مبوَّب بصلاحية** (WP-17) — ثغرة قراءة موجودة اليوم، مستقلة عن أي نقص واجهة.
2. **`ExecuteBulkPrintCommandHandler` ينادي `MarkPrinted` بلا طباعة** (WP-06) — سجل التدقيق مضلّل.
3. **زر «مسح» في الإدخال البسيط لا يحفظ** (WP-12) — انطباع خاطئ للمستخدم.
4. **شاشة التاريخ لا يمكن فتحها** (WP-02+WP-10) — مسار ميت كامل.
5. **مستخدم برمجي في CultureEntryViewModel`ResultFlag` ComboBox بلا `ItemsSource`** (WP-12) — لا يمكن ضبط العلم إطلاقاً.
6. **`ReportSettings.HeaderColor/FooterColor` عمودان بلا setter** (WP-20) — قرار 5.

7. **[C20] تسع مسارات قراءة ساخنة تُحمّل جداول كاملة في الذاكرة — خارج النطاق، مقبول صراحةً.**
   كلها من صنف `db.Set<T>().ToDictionary(...)` بلا `Where`:
   `PatientBillingReader.cs:141,158` · `GetResultEntryQueryHandler.cs:104` · `GetResultWorklistQueryHandler.cs:37` ·
   `GetCombinableTestsQueryHandler.cs:194` · `BuildCombinedReportCommandHandler.cs:60,86` ·
   `GetCultureReportQueryHandler.cs:28` · `GetPatientTestsForDrawQueryHandler.cs:45` · `GetPatientTestAuditQueryHandler.cs:61`.
   **هذا إهمال مقصود في هذه الخطة، لا عيب غير مغطّى.** الأثر يعتمد على حجم البيانات ولا يمكن تقديره
   من المصدر (`Project_completion_status.md` §12.7). WP-29 يعالج `PatientBillingReader` ضمن مسار
   المعاملة؛ الباقي يُترك ويُسجَّل هنا ليُراجَع عند أول شكوى أداء.
8. **الفرق الدقيق في أسماء الملفات:** التقرير الفرعي الثاني استخدم `ExternalEntityEditorView.xaml` بينما الملف الفعلي `ExternalEntityEditorWindow.xaml`.

---

## 7) Definition of Done

### 7.1 لكل حزمة (WP-level)

- [ ] البناء ينجح: `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` ⇒ **0 تحذير، 0 خطأ**
- [ ] كل اختبارات xUnit خضراء، **والاختبارات الجديدة المذكورة في الحزمة موجودة وخضراء**
- [ ] الفحص اليدوي المذكور في `معايير القبول` منفَّذ ومُوثَّق
- [ ] إن وُجدت هجرة: وُلّدت باسم متوقّع، مطبّقة، **ومراجعة من المالك**
- [ ] لا `NotImplementedException` / `TODO` / `Task.CompletedTask` خلف خاصية
- [ ] الوصول: كل أمر جديد مربوط بعنصر في XAML (أو مُوثَّق كـ API غير مبوّب)
- [ ] الصلاحيات: كل command/query جديد يحوي `IAuthorizedRequest`

### 7.2 للمشروع (Release-level)

- [ ] **Build:** 0/0 على `Release`
- [ ] **Tests:** 2,158 اختباراً قائم + كل الجديدة ⇒ 0 فشل على Windows CI
- [ ] **Migrations:** الـ**20** هجرة مطبّقة ومرتّبة، `ModelSnapshot` محدَّث
- [ ] **No stubs:** `grep -rn "NotImplementedException\|TODO:" src/` ⇒ صفر
- [ ] **No dead code paths:** اختبار بنيوي يثبت أن كل ViewModel مُسجَّل في DI له متصل تنقّل
- [ ] **Property binding:** لكل ViewModel في نطاق العمل، **كل خاصية معلنة تُقرأ أو تُكتب** يجب أن يوجد لها `Binding` في XAML الموافقة (أو `Xyz=` في `InputBindings`)، وإلا يفشل الاختبار. **[C18]** هذا هو الفحص الذي كان سيلتقط `AccountsHubView.xaml:106-137` (صفر ربط بـ `ElementKind`/`ReportType`) و`OpenDetailCommand` غير المربوط في آنٍ واحد — اختبار `MainWindow_Xaml_Bindings_ResolveToPublicProperties` الحالي يفحص `MainWindow.xaml` وحده ولا يكشف ذلك.
- [ ] **Print honesty:** اختبار يثبت أن فشل الطابعة لا يُنتج وسم `IsPrinted`
- [ ] **Settings coverage:** كل خاصية على `ReportSettings`/`SystemSettings`/`ReceiptSettings`/`EnvelopeSettings` لها قارئ في كاتب **أو** حُذفت
- [ ] **Cross-patient isolation:** لا استعلام يحمل معرّفات واجب أن يفحص الملكية دون فحص
- [ ] **Traceability:** كل `R-xx` من الـ**84** مغطّى بحزمة أو موثّق كـ «مطبَّق مسبقاً»
- [ ] **Docs:** `Docs/` محدَّث بجدول R→WP وحالة كل حزمة

### 7.3 قالب تسجيل الحزمة

```
WP-xx: ____________________  [ ] مكتملة  [ ] متوقفة  [ ] ملغاة
الاختبارات المضافة: ____   الجديدة: ____   المتوقفة: ____
الهجرة: ________________  مُطبّقة: [ ] نعم [ ] لا
الصلاحيات الجديدة: ____  مُزروعة: [ ] نعم [ ] لا
الوصول (Bindings): ____  مُتحقَّق: [ ] نعم [ ] لا
فحص يدوي: ____________________
ملاحظات: ________________________________
```

---

## 8) ملحق أ: خارج النطاق — Natigh / نتيجة.كوم

هذه المتطلبات **مُدرجة في تقرير التدقيق** لكنها **مستبعَدة بحكم صريح من المالك** ولا تُغطّيها أي حزمة في هذا الملف.

| المتطلب | العيب | الوصف | صفحة المرجع | السبب |
|---|---|---|---|---|
| **R-29** | D-14 | تسليم النتائج عبر موقع Natigh.com: رفع نتيجة المريض، حسابات المريض/الطبيب/المختبر على الموقع، عرض النتائج، تصديرها Word/PDF، تطبيق أندرويد | `RLS_Learn` pp.58-69 | موقع ويب + تطبيق موبايل خارجيان عن مستودع سطح مكتب. لا يمكن تنفيذه داخل TopLab. |
| **R-30** | D-14 | عدّاد مرضى Natigh عن فترة، حظر نتيجة من الموقع، طباعة بيانات الموقع على الإيصال | `RLS_Learn` pp.65-69 | مرتبط بموقع خارجي غير قائم. |
| **R-31** | D-14 | إنشاء حساب Natigh لطبيب أو مختبر (توليد رقم، الرقم = كلمة المرور) | `RLS_Learn` pp.142-147 | مفهوم «حساب موقع» بلا موقع. |

### 8.1 البديل الجزئي الموجود — ولا يُعتبر تغطية

`GenerateEntityIdCodeCommandHandler` + `SecureEntityIdCodeGenerator` يولّدان رمزاً عشوائياً 12 حرفاً لكل جهة خارجية. **هذا ليس حساب Natigh**: يُولَّد تلقائياً عند الإنشاء، للقراءة فقط، بلا كلمة مرور، بلا نطاق طبيب/مختبر، وبلا موقع.

**القرار: يُبقى كما هو ولا يُوسَّع.** أي توسيع له (إضافة كلمة مرور، حساب دخول) كان سيصبح «بديلاً أصغر» — وهو ممنوع صراحةً.

### 8.2 أثر الاستبعاد على باقي المتطلبات

| المتطلب المتأثر | الجزء المنفَّذ | الجزء المستبعَد |
|---|---|---|
| **R-55** (الجهات الخارجية) | الاسم، العنوان، المدينة، الهاتف، الفاكس، المسؤول + هاتفه، **البريد الإلكتروني (WP-04)**، قائمة الأسعار، نسبة العمولة | أي بعد شبكي — غير مطلوب أصلاً في هذا المتطلب |
| **R-79** (إعدادات النظام) | كل البنود عدا «طباعة بيانات الموقع على الإيصال» | البند الأخير (من R-83 فعلياً) |
| **R-83** (إعدادات الإيصال) | التذييل، الهامش، العملة، وقت الاستلام، الطباعة لمرة، عرض تفاصيل الاختبار، عدد الباركود | «عرض بيانات Natigh على الإيصال» |

**لماذا لا استبدال:** المواصفة التي استُبعدت (D-14) تصف نظاماً موزّعاً (موقع + موبايل + خادم). أي «بديل» (حفظ رابط، تصدير لموقع، حساب محلي) ينشئ **التزاماً صورياً غير قابل للوفاء** ويضيف سطح صيانة بلا قيمة.

### 8.3 أثر على مصفوفة التتبّع

87 متطلباً في التقرير الأصلي.
- **3 مستبعَدة** (R-29, R-30, R-31) ⇒ **84 في النطاق**.
- في ملحق ب أدناه: **84 صفاً**، منها 3 معلّمة «خارج النطاق».

---

## ملحق ب: جدول التتبّع R-xx → WP-xx

**مولَّد برمجياً** من `Project_completion_status.md` §5 (عمود الحالة الوظيفية) و§8.1 (سجل العيوب).

> **[C9] تصحيح منهجي:** المصدر نفسه فيه **خرائط عيوب متعارضة** — عمود `Defects` في §5 لا يطابق
> سجل §8.1. أمثلة مؤكدة:
> | المتطلب | §5 يقول | §8.1 يقول |
> |---|---|---|
> | R-47 | D-03 | **D-02** و D-12 |
> | R-60 | D-28 | **D-32** |
> | R-62 | D-13, D-29 | **D-13, D-31** |
> | R-63 | D-29 | **D-31** |
> | R-11 | D-24 | *(لا مُدخل في السجل)* |
> | R-56 | D-22 | *(لا مُدخل في السجل)* |
> | R-85 | D-27 | *(لا مُدخل في السجل)* |
>
> **القاعدة المعتمدة هنا:** عمود «العيب» يُؤخذ من **§8.1 حصراً**، ويترك فارغاً `—`
> حيث لا مُدخل في السجل. (§5 لا يُستشهد به في هذا العمود إطلاقاً.)

| # | المتطلب | الحالة الوظيفية (المصدر §5) | جودة المخرج | العيب (§8.1 فقط) | الحزمة | الحالة بعد التنفيذ |
|---|---|---|---|---|---|---|
| R-01 | القائمة الرئيسية بأربعة مداخل | FI | Good | — | WP-02 | FI عبر WP-02 |
| R-02 | تسجيل مريض جديد | FI | Good | — | WP-18 | FI عبر WP-18 |
| R-03 | التاريخ الطبي (رايات) | PI | Defective | D-16 | WP-18 | FI عبر WP-18 |
| R-04 | ملاحظات المريض | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-05 | Lab ID وسجل الزيارات | FI | Good | — | WP-18 | FI عبر WP-18 |
| R-06 | الطلب: إضافة/إزالة/مسح | FI | Good | — | WP-28 | FI عبر WP-28 |
| R-07 | رايات نوع العينة | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-08 | ملاحظة «أُخذت خارج المعمل» | PI | — | D-01 | WP-13 | FI عبر WP-13 |
| R-09 | إضافة مجموعة كاملة | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-10 | حساب حساب المريض | FI | Good | — | WP-18 | FI عبر WP-18 |
| R-11 | Enter مزدوج + تسوية Net | PI | Good | — | WP-28, WP-18 | FI عبر WP-28, WP-18 |
| R-12 | حذف/تعديل عملية دفع | PI | Good | — | WP-18 | FI عبر WP-18 |
| R-13 | ملخّص الحساب | FI | Good | — | WP-18 | FI عبر WP-18 |
| R-14 | شاشة تسليم النتائج | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-15 | تعديل المريض وإضافة/إزالة اختبارات | FI | Good | — | WP-18 | FI عبر WP-18 |
| R-16 | شبكة إدخال النتائج | FI | Good | — | WP-02, WP-12, WP-30 | FI عبر WP-02, WP-12, WP-30 |
| R-17 | معاينة الطباعة | PI | Defective | D-01, D-08 | WP-01, WP-06 | FI عبر WP-01, WP-06 |
| R-18 | التقرير المركّب وإعادة الترتيب | FI | Good | — | WP-13 | FI عبر WP-13 |
| R-19 | خيارات التقرير المركّب | M | — | — | WP-13 | FI عبر WP-13 |
| R-20 | التقرير الفارغ | FI | Defective output | D-01 | WP-01, WP-02 | FI عبر WP-01, WP-02 |
| R-21 | حقول المزرعة الأساسية | FI | Good | — | WP-14 | FI عبر WP-14 |
| R-22 | الفحص المجهري للمزرعة | M | — | D-09 | WP-14 | FI عبر WP-14 |
| R-23 | الحساسية + منطقة التثبيط + الاسم التجاري | PI | Defective | D-10 | WP-03, WP-14 | FI عبر WP-03, WP-14 |
| R-24 | مفاتيح عرض تقرير المزرعة | M | — | D-09 | WP-14 | FI عبر WP-14 |
| R-25 | فلاتر البحث الـ13 | PI | Good | D-26 | WP-16 | FI عبر WP-16 |
| R-26 | البحث برقم الفرع | M | — | D-13 | WP-15 | FI عبر WP-15 |
| R-27 | سجل الزيارات | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-28 | تدقيق الحالة/النتيجة/الطباعة | FI | Good | — | WP-06, WP-30 | FI عبر WP-06, WP-30 |
| R-29 | Natigh — تسليم النتائج | M | — | D-14 | — | خارج النطاق |
| R-30 | Natigh — إحصاءات وحظر | M | — | D-14 | — | خارج النطاق |
| R-31 | Natigh — حسابات طبية | M | — | D-14 | — | خارج النطاق |
| R-32 | إدراج تلقائي للتاريخ | FI | Good | D-01 | WP-01 | FI عبر WP-01 |
| R-33 | إدراج يدوي (نفس/منفصل) | FI | Defective output | D-01 | WP-02, WP-10 | FI عبر WP-02, WP-10 |
| R-34 | تاريخ اختبار / مصفوفة CBC | PI | Defective | D-11 | WP-10 | FI عبر WP-10 |
| R-35 | تاريخ متعدد + فرز + مدى | FI | Defective output | D-01 | WP-10 | FI عبر WP-10 |
| R-36 | إعدادات التاريخ (غامق لكل صنف) | PI | — | D-12 | WP-20 | FI عبر WP-20 |
| R-37 | ورقة عمل بأسماء المرضى | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-38 | ورقة عمل بأسماء الاختبارات | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-39 | ورقة عمل لاختبار محدد | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-40 | ورقة عمل بمجموعة العمل | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-41 | إحصاء الاختبارات | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-42 | غير المكتملة + معاينة | PI | Defective | D-08 | WP-06, WP-27 | FI عبر WP-06, WP-27 |
| R-43 | أعمدة كتالوج الاختبارات + بحث بالرقم | PI | Defective | D-15 | WP-17 | FI عبر WP-17 |
| R-44 | حقول تعديل الاختبار | PI | Defective | D-15 | WP-17 | FI عبر WP-17 |
| R-45 | إضافة اختبار جديد | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-46 | النطاقات المرجعية + وحدة الاختبار | PI | Defective | D-17 | WP-08 | FI عبر WP-08 |
| R-47 | تعليقو الحد الأدنى/الأعلى | IWD | Defective | D-02, D-12 | WP-07 | FI عبر WP-07 |
| R-48 | دلالة وحدة العمر | FI | Correct | — | — | FI (مُتحقَّق منه) |
| R-49 | تحديث النطاق على النتائج القديمة | PI | — | D-07 | WP-11 | FI عبر WP-11 |
| R-50 | الوحدات + أنواع الباركود | M | — | D-19 | WP-08 | FI عبر WP-08 |
| R-51 | مجموعات الاختبارات وسجلات العمل | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-52 | المجموعات المخصصة + السعر الإجمالي | PI | Defective | D-20 | WP-27 | FI عبر WP-27 |
| R-53 | تعليقات الاختبار في التقرير | FI | Defective output | D-01 | WP-13 | FI عبر WP-13 |
| R-54 | قوائم الأسعار + الطباعة | PI | Defective | D-21 | WP-27 | FI عبر WP-27 |
| R-55 | الجهات الخارجية (3 أنواع) | IWD | Defective | D-03 | WP-04 | FI عبر WP-04 |
| R-56 | المزارع 118–139 | PI | — | — | WP-28 | FI عبر WP-28 |
| R-57 | بيانات المضادات + الطباعة | PI | Defective | D-22 | WP-14, WP-27 | FI عبر WP-14, WP-27 |
| R-58 | الإرفاق بعتبة + تصنيف الأطفال | PI | Defective | D-10, D-23 | WP-14 | FI عبر WP-14 |
| R-59 | تعليم عينة مرسلة خارجاً | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-60 | شاشة حساب العينات المرسلة | PI | Good | D-32 | WP-27 | FI عبر WP-27 |
| R-61 | إنشاء المستخدمين | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-62 | الفرع + ساعات/دقائق الراحة | PI | Defective | D-13, D-31 | WP-15, WP-28 | FI عبر WP-15, WP-28 |
| R-63 | الصلاحيات الدقيقة | PI | Good | D-31 | WP-28, WP-22 | FI عبر WP-28, WP-22 |
| R-64 | تعديل مستخدم وصلاحياته | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-65 | الحضور + التأخير والإضافي | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-66 | كاشف الدخول/الخروج | M | — | D-30 | WP-26 | FI عبر WP-26 |
| R-67 | رسالة منع الوصول | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-68 | إحصاءات المرضى (6 تجميعات) | PI | Good | D-24 | WP-19 | FI عبر WP-19 |
| R-69 | إحصاءات الاختبارات (4 تجميعات) | PI | Good | D-24 | WP-19 | FI عبر WP-19 |
| R-70 | إحصاءات العينات المرسلة | FI | Good | — | WP-19 | FI عبر WP-19 |
| R-71 | إنتاجية المستخدم | PI | Good | D-24 | WP-19 | FI عبر WP-19 |
| R-72 | كلمة المرور الثانية للإحصائيات | IWD | Defective | D-05 | WP-19 | FI عبر WP-19 |
| R-73 | تقرير متابعة النتائج | M | — | D-24, D-25 | WP-19 | FI عبر WP-19 |
| R-74 | جرد الدرج + الربح | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-75 | فترات جاهزة + فرع | PI | Defective | D-13 | WP-09, WP-15 | FI عبر WP-09, WP-15 |
| R-76 | 5 عناصر × 4 تقارير | IWD | Defective | D-04 | WP-09 | FI عبر WP-09 |
| R-77 | تنزيل إلى تفصيل المريض | PI | Good | D-04, D-34 | WP-09 | FI عبر WP-09 |
| R-78 | نقد داخل/خارج + عمولات | PI | Defective | D-03, D-04, D-33 | WP-04, WP-09 | FI عبر WP-04, WP-09 |
| R-79 | إعدادات النظام | PI | Defective | D-13, D-27 | WP-15, WP-24 | FI عبر WP-15, WP-24 |
| R-80 | إعدادات القاعدة + نسخ/استعادة | FI | Good | — | — | FI (مُتحقَّق منه) |
| R-81 | هوامش/ورق/رأس/تذييل التقرير | IWD | Defective | D-01 | WP-01, WP-20 | FI عبر WP-01, WP-20 |
| R-82 | مواضع/ألوان/غامق العناصر | M | — | D-28 | WP-20 | FI عبر WP-20 |
| R-83 | إعدادات الإيصال (دون Natigh) | IWD | Defective | D-18 | WP-21 | FI عبر WP-21 |
| R-84 | إعدادات المظروف | PBU | Inert | D-06 | WP-22 | FI عبر WP-22 |
| R-85 | إعدادات الكارنيه | M | — | — | WP-22 | FI عبر WP-22 |
| R-86 | الباركود (38×25، اتجاه، أنابيب) | PI | Defective | D-29 | WP-23 | FI عبر WP-23 |
| R-87 | فصل وسحب العينات | FI | Good | — | WP-25 | FI عبر WP-25 |

### ب.1 حصيلة التغطية

> **[C11] الأعداد أدناه مولَّدة من صفوف الجدول أعلاه، فلا يمكن أن تنحرف عنها.**

| الفئة | العدد |
|---|---|
| **إجمالي المتطلبات** | **87** |
| خارج النطاق (R-29, R-30, R-31) | 3 |
| **داخل النطاق** | **84** |
| …مغطّاة بحزمة واحدة | 49 |
| …مغطّاة بحزمتين أو أكثر | 15 |
| …مغطّاة (مجموع) | **64** |
| …مطبَّقة مسبقاً بلا حزمة (`WP = —`) | 20 |

**تحقّق:** 49 + 15 + 20 = 84 = داخل النطاق ✔
**و:** 87 = 84 داخل النطاق + 3 خارج النطاق ✔

**نسبة التغطية بعد التنفيذ: 64 / 84 = 100%** من المتطلبات المقصودة.

> **تصحيح [C10]:** الصفات في عمود «الحالة الوظيفية» و«جودة المخرج» مأخوذة حرفياً من §5.
> الإصدار السابق خفّض R-87 من FI إلى PI، ووسم R-53 بـ PI مع أن المصدر `FI / Defective output / D-01`،
> وأسقط `D-01` من R-20 وR-33 وR-35 رغم أن المصدر يصف مخرجاتها بـ `Defective output`.

---

## ملحق ج: سجل الهجرات

**المصدر الوحيد للحقيقة:** ترجمة `أثر على الهجرات` في ترويسة كل حزمة (§4). أي عدد في مكان آخر يجب أن يطابق هذا الجدول.

**الحصيلة: 20 هجرة EF فعلية + 1 تعديل تعداد (`PrinterOutputType`).** الإضافات على مواقع `HasData`
القائمة (أربع منها `UpdateData` إلزامي) ليست هجرات مستقلة بل أجزاء من الهجرات أعلاه — توثيقها في §ج.2.
**المصدر الوحيد:** ترويسة `الهجرات` في كل حزمة في القسم 4 — يُقابَل بجدول ج.1 أدناه. أي عدد في مكان آخر يجب أن يطابق هذا الجدول.

### ج.1 هجرات EF الجديدة

| # | الاسم | الحزمة | المحتوى | `HasData`؟ | للتراجع |
|---|---|---|---|---|---|
| 1 | `FixCultureSensitivityCategoryOffByOne` | WP-03 | ترميم `CultureAntibioticResults.SensitivityCategory` + جدول نسخ `CultureSensitivityBackup` | لا | جدول النسخ |
| 2 | `AddExternalEntityEmail` | WP-04 | `ExternalEntity.Email` | لا | nullable ⇒ آمن |
| 3 | `AddUnitsAndBarcodeTypes` | WP-08 | جدولان + `Analyte.UnitId` + `ReferenceRange.UnitId` | لا | جدولان يُفرغان |
| 4 | `AddCombinedReportPrintOptions` | WP-13 | `ReportSettings.PrintGroupSubTitle` + `SuppressReprintMessage` | **نعم** — `ReportSettingsConfiguration.cs:25` | nullable |
| 5 | `AddCultureMicroscopyAndZone` | WP-14 | `CultureMicroscopy` + `InhibitionZoneMm` + `ThresholdMm` | لا | nullable |
| 6 | `AddAntibioticMasterFields` | WP-14 | `Antibiotic.Symbol` + `ScientificName` + جدول `AntibioticCommercialNames` | لا | nullable |
| 7 | `AddBranchNumber` | WP-15 | `SystemSettings.BranchNumber` + `User.BranchNumber` | **نعم إلزامي** — `SystemSettingsConfiguration.cs:26` (صف `Id=1` بُذِر بـ `BranchNumber=1`) | nullable |
| 8 | `CompleteTestCatalogFields` | WP-17 | 9 حقول على `Tests` + `BarcodeTypeId` + `ExternalEntityId` | لا | nullable |
| 9 | `AddPatientCardAndImaging` | WP-18 | `CardNumber` + فهرس فريد جزئي + `RecentImaging*` + فهرس فريد على `LabId` | لا | ⚠ فهرس `LabId` يحتاج فحص تكرار |
| 10 | `SeedClinicalHistoryConditionTypes` | WP-18 | بذر 11 نوع حالة سريرية | **نعم** — `MedicalConditionTypeConfiguration.cs:17` | Upsert ⇒ آمن |
| 11 | `AddReportHeaderFooterItemSettings` | WP-20 | جدول `ReportPrintItemSettings` + `HistoryBoldAnalytes` + (ألوان إن القرار 5=أ) | لا | جدول يُفرغ |
| 12 | `ExtendReceiptSettings` | WP-21 | `FooterText` + `HeaderImagePath` + `FooterImagePath` + `PaperSize` + `PrintOnceAction` | **نعم إلزامي** — `ReceiptSettingsConfiguration.cs:21` (عمودا `PaperSize` و`PrintOnceAction` ‏`IsRequired()` وليسا nullable) | nullable |
| 13 | `AddCardSettings` | WP-22 | جدول `CardSettings` + `CardPrintItemPositions` + seed `Id=1` | **نعم** — `CardSettingsConfiguration` (جديد) + `EnvelopeSettingsConfiguration.cs:17` كنموذج | الجدول يُفرغ |
| 14 | `AddBarcodeSettings` | WP-23 | جدول `BarcodeSettings` + seed `38×25mm` أفقي | **نعم** — `BarcodeSettingsConfiguration` (جديد) | الجدول يُفرغ |
| 15 | `AddTubes` | WP-25 | جدول `Tube` + `PatientTest.TubeId` + فهرس + بذر أسماء الأنابيب | **نعم** — `TubeConfiguration` (جديد) | nullable |
| 16 | `AddLoginSessions` | WP-26 | جدول `LoginSessions` + فهارس | لا | لا مراجع ⇒ آمن |
| 17 | `AddCustomGroupTotalPrice` | WP-27 | `CustomGroup.TotalPrice` | لا | nullable |
| 18 | `AddBreakHoursAndPermissions` | WP-28 | `User.BreakDurationHours` + **رمزان** + **زرع `UserPermissionGrant`** | **نعم** — `PermissionConfiguration.cs:17-31` | ⚠ **الترتيب حاسم** |
| 19 | `AddResultAmendments` | WP-30 | جدول `ResultAmendment` (نوع-agnostic) لـ `PatientTestId` | لا | الجدول يُفرغ |
| 20 | `CompleteSystemSettings` | WP-24 | `Patient.EnglishName` + `ExternalEntity.EnglishName` + `SystemSettings.PrintEnglishNamesOnReceipt` | **نعم إلزامي** — `SystemSettingsConfiguration.cs:26` (صف `Id=1` بُذِر) | nullable، والإبطال بـ`Down` كامل |

> **[C5]** الصف 20 كان معلَناً في ترويسة WP-24 («بند 20») دون أن يوجد في هذا الجدول.
> استُرجع هنا ليطابق عدّادَ ترويسات الحزم (20 هجرة EF). إن اختار المالك **القرار 9**
> بدمج `ExternalEntity.EnglishName` في `AddExternalEntityEmail` (بند 2) فلا يُنشأ البند 20
> مستقلاً، ويتناقص العدّ إلى **19** — ويجب تعديل الترويسة وملحق ج معاً في الحالة تلك.

### ج.2 تعديلات `HasData` الحالية — الأثر الصامت

> **[C8]** المستودع فيه **ثمانية** مواقع `HasData`. ثلاثة منها في القائمة أعلاه تفرض `UpdateData`
> رغم إضافة أعمدة nullable.
> أي هجرة تضيف عموداً إلى جدول له `HasData` **لا** تُحدِّث الصف المزروع ⇒ يبقى الصف على القيم القديمة، ويُظهر
> الـ ModelSnapshot فرقاً كاذباً في الترحيل التالي.

| الملف | السطر | في أي هجرة | إلزامي؟ |
|---|---|---|---|
| `ReportSettingsConfiguration.cs` | 25 | WP-13 | **نعم** — عمودا `bool` جديدان على صف `Id=1` |
| `SystemSettingsConfiguration.cs` | 26 | WP-15 | **نعم** — `BranchNumber = 1` |
| `ReceiptSettingsConfiguration.cs` | 21 | WP-21 | **نعم** — `PaperSize` + `PrintOnceAction` ‏`IsRequired()` |
| `MedicalConditionTypeConfiguration.cs` | 17 | WP-18 | اختياري — الهجرة **بيانات** لا مخطط |
| `PermissionConfiguration.cs` | 17–31 | WP-28 (المالك للزرع) · يستهلكها WP-22 · WP-26 | **نعم** — **أربعة** رموز جديدة: 14 · 15 تُزرعان في WP-28، و16 `PRINT_CARD` · 17 `VIEW_LOGIN_SESSIONS` تُزرعان في **الموجة 3 قبل** Wave 4 (§WP-28 ثلاث مراحل) |
| `EnvelopeSettingsConfiguration.cs` | 17 | — | لا (لا هجرة على المظروف) |
| `EnvelopePrintItemPositionConfiguration.cs` | 16 | — | لا (يُقرأ فقط في WP-22) |
| `PrinterAssignmentConfiguration.cs` | 14 | WP-22 | اختياري — إن أُضيف صف طابعة كارنيه |

### ج.3 تعديلات تعداد

| # | البند | الحزمة | ملاحظة |
|---|---|---|---|
| 1 | `PrinterOutputType.Card = 4` | WP-22 | إلحاق فقط — **لا إعادة ترتيب**؛ القيم 0..3 مخزَّنة. اختبار `PrinterOutputType_Card_IsStableAcrossMigration` |

### ج.4 تحقّق ما قبل الدمج

```sql
-- (أ) تكرار Lab ID قبل WP-18
SELECT LabId, COUNT(*) c FROM Patients WHERE LabId IS NOT NULL GROUP BY LabId HAVING c > 1;
-- (ب) نطاق 118-139 قبل WP-28  — عمود المفتاح في Tests هو TestId لا Id
--     (RenamePkColumns.cs:25-26 / ModelSnapshot: b.Property<int>("Id").HasColumnName("TestId"))
SELECT TestId, TestCode FROM Tests WHERE TestId BETWEEN 118 AND 139;
-- (ج) بعد زرع الصلاحيات — يجب ألا يعود صف فارغ  (الجدول بالجمع: UserPermissionGrants)
SELECT PermissionId, COUNT(*) FROM UserPermissionGrants
WHERE PermissionId IN (14, 15, 16, 17) GROUP BY PermissionId;
```

> **[C1 + N1]** صيغتا (ب) و(ج) كانتا ترميان استثناءً: عمود `Tests.Id` غير موجود
> (الصحيح `TestId`)، وجدول `UserPermissionGrant` بالمفرد غير موجود
> (الصحيح `UserPermissionGrants` بالجمع). استُعمِل الفحص على أسماء الجداول والأعمدة
> الحقيقية في `ApplicationDbContextModelSnapshot.cs` و`BaselineDataModel.cs` قبل كل جملة.

---

## ملحق د: سجل قرارات التنفيذ

| # | الحزمة | القرار | التاريخ | ملاحظات |
|---|---|---|---|---|
| | | | | |

---

*نهاية الوثيقة. أُعدّت على commit `9b912c1a0ec51fb9786831a7d8b3362141e78e18` بعد تحقّق مستقل من المرفقات الثلاثة ومن الشيفرة المصدرية. المستودع لم يُعدَّل (قراءة فقط).*
