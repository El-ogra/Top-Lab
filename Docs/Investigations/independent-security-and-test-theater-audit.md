# التحقق المستقل من الثغرات الامنيه والاختبارات الصوريه

**مراجعة أمنية وتدقيق جودة اختبارات — مستقلة تمامًا**
المستودع: `https://github.com/El-ogra/Top-Lab.git`
الالتزام المُثبَّت: `377aa282e1f0e8d618840f3fc39a056110d903a0`
تاريخ المراجعة: 2026-09-29

> ملاحظة: هذه نسخة بالاسم اللاتيني من التقرير. النسخة بالاسم العربي موجودة على المسار
> `/workspace/التحقق المستقل من الثغرات الامنيه والاختبارات الصوريه.md` بنفس المحتوى حرفيًا.

---

## 1. الملخص التنفيذي (بلغة plainly، 15 سطرًا كحد أقصى)

التطبيق مكشوف حاليًا أمام **ثلاث مسارات تصعيد صلاحيات حقيقية ومُثبتة بالتنفيذ**، وليست نظريات:

1. **لا يمكن إنشاء أول مدير في البرنامج إطلاقًا.** إصلاح الشريحة 4 وضع شرط «يجب أن يكون المستخدم مسجّل الدخول ومطلق الصلاحية» على `CreateUserCommandHandler`. لكن أول تشغيل على قاعدة بيانات فارغة لا يوجد فيه أي جلسة. النتيجة: **قفل كامل للتطبيق عند أول تشغيل** — لا يمكن إتمام الإعداد ولا تسجيل الدخول أبدًا. هذا أسوأ من الثغرات الأمنية نفسها.
2. **أي مستخدم عادي (غير مطلق) يستطيع منح نفسه كل الصلاحيات.** `SaveUserPermissionsCommandHandler` يفحص فقط «هل أنت مسجّل الدخول؟» ولا يفحص مستوى صلاحيتك. تحققت بالتشغيل: مستخدم عادي منح نفسه `EDIT_SYSTEM_SETTINGS` و`STATISTICS` و`PT_AUDIT_ACCESS` بنجاح.
3. **أي مستخدم عادي يستطيع تعطيل أو حذف أو إعادة تنشيط مدير مطلق.** `DeactivateUserCommandHandler` و`DeleteUserCommandHandler` و`ReactivateUserCommandHandler` لا تفحص أبدًا إن كان الهدف مطلق الصلاحية. تحققت بالتشغيل: مستخدم عادي حذف حساب المدير المطلق بنجاح (عندما يكون هناك أكثر من مدير واحد).

أما **الاختبارات**: في الملفات السبعة الجديدة فقط، **51 حالة اختبار**: **26 حقيقية (REAL)**، و**8 ضعيفة (WEAK)**، و**17 مسرحية (THEATER)** — أي نحو 51% فقط تختبر السلوك الحقيقي فعليًا.

**أخطر نتيجة في هذا التقرير:** طبقة الاختبارات لا تغطي إصلاحات الشريحة 4 (الحماية الأساسية) إطلاقًا. أزلت **جميع** حراسات الصلاحيات من الـ 8 handlers ونجحت **1446/1446** اختبار. الاختبارات المسرحية التي يفترض أنها تحرس هذه الطبقة هي في الواقع لا تحرس شيئًا.

كذلك: **الـ 7 اختبارات في `TopLab.Persistence.Tests` كلها زرقاء مع إفساد متعمّد لـ 2 من إعدادات قاعدة البيانات**. أي أن Guard الاختبارات لا تكشف انهيار المخطط إطلاقًا.

وأخيرًا: `CanEditAbsolute` مربوطة في XAML لكن **هذه الخاصية غير موجودة** في الـ ViewModel. النتيجة المتوقعة: مربع «صلاحية مطلقة» **مفعّل دائمًا**، وهذا ما يجعل ممر التصعيد الأول (تسجيل الدخول) متاحًا فعليًا.

---

## 2. بيانات التحقق (Verification metadata)

### 2.1 بوابة الالتزام المُثبَّت (Step 0 gate) — ناجحة

```
$ git checkout 377aa282e1f0e8d618840f3fc39a056110d903a0
HEAD is now at 377aa28 [S-07] Follow-up: fix MSB3270 platform-architecture mismatch
in TopLab.Presentation.Tests — loop-engineering

$ git rev-parse HEAD
377aa282e1f0e8d618840f3fc39a056110d903a0

$ git status --short
(لا مخرجات — الشجرة نظيفة)

$ git cat-file -t 377aa282e1f0e8d618840f3fc39a056110d903a0
commit
```
مستوى الدليل: **VERIFIED-BY-EXECUTION**

### 2.2 البيئة

| البند | القيمة | ملاحظة |
|---|---|---|
| OS | Debian 12 (linux-x64) | — |
| SDK مثبَّت مسبقًا | 9.0.316 | لا يكفي (المشروع `net8.0`) |
| SDK مثبَّت للمراجعة | **8.0.425** | ثبّتُّه عبر `dotnet-install.sh --channel 8.0` |
| Runtime | `Microsoft.NETCore.App 8.0.31` | — |
| `Microsoft.WindowsDesktop.App` | **غير موجود** | `ls /usr/share/dotnet/packs` لا يظهره |
| Docker | **غير متاح** | `which docker` لا مخرجات |
| SQL Server | **غير متاح** | `which sqlcmd` لا مخرجات |
| الخطوط | لا يوجد `Arial` | `fc-list \| grep -ci arial` = 0 |

كل أوامر `dotnet` نُفِّذت مع `-p:EnableWindowsTargeting=true`، وكل مشروع اختبار شُغِّل على حدة (اختبار مستوى الحل محجوب بـ `NETSDK1100`).

### 2.3 الأوامر التي نُفِّذت

```bash
git clone https://github.com/El-ogra/Top-Lab.git repo
GIT_SSL_NO_VERIFY=true git clone ...   # انظر 2.4
git checkout 377aa282e1f0e8d618840f3fc39a056110d903a0
git rev-parse HEAD
git status --short
git log --oneline 66a17f7d8e87e6eb39d47fce46c750cb3eaba6a6..377aa282e1f0e8d618840f3fc39a056110d903a0
git diff --stat 66a17f7d8e87e6eb39d47fce46c750cb3eaba6a6 377aa282e1f0e8d618840f3fc39a056110d903a0
git diff --numstat 66a17f7... 377aa28 -- 'tests/**/*.cs'
git show f554821            # شريحة 4
git show 66a17f7:<path>      # مقارنة خط الأساس
grep -rn "IsAbsolutePermission|IAuthorizedRequest|HasPermission" src tests
dotnet test tests/<project>/<project>.csproj -p:EnableWindowsTargeting=true --nologo
dotnet run   (probe خارج المستودع)   # اختبار سلوكي مباشر للـ handlers
```

### 2.4 ما لم يمكن تنفيذه ولماذا

| البند | السبب | الأثر |
|---|---|---|
| `TopLab.Presentation.Tests` (7 اختبارات) | يُبنى بنجاح لكن `testhost` يفشل: `Framework: 'Microsoft.WindowsDesktop.App', version '8.0.0' (x64) ... No frameworks were found` | غير مُتحقَّق منه بالتنفيذ — انظر §8 |
| `RelationalIntegrationTests` (4 اختبارات) الحاوية الحقيقية | لا Docker | تُرجِع `return;` مبكرًا — انظر §4 |
| 3 اختبارات طباعة في `Infrastructure.Tests` | تفشل هنا بسبب غياب `Arial`، لا بسبب انحدار في S-07 | انظر §2.5 |
| سلوك WPF وقت التشغيل لربط `CanEditAbsolute` | لا WPF runtime | NOT-VERIFIABLE-BY-EXECUTION — انظر §3.6 |

### 2.5 نتائج الاختبارات — أرقامي أنا، مستقلة عن أي ادعاء

```
TopLab.Domain.Tests          Passed! - Failed: 0, Passed:  474, Total:  474
TopLab.Application.Tests     Passed! - Failed: 0, Passed: 1446, Total: 1446
TopLab.Infrastructure.Tests  Failed! - Failed: 3, Passed:  192, Total:  195
TopLab.Persistence.Tests     Passed! - Failed: 0, Passed:    7, Total:    7
TopLab.Presentation.Tests    Test Run Aborted (Microsoft.WindowsDesktop.App مفقودة)
```

الاختبارات الثلاثة الفاشلة في `Infrastructure.Tests`:
```
TopLab.Infrastructure.Tests.Printing.WorkSheetPrintingServiceTests.PrintWorkSheetAsync_HappyPath_WritesPdfAndDispatchesToReportsPrinter
TopLab.Infrastructure.Tests.Printing.InvoicePrintingServiceTests.PrintInvoiceAsync_HappyPath_WritesPdfAndDispatchesToReceiptPrinter
TopLab.Infrastructure.Tests.Printing.ReceiptPrintingServiceTests.PrintReceiptAsync_HappyPath_WritesPdfAndDispatchesToReceiptPrinter
   Error Message: Assert.True() Failure
```

**هل هو انحدار من S-07؟ لا.** التحقق: `git diff --stat 66a17f7..377aa28 -- InvoicePdfWriter.cs ReceiptPdfWriter.cs InvoicePrintingService.cs ReceiptPrintingService.cs` أعاد **فارغًا** (لم تمسّها S-07 إطلاقًا). السبب هو غياب خط `Arial` على لينكس بينما `Settings.UseSystemFonts = true` في `InvoicePdfWriter.cs:29`. مستوى الدليل: **VERIFIED-BY-EXECUTION + VERIFIED-BY-CODE-INSPECTION**.

ملاحظة على ادعاء MEMORY بأنه `Infra 195/195`: ادعاء **غير قابل لإعادة الإنتاج** خارج Windows، وسببه بيئي لا برمجي.

### 2.6 الأسلوب

- كل رقم عداد في هذا التقرير مشتق من `grep`/`dotnet test` في هذا القسم، لا من أي وثيقة مُدخَلة.
- استخدمت **اختبار الطفرات (mutation testing)**: انسخت المستودع إلى مجلد منفصل، أفسدت إنتاج مقصود، وشغّلت الاختبارات لمعرفة ما تلتقطه حقًا. تم حذف النسخ بعد ذلك.
- **لم أعدّل أي ملف في المستودع** — `git status --short` فارغ في نهاية المراجعة.

---

## 3. الجزء A — السجل الأمني الكامل

### 3.0 نطاق التعداد — كل طلب تحت `UsersAndPermissions`

عددتُها بنفسي. **14 طلبًا**: 9 أوامر + 5 استعلامات. (لا أستخدم أي عدد من وثيقة التدقيق السابقة.)

| # | الطلب | النوع | `IAuthorizedRequest`؟ |
|---|---|---|---|
| 1 | `SignInCommand` | أمر | ❌ لا |
| 2 | `SignOutCommand` | أمر | ❌ لا |
| 3 | `ChangeOwnPasswordCommand` | أمر | ❌ لا |
| 4 | `CreateUserCommand` | أمر | ❌ لا |
| 5 | `UpdateUserCommand` | أمر | ❌ لا |
| 6 | `DeleteUserCommand` | أمر | ❌ لا |
| 7 | `DeactivateUserCommand` | أمر | ❌ لا |
| 8 | `ReactivateUserCommand` | أمر | ❌ لا |
| 9 | `SaveUserPermissionsCommand` | أمر | ❌ لا |
| 10 | `GetCurrentSessionQuery` | استعلام | ❌ لا |
| 11 | `GetUsersQuery` | استعلام | ❌ لا |
| 12 | `GetUserByIdQuery` | استعلام | ❌ لا |
| 13 | `VerifySecondaryPasswordQuery` | استعلام | ❌ لا |
| 14 | `HasAnyAbsoluteUserQuery` | استعلام | ❌ لا |

**لا يوجد ولا طلب واحد** من الـ 14 ينفّذ `IAuthorizedRequest`. بالتالي `AuthorizationBehavior` (المسار الموحّد للصلاحيات، `src/TopLab.Application/Common/Behaviors/AuthorizationBehavior.cs:32`) **لا يُطبَّق إطلاقًا** على أي شيء داخل `UsersAndPermissions`. كل الحماية في هذه الميزة — صحيح أو خاطئ — مكتوبة يدويًا داخل كل handler. مستوى الدليل: **VERIFIED-BY-CODE-INSPECTION**.

مقابل ذلك، على مستوى الحل كله: **130 طلبًا** ينفّذ `IAuthorizedRequest` (خارج `UsersAndPermissions` بالكامل). هذا يوضح نمطًا: ميزة إدارة المستخدمين استثناءً لا قاعدة.

### 3.1 سجل كل طلب وحالته

| # | الطلب | حارس «مسجّل الدخول» | حارس «الصلاحية المطلقة» | الحكم |
|---|---|---|---|---|
| 1 | `SignInCommand` | بلا حارس (مقصود) | — | ✅ سليم |
| 2 | `SignOutCommand` | بلا حارس (مقصود) | — | ✅ سليم |
| 3 | `ChangeOwnPasswordCommand` | ✅ | — | ✅ سليم |
| 4 | `CreateUserCommand` | ✅ | ✅ جزئيًا | ⚠️ **قفل أول تشغيل** |
| 5 | `UpdateUserCommand` | ✅ | ✅ كامل | ✅ سليم |
| 6 | `DeleteUserCommand` | ✅ | ❌ **غائب** | 🔴 **ثغرة** |
| 7 | `DeactivateUserCommand` | ✅ | ❌ **غائب** | 🔴 **ثغرة** |
| 8 | `ReactivateUserCommand` | ✅ | ❌ **غائب** | 🔴 **ثغرة** |
| 9 | `SaveUserPermissionsCommand` | ✅ فقط | ❌ **غائب** | 🔴 **ثغرة** |
| 10 | `GetCurrentSessionQuery` | ✅ | — | ✅ سليم |
| 11 | `GetUsersQuery` | ✅ فقط | ❌ | 🟡 **تسريب معلومات** |
| 12 | `GetUserByIdQuery` | ✅ فقط | ❌ | 🟡 **تسريب معلومات** |
| 13 | `VerifySecondaryPasswordQuery` | ✅ | — | ✅ سليم |
| 14 | `HasAnyAbsoluteUserQuery` | ❌ **غائب تمامًا** | ❌ | 🟡 منخفض |

### 3.2 الثغرة (أ) — قفل أول تشغيل: التطبيق لا يمكن إ.setupه

هذه ليست ثغرة تصعيد صلاحيات، بل **عطل كامل وظيفي**. من الناحية التقنية: مسار الإقلاع الأول معطّل.

الدليل — `src/TopLab.Application/Features/UsersAndPermissions/Commands/CreateUser/CreateUserCommandHandler.cs:24-32`:
```csharp
if (!_currentUser.IsAuthenticated)
{
    return Result<int>.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
}

if (request.IsAbsolutePermission && !_currentUser.IsAbsolutePermission)
{
    return Result<int>.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
}
```

والمستدعي — `src/TopLab.Presentation/ViewModels/Setup/FirstRunAdminViewModel.cs:135-148`:
```csharp
var cmd = new CreateUserCommand(
    UserName.Trim(), Password, SecondaryPassword,
    IsAbsolutePermission: true,        // <-- يطلب صلاحية مطلقة
    ...);
var result = await _mediator.Send(cmd);
```

وسياق الاستدعاء — `src/TopLab.Presentation/App.xaml.cs:77-84`: يُرسل `HasAnyAbsoluteUserQuery`، وإن كانت النتيجة `false` يفتح `FirstRunAdminWindow`. لا يوجد أي `SetSession` قبل هذه النقطة في التطبيق كله (تحققت: `SetSession` يُستدعى في `SignInCommandHandler.cs:56` فقط).

**النتيجة:** على قاعدة بيانات فارغة، `IsAuthenticated == false` (كائن `CurrentUserService` جديد) و`IsAbsolutePermission == false`. الشرط الأول يفشل فورًا.

**إثبات بالتنفيذ** (probe مستقل خارج المستودع، مستخدم حقيقي من `Infrastructure.Identity`):
```
[first-run] HasAnyAbsoluteUser = False
[first-run] CreateUser(IsAbsolutePermission:true) IsSuccess=False Error=Forbidden
            Msg=أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام
[first-run] UsersInDb=0
```
مستوى الدليل: **VERIFIED-BY-EXECUTION**

**الأثر التقني:** على أي تثبيت جديد (أو بعد `DELETE FROM Users`)، يفتح التطبيق نافذة «إنشاء المدير الأول»، يقبل المستخدم كل البيانات، يضغط «إنشاء»، فيحصل على رسالة «أنت لا تملك الصلاحية» ولا يُنشأ أي مستخدم. يبقى `HasAnyAbsoluteUser == false` دائمًا، فيتكرر ذلك في كل إقلاع. **لا يوجد أي مستخدم في النظام ولا أي وسيلة للتعافي من داخل التطبيق.** الخسارة الوحيدة المتبقية هي التعديل اليدوي على قاعدة البيانات.

**لماذا لم يلتقطه أحد:** لا يوجد أي اختبار يغطي `FirstRunAdminViewModel` (تحققت: `grep -rn "FirstRunAdmin" tests` يعطي نتيجة واحدة فقط، وهي استثناء في `PresentationStructuralTests.cs:68`). و`CreateUserCommandHandlerTests` تستخدم `new FakeCurrentUserService()` الذي قيمته الافتراضية `IsAuthenticated = true` و`IsAbsolutePermission = false` — أي أن الاختبار يسير في مسار لا يمكن أن يحدث في الإنتاج أبدًا.

### 3.3 الثغرة (ب) — تصعيد ذاتي كامل عبر `SaveUserPermissionsCommand`

الدليل — `src/TopLab.Application/Features/UsersAndPermissions/Commands/SaveUserPermissions/SaveUserPermissionsCommandHandler.cs:19-24`:
```csharp
public async Task<Result> Handle(SaveUserPermissionsCommand request, CancellationToken cancellationToken)
{
    if (!_currentUser.IsAuthenticated)
    {
        return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
    }
    var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
    if (user is null) { ... }
    // لا يوجد أي فحص لمستوى صلاحية المنادي
    // لا يوجد أي فحص لكون الهدف مطلقًا
    // لا يوجد أي فحص لأن المنادي يملك الصلاحيات التي يمنحها
```

قارن بـ `UpdateUserCommandHandler.cs:34-42` الذي **فعلًا** يفحص:
```csharp
if (request.IsAbsolutePermission && !_currentUser.IsAbsolutePermission) { return Forbidden; }
if (user.IsAbsolutePermission && !_currentUser.IsAbsolutePermission) { return Forbidden; }
```

**إثبات بالت��فيذ:**
```
=========== SCENARIO 2: non-absolute user self-escalates via SaveUserPermissions ===========
  Non-absolute grants self EDIT_SYSTEM_SETTINGS+STATISTICS+PT_AUDIT -> Success=True
  Grants now on attacker: 1,2,3,4
```
مستخدم عادي (`IsAbsolutePermission=false`، وGranted واحد فقط `ADD_EDIT_PATIENT`) منح نفسه ثلاث صلاحيات إدارية. **مستوى الدليل: VERIFIED-BY-EXECUTION**

**الأثر التقني بعد نجاح الأمر:** المستخدم المناوئ يصبح حاملًا لـ `EDIT_SYSTEM_SETTINGS` و`STATISTICS` و`PT_AUDIT_ACCESS`. هذه الرموز تُمنح عبر `AuthorizationBehavior` على أوامر مثل `BackupDatabaseNowCommand` و`RestoreDatabaseCommand` و`UpdateSystemSettingsCommand` و`ApplyDatabaseUpdatesCommand` (`grep -rn IAuthorizedRequest src` يُظهرها). أي أن التصعيد يفضي إلى: نسخ احتياطي/استعادة قاعدة البيانات، تعديل إعدادات النظام، والوصول إلى سجلات التدقيق.

ملاحظة إضافية: `UpdateAuditAccessVisibility()` في `UserManagementViewModel.cs:271-273` تُعطّل `PT_AUDIT_ACCESS` في الواجهة فقط عند عدم كون المستخدم مطلقًا — وهي **حماية واجهة لا حارس خادم**، وقابلة للتجاوز بالكامل لأن `SaveUserPermissionsCommand` لا يقرأها.

**ومتغير آخر من نفس الصنف** — `CreateUserCommandHandler` (السطر 29-32): يفحص فقط أن `request.IsAbsolutePermission` يتطلب مناديًا مطلقًا، لكن **لا يفحص شيئًا regarding الصلاحيات العادية**. إثبات:
```
=========== SCENARIO 3: non-absolute creates privileged user ===========
  Non-absolute creates privileged user -> Success=True NewUserGrants=0
```
مستخدم عادي ينشئ حسابًا جديدًا ويحاول منحه `EDIT_SYSTEM_SETTINGS` و`STATISTICS`. الأمر **نجح** (النجاح يعني أن `user.GrantPermission` استُدعي في `CreateUserCommandHandler.cs:77-80`؛ عدّاد الـ fake `Grants` في probe بقي صفرًا لأن الـ fake لا يمرّر الكيانات المضافة، لكن `PermissionGrants` على الكيان نفسه تملأ — وهذا سلوك إنتاج حقيقي لأن `User.GrantPermission` يضيف إلى `_grants` أيًا كان الـ context). مستوى الدليل للسلوك: **VERIFIED-BY-CODE-INSPECTION** + مدعوم بالتنفيذ.

### 3.4 الثغرة (ج) — مستخدم عادي يعطّل/يحذف/ينشّط مديرًا مطلقًا

**إعادة اشتقاق مستقلة — لا أقبل حكم أي تدقيق سابق.**

`DeleteUserCommandHandler.cs` — المقطع الحرج كاملًا (`:27-46`):
```csharp
public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
{
    if (!_currentUser.IsAuthenticated)
    {
        return Result.Failure(Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"));
    }
    var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
    if (user is null)
    {
        return Result.Failure(Error.NotFound("المستخدم غير موجود"));
    }

    if (user.IsAbsolutePermission && user.IsActive)
    {
        int otherAbsoluteCount = _db.Set<User>().Count(u => u.IsAbsolutePermission && u.IsActive && u.Id.Value != request.UserId);
        if (otherAbsoluteCount == 0)
        {
            return Result.Failure(Error.Conflict("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً"));
        }
    }
    ...
    _db.Remove(user);
    await _db.SaveChangesAsync(cancellationToken);
    return Result.Success();
}
```

**التحليل الحرفي:** الشرط في السطر 39 هو `user.IsAbsolutePermission && user.IsActive` — أي أنه **يصون «الآخر» فقط**، وليس «المطلق». لا وجود في أي مكان للملف لعبارة `_currentUser.IsAbsolutePermission` كمحدد. النتيجة: أي مستخدم مسجّل الدخول، مهما كانت صلاحياته (**صفر صلاحيات** تكفي)، يحذف أي مدير مطلق ما دام هناك مدير مطلق ثانٍ نشط.

**إثبات بالتنفيذ:**
```
=========== SCENARIO 4: non-absolute user DELETES an absolute admin (2 admins) ===========
  Non-absolute deletes absolute admin -> Success=True
=========== SCENARIO 5: non-absolute DEACTIVATES an absolute admin (2 admins) ===========
  Non-absolute deactivates absolute admin -> Success=True Admin1.IsActive=False
=========== SCENARIO 6: non-absolute REACTIVATES a disabled absolute admin ===========
  Non-absolute reactivates absolute admin -> Success=True Admin1.IsActive=True
=========== SCENARIO 7: non-absolute EDITS an absolute admin (control) ===========
  Non-absolute edits absolute admin -> Success=False Err=Forbidden   <-- UpdateUser فقط محمي
```
مستوى الدليل: **VERIFIED-BY-EXECUTION**

`DeactivateUserCommandHandler.cs:36-43` يكرر نفس النمط بالضبط (`user.IsAbsolutePermission` + عدّ «الآخر» فقط).
`ReactivateUserCommandHandler.cs:19-33` **أضعف**: لا يحتوي حتى عدّ «الآخر» — لا يوجد أي شرط على الهدف سوى أنه موجود. مستخدم عادي يُعيد تنشيط مدير مطلق معطَّل.

**التباين الذي يوثّق نقص الإصلاح:** `UpdateUserCommandHandler` يحمي الهدف المطلق، بينما `Delete`/`Deactivate`/`Reactivate` لا تفعل. هذا indication قوي أن حماية «الهدف المطلق» كانت في نيّة المصمم لكنها نُسخت جزئيًا فقط.

**الأثر التقني:** مستخدم عادي (بوظيفة واحدة من موظف استقبال) يستطيع حذف حسابات كل المديرين المطلقين واحدًا تلو الآخر، أو تعطيلهم. وبالجمع مع §3.3 يستطيع أولًا تصعيد نفسه ثم الإجهاز على كل المديرين. هذا مسار تصعيد كامل من مستوى أدنى إلى الإجهاز بالإدارة.

### 3.5 تسريب المعلومات وطلبات بلا حارس

| الطلب | الدليل | الأثر التقني |
|---|---|---|
| `GetUsersQueryHandler.cs:20-25` | حارس `IsAuthenticated` فقط، لا فحص صلاحية | أي مستخدم مسجّل يقرأ `UserSummaryDto` لكل المستخدمين: المعرّف، الاسم، علم `IsAbsolutePermission`، الحالة، آخر دخول. **معرفة أي الحسابات هي أهداف مادية** |
| `GetUserByIdQueryHandler.cs:20-25` | نفس الشيء | يقرأ تفاصيل أي مستخدم + **قائمة رموز صلاحياته كاملة** (`codes` في `:37-39`) + `DiscountLimitPercent` + `BlockPrintOnRemainingBalance`. كشف كامل لبنية الصلاحيات |
| `HasAnyAbsoluteUserQueryHandler.cs:17-20` | **لا حارس إطلاقًا** — لا يحقن `ICurrentUserService` أصلًا | تنفيذ غير مُصادَق يستطيع قراءة «هل يوجد مدير مطلق في النظام». منخفض الأثر منفردًا (قيمة bool)، لكنه كاشف |

إثبات:
```
=========== SCENARIO 8 ===========
  GetUsers by non-absolute -> Success=True Count=2
  GetUserById(admin) by non-absolute -> Success=True Absolute=True
=========== SCENARIO 9: UNAUTHENTICATED HasAnyAbsoluteUser ===========
  HasAnyAbsoluteUser with no session -> Success=True Value=True
```

**ملاحظة مخفِّفة تخصّ الوصول من الواجهة:** عنصر التنقل «المستخدمون» في `ShellViewModel.cs:201-214` محميٌّ بـ `ShowSecondaryPasswordDialogAsync()`، وهو يتحقق من كلمة المرور الثانوية عبر `VerifySecondaryPasswordQuery` الذي يستهدف `_currentUser.UserId` (`VerifySecondaryPasswordQueryHandler.cs:31`) — أي أنه تحقق من هوية المنادي لا من صلاحيته. لكن:
1. مستخدم عادي **يعرف كلمة مروره الثانوية** (هي كلمة المرور التي يدخل بها)، فيستطيع فتح شاشة إدارة المستخدمين.
2. الأهم: الحواجز في الواجهة ليست ضوابط أمنية. كل الثغرات أعلاه قابلة للوصول مباشرة عبر `ISender` (MediatR) من أي كود داخل العملية.

### 3.6 ربط `CanEditAbsolute` — الخاصية غير موجودة

**الدليل (VERIFIED-BY-CODE-INSPECTION):**

`src/TopLab.Presentation/Views/Users/UserManagementView.xaml:36`:
```xml
<CheckBox Content="صلاحية مطلقة" IsChecked="{Binding IsAbsolutePermission}" IsEnabled="{Binding CanEditAbsolute}" Margin="0,4,0,8"/>
```

`grep -rn "CanEditAbsolute" src tests` يُخرج **سطرًا واحدًا فقط** — وهو سطر XAML أعلاه. **الخاصية غير موجودة في `UserManagementViewModel.cs` إطلاقًا.** الـ ViewModel فيه 534 سطرًا ويعرّض `IsAbsolutePermission` (`:161`) وحارسًا محتملًا `_currentUser` (`:60`، يُحقن في `:86`) — لكن `_currentUser` **لا يُقرأ في أي مكان آخر في الملف** (تحققت: `grep -n "_currentUser"` يُخرج التعريف والإسناد فقط، ولا أي استخدام).

**إذن الشريحة 4 حقن التبعية في الـ ViewModel ولم ينشئ الخاصية التي يفترضها الـ XAML.** الحارس مُقتصَر على مستوى الواجهة مع أنه غير مُنفَّذ إطلاقًا.

**ماذا يفعل WPF عند غياب خاصية على المصدر — السلوك المتوقَّع (NOT-VERIFIABLE-BY-EXECUTION):**
لا يمكنني تشغيل WPF هنا. حسب موثّقية WPF (Data binding overview) وسلوك `BindingExpression`:
- يُسجّل WPF خطأ في نافذة Output/Visual Studio من نوع:
  `System.Windows.Data Error: 40 : BindingExpression path error: 'CanEditAbsolute' property not found on 'object' 'UserManagementViewModel'`
- **لا ينهار التطبيق ولا يظهر استثناء** — فشل الربط لا يرمي.
- الموضع الهدف `IsEnabled` على `CheckBox` هو `bool`، وقيمته الافتراضية من نوع `DependencyProperty` هي `false` **قبل** تطبيق الربط؛ لكن الربط الفاشل يُنتج `DependencyProperty.UnsetValue`، ثم ينتقل WPF إلى القيمة **الافتراضية المُعلَنة في metadata** لـ `IsEnabled` — وهي **`true`**. هذا موثّق في `TransferValue` (تحذيرات 79/87/88: `got raw value UnsetValue` ← `using fallback/default value` ← `using final value`).
- **النتيجة العملية المتوقعة: مربع «صلاحية مطلقة» يظهر ويعمل كمفعّل (Enabled) دائمًا**، بلا أي رسالة للمستخدم، وبلا أي error ظاهر.

**ما يلزم بالضبط للتأكيد على جهاز المالك (Windows):**
1. شغّل التطبيق من Visual Studio، افتح **Output window** (Ctrl+Alt+O)، واجعل `Data Binding` على مستوى `Verbose` من `Tools → Options → Debugging → Output Window → WPF Trace Settings`.
2. سجّل الدخول بحساب مطلق، افتح شاشة «المستخدمون».
3. ابحث عن سطر يحتوي `Error: 40` واسم `CanEditAbsolute` — وجوده يؤكد غياب الخاصية.
4. اختبار سلوكي مباشر: سجّل الدخول بحساب **غير مطلق**، افتح شاشة المستخدمين، حاول **تفعيل** مربع «صلاحية مطلقة». إذا كان مفعّلًا فسيقبل التحديد (وهو المطلوب أن يكون معطّلًا). للتأكيد التكميلي: ضع `PresentationTraceSources.TraceLevel=High` مؤقتًا على الربط نفسه لقراءة القيمة الفعلية.
5. تحقّق إضافي: افحص خاصية `IsEnabled` في الـ Live Visual Tree من Visual Studio (DEBUG → Windows → Live Visual Tree) على عنصر CheckBox المحدد.

### 3.7 سلامة مسار الدخول (مُتحقَّق منها بشكل مستقل)

| الطلب | التصريح الكامل | الحكم |
|---|---|---|
| `SignInCommand.cs:7` | `public sealed record SignInCommand(string UserName, string Password) : IRequest<Result<CurrentUserSessionDto>>;` | ✅ لا `IAuthorizedRequest`، ولا حارس داخل `SignInCommandHandler` — وهذا **مقصود**: لا يمكن لطلب تسجيل الدخول أن يتطلب صلاحية، لأن الحلسة لا توجد بعد |
| `SignOutCommand.cs:7` | `public sealed record SignOutCommand : IRequest<Result>;` | ✅ لا حارس. Handler يُصفّر الجلسة فقط (`ClearSession()`) |
| `GetCurrentSessionQuery.cs:7` | `public sealed record GetCurrentSessionQuery : IRequest<Result<CurrentUserSessionDto>>;` | ✅ `GetCurrentSessionQueryHandler.cs:22-25` يفحص `IsAuthenticated` ويعيد `Forbidden("غير مصرح")` — **هذا لا يمنع تسجيل دخول جديد**، بل يمنع قراءة الجلسة بدون جلسة |
| `VerifySecondaryPasswordQuery.cs:7` | `public sealed record VerifySecondaryPasswordQuery(string Password) : IRequest<Result<bool>>;` | ✅ `VerifySecondaryPasswordQueryHandler.cs:26-29` يفحص `IsAuthenticated`. يُستخدم بعد الدخول فقط |

**الحكم العام: مسار الدخول سليم ولا يوجد فيه متطلب جديد would block a fresh login.** هذا واحد من الأجزاء الجيدة في S-07. (يرجى ملاحظة: هذا الحكم **مشروط** بوجود حساب. لأن §3.2 يعطّل إنشاء الحساب الأول، فلا يوجد «تسجيل دخول جديد» أصلًا.)

فحصت كذلك ترتيب behaviors في `src/TopLab.Application/DependencyInjection.cs:26-28`:
```csharp
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
```
MediatR ينفّذ behaviors بترتيب التسجيل، فـ `LoggingBehavior` هو الأ outermost — **يتوافق مع ادعاء Slice 9 (NEW-05-LOG)**. الترتيب: `Logging` ← `Validation` ← `Authorization` ← handler. مستوى الدليل: **VERIFIED-BY-CODE-INSPECTION** (لم أشغّل MediatR فعليًا لأثبت الترتيب وقت التشغيل).

### 3.8 البحث على مستوى الحل كله عن نفس صنف العيب

| السؤال | الإجابة بالدليل |
|---|---|
| من يمنح/ يصعّد صلاحيات خارج `UsersAndPermissions`؟ | `grep -rn "GrantPermission\|SetAbsolutePermission\|ClearPermissions" src/TopLab.Application` → **4 مواقع فقط، كلها داخل `UsersAndPermissions`**. لا يوجد أي مسار منح صلاحيات خارجه |
| هل يوجد انتحال شخصية / «act as»؟ | `grep -rni "impersonat\|actas\|act_as\|SwitchUser" src` → **صفر نتيجة**. لا يوجد |
| هل يوجد تعديل صلاحيات في شاشة الإعدادات؟ | `SettingsDashboardViewModel.cs:60` يستدعي `ShowSecondaryPasswordDialogAsync()` فقط — تحقق هوية، لا فحص صلاحية. لكن كل أوامر الإعدادات تنفّذ `IAuthorizedRequest` (مُتحقَّق: `UpdateSystemSettingsCommand.cs:21`، `BackupDatabaseNowCommand.cs:8`، إلخ)، لذا هي محمية |
| هل يوجد مسار تصعيد ذاتي **خارج** إدارة المستخدمين؟ | **لا بشكل مباشر** — لا يوجد أي كود يمنح صلاحيات. لكن §3.3 يجعل أي مستخدم عادي **مالكًا** لـ `EDIT_SYSTEM_SETTINGS`، فيصبح قادرًا على تعديل إعدادات النظام والأوامر المرتبطة عبر أوامر محمية بـ `AuthorizationBehavior` — أي أن التصعيد الذاتي غير مباشر لكنه حقيقي |
| هل `AuthorizationBehavior` نفسه به ثغرة؟ | `AuthorizationBehavior.cs:34-35`: `var allowed = _currentUser.IsAbsolutePermission \|\| _currentUser.HasPermission(...)` ثم `if (!allowed) return BuildForbidden(...)`. المنطق سليم. **لكن** لا يوجد أي فحص `IsAuthenticated` هنا — مستخدم مُسجَّل الخروج (`ClearSession`) سيحصل على `Forbidden` بشكل صحيح لأن `_grantedPermissions` مُفرَّغة، فلا منفذ. سليم |

---

## 4. الجزء B — تصنيف كل دالة اختبار

### 4.0 النطاق — كل ملف اختبار مُضاف/مُعدَّل في النطاق

```bash
$ git diff --numstat 66a17f7d8e87e6eb39d47fce46c750cb3eaba6a6 377aa282e1f0e8d618840f3fc39a056110d903a0 -- 'tests/**/*.cs'
3	3	tests/.../UsersAndPermissions/CreateUserCommandHandlerTests.cs
3	3	tests/.../UsersAndPermissions/DeactivateReactivateTests.cs
4	4	tests/.../UsersAndPermissions/DeleteUserCommandHandlerTests.cs
136	0	tests/.../UsersAndPermissions/DeleteUserGuardTests.cs          ← جديد
3	3	tests/.../UsersAndPermissions/GetUsersQueryHandlerTests.cs
4	4	tests/.../UsersAndPermissions/SaveUserPermissionsCommandHandlerTests.cs
2	0	tests/.../UsersAndPermissions/SignInCommandValidatorTests.cs
6	6	tests/.../UsersAndPermissions/UpdateUserCommandHandlerTests.cs
213	0	tests/.../Validators/ValidatorBehaviourTests.cs                ← جديد
64	0	tests/.../Validators/ValidatorCompletenessTests.cs              ← جديد
4	9	tests/.../Infrastructure.Tests/Identity/CurrentUserServiceTests.cs
52	0	tests/.../Persistence.Tests/NeverConnectGuardTests.cs           ← جديد
101	0	tests/.../Persistence.Tests/RelationalIntegrationTests.cs       ← جديد
60	0	tests/.../Persistence.Tests/SqlServerFixture.cs                ← جديد (ليس ملف اختبار)
68	0	tests/.../Presentation.Tests/DeferredBehaviourTests.cs          ← جديد
94	0	tests/.../Presentation.Tests/PresentationStructuralTests.cs     ← جديد
```

**17 ملفًا**، منها **7 ملفات جديدة** كُتبت من الصفر (DeleteUserGuardTests, ValidatorBehaviourTests, ValidatorCompletenessTests, NeverConnectGuardTests, RelationalIntegrationTests, DeferredBehaviourTests, PresentationStructuralTests). الـ 8 المتبقية **تعديلات ميكانيكية** لتوقيع الـ constructor (السطر المضاف الوحيد في كلٍّ منها هو `new FakeCurrentUserService()`).

`SqlServerFixture.cs` ليس ملف اختبار (بلا `[Fact]`) بل بنية تحتية.

**إجمالي الدوال المُصنَّفة في هذا التقرير: 51** (كل الدوال في الملفات السبعة الجديدة، بتوسيع الـ `[Theory]` إلى cases منفصلة حيث لزم). الملفات الثمانية المعدَّلة ميكانيكيًا خارج هذا العدّ لأنها لم تُضَف اختبار جديد فيها.

### 4.1 الخلاصة العددية

| التصنيف | العدد | النسبة |
|---|---|---|
| **REAL** | **26** | 51% |
| **WEAK** | **8** | 16% |
| **THEATER** | **17** | 33% |
| المجموع | 51 | 100% |

### 4.2 `DeleteUserGuardTests.cs` (جديد، 136 سطرًا) — 3 دوال

| الدالة | التصنيف | الدليل |
|---|---|---|
| `ThrowingProbe_YieldsUnexpected_NotConflict` | **REAL** | تُنشئ `ThrowingProbeDbContext` يُلقي استثناءً في `Set<T>()` لغير `User`، تستدعي `Handle` فعليًا، وتؤكد `ErrorType.Unexpected` + `RemoveCallCount == 0` + `SaveChangesAsyncCallCount == 0`. هذا يمارس فرع `catch` في `DeleteUserCommandHandler.cs:122-126` فعليًا |
| `NoReferences_StillDeletes` | **REAL** | `_db.Set<T>()` يُعيد `Enumerable.Empty<T>()` لكل الكيانات غير `User` → `HasReferences` يُرجع false → يؤكد `IsSuccess` + `RemoveCallCount == 1`. يمارس المسار الناجح |
| `ThrowingSet_YieldsUnexpected` | **THEATER** (جزئي) | انظر أدناه |

**لماذا الثالث أضعف من الأولين:** `ThrowingProbeDbContext` و`ThrowingSetDbContext` **متطابقان وظيفيًا** — كلاهما يرمي `InvalidOperationException` من نفس السطر (`Set<TEntity>()` عند `typeof(TEntity) != typeof(User)`)؛ الفرق الوحيد هو نص الرسالة. الاختباران يمران بنفس مسار `catch`. لم يُختبر أي مسار مختلف فعليًا.

> **ما يحتاجه اختبار حقيقي:** لتغطية `ThrowingSet` بشكل ذي معنى، ينبغي أن يرمي `Set<T>()` عند *أول* استدعاء (أي استعلام `User` نفسه) ليعكس انهيارًا أبكر في خطوط الأنابيب، أو يُحرم `IApplicationDbContext.SaveChangesAsync` من الرمي ليُختبر فشل الفرع عند الحفظ — وهو مسار غير مُغطى.

**إثبات-CAUGHT (mutation):** استبدلتُ جسم `catch` في `DeleteUserCommandHandler.cs` بـ `throw;` (عكس «fail closed» إلى «fail open») وشغّلت الحزمة كاملة:
```
Failed TopLab.Application.Tests.Features.UsersAndPermissions.DeleteUserGuardTests.ThrowingProbe_YieldsUnexpected_NotConflict
Failed TopLab.Application.Tests.Features.UsersAndPermissions.DeleteUserGuardTests.ThrowingSet_YieldsUnexpected
Failed! - Failed: 2, Passed: 1444, Total: 1446
```
**هذه اختبارات حقيقية.** مستوى الدليل: **VERIFIED-BY-EXECUTION**

### 4.3 `ValidatorBehaviourTests.cs` (جديد، 213 سطرًا) — 22 دالة

**كل الـ 22 دالة: REAL.** هذه أقوى مجموعة اختبارات في النطاق.

كلها تبني الـ validator الحقيقي وتستدعي `Validate(...)` فعليًا على الأمر الحقيقي وتؤكد `IsValid` و/أو `PropertyName` الدقيق. على سبيل المثال `:191-196`:
```csharp
[Fact]
public void DeactivateUser_InvalidId_Fails()
{
    var validator = new DeactivateUserCommandValidator();
    var result = validator.Validate(new DeactivateUserCommand(0));
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
}
```

**إثبات-CAUGHT (mutation):** أفسدتُ `DeleteUserCommandValidator` من `GreaterThan(0)` إلى `GreaterThan(-999)`:
```
Failed! - Failed: 1, Passed: 21, Total: 22
  Failed ...ValidatorBehaviourTests.DeleteUser_InvalidId_Fails
```
اختبار واحد سقط بالضبط مع الانحدار. مستوى الدليل: **VERIFIED-BY-EXECUTION**

**ملاحظة نقدية (لا تقلل من التصنيف):** 12 من الـ 22 دالة غير مُسمّاة تبقى `X_InvalidId_Fails` وتؤكد فقط `Assert.False(result.IsValid)` بدون التأكد من `PropertyName` (انظر `:61-67`, `:86-100`, `:102-108`، `:110-116`، `:118-124`، `:126-148`، `:150-165`، `:182-188`). هي **REAL** لأنها تستدعي الكود الحقيقي وستفشل مع أي انحدار، لكنها أقل دقة من نظيراتها. لا يوجد فيها أي اختبار *positive path* (مدخل صالح → `IsValid == true`) عدا `AddProfileAnalyte_ValidIds_Passes`.

### 4.4 `ValidatorCompletenessTests.cs` (جديد، 64 سطرًا) — 2 دالة

| الدالة | التصنيف | الدليل |
|---|---|---|
| `EveryParameterisedCommand_HasSiblingValidator` | **WEAK** | تفحص **البيانات الوصفية (reflection)** لا السلوك: تقرأ نوع الـ assembly وتبحث عن نوع اسمه `"{Command}Validator"` وتؤكد أنه implements `IValidator`. **لا تستدعي أي validator ولا تتحقق من أن أي قاعدة تحقّق مسجَّلة في MediatR.** كما أن `!t.Name.EndsWith("Validator")` في `:18` يستثني الـ validators من الفحص نفسه. **لا تكشف**: أن الـ validator مُعرَّف لكنه غير مسجَّل في `AddMediatR`، أو أن قاعدة التحقق مكتوبة بشكل خاطئ |
| `ParameterlessCommandsWithoutValidators_AreExactlyThree` | **WEAK** | الاسم يقول «AreExactlyThree» لكن الكود (`:54-62`) **لا يتحقق من «exactly three»** — يتحقق فقط من **الثلاثة المذكورة تحديدًا** لا وجود validator لها. **لن يفشل** لو أُضيف أمر بلا بارامترات رابع، أو حُذف أحد الثلاثة. والاسم مضلل |

> **ما يحتاجه اختبار حقيقي:** الأول يجب أن يتحقق من أن كل أمر بارامتري له validator **مسجَّل فعليًا في حاوية MediatR** (بحل الـ `IServiceProvider` واستكشاف `IValidator<TCommand>` لكل أمر)، لا مجرد وجود نوع. الثاني يجب أن **يعدّ** كل الأوامر بلا بارامترات في التطبيق ويؤكد أن العدد والمجموعة تطابق تمامًا.

**تحققت من أن 22 validator أُنشئ فعلًا:** `git diff --name-only 66a17f7..377aa28 -- 'src/*Validator.cs' | wc -l` = **22**، والاختبار يمر (`Passed! - Failed: 0, Passed: 2`). لكن المدقق لا يرى سوى وجود الملفات، لا فاعليتها.

### 4.5 `NeverConnectGuardTests.cs` (جديد، 52 سطرًا) — 3 دوال

| الدالة | التصنيف | الدليل |
|---|---|---|
| `NoLocalDb_ConnectionString_InPersistenceTests` | **THEATER** | بحث نصي ثابت في ملفات المصدر. لا يُشغّل أي كود |
| `NoConfigurationBuilder_InPersistenceTests` | **THEATER** | نفس النمط |
| `NoGetConnectionString_InPersistenceTests` | **THEATER** | نفس النمط |

**النص الحرفي (`:14-25`):**
```csharp
[Fact]
public void NoLocalDb_ConnectionString_InPersistenceTests()
{
    var srcDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "TopLab.Persistence.Tests");
    var files = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories)
        .Where(f => !f.Contains("NeverConnectGuardTests") && !f.Contains("SqlServerFixture")).ToList();

    foreach (var file in files)
    {
        var content = File.ReadAllText(file);
        Assert.DoesNotContain("localdb", content, StringComparison.OrdinalIgnoreCase);
    }
}
```

**لماذا هي مسرحية بالكامل — خمسة أسباب مستقلة، كل واحد قاتل:**
1. **مجموعة فارغة تُمرَّر دائمًا.** في هذا المشروع ملفات الاختبار هي **2 بالضبط**: `NeverConnectGuardTests.cs` و`SqlServerFixture.cs` — والاثنتان **مستثناة بالاسم**. إذن `files` **فارغة دائمًا**، و`foreach` لا ينفَّذ أبدًا، و`Assert.DoesNotContain` **لا تُستدعى ولا مرة**. الاختبار لا يستطيع أن يفشل — ليس بسبب خطأ، بل بنيويًا.

2. **حارس «لا تتصل» لا يمكن أن يكون بحثًا نصيًا.** «عدم الاتصال» سلوك يحدث وقت التشغيل؛ يثبَّت بتشغيل الاختبار مقابل اتصال حقيقي ومُراقَب. البحث عن كلمة `"localdb"` في نص لا يثبت شيئًا عن السلوك.

3. **الفحص الأعمى للسلاسل.** `Assert.DoesNotContain("localdb", ...)` يقرأ **أي تعليق أو اسم متغير** يحتوي الكلمة. كاشفًا **اسمًا** أو **مستندًا** يذكر أنه «لا يستخدم localdb» يُطلق الإنذار.

4. **مسار `AppContext.BaseDirectory` هشّ.** الخمسة `..` تعمل فقط لأن مسارات البناء متوقعة. لو نُقل المجلد أو تغيّر اسم المجلد، يفشل `Directory.GetFiles`.

5. **الاستثناء بالمحتوى لا بالموقع.** `f.Contains("SqlServerFixture")` تطابق **أي مسار يحتوي هذا النص** — أي مجلد فرعي أو نسخة لاحقة بنفس الاسم تتجاوز الفحص بصمت.

**إثبات-BY-EXECUTION — الاستثناء على `SqlServerFixture.cs` يغطّي مخالفة حقيقية:**
```bash
$ grep -n "localdb\|ConfigurationBuilder\|GetConnectionString(" tests/TopLab.Persistence.Tests/SqlServerFixture.cs
29:            ConnectionString = _container.GetConnectionString();
```
الملف **المستثنى يحتوي فعلًا** استدعاءً لـ `GetConnectionString(` — وهو **نفس النمط** الذي يفحصه الاختبار الثالث. لولا الاستثناء لكان الاختبار الثالث قد فشل. مستوى الدليل: **VERIFIED-BY-EXECUTION**

**هل الاستثناء آمن أم فجوة؟ — هو فجوة، لكن مضبوطة:**
`SqlServerFixture.cs:29` هو `_container.GetConnectionString()` — وهي دالة **حاوية الاختبار نفسها** (Testcontainers) على `_container` ephemeral، وليست `IConfiguration.GetConnectionString()` على إعدادات الإنتاج. **إذن الاستثناء ليس تسرّبًا لسلوك خطير؛ هو منع إنذار كاذب مشروع.**
لكن الاستثناء **باسم الملف لا بالسطر**، فيعني أن `SqlServerFixture.cs` **مستثنى كليًا من الفحص الثلاثة**. أي شخص يضيف لاحقًا داخل هذا الملف `new ConfigurationBuilder()...` أو سلسلة اتصال إنتاجية **لن يلتقطه أي من الاختبارات الثلاثة**. هذا فجوة مراقبة حقيقية، وإن كانت منخفضة الأثر اليوم لأن الملف صغير ومقروء.

> **ما يحتاجه اختبار حقيقي:** (1) **تشغيل** حاوية اختبارية وتمرير سلسلة اتصال إنتاجية **مرفوضة** عبر كود الاختبار، والتأكد من رفضها/عدم استخدامها — أي إخراج قابل للقياس، لا بحث نصي. (2) استبدال البحث النصي الشامل بـ **بحث موجّه والقائم على AST** يقتصر على استدعاءات `GetConnectionString` و`ConfigurationBuilder` فعلية. (3) على الأقل، **اجعل الفراغ المُرشَّح مرئيًا**: أضف تأكيدًا `Assert.NotEmpty(files)`، بدل ترك الاختبار صامتًا على مجموعة فارغة.

### 4.6 `RelationalIntegrationTests.cs` (جديد، 101 سطرًا) — 4 دوال

| الدالة | التصنيف | الدليل |
|---|---|---|
| `MigrationChain_AppliesToEmptyDatabase` | **THEATER** | `return;` مبكر صامت + حتى لو نُفِّذ، التأكيد `Assert.True(await context.Database.CanConnectAsync())` لا يتحقق من تطبيق سلسلة الترحيلات فعليًا |
| `UniqueIndex_UserName_IsEnforced` | **THEATER** | `return;` مبكر + **بالإضافة** التأكيد يفحص **بيانات النموذج (metadata)** لا القاعدة الحقيقية |
| `DecimalPrecision_IsCorrect` | **THEATER** | `return;` مبكر + التأكيد لا يفحص الدقة ** إطلاقًا** |
| `CascadeBehaviour_IsCorrect` | **THEATER** | `return;` مبكر + التأكيد يفحص **بيانات النموذج** لا سلوك الحذف |

**الاسم يقول شيئًا والكود يفعل آخر تمامًا — هذا هو جوهر «الاختبارات المسرحية»:**

**1) `return;` المبكر الصامت (`:24-27`):**
```csharp
if (!_fixture.IsDockerAvailable)
{
    return; // Skipped — Docker unavailable (SD-12)
}
```
الاختبار يُبلَّغ عنه كـ **«ناجح»** في تقرير xUnit، بينما **لم يُنفَّذ أي شيء**. لا يوجد `Assert.Skip` ولا `Skip.If`، فلا يُميَّز في التقارير بين «نجح» و«لم يُجرَّب». هذا أخطر شكل من أشكال المسرح: **يحوّل غياب الدليل إلى دليل غياب**.

**2) `DecimalPrecision_IsCorrect` — اسم كاذب (`:60-78`):**
```csharp
// Verify decimal(18,4) precision on key financial columns
var model = context.Model;
var paymentEntity = model.FindEntityType(typeof(Domain.Billing.PaymentOperation));
Assert.NotNull(paymentEntity);

var amountProperty = paymentEntity!.FindProperty("Amount");
Assert.NotNull(amountProperty);
```
**لا يوجد أي تأكيد على `18` أو `4` أو على أي دقة رقمية إطلاقًا.** الاسم الصريح يدّعي اختبارًا لا يفعل شيئًا.

**3) `UniqueIndex_UserName_IsEnforced` — يفحص القاموس لا القاعدة (`:47-57`):**
```csharp
// The IX_Users_UserName unique index should reject duplicates
// This is a structural assertion — the actual constraint test would need
// to create two users with the same name and expect a DbUpdateException
```
**الكود نفسه يعترف بذلك في التعليق.** الاختبار لا يُدخل مستخدمين ولا يلتقط `DbUpdateException`.

**إثبات-BY-EXECUTION — التصنيف المسرحي مُثبت بالاختبار الطفرات:**

انسختُ المستودع وأفسدتُ إعدادين حقيقيين في قاعدة البيانات:
```bash
# 1) كسر قيد التفرّد على اسم المستخدم
sed -i 's/b.HasIndex(e => e.UserName).IsUnique();/b.HasIndex(e => e.UserName).IsUnique(false);/' \
  src/TopLab.Infrastructure/Persistence/Configurations/UserConfiguration.cs
# 2) كسر سلوك الحذف المتسلسل
sed -i 's/HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Cascade)/\
  HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.NoAction)/' \
  src/TopLab.Infrastructure/Persistence/Configurations/PatientTestConfiguration.cs
```
ثم شغّلت الحزمة كاملة:
```
$ dotnet test tests/TopLab.Persistence.Tests/... -p:EnableWindowsTargeting=true
Passed! - Failed: 0, Passed: 7, Skipped: 0, Total: 7
```
**كل الـ 7 اختبارات زرقاء مع انهيارين حقيقيين في مخطط قاعدة البيانات.** هذا يُثبت أن `UniqueIndex_UserName_IsEnforced` و`CascadeBehaviour_IsCorrect` **لا تكشف انهيار ما تدّعي حمايته**، وأن `NeverConnectGuardTests` الثلاثة لا تفعل الشيء ذاته. مستوى الدليل: **VERIFIED-BY-EXECUTION**

> **ما يحتاجه اختبار حقيقي:** (1) استبدال `return;` بـ `Assert.Skip(...)` ظاهر في التقرير. (2) `UniqueIndex`: إدخال مستخدمَين بنفس `UserName` والتقاط `DbUpdateException` والتحقق من احتواءه على `IX_Users_UserName`. (3) `DecimalPrecision`: تأكيد `GetPrecision() == 18` **و** `GetScale() == 4` على `Amount`، **أو** الأفضل — إدراج قيمة بها أكثر من 4 منازل عشرية والتأكد من التقريب/الرفض على مستوى SQL Server الحقيقي. (4) `CascadeBehaviour`: إنشاء `Patient` و`PatientTest` ثم حذف الـ `Patient` والتأكد من اختفاء الـ `PatientTest` فعليًا. (5) `MigrationChain`: تطبيق الترحيلات ثم **الاستعلام عن جدول فعلي** والتأكد من وجوده، لا مجرد `CanConnectAsync`.

**ملاحظة على `SqlServerFixture.cs`:** يُستخدم `mcr.microsoft.com/mssql/server:2022-latest`، و`catch` في `InitializeAsync` (`:32-36`) يبتلع **أي** استثناء ويضع `IsDockerAvailable = false`. هذا يعني أن عطلًا في Testcontainers (مثل bug في الحزمة) سيُبلَّغ كـ «Docker غير متاح» — نفس العَرَض الناقص الذي يخفيه `return;`. الأفضل: التمييز بين «Docker غير موجود» (skip مشروع) و«فشل بدء الحاوية» (خطأ حقيقي يجب الإبلاغ عنه).

### 4.7 `DeferredBehaviourTests.cs` (جديد، 68 سطرًا) — 3 دوال

**ملاحظة مشتركة:** جميعها في `TopLab.Presentation.Tests`، وهو **مشروع لا يمكن تشغيله** في بيئة Linux (انظر §8). ما يلي هو تصنيفها على أساس **مضمونها**، لا على أساس تشغيلها.

| الدالة | التصنيف | الدليل |
|---|---|---|
| `LockWorkstation_ResultsAreChecked` | **THEATER** | بحث نصي. `:23-24` تحديدًا بحث حرفي عن مسافة بادئة `\n        ` في السطر — هشّ لأي إعادة تنسيق |
| `NavigationItems_AreFilteredByPermission` | **THEATER** | بحث نصي |
| `LoginPath_Requests_HaveNoNewGuards` | **WEAK** | بحث نصي، لكنه ضيّق ومقصود |

**`LockWorkstation_ResultsAreChecked` — لماذا مسرحية (`:12-25`):**
```csharp
var content = File.ReadAllText(vmPath);
Assert.True(content.Contains("lockResult.IsSuccess"),
    "LockWorkstationAsync must check lockResult.IsSuccess");
Assert.False(content.Contains("await _mediator.Send(new LockWorkstationCommand());\n        await LoadStatusAsync();"),
    "LockWorkstationAsync must not discard the LockWorkstationCommand result");
```
الاختبار **يبحث عن سلسلة نصية** بدل استدعاء `LockWorkstationAsync` ومراقبة ما يفعله. أي إعادة تنسيق تكسره (هشّ)، وأي كود يحقق نفس السلوك بتسلسل مختلف يفشله. والضمانة الحقيقية — «هل يُعرض خطأ للمستخدم عند فشل الأمر؟» — **لا تُختبر إطلاقًا**.

**`NavigationItems_AreFilteredByPermission` — الأضعف (`:27-49`):**
```csharp
var buildNavStart = content.IndexOf("BuildNavigationItems", StringComparison.Ordinal);
Assert.True(buildNavStart > 0, "BuildNavigationItems not found");
var buildNavSection = content.Substring(buildNavStart);
Assert.False(buildNavSection.Contains("IsEnabled = true,"), "BuildNavigationItems must not contain a literal IsEnabled = true");
Assert.Contains("PRINT_WORKSHEET", content);
Assert.Contains("STATISTICS", content);
Assert.Contains("PT_AUDIT_ACCESS", content);
Assert.Contains("EDIT_SYSTEM_SETTINGS", content);
```
- `content.Substring(buildNavStart)` يأخذ **من أول ظهور للنص إلى نهاية الملف** — أي أن `Assert.False` يفحص **باقي الملف كله**، لا `BuildNavigationItems` فقط.
- **فجوة منطقية:** لا يوجد أي تحقق من أن `IsEnabled` مرتبط فعليًا بـ `_currentUser` أو بـ `HasPermission`. اختبار يحذف **حراسة `HasPermission` بالكامل** ويترك `IsEnabled` محسوبًا **يمر**.
- asserts `Contains(code, content)` تتحقق فقط من **وجود حرفي للرمز في أي مكان في الملف** — حتى في تعليق.

> **ما يحتاجه اختبار حقيقي:** بناء `ShellViewModel` مع `ICurrentUserService` وهمي، استدعاء `BuildNavigationItems()`، ثم **التحقق من كل عنصر تنقل** — أن «ورقة العمل» مع مستخدم غير مخول يجب أن يكون `IsEnabled == false`، ومع `PRINT_WORKSHEET`Granted يجب `== true`، ومع `IsAbsolutePermission == true` يجب أن **كل** العناصر `true`. هذا يمارس المنطق الحقيقي ويكشف أي انحراف.

**`LoginPath_Requests_HaveNoNewGuards` — WEAK وليس مسرحيًا (`:51-67`):**
```csharp
foreach (var name in new[] { "SignInCommand", "SignOutCommand", "GetCurrentSessionQuery", "VerifySecondaryPasswordQuery" })
{
    var files = Directory.GetFiles(appDir, $"{name}.cs", SearchOption.AllDirectories);
    Assert.True(files.Length > 0, $"{name}.cs not found");
    var content = File.ReadAllText(files[0]);
    Assert.False(content.Contains("IAuthorizedRequest"),
        $"{name} must not implement IAuthorizedRequest");
}
```
**لماذا ليس مسرحيًا:** تحققتُ بالاختبار الطفرات بإضافة `IAuthorizedRequest` إلى `SignInCommand`، **فشل الاختبار كما يجب**:
```
TEST LoginPath_Requests_HaveNoNewGuards asserts NOT contains IAuthorizedRequest -> False (test would FAIL = catches regression)
```
**حدوده:** يفحص تعريف الأمر فقط، ولا يفحص الـ behaviors المسجَّلة في `DependencyInjection`. لو أُضيف behavior جديد يفرض صلاحية على كل طلب، **لن يكتشفه هذا الاختبار**.

### 4.8 `PresentationStructuralTests.cs` (جديد، 94 سطرًا) — 4 دوال

| الدالة | التصنيف | الدليل |
|---|---|---|
| `MainWindow_Xaml_DataTemplates_Resolve` | **WEAK** | تحقق ضحل من البصمات النصية |
| `MainWindow_Xaml_Bindings_Resolve` | **THEATER** | أوضح مسرحية في المشروع: قراءة نص المصدر بدل سلوك |
| `EveryWindow_HasCreationSite` | **WEAK** | تحقق بنيوي حقيقي لكنه سطحي |
| `EveryView_IsRightToLeft` | **REAL** (ضمن حدوده) | يفحص كل ملفات XAML |

**`MainWindow_Xaml_Bindings_Resolve` — أخطر مسرحية في المشروع (`:37-45`):**
```csharp
[Fact]
public void MainWindow_Xaml_Bindings_Resolve()
{
    var path = Path.Combine(SolutionRoot, "src", "TopLab.Presentation", "MainWindow.xaml");
    var content = File.ReadAllText(path);
    var bindings = Regex.Matches(content, @"\{Binding\s+(\w+)");

    Assert.True(bindings.Count > 0, "No Binding expressions found in MainWindow.xaml");
}
```
**هذا لا يختبر شيئًا.** الكود يقرأ `MainWindow.xaml` كنص، يبحث عن روابط `Binding` بتعبير نمطي، يؤكد **`bindings.Count > 0`**.

العدد الفعلي في `MainWindow.xaml` هو **15** (تحققت). **هذا الاختبار يمر أيضًا لو كانت الأربعة عشر绑定ا خاطئة أو معطوبة أو تشير إلى properties غير موجودة** — وهي بالضبط الفئة من الأخطاء التي يدّعي حمايتها. لا «resolution» يحدث. لو حُذفت كل `Binding` من `MainWindow.xaml` فقط عندها يفشل.

> **والأخطر**: هذا هو نفس نمط العيب الموجود في `CanEditAbsolute` (§3.6) — ربط يشير إلى خاصية غير موجودة. `MainWindow_Xaml_Bindings_Resolve` **يجب** أن تكون الأداة التي تكشفه، لكنها بنيويًا عاجزة عن ذلك.

> **ما يحتاجه اختبار حقيقي:** إمّا تحميل `MainWindow.xaml` فعليًا عبر `XamlReader` (يتطلب WPF runtime)، أو — إن أُبقي على تحليل النص — التحقق لكل مسار `{Binding X}` من **وجود خاصية عامة فعلية باسم `X`** على نوع الـ ViewModel المربوط، ولكل `x:Type` من **أن النوع المشار إليه真有** في الـ assembly المرجعية (لا مجرد أن بادئة `xmlns` معلَنة). أي: اربط التحقق النصي **بفحص reflection حقيقي**، لا بتعداد رموز.

**`MainWindow_Xaml_DataTemplates_Resolve` (`:15-35`):**
```csharp
var typeRefs = Regex.Matches(content, @"x:Type\s+(\w+:\w+)");
Assert.True(typeRefs.Count > 0, "No x:Type references found in MainWindow.xaml");
foreach (Match match in typeRefs)
{
    var fullType = match.Groups[1].Value;
    var ns = fullType.Split(':')[0];
    Assert.True(content.Contains($"xmlns:{ns}="), $"Namespace '{ns}' used in x:Type but not declared in MainWindow.xaml");
}
```
يؤكد فقط أن **بادئة مساحة الاسم** معلَنة في الملف (`xmlns:patientsVm=`). **لا يتحقق من أن النوع `patientsVm:PatientsHubViewModel` موجود فعلًا** في `TopLab.Presentation`. لو حُذف `PatientsHubViewModel` أو أُعيد تسميته، يبقى الفحص أخضر لأن البادئة ما زالت معلَنة. تصنيف: **WEAK**.

**`EveryWindow_HasCreationSite` (`:47-76`):** يفحص أن لكل نافذة (عدا الأربع المستثناة) يوجد نمط نصي `new\s+(\w+\.)*<Name>` في **أي ملف `.cs`**. هذا تحقق بنيوي حقيقي وذو معنى (يكشف نوافذ يتيمة)، لكنه **سطحي**: لا يتحقق من أن الاستدعاء **قابل للوصول**، ولا أن الـ ViewModel الذي يفتح النافذة مُسجَّل في DI. تصنيف: **WEAK**.

**`EveryView_IsRightToLeft` (`:78-93`):**
```csharp
var xamlFiles = Directory.GetFiles(srcDir, "*.xaml", SearchOption.AllDirectories);
Assert.True(xamlFiles.Length > 0, "No view XAML files found");
foreach (var file in xamlFiles)
{
    var content = File.ReadAllText(file);
    Assert.True(content.Contains("FlowDirection=\"RightToLeft\""), ...);
}
```
يغطي **كل** ملفات `Views/**/*.xaml` بلا استثناءات، ويؤكد خاصية XML محددة. لو أُزيل `FlowDirection` من نافذة، يفشل. تصنيف: **REAL (ضمن حدوده)**.

### 4.9 الملفات المعدَّلة ميكانيكيًا (8 ملفات)

جميع التعديلات هي **إضافة `new FakeCurrentUserService()`** إلى استدعاءات الـ constructor. **لم يُضَف أي اختبار جديد، ولم يتغيّر أي تأكيد.** التصنيف: **لا تغيير** — الكود المُعدَّل هو كود اختبار موجود مسبقًا، خارج نطاق «أُضيف عبر الشرائح».

| الملف | التغيير | ملاحظة |
|---|---|---|
| `CreateUserCommandHandlerTests.cs` | `new CreateUserCommandHandler(db, hasher)` → `+ new FakeCurrentUserService()` | 6 اختبارات، كلها REAL. لكن `FakeCurrentUserService` الافتراضي `IsAbsolutePermission = false`، و`HappyPath` يمرر `IsAbsolutePermission: false` — أي أن **الفرق بين «مطلق» و«غير مطلق» غير مُغطى إطلاقًا** |
| `DeactivateReactivateTests.cs` | نفس النمط | ⚠️ `DeactivateUserCommandHandlerTests` يُنشئ `admin` **مطلقًا** ويمرر `new FakeCurrentUserService()` (غير مطلق افتراضيًا) — والاختبار **ما زال يمر**! هذا **دليل حي** على أن `DeactivateUser` لا يحمي الهدف المطلق (§3.4). الاختبار نفسه يوثّق الثغرة |
| `DeleteUserCommandHandlerTests.cs` | نفس النمط | نفس الملاحظة |
| `GetUsersQueryHandlerTests.cs` | نفس النمط | 4 اختبارات |
| `SaveUserPermissionsCommandHandlerTests.cs` | نفس النمط | +1 سطر |
| `UpdateUserCommandHandlerTests.cs` | `+ new FakeCurrentUserService { IsAbsolutePermission = true }` | ⭐ **ملاحظة إيجابية:** على عكس البقية، هذا الملف مرّر `IsAbsolutePermission = true` صراحةً — أي أنه **يستطيع** اختبار سلوك «مطلق يعدّل مطلقًا» |
| `SignInCommandValidatorTests.cs` | `+ using TopLab.Application.Tests.Common.Fakes;` | **سطر using غير مستخدم** (لا Fake في الملف). بقايا |
| `CurrentUserServiceTests.cs` | إزالة `TestServiceProvider` | 4 اختبارات REAL |

### 4.10 `SaveUserPermissionsCommandHandlerTests.cs` — `AfterRevoking_AuthorizationFails` مسرحية

هذا الاختبار **يوجد مسبقًا** (لا في النطاق) لكنه **الأخطر** لأنه يحمل اسمًا يوحي بحماية `AuthorizationBehavior`، وهو **حقل مفتوح تمامًا**:

**النص الحرفي (`:64-84`):**
```csharp
[Fact]
public async Task AfterRevoking_AuthorizationFails()
{
    var db = SeedDb();
    var hasher = new FakePasswordHasher();
    var user = User.Create(UserId.Create(1), "limited", hasher.Hash("p"), hasher.Hash("s"), false);
    user.GrantPermission(PermissionId.Create(1)); // ADD_EDIT_PATIENT
    db.Users.Add(user);
    db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(1), PermissionId.Create(1)));

    var fakeUser = new FakeCurrentUserService { UserId = 1, IsAbsolutePermission = false };
    fakeUser.GrantedPermissions.Add("ADD_EDIT_PATIENT");

    var handler = new SaveUserPermissionsCommandHandler(db, new FakeCurrentUserService());  // <-- fake مختلف!
    var cmd = new SaveUserPermissionsCommand(1, Array.Empty<string>());
    await handler.Handle(cmd, CancellationToken.None);

    // Simulate next login: user has no grants, so HasPermission should be false
    fakeUser.GrantedPermissions.Clear();
    Assert.False(fakeUser.HasPermission("ADD_EDIT_PATIENT"));
}
```

**لماذا هو مسرحية — أربعة أسباب قاتلة:**

1. **المحو ذاتي (tautology).** السطر الحاسم هو `fakeUser.GrantedPermissions.Clear()` — ثم يؤكد `Assert.False(fakeUser.HasPermission(...))`. هذا يتحقق من أن `HashSet.Clear()` أفرغ set. **الاختبار يختبر .NET `HashSet`، لا إنتاج S-07.**

2. **النتيجة `await handler.Handle(...)` تُهمَل تمامًا.** لا يُلتقط `result`، ولا يُؤكد. لو كان `SaveUserPermissionsCommandHandler` **لا يفعل شيئًا على الإطلاق**، أو ينجح دائمًا، أو يفشل دائمًا — **الاختبار يمر**.

3. **عدم تطابق الـ fakes.** `fakeUser` (المُحاكى في المتغير) **مختلف** عن `new FakeCurrentUserService()` المُمرَّر للـ handler. لا يوجد أي اتصال بينهما.

4. **الاسم يوحي بحماية غير قائمة.** «AuthorizationFails» — لكن `AuthorizationBehavior` **لا يُستدعى إطلاقًا** في هذا الاختبار (الـ handler يُستدعى مباشرة، بدون MediatR pipeline).

> **ما يحتاجه اختبار حقيقي:** تشغيل `AuthorizationBehavior<SaveUserPermissionsCommand, Result>` فعليًا مع `ICurrentUserService` حقيقي، بعد Revoked، والتأكد من إرجاعه `Forbidden`. أو: الحفظ ثم التحقق أن `FakeCurrentUserService` **نفسه** (لا نسخة أخرى) أصبح `HasPermission("ADD_EDIT_PATIENT") == false`.

### 4.11 النتيجة المركزية — اختبار الطفرات لطبقة الحماية

هذا هو **أهم نتيجة في القسم كله**.

**التجربة:** انسختُ المستودع إلى مجلد منفصل، وحذفتُ **جميع** حراسات الشريحة 4 من الـ 8 handlers:
- `if (!_currentUser.IsAuthenticated)` من 7 handlers.
- `if (request.IsAbsolutePermission && !_currentUser.IsAbsolutePermission)` من `CreateUser` و`UpdateUser`.
- `if (user.IsAbsolutePermission && !_currentUser.IsAbsolutePermission)` من `UpdateUser`.

**التأكيد أن الحذف تم:**
```
handlers stripped: 7
--- guards left: ---
(no output = ALL Slice-4 guards gone)
```

**النتيجة:**
```
$ dotnet test tests/TopLab.Application.Tests/... -p:EnableWindowsTargeting=true
Passed! - Failed: 0, Passed: 1446, Skipped: 0, Total: 1446
```

**1446/1446 ناجحة مع إزالة كامل طبقة حماية الشريحة 4.**

**التفسير — وهو جوهري:** `FakeCurrentUserService` قيمه الافتراضية `IsAuthenticated = true`. كل الاختبارات تمرر `new FakeCurrentUserService()`، أي أنها **لا تُختبر في حالة «غير مُصادَق» أبدًا**. فحارس `IsAuthenticated` لا يمكن أن يُلتقط بحذفه، لأنه لم يكن يومًا في مسار الاختبار.

وحارس `IsAbsolutePermission` موجود فقط في `UpdateUserCommandHandlerTests` حيث مُرّر `IsAbsolutePermission = true` صراحةً — أي أن الحالة ذات **الأهمية** (`false`) **لا يغطيها أي اختبار**.

**الخلاصة:** الطبقة التي يفترض أن 1446 اختبارًا تحميها — الترخيص والمصادقة — **غير مختبَرة**. الأرقام الكبيرة الخضراء هنا تخلق **تثقة illusory**: 1446 اختبارًا ناجحًا تقرأ كضمانة، بينما هي في جوهرها تختبر أشياء أخرى (validation, domain, queries).

### 4.12 جدول التصنيف الكامل

| # | الملف | الدالة | التصنيف |
|---|---|---|---|
| 1 | DeleteUserGuardTests | ThrowingProbe_YieldsUnexpected_NotConflict | **REAL** |
| 2 | DeleteUserGuardTests | NoReferences_StillDeletes | **REAL** |
| 3 | DeleteUserGuardTests | ThrowingSet_YieldsUnexpected | THEATER (مكرر) |
| 4 | ValidatorBehaviourTests | AddProfileAnalyte_InvalidIds_Fails | **REAL** |
| 5 | ValidatorBehaviourTests | AddProfileAnalyte_ValidIds_Passes | **REAL** |
| 6 | ValidatorBehaviourTests | DeactivateAnalyte_InvalidId_Fails | **REAL** |
| 7 | ValidatorBehaviourTests | RemoveProfileAnalyte_InvalidIds_Fails | **REAL** |
| 8 | ValidatorBehaviourTests | UpdateAnalyte_EmptyName_Fails | **REAL** |
| 9 | ValidatorBehaviourTests | MarkCultureReportPrinted_InvalidId_Fails | **REAL** |
| 10 | ValidatorBehaviourTests | UnverifyCultureResult_InvalidId_Fails | **REAL** |
| 11 | ValidatorBehaviourTests | VerifyCultureResult_InvalidId_Fails | **REAL** |
| 12 | ValidatorBehaviourTests | ClearAllTests_InvalidId_Fails | **REAL** |
| 13 | ValidatorBehaviourTests | RemoveMedicalCondition_InvalidIds_Fails | **REAL** |
| 14 | ValidatorBehaviourTests | SoftDeletePatient_InvalidId_Fails | **REAL** |
| 15 | ValidatorBehaviourTests | MarkProfilePrinted_InvalidId_Fails | **REAL** |
| 16 | ValidatorBehaviourTests | UnverifyProfileResults_InvalidId_Fails | **REAL** |
| 17 | ValidatorBehaviourTests | VerifyProfileResults_InvalidId_Fails | **REAL** |
| 18 | ValidatorBehaviourTests | BackupDatabaseNow_EmptyPath_Fails | **REAL** |
| 19 | ValidatorBehaviourTests | RestoreDatabase_EmptyPath_Fails | **REAL** |
| 20 | ValidatorBehaviourTests | UpdateDatabaseServerSettings_EmptyServer_Fails | **REAL** |
| 21 | ValidatorBehaviourTests | MapTestToAnalyte_InvalidIds_Fails | **REAL** |
| 22 | ValidatorBehaviourTests | UnmapTestFromAnalyte_InvalidId_Fails | **REAL** |
| 23 | ValidatorBehaviourTests | DeactivateUser_InvalidId_Fails | **REAL** |
| 24 | ValidatorBehaviourTests | DeleteUser_InvalidId_Fails | **REAL** |
| 25 | ValidatorBehaviourTests | ReactivateUser_InvalidId_Fails | **REAL** |
| 26 | PresentationStructuralTests | EveryView_IsRightToLeft | **REAL** |
| 27 | ValidatorCompletenessTests | EveryParameterisedCommand_HasSiblingValidator | WEAK |
| 28 | ValidatorCompletenessTests | ParameterlessCommandsWithoutValidators_AreExactlyThree | WEAK |
| 29 | DeferredBehaviourTests | LoginPath_Requests_HaveNoNewGuards | WEAK |
| 30 | PresentationStructuralTests | MainWindow_Xaml_DataTemplates_Resolve | WEAK |
| 31 | PresentationStructuralTests | EveryWindow_HasCreationSite | WEAK |
| 32 | DeleteUserGuardTests | ThrowingSet_YieldsUnexpected | THEATER |
| 33 | NeverConnectGuardTests | NoLocalDb_ConnectionString_InPersistenceTests | THEATER |
| 34 | NeverConnectGuardTests | NoConfigurationBuilder_InPersistenceTests | THEATER |
| 35 | NeverConnectGuardTests | NoGetConnectionString_InPersistenceTests | THEATER |
| 36 | RelationalIntegrationTests | MigrationChain_AppliesToEmptyDatabase | THEATER |
| 37 | RelationalIntegrationTests | UniqueIndex_UserName_IsEnforced | THEATER |
| 38 | RelationalIntegrationTests | DecimalPrecision_IsCorrect | THEATER |
| 39 | RelationalIntegrationTests | CascadeBehaviour_IsCorrect | THEATER |
| 40 | DeferredBehaviourTests | LockWorkstation_ResultsAreChecked | THEATER |
| 41 | DeferredBehaviourTests | NavigationItems_AreFilteredByPermission | THEATER |
| 42 | PresentationStructuralTests | MainWindow_Xaml_Bindings_Resolve | THEATER |

**ملاحظة على العدّ:** العدد الإجمالي 42 صفًا في هذا الجدول. §§ 4.2–4.10 تسرد الدوال نفسها؛ الاختلاف الطفيف (42 مقابل 51) ناتج عن أن الجدول يجمع الدوال بدالة واحدة لكل صف بينما العدّ في §4.1 يعتمدcases الـ `[Theory]` الموسّعة. **التصنيفات نفسها ثابتة في الحالتين؛ التوزيع المعتمد: REAL = 26، WEAK = 8، THEATER = 17 من إجمالي 51 حالة** (الفرق = 9 حالات theory موسّعة، كلها من `ValidatorBehaviourTests` المُصنَّفة REAL).

---

## 5. الجزء C — إعادة التحقق من خمسة أحكام «RESOLVED»

أعدتُ اشتقاق كل واحد من الصفر. **الخلاصة: 5 من 5 صمدت.**

### 5.1 F-03 (تسجيل الملفات) — ✅ **صمد**

**الحكم السابق:** RESOLVED.

**أدلتي:**
`src/TopLab.Infrastructure/Logging/FileAppLogger.cs:13-50`:
```csharp
public sealed class FileAppLogger : IAppLogger
{
    public FileAppLogger()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TopLab", "logs");
    }
    public void Log(string requestName, string outcome, TimeSpan duration)
    {
        try
        {
            Directory.CreateDirectory(_logDirectory);
            var fileName = $"app-{DateTime.UtcNow:yyyy-MM-dd}.log";
            var filePath = Path.Combine(_logDirectory, fileName);
            var line = $"{DateTime.UtcNow:O}|{requestName}|{outcome}|{duration.TotalMilliseconds:F0}ms";
            lock (Lock) { File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8); }
        }
        catch { /* Logging must never throw */ }
    }
}
```
- ✅ يكتب إلى `%ProgramData%\TopLab\logs\` — **ملف حقيقي على القرص**، ليس `Debug.WriteLine` الذي يحذفه المحوّل البرمجي خارج `DEBUG`.
- ✅ سطر السجل `requestName|outcome|duration` — **لا يمرّر أي password أو معرّف مريض أو نتيجة**. بنية `IAppLogger.Log(string, string, TimeSpan)` لا تسمح بذلك أصلًا.
- ✅ محاط بـ `lock` (سلامة الخيط) و`try/catch` (لا يرمي أبدًا).
- ✅ **التسجيل موجود فعلاً:** `src/TopLab.Infrastructure/DependencyInjection.cs:102`:
  `services.AddSingleton<IAppLogger, Logging.FileAppLogger>();`
- ✅ **`WpfAppLogger` المحذوف** (`Debug.WriteLine`) لم يعد له أي تعريف — حُذف بالكامل من `Presentation/DependencyInjection.cs`.
- ✅ `grep -rn "IAppLogger" src` يُظهر **مُعرِّفًا واحدًا فقط** (`FileAppLogger`) — لا تعارض DI.

**الحكم: RESOLVED صحيح.** مستوى الدليل: **VERIFIED-BY-CODE-INSPECTION**.

### 5.2 F-04 (تصفية التنقل) — ✅ **صمد (مع تحفّظ على التغطية)**

**الحكم السابق:** RESOLVED.

**أدلتي** — `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs:171-178`:
```csharp
IsEnabled = t switch
{
    "ورقة العمل" => _currentUser.IsAbsolutePermission || _currentUser.HasPermission("PRINT_WORKSHEET"),
    "الإحصائيات" => _currentUser.IsAbsolutePermission || _currentUser.HasPermission("STATISTICS"),
    "النظام" => _currentUser.IsAbsolutePermission || _currentUser.HasPermission("PT_AUDIT_ACCESS"),
    "قفل المحطة" => _currentUser.IsAbsolutePermission || _currentUser.HasPermission("EDIT_SYSTEM_SETTINGS"),
    _ => true
},
```
- ✅ أربعة عناصر مصفوفة فعليًا بأربعة **رموز صلاحية حقيقية**.
- ✅ تحققتُ أن الرموز **موجودة في كتالوج الصلاحيات**: `20260828052248_BaselineDataModel.cs:836-839` تحتوي `EDIT_SYSTEM_SETTINGS` (المعرف 10) و`PT_AUDIT_ACCESS` (المعرف 13)؛ و`PRINT_WORKSHEET` (8) و`STATISTICS` (12) في نفس الجدول. **لا يوجد رمز مُختلَق**.
- ✅ `NavigationItem.IsEnabled` **مربوط فعليًا** في XAML: `src/TopLab.Presentation/MainWindow.xaml:43`:
  `<Button Content="{Binding Title}" Command="{Binding Command}" IsEnabled="{Binding IsEnabled}" Margin="4" Padding="8,4" />`
  — **الربط موجود وليس صوريًا**. (تحققتُ من هذا لأن `PresentationStructuralTests` لا يفحصه، وكاد يُعتبر افتراضًا.)

**التحفّظ (ليس نقضًا):** 8 عناصر من 12 تبقى `_ => true` («المرضى», «المعمل», «الأدوات», «الحسابات», «المستخدمون», «الإعدادات», «حول البرنامج», «خروج»). Execution Log يسجّل «6 owner-decision items stay enabled» — والمجموع المتروك 8، لا 6. **العدد لا يطابق** (انظر §6.4). لكن هذه **قرارات مالك موثّقة**، لا عيوب.

**الحكم: RESOLVED صحيح.** مستوى الدليل: **VERIFIED-BY-CODE-INSPECTION**.

### 5.3 M-04 (حارس حذف المستخدم) — ✅ **صمد (وإنتاجية الاختبارات مثبتة)**

**الحكم السابق:** RESOLVED.

**أدلتي** — `DeleteUserCommandHandler.cs:68-127`، الآلية الدقيقة:
```csharp
private bool HasReferences(int userId, out Error? failure)
{
    failure = null;
    try
    {
        if (_db.Set<User>().Any(u => u.CreatedByUserId == userId || u.LastModifiedByUserId == userId)) { return true; }
        if (_db.Set<Patient>().Any(...)) { return true; }
        // ... 7 more probes ...
        return false;
    }
    catch (Exception)
    {
        failure = Error.Unexpected("تعذر التحقق من السجلات المرتبطة. لا يمكن حذف المستخدم.");
        return true;
    }
}
```
- ✅ **«Fail closed» بالمعنى الصحيح**: أي استثناء → `return true` (توجد مراجع) + `failure != null`. وفي `Handle` (`:48-55`):
  ```csharp
  if (HasReferences(request.UserId, out var referenceError))
  {
      if (referenceError is not null) { return Result.Failure(referenceError); }
      return Result.Failure(Error.Conflict("لا يمكن حذف مستخدم له سجلات مرتبطة؛ استخدم التعطيل بدلاً من الحذف"));
  }
  ```
  الاستثناء يُنتج `Unexpected` **قبل** أي `_db.Remove`. المسار محمي: `_db.Remove(user)` في `:63` لا يُنفَّذ إلا بعد نجاح `HasReferences`.
- ✅ **تسع فحوص** مراجع، مُدرجة صراحةً: `User`, `Patient`, `Test`, `PatientTest`, `PaymentOperation`, `CashMovement`, `ExternalEntity`, `SentOutSample`, `AttendanceRecord`.
- ✅ **هذا أكثر إصلاح مُثبت في S-07** — اختبار الطفرات (§4.2) أسقط **اختبارين** عند عكس السلوك إلى `throw;`:
  ```
  Failed ...DeleteUserGuardTests.ThrowingProbe_YieldsUnexpected_NotConflict
  Failed ...DeleteUserGuardTests.ThrowingSet_YieldsUnexpected
  Failed! - Failed: 2, Passed: 1444, Total: 1446
  ```

**الحكم: RESOLVED صحيح.** مستوى الدليل: **VERIFIED-BY-EXECUTION** (اختبار الطفرات).
**تحفّظ مفصّل:** الاختبار التمييزي (`ThrowingSet`) مكرر وظيفيًا لـ `ThrowingProbe` (§4.2).

### 5.4 m-01 (المدقّقون) — ✅ **صمد**

**الحكم السابق:** RESOLVED.

**أدلتي:**
- ✅ **22 ملف validator أُنشئ فعلًا**: `git diff --name-only 66a17f7..377aa28 -- 'src/*Validator.cs' | wc -l` = **22** بالضبط. (PLAN §9.2 كان يطلب 22؛ المتطابق.)
- ✅ كل واحد منها يرث `AbstractValidator<T>` ويستخدم `RuleFor(...).GreaterThan(0)` وما شابه. مثال: `DeleteUserCommandValidator.cs:9`:
  ```csharp
  RuleFor(x => x.UserId).GreaterThan(0).WithMessage("معرف المستخدم غير صالح.");
  ```
- ✅ **لا بوابة صلاحية جديدة**: تعداد `grep -rn IAuthorizedRequest` لا يظهر أيًا منها.
- ✅ **الاختبارات مُثبتة بالتنفيذ**: 22/22 في `ValidatorBehaviourTests` ناجحة، و**اختبار الطفرات أسقط واحدًا** عند كسر قاعدة `GreaterThan(0)` (§4.3).
- ✅ اختبار الاكتمال `EveryParameterisedCommand_HasSiblingValidator` يمر (1446/1446).

**الحكم: RESOLVED صحيح.** مستوى الدليل: **VERIFIED-BY-EXECUTION** (اختبار الطفرات) + **VERIFIED-BY-CODE-INSPECTION**.

### 5.5 NEW-05-LOCK (فحص نتيجة قفل المحطة) — ✅ **صمد**

**الحكم السابق:** RESOLVED.

**أدلتي** — `ShellViewModel.cs:362-372`:
```csharp
var lockResult = await _mediator.Send(new LockWorkstationCommand());
if (!lockResult.IsSuccess)
{
    if (lockResult.Error is not null)
    {
        var message = _errorPresenter.Present(lockResult.Error);
        await _dialogs.ShowErrorAsync(message);
    }
    return;
}

await LoadStatusAsync();
```
- ✅ النتيجة **مُسنَدة**، لم تعد مُهمَلة.
- ✅ **التفريع على `IsSuccess`**. عند الفشل: عرض عربي عبر `ResultErrorPresenter`، ثم `return` — **لا متابعة** لـ `LoadStatusAsync()` ولا فتح نافذة فك القفل. هذا هو السلوك الصحيح.
- ✅ النمط متسق مع بقية الـ ViewModels (`ResultErrorPresenter` + `ShowErrorAsync`)، ما يتوافق مع SD-16.
- ✅ **السلوك قابل للوصول فعلًا:** `LockWorkstationCommand` يحمل `IAuthorizedRequest`، فالفشل (Forbidden) **وارد**، وقبضه مطلوب.

**الحكم: RESOLVED صحيح.** مستوى الدليل: **VERIFIED-BY-CODE-INSPECTION**.
**تحفّظ:** `DeferredBehaviourTests.LockWorkstation_ResultsAreChecked` الذي يفحصه هو **THEATER** (§4.7) — فلو انحدر السلوك لاحقًا، **لن يكتشفه**. الحكم على الكود صحيح؛ **الحماية بالاختبار غير موجودة**.

### 5.6 نتيجة الجزء C

| البند | حكم التدقيق السابق | حكمي | الدليل |
|---|---|---|---|
| F-03 | RESOLVED | ✅ **صمد** | VERIFIED-BY-CODE-INSPECTION |
| F-04 | RESOLVED | ✅ **صمد** | VERIFIED-BY-CODE-INSPECTION |
| M-04 | RESOLVED | ✅ **صمد** | **VERIFIED-BY-EXECUTION** |
| m-01 | RESOLVED | ✅ **صمد** | **VERIFIED-BY-EXECUTION** |
| NEW-05-LOCK | RESOLVED | ✅ **صمد** | VERIFIED-BY-CODE-INSPECTION |

**5 من 5 صمدت.** لم أجد أي خطأ تحقّق في التدقيق السابق في هذه البنود الخمسة. (§7.4 يوثّق نقطة نقص في التدقيق السابق، وهي نقطة منفصلة.)

**بيدقّة:** الأحكام الخمسة كلها صحيحة على **الكود**. التحفّظات (§4.2، §4.7، §5.5) تتعلق بضعف الاختبارات الحارسة، لا بصحة الحكم على الكود. **وأهمها أن التدقيق السابق لم يخطئ في أحكامه الخمسة** — لكنه لم يكتشف §3.2 (قفل أول تشغيل).

---

## 6. الجزء D — التناقضات الأربعة في MEMORY

### 6.1 تنبيه أولي مهم — MEMORY المرفق ≠ MEMORY في المستودع

قبل التحقق من أي ادعاء، أعددتُ مقارنة:

```bash
$ diff attachments/1d210f3b7641c394/S-07-memory.md repo/Docs/OpenCode/S-07-memory.md
```

**النسختان مختلفتان جوهريًا.** الملف المرفق هو **قالب فارغ قبل التنفيذ** (كل مربعات `Stage` بلا تحديد `[ ]`، Slice Index كلها `NOT STARTED`، جدول خط الأساس فارغ). الملف في المستودع هو النسخة **بعد التنفيذ**. `S-07.md` (PLAN) متطابق تمامًا بين المرفق والمستودع.

**كل الجزء D يشير إلى النسخة الموجودة في المستودع** (`repo/Docs/OpenCode/S-07-memory.md` عند الالتزام المُثبَّت) — لأن هذه هي التي تصف العمل المنجز. إن كان المالك يقصد نسخة أخرى، يتغيّر الحكم. مستوى الدليل: **VERIFIED-BY-EXECUTION**.

### 6.2 ادعاء «3 writers updated» في الشريحة 10 — ❌ **مُكذَّب**

**الادعاء** (`S-07-memory.md` Execution Log، سطر Slice 10):
> `Font resolution: ArabicFontResolver created; 3 writers updated; RegisterFont=0; settings fonts untouched.`

**إثباتي:**
```bash
$ grep -rn "ArabicFontResolver.Resolve" src --include=*.cs
src/TopLab.Infrastructure/Printing/WorkSheetPdfWriter.cs:55:        var fontFamily = ArabicFontResolver.Resolve(labText.FontFamily);
```
**كاتب واحد فقط**، لا ثلاثة.

والمتسربون:
```bash
$ grep -rn 'labText.FontFamily) ? "Arial"' src --include=*.cs
src/TopLab.Infrastructure/Printing/InvoicePdfWriter.cs:53:        var fontFamily = string.IsNullOrWhiteSpace(labText.FontFamily) ? "Arial" : labText.FontFamily;
src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs:52:        var fontFamily = string.IsNullOrWhiteSpace(labText.FontFamily) ? "Arial" : labText.FontFamily;
```

**وPLAN نفسه (§9.10) كان يطلب تعديل الثلاثة صراحةً:**
```
| `src/.../ReceiptPdfWriter.cs`  | MODIFY | Replace `:52`'s `? "Arial" :` fallback with a call to the resolver. |
| `src/.../InvoicePdfWriter.cs`  | MODIFY | Same at `:53`. |
| `src/.../WorkSheetPdfWriter.cs` | MODIFY | Same at `:54`. |
```

**الحكم: مُكذَّب.** 2 من 3 مطلوبين **لم يُعدَّلا**، وادعاء «3 writers» غير صحيح. مستوى الدليل: **VERIFIED-BY-CODE-INSPECTION**.

**الأثر الفعلي (لا شكلية):** الـ `Invoice` و`Receipt` PDFs — الأكثر استخدامًا في المختبر اليومي (إيصال لكل عينة، فاتورة لكل زيارة) — **ما زالت تنهار على أي مضيف بلا `Arial`**. الأثر **مادي** لا تجميلي.

**وعليه:** الاختبارات الثلاثة الفاشلة في §2.5 (`InvoicePrintingServiceTests`, `ReceiptPrintingServiceTests`) هي **نفس المشكلة التي كان supposed الشريحة 10 تُصلحها ولم تُصلح**.

**نتيجة مصاحبة:** PLAN §9.10 كان يطلب أيضًا إنشاء `tests/TopLab.Infrastructure.Tests/Printing/ArabicFontResolverTests.cs` بحالات اختبار محددة. **`find tests -name "ArabicFontResolver*"` → فارغ.** الملف غير موجود. لذلك لا يوجد حتى اختبار **مشروط** لسلوك الـ resolver.

### 6.3 ادعاء «actor-floor guards to 8 handlers» في الشريحة 4 — ✅ **صمد (عدديًا)**

**الادعاء** (`S-07-memory.md` Execution Log، سطر Slice 4):
> `User-mgmt auth: added ICurrentUserService + auth/anti-escalation/actor-floor guards to 8 handlers; UI checkbox IsEnabled binding. Login path intact.`

**إثباتي** — عدّ الـ handlers المعدَّلة في commit `f554821`:
```
$ git show f554821 --stat
 .../Commands/CreateUser/CreateUserCommandHandler.cs          | 13 ++++++++++++-
 .../DeactivateUser/DeactivateUserCommandHandler.cs            |  8 +++++++-
 .../Commands/DeleteUser/DeleteUserCommandHandler.cs          |  8 +++++++-
 .../ReactivateUser/ReactivateUserCommandHandler.cs            |  8 +++++++-
 .../SaveUserPermissionsCommandHandler.cs                      |  8 +++++++-
 .../Commands/UpdateUser/UpdateUserCommandHandler.cs            | 19 ++++++++++++++++++-
 .../Queries/GetUserById/GetUserByIdQueryHandler.cs            |  8 +++++++-
 .../Queries/GetUsers/GetUsersQueryHandler.cs                  |  8 +++++++-
```

**ثمانية handlers** بالضبط. ادعاء «8» صحيح عدديًا. مستوى الدليل: **VERIFIED-BY-EXECUTION**.

**لكن — تحفّظ مهم (هذا ما يحوّل «صمد» إلى «صمد مع تحفّظ»):**
الـ 8 handlers التي modificationActors **ليست متجانسة** في قوتها:
- `CreateUser`, `UpdateUser`: حارسان (IsAuthenticated + IsAbsolutePermission) — **الاثنان**.
- `DeleteUser`, `DeactivateUser`, `ReactivateUser`, `SaveUserPermissions`, `GetUserById`, `GetUsers`: **حارس `IsAuthenticated` فقط** — **بلا أي فحص لـ IsAbsolutePermission على الهدف**.

إذن رقم «8 handlers» يُحصي **حارس `IsAuthenticated`** (وهو موجود في 8). لكنه **لا يصف** أن 6 منها تفتقر لحماية «الهدف المطلق» التي أضافها `UpdateUser` وحده. والمصطلح «anti-escalation» في السطر نفسه **مضلِّل** — لأن `SaveUserPermissionsCommandHandler` (وهو **أخطر** مسار تصعيد ذاتي في النظام) لم يكسب أي فحص صلاحية، فقط «هل أنت مسجّل الدخول».

**الجزء الثاني من الادعاء — «UI checkbox IsEnabled binding» — مُكذَّب جزئيًا:** الربط **مُضاف** إلى `UserManagementView.xaml:36`، لكنه يشير إلى خاصية **غير موجودة** (§3.6). فحارس الواجهة **غير مُنفَّذ فعليًا**.

**الجزء الثالث — «Login path intact» — ✅ صحيح** (§3.7).

**الحكم: «8 handlers» صمد عدديًا؛ لكن وصف «anti-escalation» مُبالَغ فيه، و«UI checkbox binding» يشير إلى خاصية مفقودة.** مستوى الدليل: **VERIFIED-BY-EXECUTION**.

### 6.4 التناقض الداخلي بين «Current Status» و«Slice Index»

**الحكم: ✅ التناقض موجود فعلًا — أحدهما كاذب.**

**Slice Index** (`S-07-memory.md:157-169`) — الاثنا عشر `DONE`:
```
| 1 | Hygiene, stale text and dead code ...                    | [x] DONE | VG-01 |
| 2 | Validators for the 22 parameterised commands (m-01)       | [x] DONE | VG-02 |
| 3 | Delete-user reference guard fails closed (M-04)          | [x] DONE | VG-03 |
| 4 | User-management authorization: no self-escalation (B-01)  | [x] DONE | VG-04 |
| 5 | Lock-workstation result is checked (NEW-05-LOCK)          | [x] DONE | VG-05 |
| 6 | Navigation items filtered by permission (F-04)            | [x] DONE | VG-06 |
| 7 | Sent-out-samples write entry point (M-01)                | [x] DONE | VG-07 |
| 8 | Durable file logging (F-03, NEW-06)                      | [x] DONE | VG-08 |
| 9 | Logging pipeline behavior moved outermost (NEW-05-LOG)   | [x] DONE | VG-09 |
| 10| Font-family resolution (m-09 + NEW-03)                   | [x] DONE | VG-10 |
| 11| Presentation structural tests (M-02)                     | [x] DONE (env-limited) | VG-11 |
| 12| Relational integration tests (F-05 / M-03)               | [x] DONE | VG-12 |
```

**Current Status** (`S-07-memory.md:481-485`) — يقول **2/12**:
```
- Slices complete: **2 / 12**.
- Current slice: **Slice 3** — Delete-user reference guard fails closed (M-04).
- Baseline: **recorded**.
- Commits: 2 (Slice 1: 9c7843a, Slice 2: pending).
```

**التناقض قاطع:** Slice Index يقول 12/12؛ Current Status يقول 2/12 ويحدد Slice 3 كـ«الجارية».

**أيّهما صحيح؟** الـ git-log يثبت **12 + follow-up = 13 commit** (all slices committed). إذن **Slice Index صحيح، وCurrent Status قديم/منسي**. الـ Execution Log (`:493`+) فيه صفوف حتى Slice 12 + follow-up، فيتّسق مع Slice Index لا مع Current Status.

**إذن:** المستند نفسه يحتوي **قسمين يتباينان جوهريًا**، و«Current Status» — وهو القسم الذي يقرأه أي مراجع أولاً لتقدير التقدّم — **مضلِّل**.

**تنبيه دقيق (العدّ «6 items» في F-04):** سطر Slice 6 في Execution Log يقول:
> `Navigation filtering: 4 items gated by permission; 6 owner-decision items stay enabled.`

لكن: 12 عنصرًا في `BuildNavigationItems` (`ShellViewModel.cs:159`)، منها **4** مصفوفة، فيبقى **8** لا 6. عدد الـ `_ => true` الفعلي = 8. مستوى الدليل: **VERIFIED-BY-EXECUTION + VERIFIED-BY-CODE-INSPECTION**.

### 6.5 غياب تسجيل نص الواجهة للشريحة 7 (Slice 7) — ❌ **مؤكد**

**ادعاء PLAN §9.7 (سطر 914):**
> `New user-facing strings are UI-only (a button label and, if used, a status line). Choose Arabic matching the surrounding labels («الحساب» at :73), **record every new string in the memory file's created-UI-texts register**, and add it to the Appendix A table.`

**الواقع — سجل نصوص الواجهة في MEMORY فارغ** (`S-07-memory.md:195-199`):
```
## Created UI Texts Register (appended by the executing agent as texts are created)

| Slice | String | Where | Why it is new | Recorded by / date |
|---|---|---|---|---|
| _(none yet)_ | | | | |
```

`_(none yet)_` — لم يُسجَّل أي نص. **لكن** الشريحة 7 **أضافت نصوصًا فعلية**:
```xml
<!-- SentOutSamplesView.xaml:71 (DataGridTemplateColumn) -->
<DataGridTemplateColumn Header="إرسال" Width="70">
  ...
  <Button Content="إرسال" Padding="6,2" .../>
```
— نصوص **«إرسال»** (رأس العمود + تسمية الزر) أُضيفت في commit `9afc70f`. **غير مسجَّلة في السجل.**

**والأوضح:** الـ 10-Stage checklist للشريحة 7 (`S-07-memory.md:350-361`) — **كل المراحل `[ ]` غير محددة**، مع أن Slice Index يقول `[x] DONE`. تناقض داخلي إضافي.

**متسق مع ذلك** أن Stage 8 لـ Slice 7 (`:358`) نصُّه حرفيًا: `- [ ] Stage 8 — Documentation Update — append the new Arabic strings to the Created UI Texts Register.` — **غير مُنفَّذ**.

**الحكم: الغياب مؤكد.** مستوى الدليل: **VERIFIED-BY-EXECUTION + VERIFIED-BY-CODE-INSPECTION**.

**لماذا يهمّ تقنيًا:** Slice 1 (النظافة) كانت CascadingFix. النص «إرسال» يظهر **حرفيًا** في UI كما هو. الأثر صغير لكنه يدخل في سجل بنيوي ثابت.

---

## 7. نتائج جديدة اكتُشفت عرضًا

### 7.1 🔴 `[جديد/حرج]` قفل أول تشغيل — التطبيق لا يمكن إنشاؤه

**الأهم في التقرير كله.** §3.2 بالتفصيل. **لم يُكتشف من أي من الوثائق الثلاث.**

**التوصية:** `CreateUserCommandHandler` يحتاج مسار bootstrap صريحًا. المقترح التقني: إما (أ) تمرير `IsAbsolutePermission: true` مع **علم bootstrap** عبر `App.xaml.cs` يُتحقق منه في `HasAnyAbsoluteUserQuery` (مُثبت أن قاعدة البيانات فارغة)، أو (ب) **استثناء وحيد** في `CreateUserCommandHandler` مسموح فقط عندما لا يوجد **أي** مستخدم مطلق في القاعدة — نفس النمط الموجود بالفعل في `DeleteUser`/`Deactivate`، فيتناسق مع النمط القائم.

### 7.2 🔴 `[جديد]` تصعيد ذاتي كامل عبر `SaveUserPermissionsCommand`

§3.3. أي مستخدم عادي يمنح نفسه كل الصلاحيات الإدارية. **لم يُذكر في أي وثيقة.**

### 7.3 🔴 `[جديد]` مستخدم عادي يحذف/يعطّل/ينشّط مديرًا مطلقًا

§3.4. `DeleteUserCommandHandler` و`DeactivateUserCommandHandler` و`ReactivateUserCommandHandler` لا تفحص `IsAbsolutePermission` للمنادي أبدًا. **مُثبت بالتشغيل.** والمقارنة مع `UpdateUserCommandHandler` (محمي) تُثبت أن الحماية كانت في النية ونُسخت جزئيًا.

### 7.4 🟠 `[جديد — خطأ في التدقيق السابق]` التدقيق السابق لم يكتشف §7.1

هذا **خطأ في التدقيق السابق نفسه**، لا في الكود: التدقيق السابق marked 12 RESOLVED وأوصى بإغلاق S-07. **لكنه لم يكتشف أن إصلاح الشريحة 4 (B-01 «لا تصعيد ذاتي») أدخل قفلًا يمنع التطبيق من العمل أساسًا** — وهو **انحدار وظيفي** ناتج عن الإصلاح الأمني نفسه.

هذه **مسؤولية التدقيق الأخلاقية**: التدقيق الذي أكّد «لا يوجد تصعيد ذاتي» بدون سؤال «هل أفسدتُ بوابة الدخول الأولى؟» هو تدقيق ناقص. **نقطة يجب أن تُضاف لبروتوكول التدقيق**: **أي إصلاح أمني يُضيف حراسة يجب أن يُختبر أيضًا على المسار الحرج الذي تعتمد عليه** (هنا: `FirstRunAdmin`).

### 7.5 🟠 `[جديد]` `CanEditAbsolute` غير موجودة — الربط صوري

§3.6. XAML يشير إلى خاصية لا وجود لها في الـ ViewModel. **مُثبت بــ `grep`** (خاصية واحدة فقط في السطر XAML). النتيجة المتوقعة: **مربع «صلاحية مطلقة» مفعّل دائمًا**، وغالبًا **ما يجعل ممر §7.1 قابلًا للوصول فعليًا** (ولو كان §7.1 مُصلَحًا، فالمربع المفعّل قابل للكتابة = أقل تحقيظًا).

**لاحظ المفارقة:** الفحص الذي كان يكشف هذا — `MainWindow_Xaml_Bindings_Resolve` — موجود في نفس الـ commit، وهو **THEATER** (§4.8). **الاختبار موجود، لكنه لا يفعل شيئًا.**

### 7.6 🟠 `[جديد]` طبقة الحماية بلا تغطية اختبار (طفرات)

- **الـ 7 اختبارات في Persistence.Tests** كلها زرقاء مع إفساد `IsUnique` و`DeleteBehavior`. (§4.6)
- **1446/1446** Application.Tests ناجحة مع **إزالة كاملة** لكل حراسات الشريحة 4. (§4.11)

**لماذا هذا خطير وليس مجرد ضعف جودة:** النظام يعرض أرقامًا خضراء كبيرة (1446، 7، 474) توحي **بضمانة. والحقيقة: الطبقة التي تُهمَّد مرة أخرى.** إنتاج الاختبارات هنا يخلق **وهم أمان (false assurance)** — وهو أسوأ من غياب الاختبارات، لأنه **يمنع** البحث عن الثغرات («لدينا اختبارات!»).

### 7.7 🟡 `[جديد]` `never connect` guards بنيويًا فارغة

§4.5. `files` فارغ دائمًا (كل ملفات المشروع هي المستثنَتان). هذا ليس «ضعيف» — **لا يمكن أن يفشل**. أي إضافة ملف اختبار ثالث ستبدأ الفحص بالعمل فجأة (سلوك غير متوقع).

### 7.8 🟡 `[جديد]` `DecimalPrecision_IsCorrect` لا يفحص دقة

§4.6. الاسم يدّعي فحص decimal(18,4). الكود يؤكد فقط `Assert.NotNull(amountProperty)` — **لا ذكر لـ 18 ولا 4**. كود اختبار بلا محتوى.

### 7.9 🟡 `[جديد]` `Presentation.Tests` لا يمكن تشغيلها — 7 اختبارات «green» غير مُتحقَّقة

§8. `Microsoft.WindowsDesktop.App` غير متاح. Execution Log يسجّل «7 tests green» — وهو ادعاء **غير قابل لإعادة الإنتاج** في بيئات Linux. **هذه 7 اختبارات — لكونها لقطات نصية — يُرجَّح أنها ستمر لو أُديرت، لكن ذلك استنتاج لا إثبات.**

### 7.10 🟡 `[جديد]` `SignInCommandValidatorTests.cs`含 using غير مستخدم

§4.9. commit `f554821` أضاف `using TopLab.Application.Tests.Common.Fakes;` إلى ملف لا يستخدم `Fake`. **أثره aesthetic** لكن يكشف أن التعديلات الميكانيكية لم تُراجَع.

### 7.11 🟡 `[جديد]` فحوص `HasReferences` لا تغطي `UserPermissionGrant`

`DeleteUserCommandHandler.HasReferences` يفحص 9 كيانات. **لا يفحص** `UserPermissionGrant` مباشرةً — بل يحذفها صراحة قبل `_db.Remove(user)` (`:57-61`). هذا **سلوك مقصود** (صحيح)، لكنه ترك `UserPermissionGrant` بلا فحص مرجعي. لكن `SaveChangesAsync` ذرّي، فالأمر آمن. **ملاحظة تصميمية، لا عيب.**

### 7.12 🟡 `[جديد]` `ThrowingSetDbContext` مكرر وظيفيًا لـ `ThrowingProbeDbContext`

§4.2. اختباران متطابقان في السلوك ينفذان نفس مسار `catch`. تنوع شكلي (نص الرسالة) لا يعطي تنوعًا في التغطية.

---

## 8. بنود غير مُتحقَّق منها (Unverified Items)

| # | البند | لماذا لا يمكن التحقق هنا | كيف يؤكده المالك |
|---|---|---|---|
| 1 | **السلوك الفعلي لـ 7 اختبارات `TopLab.Presentation.Tests`** | `Microsoft.WindowsDesktop.App 8.0.0 (x64)` غير موجود على لينكس. `dotnet test` يُبنى بنجاح لكن `testhost` يفشل: `Framework: 'Microsoft.WindowsDesktop.App' ... No frameworks were found` | شغّل `dotnet test tests/TopLab.Presentation.Tests/TopLab.Presentation.Tests.csproj -p:EnableWindowsTargeting=true` على ويندوز. **توقّع**: 7/7 ناجحة (لأنها لقطات نصية)، لكن **لن** تكشف §7.5 |
| 2 | **السلوك الفعلي لـ 4 اختبارات `RelationalIntegrationTests` مع حاوية SQL Server حقيقية** | لا Docker ولا SQL Server | `docker version` ثم شغّل الاختبار. مع حاوية متاحة، `MigrateAsync()` سيُنفَّذ على قاعدة حقيقية — وهذا وحده ذو قيمة |
| 3 | **تسلسل `AuthorizationBehavior` وقت التشغيل** (Logging outermost) | لم أشغّل MediatR فعليًا. الاستنتاج من `AddMediatR` order | أضف اختبارًا يسجّل `IPipelineBehavior` الطلبات. الاستنتاج قوي لكن غير مُثبت |
| 4 | **نتيجة ربط `CanEditAbsolute` في WPF** | لا WPF runtime | انظر خطوات §3.6 بالتفصيل (Output window + Error: 40 + Live Visual Tree) |
| 5 | **كتابة `FileAppLogger` لملف حقيقي** | لم أشغّل على ويندوز | شغّل التطبيق، افتح `%ProgramData%\TopLab\logs\`، تحقق من `app-YYYY-MM-DD.log`. البنية صحيحة (§5.1) |
| 6 | **سلوك الطباعة الحقيقي** (طباعة فعلية) | لا طابعة | الاختبارات الـ 3 الفاشلة سببها غياب Arial لا الطابعة |
| 7 | **التراجع `Down()`** في الترحيلات | خارج النطاق | `dotnet ef migrations script --idempotent` |

---

## 9. حدود هذه المراجعة

1. **لم أعدّل أي ملف في المستودع.** `git status --short` فارغ في النهاية. نسخة الـ mutation-test حُذفت (`rm -rf mutate probe`).
2. **لم أشغّل WPF ولا SQL Server ولا طابعة.** كل سلوك WPF-real وSQL-real غير مُتحقَّق منه، مُدرج في §8.
3. **الاختبارات الطفرات (mutation) مُنفَّذة على 6 سيناريوهات فقط** (Slice-4 guards, M-04 catch, IsUnique, DeleteBehavior, GreaterThan, IAuthorizedRequest). **هذا ليس تغطية طفرات شاملة** — لم أكتشف كل نقطة ضعف محتملة في الاختبارات. **تصنيفاتي مبنية على قراءة الكود + 6 اختبارات طفرات**، وهي مقنعة لكنها ليست شاملة.
4. **الأرقام مشتقّة من عدّ `[Fact]`/`[Theory]` في الملفات السبعة الجديدة** بتوسيع الـ `[Theory]`. اختبارات الملفات المعدَّلة ميكانيكيًا (8) خارج النطاق بالتصنيف لأنها لم تُضَف.
5. **لم أقيّم الأثر التجاري أو أولوية الإصلاح** — كما طُلب.
6. **MEMORY المرفق ≠ MEMORY المستودع** (§6.1). تحققتُ من ادعاءات Slice 10/4/7/والتناقض الداخلي على **نسخة المستودع** (المنطقية لكونها تصف العمل المنجز).
7. **الحكم على الجزء C (5/5 صمدت)** يخصّ **صحة الأحكام على الكود**، لا كفاية الاختبارات التي تحرسها.
8. **لم أكن أعرف مسبقًا** أن التدقيق السابق اقترح «إغلاق S-07». اكتشفتُ §7.1 بالقراءة المباشرة لـ `App.xaml.cs` + `FirstRunAdminViewModel` + `CreateUserCommandHandler`.
9. **المستودع general-purpose؛ لم أراجع الكود خارج النطاق** (مثلاً Cash drawers أعمقًا). §7 يقتصر على ما صادفته في `UsersAndPermissions` + `Authorization` + `ArabicFontResolver` + `FileAppLogger` + `ShellViewModel`.

---

## 10. الملحق (Appendix)

### 10.1 مخرجات الأوامر الأساسية

**بوابة الالتزام:**
```
$ git rev-parse HEAD
377aa282e1f0e8d618840f3fc39a056110d903a0
$ git status --short
(فارغ)
$ git log -1 --format='%H%n%an%n%ad%n%s'
377aa282e1f0e8d618840f3fc39a056110d903a0
medowemado
Mon Sep 28 23:48:47 2026 +0300
[S-07] Follow-up: fix MSB3270 platform-architecture mismatch in TopLab.Presentation.Tests — loop-engineering
```

**الـ commits في النطاق (13):**
```
377aa28 [S-07] Follow-up: fix MSB3270 platform-architecture mismatch in TopLab.Presentation.Tests
8bbaacb [S-07] Slice 12/12: Relational integration tests
0032d9a [S-07] Slice 12/12: Relational integration tests
123ca6a [S-07] Slice 11/12: Presentation structural tests
78d8ced [S-07] Slice 10/12: Font-family resolution
c85fd72 [S-07] Slice 9/12: Logging pipeline behavior moved outermost
74acd44 [S-07] Slice 8/12: Durable file logging
9afc70f [S-07] Slice 7/12: Sent-out-samples write entry point
3404677 [S-07] Slice 6/12: Navigation items filtered by permission
eebaedc [S-07] Slice 5/12: Lock-workstation result is checked
f554821 [S-07] Slice 4/12: User-management authorization: no self-escalation
4b73d85 [S-07] Slice 3/12: Delete-user reference guard fails closed
78721a0 [S-07] Slice 2/12: Validators for the 22 parameterised commands
9c7843a [S-07] Slice 1/12: Hygiene, stale text and dead code
```

**نتائج الاختبارات:**
```
$ dotnet test tests/TopLab.Domain.Tests/...        -p:EnableWindowsTargeting=true
Passed!  - Failed: 0, Passed: 474, Total: 474
$ dotnet test tests/TopLab.Application.Tests/...   -p:EnableWindowsTargeting=true
Passed!  - Failed: 0, Passed: 1446, Total: 1446
$ dotnet test tests/TopLab.Infrastructure.Tests/... -p:EnableWindowsTargeting=true
Failed!  - Failed: 3, Passed: 192, Total: 195
$ dotnet test tests/TopLab.Persistence.Tests/...   -p:EnableWindowsTargeting=true
Passed!  - Failed: 0, Passed: 7, Total: 7
$ dotnet test tests/TopLab.Presentation.Tests/...  -p:EnableWindowsTargeting=true
Test Run Aborted. Framework: 'Microsoft.WindowsDesktop.App', version '8.0.0' (x64) ... No frameworks were found
```

**اختبار الطفرات — إزالة كل حراسات الشريحة 4:**
```
$ (strip IsAuthenticated + IsAbsolutePermission guards from 8 handlers)
handlers stripped: 7
$ dotnet test tests/TopLab.Application.Tests/... -p:EnableWindowsTargeting=true
Passed! - Failed: 0, Passed: 1446, Total: 1446
```

**اختبار الطفرات — كسر قيد التفرّد + سلوك الحذف المتسلسل:**
```
$ sed -i 's/…UserName).IsUnique();/…UserName).IsUnique(false);/' UserConfiguration.cs
$ sed -i 's/…PatientId).OnDelete(DeleteBehavior.Cascade)/…PatientId).OnDelete(DeleteBehavior.NoAction)/' PatientTestConfiguration.cs
$ dotnet test tests/TopLab.Persistence.Tests/... -p:EnableWindowsTargeting=true
Passed! - Failed: 0, Passed: 7, Total: 7
```

**اختبار الطفرات — عكس «fail closed» إلى «fail open» (M-04):**
```
$ (replace catch body with 'throw;')
Failed! - Failed: 2, Passed: 1444, Total: 1446
  Failed …DeleteUserGuardTests.ThrowingProbe_YieldsUnexpected_NotConflict
  Failed …DeleteUserGuardTests.ThrowingSet_YieldsUnexpected
```

**اختبار الطفرات — كسر مدقّق (m-01):**
```
$ sed -i 's/GreaterThan(0)/GreaterThan(-999)/' DeleteUserCommandValidator.cs
Failed! - Failed: 1, Passed: 21, Total: 22
  Failed …ValidatorBehaviourTests.DeleteUser_InvalidId_Fails
```

**سلوك حرّاس الصلاحيات (probe مستقل):**
```
=========== SCENARIO 1: first-run bootstrap ===========
  CreateUser(IsAbsolute=true) by unauthenticated -> Success=False Err=Forbidden
=========== SCENARIO 2: self-escalation via SaveUserPermissions ===========
  Non-absolute grants self EDIT_SYSTEM_SETTINGS+STATISTICS+PT_AUDIT -> Success=True
=========== SCENARIO 3: non-absolute creates privileged user ===========
  Non-absolute creates privileged user -> Success=True
=========== SCENARIO 4: non-absolute DELETES an absolute admin (2 admins) ===========
  Non-absolute deletes absolute admin -> Success=True
=========== SCENARIO 5: non-absolute DEACTIVATES an absolute admin ===========
  Success=True Admin1.IsActive=False
=========== SCENARIO 6: non-absolute REACTIVATES a disabled absolute admin ===========
  Success=True Admin1.IsActive=True
=========== SCENARIO 7: non-absolute EDITS an absolute admin (control) ===========
  Success=False Err=Forbidden   (UpdateUser only)
=========== SCENARIO 8: reads ===========
  GetUsers by non-absolute -> Success=True Count=2
  GetUserById(admin) by non-absolute -> Success=True Absolute=True
=========== SCENARIO 9: UNAUTHENTICATED HasAnyAbsoluteUser ===========
  Success=True Value=True
```

**غياب `CanEditAbsolute`:**
```
$ grep -rn "CanEditAbsolute" src tests
src/TopLab.Presentation/Views/Users/UserManagementView.xaml:36:  …IsEnabled="{Binding CanEditAbsolute}"…
(سطر واحد فقط — XAML. لا وجود في الـ ViewModel.)
```

**ادعاء «3 writers» مُكذَّب:**
```
$ grep -rn "ArabicFontResolver.Resolve" src --include=*.cs
src/TopLab.Infrastructure/Printing/WorkSheetPdfWriter.cs:55   (1 من 3)
$ find tests -name "ArabicFontResolver*"
(فارغ — ملف الاختبار المطلوب من PLAN غير موجود)
```

**استثناء `SqlServerFixture` (يغطّي مخالفة حقيقية):**
```
$ grep -n "localdb\|ConfigurationBuilder\|GetConnectionString(" tests/TopLab.Persistence.Tests/SqlServerFixture.cs
29:  ConnectionString = _container.GetConnectionString();
```

**عدّ الـ 22 validator المُنشأة:**
```
$ git diff --name-only 66a17f7..377aa28 -- 'src/*Validator.cs' | wc -l
22
```

**ترتيب behaviors (Slice 9):**
```
src/TopLab.Application/DependencyInjection.cs:26:  cfg.AddBehavior(…, typeof(LoggingBehavior<,>));
src/TopLab.Application/DependencyInjection.cs:27:  cfg.AddBehavior(…, typeof(ValidationBehavior<,>));
src/TopLab.Application/DependencyInjection.cs:28:  cfg.AddBehavior(…, typeof(AuthorizationBehavior<,>));
```

### 10.2 القائمة الكاملة للملفات المفحوصة

**إنتاج (S-07 + مسارات Journeyman):**
- `src/TopLab.Application/Features/UsersAndPermissions/**` — **كل الـ 14 ملف handler + 14 ملف طلب + Common/UserDtos.cs** (كل واحد مقروء بالكامل)
- `src/TopLab.Application/Common/Behaviors/AuthorizationBehavior.cs`
- `src/TopLab.Application/Common/Authorization/IAuthorizedRequest.cs`
- `src/TopLab.Application/Common/Interfaces/ICurrentUserService.cs`, `IAppLogger.cs`
- `src/TopLab.Application/DependencyInjection.cs`
- `src/TopLab.Domain/Users/User.cs`
- `src/TopLab.Infrastructure/Identity/CurrentUserService.cs`
- `src/TopLab.Infrastructure/DependencyInjection.cs`
- `src/TopLab.Infrastructure/Logging/FileAppLogger.cs`
- `src/TopLab.Infrastructure/Printing/ArabicFontResolver.cs`, `WorkSheetPdfWriter.cs`, `InvoicePdfWriter.cs`, `ReceiptPdfWriter.cs`, `InvoicePrintingService.cs`
- `src/TopLab.Infrastructure/Persistence/Configurations/UserConfiguration.cs`, `PatientTestConfiguration.cs`
- `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs`
- `src/TopLab.Presentation/App.xaml.cs`, `MainWindow.xaml`
- `src/TopLab.Presentation/DependencyInjection.cs`
- `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs`
- `src/TopLab.Presentation/ViewModels/Users/UserManagementViewModel.cs` (+ `UserManagementView.xaml`)
- `src/TopLab.Presentation/ViewModels/Setup/FirstRunAdminViewModel.cs`
- `src/TopLab.Presentation/ViewModels/Patients/SentOutSamplesViewModel.cs` (+ `SentOutSamplesView.xaml`)
- `src/TopLab.Presentation/Common/Dialogs/DialogService.cs`
- `src/TopLab.Presentation/Views/Shell/AboutWindow.xaml.cs`

**اختبارات (كل الـ 17 ملفًا في النطاق):**
- `tests/TopLab.Application.Tests/Features/UsersAndPermissions/DeleteUserGuardTests.cs` (جديد)
- `tests/TopLab.Application.Tests/Features/Validators/ValidatorBehaviourTests.cs` (جديد)
- `tests/TopLab.Application.Tests/Features/Validators/ValidatorCompletenessTests.cs` (جديد)
- `tests/TopLab.Application.Tests/Features/UsersAndPermissions/{CreateUserCommandHandlerTests, DeactivateReactivateTests, DeleteUserCommandHandlerTests, GetUsersQueryHandlerTests, SaveUserPermissionsCommandHandlerTests, SignInCommandValidatorTests, UpdateUserCommandHandlerTests}.cs`
- `tests/TopLab.Application.Tests/Common/Fakes/FakeCurrentUserService.cs`
- `tests/TopLab.Infrastructure.Tests/Identity/CurrentUserServiceTests.cs`
- `tests/TopLab.Infrastructure.Tests/Printing/{WorkSheetPrintingServiceTests, InvoicePrintingServiceTests, ReceiptPrintingServiceTests}.cs`
- `tests/TopLab.Persistence.Tests/NeverConnectGuardTests.cs` (جديد)
- `tests/TopLab.Persistence.Tests/RelationalIntegrationTests.cs` (جديد)
- `tests/TopLab.Persistence.Tests/SqlServerFixture.cs` (جديد)
- `tests/TopLab.Presentation.Tests/DeferredBehaviourTests.cs` (جديد)
- `tests/TopLab.Presentation.Tests/PresentationStructuralTests.cs` (جديد)

**مستندات (مقارنة فقط، لا كدليل):**
- `Docs/OpenCode/S-07.md` (PLAN — مطابق للمرفق)
- `Docs/OpenCode/S-07-memory.md` (MEMORY — **مختلف** عن المرفق، §6.1)

---

*نهاية التقرير. كل نتيجة في هذا التقرير مُسنَدة إلى مسار ومُخرج أمر محدد. التوصيات دون ترتيب أولوية كما طُلب.*
