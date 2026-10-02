# خطة معالجة العيوب — مقدَّمة من مزوّد السحابة

**المشروع:** Top-Lab
**الالتزام المُدقَّق:** `ee9acf8a7e60443f38c45e1958cc36c51401f55d`
**نوع الوثيقة:** خطة فنية فقط — **لم يُنفَّذ أي تعديل على المشروع**
**تاريخ الإعداد:** 2026-10-02

---

## 0. حالة الوثيقة аналог — قراءة مسبقة

هذه الوثيقة **خطة** فقط. لم يُنشأ أي commit، ولم تُعدَّل أي ملفات مصدرية، ولم تُضَف أو تُحذف أي migrations، ولم تُنشأ ملفات داخل المستودع. شجرة العمل remained نظيفة طوال التحقيق (`git status --porcelain` فارغ).

### حدود المُدقِّق

| البند | القيمة |
|---|---|
| المستودع | `https://github.com/El-ogra/Top-Lab.git` |
| HEAD المُتحقَّق | `ee9acf8a7e60443f38c45e1958cc36c51401f55d` (مطابقة حرفية) |
| الفرع | HEAD منفصل (الالتزام ضمن `main`) |
| تعديلات على المستودع | **صفر** |

---

## 1. قرارات المالك (قيد ملزم — ليست مقترحات)

هذه قرارات مُلزِمة اعتمدها المالك بعد مراجعة النتائج. يجب أن تحترمها هذه الخطة بالكامل، ويجب ألا تقترح أي قرار بديل يناقضها.

> **قرار 1 — Printed = successful printing**
> لا تُسجَّل حالة "مطبوع" إلا **بعد نجاح** عملية الطباعة ذات الصلة، ويجب تطبيق هذا القاعدة **بشكل متسق على جميع مسارات الطباعة الإنتاجية**.
>
> **قرار 2 — amending يبقى متاحًا بعد الطباعة**
> طباعة البروفايل **لا يُحظر** نهائيًا تعديل البروفايل. يجب الحفاظ على إمكانية التعديل بعد الطباعة.
>
> **قرار 3 — الإبقاء على أوامر حالة الطباعة الحالية مؤقتًا**
> يجب بقاء `MarkProfilePrintedCommand` و`MarkCultureReportPrintedCommand` في النظام حاليًا. إعادة تقييم ضرورتهما أو إعادة تصميمها تُؤجَّل لمراجعة المشروع اللاحقة.

**أثر القرار 3 على هذه الخطة:** لا يجوز حذف أي من الأمرين، ولا يجوز إضافة `Send` جديد لهما في الواجهة. تظل حيّة عبر MediatR لكن غير مستدعَاة (انظر §3.4). إعادة ربطهما بـ coordinator — إن لزم — يجب أن يكون **توجيهاً داخليًا محافظًا**، لا إعادة تشغيل لهما كواجهة جديدة.

---

## 2. منهجية التحقق (Evidence-First)

أُعيد بناء كل مسار إنتاج من المصدر مباشرة، **دون** الاعتماد على أسماء الاختبارات أو التوثيق. التسلسل المُتبع:

1. `git clone` + تثبيت الالتزام المطلوب + `git rev-parse HEAD` + `git status --porcelain`
2. `git grep "\.MarkPrinted("` على HEAD **و** على `9636466^` (ما قبل S5) للمقارنة السببية
3. تتبّع كل استدعاء طباعة من `Send(...)` في `src/TopLab.Presentation` حتى المِعالِج
4. فحص كيانات النطاق و EF Core configurations و migrations
5. تنفيذ الاختبارات الفعلية

### 2.1 تعذّر بسيط يجب التصريح به

في التعليمات أُشير إلى التقرير بـ `Docs/TopLab_Wave2_Independent_Audit_ee9acf8.md`. **هذا المسار غير موجود في المستودع** عند الالتزام المُدقَّق:

```
$ ls Docs/TopLab_Wave2_Independent_Audit_ee9acf8.md
ls: cannot access ...: No such file or directory
$ git ls-files | grep -i audit
.opencode/agents/auditor-final.md
Docs/Investigations/independent-security-and-test-theater-audit.md
Docs/project-completeness-audit.md
```

تقرير التدقيق الذي أنتجه هذا المُدقِّق سابقًا في-wave موجودة في مساحة العمل الخارجية، **وليست ملفًا متتبَّعًا في git**. لذلك تعاملت معه كـ«ادعاء يجب التحقق منه» لا كـ«مصدر حقيقة». هذا موافق لقاعدة الأدلة (المستوى 6: التوثيق أدنى من المصدر).

**لا توجد تبعية في هذه الخطة على ذلك الملف.** كل استنتاج أدناه مُسنَد إلى سطر مُحدَّد في المصدر.

---

## 3. إعادة بناء العيوب من المصدر

### 3.1 خريطة مسارات الطباعة الإنتاجية (مُتحقَّق منها)

الفحص الشامل لـ `grep -rn "Send(new .*Print.*Command" src` + `grep -rn "IResultPrintCoordinator|IReportPrintingService" src/TopLab.Presentation` أعطى الصورة التالية:

| # | نقطة الدخول (UI) | المُعالِج | يمرّ عبر coordinator؟ | يُسجّل `PatientTest.IsPrinted`؟ | يُسجّل `ProfileResultItem.IsPrinted`؟ |
|---|---|---|---|---|---|
| P1 | `ProfileEntryViewModel.PrintCommand` (سطر 113 → 353-383) | `ResultPrintCoordinator` | ✅ نعم | ❌ **لا** | ❌ **لا** |
| P2 | `CultureEntryViewModel` (سطر 468-470) | `ResultPrintCoordinator` | ✅ نعم | ❌ **لا** | n/a (لا بنود بروفايل) |
| P3 | `BulkPrintDialogViewModel` → `ExecuteBulkPrintCommandHandler` (سطر 83) | `ResultPrintCoordinator` | ✅ نعم | ❌ **لا** | ❌ **لا** |
| P4 | `CombinedReportViewModel.PrintAsync` (سطر 272-274) | `PrintCombinedReportCommandHandler` | ❌ **لا** | ✅ نعم (سطر 77) | ❌ **لا** |
| P5 | `HistoryReportsViewModel` (سطر 307) | `PrintHistoryReportCommandHandler` | ❌ **لا** | ✅ نعم (سطر 82) | ❌ **لا** |

مسارات أخرى في النظام (`PrintInvoice`, `PrintReceipt`, `PrintBarcode`, `PrintWorkSheet`, `PrintBlankReport`) **خارج النطاق**: لا تلمس `PatientTest`. تحققت من `PrintBlankReportCommandHandler` — لا `MarkPrinted`، بتعليق صريح `OD-07-E` (سطر 51).

**المحصّلة: 5 مسارات إنتاجية، تنقسم إلى مجموعتين متمايزتين ماديًا، ولا تقاطع بينهما.**

### 3.2 العيب (أ) المؤكَّد — انقسام المسارات

**الدليل على عدم التقاطع:** `ResultPrintCoordinator.cs` لا يحتوي استيرادًا (`using`) لأي من `PrintCombinedReportCommandHandler` أو `PrintHistoryReportCommandHandler`. المنسّق يبني حزمته بنفسه عبر `BuildCombinedReportCommand` → `ReportPrintEnvelope.CreateToken` → `_printing.PrintReportAsync`.末端 الطابعة مشترك، لكن **جزء حالة النطاق مفصول تمامًا**.

الحارس البنيوي موجود فعلًا: `grep -n "MarkPrinted" src/TopLab.Application/Features/ResultsEntry/Common/*.cs` يُرجع **سطرًا واحدًا فقط**، وهو تعليق توثيقي في `IResultPrintCoordinator.cs:37`. هذا بالضبط ما يدّعيه الكود («the forbidden method name is deliberately absent from this file»).

### 3.3 العيب (ب) المؤكَّد — **انحدار مُستحدث من Wave 2**

هذه أهم نقطة في التقرير، وأعيد التحقق منها مستقلة:

```
# قبل Wave 2 (9636466^):
$ git grep -c "\.MarkPrinted(" 9636466^ -- 'src/*'
→ 7 مواقع

# عند HEAD:
$ git grep -c "\.MarkPrinted(" HEAD -- 'src/*'
→ 4 مواقع
```

المواقع المحذوفة تحديدًا (تحققت منها بـ `git show`):
- `ExecuteBulkPrintCommandHandler.cs:76` — كان `pt.MarkPrinted(...)`، صار coordinator
- `MarkResultPrintedCommandHandler.cs:57` — كان `pt.MarkPrinted(...)`، صار coordinator
- `ProfileEntryViewModel` — كان `Send(new MarkProfilePrintedCommand(...))` (سطر 355 في `9636466^`)، صار coordinator

**الاستنتاج:** قبل Wave 2 كانت **كل** مسارات الإنتاج تُسجّل الحالة. Wave 2 (S4/S5/S6) هي التي أنشأت الانقسام، بإضافة منسّق «لا يُسجّل» وتحويل ثلاثة مسارات إليه **دون تحديث `PrintCombinedReport` و`PrintHistoryReport`**.

هذا انحدار ناتج عن فعل الإصلاح — أخطر من تعارضين متعايشين، لأنه يعني أن 절반 الثاني من الإصلاح لم يُطبَّق.

### 3.4 حالة أوامر حالة الطباعة (تحققت مستقلاً)

فحص الاستدعاءات في `src/` فقط:

```
$ grep -rn "MarkProfilePrintedCommand\b\|MarkCultureReportPrintedCommand\b" src/
→ تعريفات + validators + معالِجات فقط
→ لا يوجد أي  new MarkProfilePrintedCommand(...)  في الإنتاج
→ لا يوجد أي  new MarkCultureReportPrintedCommand(...)  في الإنتاج
```

**الأمران ميتان في الإنتاج.** ووجدت تفصيلاً إضافياً لم يذكره التقرير السابق: **`using` directives عتيقة باقية**:

```
src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs:7:
    using TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted;
src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs:3:
    using TopLab.Application.Features.CultureResults.Commands.MarkCultureReportPrinted;
```

`using` بلا استخدام = **أثر مادي для الإصلاح غير المكتمل**، قابل للفحص الآلي. سأستغله كمؤشر جودة في معايير القبول (§10).

**وحالة ثالثة اكتشفتها:** `MarkResultPrintedCommand` —Despite أن S5 «أسلكه» للمنسّق (القرار SD-16)، فهو أيضًا **بلا أي مستدعٍ في الإنتاج**:

```
$ grep -rn "MarkResultPrinted" src/TopLab.Presentation/
→ (فارغ)
```

أي أن **الأوامر الثلاثة** `MarkResultPrinted` و`MarkProfilePrinted` و`MarkCultureReportPrinted` ميتة في الإنتاج. لم يُذكر هذا في التقرير السابق.

### 3.5 العيب (ج) المؤكَّد — انفصال حالة البند عن حالة الاختبار

هذه أخطر نتيجة في التدقيق، وأُعيد تأكيدها:

- `ProfileEntryViewModel.CanAmend => IsPrinted` (سطر 61) — و`IsPrinted` هنا من `GetProfileEntryGridQueryHandler:61` وهو `item?.IsPrinted ?? false` أي **`ProfileResultItem.IsPrinted`**، وليس `PatientTest.IsPrinted`.
- `AmendProfileResultCommandHandler:43-46` يحرس `if (!item.IsPrinted) return Conflict("لا يمكن تعديل نتيجة البروفايل قبل الطباعة.")`.
- `ProfileResultItem.MarkPrinted` (سطر 128-139) هو **الطريقة الوحيدة** لضبط `ProfileResultItem.IsPrinted`.
- مستدعيها الوحيد في الإنتاج كان `MarkProfilePrintedCommandHandler:71` — وهو **ميت** (§3.4).
- `PrintCombinedReportCommandHandler` يستعلم `PatientTest` فقط (سطر 72) — لا يمس `ProfileResultItem` إطلاقاً. تحققت: `BuildCombinedReportCommandHandler` يقرأ `ProfileResultItem` للقراءة فقط (سطور 78-91)، ولا `MarkPrinted`.

**النتيجة المنطقية:**
- P1 (بروفايل): لا يُسجّل لا `PatientTest` ولا `ProfileResultItem` → البند `false`
- P4 (تقرير مدمج): يُسجّل `PatientTest` فقط → البند يبقى `false`
- P3 (جماعي): لا يُسجّل شيئًا
- ⇒ `ProfileResultItem.IsPrinted = false` **دائمًا في الإنتاج** ⇒ التعديل بعد الطباعة **مُعطَّل كليًا**

وهذا يناقض صراحةً **قرار المالك 2**. إن كانت الميزة صامتة الفشل، فهذا خرق وظيفي يجب إصلاحه ضمن هذا النطاق.

**ملاحظة نطاق مهمة:** البنية التحتية للتعديل (الحوار، `AmendDialogViewModel`, `AmendmentsLogViewModel`, سجل `ProfileResultAmendment` الدائم) سليمة وworking — العطل في **الوصول** فقط. لذا الإصلاح هو إعادة توصيل البوابة، لا إعادة بناء الميزة.

### 3.6 ترحيل الطباعة متعدد المستويات (شفافية)

`ExecuteBulkPrintCommandHandler` يمرّ على حلقة `foreach` فوق اختبارات مريض واحد (سطور 81-97). عند الفشلContinued 나머ون ولا يُلغون الدفعة (سلوك S6 المقصود)، ويصبح الناتج `BulkPrintOutcomes.Failed` مع `printed` = عدد الناجحين.

**تقييم:** هذا نمط تصميمي مقصود (W-02 S6) وليس عيبًا. **لكنه يصبح هشًّا بمجرد إضافة تسجيل الحالة**: بعد الإصلاح ستُسجَّل حالة الاختبارات الناجحة فقط، بينما يُبلَّغ الدفعة كـ`Failed`. هذا سلوك متسق عمدًا (ما طُبع فعلاً هو ما سُجِّل)، ويجب **تثبيته باختبار** لا تغييره. **لا أقترح تعديله** — توثيقه كسلوك مقصود مُثبَّت.

### 3.7 نقاط لم أستطع تأكيدها (تصنيف صريح: غير مُتحقَّق)

| النقطة | الحالة | السبب |
|---|---|---|
| وجود اختبار تكامل حقيقي يغطي طباعة→تسليم | **غير موجود** | بحثتُ عن `ResultPrintCoordinator` معبَّرًا عن التسليم → صفر نتائج |
| صلاحية SQL المُنتَجة من إصلاح F1 | **غير مُتحقَّق** | لا SQL Server في هذه البيئة — خارج النطاق |
| سلوك إعادة الطباعة عبر الواجهة فعليًا | **غير مُتحقَّق** | `BulkPrintPreflightQueryHandler:43` يحسب `verified.Any(pt => pt.IsPrinted)` لكنه **لن يكون له أثر** بعد الإصلاح ما لم تُصلَح الفجوة (انظر §5.2) |
| موت `PrintResultSheet` / شاشات أخرى | **غير مُطبَّق** | `SimpleResultEntryViewModel` و`ResultsWorklistViewModel` لا تحتوي أي استدعاء طباعة — **`ResultsWorklistView.xaml:81` يعرض العمود فقط** |

---

## 4. تحليل نزاهة الاختبارات (مُتحقَّق منه بالتنفيذ)

### 4.1 نتائج التنفيذ الفعلية

| المشروع | النتيجة |
|---|---|
| `TopLab.Application.Tests` | ✅ **1583 / 1583** ناجح |
| `TopLab.Domain.Tests` | ✅ **507 / 507** ناجح |
| `TopLab.Persistence.Tests` | ✅ 13 ناجح، 2 متخطّى (لا Docker) |
| `TopLab.Infrastructure.Tests` | ⚠️ 247 ناجح / **18 فاشل — بيئي بالكامل** |
| `TopLab.Presentation` | غير قابل للبناء على Linux (WPF / Windows) |

**تحليل الـ18 فاشلة:** جميعها `QuestPDF ... font families that are not available: 'Arial'`. `fc-list | grep -ci "arial\|lato"` = **0**. الاستثناء يُبتلَع في `ReportPrintingService:79-83` فيعود `Result.Failure`، فتسقط التأكيدات اللاحقة بالتسلسل. **نتيجة غياب خطوط Windows على Linux، لا خلل في الكود.** لم تُعدَّل أي ملفات.

### 4.2 الفخّ الذي concealmentه الاختبارات

**(أ) اختبارات التسليم تُرتّب الحالة يدويًا — لا تطبع:**

```
DeliverWithSettlementCommandHandlerTests.cs:33   pt.MarkPrinted(1, Now);
GetDeliveryGridQueryHandlerTests.cs:31            pt.MarkPrinted(1, DateTime.UtcNow);
GetUndeliveredResultsQueryHandlerTests.cs:34      pt.MarkPrinted(1, Day);
```

**الأثر:** لو حُذف كل `MarkPrinted` من الإنتاج بالكامل، لَبقيت كل اختبارات التسليم خضراء. تُثبت أن التسليم يعمل *بشرط* `IsPrinted`، ولا تُثبت *كيف* يصبح `IsPrinted` صحيحًا.

**(ب) اختبار التعديل يتفادى العيب بنيويًا:**

```
AmendProfileResultCommandHandlerTests.cs:24   pt.MarkPrinted(1, DateTime.UtcNow);
                                  +  ProfileResultsSeed.AddPrintedItem(..., isPrinted: true)
```

`AddPrintedItem` (سطر 74-92) يُنشئ البند بـ `isPrinted: true` مباشرةً. **يتجاوز الطباعة كليًا** ⇒ العيب §3.5 غير مكتشف.

**(ج) الاختبارات測 «الطرفين» تثبت التباين ولا تحكمه:**
- `PrintCombinedReportCommandHandlerTests:69-72` → **يؤكد** `Assert.True(row.IsPrinted)` و `PrintCount == 1`
- `ReviewPrintDeliverCommandHandlerTests:154-157` → **يؤكد** `Assert.False(row.IsPrinted)`، بتعليق صريح:
  > *"The old assertion (IsPrinted == true) asserted the very dishonesty WP-06 removes."*

كلاهما صحيح wrt سلوكه. لكن **لا يوجد اختبار يعبر من أحدهما إلى الآخر**، فلا يظهر التباين كخلل. مقاييس S5/S6 تقيس P1–P3 فقط، وقياسات M-07 تقيس P4–P5 فقط.

**المحصّلة:** 2093 اختبارًا ناجحًا + صفر تغطية للسيناريو المتقاطع. هذا بالضبط سبب بقاء العيب مرصودًا.

---

## 5. التصميم المقترح للإصلاح

### 5.1 المبدأ الحاكم

> **تسجيل حالة «مطبوع» ليس مسؤولية طباعة، بل مسؤولية **ما بعد نجاح الطباعة** — ويجب أن يحدث في مكان واحد، بعد نجاح الطابعة، لكل المسارات.**

المشكلة ليست أن P4/P5 يُسجّلان، بل أن التسجيل **مُوزَّع على مسارين بمعاملتين مختلفين**، وأن P1–P3 لا تُسجّل إطلاقًا. الإصلاح = **توحيد**، لا حذف.

**لماذا لا نحذف التسجيل من P4/P5؟** لأنه يخلّ بالمتطلبين 1 و2 معًا: إن حذف التسجيل، لنصبح P1–P3 وP4–P5 متسقين في «عدم التسجيل»، لكن التسليم سيصبح **مستحيلًا دائمًا** (لأن `MarkDelivered` يتطلب `IsPrinted`، `PatientTest.cs:193`)، والتعديل يظل معطّلًا. أي أن الحذف يعمّق العطل بدل إصلاحه.

### 5.2 القيد architectural المكتشف: فجوة الدفعة

التصميم المقترح أدناه يكشف مشكلة ثانية يجب حلها في نفس المعاملة:

`ExecuteBulkPrintCommandHandler:60` — `var requiresConfirmation = !suppressReprint && verified.Any(pt => pt.IsPrinted);`

**قبل الإصلاح:** كانت `IsPrinted` تُضبط بواسطة P4/P5، فكان هذا الشرط يعمل. **بعد توحيد P4/P5 على المنسّق:** إذا لم يُعد تسجيل P4/P5 (وهو المطلوب لدمجها)، فسيبقى `IsPrinted` خاطئًا للاختبارات المطبوعة عبر P4، **فسيتوقف التنبيه قبل إعادة الطباعة**.

**إذن إصلاح P4/P5 ليس تجميلًا — هو شرط لصحّة آلية إعادة الطباعة الحالية.** هذه تبعية يجب تعديلها في نفس المعاملة. (§5.4، الخطوة 3)

### 5.3 التعديل المقترح — طبقة registrations واحدة

**المبدأ:** التسجيل في طبقة التطبيق (Application)، في **خدمة واحدة**، تُستدعى **بعد** نجاح الطابعة.

**لماذا طبقة التطبيق وليس النطاق؟**
- النطاق (`Domain`) يملك الانتقال الصحيح بالفعل: `PatientTest.MarkPrinted` (178-189) و`ProfileResultItem.MarkPrinted` (128-139) — كلاهما يتحقق من الاعتماد ويincrements `PrintCount` ويسجّل المستخدم والوقت. **هذا صحيح ولا يحتاج تعديلًا.**
- ما ينقص هو **الربط** بين نجاح الطباعة واستدعاء هذا الانتقال.

**لماذا لا نضع التسجيل داخل `ResultPrintCoordinator` مباشرة؟**

هذا هو الخيار المبدئي، لكنني أتراجع عنه بعد الفحص، لسبب مثبت بالأدلة:

عقد `IResultPrintCoordinator` **مُعلَن** صراحةً كـ«build, then print, and **never mark**» (`IResultPrintCoordinator.cs:36-40`)، وهو قرار SD-1 الموثَّق، والعقد يحمل `ResultPrintOutcome` مصمَّمة لتُرجع `Printed` فقط دون أي مسؤولية عن الحالة. وتوسيعه لكسر موثَّق في S4–S6 سيُلغي قرارًا معمارياً مُتّفقًا عليه بلا ضرورة معمارية:add_state_required_here.

**الخيار المعتمد:** خدمة تطبيق منفصلة، e.g. `IPrintedStateRecorder` في `Features/ResultsEntry/Common/`، تستقبل `patientTestId` ومعرّفات البنود، وتستدعي بِناءً على `IApplicationDbContext` توابع النطاق. تبقى `IResultPrintCoordinator` **كما هي حرفياً**.

| البند | القرار |
|---|---|
| `IResultPrintCoordinator` | **لا يتغير** (احترام SD-1) |
| `IResultPrintCoordinator` DI registration | **لا يتغير** |
| `PatientTest.MarkPrinted` | **لا يتغير** (السطر 178) |
| `ProfileResultItem.MarkPrinted` | **لا يتغير** (السطر 128) |
| `PatientTest.MarkDelivered` | **لا يتغير** |
| `ProfileResultItem.Amend` | **لا يتغير** |
| `AmendProfileResultCommandHandler` | **لا يتغير** (الحراسة ستصبح قابلة للعبور بعد إصلاح التسجيل) |
| EF configurations | **لا تتغير** |
| Migrations | **لا تُضاف** (§6) |

**مسؤوليات الخدمة الجديدة (مقترحة، لأخذها كاتجاه لا كاسم ملزم):**
1. تُستدعى **فقط** بعد `print.IsSuccess == true`
2. للاختبار الأب: `pt.MarkPrinted(userId, utcNow)`
3. للبنود: `ProfileResultItem.MarkPrinted(userId, utcNow)` على بنود ذلك الاختبار
4. `await _db.SaveChangesAsync(ct)` — **مرة واحدة**
5. ترجمة `InvalidOperationException` عبر `DomainFailureTranslator` **المتوفر** (نمط قائم في `PrintCombinedReportCommandHandler:81`)

### 5.4 التعديلات على المسارات الخمسة

| المسار | التعديل | السبب |
|---|---|---|
| **P1** `ProfileEntryViewModel` | بعد `outcome.Printed` بنجاح → استدعاء التسجيل للاختبار **ولبنوده** | ينحني لقاراري 1 و2؛ يفتح التعديل |
| **P2** `CultureEntryViewModel` | بعد `outcome.Printed` → تسجيل الاختبار فقط | لا بنود بروفايل |
| **P3** `ExecuteBulkPrintCommandHandler` | داخل الحلقة، عند `outcome.Printed` فقط → تسجيل ذلك الاختبار | تسجيل ما طُبع فعلاً فقط |
| **P4** `PrintCombinedReportCommandHandler` | **يُعاد توجيهه** إلى المنسّق (بنفسه يبني)، والتسجيل ينتقل للخدمة | القرار 1 (اتساق)؛ يحل فجوة الدفعة §5.2 |
| **P5** `PrintHistoryReportCommandHandler` | **نفس الأمر** — مع إبقاء ترشيح `IsReviewed` القائم | القرار 1؛Enrollment نفس الفجوة |

**ملاحظة على P4/P5 — قرار تصميمي متعذّر حسمه بالملكية:**

التصميم أعلاه يفترض **إعادة توجيه P4/P5 إلى `IResultPrintCoordinator`**. لكن المنسّق يُبنى عليه:
- `BuildProfileTokenAsync` يرسل `GetProfileReportQuery` ويغلّفه في `CombinedReportDto` داخليًا
- `BuildCombinedTokenAsync` يرسل `BuildCombinedReportCommand(patientId, [patientTestId])` — **اختبار واحد فقط**
- `BuildBlankTokenAsync` يرسل `BuildBlankReportCommand(patientId)`

بينما P4 يقبل **قائمة مرتبة** `request.OrderedPatientTestIds` (اختبارات متعددة) وP5 يبني من `GetSeparateHistoryReportQuery` (مستوى المريض، ليس مستوى الاختبار). **فلا يمكن استدعاء المنسّق الحالي بأي منهما دون توسيع واجهته** — وهو ما يخالف «لا تغيّر IResultPrintCoordinator» أعلاه.

**لذلك أطرح هذا كسؤال مفتوح (§13) لا كقرار مفترض**، مع ثلاثة خيارات متاحة:

- **خيار أ (المفضّل):** توسيع `IResultPrintCoordinator` بقدرة `PrintManyAsync(patientId, IReadOnlyList<int>, kind)`، مع **الإبقاء على عقد «لا تُسجّل»** وسَمحور التسجيل في الخدمة الجديدة خارج المنسّق. الواجهة تتوسّع، العقد لا يُكسر.
- **خيار ب:** إبقاء P4/P5 يبنيان ويطبعان كما هما (بناء مختلف فعلاً)، ونقل **التسجيل فقط** إلى الخدمة المشتركة. أصغر تغيير، ويحقق الاتساق في上都nível التسجيل.
- **خيار ج:** توحيد كامل وفرض المنسّق — تغيير بنيوي أوسع، **غير مُوصى به** لأنه يخالف يُعدَّم تغيير ما لا يترتب عليه عيب.

**توصيتي: الخيار (أ)** لأنه يحقق قرار المالك 1 (اتساق حقيقي على مستوى التسليم لا مجرد تطبيع)، ويحافظ على قرار SD-1، ويحل فجوة الدفعة. **هذا يحتاج موافقة المالك قبل التنفيذ.**

### 5.5 تكرار الطباعة — القواعد القائمة

`PatientTest.MarkPrinted` و`ProfileResultItem.MarkPrinted` **لا ترفض** إعادة الطباعة: كلاهما يـincrements `PrintCount` بلا حد. والواجهة تحمي عبر `requiresConfirmation` + `suppressReprint`.

**قرار صريح:** **لا نغيّر دلالات النطاق هذه.**_owner لم يطلب تغييرها، ولا يوجد عيب مُثبَّت عليها. الإصلاح يحافظ على: طباعة متكررة → `PrintCount++`، `IsPrinted` تبقى `true`، `LastPrintedAtUtc` يُحدَّث.

### 5.6 الفشل والإلغاء والاستثناء

| الحالة | المطلوب | الدليل |
|---|---|---|
| فشل بناء التقرير | لا طباعة، لا تسجيل | `ResultPrintCoordinator:52-55` (موجود) |
| فشل الطابعة | لا تسجيل | `PrintCombinedReport:66-69`، `PrintHistoryReport:71-74` (موجود) |
| `OperationCanceledException` | لا تسجيل + لا تُبلَّغ كخطأ | `ReportPrintingService:75-78` (موجود) |
| استثناء غير متوقع | لا تسجيل + يُبلَّغ للتشخيص | `ReportPrintingService:79-83` + `PrintingDiagnostics` (موجود) |
| **فشل أثناء التسجيل** | تراجع الدفعة | **جديد — انظر §5.7** |

### 5.7 التس化和 والفشل الجزئي

**الوضع الحالي:** P4/P5 يحفظان مرة واحدة بعد الحلقة (سطر 85 / 90) — أي ذرّي على مستوى `SaveChangesAsync`. لكن `SaveChangesAsync` ليس معاملة صريحة في هذا الكود: `grep -rn "BeginTransaction" src/` يُرجع **سطرًا واحدًا فقط**، وهو `AppUnitOfWork.cs:34`.

**الخطر الجديد الذي ينشئه الإصلاح:** في P3 (الجماعي)، إذا نجح بناء الحلقة لكن فشل `MarkPrinted` لأحد البنود (بند غير مُعتمد — `ProfileResultItem.MarkPrinted` يرمي إن `!IsVerified`)،\paragraph لا يوجدRollback صريح ⇒ **حالة جزئية**: الاختبار الأم مسجَّل، بعض البنود مسجَّلة وبعضها لا.

**التوصية (مُثبتة على بنية قائمة):** استخدام `IAppUnitOfWork` (من S14، مُسجَّل في `Infrastructure/DependencyInjection.cs:114`) حول حلقة التسجيل، بنفس النمط المطبَّق في `SettleAccountInFullCommandHandler`. هذا:
- يضيف **صفر بنية معمارية جديدة** (المنفذ موجود ومُسجَّل)
- يمنح ذرّية حقيقية بدل الاعتماد على EF
- يحترمClean Architecture (الواجهة في `Application/Common/Interfaces`، التنفيذ في `Infrastructure/Persistence`)

**للـP1/P2 (اختبار واحد):** لا حاجة — `SaveChangesAsync` واحدة تكفي، لفشل أي `MarkPrinted` = لا تغيير.
**للـP4/P5 (متعدد):** يُوصى بـ `IAppUnitOfWork`، لكن تقييمه منفصل: `PrintCombinedReport` يحفظ مرة واحدة أصلاً، والتحويل تغيير سلوكي طفيف يستحق قراراً صريحاً.

### 5.8 التزامن

`PatientTestConfiguration` يُعرّف `HasIndex(e => new { e.IsReviewed, e.IsPrinted, e.IsDelivered })` — فهرس **قراءة**، لا قفل. لا يوجد `RowVersion` / concurrency token على `PatientTest` أو `ProfileResultItem` (تحققت من الـconfigurations).

**الأثر:** طباعة متزامنة لنفس الاختبار من نافذتين قد تُنتج `PrintCount` غير متوقع (lost update).

**التصنيف:Watch-list، ليس عيبًا مُثبَّتًا.** الدليل على عدم كونه عيبًا حاليًا: S14 أضاف أقفال Settlement صراحةً لـ`PaymentOperation` — أي أن المشروع aware بمشكلة التزامن تعامل معها حيث frecuencias كانت كافية. الطباعة أقل تكرارًا. **لا أوصي بإضافة concurrency token في هذه الخطة** (سيتطلب migration، §6)، بل بتوثيقه كخطر مقبول في §11.

---

## 6. تحديد الحاجة لـ Migration

## **Migration Required: No**

### 6.1 الدليل

**كل الأعمدة المطلوبة موجودة بالفعل في المخطط وفي الـsnapshot الحالي:**

```
$ grep -n "IsPrinted\|PrintCount" ApplicationDbContextModelSnapshot.cs
675:  b.Property<bool>("IsPrinted")        ← PatientTest
722:  b.Property<int>("PrintCount")
869:  b.Property<bool>("IsPrinted")        ← ProfileResultItem
884:  b.Property<int>("PrintCount")
```

وكلاهما **غير nullable** في الـconfigurations الحالية:
- `PatientTestConfiguration`: `b.Property(e => e.IsPrinted).IsRequired();` و `b.Property(e => e.PrintCount).IsRequired();`
- `ProfileResultItemConfiguration`: `b.Property(e => e.IsPrinted).IsRequired();` و `b.Property(e => e.PrintCount).IsRequired();`

وعمودا `IsPrinted` أُنشئا في الـbaseline `20260828052248_BaselineDataModel.cs` (سطران 596 و710).

### 6.2 لماذا لا يحتاج الإصلاح ترحيلاً

الإصلاح المقترح = **تغيير في سلوك استدعاء دوال النطاق القائمة**، لا إضافة أعمدة ولا تعديل كيانات ولا تعديل mappings. لا يتغير:
- أي `EntityTypeConfiguration`
- أي `IEntityTypeConfiguration<T>`
- `ApplicationDbContext` (لا `OnModelCreating` ولا `ApplyConfigurationsFromAssembly`)
- أي `DbSet`
- أي `HasIndex`

**التطبيق بالكامل في مستوى التطبيق (Application)**، عبر تغيير تسلسل الاستدعاءات.

### 6.3 تحفّظات صريحة

| البند | الحالة |
|---|---|
| هل الإصلاح يغيّر الـsnapshot؟ | **لا.** `dotnet ef migrations add` غير مطلوب ولا صحيح |
| هل سيُنتج EF انحرافاً (drift)؟ | لا — لا تغيير نموذج |
| حالة §5.8 (concurrency token) | لو فُرِض لاحقاً فسيحتاج migration — **مستبعد عمداً من هذه الخطة** |
| صلاحية الترحيلات الحالية (F1) | **غير مُتحقَّق** — خارج النطاق (لا SQL Server) |

---

## 7. الأثر المعماري

| الطبقة | الأثر | التبرير |
|---|---|---|
| **Domain** | **صفر** | `MarkPrinted` صحيح في `PatientTest` و`ProfileResultItem`. المشكلة ربط لا منطق |
| **Application** | **الأساسي** | إضافة خدمة تسجيل + ربطها بالمسارات. تبقى داخل `Features/ResultsEntry/Common/` |
| **Infrastructure** | **صفر** (اختياري: استخدام `IAppUnitOfWork` الموجود) | لا استدعاءات جديدة لنظام الملفات ولا driver |
| **Presentation** | **منطقي فقط** | استدعاء الخدمة بعد نجاح الطباعة. لا new ViewModel |

**حدود Clean Architecture المحفوظة:**
- لا new `using` من `Application` إلى `Infrastructure`
- `IAppUnitOfWork` مAlready في `Application/Common/Interfaces` — لا اختراق
- `IPrintingDiagnostics` (S13) — لا علاقة

**نقطة معمارية تستحق التسجيل:** الفجوة الأصلية لم تكن «نقص تنظيف» بل **غياب قاعدة موحّدة**. الإصلاح يُضيف قاعدة واحدة بدل أربعة ممارسات. هذا تغيير **ترسيكي** في طبقة التطبيق فقط — الأصغر الممكن纠正 العطل.

---

## 8. حدود التنفيذ (GIT / IMPLEMENTATION BOUNDARIES)

### 8.1 ملفات يُتوقَّع تغيُّرها

| الملف | نوع التغيير |
|---|---|
| `Features/ResultsEntry/Common/<خدمة التسجيل الجديدة>.cs` | **جديد** (الملف الوحيد الجديد الإلزامي) |
| `Features/ResultsEntry/Commands/BulkPrint/ExecuteBulkPrintCommandHandler.cs` | تسجيل داخل الحلقة عند `outcome.Printed` |
| `Features/ProfileResults/Commands/MarkProfilePrinted/MarkProfilePrintedCommandHandler.cs` | توجيه إلى التسجيل (الاحتفاظ بالأمر — قرار 3) |
| `Features/CultureResults/Commands/MarkCultureReportPrinted/MarkCultureReportPrintedCommandHandler.cs` | توجيه إلى التسجيل (الاحتفاظ بالأمر — قرار 3) |
| `Features/ReportProduction/Commands/PrintCombinedReport/PrintCombinedReportCommandHandler.cs` | استبدال حلقة `MarkPrinted` بالخدمة |
| `Features/ReportProduction/Commands/PrintHistoryReport/PrintHistoryReportCommandHandler.cs` | استبدال حلقة `MarkPrinted` بالخدمة |
| `ViewModels/Patients/ProfileEntryViewModel.cs` | استدعاء بعد نجاح الطباعة + **حذف `using` عتيق** (سطر 7) |
| `ViewModels/Patients/CultureEntryViewModel.cs` | استدعاء بعد نجاح الطباعة + **حذف `using` عتيق** (سطر 3) |
| اختبارات (§9) | إضافة/تعديل |

### 8.2 ملفات يجب **عدم** تغييرها

| الملف | السبب |
|---|---|
| `Domain/Results/PatientTest.cs` | منطق النطاق صحيح |
| `Domain/Results/ProfileResultItem.cs` | منطق النطاق صحيح |
| `Domain/Results/ProfileResultItem.cs` → `Amend` | قرار 2 يفرض بقاء التعديل |
| `Features/ResultsEntry/Common/IResultPrintCoordinator.cs` | عقد SD-1 «لا تُسجّل» |
| `Features/ResultsEntry/Common/ResultPrintCoordinator.cs` | **لا تسجيل هنا** (مبعد مقصود) |
| `Features/ProfileResults/Commands/AmendProfileResult/…Handler.cs` | الحراسة ستصبح قابلة للعبور تلقائياً |
| `Features/ResultDelivery/**` | لا تعديل — التسليم سيعمل |
| `Infrastructure/Persistence/Configurations/**` | لا تغيير مخطط |
| `Infrastructure/Persistence/Migrations/**` | **لا migration** (§6) |
| `Infrastructure/DependencyInjection.cs` | التسجيل القائم لـ`IResultPrintCoordinator` و`IAppUnitOfWork` كافٍ |
| `Features/ResultsEntry/Commands/MarkResultPrinted/**` | غير ممس (§3.4) — قرار 3 لا يغطّيه، خارج النطاق |

### 8.3 ملاحظات حدود

- **هل يلزم ملف جديد؟** نعم — خدمة تسجيل واحدة. **هذا هو الحد الأدنى**.
- **هل يلزم migration؟** لا (§6).
- **هل يلزم تعديل اختبارات؟** نعم، إلزامي (§9).
- **هل يجب تحديث التوثيق بعد التنفيذ؟** نعم — الملف المحدد في §12.

---

## 9. استراتيجية الاختبارات

### 9.1 المشكلة التي يجب معالجتها أولاً

الاختبارات الحالية تُخفي العيب (§4.2). قبل كتابة أي اختبار إصلاح، يجب **تصنيف** الاختبارات القائمة إلى: «صالحazoMeasurement فعلي» مقابل «ترتيب يدوي للحالة».

### 9.2 اختبارات مطلوبة — مُسنَدة إلى الملفات القائمة

| # | الهدف | المستوى | الملف القائم المُوسَّع | الفرضية الأساسية |
|---|---|---|---|---|
| T1 | الطباعة الناجحة تُسجّل `PatientTest.IsPrinted` | Handler | `PrintCombinedReportCommandHandlerTests` | `FakeReportPrintingService` تعيد نجاحاً ⇒ `IsPrinted=true`, `PrintCount=1` |
| T2 | الطباعة الفاشلة **لا** تُسجّل | Handler | الملف نفسه | الخدمة ترجع `Failure` ⇒ `IsPrinted=false`, `SaveChangesCallCount=0` |
| T3 | **البروفايل يُسجّل البند + الاختبار** | Handler/جديد | `MarkProfilePrintedCommandHandlerTests` | نجاح ⇒ `item.IsPrinted=true` **و** `pt.IsPrinted=true` |
| T4 | تعديل البند ممكن **بعد** الطباعة الحقيقية | تكامل | جديد | اطبع حقاً عبر P1 ⇒ `item.IsPrinted=true` ⇒ `Amend` ينجح (قرار 2) |
| T5 | **طباعة→تسليم حقيقية** | تكامل | جديد أو موسَّع في `DeliverWithSettlementCommandHandlerTests` | اطبع P4 ثم سُلِّم ⇒ ينجح **دون** استدعاء `MarkPrinted` يدوي |
| T6 | التسليم مرفوض قبل الطباعة | Handler | `DeliverWithSettlementCommandHandlerTests` | `IsPrinted=false` ⇒ Conflict (سلوك قائم يُثبَّت) |
| T7 | الطباعة الجماعية تُسجّل الناجح فقط | Handler | `BulkPrintHonestyTests` | 2 ناجحين + 1 فاشل ⇒ 2 مُسجَّلان، الثالث لا |
| T8 | اتساق المسارات | تكامل | جديد | نفس المدخلات عبر P1 و P4 ⇒ نفس الحالة النهائية |
| T9 | الأوامر المحتفظ بها تعمل | Handler | `MarkProfilePrintedCommandHandlerTests` | الأمران يُسجّلان بعد نجاح الطباعة (قرار 3) |
| T10 |-culture: يسجّل الاختبار | Handler | جديد/موسَّع | P2 نجاح ⇒ `pt.IsPrinted=true` |
| T11 | إعادة الطباعة | Domain/Handler | `PatientTestTests` | طباعة ثانية ⇒ `PrintCount=2`, `IsPrinted=true` (سلوك قائم) |
| T12 |ذرّية التسجيل عند فشل بند | Handler | جديد | بند غير مُعتمد ⇒ **لا** حفظ جزئي (§5.7) |
| T13 | التراجع عن الاختبارات الميتة | — | — | `ExecuteBulkPrintCommandHandlerTests` / `BulkCommandHandlerTests` قد تحتوي تأكيدات `IsPrinted=true` **يجب تحديثها** |

### 9.3 تحذير صريح بشأن T13

عند تفعيل التسجيل في P3، **اختبارات S6 الحالية التي تؤكد `IsPrinted=false` ستصير خاطئة**. حددتها مسبقاً: `ReviewPrintDeliverCommandHandlerTests:157` و `BulkPrintHonestyTests`. **هذه ليست «انحداراً» — هي تأكيدات تحتاج تحديثاً** لتش-reflect السلوك المُصحَّح. يجب التعامل معها صراحة، لا تجاهُلها.

كذلك `ReviewPrintDeliverCommandHandlerTests` تبني `MarkResultPrintedCommandHandler` بـ `FakeResultPrintCoordinator` — وهو أمر ميت (§3.4). **لا تُحذف** (خارج النطاق)، لكن يُوصى بإضافة `FakePrintedStateRecorder` لتغطية المسار الجديد.

### 9.4 fixture متاح للاستخدام

`tests/TopLab.Persistence.Tests/SqlServerFixture.cs` يستخدم `Testcontainers.MsSql` مع تخطٍّ نظيف عند غياب Docker (SD-12). مثالي لـ T4/T5/T8 **إذا توفّر Docker**، مع تخطٍّ مقبول إن لم يتوفّر.

**لكن `FakeApplicationDbContext` كافٍ لـ T1–T3 و T6–T13**، ويستخدمه المشروع على نطاق واسع. **لا أُنشئ fixture جديدًا**.

---

## 10. معايير القبول الموضوعية

### 10.1 صحّة المصدر

- [ ] `grep -rn "\.MarkPrinted(" src/` يُظهر **5 مواقع** (الخدمة الجديدة + العُقد المُوجَّهة)، لا 4 ولا 7
- [ ] `grep -n "MarkPrinted" src/TopLab.Application/Features/ResultsEntry/Common/ResultPrintCoordinator.cs` يُرجع **صفراً**
- [ ] `grep -n "using TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted" ViewModels/Patients/ProfileEntryViewModel.cs` يُرجع **صفراً** (تنظيف العتيق)

### 10.2 صحّة حالة الطباعة

- [ ] كل مسارات الإنتاج الخمسة تُسجّل `PatientTest.IsPrinted = true` **بعد** `print.IsSuccess == true`
- [ ] P1 يُسجّل `ProfileResultItem.IsPrinted` أيضًا
- [ ] فشل الطابعة في أي مسار ⇒ `IsPrinted` تبقى `false` **و** `SaveChanges` لا تُنادى
- [ ] `PrintCount` يزيد بمقدار 1 لكل طباعة ناجحة لكل صف

### 10.3 التسليم

- [ ] T5: طباعة P4 → تسليم **ينجح** دون ترتيب يدوي للحالة
- [ ] T6: تسليم صف غير مطبوع يُرفض (سلوك قائم محفوظ)

### 10.4 التعديل بعد الطباعة (قرار 2)

- [ ] T4: طباعة بروفايل حقيقية ⇒ `item.IsPrinted=true` ⇒ `AmendProfileResultCommand` **ينجح**
- [ ] `AmendProfileResultCommandHandler` لم يُعدَّل (تحقق عبر `git diff`)

### 10.5 الاحتفاظ بالأوامر (قرار 3)

- [ ] `MarkProfilePrintedCommand.cs` و`MarkCultureReportPrintedCommand.cs` **موجودان وغير محذوفين**
- [ ] كلاهما يعمل ويُسجّل الحالة بعد نجاح الطباعة (T9)

### 10.6 التغطية

- [ ] T1–T13 موجودة وتُنفَّذ
- [ ] **على الأقل** اختبار واحد يثبت سلسلة **حقيقية**: طباعة → حالة → تسليم/تعديل، دون استدعاء `MarkPrinted` يدوي
- [ ] T12 (الذرّية) موجود — يغطي فشل السطر في P3

### 10.7 سلامة البناء والاختبارات

- [ ] `dotnet build` ينجح
- [ ] `TopLab.Application.Tests`: 1583+ ناجح، **صفر فشل**
- [ ] `TopLab.Domain.Tests`: 507+ ناجح
- [ ] فشل `Infrastructure` إمّا 0 أو **18 بيئية فقط** (خطوط) — أي فشل مختلف يُعدّ انحداراً
- [ ] `TopLab.Presentation` يُبنى على Windows (لا تغيير)

### 10.8 الحالة

- [ ] **لا migration جديد**؛ `git status` لا يظهر ملفات في `Migrations/`
- [ ] `ApplicationDbContextModelSnapshot.cs` **غير معدَّل**
- [ ] `git status --porcelain` نظيف عدا الملفات المقصودة
- [ ] لا تعديل على `Domain/Results/*`
- [ ] commits واضحة وموثّقة

---

## 11. تحليل المخاطر

| # | الخطر | لماذا يوجد | المجال | التخفيف | الكشف |
|---|---|---|---|---|---|
| R1 | **كسر تأكيدات الاختبارات القائمة** | تأكيدات S6 تفترض «لا تسجيل» | الاختبارات | تحديثها صراحة (T13)، لا تجاهلها | فشل التطبيق بعد التغيير |
| R2 | **الاختبارات الميتة تُبقي فجوة** | `MarkResultPrinted` ميت (§3.4) | P4 area | توثيق صريح كخارج نطاق | مراجعة يدوية |
| R3 | **فشل جزئي في P3** | حلقة داخل معاملة ضمنية | الحالة | `IAppUnitOfWork` (§5.7) | T12 |
| R4 | **كسر آلية إعادة الطباعة** | `requiresConfirmation` تعتمد على `IsPrinted` (§5.2) | UX | إصلاح P4/P5 **ضمن نفس المعاملة** | اختبار يعيد الطباعة بعد P4 |
| R5 | **تسجيل بنود غير مُعتمدة** | `ProfileResultItem.MarkPrinted` يرمي إن `!IsVerified` | P1 | الترتيب السليم: اعتماد قبل طباعة؛ معاملة الخطأ | T12 |
| R6 | **توسيع الواجهة يكسر SD-1** | عقد «لا تُسجّل» | البنية | التسجيل خارج المنسّق دائماً | مراجعة `grep` (§10.1) |
| R7 | **تعارض مع قرار 3** | temptation لحذف الأمرين | النطاق | قرار 3 مُلزِم | فحص وجود الملفين |
| R8 | **تعقيد متسارع** | 5 مسارات + خدمة جديدة | الصيانة | تغيير محلي، بلا refactor شامل | مراجعة diff |
| R9 | **فقدان الذرّية عند فشيل P4/P5** | `SaveChangesAsync` واحدة اليوم | الحالة | تقييم `IAppUnitOfWork` | مراجعة معاملة |
| R10 | **خطوط PDF** | 18 اختبارًا فاشلًا بيئيًا | البنية التحتية | **لا يُعامل كعيب** | المقارنة بقائمةBaseline |

**مخاطر غير مدرَجة عمداً:** التزامن (§5.8) كـwatch-list، لا كخطر تنفيذي — لا حل مبدئي، وتوثيقه كافٍ.

---

## 12. متطلبات التوثيق (لاحقة للتنفيذ)

**اسم الملف المطلوب حرفياً:**
`Fixes for issues that emerged after the implementation of the second wave..md`

**⚠️ لا يُنشأ هذا الملف الآن.** يُنشأ بعد التنفيذ الفعلي فقط.

### المحتوى المطلوب

| القسم | المحتوى |
|---|---|
| 1. العيوب المكتشَفة بعد Wave 2 | انقسام مسارات الطباعة؛ انفصال `ProfileResultItem.IsPrinted`؛ العطل الصامت للتعديل بعد الطباعة |
| 2. أدلة التدقيق المستقل | الالتزام المُدقَّق؛ مسارات P1–P5؛ الأدلة السطرية؛ نتيجة `git grep` قبل/بعد |
| 3. قرار المالك | المعالجة قبل الموجة التالية |
| 4. قرارات المالك الثلاثة | القرار 1/2/3 بنصها |
| 5. المجالات الوظيفية المتأثرة | شاشات الإدخال؛ التقرير المدمج؛ السجل؛ التسليم؛ التعديل |
| 6. الإجراءات التصحيحية المنفَّذة | **ما نُفِّذ فعلاً** — لا المخطط |
| 7. الاختبارات المضافة/المعدَّلة | T1–T13 مع نتائجها الفعلية |
| 8. نتائج التحقق | أرقام الاختبارات الفعلية بعد التنفيذ |
| 9.病例 Git | هاش كل commit، مع رسالة واضحة |
| 10. الحالة النهائية | ✅ / ⚠️ |
| 11. التأكيد الصريح | تأكيد مُصاغ بأن العيوب حُلّت **قبل** الموجة التالية |

**قاعدة صارمة:** لا ادّعاء بأن أي إصلاح نُفِّذ قبل تنفيذه فعلياً. الأرقام يجب أن تكون ناتجة عن تشغيل حقيقي.

---

## 13. الأسئلة المفتوحة / العوائق

| # | السؤال | الأثر | لماذا لا أفترض |
|---|---|---|---|
| **Q1** | **خيار إعادة توجيه P4/P5** (§5.4): أ / ب / ج | **حاجز — يحدد بنية التنفيذ** | قرار معماري affects عقد SD-1؛ المالك لم يحسم |
| **Q2** | `MarkResultPrintedCommand` ميت (§3.4) ولا يشمله قرار 3 | نطاق متوسط | قرار 3 ذكر أمرين فقط؛ الثالث خارج التغطية الصريحة |
| **Q3** | `IAppUnitOfWork` في P4/P5 (§5.7) | متوسط | تغيير سلوكي؛ قد لا warrants توسّعاً |
| **Q4** | إعادة الطباعة: هل يحتاج UX بعد الإصلاح؟ | منخفض | `requiresConfirmation` سيعمل مجدداً بعد إصلاح P4 |
| **Q5** | توثيق `using` عتيق في报告 (§3.4) | منخفض | ت_vs تنظيف |

**Q1 هو الحاجز الوحيد.** Q2–Q5 لا تمنع البدء.

---

## 14. تسلسل التنفيذ

| الخطوة | الغرض | الدليل الداعم |和相关 |
|---|---|---|---|
| **1. قرار المالك على Q1** | تثبيت البنية | §5.4 | **حاجز** |
| **2. إنشاء خدمة التسجيل** | نقطة تسجيل واحدة | §5.3 | بلا ملف |
| **3. ربط P1 / P2** | إصلاح الواجهة الأساسية | §3.1, قرار 1 | متوسط |
| **4. ربط P3** | تسجيل ما طُبع فقط | §3.6, §5.4 | متوسط |
| **5. إعادة توجيه P4 / P5** | الاتساق + فجوة الدفعة | §3.2, §3.3, §5.2 | **عالٍ** |
| **6. توحيد `MarkProfilePrinted` / `MarkCultureReportPrinted`** | قرار 3 | §3.4 | منخفض |
| **7. ذرّية التسجيل** | منع الحالة الجزئية | §5.7 | متوسط |
| **8. تحديث + إضافة الاختبارات** | كشف العيب | §9, T13 | **عالٍ** |
| **9. التحقق الكامل** | قبول | §10 | — |
| **10. التوثيق** | الإغلاق | §12 | — |

**تفاصيل الخطوات 3–5:**

- **الخطوة 3:** في `ProfileEntryViewModel.PrintAsync` (353-383)، عند `if (outcome.Printed)` → استدعاء التسجيل للاختبار **ولبنوده**. نفس النمط في `CultureEntryViewModel` (468-470) للاختبار فقط. **ليس قبل** `outcome.Printed` إطلاقاً.
- **الخطوة 4:** داخل `foreach` في `ExecuteBulkPrintCommandHandler` (81-97)، عند `if (outcome.Printed)` → تسجيل ذلك الاختبار. الفاشل لا يُسجَّل.
- **الخطوة 5:** استبدال حلقات `MarkPrinted` في P4 (71-83) وP5 (76-88) بالخدمة المشتركة، مع **الإبقاء** على: فحص `IsReviewed` في P5 (57-60)، وبوابة الرصيد، وترجمة `DomainFailureTranslator` (81/86).
- **الخطوة 8:** **ابدأ بتصحيح تأكيدات S6 أولاً** (T13) قبل إضافة اختبارات جديدة — وإلا فستتعارض النتائج ولا يمكن تمييز السبب.

---

## 15. الخلاصة

| البند | النتيجة |
|---|---|
| العيوب المؤكَّدة | 3 (انقسام المسارات · انفصال حالة البند · انحدار مُستحدث) |
| العيوب المُستبعدة | 0 مُختلقة — الفشل الـ18 بيئي، وليس عيبًا |
| غير مُتحقَّق | 4 بنود مُصنَّفة صراحة (§3.7) |
| قرار migration | **Migration Required: No** |
| حاجز واحد | Q1 (§5.4) |
| Changes المطلوبة | ملف جديد واحد + 6 ملفات إنتاج + اختبارات |

**الحكم الهندسي:** العيب حقيقي وموثَّق بسطر مُحدَّد في كل حالة، وأصله إصلاح ناقص بدأ في S4 ومُهل منذ S6. الإصلاح المقترح محصور في طبقة التطبيق، ويحافظ على نطاق النطاق وعلى عقد SD-1، ويحترم قرارات المالك الثلاثة.
