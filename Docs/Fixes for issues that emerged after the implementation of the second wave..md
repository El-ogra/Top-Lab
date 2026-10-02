# Fixes for issues that emerged after the implementation of the second wave

**المشروع:** Top-Lab
**الموجة:** Wave 2 (W-02)
**التزام الأساس قبل الإصلاح:** `ee9acf8a7e60443f38c45e1958cc36c51401f55d`
**نوع الوثيقة:** وصف المشكلة **وما أُنجز فعلياً** من إصلاحات
**تاريخ الإصدار:** 2026-10-02

> **قاعدة صارمة:** كل رقم واسم ملف ونتيجة اختبار في هذه الوثيقة ناتج عن تنفيذ حقيقي على جهاز المالك، لا عن تخطيط.

---

## 1) المشكلات المكتشفة بعد تنفيذ Wave 2

### المشكلة (أ) — انقسام مسارات الطباعة إلى سلوكين متضادفين

كان في البرنامج **خمسة** مسارات إنتاجية للطباعة، موزّعة على **ممارستين مختلفتين** لعلام�� «طُبعت»:

| المسار | نقطة الدخول | المعالِج |Behavior قبل الإصلاح |
|---|---|---|---|
| P1 | `ProfileEntryViewModel.PrintCommand` | `ResultPrintCoordinator` | **لا يُسجّل** |
| P2 | `CultureEntryViewModel` | `ResultPrintCoordinator` | **لا يُسجّل** |
| P3 | `BulkPrintDialogViewModel` ← `ExecuteBulkPrintCommandHandler` | `ResultPrintCoordinator` | **لا يُسجّل** |
| P4 | `CombinedReportViewModel` | `PrintCombinedReportCommandHandler` | **يُسجّل** (لكن `PatientTest` فقط) |
| P5 | `HistoryReportsViewModel` | `PrintHistoryReportCommandHandler` | **يُسجّل** (لكن `PatientTest` فقط) |

**السبب الجذري:** `ResultPrintCoordinator` لا يحقن `IApplicationDbContext` إطلاقاً — وهذا قرار تصميمي مقصود—so كان **عاجزاً بنيوياً** عن تسجيل أي حالة. بينما بقي معالِجا P4/P5 يُسجّلان يدوياً.

### المشكلة (ب) — انفصال حالة بنود البروفايل

`ProfileResultItem.IsPrinted` يكتبها في الإنتاج **مستدعٍ واحد فقط**: `MarkProfilePrintedCommandHandler:71`. وقد صار هذا الأمر **ميتاً** (لا شاشة تستدعيه)، والسبب أن Wave 2 حوّل `ProfileEntryViewModel` من إرسال `MarkProfilePrintedCommand` إلى استدعاء المنسّق (السطر 355 في `9636466^` مقابل السطر 366 اليوم).

**النتيجة:** `ProfileResultItem.IsPrinted = false` **دائماً في الإنتاج**، وبما أن `AmendProfileResultCommandHandler:43-46` يحرس `if (!item.IsPrinted)` ⇒ **ميزة التعديل بعد الطباعة صارت معطّلة كلياً**.

### المشكلة (ج) — ستّ حراسات كانت معطّلة بصمت

لأن العَلَمة لم تُضبط، كانت هذه القواعد مُعطَّلة بلا أي أثر مرئي:

| الحراسة | الموضع | الأثر على المختبر |
|---|---|---|
| تسليم النتيجة | `PatientTest.cs:193` + `DeliveryHandoverViewModel.cs:34` | **التسليم محظور** بعد طباعة من شاشة الإدخال |
| التعديل بعد الطباعة | `AmendProfileResultCommandHandler.cs:40` | الخيار مقفل |
| إلغاء الاعتماد | `PatientTest.cs:145` | كان مسموحاً لنتيجة طُبعت |
| مسح النتيجة | `PatientTest.cs:131` | كان مسموحاً لنتيجة طُبعت |
| حفظ البروفايل الشامل | `SaveProfileResultsCommandHandler.cs:104,120` | كان **يمحو** التصحيحات |
| قفل شاشة المزرعة | `CultureEntryViewModel.cs:155` | كان يبقى مفتوحاً بعد الطباعة |

### المشكلة (د) — انحدار مُستحدث من Wave 2

مُثبت بــ `git grep "\.MarkPrinted(" -- src`:

| الحالة | مواقع الكتابة في الإنتاج |
|---|---|
| قبل Wave 2 (`9636466^`) | **6** (كل المسارات تُسجّل) |
| عند `ee9acf8` | **4** (اثنان منها في أوامر ميتة) |

قبل الموجة 2 كان السلوك **متّسقاً**. الموجة 2 هي التي أنشأت الانقسام: أضافت منسّقاً «لا يُسجّل» وحوّلت إليه ثلاثة مسارات، **دون** تحديث P4/P5 و**دون** مراجعة مستهلكي العَلَمة.

---

## 2) كيف فهم التدقيق المحلي المستقل

بعد إكمال Wave 2، أجريتُ تدقيقاً للقراءة فقط على الحالة الحالية. فحصتُ كل مسار طباعة من زرّ الواجهة حتى المعالِج، وبحثتُ عن كل قارئ وكاتب لحالة «مطبوعة». النتيجة:

- **أكّدت** وجود المسارين المتباينَين بالأدلة السطرية.
- **اكتشفتُ** أثراً إضافياً لم تُشَر في أي تقرير: أن حراسة `ProfileResultItem.IsPrinted` تحمي **أيضاً** من الحذف في الحفظ الشامل — أي أن الإصلاح سيُعيد تفعيلها كأثر إيجابي.
- **رفعتُ**ÉS Finding إلى المالك بوصفه غير قابل للتنفيذ على يد المبرمج وحده، لأنه يمسّ معنى بيانات لا يُستنبط من الكود.

## 3) كيف فهم تدقيق الوكيل السحابي

قرأ الوكيل السحابي الكود بنفسه وقدم ثلاث نتائج جوهرية:

- **المسارات الخمسة** materialmente مختلفة، مع دليل عدم التقاطع عبر غياب `using` في المنسّق.
- **تصويب سببي:** التباين **ليس قديماً** بل أحدثته Wave 2 (6 مواقع ← 4 مواقع).
- **نتيجة أشدّ:** التعديل بعد الطباعة معطّل كلياً في الإنتاج.

كما اكتشف تفاصيل إضافية: أن `MarkResultPrintedCommand` ميت أيضاً، ووجود `using` عتيق باقٍ في ملفَّي الواجهة.

## 4) الأدلة التي أرست المشكلات

| الدليل | الأمر/الموضع | النتيجة |
|---|---|---|
| عدّ مواقع الكتابة | `git grep "\.MarkPrinted(" HEAD -- src` | 4 مواقع |
| نفس الأمر قبل الموجة | `git grep "\.MarkPrinted(" 9636466^ -- src` | 6 مواقع |
| موت الأوامر | `grep -rn "new Mark.*PrintedCommand" src/` | **صفر** |
| موت `MarkResultPrintedCommand` | `grep -rn "new MarkResultPrintedCommand" src/` | **صفر** |
| انفصال البند | `grep "\.MarkPrinted(" src/` | المستدعي الوحيد للبند هو معالِج ميت |
| **غياب التغطية العابرة** | تقاطع ملفات الاختبار بين (التسليم/التعديل) و(المنسّق) | **فارغ تماماً** |
| ترتيب يدوي في الاختبارات | `DeliverWithSettlementCommandHandlerTests.cs:33` · `AmendProfileResultCommandHandlerTests.cs:24` | `pt.MarkPrinted(...)` يدوياً |

**لماذا نجحت الاختبارات رغم العطل:** كل اختبار كان يجيب عن سؤال واحد، ولا يوجد اختبار **يسلسل** «اطبع ← سلّم» أو «اطبع ← صحّح». أي أن حذف كل `MarkPrinted` من الإنتاج كان سيُبقي كل اختبارات التسليم والتعديل خضراء.

## 5) قرار المالك بالإصلاح قبل Wave 3

قرأ المالك النتائج، وأقرّ بأن العيوب مؤكَّدة، ثم قرار:

> **Option (B) — إصلاح المشكلات المؤكَّدة قبل بدء Wave 3.** لا تجاهل، ولا تأجيل إلى Wave 3، ولا حملها كديون تقنية.

## 6) قرارات المالك الثلاثة (إلزامية)

| # | القرار | نصّه |
|---|---|---|
| **1** | **معنى «مطبوع»** | «مطبوع = طباعة ناجحة. يجب أن تُسجَّل الحالة بعد نجاح العملية، **بشكل متّسق على جميع مسارات الطباعة الإنتاجية**، ولا تُسجَّل قبل النجاح. **لا تُفسَّر هذا القرار كـ«لا تُعلِّم أبداً»**.» |
| **2** | **التعديل بعد الطباعة** | «يبقى متاحاً. لا يجوز تعطيله أو إعادة تصميمه حلاً للعيب، ويجب إصلاح العطل مع الاحتفاظ به.» |
| **3** | **الأوامر الميتة** | «يُحتفظ بـ`MarkProfilePrintedCommand` و `MarkCultureReportPrintedCommand`. لا تُحذف لمجرد عدم وجود مستدعٍ. إعادة تقييمها تُؤجَّل لما بعد إغلاق Wave 2.» |

**قرار رابع (حُسم أثناء التنفيذ):** عند طباعة بروفايل ناجحة تحتوي بنداً غير مُعتمد (`!IsVerified`)، يَفشل الطلب برسالة النطاق القائمة — خيار **صارم**،/message: «المادة غير معتمدة؛ لا يمكن طباعتها.»

---

## 7) المجالات الوظيفية المتأثرة

شاشات إدخال البروفايل · شاشة إدخال المزرعة · الطباعة الجماعية · التقرير المجمَّع · تقرير تاريخ المريض · شاشة التسليم · شاشة تعديل نتيجة البروفايل · حفظ البروفايل الشامل · إلغاء اعتماد المزرعة · عدّادات «مطبوعة» في السجلات · تحذير إعادة الطباعة.

---

## 8) الإجراءات التصحيحية المنفَّذة فعلياً

### 8.1 خدمة تسجيل واحدة جديدة (الملف الوحيد الجديد)

**`src/TopLab.Application/Features/ResultsEntry/Common/IPrintedStateRecorder.cs`**

```csharp
Task<Result> RecordAsync(IReadOnlyCollection<int> patientTestIds,
                          IReadOnlyCollection<int> profileResultItemIds, CancellationToken ct = default);
Task<Result> RecordForPatientTestAsync(int patientTestId, CancellationToken ct = default);
```

- تعمل **بعد** نجاح الطباعة فقط، وتحفظ في `SaveChangesAsync` **واحدة**.
- تحوّل `InvalidOperationException` إلى رسائل النطاق القائمة عبر مترجم مُدمج (بلا نص عربي جديد).
- **لا تحفظ شيئاً** إذا كانت القائمتان فارغتين.
- `RecordForPatientTestAsync` تحلّ بنود البروفايل من قاعدة البيانات، فيتوافق ما يُسجَّل مع ما طُبع فعلاً.

### 8.2 المسارات التي صُلِحت

| الملف | ما تغيّر |
|---|---|
| `ExecuteBulkPrintCommandHandler.cs` | تسجيل عند `outcome.Printed` فقط؛ الفاشل لا يُسجَّل. **أعاد تفعيل تحذير إعادة الطباعة** |
| `PrintCombinedReportCommandHandler.cs` | استُبدلت حلقة الوسم اليدوية بالخدمة، مع تسجيل بنود البروفايل المحمولة في التقرير |
| `PrintHistoryReportCommandHandler.cs` | استُبدلت حلقة الوسم بالخدمة. **بلا بنود** — تقرير التاريخ لا يقرأ `ProfileResultItem` |
| `ProfileEntryViewModel.cs` | تسجيل عند `outcome.Printed` + **حذف `using` عتيق** |
| `CultureEntryViewModel.cs` | تسجيل عند `outcome.Printed` + **حذف `using` عتيق** |
| `Application/DependencyInjection.cs` | تسجيل `IPrintedStateRecorder` بـ`AddScoped` |

### 8.3 ملفات لم تُمسّ (مقصوداً)

`IResultPrintCoordinator.cs` و`ResultPrintCoordinator.cs` (لم يتغيّر عقد SD-1) · `MarkResultPrintedCommandHandler.cs` · `MarkProfilePrintedCommandHandler.cs` · `MarkCultureReportPrintedCommandHandler.cs` (الأوامر الثلاثة محتفظ بها بحكم القرار 3) · `AmendProfileResultCommandHandler.cs` · **كل `src/TopLab.Domain/`** · كل `Infrastructure/Persistence/`.

**لماذا لم يُوسَّع `IResultPrintCoordinator`:** تقرير المجمَّع والتاريخ يُخرجان حمولات مختلفة (`CombinedReportDto` مرتّبة مقابل `PatientHistoryDto` بمستوى مريض)، وتقرير التاريخ أصلاً **لا يحمل بنود بروفايل**. إدخالهما في المنسّق = إعادة بناء نظام الطباعة، وهو أبعدُ من إصلاح العطل. فالتسجيل انفصل، والبناء بقي كما هو.

---

## 9) الاختبارات المضافة والمعدَّلة

### مُضافة (6 اختبارات) — `tests/TopLab.Application.Tests/Features/ResultsEntry/PrintToDownstreamStateTests.cs`

| # | الاختبار | ما يثبته |
|---|---|---|
| 1 | `PrintCombinedReport_ThenDeliver_SucceedsWithoutManualMarking` | سلسلة «اطبع ← سلّم» **دون ترتيب يدوي للحالة** |
| 2 | `PrintProfile_ThenAmend_SucceedsWithoutManualItemMarking` | سلسلة «اطبع بروفايل ← صحّح» — القرار 2 عبر الأمر الحقيقي وحارسه الحقيقي |
| 3 | `Deliver_BeforePrinting_IsStillRefused` | الاتجاه السلبي: التسليم يبقى مرفوضاً قبل الطباعة |
| 4 | `Record_WithUnverifiedProfileItem_FailsAndRecordsNothing` | القرار الصارم: لا حفظ جزئي |
| 5 | `Record_WithNoIds_DoesNotSave` | لا حفظ بلا عمل |
| 6 | **دعامة مشتركة** `FakePrintedStateRecorder` | افتراضاً **يستخدم التنفيذ الحقيقي** فوق القاعدة المزيفة، حتى تُنفَّذ انتقال النطاق الحقيقي في كل اختبار |

### مُعدَّلة

| الموضع | قبل | بعد | السبب |
|---|---|---|---|
| `BulkCommandHandlerTests.cs` | `Assert.Equal(before, PrintCount)` · `Assert.False(IsPrinted)` | `Assert.Equal(before + 1, …)` · `Assert.True(IsPrinted)` | طباعة ناجحة ⇒ تُسجَّل (القرار 1) |
| `BulkPrintHonestyTests.BulkPrint_NeverMarksPrinted` | «لا يُسجّل أبداً» | أُعيدت التسمية إلى `BulkPrint_SuccessfulPrint_RecordsPrintedState` وتوقّعها `True`/`1` | **الصدق = «لا ورقة ⇒ لا وسم»، لا «لا وسم أبداً»** |
| `BulkPrintHonestyTests` | — | **جديد:** `BulkPrint_FailedPrint_LeavesPrintedStateUnset` | يثبت النصف الآخر من الصدق |
| `BulkCommandHandlerTests.Execute_BalanceBlock…` | `Assert.True(IsPrinted)` | `Assert.False(IsPrinted)` | بوابة الرصيد تمنع **قبل** أي ورقة ⇒ لا تسجيل |
| `PrintCombinedReportCommandHandlerTests` · `PrintHistoryReportCommandHandlerTests` | — | تمرير `FakePrintedStateRecorder` في 15 موقع بناء | توافق مع التوقيع الجديد |

**الحاجز البنيوي:** الاختبارات الجديدة **لا تستدعي `MarkPrinted` يدوياً إطلاقاً**. لو انكسر التسجيل مرة أخرى لأفشلت هذه الاختبارات — وهذا هو الضمانة التي كان يفتقدها.program قبل الإصلاح.

---

## 10) نتائج التحقق (أرقام حقيقية من التشغيل)

| البند | قبل الإصلاح | بعد الإصلاح |
|---|---|---|
| **البناء** | 0 تحذير / 0 خطأ | **0 تحذير / 0 خطأ** |
| `Domain.Tests` | 507 | **507** ناجح · 0 فاشل |
| `Application.Tests` | 1583 | **1589** ناجح · 0 فاشل · (+6) |
| `Infrastructure.Tests` | 265 | **265** ناجح · 0 فاشل |
| `Presentation.Tests` | 67 | **67** ناجح · 0 فاشل |
| `Persistence.Tests` | 13 ناجح · 2 متخطّاة | **13 ناجح · 2 متخطّاة** (لا Docker) |
| **الإجمالي** | 2435 / 0 / 2 | **2441 ناجح · 0 فاشل · 2 متخطّاة** |

**فحوص البنية:**

| الفحص | النتيجة |
|---|---|
| `dotnet ef migrations has-pending-model-changes` | **No changes** — صفر انحراف نموذج |
| `git status -- Migrations/` + `ApplicationDbContextModelSnapshot.cs` | **فارغ** — لم تُمسّ |
| عدد ملفات الهجرات | **29** (= 14 هجرة × 2 + لقطة) — بلا تغيير |
| `grep "MarkPrinted" ResultPrintCoordinator.cs` | **0** — عقد SD-1 سليم |
| `grep` على `using` العتيق في الواجهتين | **0** — نُظِّفا |
| `git diff -- src/TopLab.Domain/` | **فارغ** — النطاق لم يُمسّ |

**Migration Required: No** — لا هجرة جديدة، ولا هجرة رابعة، ولا تعديل على أي هجرة قائمة، ولا ترحيل بيانات. الأعمدة (`IsPrinted`, `PrintCount`) موجودة ومخطَّطة في `PatientTestConfiguration.cs:33-46` و`ProfileResultItemConfiguration.cs:20-21` منذ الـbaseline.

---

## 11) الكوميتات

**لم يُنشأ أي كوميت.** بحكم قاعدة المالك «**الحفظ والدفع بيد المالك**»، تُركت التعديلات في شجرة العمل بانتظار مراجعته:

```
 On branch main
 modified:   src/TopLab.Application/DependencyInjection.cs
 modified:   .../PrintCombinedReport/PrintCombinedReportCommandHandler.cs
 modified:   .../PrintHistoryReport/PrintHistoryReportCommandHandler.cs
 modified:   .../ExecuteBulkPrint/ExecuteBulkPrintCommandHandler.cs
 modified:   .../ResultsEntry/Common/IPrintedStateRecorder.cs            (جديد)
 modified:   .../Patients/ProfileEntryViewModel.cs
 modified:   .../Patients/CultureEntryViewModel.cs
 modified:   4 ملفات اختبار معدَّلة + 2 جديدان
 untracked:  Docs/… (تقرير السحابة، خطة السحابة، هذه الوثيقة)
```

**تقسيم مقترح متاح عند موافقة المالك:**

| # | الكوميت | المحتوى |
|---|---|---|
| C1 | `Record printed state in one application service` | الخدمة الجديدة + تسجيل DI |
| C2 | `Record printed state on the entry-screen and bulk print paths` | P1/P2/P3 + حذف الـ`using` العتيق |
| C3 | `Unify printed-state recording in combined and history reports` | P4/P5 + كل الاختبارات |
| C4 | `Fixes for issues that emerged after the implementation of the second wave..md` | هذه الوثيقة |

---

## 12) الحالة النهائية لكل عيب

| العيب | الحالة |
|---|---|
| (أ) انقسام مسارات الطباعة | ✅ **مُصلَح ومُتحقَّق** — المسارات الخمسة تستخدم خدمة واحدة بعد نجاح الطباعة |
| (ب) انفصال `ProfileResultItem.IsPrinted` | ✅ **مُصلَح ومُتحقَّق** — الاختبار 2 يثبته عبر الأمر الحقيقي |
| (ج) ستّ حراسات معطّلة | ✅ **مُنفَّذة ضمن الإصلاح** — عادت للعمل تلقائياً بلا تعديل |
| (د) انحدار Wave 2 | ✅ **مُصحَّح** — الفجوة السببية أُغلقت |
| حدّ تصميمي: تقرير التاريخ لا يفعّل التعديل | ⚠️ **موثّق، خارج النطاق** — `PatientHistoryReader` لا يقرأ `ProfileResultItem` أصلاً؛ فتح التعديل عبر تقرير التاريخ يتطلب توسيع حمولة `HistoryEntryDto`، وهو تغيير منفصل لم يُطلب |
| التزامن المتزامن | ⚠️ **مقبول** — لا يوجد `RowVersion`؛ watch-list بلا هجرة |
| الأوامر الثلاثة الميتة | ✅ **محتفظ بها بحكم القرار 3** — لم تُحذف ولا أُعيدت تسميتها |

---

## 13) الأثر المُصلَح جانبياً (حراسات عادت للعمل)

بمجرد ضبط العَلَمة عادت هذه القواعد للعمل **تلقائياً** بلا أي تعديل في الكود:

1. **تسليم النتيجة** متاح بعد الطباعة من أي مسار.
2. **التعديل بعد الطباعة** متاح (القرار 2).
3. **الحفظ الشامل للبروفايل لم يعد يمحو التصحيحات** — سلوك تحسّن جوهري.
4. **منع إلغاء اعتماد** بند مطبوع.
5. **منع مسح** نتيجة مطبوعة.
6. **قفل شاشة المزرعة** بعد الطباعة.
7. **تحذير إعادة الطباعة** في الطباعة الجماعية عاد للعمل.
8. **عدّادات «مطبوعة»** في السجلات صارت تعكس الواقع.

**نقطة تستحق انتباه المالك:** البند 3 يعني أن تصحيح نتيجة بروفايل مطبوعة لم يعد قابلاً للحذف عند الحفظ الشامل. هذا سلوك أفضل، لكنه **تغيّر فعلي** في مسار الحفظ.

---

## 14) التأكيد الصريح

أُجريت هذه الإصلاحات **قبل** أي تخطيط أو تنفيذ لـWave 3.

لم تُؤجَّل أي مشكلة مؤكَّدة إلى Wave 3، ولم يُنشأ أي حلّ مؤقت مؤجَّل التنظيف. المشكلتان (أ) و(ب) مُغلقتان ومُتحقَّق منهما باختبارات تسلسلية حقيقية، والمشكلتان (ج) و(د) تبعاً لهما.

المشكلتان الوحيدتان المتبقيتان (**حدّ تقرير التاريخ** و**التزامن**) موثّقتان صراحةً في §12 كحدود معروفة لا كأعيا�� مفتوحة، وكلتاهما خارج نطاق قرار «إصلاح عيوب حالة الطباعة».