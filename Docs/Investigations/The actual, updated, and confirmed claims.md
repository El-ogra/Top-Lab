# The actual, updated, and confirmed claims — الادعاءات المؤكدة والمحدَّثة

- **التاريخ:** 2026-09-29
- **مصدر الحقيقة:** الحالة الراهنة للمستودع عند `HEAD = 0aea311` (ويندوز، فرع `main`) — 23 commitًا بعد `377aa28` — تم التحقق من كل ادعاء بالفحص المباشر للشيفرة/الإعدادات/الاختبارات، وبتنفيذ البناء والاختبارات.
- **المصادر الثلاثة المدقَّقة (اختصارات تُستخدم أدناه):**
  - «البنود» = `تقرير البنود من الاول الى الثالث.md`
  - «الوكيل» = `تقرير الوكيل المحلي.md`
  - «المستقل» = `independent-security-and-test-theater-audit.md`
- **ملاحظة منهجية:** المستودع تقدَّم 23 commitًا بعد الالتزام الذي دقَّقته التقارير الثلاثة (سلسلة `Security-Fix-1..5` و`Fix-N1..N4` و`Fix-T1..T3` و`Fix-M1` و`Fix-H1` و`R1` و`D1` و`D2` و`Owner-D1..D4`)، فكثير من الثغرات والاختبارات «المسرحية» الموثَّقة فيها صُلحت. ما يلي هو **الادعاءات الصحيحة والقائمة فعليًا الآن فقط**، مع ذكر الملفات المصدر لكل ادعاء (وعند تكراره في أكثر من ملف تُذكر كلها مرة واحدة).

---

## 1. الأمن وإدارة المستخدمين

1. لا ينفّذ أي طلب من طلبات `UsersAndPermissions` الـ14 (9 أوامر + 5 استعلامات) الواجهة `IAuthorizedRequest`؛ الإشارة الوحيدة للواجهة داخل الميزة هي **تعليق** في `ChangeOwnPasswordCommand.cs:10` يشرح غيابها. — المصادر: «المستقل» §3.0؛ «البنود» D-3.
2. منح/سحب الصلاحيات (`GrantPermission`/`SetAbsolutePermission`/`ClearPermissions`) لا يحدث إلا داخل `UsersAndPermissions`، في 4 مواضع فقط: `CreateUserCommandHandler.cs:89`، `SaveUserPermissionsCommandHandler.cs:49,57`، `UpdateUserCommandHandler.cs:63`. لا يوجد أي مسار منح صلاحيات خارجه. — المصدر: «المستقل» §3.8.
3. `SaveUserPermissionsCommandHandler` يشترط جلسة مسجَّلة **و**صلاحية مطلقة قبل أي تعديل صلاحيات (`:21-27`)؛ الادعاء السابق بأن أي مستخدم يمنح نفسه صلاحيات لم يعد صحيحًا. — المصادر: «البنود» (B-01)؛ «الوكيل» (S4)؛ «المستقل» §3.3.
4. معالجات `DeleteUser`/`DeactivateUser`/`ReactivateUser` ترفض استهداف مدير مطلق من منادي غير مطلق (`DeleteUserCommandHandler.cs:39`، `DeactivateUserCommandHandler.cs:31`، `ReactivateUserCommandHandler.cs:31`)، وتصون «آخر مدير مطلق نشط» (`Delete:44-46`، `Deactivate:41-43`، و`UpdateUser:49-55` للتعديل). — المصادر: «البنود» (B-01)؛ «الوكيل» (S4)؛ «المستقل» §3.4.
5. `CreateUserCommandHandler` يملك مسار إقلاع أول (bootstrap) مشروطًا بدقة: لا جلسة + `IsAbsolutePermission:true` + بلا رموز صلاحيات + لا يوجد أي مدير مطلق نشط في القاعدة (`:24-27`)؛ وغير المطلق ممنوع من إنشاء مستخدم مطلق أو منح رموز صلاحيات (`:36-40`). الادعاء السابق بـ«قفل أول تشغيل» لم يعد صحيحًا. — المصدر: «المستقل» §3.2/§7.1.
6. استعلاما `GetUsers` و`GetUserById` محصوران بالمستخدم المطلق (`GetUsersQueryHandler.cs:22`، `GetUserByIdQueryHandler.cs:22`). — المصدر: «المستقل» §3.5.
7. الخاصية `CanEditAbsolute` موجودة فعلًا في `UserManagementViewModel.cs:174` مع إشعار تغيير في `:285`، ومربوطة بمربع «صلاحية مطلقة» في `UserManagementView.xaml:36`. الادعاء السابق بـ«خاصية غير موجودة» لم يعد صحيحًا. — المصادر: «البنود» (B-01)؛ «الوكيل» (U-6)؛ «المستقل» §3.6.
8. `HasAnyAbsoluteUserQueryHandler` بلا حارس مصادقة إطلاقًا (لا يحقن `ICurrentUserService` — `HasAnyAbsoluteUserQueryHandler.cs:12-22`)؛ وهو مقصود كبوابة «أول تشغيل» في `App.xaml.cs`. — المصدر: «المستقل» §3.5.
9. لا يوجد أي انتحال شخصية (impersonation / act-as) في الشيفرة (بحث = 0). — المصدر: «المستقل» §3.8.
10. منطق `AuthorizationBehavior` سليم: `IsAbsolutePermission || HasPermission(code)` وإلا `Forbidden` (`AuthorizationBehavior.cs:34-39`). — المصدر: «المستقل» §3.8.
11. كتالوج الصلاحيات المزروع = 13 رمزًا فقط (`PermissionId.Create(1)`…`(13)` في `PermissionConfiguration.cs:17-31`)، ولا يوجد رمز `MANAGE_USERS`. — المصادر: «البنود» §3.4/D-3؛ «الوكيل» (السبب 7)؛ «المستقل» §5.2.
12. حذف المستخدم «فاشل مغلقًا»: تعريف `HasReferences` عند `DeleteUserCommandHandler.cs:73` و`catch (Exception)` عند `:127` يعيد `Error.Unexpected` + `true` عند أي استثناء (قبل أي `_db.Remove` (النطاق :62-68))؛ 9 فحوص مراجع (User, Patient, Test, PatientTest, PaymentOperation, CashMovement, ExternalEntity, SentOutSample, AttendanceRecord)؛ ويحذف صفوف `UserPermissionGrant` صراحةً (`:62-66`) قبل `_db.Remove(user)` (`:68`). — المصادر: «البنود» (M-04)؛ «الوكيل» (S3)؛ «المستقل» §5.3/§7.11.
13. ستة استعلامات قراءة مالية مُبوّبة الآن على الرمز المزروع `CASH_DISBURSE_DEPOSIT` عبر `IAuthorizedRequest`: `GetPatientAccountQuery`، `ListPatientPaymentsQuery`، `GetPatientInvoiceQuery`، `GetSentOutLabAccountQuery`، `GetPatientReceiptQuery`، `GetSentOutSamplesQuery`. **أثر موثّق:** `PatientEditor.RefreshBillingAsync` يعرض `Forbidden` عبر `ResultErrorPresenter` لمستخدم `ADD_EDIT_PATIENT` بلا رمز نقدي (المجامير تبقى صفر). — المصدر: `FinancialReadAuthorizationTests.cs` + `R1`.
14. `VerifySecondaryPasswordQueryHandler` يشترط جلسة ويتحقق من هوية **المنادي نفسه** (`u.Id.Value == _currentUser.UserId` — `:26-31`). — المصدر: «المستقل» §3.5/§3.7.

## 2. مسار الدخول والإقلاع

15. `SignInCommand`/`SignOutCommand`/`GetCurrentSessionQuery`/`VerifySecondaryPasswordQuery` لا تنفّذ `IAuthorizedRequest` — مسار الدخول بلا حارس صلاحية يعترضه؛ ويغطيه اختبار `LoginPath_Requests_DoNotImplementIAuthorizedRequest`. — المصادر: «البنود» §4.3؛ «الوكيل» (S4)؛ «المستقل» §3.7.
16. `GetCurrentSessionQueryHandler` و`VerifySecondaryPasswordQueryHandler` يفحصان `IsAuthenticated` ويرجعان `Forbidden`. — المصدر: «المستقل» §3.7.
17. تسلسل الإقلاع في `App.xaml.cs`: قراءة `%ProgramData%\TopLab\appsettings.json` → معالج إعداد قاعدة البيانات عند غياب سلسلة الاتصال → تسجيل DI → **هجرة تلقائية** `MigrateAsync` عند البدء (`:64` تقريبًا) → بوابة أول تشغيل عبر `HasAnyAbsoluteUserQuery` تفتح `FirstRunAdminWindow` → `LoginWindow` → `MainWindow`. — المصادر: «الوكيل» §9/§11؛ «المستقل» §3.2.
18. فشل الهجرة عند الإقلاع يعرض رسالة عربية ويغلق التطبيق (`Shutdown(1)`). — المصدر: «الوكيل» §11.

## 3. التنقل والواجهة

19. `BuildNavigationItems` فيه 12 عنصرًا؛ المبوَّبة: «ورقة العمل»/«الإحصائيات»/«النظام»/«قفل المحطة» برموز الكتالوج الأربعة + «المستخدمون» بالمطلق فقط (`ShellViewModel.cs:171-178`)؛ والعناصر السبعة الباقية مفعَّلة بقرار مالك موثَّق. — المصادر: «البنود» (F-04)؛ «الوكيل» (S6)؛ «المستقل» §5.2 (محدَّث بعد بوابة «المستخدمون»).
20. زر التنقل مربوط فعليًا: `IsEnabled="{Binding IsEnabled}"` في `MainWindow.xaml:43`. — المصادر: «الوكيل» (S6)؛ «المستقل» §5.2.
21. فتح شاشة «المستخدمون» يتطلب حوار كلمة المرور الثانوية قبل التنقل (`ShellViewModel.cs` — فرع `ShowSecondaryPasswordDialogAsync`). — المصدر: «المستقل» §3.5.
22. نتيجة قفل المحطة تُفحص: عند الفشل يُعرض الخطأ عبر `ResultErrorPresenter` ولا تُفتح نافذة الفتح (`ShellViewModel.cs:370-376`، وانتهاء الكتلة عند `ShowErrorAsync :376`)، مع إعادة تحميل الحالة عبر `LoadStatusAsync` (`:381` و`:398`) و seam خاصية `showUnlock` العام (`:366` مع `:385-387`)؛ ومغطاة باختبارين سلوكيين (فشل/نجاح). — المصادر: «البنود» (NEW-05-LOCK)؛ «الوكيل» (S5)؛ «المستقل» §5.5.
23. نقطة دخول «إرسال عيّنة»: `SetupAsync` **منتظَرة** داخل `try/catch` والقائمة تُعاد تحميلها بعد نجاع الحوار (`SentOutSamplesViewModel.cs:186-199`). الادعاء السابق بـ fire-and-forget وعدم التحديث لم يعد صحيحًا. — المصدر: «البنود» (M-01).
24. الأزرار الميتة أُزيلت من `MainWindow.xaml`؛ الزران الفعليان: تنقل (`:43`) وتغيير كلمة المرور (`:50`)؛ و`HomeViewModel.cs` فارغ (5 أسطر) بلا أثر وظيفي. — المصدر: «الوكيل» (M-01/§11.3).
25. نافذة «حول البرنامج»: سطر الإصدار حقيقي بلا Placeholder (`AboutWindow.xaml.cs:12`)؛ و3 أسطر Placeholder باقية عمدًا لمحتوى المالك (`AboutWindow.xaml:21-23`). — المصادر: «البنود» D-6؛ «الوكيل» (السبب 9).
26. كل ملفات العرض `Views/**/*.xaml` تحمل `FlowDirection="RightToLeft"` ويفرضها اختبار `EveryView_IsRightToLeft`. — المصدر: «المستقل» §4.8 (الوضع الحالي).

## 4. الطباعة والخطوط

27. كاتبا PDF الثلاثة — `WorkSheetPdfWriter.cs:55`، `InvoicePdfWriter.cs:54`، `ReceiptPdfWriter.cs:53` — يستخدمون جميعًا `ArabicFontResolver.Resolve`؛ الادعاء السابق بـ«كاتب واحد من ثلاثة» و«3 writers updated كاذب» لم يعد صحيحًا. — المصادر: «البنود» (m-09+NEW-03)؛ «الوكيل» (السبب 1/D-1)؛ «المستقل» §6.2.
28. لا يوجد خط مضمَّن ولا أي ملف خط (ttf/otf/…) متتبَّع في المستودع، ولا استدعاء `RegisterFont` (بحث = 0). — المصادر: «البنود» D-8؛ «الوكيل» (السبب 9/D-8).
29. القيم الافتراضية لـ FontFamily في 4 ViewModels (`ReceiptSettingsViewModel`، `ReportSettingsViewModel`، `EnvelopeSettingsViewModel`، `SystemSettingsViewModel`) أصبحت `string.Empty` ليقرر `ArabicFontResolver` البديل (لا Windows-only name). — المصدر: `SettingsFontFamilyDefaultTests.cs` + `D1`.
30. الكاتبان الثلاثة يضبطون `Settings.License = LicenseType.Community` (QuestPDF) بتعليق owner-confirmed؛ إعادة تأكيد أهلية Community للتوزيع التجاري بقرار المالك. — المصدر: «الوكيل» (السبب 9).

## 5. التسجيل وخط الأنابيب

31. `FileAppLogger` يكتب ملفًا يوميًا تحت `%ProgramData%\TopLab\logs` بسطر `requestName|outcome|duration`، مع `lock` و`catch` لا يرمي أبدًا؛ مُسجَّل Singleton في `Infrastructure/DependencyInjection.cs:102` فقط (لا تسجيل في Presentation)؛ توقيع `IAppLogger.Log(string, string, TimeSpan)` دون تغيير — فلا يمكن للسجل أن يحمل كلمات مرور أو بيانات مرضى. — المصادر: «البنود» (F-03)؛ «الوكيل» (S8)؛ «المستقل» §5.1.
32. ترتيب pipeline behaviors: `Logging → Validation → Authorization` (`Application/DependencyInjection.cs:26-28`)، والتعليق التوثيقي أعلى الملف محدَّث ومطابق للترتيب الفعلي. — المصادر: «البنود» (NEW-05-LOG)؛ «الوكيل» (S9)؛ «المستقل» §3.7.

## 6. الاختبارات — الحالة الراهنة

33. البناء نظيف (0 تحذير / 0 خطأ)، وكل الاختبارات خضراء على ويندوز: Domain **474**، Application **1484**، Infrastructure **201**، Persistence **7 Passed + 1 Skipped**، Presentation **44**. مجموع الناجحة = **2210** (2211 مع Skipped). (تحل هذه الأرقام محلّ 474/1469/201/7/32.) — المصادر: «البنود» §4.2؛ «الوكيل» §2؛ «المستقل» §2.5 (تحديث بالتنفيذ).
34. اختبارات `RelationalIntegrationTests` التابعة لـDocker تُبلَّغ **Skipped صراحةً** عبر `DockerFactAttribute` (لا «نجاح أجوف» بـ`return;` مبكر)؛ والاختبار الحي يشغّل `MigrateAsync` فعليًا على حاوية عند توفر Docker؛ واختبارات النموذج صريحة كعقد تصميمي: فهرس `UserName` الفريد، دقة `decimal(18,2)` على `PaymentOperation.Amount`، وسلوك `Cascade`. — المصادر: «البنود» (F-05/M-03)؛ «الوكيل» (السبب 3)؛ «المستقل» §4.6 (بعد إصلاح التخطي الصامت).
35. `NeverConnectGuardTests` تؤكد `Assert.NotEmpty(files)` وتفحص فعليًا `RelationalIntegrationTests.cs` — الادعاء السابق بـ«مجموعة فارغة لا يمكن أن تفشل» لم يعد صحيحًا. — المصادر: «البنود» (F-05/M-03)؛ «المستقل» §4.5.
36. `DeferredBehaviourTests` اختبارات **سلوكية حقيقية** (بناء `ShellViewModel` مع `FakeCurrentUserService`/`FakeSender`) تغطي بوابات الكتالوج الأربع + بوابة «المستخدمون» + فشل/نجاح القفل + تسجيل validators وbehaviors في DI — لا بحث نصي في المصدر. — المصادر: «البنود» (M-02)؛ «المستقل» §4.7.
37. `PresentationStructuralTests` تحلّ `x:Type` إلى أنواع CLR فعلية وأسماء `{Binding}` إلى خصائص عامة موجودة على `ShellViewModel`/`NavigationItem` — لا اكتفاءً بعدّ النصوص. — المصادر: «البنود» (M-02)؛ «المستقل» §4.8.
38. اختبار `AfterRevoking_AuthorizationFails` يشغّل `AuthorizationBehavior` الحقيقي ويقرأ المنح من قاعدة الاختبار بعد السحب (لا tautology على `HashSet.Clear`). — المصدر: «المستقل» §4.10.
39. `ValidatorCompletenessTests` تثبت التسجيل عبر حاوية DI حقيقية: كل أمر مُعلَّم (من أصل 129 أمرًا) يحلّ إلى `IValidator<T>`، وثلاثة أوامر بلا معاملات بلا validator بالضبط: `LockWorkstationCommand`، `ApplyDatabaseUpdatesCommand`، `SignOutCommand`. — المصادر: «البنود» (m-01)؛ «الوكيل» (S2)؛ «المستقل» §4.4.
40. يوجد ملف اختبار للمحلل: `tests/TopLab.Infrastructure.Tests/Printing/ArabicFontResolverTests.cs` (حالات null/فارغ/Arial). — المصدر: «الوكيل» (D-2).
41. أُضيفت اختبارات حارسة أمنية جديدة: `CreateUserBootstrapTests.cs` و`SaveUserPermissionsGuardTests.cs` و`ActorTargetGuardTests.cs` و`CreateUserPermissionCodesGuardTests.cs` و`GetUsersGuardTests.cs` إلى جانب `DeleteUserGuardTests.cs`، و`FinancialReadAuthorizationTests.cs` و`SettingsFontFamilyDefaultTests.cs`. — المصدر: «المستقل» §4.2 (تحديث).
42. لا `NotImplementedException` إنتاجية ولا `TODO`/`FIXME`/`HACK` في `src/` (بحث = 0). — المصدر: «الوكيل» (U-09).

## 7. قاعدة البيانات والهجرات

43. مجلد `Migrations` = 8 هجرات + snapshot؛ لا `AlterColumn` في أي هجرة؛ وطبقة `Persistence` لم تتغير إطلاقًا منذ الالتزام `377aa28` الذي دقَّقته التقارير. — المصادر: «البنود» §3.4–3.5؛ «الوكيل» §3.
44. عيب قائم في `Down()` لهجرة `20260909033414_AddAnalyteProfileDomain`: تُحدَّث العمود `AnalyteName` من `Analytes` قبل إعادة إنشاء العمود بأسطر (UPDATE ≈ سطر 394، `AddColumn` ≈ سطر 449) — أي تراجع يستهدف ما قبل هذه الهجرة سيفشل. — المصادر: «البنود» D-1؛ «الوكيل» (السبب 4).
45. لا تفاؤلية تزامن: لا `RowVersion`/`ConcurrencyToken`/`ConcurrencyStamp` في `src/` ولا `tests/` (بحث = 0). — المصادر: «البنود» D-4؛ «الوكيل» (السبب 5).

## 8. بنود مؤجلة مفتوحة (قرارات مالك معلّقة)

46. لا نظام ترخيص/تفعيل للمنتج: لا Entity ترخيص، لا أمر تفعيل، لا بوابة ترخيص في تسلسل الإقلاع (بحث `License|Activation|Trial` في الكود = 0 خارج تعليقات QuestPDF). **قرار المالك (إخلاء مسؤولية الترخيص):** استخدام داخلي فقط، لا إنفاذ للترخيص بقرار المالك، يُعاد فتحه قبل أي توزيع خارجي. — المصدر: «الوكيل» (السبب 2/F-02).
47. لا قفل تلقائي عند الخمول (idle auto-lock): لا مؤقّت خمول في المشروع (بحث = 0)؛ القفل اليدوي فقط عبر `LockWorkstationCommand`. — المصادر: «البنود» D-7؛ «الوكيل» (السبب 6).
48. مجلد `Features/SamplePipeline` حُذف بالكامل؛ اختبارا سلوك الأنبوب (`BehaviorsAuthorizationBehaviorTests` و`BehaviorsValidationBehaviorTests`) يعتمدان الآن على أنواع طلب محلية داخل مشروع الاختبار (`LocalAuthProbe` / `LocalValidationProbe`) بلا رمز إذن منتج. (grep `SamplePipeline|EchoName|SAMPLE_PIPELINE` = 0.) — المصادر: «البنود» D-5؛ «الوكيل» (السبب 9/F-06).

## 9. قيود دائمة وإعدادات

49. لا بوابة نتائج مرضى أونلاين (patient/web portal) في أي مكان (بحث = 0 في `src` و`tests`). — المصدر: «الوكيل» §9.
50. الملف المتتبَّع الوحيد للإعدادات هو `appsettings.example.json` بقيم placeholder (`YOUR_SERVER/YOUR_USER/YOUR_PASSWORD`) بلا `localdb` وبلا أسرار؛ `appsettings.json` الحقيقي غير متتبَّع. — المصادر: «البنود» (m-10)؛ «الوكيل» §9.
51. لا ملفات نتائج اختبارات متتبَّعة؛ `.gitignore` يمنع `TestResults/` و`*.coverage` و`*.trx`. — المصدر: «البنود» (m-06).
52. `TopLab.Presentation.Tests`: `net8.0-windows` + `UseWPF` + `PlatformTarget x64` (إصلاح MSB3270)؛ البناء الكامل بلا تحذيرات يؤكد زوال التحذير. — المصادر: «البنود» (MSB3270)؛ «الوكيل» §7.
53. الحزم المركزية: `MediatR 12.5.0`، `FluentValidation 12.1.1`، `QuestPDF 2026.9.0`، `Testcontainers.MsSql 3.10.0`، `xunit 2.9.3`. — المصدر: «الوكيل» (Directory.Packages.props).
54. `Docs/OpenCode/S-07-memory.md` ما زال يحمل قسم «Current Status» بقيمة `Slices complete: 2 / 12` المتناقضة مع Slice Index — التناقض التوثيقي قائم في المستند. — المصادر: «البنود» §4.5؛ «الوكيل» (D-3)؛ «المستقل» §6.4.
55. تفاصيل أُغفلت سابقًا من هذه القائمة وموجودة في الشيفرة: (أ) `ArabicFontResolver.cs:73-95` — قائمة الخطوط المفضّلة `PreferredArabicFonts` وفحص `GdiCharSet == 178` في `SupportsArabic`؛ (ب) `ShellViewModel.cs:366` — `LockWorkstationAsync` عامة مع فتحة الاختبار `showUnlock` عند `:385-387`؛ (ج) `SentOutSamplesViewModel.cs:183/:211` — فتحة حوار `showDialog` (فرع seam) في `OpenSendSampleOutAsync`.

---

### ملاحظة ختامية
كل ما ورد أعلاه مُثبت على الحالة الراهنة للمستودع بمسار وملف/سطر أو بتنفيذ مباشر. الادعاءات المستبعدة من هذه القائمة راجع تقرير التدقيق المرافق في نافذة المحادثة (ثغرات أُصلحت في سلسلة `Security-Fix/Fix-*`، أرقام اختبارات قديمة، ادعاءات بيئية خاصة بأجهزة التدقيق، وادعاءات تعذّر التحقق منها حاليًا).
