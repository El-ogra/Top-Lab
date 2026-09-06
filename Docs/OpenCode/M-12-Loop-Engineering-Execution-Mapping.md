# M-12 — خريطة تطبيق Loop Engineering على شرائح خطة Module 12

> **طبيعة هذه الوثيقة:** توثيق وتخطيط فقط (Read-Only). لا تنفّذ هذه الوثيقة أي شريحة، ولا تعدّل أي كود، ولا تشغّل build أو test أو migration، ولا تقوم بأي commit/push. سياسة عدم الـ commit/push التلقائي سارية بلا استثناء: كل commit يتطلّب تأكيدًا بشريًا صريحًا لاحقًا عند التنفيذ الفعلي.

---

## 0. التحقق المسبق (قبل البدء — لا افتراضات)

| البند | القيمة المعتمدة فعليًا |
|---|---|
| **مسار ملف الخطة M-12.md** | `C:\Users\LAP LINK\source\repos\Top-Lab\Docs\OpenCode\M-12.md` |
| **مسار مهارة loop-engineering** | `C:\Users\LAP LINK\source\repos\Top-Lab\.opencode\skills\loop-engineering\SKILL.md` + مجلد `references/` التابع له |
| **رقم commit الحالي (HEAD)** | `e5389051d56a8b725f6c037aa269fb0a33ddb1df` |
| **الفرع الحالي** | `main` |
| **عدد الشرائح في الخطة** | 5 شرائح (صريحة ومرقّمة) |

### ⚠️ اكتشاف حرِج (يجب عرضه كقرار مفتوح)

مجلد مهارة `loop-engineering` الموجود فعليًا هو **غلاف قديم (Legacy Wrapper) مُهمَل**. النص الموجود في `SKILL.md` سطر 1-6 يصرّح:

> "DEPRECATED — This skill has been split into two independent skills. For PLANNING: use 'module-planning' skill... For EXECUTION: use 'module-execution' skill."

والمحتوى الفعلي للملف (سطور 8-58) عبارة عن غلاف يوجّه إلى `module-planning` و`module-execution` فقط، ولا يحتوي على نص الحلقة العشرية في `SKILL.md` نفسه.

**لكن** — وطبقًا لتعليمات المهمة ("لا تُعِد بناءها أو تعرّفها من جديد، فقط اقرأها كما هي") — قرأتُ المهارة كما هي كما أنّ مجلدها يحوي ملفات مرجعية كاملة تُشكّل **المنهجية الفعلية للحلقة العشرية**، وهي:
- `references/10-stage-loop.md` — الإجراء التفصيلي للمراحل 1-10 (هذا هو المرجع المعياري للتنفيذ).
- `references/stop-conditions.md` — شروط التوقف (العتبة = 4 محاولات متتالية).
- `references/execution-prompt.md` — قالب أمر التنفيذ C1-C10.
- `references/memory-file-template.md` — بنية ملف الذاكرة B1-B5.
- `references/anti-patterns.md` — الأنماط الممنوعة (AP-01..AP-11).

**إذن:** المهارة كما توجد في المشروع تُقدّم المنهجية عبر ملفاتها المرجعية، وليس عبر نصها الرئيسي. سأعتمد هذه الملفات المرجعية كمرجع «الحلقة القياسية». أي أنّ المهارة الواحدة القابلة للتفعيل فعليًا للتنفيذ هي `module-execution`؛ لكن المهمة حصرتني بمهارة `loop-engineering` كما هي. هذا يُسجَّل كنقطة قرار مفتوح في نهاية هذا التقرير (راجع §6).

---

## 1. قائمة الشرائح المستخرجة (كما وردت فعليًا في M-12.md)

مصدر الاستخراج: القسم §11 «Slicing Model and Execution Order» (سطور 609-628)، مع تفاصيل كل شريحة في أقسامها §3-§6:

| # | عنوان الشريحة (كما في الخطة) | موقع التعريف في الملف | طبقة النطاق |
|---|---|---|---|
| **S1** | Domain behaviors + tests — «slice with highest risk: BR-04, TestCode property change, IsActive + lifecycle mutators, WorkGroupLogItem ctor visibility» | سطر 613؛ التفاصيل في §3 (سطور 67-170) | Domain فقط |
| **S2** | Application read surface + DTOs + fake-extension + tests | سطر 614؛ التفاصيل في §4.2 (سطور 182-247) | Application فقط (جانب القراءة) |
| **S3** | Application write surface + validators + auth tests | سطر 615؛ التفاصيل في §4.3 (سطور 249-341) | Application فقط (جانب الكتابة) |
| **S4** | Infrastructure: TestCode + IsActive configuration, unique index, migration, ADR-0028/0029, R-1 verifier, integration test | سطر 616؛ التفاصيل في §5 (سطور 344-400) | Infrastructure + migration + أدوات |
| **S5** | Hardening, documentation, module close-out | سطر 617؛ التفاصيل في §6 (سطور 403-438) | مستندات + إغلاق الوحدة |

**ملاحظة استخراج حرفية:** أسماء الشرائح وردت في الخطة كمختصرات (S1..S5 بعناوين وصفية قصيرة في §11)، والنطاقات الكاملة في أقسامها. أعلاه أعنونان بنفس الصياغة الواردة فعليًا في أسطر §11.

---

## 2. شرح تفصيلي مستقل لكل شريحة

> **تنبيه منهجي شامل (ينطبق على كل الشرائح):** الحلقة العشرية القياسية في `10-stage-loop.md` تفترض شرائح «رأسية» (Vertical Slices) تعبر الطبقات (UI+API+DB) وأنّ Stage 7 التحقق النهائي يجب أن يكون اختبار مسار مستخدم من طرف لطرف (سطور 87-95 في الملف المرجعي). **خطة M-12 صرّحت صراحةً أنّ شرائحها أفقية/حسب الطبقة** (سطر 33: «following the M22 pattern»؛ وسطر 18، 60، 639: كل عمل الواجهة خارح النطاق، وشرط A7: لا محتوى UI في أي مكان). إذن **لا توجد أي شريحة هنا قابلة للتجربة من طرف المستخدم (UI) في M-12**.
>
> النتيجة الإلزامية: كل مرحلة Stage 7 («Validation Gate») في هذه الوحدة سوف تُنفَّذ عبر «بوابات الخروج» (Exit Criteria) النصية لكل شريحة في الخطة (§3.5، §4.4، §5.6، §6.3/6.4) — وهي بوابات build/test/فحص وليست رحلة UI. هذا انحراف عن النمط القياسي مفصَّلٌ داخل كل شريحة أدناه، ويُسجَّل كقرار مفتوح رئيسي في §6.

---

### الشريحة S1 — Domain behaviors + tests

**(1) معنى «التنفيذ الكامل» لهذه الشريحة تحديدًا:**

تُكمِّل أسطح السلوك (Mutator/Properties) لكيانات Domain الستة: `Test`، `TestGroup`، `ReferenceRange`، `WorkGroupLog`، `WorkGroupLogItem`، والقيمة الجديدة `ReferenceRangeSnapshot`. الملفات المتأثرة كما وردت في §3.2 (سطور 75-87):

- **تعديل:**
  - `src/TopLab.Domain/Tests/Test.cs` — إضافة خاصية `TestCode` (تمريرها عبر `Create`/`Update` + حراسة required/trimmed/max-50)، إضافة `IsActive` (bool افتراضي `true`)، إضافة `Deactivate()` و`Reactivate()`.
  - `src/TopLab.Domain/Tests/TestGroup.cs` — إضافة `Rename(string)` مع حراسة required+trimmed، إضافة `IsActive`، إضافة `Deactivate()`/`Reactivate()`.
  - `src/TopLab.Domain/Tests/ReferenceRange.cs` — إضافة `Update(...)` (بنفس حراسة `Create`)، حراسة `AgeMin >= 0 && AgeMax >= 0` وطول التعليق ≤ 500، إبقاء `Matches` كما هي (BR-04)، إضافة `CaptureSnapshot()`.
  - `src/TopLab.Domain/Tests/WorkGroupLog.cs` — إضافة `Rename`، `ContainsTest(TestId)`، `AddItem(TestId)`، `RemoveItem(TestId)`، `ClearItems()`.
  - `src/TopLab.Domain/Tests/WorkGroupLogItem.cs` — جعل المُنشئ ذي المعاملات `private`، إضافة مصنع ثابت `Create(WorkGroupLogId, TestId)`، إبقاء المُنشئ المعامِلي-خالي/الافتراضي العام ليدعم EF. **شرط مسبق قبل التعديل:** `grep -rn "new WorkGroupLogItem(" src tests` يجب أن يعيد 0 نتائج (سطر 80، ورِسك R-5 سطر 568).
- **إنشاء:**
  - `src/TopLab.Domain/Tests/ReferenceRangeSnapshot.cs` — `sealed record` غير قابل للتغيير (Immutable) بحقول `TestId`, `Sex?`, `AgeUnit`, `AgeMin`, `AgeMax`, `MinValue`, `MaxValue`, `LowComment?`, `HighComment?`, `CapturedAtUtc` (سطور 83).
- **غير معدَّلة:** `TestComment.cs` (سطر 86) و`PatientTitle.cs` (سطر 87).
- **اختبارات Domain (تُنشأ/تُعدَّل) في `tests/TopLab.Domain.Tests/`:** `TestTests.cs`، `TestGroupTests.cs`، `ReferenceRangeTests.cs` (بما فيها مصفوفة BR-04)، `WorkGroupLogTests.cs`، `WorkGroupLogItemTests.cs` (سطور 164-168)، وتعديل `TestCatalogTests.cs` لمواقع استدعاء `Test.Create` (سطر 490).

انظر مواصفات الـ mutators المفعّلة حرفيًا في §3.3 (سطور 89-146)، وملاحظة مسؤولية الـ Cascade في سطر 148.

**(2) كيف تُطبَّق مراحل الحلقة على S1 تحديدًا:**

- **نطاق البناء/الاختبار:** البناء المستهدف هو `dotnet build src/TopLab.Domain` بمتطلب صفر أخطاء وصفر تحذيرات (سطر 156)، ثم `dotnet test` على `tests/TopLab.Domain.Tests` لتشغيل اختبارات Domain الجديدة والحالية (سطر 157). المرحلتان 1 و6 من الحلقة تستخدمان هذا الأمر تحديدًا لهذه الشريحة.
- **لماذا هذا المشروع فقط:** S1 محصورة في طبقة Domain فقط (سطر 160: «No Infrastructure or Application changes»)؛ لذا لا داعي لبناء Application/Infrastructure في مراحل التحقق الخاصة بهذه الشريحة. (بناء الحل الكامل مؤجَّل إلى S4/S5 حيث تنضغط كل الطبقات.)
- **EF Migration:** **لا توجد** migration في S1. الـ schema يتغيّر فقط عبر EF Configurations في S4 (سطر 617، وقسم §5.5). لذلك لا شيء يُعامل بخصوص migration هنا.
- **صيغة رسالة الـ commit المتوقعة لهذه الشريحة تحديدًا:**
  ```
  [M-12] Slice 1/5: Domain behaviors + tests — loop-engineering

  Stages 1-10 verified. Gate VG-01 passed.
  ```
  (مع ملاحظة أنّ "VG-01" هنا تقابل بوابات خروج §3.5/§3.6 وليست بوابة خطة بصيغة VG صريحة — انظر §3 أدناه في التوضيح العام لمرحلة Stage 7.)

**(3) انحرافات أو خصوصيات عن النمط القياسي:**

- **البوابة (Stage 7) ليست تجربة مستخدم:** بوابات §3.5 (سطور 155-160) كلها build/test/grep — منها `grep -rn "new WorkGroupLogItem(" src tests` يعيد 0 (سطر 159). لا توجد رحلة UI للتجربة.
- **فكّ مسؤولية الـ Cascade عمدًا:** المُغيِّر `TestGroup.Deactivate()` يضبط حالة المجموعة فقط؛ أمّا تتابع تعطيل الأعضاء فهو مسؤولية المعالج في طبقة Application (سطر 148، و§4.3 `DeactivateTestGroupCommandHandler`). هذا يعني أنّ اختبار `Deactivate` للمجموعة *في طبقة Domain* يختبر المُغيِّر في عزلة فقط (سطر 165)، بينما يُختبر التتابع في S3 — لذا يمكن اعتبار S1 وS3 مرتبطة دلاليًا عبر هذا العقد (يجب عدم تمييز اختبار الـ cascade بأنه «مكتمل» في S1).
- **خطر R-5 (تغيير منشئ `WorkGroupLogItem` المعاملي إلى private):** هذا تغيير كاسر محتمل يفرض `grep` قبل وبعد التعديل (سطر 159، وR-5 سطر 568). إن عُثر على أي استدعاء بمعاملات لم يُرحَّل بعد، فالتعديل لا يمكن أن ينزل — وهذا يشبه شرط إيقاف خاصًا بالشريحة (تابع أدناه).
- **BR-04 مُثبَّتة بالاختبارات:** شرط صراحي أن تبقى `Matches` حرفية (سطر 78، وسطر 158: «BR-04 pinned by tests»)، مع مصفوفة اختبار محددة في سطر 166.

**(4) شرط إيقاف خاص بها (غير الشرط العام: 4 محاولات متتالية لفشل بناء/اختبار):**

- **شرط مسبق مُلزِم (نوع Stop-gate):** إذا أعاد `grep -rn "new WorkGroupLogItem(" src tests` عددًا غير صفري، فلا يُسمح بتغيير منشئ `WorkGroupLogItem` (سطر 80 ورِسك R-5 سطر 568: «the change cannot land until they are migrated») — يجب إيقاف الشريحة وعرضها على القرار البشري قبل أي تعديل. إن أُجري التعديل ثم ظهرت نتائج grep غير صفرية بعد التعديل، فهذا فشل تحقق خاص يجب وقفه.
- **عتبة الإيقاف العامة ما زالت 4 متتالية (وليس 5 كما ذُكرت في نص المهمة):** لاحظ — النص القياسي للمهارة في `SKILL.md` و`stop-conditions.md` يحدد العتبة بـ **4 محاولات متتالية**، بينما نص المهمة ذكر «أكثر من 5 محاولات متتالية» كشرط عام. سأعتمد العتبة القياسية **4** لأنّ مرجع المهارة (`stop-conditions.md` سطر 11: «Threshold: 4 (not 3, not 5)») هو المرجع المعياري؛ هذا التناقض يُسجَّل كالقرار المفتوح في §6.

---

### الشريحة S2 — Application read surface + DTOs + fake-extension + tests

**(1) معنى «التنفيذ الكامل» لهذه الشريحة تحديدًا:**

كامل الأسطح القرائية (Queries) وDTOs، مع استبعاد السجلات غير النشطة من نتائج البحث الافتراضية (سطر 184، وفرضيتا FR-M12-001). الملفات كما في §4.2 (سطور 186-201) و§7.1:

- **إنشاء (في `src/TopLab.Application/Features/TestCatalogAndReferenceRanges/`):**
  - `Common/TestCatalogDtos.cs` — كل DTOs (ختم records): `TestSummaryDto`, `TestDetailDto`, `TestGroupDto`, `WorkGroupLogDto`, `WorkGroupLogItemDto`, `ReferenceRangeDto` (سطور 204-224).
  - `Queries/SearchTestCatalog/{SearchTestCatalogQuery.cs, SearchTestCatalogQueryHandler.cs}`.
  - `Queries/GetTestById/{...}` . `Queries/GetTestGroups/{...}` . `Queries/GetWorkGroupLogs/{...}` . `Queries/GetReferenceRanges/{...}`.
- **تعديل:** `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` — إضافة قوائم `TestGroup`, `ReferenceRange`, `WorkGroupLog`, `WorkGroupLogItem`, `TestComment` (قراءة فقط للتقاطع) مع فروع `Set/Add/Remove` (سطر 332، و§7.2 سطر 491).
- **اختبارات (سطور 333-337):** ملفات handler tests لكل Query الخمسة (SearchTestCatalog، GetTestById، GetTestGroups، GetWorkGroupLogs، GetReferenceRanges).

سلوكيات القراءة التفصيلية (مرشّح البحث، الـ `IncludeInactive`، ثمّ أن الاستعلامات **ليست** `IAuthorizedRequest`) موضَّحة في سطور 226-247.

**(2) كيف تُطبَّق مراحل الحلقة على S2 تحديدًا:**

- **نطاق البناء/الاختبار:** البناء `dotnet build src/TopLab.Application` (سطر 319 في §4.4 يشمل S2+S3) بمتطلب صفر أخطاء/صفر تحذيرات، ثم `dotnet test tests/TopLab.Application.Tests` لتشغيل اختبارات Query handlers. المرحلتان 1 و6 تستخدمان هذا الأمر.
- **لماذا هذا المشروع فقط:** S2 محصورة في Application (قراءة) + اختباراتها + الـ Fake. لا Migration ولا Infrastructure ولا Domain — فالسلوكيات Domain قد اكتملت في S1 ويُفترض إعادة استخدامها كما هي.
- **EF Migration:** لا توجد في S2 (لا schema جديد).
- **صيغة رسالة الـ commit المتوقعة:**
  ```
  [M-12] Slice 2/5: Application read surface + DTOs + fake-extension + tests — loop-engineering

  Stages 1-10 verified. Gate VG-02 passed.
  ```

**(3) انحرافات أو خصوصيات عن النمط القياسي:**

- **الاستعلامات ليست `IAuthorizedRequest`** (سطر 245): هذا استثناء نموذجي من نمط التخويل؛ إذ يقال إنّ سطح القراءة مفتوح (تأسيًا على `GetUsersQuery` و`GetSystemSettingsQuery`) والحساسية تُبوَّب في طبقة Presentation (خارج نطاق M-12). لذا فإنّ اختبارات الـ `AuthorizationBehavior` في هذه الوحدة تنتمي لسطح الكتابة S3 (سطور 322، 340) وليس S2.
- **خصوصية الـ IncludeInactive:** لا يُحدد سلوك `IsActive` التصفية عبر عديد من الاستعلامات (SearchTestCatalog سطر 230، GetTestGroups سطر 243، GetTestById لا يرشّح سطر 242) — يجب التحقق لكل Query على حدة، وهي موزعة في §4.2 أكثر من كونها بوابة واحدة.
- **يُفترض الاعتماد على S1:** هذه الشريحة تعبر عناصر Domain (مثل `Test.TestCode`, `IsActive`) التي أُنجزت في S1؛ فلا بد أنّ S1 مكتملة و�صفر عليه قبل S2 (سطر 619: S1 → S2).

**(4) شرط إيقاف خاص بها (غير الشرط العام):**

- **لا شرط إيقاف خاص نصّي إضافي لهذه الشريحة في الخطة** بخلاف بوابات الخروج §4.4 (سطور 317-328 الخاصة بـ S2+S3) والإيقاف العام (4 متتالية). لكن يُلاحظ أن بوابة S2 تعتمد على اكتمال S1؛ إن لم تكن S1 قد اجتازت بواباتها، فلا يجوز البدء بـ S2 (ترتيب DAG سطور 619-627) — هذا «شرط تبعية» وليس عتبة فشل.

---

### الشريحة S3 — Application write surface + validators + auth tests

**(1) معنى «التنفيذ الكامل» لهذه الشريحة تحديدًا:**

كل أوامر الكتابة (14 أمرًا)، والـ validators، والتخويل، مع **عدم وجود حذف صلب لـ Test/TestGroup** (سطر 253). الملفات كما في §4.3 (سطور 255-273) و§7.1:

- **إنشاء (مجلد لكل Use Case في `src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/`):** `CreateTest`, `UpdateTest`, `DeactivateTest`, `ReactivateTest`, `CreateTestGroup`, `UpdateTestGroup`, `DeactivateTestGroup`, `ReactivateTestGroup`, `CreateWorkGroupLog`, `RenameWorkGroupLog`, `SaveWorkGroupLogItems`, `CreateReferenceRange`, `UpdateReferenceRange`, `DeleteReferenceRange` — كلٌّ بـ3 ملفات (Command, Handler, Validator) إلا ما كان مختلفًا (سطور 453-466).
- **اختبارات Application (سطور 339-340):** اختبارات handler لكل أمر (happy path + كل فرع تعرّف/فشل/تعارض/تحقق FK/تكرار TestCode/دلالة الاستبدال الذري/تحديث في المكان/حذف live-row/انتقالات دورة الحياة)، اختبارات validator لكل أمر، و`TestCatalogAndReferenceRangesAuthorizationTests.cs` (نظرية عبر 14 نوع أمر تؤكد `EDIT_SYSTEM_SETTINGS`).
- **التخويل:** كل أمر ينفّذ `IAuthorizedRequest` بـ`RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` (سطر 276). لا رموز تخويل جديدة (كي لا نحتاج migration إضافية).
- **اختبار منطق الـ Cascade:** `DeactivateTestGroup` handler ×each member Test atomically via single `SaveChangesAsync` (سطر 265، واختبار A13 سطر 645، EC-19..EC-22 سطور 600-603).

**(2) كيف تُطبَّق مراحل الحلقة على S3 تحديدًا:**

- **نطاق البناء/الاختبار:** البناء `dotnet build src/TopLab.Application` (سطر 319)، ثم `dotnet test tests/TopLab.Application.Tests` (اختبارات الـ 14 أمرًا + validators + auth). المرحلتان 1 و6 تستخدمان ذلك.
- **لماذا هذا المشروع فقط:** S3 محصورة في Application (كتابة) + validators + auth + اختباراتها. لا Infrastructure/Layer أخرى.
- **EF Migration:** لا توجد في S3 (لا schema جديد؛ «الإذن» الوحيد للـ migration هو `TestCode`+`IsActive` في S4 — سطر 62، 276).
- **صيغة رسالة الـ commit المتوقعة:**
  ```
  [M-12] Slice 3/5: Application write surface + validators + auth tests — loop-engineering

  Stages 1-10 verified. Gate VG-03 passed.
  ```

**(3) انحرافات أو خصوصيات عن النمط القياسي:**

- **تحذير الـ Cascade المنطقي (R-11):** التفعيل المتتابع لتعطيل المجموعة يستعلم `TestGroupId == groupId && IsActive == true` داخل سياق واحد؛ تغيير التتبع في EF ليس معزولًا بمثابة snapshot ضمن `SaveChangesAsync` واحدة، لذا قد تُفوَّت إضافة اختبار متزامن جديد للمجموعة أثناء نافذة التعطيل (سطر 574). تُخفَّف كـ«نافذة إهمال في بيئة سطح مكتب أحادية المحطة»، وتُوثَّق في `Handoff_M12.md` كقيد معروف. هذا انحراف/قيد يجب تسجيله في اختبار/وثيقة الشريحة.
- **`SaveWorkGroupLogItems` استبدال ذري كامل:** دلالة خاصة (سطر 269، وEC-12 سطر 593): تحميل اللوغ، التحقق من وجود كل TestId، `ClearItems()` + `_db.Remove(...)`، ثم `AddItem` + `_db.Add(...)`، كلها في `SaveChangesAsync` واحدة. اختباره يؤكد atomicity لحالة الـ fake (لا حفظ جزئي) (سطر 324).
- **قرارات الـ Lifecycle اللا-متماثلة:** التعطيل يتعاقب (عبر handler) بينما إعادة التفعيل لا تتعاقب (سطر 12، 266، 381) — هذه خصوصية سلوكية يجب أن تعكسها الاختبارات (A13 سطر 645، واختبارات ReactivateTestGroup سطر 327).
- **قاعدة `fake`'s `SaveChangesCallCount == 1`:** لتأكيد العملية بمعاملة واحدة (سطر 645) — هذا معيار تحقق خاص بالشريحة.

**(4) شرط إيقاف خاص بها (غير الشرط العام):**

- **شرط التوحّد/الذريّة (Stop-gate على الانعكاس):** حزمة التعطيل يجب أن تُخصَّص بعملية واحدة؛ إن اختبر `fake` أنه حدث أكثر من `SaveChangesAsync` واحد (أي `SaveChangesCallCount != 1`) في سيناريو المجموعة المتعطّلة، فهذا انتهاك لدلالة «الكل أو لا شيء» (سطر 265، A13 سطر 645) ويجب وقف الشريحة لعرض قرار، لا إصلاح افتراضي.
- **شرط عدم وجود أوامر حذف:** إن ظهر أي `DeleteTest` أو `DeleteTestGroup` في سطح الكتابة، فهذا انتهاك مباشر (فرضية A12 سطر 644، وسطر 253) — يجب وقفه.
- **تبعية S1:** أسطح سلوكية Domain (كـ `Deactivate` على Test/TestGroup) تُستهلك هنا؛ تُفترض S1 مكتملة (سطر 619 S1 → S3).

---

### الشريحة S4 — Infrastructure: الترقي/التكوين + migration + ADR + R-1 + اختبار التكامل

**(1) معنى «التنفيذ الكامل» لهذه الشريحة تحديدًا:**

تنفيذ تغييرات الـ schema المصرَّح بها (`Tests.TestCode`, `Tests.IsActive`, `TestGroups.IsActive`)، وإعداد EF Configurations، وتوليد الـ migration، وكتابة ADR-0028/0029، والتحقق من عدم وجود فجوة استمرار أخرى. الملفات كما في §5.2 (سطور 350-368) و§7:

- **تعديل (Configurations):**
  - `src/TopLab.Infrastructure/Persistence/Configurations/TestConfiguration.cs` — إضافة `b.Property(e => e.TestCode).HasMaxLength(50).IsRequired();` + `b.HasIndex(e => e.TestCode).IsUnique();` (اسم الفهرس `IX_Tests_TestCode` على غرار `IX_Users_UserName`) + `b.Property(e => e.IsActive).HasDefaultValue(true);` (سطر 353).
  - `src/TopLab.Infrastructure/Persistence/Configurations/TestGroupConfiguration.cs` — `b.Property(e => e.IsActive).HasDefaultValue(true);` (سطر 354).
- **إنشاء (أدوات — tool-generated):** `src/TopLab.Infrastructure/Persistence/Migrations/<timestamp>_AddTestCodeAndLifecycleColumns.cs` (+`.Designer.cs`) وتعديل `ApplicationDbContextModelSnapshot.cs` (سطور 357-359).
- **إنشاء/تعديل مستندات:** إلحاق **ADR-0028** و**ADR-0029** في `Docs/Source/Top_Lab_ADR.md` (سطر 361).
- **تعديل اختبارات Infrastructure:**
  - `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs` — `Test_HasTestCodeColumn_UniqueIndex`, `Test_HasIsActiveColumn_DefaultTrue`, `TestGroup_HasIsActiveColumn_DefaultTrue` (سطر 364).
  - `tests/TopLab.Infrastructure.Tests/Persistence/TestDeletionCascadeTests.cs` — تأكيد سلوك الـ cascade بالاتفاقية من `Test` → `WorkGroupLogItem` و`Test` → `ReferenceRange` (سطر 365).
- **تعديل شرطي (بناءً على R-1):** `src/TopLab.Application/DependencyInjection.cs` — إضافة `builder.Services.AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>();` **فقط إذا** أكّد R-1 الفجوة (سطور 368، 309-313، 489، 564).

**(2) كيف تُطبَّق مراحل الحلقة على S4 تحديدًا:**

- **نطاق البناء/الاختبار:** **أولًا** التحقق المسبق مع الحالة الـ baseline الكاملة: `dotnet build TopLab.sln` ثم `dotnet test tests/TopLab.Infrastructure.Tests` — لأنّ S4 تعبر إلى Infrastructure وتتطلب أن يكون الحل الكامل (كل الطبقات) خضراء (سطور 396-397). بعد التعديل: توليد migration بـ `dotnet ef migrations add AddTestCodeAndLifecycleColumns` (سطر 394)، ثم `dotnet build TopLab.sln` (0/0) و`dotnet test tests/TopLab.Infrastructure.Tests`. إذا نُفّذ إصلاح R-1، فاختبار `dotnet test TopLab.sln -m:1` (سطر 399).
- **لماذا هذا المشروع + الحل الكامل:** S4 هي الشريحة الوحيدة التي تنشئ migration وتلمس Infrastructure وتدمج الطبقات كلها؛ لذا يتجاوز نطاق التحقق مشروعًا واحدًا ليصبح الحل الكامل (سطور 396-399).
- **المعاملات الخاصة بالـ Migration (واحدة من أهم خصوصيات الوحدة):**
  - **نطاق الـ migration مقفل:** يجب أن يمسّ **فقط** جدولي `Tests` و`TestGroups` (سطر 386، وقسم §5.5)، بالأعمدة والفهرس المحددين حرفيًا (سطور 387-388). بوابة مراجعة تؤكد عدم وجود أي تغيير آخر (سطر 390، وR-7 سطر 570).
  - يجب أن يُطبَّق الـ migration ويرجع بسلاسة ضد SQL Server مؤقت (سطر 395)، وأن تُتحقق سلامة التطبيق (سطر 396)، واختبار R-3 لسلامة قيمة المعرف المولّد من DB (سطر 566).
  - **ملاحظة فنية مهمة:** أمر توليد migration هو أداة EF (`dotnet ef migrations add`)؛ **هذا يعد فعلًا تنفيذيًا ممنوعًا في هذه المهمة التوثيقية**، لكنه سيُشغَّل لاحقًا عند التنفيذ الفعلي. أُنشره هنا فقط كوصف، لا كتشغيل.
- **صيغة رسالة الـ commit المتوقعة:**
  ```
  [M-12] Slice 4/5: Infrastructure (TestCode + IsActive config, unique index, migration, ADR-0028/0029, R-1 verifier, integration test) — loop-engineering

  Stages 1-10 verified. Gate VG-04 passed.
  ```

**(3) انحرافات أو خصوصيات عن النمط القياسي:**

- **بوابات S4 مركّبة من عدة أنواع تحقق متمايزة:** توليد migration نظيف (سطر 394) + فحص نطاق الـ migration بقراءة الملف (سطر 394) + تطبيق/تلقيع على SQL Server (سطر 395) + بناء الحل كلّه (سطر 396) + اختبار Infra شامل (سطر 397) + وجود ADR كامل (سطر 398) + نجاح R-1 (سطر 399). هذه ليست بوابة «تجربة مستخدم» واحدة بل شبكة بوابات تقنية.
- **الـ R-1 (حالة فرع شرطية):** خطوة pre-flight متنازع عليها — يتم فحص غياب `AddValidatorsFromAssembly` في `src/TopLab.Application/` (سطر 311)؛ إن وُجد، لا يُضاف إصلاح. إن لم يوجد، يُضاف الإصلاح + اختبار تراجعي. هذا إصلاح مشترك/عابر للحدود (يؤثر على M-17 وM-22 — سطر 315) ويحتاج **موافقة بشرية مسبقة** (R-9 سطر 572: «conditional on owner approval»).
- **حارس الحدود (Freeze-Contract) لـ ReferenceRangeSnapshot:** بوابة ما-لا-شيء: `modelBuilder.Model.GetEntityTypes()` يجب ألا يعيد أي كيان باسم `ReferenceRangeSnapshot` (أو أي كيان تشبه M-04) (سطر 428، A8 سطر 640) — يضمن عدم تسريب القيمة Domain إلى الاستمرار.
- **ملاحظة عن النمط الطبقي:** هذه شريحة «أفقية» Infrastructure بحتة (لا UI), بعكس افتراض «الشريحة الرأسية» للحلقة القياسية (راجع §2, المنبّه المنهجي أعلاه، وAP-09 سطور 86-91). هذا الانحراف **بصريح من الخطة نفسها** (سطر 33) وليس تخمينًا مني.

**(4) شرط إيقاف خاص بها (غير الشرط العام):**

- **بوابة نطاق الـ migration (Stop-gate):** إن احتوى ملف الـ migration المُولَّد على أي عمود/جدول/فهرس خارج `TestCode`+`IsActive`+فهرس `TestCode` الفريد (سطور 386-390)، يجب **إيقاف الشريحة وإعادة توليد/مراجعة يدوية** — لا يُقبل تعديل النطاق من عندي (R-7 سطر 570).
- **قد يتطلب الـ migration بيئة خارجية:** إن لم يتوفر SQL Server مؤقت (Testcontainer/ephemeral) لتطبيق الـ migration (سطر 395, 566)، فهذه نقطة توقف عملية (لا عتبة فشل عدّية بل شرط بيئة) يجب عرضها على القرار البشري.
- **تتطلب خطوة R-1 مدخلًا بشريًا قبل الإصلاح العابر للحدود:** R-9 (سطر 572): الإصلاح مشروط بموافقة المالك لأنّه يمسّ M-17/M-22.

---

### الشريحة S5 — Hardening, documentation, module close-out

**(1) معنى «التنفيذ الكامل» لهذه الشريحة تحديدًا:**

إغلاق الوحدة وفق `Top_Lab_Test_Strategy.md` §8 (سطر 407). الملفات كما في §6.2 (سطور 411-420):

- **تعديل:**
  - `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` — قلب صفّ M12 إلى `🟩 Done` مع ملاحظة deliverables، وإضافة صف سجل تغييرات مؤرّخ في §9 (سطر 412).
  - `Docs/Source/Top_Lab_Data_Model_Blueprint.md` — إضافة عمودي `TestCode` و`IsActive` في §5.2 (توثيق جدول `Test`) وعمود `IsActive` لجدول `TestGroups` مع إحالة لـ ADR-0028/0029 (سطر 413).
- **إنشاء:**
  - `Docs/Handoff_M12.md` — وفق قالب `Top_Lab_Handoff_Template.md`، يشمل: قسم الانحرافات/الإعفاءات (المتوقع: إصلاح R-1 عابر للحدود يؤثر على M-17/M-22) + تقرير التغطية + أرقام البناء/الاختبار النهائية 0/0 والحل الكامل أخضر (≈268 + جديد) (سطور 416-419).
- **لم تُعدَّل ملفات كود** — هذه الشريحة مستندات فقط، لكنّها تمسّ فعلًا سجلات المشروع.

**(2) كيف تُطبَّق مراحل الحلقة على S5 تحديدًا:**

- **نطاق البناء/الاختبار (النهائي — الحل الكامل):** `dotnet build TopLab.sln -c Release` → 0/0، و`dotnet test TopLab.sln -m:1` → كلها خضراء (سطور 424-425). المرحلتان 1 و6 في S5 تشملان الحل كله لأنهما بوابة الإغلاق.
- **بوابات الجودة (سطور 426-429):** عتبات تغطية لكل مشروع (Domain ≥ 90%، Application ≥ 80%, Infrastructure ≥ 70%) مقيسة بـ coverlet (سطر 426)؛ تحقق تدقيق التكامل من زيادة `ModificationCount` وتحديث `LastModifiedByUserId`/`LastModifiedAtUtc` دون المساس بـ`CreatedByUserId`/`CreatedAtUtc` بعد حفظ مشابه لـ`UpdateTestCommand` (سطر 427)؛ بوابة اللا-شيء لـ`ReferenceRangeSnapshot` (سطر 428)؛ مرور Slopwatch إن أمكن (سطر 429).
- **EF Migration:** لا جديد في S5 (كل الـ migration في S4). لكن قبول S5 يتطلب أنّ migration S4 قد طُبّقت بنجاح (تبعية DAG سطور 622-627).
- **صيغة رسالة الـ commit المتوقعة:**
  ```
  [M-12] Slice 5/5: Hardening, documentation, module close-out — loop-engineering

  Stages 1-10 verified. Gate VG-05 passed.
  ```

**(3) انحرافات أو خصوصيات عن النمط القياسي:**

- **شريحة الإغلاق تجمع عدة أوامر بناء/اختبار/تحقق مختلفة في مرحلة تحقق واحدة:** البناء بـ `-c Release` (سطر 424) مختلف عن البناء العادي للشرائح السابقة؛ واختبارات التغطية بـ coverlet تطالب أدوات إضافية. يجب عدم دمج مراحل الحلقة (تحريم AP-02/AP-03 في `anti-patterns.md` سطور 16-30) رغم تعدد الأدوات.
- **بوابة «اللاشيء» و«التدقيق» هما بوابات تحقق إضافية فوق الخروج المعياري:** عتبات التغطية (سطر 426) والتدقيق (سطر 427) واللا-شيء (سطر 428) — لا علاقة لها ببوابة «تجربة مستخدم» القياسية (انعطاف منهجي بذات المبرر كما في §2 أعلاه).
- **تبعيات كاسحة:** S5 تعتمد على اكتمال S1-S4 جميعًا (سطور 617-627)؛ أي فجوة في شريحة سابقة تمنع قبول S5.

**(4) شرط إيقاف خاص بها (غير الشرط العام):**

- **شرط لغات التغطية (Stop-gate):** إن لم تبلغ الأغطية فوق الأرضيات المطلوبة، فلا يُسمح بتبطين الاختبار كإصلاح مقبول؛ القاعدة: أي عضو عام غير مكشوف يُسجَّل كإعفاء موثّق في `Handoff_M12.md` لا كاختبار إضافي مرقّع (سطر 426). إن لم يُدوَّن الإعفاء في مكانه، يجب وقف الشريحة.
- **شرط قبول الحل الكامل:** يجب أن يبقى `dotnet test TopLab.sln -m:1` أخضر للنهاية، وإن انكسر (لا نتيجة للنقص من S5 نفسها بل من شريحة سابقة)، يجب عكسه في المصدر الأصلي بدلًا من تمريره هنا (سطر 425، 635).

---

## 3. جدول الملخص الموحّد

| رقم الشريحة | العنوان | الملفات/الطبقات المتأثرة | هل توجد Migration | نطاق الاختبارات | نمط رسالة الـ commit | ملاحظات / نقاط مفتوحة |
|---|---|---|---|---|---|---|
| **S1** | Domain behaviors + tests | Domain فقط: `Test.cs`, `TestGroup.cs`, `ReferenceRange.cs`, `WorkGroupLog.cs`, `WorkGroupLogItem.cs` (تعديل)؛ `ReferenceRangeSnapshot.cs` (إنشاء)؛ `TestComment.cs`/`PatientTitle.cs` (لا تعديل) (§3.2/§7) | **لا** | `tests/TopLab.Domain.Tests` (TestTests, TestGroupTests, ReferenceRangeTests/BR-04, WorkGroupLogTests, WorkGroupLogItemTests, تعديل TestCatalogTests) — §3.6 | `[M-12] Slice 1/5: Domain behaviors + tests — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` | Stop-gate خاص: grep `new WorkGroupLogItem(` يجب أن يعيد 0 قبل/بعد التعديل (R-5، سطر 80/159). الـ Cascade يُختبر في عزلة Domain فقط، والتتابع في S3 (سطر 148، 165). |
| **S2** | Application read surface + DTOs + fake-extension + tests | Application (قراءة): `TestCatalogDtos.cs` + 5 مجلدات Queries (SearchTestCatalog, GetTestById, GetTestGroups, GetWorkGroupLogs, GetReferenceRanges) + تعديل `FakeApplicationDbContext.cs` (§4.2/§7.1) | **لا** | `tests/TopLab.Application.Tests` — 5 ملفات QueryHandlerTests (§4.5) | `[M-12] Slice 2/5: Application read surface + DTOs + fake-extension + tests — loop-engineering` + `Gate VG-02 passed.` | الاستعلامات **ليست** `IAuthorizedRequest` (سطر 245). يعتمد على اكتمال S1 (DAG سطر 619). |
| **S3** | Application write surface + validators + auth tests | Application (كتابة): 14 مجلد Commands (Create/Update/Deactivate/Reactivate × Test & TestGroup، WorkGroupLog×3، ReferenceRange×3) (§4.3/§7.1) | **لا** | `tests/TopLab.Application.Tests` — اختبارات 14 handler + 14 validator + `TestCatalogAndReferenceRangesAuthorizationTests` (نظرية 14 أمرًا) (§4.5) | `[M-12] Slice 3/5: Application write surface + validators + auth tests — loop-engineering` + `Gate VG-03 passed.` | Stop-gate: `SaveChangesCallCount == 1` لتعطيل المجموعة (A13 سطر 645)؛ لا `DeleteTest`/`DeleteTestGroup` (A12 سطر 644)؛ لا-تماثل تعطيل/إعادة تفعيل (سطر 12، 266)؛ R-11 قيد نافذة متزامنة (سطر 574). |
| **S4** | Infrastructure: TestCode + IsActive config, unique index, migration, ADR-0028/0029, R-1 verifier, integration test | Infrastructure + أدوات: `TestConfiguration.cs`, `TestGroupConfiguration.cs` (تعديل)؛ **Migration مضافة** `AddTestCodeAndLifecycleColumns` + `.Designer` + `ModelSnapshot` (أدوات)؛ `Top_Lab_ADR.md` (إلحاق ADR-0028/0029)؛ `F5ConfigurationTests.cs`, `TestDeletionCascadeTests.cs` (تعديل)؛ `Application/DependencyInjection.cs` (شرطي R-1) (§5.2/§7) | **نعم** — نطاقه مقفل على `TestCode`+`IsActive`+فهرس فريد فقط (§5.5) | `dotnet build TopLab.sln` (0/0)؛ `tests/TopLab.Infrastructure.Tests`؛ إن نُفّذ R-1 فـ`dotnet test TopLab.sln -m:1` (§5.6) | `[M-12] Slice 4/5: Infrastructure (...) — loop-engineering` + `Gate VG-04 passed.` | Stop-gate: نطاق الـ migration لا يخرج عن المسموح (R-7 سطر 570)؛ تطبيق/تلقيع ضد SQL Server مؤقت (سطر 395، 566)؛ R-1 إصلاح عابر للحدود مشروط بموافقة بشرية (R-9 سطر 572)؛ حارس اللا-شيء لـReferenceRangeSnapshot (سطر 428). |
| **S5** | Hardening, documentation, module close-out | مستندات + إغلاق: `Top_Lab_Master_Tracking_Sheet.md`, `Top_Lab_Data_Model_Blueprint.md` (تعديل)؛ `Handoff_M12.md` (إنشاء)؛ و«توثيق/تثبيت» `M-12-Execution-Plan.md` (§6.2) | **لا** (يعتمد على migration S4) | `dotnet build TopLab.sln -c Release` (0/0)؛ `dotnet test TopLab.sln -m:1`؛ عتبات تغطية coverlet (Domain ≥90, App ≥80, Infra ≥70)؛ تدقيق Audit؛ Slopwatch (§6.3) | `[M-12] Slice 5/5: Hardening, documentation, module close-out — loop-engineering` + `Gate VG-05 passed.` | Stop-gate: عدم-تبطين التغطية — الإعفاءات تُدوَّن في `Handoff_M12.md` لا كاختبارات مرقّعة (سطر 426). يعتمد على S1-S4 جميعًا (سطور 617-627). |

---

## 4. ملف الذاكرة الناتج عن تطبيق المهارة (نقاط المواصفات العامة)

> هذه وصف لبنية ملف الذاكرة وفق `memory-file-template.md` (B1-B5) كما ستُولَّد عند التنفيذ الفعلي، وليست إنشاءً فعليًا للملف (يتطلب ذلك توليدًا تنفيذيًا خارج نطاق هذه المهمة التوثيقية).

- **B1 (Header/Metadata):** Module = Test Catalog & Reference Ranges؛ Module Number = M-12؛ Source Plan = `Docs/OpenCode/M-12.md`؛ Total Slices = **5**؛ Current Slice = 0؛ Current Branch = `main`؛ تاريخ ISO `YYYY-MM-DD`. الشريحة **عدّ صرّح → لا تُضاف سطرا "Inferred Slices"** (سطر 27 في template). `Current Slice` يتقدم 0..5.
- **B2 (الملخص والبوابات):** ملخص بالعربية (1-3 جمل) + G0 (قبل التنفيذ: build 0 أخطاء 0 تحذيرات + اختبارات 100%) وG1 (بعد التنفيذ). جدول بوابات الشرائح — **يُنقل من «Exit Criteria» لكل شريحة** (غ‹3.5/§4.4/§5.6/§6.3-6.4) لأنّ الخطة لا توفر بوابات بصيغة `VG-0N` صريحة؛ ستعيّن بوابات `VG-01..VG-05` مقابلةً لبوابات الخروج.
- **B3 (Slice Index):** جدول 5 صفوف بعناوين الشرائح حرفيًا، كلها `[ ] Not started`.
- **B4 (قوائم المراحل العشرة لكل شريحة):** كل شريحة تكرار 10 مربعات `- [ ]` بالترتيب غير مدموجة (سطر 84-85 في template)؛ Stage 1 و6 تذكران «zero errors + zero warnings» حرفيًا (سطر 95/100).
- **B5 (Current Status & Execution Log):** `Overall: 0/5 slices done` ثم 5 أسطر شريحة كلها `[ ] Not started` (إجمالي 6 أسطر = N+1، بلا `...`)؛ جدول سجل تنفيذ فارغ + قسم Stop Report فارغ جاهز للدونا.

---

## 5. خلاصة إجرائية (كيف يُستخدم كل هذا عند التنفيذ الفعلي)

1. عند تفعيل مهارة التنفيذ سيُنشَأ ملف الذاكرة `Docs/OpenCode/M-12-memory.md` بصيغة B1-B5 أعلاه، ثم يُصاغ أمر التنفيذ وفق `execution-prompt.md` (C1-C10) — بتنفيذ الحلقة من المرحلة 1 إلى 10 لكل شريحة حسب الترتيب S1 → (S2 ∥ S3) → S4 → S5 (سطر 619).
2. تبدأ الشريحة بالتحقق المسبق Stage 1 بنطاق البناء/الاختبار المحدد لها في §2 أعلاه (Domain لـ S1؛ Application لـ S2/S3؛ الحل الكامل لـ S4/S5).
3. تُنفَّذ المراحل 2-9 بالترتيب بلا دمج/تخطي، مع تمييز بوابات الخروج كبوابات Stage 7.
4. عن Stage 10: تُنظَّم الملفات (`git add` صريح)، ويُصاغ commit من النمط المذكور لكل شريحة، ثم **يتوقف للانتظار حتى تأكيد بشري صريح** قبل أي `git commit` أو `git push` — على الفرع الحالي `main` فقط، بلا فروع جديدة (فرضية C6 / Stage 10 في `10-stage-loop.md` سطور 127-150).
5. أي شريحة تصطدم ببابتها الخاصة أو تصل 4 فشلات متتالية لنفس السبب → إيقاف وتقرير حالة (صياغة `stop-conditions.md` سطور 136-147، وC10 في `execution-prompt.md`).

---

## 6. القرارات المفتوحة التي تتطلب قرارًا بشريًا (لا أفترض حلاً)

هذه قائمة النقاط التي يجب أن يحسمها المالك، لأنّ الحلقة القياسية لا تقدم حلًّا صريحًا لها والمهمة تمنعني من افتراض حل:

1. **مهارة التنفيذ المعتمدة:** مهارة `loop-engineering` الموجودة غلاف قديم مهمَل يوجّه إلى `module-execution` (سطر 3 في `SKILL.md`)؛ منهجيتها الفعلية في ملفاتها المرجعية. هل نفّذ عبر `module-execution` (الوصية الجديدة) أم نلتزم بمجلد `loop-engineering` حرفيًا رغم فقدان نصه الرئيسي؟
2. **اصطدام النمط الطبقي بالأفقية:** خطة M-12 صنّفت شرائحها أفقية/حسب-الطبقة (سطر 33)، والواجهة خارج النطاق (سطر 18، 60، 639)، بينما الحلقة القياسية (Stage 7 و`anti-patterns` AP-09) تفترض شرائح رأسية بقائمة مستخدم UI. رفضت تعديل نطاق الخطة، لذا ستكون كل بوابات Stage 7 بوابات build/test/فحص وليست رحلات UI — هل يُقبل هذا كتعادل للبوابة؟
3. **تعارض عتبة الإيقاف (4 مقابل 5):** النص القياسي (`stop-conditions.md` سطر 11) حدد **4** محاولات متتالية للتوقف؛ نص المهمة ذكر «أكثر من 5». اعتمدت 4 (المرجع المعياري للمهارة). أيهما يعتمد؟
4. **الإشارة إلى بوابات `VG-0N`:** قالب `memory-file-template.md` و`execution-prompt.md` يعتمدان بوابات بصيغة `VG-0N`، بينما خطة M-12 لا توفر بوابات بهذه الصيغة الصريحة وإنما عبر «Exit Criteria» لكل شريحة (§3.5/§4.4/§5.6/§6.3-6.4). سأعيِّن مقابلها بوابة `VG-01..VG-05` ربطًا بوابات الخروج. هل يُقبل هذا التعيين؟
5. **إصلاح R-1 العابر للحدود:** تغيير في `Application/DependencyInjection.cs` يمسّ أيضًا M-17/M-22 (سطر 315، R-9 سطر 572) — مشروط بموافقة صريحة على نطاقه قبل اعتماده في S4.
6. **بوابات بيئة التكامل:** تطبيق/تلقيع الـ migration يتطلّب SQL Server مؤقتًا (سطر 395، 566)؛ إن لم تكن البنية التحتية (Testcontainer/ephemeral) متاحة في بيئة التنفيذ، فذلك يحتم وقفة تشغيلية.

---

*نهاية التقرير. وثيقة توثيق وتخطيط — لا تعديلات كود، لا بناء، لا اختبار، لا commit/push.*
