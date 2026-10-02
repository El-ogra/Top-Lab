# تقرير تدقيق مستقل — Top-Lab Wave 2

**التاريخ:** 2026-10-02
**نوع التدقيق:** تحقيق مستقل في السحابة (قراءة فقط — لم يُعدَّل المستودع)

---

## 1. خط أساس التدقيق

| البند | القيمة |
|---|---|
| المستودع | `https://github.com/El-ogra/Top-Lab.git` |
| الالتزام المطلوب | `ee9acf8a7e60443f38c45e1958cc36c51401f55d` |
| HEAD المُتحقَّق منه | `ee9acf8a7e60443f38c45e1958cc36c51401f55d` |
| الفرع | `HEAD` منفصل (الالتزام موجود ضمن `main`، ومحدَّث في `origin/main`) |
| رسالة الالتزام | `Verifying the completion of the first half` — medowemado، 2026-10-02 |
| هل تم تدقيق الالتزام المطلوب بالضبط؟ | **نعم** — `git rev-parse HEAD` طابق حرفياً، و`git status --porcelain` فارغ (شجرة نظيفة) |
| البناء | ناجح (`dotnet build` على المشاريع القابلة للبناء) |

---

## 2. نتيجة التدقيق المستقل

الادعاء السابق **مؤكَّد جوهرياً**، ومُصاغ بشكل أدق مما ورد في التقرير السابق.

الالتزام `ee9acf8` يحتوي بالفعل على **مسارات طباعة متعددة ذات سلوك مادي متباين** تجاه `IsPrinted`. النتيجة الأهم التي توصلت إليها مستقلة: **التباين لم يكن موجوداً قبل Wave 2 — بل Wave 2 هي التي أنشأته.** هذا انحدار مُستحدث بفعل الإصلاح، وهو أخطر من مجرد "مشكلتين متعايشتين".

كما اكتشفت نتيجة إضافية عالية الأثر لم تُذكر سابقاً: مسار التعديل بعد الطباعة (Amendment) أصبح **غير قابل للوصول عبر أي مسار طباعة في النظام**.

---

## 3. الإجابة الحاسمة: هل هناك مسارات طباعة متعددة؟

**نعم — يوجد مساران ماديان متباينان، لا تتقاطعان إطلاقاً.**

### المسار (أ) — "المنسّق الصادق" (لا يُعلِّم `IsPrinted` أبداً)

```
ProfileEntryViewModel.PrintCommand          (ViewModels/Patients/ProfileEntryViewModel.cs:113, 353-383)
CultureEntryViewModel (طباعة)               (ViewModels/Patients/CultureEntryViewModel.cs:468-470)
MarkResultPrintedCommandHandler             (Commands/MarkResultPrinted/…:67-68)
ExecuteBulkPrintCommandHandler              (Commands/BulkPrint/…:81-97)
        └──────────────► IResultPrintCoordinator.PrintAsync
                              └─► build token ─► IReportPrintingService.PrintReportAsync
                              └─► return outcome.   ❌ لا MarkPrinted إطلاقاً
```

`ResultPrintCoordinator` (196 سطراً) لا يحقن `IApplicationDbContext` عمداً، ويWTOHe's commented: "Never marks a result as printed". الاستدعاء الوحيد لاسم `MarkPrinted` في مجلد `ResultsEntry/Common/` هو **تعليق توثيقي** في `IResultPrintCoordinator.cs:37` — وهذا بالضبط ما يدّعيه الكود.

### المسار (ب) — "المسار القديم المتروك" (يُعلِّم `IsPrinted`)

```
CombinedReportViewModel.PrintAsync           (ViewModels/Patients/CombinedReportViewModel.cs:272-274)
        └─► PrintCombinedReportCommandHandler
              build ─► print ─► ✅ row.MarkPrinted(...)  ─► SaveChangesAsync
                                        (PrintCombinedReportCommandHandler.cs:71-86)

HistoryReportsViewModel (طباعة السجل)        (ViewModels/Patients/HistoryReportsViewModel.cs:307)
        └─► PrintHistoryReportCommandHandler
              build ─► print ─► ✅ row.MarkPrinted(...)  ─► SaveChangesAsync
                                        (PrintHistoryReportCommandHandler.cs:76-91)
```

**الدليل القاطع على عدم التقاطع:** المنسّق لا يمرّ عبر `PrintCombinedReportCommandHandler` ولا `PrintHistoryReportCommandHandler`، لأن ملف `ResultPrintCoordinator.cs` لا يحتوي استيراداً (`using`) لأي منهما. المنسّق يبني الحزمة بنفسه عبر `BuildCombinedReportCommand` ثم `ReportPrintEnvelope.CreateToken` ثم `_printing.PrintReportAsync` — أي末端 الطباعة مشترك، لكن **الجزء الذي يلمس حالة النطاق** مفصول تمامًا.

### عدد المسارات

مساران ماديان متباينان. المسارات الإضافية (الفاتورة، الإيصال، الباركود، ورقة العمل، التقرير الفارغ) **خارج النطاق** — لا تلمس `PatientTest` إطلاقاً (تحققت: `PrintBlankReportCommandHandler` لا يحتوي `MarkPrinted`، بتعليق صريح `OD-07-E`).

**ملاحظة مهمّة:** أمرا `MarkProfilePrintedCommand` و`MarkCultureReportPrintedCommand` كانا مسارين ثالثاً ورابعاً يعلّمان مباشرة، لكنهما **ميّتان في الإنتاج**: `grep` على `src/` يُظهر **صفراً** لمواقع استدعاء (`new MarkProfilePrintedCommand(...)` لا يظهر إلا في الاختبارات). 並 لم ResidualSداد من مهل.

---

## 4. تحقيق `IsPrinted`

### أين يُضبط؟

`PatientTest.MarkPrinted` (Domain/Results/PatientTest.cs:178-189):
```csharp
if (EnteredAtUtc is null || !IsReviewed) throw ...;   // يتطلب اعتماداً
IsPrinted = true;
PrintCount++;                                          // يزيد PrintCount
LastPrintedByUserId = ...; LastPrintedAtUtc = ...;
```

**مواقع الإنتاج لكتابة `PatientTest.IsPrinted` في HEAD (من `git grep`):**
1. `PrintCombinedReportCommandHandler.cs:77`
2. `PrintHistoryReportCommandHandler.cs:82`
3. `MarkCultureReportPrintedCommandHandler.cs` — ميت (لا مستدعين في الإنتاج)
4. `MarkProfilePrintedCommandHandler.cs:74` — ميت (لا مستدعين في الإنتاج)

### أين لا يُضبط؟

**شاشات الإدخال (Profile / Culture) والطباعة الجماعية — لا تُعلِم إطلاقاً.** قبل S5 كانت `ProfileEntryViewModel.PrintAsync` ترسل `MarkProfilePrintedCommand` (تحققت في `git show 9636466^`، السطر 355). بعد S5 صارت تستدعي المنسّق.

### التوقيت: بعد نجاح الطباعة أم قبلها؟

في المسار (ب): **بعد** نجاح الطباعة — `if (!print.IsSuccess) return print;` (السطر 66-69 / 71-74) يسبق `MarkPrinted`. هذا سليم.

في المسار (أ): لا يُطبَق إطلاقاً.

### هل يمكن لفشل الطباعة أن يترك `IsPrinted` مُغيَّراً؟

**لا، في كلا المسارين.** المسار (ب) يفحص نجاح الطابعة قبل التعليم. المنسّق لا يكتب أصلاً. فالقيد `MarkDelivered` و`Unreview` يبقى محمياً من الفشل.

### متى يُحفظ؟

`await _db.SaveChangesAsync(...)` بعد الحلقة مباشرة (السطر 85 / 90). في المنسّق: **لا يوجد حفظ إطلاقاً** (لا DbContext محقون).

### اعتماد التسليم

- `PatientTest.MarkDelivered` (191-196): `if (!IsPrinted) throw "Result not printed."` — **التبعية حقيقية وغير مشكوك فيها**.
- `DeliveryHandoverViewModel`: `CanDeliver => IsPrinted && !IsDelivered` (سطر 34)، ويُقرأ من `GetDeliveryGridQueryHandler` الذي يمرّر `pt.IsPrinted` مباشرة (سطر 51).
- `DeliverWithSettlementCommandHandler:58` يستدعي `MarkDelivered` — أي أن layer التطبيق يلتزم بحارس النطاق.

### اعتماد التعديل (Amendment)

- `ProfileEntryViewModel.CanAmend => IsPrinted` (سطر 61) — لكن هذا **`ProfileResultItem.IsPrinted`** وليس `PatientTest.IsPrinted` (يأتي من `GetProfileEntryGridQueryHandler:61` ← `item?.IsPrinted ?? false`).
- `AmendProfileResultCommandHandler`: `if (!item.IsPrinted) return Conflict("لا يمكن تعديل نتيجة البروفايل قبل الطباعة.")`.

---

## 5. حكم المدقق السابق

**الحكم: «مؤكَّد جزئياً»** — صحيح في ملاحظاته الجوهرية، لكنه mosaub في التفسير causal، ويغفل نتيجة أشدّ.

**ما هو صحيح ومُثبت بالكود:**
- ✅ `PrintCombinedReportCommandHandler` و`PrintHistoryReportCommandHandler` يستدعيان `row.MarkPrinted(...)` مباشرة (السطور 77 و82).
- ✅ مسارات أخرى تمرّ عبر `IResultPrintCoordinator`.
- ✅ Behavioral الصلة صحيحة: الطباعة من شاشات البروفايل تنجح بينما `IsPrinted` يبقى `false`، والتسليم يعتمد على `IsPrinted`.
- ✅ "مسارات طباعة متعددة بمعالجة حالة غير متسقة" — وصف دقيق.

**ما يحتاج تصحيحاً:**
- ⚠️ **النسب causal معكوس.** التقرير السابق يوحي بأن التباين قديم و قائمة. الحقيقة من `git show`: قبل Wave 2 كانت **كل** مسارات الإنتاج تُعلِّم (`git grep` على `9636466^` يُظهر 7 مواقع `MarkPrinted` مقابل 4 في HEAD). **S4/S5/S6 هي التي شقّت النظام**، عبر إضافة منسّق "لا يُعلِّم" وتحويل شاشات الإدخال إليه، دون تحديث `PrintCombinedReport`/`PrintHistoryReport`. هذا انحدار مُحدث، وهو أخطر لأن نصف الإصلاح نُفِّذ.
- ⚠️ هناك أيضاً **بُعد ثالث** كان مفقوداً من التحليل: انفصال حالة `ProfileResultItem` عن `PatientTest` (القسم 6).

---

## 6. نتائج Wave 2 إضافية (مثبتة من الكود)

### (أ)严重: التعديل بعد الطباعة أصبح غير قابل للوصول — انحدار جوهري

`ProfileResultItem.MarkPrinted` (سطر 128-139) هو **الطريقة الوحيدة** التي تضبط `ProfileResultItem.IsPrinted` في الإنتاج. repuls唯一的 مستدعيها كان `MarkProfilePrintedCommandHandler:71` — وهو **ميت** (صفر مواقع استدعاء في `src/`).

النتيجة:
- الطباعة من `ProfileEntryView` → المنسّق → لا يُعلِّم `PatientTest` ولا `ProfileResultItem`.
- الطباعة من `CombinedReportView` → `PrintCombinedReportCommandHandler` → يُعلِّم **`PatientTest` فقط** (السطر 72-83: `_db.Set<PatientTest>()`) — لا يمس `ProfileResultItem` إطلاقاً.
- ⇒ `CanAmend` = `ProfileResultItem.IsPrinted` = **false دائماً** في الإنتاج.
- ⇒ `AmendProfileResultCommandHandler` سيرفض دائماً بـ `"لا يمكن تعديل نتيجة البروفايل قبل الطباعة."`

**للتأكد:** `grep -n "ProfileResultItem" BuildCombinedReportCommandHandler.cs` — يُظهر استخدامات للقراءة فقط (سطر 78-91)، ولا `MarkPrinted`. المسار الوحيد المتبقي لإتاحة التعديل كان `MarkProfilePrintedCommandHandler`، وقد became غير قابل للاستدعاء.

هذا **أخطر** من انحراف `IsPrinted`: ميزة معمارية كاملة (Decision 3 — تعديل مُثبَّت مع سجل تدقيق) صارت صامتة الوصول.

### (ب)确认: أمران ميتان في الإنتاج

`MarkProfilePrintedCommand` و`MarkCultureReportPrintedCommand` — لا يوجد في `src/` أي `new XCommand(...)` (فقط تعريفات + اختبارات). يبقى مُسجَّلاً عبر MediatR لكن لا شاشة تستدعيه. `grep`oszettes，只会 الاختبارات.

### (ج)较好: F1 و F2 مُصلَحان فعلاً (على مستوى تمثيل المصدر)

- **F1:** في `5075613`، استُبدل `columns: new string[0], values: new object[0]` بـ `UpdateData` صريح بأعمدة وقيم. التعليق يذكر أن الفراغ يُنتج `"UPDATE [ReportSettings] SET WHERE ..."` وهو T-SQL غير صالح. **تحققت من الشكل في المصدر فقط** — لم أُنفّذ أي SQL.
- **F2:** `DependencyInjection.cs` عاد للتنسيق الصحيح، وسطر `services.AddScoped<IResultPrintCoordinator, …>` (سطر 34) مسطّح بشكل صحيح مع سطر نهائي في الملف.

### (د)良好: S13 (تشخيص الاستثناءات) و S15/S16 (حارس الطبقات + تنظيف الملفات المؤقتة)

`PrintingDiagnostics` يلتقط الاستثناءات المبتلعة في `ReportPrintingService:82`، ولا يرمي أبداً (`catch {}` في نهاية `ReportSwallowed`) — وهو تطبيق متسق لقاعدة "مسار التشخيص لا يصبح مسار خطأ". حارس الطبقات `PresentationLayeringTests` موجود.

### (هـ)ملاحظة على نية التصميم لا تُعدّ عيباً

`IResultPrintCoordinator.cs:37-39` يشرح صراحةً أن SD-1 كان **قراراً مقصوداً** ("forbids MarkPrinted anywhere in this path")، وأن الهدف هو "منع الكتابات المستقبلية". أي أن **عدم التعليم مقصود** — لكن التطبيق أُسند إلى نطاق التطبيق المعماري (Coordinators) لا إلى النطاق (Domain)، فلم يكن كافياً. كما أن العقد نفسه يحمل تناقضاً: `MarkResultPrintedCommandHandler` لم يُحذف بل "سُلك" للمنسّق (SD-16، سطر 63-66)، بينما `MarkProfilePrinted` و`MarkCultureReportPrinted` — وهو نفس النمط — تُركا ميّتين. **عدم اتساق في تطبيق قرار معلن.**

---

## 7. تغطية الاختبارات / نزاهتها

**النتيجة: الاختبارات خضراء لكنها لا تغطي السيناريو المُبلَّغ عنه إطلاقاً.**

نتائج التنفيذ الفعلية:

| المشروع | النتيجة |
|---|---|
| `TopLab.Application.Tests` | ✅ **1583/1583 ناجح** |
| `TopLab.Domain.Tests` | ✅ **507/507 ناجح** |
| `TopLab.Persistence.Tests` | ✅ 13 ناجح (2 متخطّى) |
| `TopLab.Infrastructure.Tests` | ⚠️ 247 ناجح / **18 فاشل** — بيئي بالكامل (انظر أدناه) |
| `TopLab.Presentation` | ⚠️ غير قابل للبناء على Linux (يستهدف Windows / WPF) |

**لماذا الـ18 فاشلة بيئية وليست عيوباً:** جميعها `QuestPDF ... font families that are not available: 'Arial'` — و`fc-list | grep -ci "arial\|lato"` = **0**. الاستثناء يُبتلَع في `ReportPrintingService:79-83` فيعود `Result.Failure`، فتسقط كل التأكيدات اللاحقة بالتسلسل. هذه **نتيجة غياب خطوط Windows على Linux، لا خلل في الكود**.

### نقاط النزاهة الجوهرية

**أ. اختبارات التسليم تُرتّب الحالة يدويًا — لا تطبع فعلاً:**
- `DeliverWithSettlementCommandHandlerTests.cs:33` → `pt.MarkPrinted(1, Now);`
- `GetDeliveryGridQueryHandlerTests.cs:31`، `GetUndeliveredResultsQueryHandlerTests.cs:34` → نفس النمط

هذا يعني: **لو حُذف كل استدعاء `MarkPrinted` من الإنتاج، ل仍将 كل اختبارات التسليم خضراء.** هي تُثبت أن التسليم يعمل *بشرط* `IsPrinted`، ولا تُثبت *كيف* يصبح `IsPrinted` صحيحاً.

**ب. لا يوجد أي اختبار يربط الطباعة بالتسليم عبر الـcoordinator.** بحثتُ عن `ResultPrintCoordinator` معبَّر عن التسليم → **صفر نتائج**. لا يوجد اختبار واحد ينفّذ (طباعة حقيقية → تسليم).

**ج. اختبار التعديل يتفادى الفخّة م maneuveringly:** `AmendProfileResultCommandHandlerTests.cs:24` يستدعي `pt.MarkPrinted(...)` **ثم** `AddPrintedItem(...)` — أي أنه يُجهّز `ProfileResultItem.IsPrinted` مباشرةً ويتجاوز الطباعة كلياً. النتيجة: الانحدار في القسم 6(أ) **غير مكتشف**.

**د. الاختبارات“两个 الاتجاهين” تثبت التباين ولا تحكمه:**
- `PrintCombinedReportCommandHandlerTests` (`:55-73`) **يؤكد** `Assert.True(row.IsPrinted)` و`PrintCount == 1`.
- `ReviewPrintDeliverCommandHandlerTests` (`:154-157`) **يؤكد** `Assert.False(row.IsPrinted)` مع تعليق صريح: *"The old assertion (IsPrinted == true) asserted the very dishonesty WP-06 removes."*

كلاهما أصحّ wrt سلوكه — لكن **لا يوجد اختبار يعبر من 하나 إلى الآخر**، فلا يظهر التباين كـخلل. مقاييس S5/S6 تقيس المسار (أ) فقط، وقياسات M-07 تقيس المسار (ب) فقط.

**الخلاصة:** 2093 اختباراً ناجحاً، وصفر تغطية للسيناريو المتقاطع المُبلَّغ عنه. هذا بالضبط لماذا remained العيب غير مرصود.

---

## 8. قيد نطاق قاعدة البيانات

**خارج هذا التحقيق — ولم أدّعِ التحقق منه.**

- لم أُحاول الاتصال بـ SQL Server، ولا تنفيذ الترحيلات (migrations)، ولا فحو انحراف البيانات.
- الترحيلات الثلاثة الأولى لـ Wave 2 (`AddCombinedReportPrintOptions` و`AddCultureMicroscopyAndZone` و`AddAntibioticMasterFields`) **خارج النطاق المطلوب**.
- فحص F1 كان على **تمثيل المصدر فقط** (شكل `UpdateData`)؛ صحّة T-SQL المُنتَجة لم تُختبر.
- `TopLab.Persistence.Tests` (13 ناجح) لا يُثبت صلاحية الهجرات على SQL Server حقيقي.

---

## 9. الأدلة

| المسار / الصنف | الأسطر | الدليل |
|---|---|---|
| `Application/Features/ResultsEntry/Common/ResultPrintCoordinator.cs` | 41-66، 88-136 | يبني ثم يطبع، **بلا** `MarkPrinted`، بلا `IApplicationDbContext` |
| `Application/Features/ResultsEntry/Common/IResultPrintCoordinator.cs` | 36-45 | عقد "build, then print, and never mark" |
| `Application/…/PrintCombinedReport/PrintCombinedReportCommandHandler.cs` | 64-86 | طباعة ← `row.MarkPrinted` ← `SaveChanges` |
| `Application/…/PrintHistoryReport/PrintHistoryReportCommandHandler.cs` | 69-91 | نفس النمط |
| `Domain/Results/PatientTest.cs` | 178-189، 191-196 | `MarkPrinted` يزيد `PrintCount`؛ `MarkDelivered` يتطلب `IsPrinted` |
| `Domain/Results/ProfileResultItem.cs` | 128-139 | `MarkPrinted` الوحيد لبند البروفايل |
| `Domain/Results/ProfileResultItem.cs` | 60 | إعادة البناء من EF هي المُعيِّن الوحيد لـ `IsPrinted` |
| `Application/…/AmendProfileResultCommandHandler.cs` | 43-46 | يحرس `item.IsPrinted` على مستوى البند |
| `Application/…/MarkProfilePrinted/MarkProfilePrintedCommandHandler.cs` | 68-76 | كان يُعلّم البند + الاختبار — **ميت في الإنتاج** |
| `Presentation/ViewModels/Patients/ProfileEntryViewModel.cs` | 60-61، 353-383 | `CanAmend` على البند؛ الطباعة عبر المنسّق |
| `Presentation/ViewModels/Patients/DeliveryHandoverViewModel.cs` | 34، 202 | `CanDeliver => IsPrinted && !IsDelivered` |
| `Application/…/GetDeliveryGrid/GetDeliveryGridQueryHandler.cs` | 51 | يمرّر `pt.IsPrinted` إلى الشبكة |
| `Application/…/DeliverWithSettlement/DeliverWithSettlementCommandHandler.cs` | 56-65 | `MarkDelivered` مع ترجمة الفشل |
| `Application/DependencyInjection.cs` | 32-34 | تسجيل المنسّق (F2 مُصلَح) |
| `git show 9636466^:…ProfileEntryViewModel.cs` | 355 | **قبل S5**: الطباعة كانت ترسل `MarkProfilePrintedCommand` |
| `git grep "\.MarkPrinted(" 9636466^ -- src` | 7 مواقع | **قبل**: كل المسارات تُعلّم |
| `git grep "\.MarkPrinted(" HEAD -- src` | 4 مواقع | **بعد**: مساران ميتان + المسار (ب) |
| `Infrastructure/Logging/PrintingDiagnostics.cs` | 28-46 | S13 — يُبتلع ولا يرمي أبداً |
| `Infrastructure/Persistence/Migrations/20261001203315_…cs` | 27-34 | F1 — `UpdateData` صريح |

---

## 10. الإجابة النهائية المباشرة

> **"استنادًا إلى الكود المصدري الفعلي للحل عند الالتزام `ee9acf8a7e60443f38c45e1958cc36c51401f55d`، هل اكتشاف المدقق السابق بشأن تعدد مسارات الطباعة وعدم اتساق سلوك `IsPrinted` صحيح فعلاً؟"**

**صحيح — في جوهره، ومُثبت من الكود، مع تصويبين جوهريين:**

1. **الملاحظة صحيحة:** يوجد مساران ماديان متباينان لا يتقاطعان — أحدهما يُعلِّم `IsPrinted` بعد نجاح الطباعة، والآخر لا يُعلِّم إطلاقاً؛ والتسليم يعتمد فعلاً على `IsPrinted`، والطباعة من شاشات الإدخال تنجح فعلاً دون ضبطه.

2. **التصويب الأول — التباين انحدار من Wave 2، لا سابقة كُشفت:** قبل S5/S6 كان **كل** مسار إنتاج يُعلِّم `IsPrinted`. الإصلاح هو ما أنشأ المسارين.

3. **التصويب الثاني — النتيجة الأشدّ التي غابت:** فصل `ProfileResultItem.IsPrinted` عن `PatientTest.IsPrinted` جعل **التعديل بعد الطباعة معطّلاً كلياً في الإنتاج** — لأن الأمر الوحيد الذي كان يُعلّم البند (`MarkProfilePrintedCommandHandler`) صار بلا أي مستدعي. هذه ميزة معمارية كاملة صامتة الفشل.

**التصنيف النهائي: «مؤكَّد جزئياً»** — صحيح في الملاحظة، ناقص في التفسير causal، وناقص في النتائج.
