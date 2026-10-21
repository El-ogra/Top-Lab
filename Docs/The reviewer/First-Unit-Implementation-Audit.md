تقرير تدقيق المطابقة الوظيفية — Top-Lab
الملف المرجعي: Docs/The reviewer/01-patient-registration-and-test-registration.md
القسم الأول: ملخص تنفيذي
الملف المرجعي الذي تم تدقيقه: الملف الأول فقط — 01-patient-registration-and-test-registration.md. تمت قراءته قراءة كاملة من السطر 1 إلى السطر 1341 (شمل: التقديم، §1.1 التسلسل الهرمي للوظائف، §1.2 جداول تعريفات العناصر، §1.3 خريطة الشاشات، §1.4 مسارات العمل، §1.5 قواعد التحقق، §1.6 المخرجات، §1.7 اللا يقينيات). لم يُستخدم الملف الثاني (02-work-sheet-for-patients-or-tests.md) لتوسيع النطاق؛ILY إشارة مرجعية وحيدة له: 1.2.2.6.11 Work Sheet Action حيث فُحص مسار التنفيذ داخل المشروع لفهم الوظيفة الحالية دون تدقيق مستقل للوحدة الأخرى.

نطاق المشروع المفحوص: TopLab.sln — أربعة مشاريع: TopLab.Domain, TopLab.Application, TopLab.Infrastructure, TopLab.Presentation (‏84 ملف XAML و83 ملف ViewModel)، إضافة إلى 5 مشاريع اختبارات (tests/) وDocs/ وinfo/ وobjects/ وrefs/. تم تتبّع المسارات عبر: الواجهة (XAML) ← ViewModel ← Application handler ← Domain ← Persistence/EF ← طبقة الطباعة.

طريقة العد: كل معرّف تعريف في جداول §1.2 (الأسطر 529–1091) يُعدّ عنصرًا واحدًا مستقلًا. المجموعات والحاويات تُعدّ مستقلة عن عناصرها الفرعية (مثلًا 1.2.2.3 يُعدّ إضافةً إلى 1.2.2.3.1…1.2.2.3.15). جداول §1.5 (قواعد التحقق) و§1.6 (المخرجات) و§1.7 (اللا يقينيات) لا تُعدّ عناصر مستقلة لأن كل مُعرَّف فيها مكرَّر داخل §1.2؛ وقد عولجت كقواعد إضافية في القسم الثامن. الإجمالي = 491 عنصرًا (تحقّق حسابي: 34+100+31+37+11+80+10+4+35+36+8+13+26+27+39).

النتائج:

التصنيف	العدد	النسبة
A — مطابق وظيفيًا	146	29.7%
B — موجود جزئيًا	108	22.0%
C — موجود بتنفيذ مختلف	61	12.4%
D — غير موجود	176	35.8%
E — غير قابل للتحقق	0	0%
الإجمالي	491	100%
أهم أوجه النقص
الوحدة 1.13 (تسليم النتائج عبر الخدمة الخارجية) غير منفَّذة إطلاقًا (22 من 26 عنصرًا D) — لا وجود لأي HttpClient في src/، ولا أمر رفع/حجب، ولا شاشة رفع، ولا حقل عنوان الموقع في Domain/Settings/*. «موقع النتائج» بأكمله مفقود.
الوحدة 1.14 (الباركود والأنابيب) — يوجد نوع باركود واحد عام فقط (Infrastructure/Barcode/BarcodeService.cs:44-92)؛ لا مفهوم «أنبوب/وعاء» إطلاقًا في Domain (‏Serum Fasting, EDTA Blood, Citrated Blood 0.4 = 0 نتائج في src/).
الوحدة 1.6 (إدخال النتائج) — لوحة النتائج نفسها لا تحوي أعمدة الوحدة والمدى وعلم abnormalный (Low/High/Critical)؛ ولا يوجد عمود حالة الاختبار ولا علم "Finish" في PatientTest (Domain/Results/PatientTest.cs:10-64).
الوحدة 1.4 (التسليم) — من أصل 13 إجراءً موثقًا في كتلة الإجراءات، 10 غير موجودة على شاشتي التسليم؛ وكتلة نطاق المرضى (مصدر/مجموعة/لوحة اليوم) غائبة بالكامل.
الوحدة 1.10 (البحث) — شريط الإجراءات الموثق بعشرة إجراءات: 0 على الشاشة؛ لا مُحدِّد فرع ولا مُحدِّد مصدر (Backup) — ولا يوجد مسار قراءة احتياطي في البنية (.bak يُقرأ فقط عبر RestoreAsync المُدمِّر في Infrastructure/Persistence/Maintenance/SqlServerDatabaseMaintenanceService.cs:49-86).
التاريخ المرضي (1.15) — مسار الطباعة التقني معزول عن المستخدم: PatientsHubViewModel.cs:80-87 يفتح شاشة التاريخ بـ patientId = 0، فتفشل الطباعة دائمًا؛ والأسطر المُدرجة في التاريخ لا تصل إلى التقرير المطبوع (PrintCombinedReportCommandHandler.cs:51-53 يعيد بناء التقرير من الاختبارات الحالية فقط).
أهم القيود التي منعت التحقق
لم يُنفَّذ أي بناء أو اختبار أو اتصال بقاعدة بيانات (قيد إلزامي)، فجميع الأحكام تحليل ساكن مدعوم بدليل مباشر.
مفاتيح التهيئة المأخوذة من قاعدة البيانات (بيانات البذور لـ MedicalConditionType، PrinterAssignment, ReportItemTemplate, SystemSettings) لم يُتحقق من قيمها الفعلية؛ أُثبتت قيم البذور存在于 في الشيفرة فقط.
سلوك محرك QuestPDF في الإخراج (تباعد الأسطر، الإزاحة، الظل) غير مُثبَّت زمن التشغيل.
القسم الثاني: سجل المطابقة الشامل
الأعمدة: المعرّف | اسم الوظيفة | التصنيف | حالة التنفيذ | الأدلة | الفرق/سبب عدم التحقق ‏‎…‎ = مسار نسبي من جذر المشروع.

2.1 — الوحدة 1.1 (تطبيق واجهة وتسجيل دخول)
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.1.1	Top Ribbon	B	شريط علوي ثابت بزر لكل وحدة	MainWindow.xaml:34-47; ShellViewModel.cs:187-317	لا يوجد إبراز للوحدة النشطة
1.1.1.1	Ribbon Item List	B	12 زرًّا	ShellViewModel.cs:189	"Send Result" غير موجود؛ أُضيف "قفل المحطة" غير الموثق
1.1.1.1.1	Exit Item	A	إغلاق البرنامج	ShellViewModel.cs:212-219	—
1.1.1.1.2	About Us Item	A	نافذة "حول البرنامج"	ShellViewModel.cs:220-227	—
1.1.1.1.3	Send Result Item	D	غير موجود	ShellViewModel.cs:189 (لا "Send Result")	شاشة التسليم تُفتح من hub المرضى لا من الشريط
1.1.1.1.4	Setting Item	A	SettingsDashboardViewModel	ShellViewModel.cs:284-287	—
1.1.1.1.5	System Item	A	AuditViewModel	ShellViewModel.cs:276-283	—
1.1.1.1.6	Users Item	A	UserManagementViewModel + بوابة كلمة مرور ثانوية	ShellViewModel.cs:232-245	—
1.1.1.1.7	Statistics Item	A	StatisticsViewModel	ShellViewModel.cs:268-275	—
1.1.1.1.8	Accounts Item	A	AccountsHubViewModel	ShellViewModel.cs:254-267	—
1.1.1.1.9	Tools Item	A	UtilitiesViewModel	ShellViewModel.cs:246-253	—
1.1.1.1.10	Work Sheet Item	A	WorkSheetsViewModel	ShellViewModel.cs:300-307	—
1.1.1.1.11	Laboratory Item	A	LabHubViewModel	ShellViewModel.cs:292-299	—
1.1.1.1.12	Patients Item	A	PatientsHubViewModel	ShellViewModel.cs:288-291	—
1.1.1.2	Active Module Indicator	D	غير موجود	ShellViewModel.cs:16-21 (لا حالة active)؛ MainWindow.xaml:41-45 (زر بلا Trigger)	لا حالة نشطة تُحمل ولا تُعرض
1.1.2	Status Bar	A	شريط سفلي بأربعة عناصر	MainWindow.xaml:48-90	—
1.1.2.1	User Name Indicator	A	اسم المستخدم	MainWindow.xaml:49-52; ShellViewModel.cs:322-324	—
1.1.2.2	Last Login Indicator	C	يعرض آخر دخول لا السابق	ShellViewModel.cs:331-333; SignInCommandHandler.cs:50 يكتب LastLoginAtUtc قبل القراءة (GetCurrentSessionQueryHandler.cs:29)	دلالة القيمة مختلفة وظيفيًا
1.1.2.3	Database Label Indicator	C	يعرض حالة الاتصال (متصل/غير متصل)	MainWindow.xaml:58-85; ShellViewModel.cs:363-373	لا يعرض اسم مصدر البيانات ("SQL DATABASE")
1.1.2.4	Today Indicator	A	التاريخ/الوقت بمؤقّت 1ث	MainWindow.xaml:87-89; ShellViewModel.cs:85-88	—
1.1.3	Startup Splash Screen	C	لا شاشة بدء	App.xaml.cs:28-113 (تسلسل: DB setup → ترحيلات → admin → login)	التسلسل موجود لكن لا شاشة splash ولا مؤشّر تقدم
1.1.3.1	Data Source Option Group	D	غير موجود	Views/Setup/DatabaseSetupWindow.xaml:26-54 (حقول نصية فقط)	مصدر واحد فقط
1.1.3.1.1	File Database Option	D	غير موجود	DatabaseSetupViewModel.cs:59-93 (SqlConnection فقط)	لا مزوّد قاعدة ملفية
1.1.3.1.2	Client Server Database Option	B	SQL Server عبر حقول خماسية	DatabaseSetupWindow.xaml:29-42; DatabaseSetupViewModel.cs:59-93	ليس "خيارًا" بل حقول إعداد، ويُعرض عند أول تشغيل فقط
1.1.3.2	Startup Progress Indicator	D	غير موجود	DatabaseSetupWindow.xaml:44-45 (نص فقط)	لا مؤشر تقدم
1.1.4	Sign-In Window	A	نافذة تسجيل دخول	App.xaml.cs:98-107; Views/Setup/LoginWindow.xaml:1-46	—
1.1.4.1	Sign-In User Name Field	A	UserNameTextBox	LoginWindow.xaml:31-32; LoginViewModel.cs:26-30	—
1.1.4.2	Sign-In Password Field	A	PasswordBox	LoginWindow.xaml:34-35	—
1.1.4.3	Show Password Characters Option	A	ShowPasswordCheckBox	LoginWindow.xaml:37; LoginWindow.xaml.cs:42-57	—
1.1.4.4	Remember Login Option	D	غير موجود	بحث `Remember	تذكرفيsrc` = 0
1.1.4.5	Application Sign In Action	A	تحقّق من كلمة المرور وجلسة	LoginWindow.xaml:42 → LoginViewModel.cs:70-97 → SignInCommandHandler.cs:31-66	—
1.1.5	Patient Search Toolbar	D	غير موجود	Views/Patients/PatientsHubView.xaml:1-24 (UniformGrid من 6 أزرار)	لا شريط أدوات في وحدة المرضى
1.1.5.1	Quick Search Field	D	غير موجود	بحث QuickSearch في src = 0	—
1.1.5.2	Quick Search Mode Options	D	غير موجود	بحث `TM	TG
2.2 — الوحدة 1.2 (تسجيل المرضى والبيانات الأساسية)
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.2.1	Main Patients Window	B	شاشة مدخل وحدة المرضى	Views/Patients/PatientsHubView.xaml:1-24; PatientsHubViewModel.cs:21-139	تحمل 6 إجراءات؛ قائمة الفترة ووجهات النتائج الثلاثة وعداد غير المسلَّم ليست عليها
1.2.1.1	Add Patient Action	A	فتح محرر المريض	PatientsHubView.xaml:10-11; PatientsHubViewModel.cs:26-33	—
1.2.1.2	Enter Results Action	A	فتح قائمة عمل النتائج	PatientsHubView.xaml:12-13; PatientsHubViewModel.cs:37-44	—
1.2.1.3	Find Patient Action	A	فتح شاشة البحث	PatientsHubView.xaml:14-15; PatientsHubViewModel.cs:48-55	—
1.2.1.4	Period List Action	B	نطاق زمني في البحث والتسليم	PatientSearchViewModel.cs:317-341,569-581; ResultDeliveryViewModel.cs:21-52	لا إجراء فترة على نافذة المرضى
1.2.1.5	Unfinished Results View	B	فلتر ثلاثي الحالة لدي نتيجة	ResultsWorklistView.xaml:10-16; ResultsWorklistViewModel.cs:53-63	موجود في شاشة قائمة العمل لا في نافذة المرضى
1.2.1.6	Unverified Results View	B	فلتر مُراجعة	ResultsWorklistView.xaml:20-25; ResultsWorklistViewModel.cs:65-75	كسابق
1.2.1.7	Unprinted Results View	B	فلتر مطبوعة	ResultsWorklistView.xaml:33-36; ResultsWorklistViewModel.cs:87-97	كسابق
1.2.1.8	Deliver Results Action	A	فتح شاشة التسليم	PatientsHubView.xaml:16-17; PatientsHubViewModel.cs:59-66	—
1.2.1.9	Settle Accounts Action	B	تسوية داخل شاشة تسليم النتائج	DeliveryHandoverViewModel.cs:200-260	لا إجراء مستقل في النافذة الرئيسية
1.2.1.10	Undelivered Results Count	B	عداد غير مسلَّم في قائمة التسليم	ResultDeliveryView.xaml:60; GetUndeliveredResultsQueryHandler.cs:44-49	عدّ اختبارات لكل مريض، لا عداد تجميعي في النافذة
1.2.1.11	Test Registration Statistics Action	B	إحصاءات في وحدة الإحصائيات	StatisticsViewModel.cs:312,417; GetTestCountStatisticsQueryHandler.cs:11-24	في وحدة أخرى؛ المرجع [NOT-SHOWN]
1.2.2	Add-and-Edit Patient Data Screen	B	الشاشة كاملة	Views/Patients/PatientEditorView.xaml:1-474; PatientEditorViewModel.cs:55-1431	كتلة الحساب مختصرة 4 أرقام؛ كتلة الاتصال بلا قنوات
1.2.2.1	Patient Header Block	B	شريط علوي	PatientEditorView.xaml:156-163	بلا تاريخ دخول ولا تاريخ استلام
1.2.2.1.1	Entry Date Field	C	يُولَّد آليًا لا يُعرض	PatientEditorViewModel.cs:867 → CreatePatientCommand.cs:17 → Patient.cs:37	لا حقل قابل للقراءة/التصحيح
1.2.2.1.2	Receipt Date Field	D	عمود موجود وم永远是 null	Patient.cs:39; CreatePatientCommand.cs:26; PatientEditorViewModel.cs:876؛ UpdatePatientCommand.cs:10-25	لا واجهة ولا مسار تعبئة
1.2.2.1.3	Patient Code Field	A	كود المريض للقراءة فقط	PatientEditorView.xaml:156-157; PatientEditorViewModel.cs:268	—
1.2.2.1.4	VIP Action	A	CheckBox VIP	PatientEditorView.xaml:163; PatientEditorViewModel.cs:869,910	—
1.2.2.1.5	Laboratory ID Action	B	مربع نصي معبّأ مسبقًا	PatientEditorView.xaml:158-159; PatientEditorViewModel.cs:518-525	يُولَّد قبل الحفظ (يتعارض مع قاعدة 1.11.1.1)؛ لا يُحفظ عند التعديل (UpdatePatientCommand.cs:10-25)
1.2.2.2	Identity Block	B	كتلة الهوية	PatientEditorView.xaml:156-209	لا رقم بطاقة ولا نوع عينة داخل الكتلة
1.2.2.2.1	Patient Name Field	B	حقل اسم	PatientEditorView.xaml:161-162	لا قائمة منسدلة بأسماء مسجّلة (البحث في شريط منفصل :66-130)
1.2.2.2.2	Sex Radio Group	B	CheckBox واحد	PatientEditorView.xaml:168; PatientEditorViewModel.cs:864	مُبدّل لا مجموعة أزرار
1.2.2.2.2.1	Male Option	A	Sex.Male	PatientEditorView.xaml:168; Domain/Common/Enums/Sex.cs	—
1.2.2.2.2.2	Female Option	B	ضمني عند عدم التحديد	PatientEditorView.xaml:168; PatientEditorViewModel.cs:864	لا عنصر منفصل
1.2.2.2.3	Age Value Field	A	AgeValue	PatientEditorView.xaml:170-171; Patient.cs:22	—
1.2.2.2.4	Age Unit Selector	A	ComboBox (يوم/شهر/سنة)	PatientEditorView.xaml:172-173; PatientEditorViewModel.cs:223-224; AgeUnit.cs:5-9	—
1.2.2.2.5	Mobile and Telephone Field	A	PhoneNumber	PatientEditorView.xaml:175-176; CreatePatientCommandHandler.cs:120	—
1.2.2.2.6	Card Number Field	D	غير موجود	بحث `CardNumber	رقم بطاقةفيsrc= 0؛Patient.cs:13-49`
1.2.2.2.7	Account Type Selector	A	ComboBox أنواع الحساب	PatientEditorView.xaml:184-186; AccountType.cs:5-11	—
1.2.2.2.8	Sample Type Field	C	لا حقل على مستوى المريض	PatientEditorView.xaml:156-187	نوع العينة سطر لكل اختبار (PatientTest.cs:16-24)
1.2.2.2.9	Patient Address Field	A	Address	PatientEditorView.xaml:181-182; PatientEditorViewModel.cs:873	—
1.2.2.2.10	Referring Entity Selector	A	منتقي جهات خارجية	PatientEditorView.xaml:205-208; PatientEditorViewModel.cs:333-346	—
1.2.2.2.10.1	Referring Entity Additional Information Action	D	غير موجود	Views/External/ExternalEntityPickerWindow.xaml:9-19	لا زر معلومات إضافية
1.2.2.2.11	Treating Doctor Selector	A	منتقي طبيب	PatientEditorView.xaml:201-204; PatientEditorViewModel.cs:317-331	—
1.2.2.2.11.1	Treating Doctor Additional Information Action	D	غير موجود	نفس ExternalEntityPickerWindow.xaml:9-19	—
1.2.2.3	Medical History Block	B	كتلة مولّدة من DB	PatientEditorView.xaml:211-224; PatientEditorViewModel.cs:510-514	البذرة تحتوي سطرًا واحدًا فقط (MedicalConditionTypeConfiguration.cs:17 = "حمل") ⇒ 12 سؤالًا من 15 مستحيل الإجابة عنه
1.2.2.3.1	Fasting Toggle	A	صائم	PatientEditorView.xaml:213; Patient.cs:41	—
1.2.2.3.2	Fasting Hours Field	A	ساعات + قاعدة نطاق	PatientEditorView.xaml:214-215; Patient.cs:43,142-145	—
1.2.2.3.3	Diabetes Treatment Toggle	D	غير موجود	MedicalConditionTypeConfiguration.cs:17	لا سطر داء سكر ولا واجهة إنشاء
1.2.2.3.4	Blood-Thinning Treatment Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.5	Gland Treatment Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.6	Antiviral Treatment Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.7	Antibiotic Treatment Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.8	Liver Treatment Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.9	Pregnancy Toggle	B	موجود كشرط عام	PatientEditorView.xaml:218-224; MedicalConditionTypeConfiguration.cs:17	يُعرض لكل الجنس ⇒ خرق قاعدة 1.2.2.3.10
1.2.2.3.10	Imaging Within Two Days Toggle	B	صبغة حديثة فقط	PatientEditorView.xaml:216; Patient.cs:45	لا تمثيل لتاريخ الأشعة فوق الصوتية
1.2.2.3.11	Anaemia Condition Toggle	D	غير موجود	MedicalConditionTypeConfiguration.cs:17	—
1.2.2.3.12	Joint Inflammation Condition Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.13	High Blood Pressure Condition Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.14	Renal Failure Condition Toggle	D	غير موجود	نفس المصدر	—
1.2.2.3.15	Lupus Condition Toggle	D	غير موجود	نفس المصدر	—
1.2.2.4	Notes and Communication Block	B	ملاحظات فقط	PatientEditorView.xaml:226-228	لا SMS/بريد/فاكس/موقع
1.2.2.4.1	Notes Field	B	يُحفظ	PatientEditorView.xaml:226-228; CreatePatientCommandHandler.cs:108	لا يظهر في نافذتي إدخال النتائج والتسليم (لا حقل في ResultsEntryDtos.cs/ResultDeliveryDtos.cs)
1.2.2.4.2	Print Result Password Field	D	غير موجود	بحث `PrintResultPassword	ResultPassword` = 0
1.2.2.4.2.1	Print Result Password Enable Option	D	غير موجود	نفس البحث	—
1.2.2.4.3	Send SMS Field	D	غير موجود	بحث `Sms	Twilio
1.2.2.4.4	Send E-mail Field	D	غير موجود	نفس البحث	—
1.2.2.4.5	Fax Field	D	غير موجود كوجهة مريض	Domain/ExternalEntities/ExternalEntity.cs:25 فقط	—
1.2.2.4.6	Send Action	D	غير موجود	لا أمر إرسال في Features/PatientRegistration/**	—
1.2.2.4.7	Site Upload Action	D	غير موجود	بحث Upload في src = 0	—
1.2.2.4.8	Site Reference Field	D	غير موجود	بحث NATIGH = 0	المرجع [UNVERIFIED]
1.2.2.5	Test Selection Block	B	الكتلة موجودة	PatientEditorView.xaml:387-471	بلا شريط فلترة ولا عدّاد ولا بريد للمجموعة
1.2.2.5.1	Test Filter Bar	D	غير موجود	PatientEditorView.xaml:445-457; GetRegistrationCatalogQueryHandler.cs:30 (فلترة=null)	—
1.2.2.5.1.1	Test Filter Field	D	غير موجود	نفس ما سبق	—
1.2.2.5.1.2	Test Filter Mode Options	D	غير موجود	لا رموز TM/TG/CG	—
1.2.2.5.1.3	Complete Test Menu Action	B	منتقي بروفايل + زر إضافة	PatientEditorView.xaml:392-397; AddProfileToVisitCommandHandler.cs	معادل وظيفي لكن بصيغة منتقي لا قائمة
1.2.2.5.1.4	Test Group Menu Action	B	منتقي مجموعات مخصصة	PatientEditorView.xaml:406-411; AddCustomGroupToVisitCommandHandler.cs:26-116	كسابق
1.2.2.5.2	Master Test List	A	ListView CatalogTests	PatientEditorView.xaml:458-470; PatientEditorViewModel.cs:485-497	—
1.2.2.5.3	Add Select Test Action	A	زر "إضافة"	PatientEditorView.xaml:462-464; PatientEditorViewModel.cs:695-715	—
1.2.2.5.4	Delete Test Action	A	زر ✕	PatientEditorView.xaml:431-433; ApplyVisitDeltasCommandHandler.cs:119-122	الفرق شكلي (نقرة مزدوجة ↔ زر)
1.2.2.5.5	All Tests Action	A	"مسح الكل" مع بوابة أول تسجيل	PatientEditorView.xaml:411; ClearAllTestsCommandHandler.cs:15,55-67	يحقق قاعدة 1.2.2.5.4
1.2.2.5.6	Patient's Tests List	A	SelectedTests	PatientEditorView.xaml:427-444	—
1.2.2.5.6.1	Patient's Tests Count	D	غير موجود	PatientEditorView.xaml:414 عنوان ثابت	—
1.2.2.5.7	Test Group List	B	ComboBox للمجموعات المخصصة فقط	PatientEditorView.xaml:407-409; PatientEditorViewModel.cs:498-502,557-568	TestGroups محمّلة لكن غير مربوطة بعنصر
1.2.2.5.8	Add All Tests In Group Action	A	يضيف كل اختبارات المجموعة	PatientEditorView.xaml:410; AddCustomGroupToVisitCommandHandler.cs:82-104	فرق شكلي
1.2.2.5.9	Group Send E-mail Action	D	غير موجود	لا بريد إلكتروني في الحل	(المرجع نفسه غير متسق الترقيم هنا)
1.2.2.5.10	Sample Type Radio Group	B	5 CheckBox لكل سطر اختبار	PatientEditorView.xaml:435-439; PatientEditorViewModel.cs:189-195	ليست مجموعة أزرار؛ يسمح بتعدد الأنواع (يوافق قاعدة المرجع)
1.2.2.5.10.1	Urine Option	A	IsUrine	PatientEditorView.xaml:435; PatientTest.cs:16	—
1.2.2.5.10.2	Stool Option	A	IsStool	PatientEditorView.xaml:436	—
1.2.2.5.10.3	Blood Option	A	IsBlood	PatientEditorView.xaml:437	—
1.2.2.5.10.4	Semen Option	A	IsSemen	PatientEditorView.xaml:438	—
1.2.2.5.10.5	CSF Option	A	IsCsf	PatientEditorView.xaml:439	—
1.2.2.5.11	Taken Outside Lab Flag	A	خارج المعمل + يظهر كملاحظة في التقرير	PatientEditorView.xaml:440; Infrastructure/Printing/ReportContentBuilder.cs:334-342	يحقق قاعدة 1.2.2.5.10
1.2.2.6	Action Band	B	شريط إجراءات	PatientEditorView.xaml:39-53	تركيبة مختلفة عن الموثق
1.2.2.6.1	Save Registration Action	A	"حفظ"	PatientEditorView.xaml:41; PatientEditorViewModel.cs:766-792; CreatePatientCommandHandler.cs:209	—
1.2.2.6.2	Add Entry Action	B	"جديد" يفرغ النموذج	PatientEditorView.xaml:40; PatientEditorViewModel.cs:717-764	لا يضيف سجلاً مستقلاً
1.2.2.6.3	Edit Entry Action	D	غير موجود	PatientEditorView.xaml:39-53	التحرير عبر "فتح" من نتائج البحث
1.2.2.6.4	Undo Action	A	"تراجع"	PatientEditorView.xaml:43; PatientEditorViewModel.cs:1037-1048	—
1.2.2.6.5	Print Barcode Action	B	باركود واحد	PatientEditorView.xaml:45; PrintBarcodeCommandHandler.cs:40-44; BarcodeService.cs:44-92	لا تُنتج طقم الملصقات (انظر 1.14)
1.2.2.6.6	Print Receipt Action	A	إيصال بهوية وأسطر ومجاميع وخصم ومدفوع	PatientEditorView.xaml:47; PatientBillingDtos.cs:33-42; GetPatientReceiptQueryHandler.cs:38-47	—
1.2.2.6.7	Patient Invoice Action	A	فاتورة برقم وتاريخ	PatientEditorView.xaml:48; PatientBillingDtos.cs:44-55	—
1.2.2.6.8	Patient Envelope Print Action	C	موجود في شاشة كشف النتائج	Views/Patients/PatientResultSheetView.xaml:84-85; PatientResultSheetViewModel.cs:201-214	موضع مختلف (خارج شريط إجراءات المحرر)
1.2.2.6.9	Modify Action	D	غير موجود	لا زر تعديل في PatientEditorView.xaml:39-53	المحرر نفسه وضع التحرير
1.2.2.6.10	Follow-up Action	D	غير موجود	بحث `FollowUp	متابع` = 0 نتيجة ذات معنى
1.2.2.6.11	Work Sheet Action	A	"ورقة العمل"	PatientEditorView.xaml:46; PrintWorkSheetCommandHandler.cs	ارتباط خارجي (وحدة أخرى) — فُحص المسار فقط
1.2.2.6.12	ID Card Action	D	غير موجود	بحث `كارنيه	IdCard` = 0
1.2.2.6.13	Patient Screen Action	D	غير موجود	PatientEditorView.xaml:39-53	—
1.2.2.6.14	Main Point Action	D	غير موجود	نفس الموضع	—
1.2.2.6.15	Patient Window Action	D	غير موجود	نفس الموضع	—
1.2.2.6.16	Lab Results Action	D	غير موجود في الشريط	ResultsWorklistViewModel يحتويه على hub المرضى لا المحرر	—
2.3 — الوحدة 1.3 (حساب المريض)
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.3.1	Account Panel	A	لوحة أسفل شاشة المريض	PatientEditorView.xaml:19-53; PatientEditorViewModel.cs:1408-1430	—
1.3.1.1	Total of Tests Field	A	إجمالي التحاليل	PatientEditorView.xaml:23-24; PatientBillingReader.cs:26,31; PatientAccountCalculator.cs:17-26	—
1.3.1.2	Discount Value Field	A	إدخال + عرض الخصم	PatientEditorView.xaml:25-26,34-35; PatientBillingReader.cs:33-35	—
1.3.1.3	Discount Percentage Field	C	الخصم مطلق فقط	PatientEditorViewModel.cs:1299-1308; RecordPaymentCommandHandler.cs:42-52	النسبة حدّ صلاحيات لا نمط خصم
1.3.1.4	Paid Amount Field	A	دفعة	PatientEditorView.xaml:32-33; PatientEditorViewModel.cs:1282-1329	—
1.3.1.4.1	Surcharge Entry Convention (+)	C	+50 يُقرأ كـ 50 مدفوع	PatientEditorViewModel.cs:1292; RecordPaymentCommandHandler.cs:57-63	مسار الإضافة الحقيقية موجود (RecordExtraChargeCommandHandler.cs:43-51) لكنه غير موصول بهذا الحقل
1.3.1.4.2	Enter Key Account Operation	D	غير موجود	بحث KeyDown|PreviewKeyDown|InputBindings في src/TopLab.Presentation = 0	لا معالجة لوحة مفاتيح إطلاقًا
1.3.1.5	Paid Previously Field	B	مغطى بـالمدفوع	PatientEditorView.xaml:27-28; PatientBillingReader.cs:32	لا رقم مستقل
1.3.1.6	Amount After Discount Field	D	غير موجود	PatientBillingDtos.cs:3-13 (4 حقول فقط)	—
1.3.1.7	Total Collected Field	B	نفس حقل المدفوع	PatientEditorView.xaml:27-28	لا رقم مستقل
1.3.1.8	Remainder for Customer Field	B	الباقي بإشارة	PatientEditorView.xaml:29-30; PatientAccountCalculator.cs:35-40	رصيد واحد يخدم الطرفين
1.3.1.9	Remainder for Laboratory Field	C	موجود فقط في لوحة التسليم	Views/Patients/DeliveryHandoverView.xaml:23-24; GetDeliveryAccountQueryHandler.cs:39-40	لا حقل في لوحة الحساب
1.3.1.10	Confirm Payment Action	B	"تسجيل مدفوع"	PatientEditorView.xaml:36; PatientEditorViewModel.cs:124,441	لا زر "موافق" ولا حوار تأكيد
1.3.1.11	Net Settlement Action	C	"تسوية كاملة" بلا حوار	PatientEditorView.xaml:37; PatientEditorViewModel.cs:1331-1358; SettleAccountInFullCommandHandler.cs:67-71	خرق قاعدة 1.3.1.10 (لا تأكيد، والرصيد الصفري خطأ)
1.3.1.12	Account Summary Action	A	"عرض الحساب"	PatientEditorView.xaml:31; PatientEditorViewModel.cs:1136-1151	—
1.3.1.13	Custody Action	D	غير موجود	بحث (?i)custody|حوافظ = 0	المرجع [UNVERIFIED]
1.3.1.14	Account Back Action	C	"تراجع" يعيد تحميل النموذج	PatientEditorView.xaml:43; PatientEditorViewModel.cs:1037-1048	لا عملية حساب نوافذية للخروج منها
1.3.2	Patient Account Screen	A	شاشة كاملة	Views/Patients/PatientAccountView.xaml; PatientAccountViewModel.cs	—
1.3.2.1	Account Transaction Grid	A	ListView معاملات	PatientAccountView.xaml:100-136; PatientAccountViewModel.cs:231	—
1.3.2.1.1	Paid Date Column	A	OperationAtUtc	PatientAccountView.xaml:104	—
1.3.2.1.2	Money Column	A	Amount+DiscountAmount	PatientAccountView.xaml:106-107	—
1.3.2.1.3	User Name Column	A	ReceivedByUserName	PatientAccountView.xaml:120; PatientBillingReader.cs:69,86	—
1.3.2.1.4	Edit Date Column	D	غير موجود	PatientBillingDtos.cs:22-31 (لا تاريخ تعديل)	العمود موجود في الكيان فقط (AuditableEntity.cs:20)
1.3.2.1.5	Color Code Column	D	غير موجود	بحث ColorCode = 0	—
1.3.2.2	Modify Amount Action	C	"قيد تصحيحي" يُنشئ قيدًا جديدًا	PatientAccountView.xaml:80; PatientAccountViewModel.cs:415-432; RecordCorrectionCommandHandler.cs:44-52	لا تعديل للمبلغ المختار
1.3.2.3	Delete Transaction Action	C	"إلغاء عملية" (إبطال)	PatientAccountView.xaml:82; PatientAccountViewModel.cs:459-495; PaymentOperation.cs:78-81	الصف يبقى بعلامة ملغاة بدل الحذف
1.3.2.4	Pay Action	A	"تسجيل مدفوع"	PatientAccountView.xaml:73; PatientAccountViewModel.cs:249-291	—
1.3.2.5	Refund Action	D	غير موجود	بحث (?i)refund|استرداد = 0؛ OperationType.cs:4-7 (3 أنواع فقط)	Domain لا يستطيع 표현 استردادًا
1.3.2.6	Add Amount Action	A	"مبلغ إضافي" على جانب المدين	PatientAccountView.xaml:81; RecordExtraChargeCommandHandler.cs:43-51	—
1.3.2.7	Clear Account Action	D	غير موجود	بحث ClearAccount|تصفير = 0	—
1.3.2.8	Account Patient List Action	B	"رجوع" → المحرر	PatientAccountView.xaml:31; PatientAccountViewModel.cs:405-412	يعود إلى المحرر لا إلى شاشة قائمة مرضى مستقلة
2.4 — الوحدة 1.4 (تسليم النتائج وتسوية الحساب)
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.4.1	Deliver Patient Results Screen	B	منقسم إلى شاشتين	Views/Patients/ResultDeliveryView.xaml; DeliveryHandoverView.xaml; DeliveryHandoverViewModel.cs:155	شرط «الباقي مع المعمل» غير مُطبَّق (لوحة الحساب دائمة) ⇒ خرق قاعدة 1.4.1
1.4.1.1	Delivery Filter Block	B	فلتر تاريخ فقط	ResultDeliveryView.xaml:7-15	—
1.4.1.1.1	Delivery Date Filter	A	From/To مرتبطة بالاستعلام	ResultDeliveryView.xaml:9,11; ResultDeliveryViewModel.cs:104	—
1.4.1.1.2	Delivery Name Filter	D	غير موجود	GetUndeliveredResultsQuery.cs:8-12 (4 حقول فقط)	—
1.4.1.1.3	Delivery Status Filter	D	غير موجود	ResultDeliveryView.xaml:7-15; DeliveryHandoverView.xaml:29-36	الفلاتر الثلاثية على شاشة قائمة العمل فقط
1.4.1.1.4	Delivery Department Filter	D	غير موجود	لا مفهوم قسم في src/TopLab.Domain	—
1.4.1.2	Patient Scope Block	D	غير موجود	ResultDeliveryView.xaml (73 سطرًا، بلا ComboBox)	—
1.4.1.2.1	Patient Source Selector	D	غير موجود	AccountType.cs:4-11 (مستخدم في المحرر فقط)	بلا فلتر AccountType في الاستعلام
1.4.1.2.2	Patient Group Selector	D	غير موجود	Patient.cs:35 (VIP) موجود لكن بلا فلتر	—
1.4.1.2.3	Patients Today Panel	D	غير موجود	ResultDeliveryView.xaml:60 يعدّ اختبارات غير مسلَّمة	—
1.4.1.2.3.1	Delivery Patient Grid	C	أعمدة مختلفة	ResultDeliveryView.xaml:49-71	لا كود ولا نوع ولا جهة إحالة ولا عمر
1.4.1.3	Delivery Result Block	A	شبكة النتائج + أدوات التسوية	DeliveryHandoverView.xaml:68-101; GetDeliveryGridQueryHandler.cs:33-52	—
1.4.1.3.1	Delivery Result List Grid	B	4 صناديق قراءة + صندوق تسليم قابل للتحرير	DeliveryHandoverView.xaml:68-101	finish/verify/print/export ليست خيارات مستقلة
1.4.1.3.1.1	Test Abbreviation Column	A	الرمز	DeliveryHandoverView.xaml:93; GetDeliveryGridQueryHandler.cs:43	—
1.4.1.3.1.2	P Column	D	غير موجود	DeliveryHandoverView.xaml:83-100	المرجع [UNVERIFIED]
1.4.1.3.1.3	T Column	D	غير موجود	نفس الموضع	المرجع [UNVERIFIED]
1.4.1.3.1.4	Result Column	A	النتيجة	DeliveryHandoverView.xaml:94	—
1.4.1.3.1.5	Status Column	D	DTO يحمله وغير مربوط	ResultDeliveryDtos.cs:31; GetDeliveryGridQueryHandler.cs:46	—
1.4.1.3.1.6	Delivery Finish Column	C	مُدخل للقراءة فقط	DeliveryHandoverView.xaml:95; PatientTest.cs:38-64	لا حالة "finished" في Domain
1.4.1.3.1.7	Delivery Verify Column	A	مُعتمد	DeliveryHandoverView.xaml:96	—
1.4.1.3.1.8	Delivery Print Column	A	مطبوع	DeliveryHandoverView.xaml:97	—
1.4.1.3.1.9	Delivery Export Column	D	غير موجود	ResultDeliveryDtos.cs:26-37 لا عضو export	العلم موجود في Domain (PatientTest.cs:62) ويُكتب من شاشة التصدير فقط
1.4.1.3.1.10	See Report Action	D	غير موجود	DeliveryHandoverViewModel.cs:73-74 (أمران فقط)	معادل على شاشة قائمة العمل
1.4.1.4	Delivery Action Block	B	3 إجراءات فقط	ResultDeliveryView.xaml:12-14; DeliveryHandoverView.xaml:33,35	10 إجراءات ناقصة
1.4.1.4.1	Patient Account Action	B	لوحة حساب دائمة	DeliveryHandoverView.xaml:17-26; GetDeliveryAccountQueryHandler.cs:32-40	الأرقام صحيحة لكن كتلة ثابتة لا إجراء، وتُعرض دائمًا ⇒ خرق قاعدة 1.4.1
1.4.1.4.2	Refresh Action	A	"عرض"	ResultDeliveryView.xaml:12; ResultDeliveryViewModel.cs:96-120	—
1.4.1.4.3	Delivery List Print Action	C	يطبع قصاصات تقارير جماعية	ResultDeliveryView.xaml:13; GroupPatientSlipsViewModel.cs:192; GroupPatientSlipPrintingService.cs:81-82	المخرَج ليس «قائمة التسليم»؛ لا كاتب DeliveryList في Infrastructure/Printing/
1.4.1.4.4	Preview Action	D	غير موجود	PdfPreviewService.cs غير مربوط بشاشتي التسليم	—
1.4.1.4.5	Copy Action	D	غير موجود	بحث Clipboard = 0	—
1.4.1.4.6	Notes Action	D	غير موجود	DeliveryHandoverView.xaml:29-36	الملاحظات تُعرض على كشف النتائج فقط
1.4.1.4.7	Delivery Patient Data Action	D	غير موجود	DeliveryHandoverViewModel.cs:73-74	—
1.4.1.4.8	Combined Report Action	D	غير موجود من التسليم	CombinedReportView.xaml يُفتح من PatientVisitHistoryViewModel.cs:238-239	—
1.4.1.4.9	Patient Medical History Action	D	غير موجود من التسليم	HistoryReportsView.xaml يُفتح من PatientsHubViewModel.cs:79-87	—
1.4.1.4.10	Delivery Empty Report Action	D	غير موجود من التسليم	BlankReportView.xaml يُفتح من PatientsHubViewModel.cs:69-77	—
1.4.1.4.11	Delivery Envelope Print Action	D	غير موجود من التسليم	PatientResultSheetViewModel.cs:201-214	—
1.4.1.4.12	Send SMS Action	D	غير موجود	بحث (?i)sms = نتائج سالب�� فقط	—
1.4.1.4.13	Delivery Main List Action	A	"رجوع" → hub المرضى	ResultDeliveryView.xaml:14; ResultDeliveryViewModel.cs:39	—
2.5 — الوحدة 1.5 (تعديل بيانات المريض والتحاليل)
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.5.1	Modification Path One	B	المسار قائم عدا اختيار يوم التسجيل	ShellViewModel.cs:189,288-291 → PatientsHubViewModel.cs:26-33 → PatientEditorView.xaml:66-130,89-91 → PatientEditorViewModel.cs:1116-1130,766-792	قائمة المرضى شريط بحث مدمج لا شاشة مستقلة
1.5.1.1	Patient Icon Action	A	مسار "المرضى" في الشريط	ShellViewModel.cs:189,288-291	فرق شكلي (نص لا أيقونة)
1.5.1.2	Patient List Open Action	B	"بحث عن مريض"	PatientEditorView.xaml:66-72; PatientEditorViewModel.cs:147,1080	بلا نطاق يوم تسجيل
1.5.1.3	Registration Day Selector	D	غير موجود	لا DatePicker في PatientEditorView.xaml؛ SearchPatientsQuery بلا تاريخ	خرق قاعدة 1.5.1.3
1.5.1.4	Edit Patient Action	A	"فتح" → وضع التحرير	PatientEditorView.xaml:89-91; PatientEditorViewModel.cs:1116-1130,572-625	—
1.5.1.5	Patient Selection Action	A	اختيار الصف	PatientEditorView.xaml:89-91; PatientEditorViewModel.cs:1118-1129	مدموج مع زر التحرير (شكلي)
1.5.1.6	Modification Save Action	A	"حفظ" يحدّث البيانات والاختبارات	PatientEditorView.xaml:41; PatientEditorViewModel.cs:901,929,952-988	—
1.5.2	Modification Path Two	B	بحث → شاشة زيارات للقراءة	PatientsHubView.xaml:14-15; PatientSearchView.xaml:126-134; PatientVisitHistoryViewModel.cs:115-150	لا يصل إلى السجل القابل للتحرير
1.5.2.1	Search Action	A	فتح البحث	PatientsHubView.xaml:14-15; PatientSearchViewModel.cs:569-581	—
1.5.2.2	Patient Data Entry Action	B	يعرض بيانات المريض تفصيليًا	PatientSearchView.xaml:126-134; PatientVisitHistoryView.xaml:75-131	شاشة العرض هي ليست شاشة التحرير
1.5.3	Patient Data Access Action	D	غير موجود	ResultsWorklistView.xaml:86-94; SimpleResultEntryView.xaml:22-55	لا مسار من شاشة إدخال النتائج إلى بيانات المريض
2.6 — الوحدة 1.6 (إدخال نتائج التحاليل) — باستثناء 1.6.1.4
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.6.1	Result Entry Screen	A	قائمة عمل + شاشات إدخال	PatientsHubView.xaml:12-13; MainWindow.xaml:108-114; ResultPrintCoordinator.cs:41-66	التقسيم شكلي
1.6.1.1	Patient List Panel	C	الشبكة صفوفها اختبارات	ResultsWorklistView.xaml:73-111; GetResultWorklistQueryHandler.cs:110-132	لا لوحة مرضى
1.6.1.1.1	Result Entry Patient Grid	C	أعمدة مختلفة	ResultsWorklistView.xaml:95-109	لا كود ولا نوع ولا جهة إحالة ولا عمر
1.6.1.1.2	Patient Name Click Action	D	غير موجود	ResultsWorklistView.xaml:95 (عمود نصي عادي)	—
1.6.1.1.3	Patient Count Indicator	C	غير معروض	ResultsWorklistViewModel.cs:142-146 (عدد الصفوف، غير مربوط)	—
1.6.1.2	Test Results Grid	B	شبكة الاختبارات (للقراءة)	ResultsWorklistView.xaml:73-111; ResultsWorklistView.xaml.cs:15-22	8 أعمدة من 15 مفقودة
1.6.1.2.1	Result Entry Test Abbreviation Column	A	الرمز	ResultsWorklistView.xaml:98; ResultsEntryDtos.cs:10	—
1.6.1.2.2	Result Entry P Column	D	غير موجود	ResultsWorklistView.xaml:85-110	المرجع [UNVERIFIED]
1.6.1.2.3	Result Entry T Column	D	غير موجود	نفس الموضع	المرجع [UNVERIFIED]
1.6.1.2.4	Result Entry Result Column	A	النتيجة	ResultsWorklistView.xaml:103; PatientTest.cs:32	—
1.6.1.2.5	Result Entry Status Column	D	غير موجود	ResultsWorklistView.xaml:99,108 (نوع النتيجة / حالة المريض)	لا عمود لحالة الاختبار
1.6.1.2.6	Abnormal Flag Column	B	العلم	ResultsWorklistView.xaml:104; ResultFlagComputer.cs:37-51; ResultFlag.cs:3-8	لا حالة Critical (Low/High فقط) ويعرض رقم التعداد
1.6.1.2.7	Low and High Column	D	غير موجود	ResultsEntryDtos.cs:5-22 بلا أعضاء مدى	—
1.6.1.2.8	Unit Column	D	غير موجود	PatientTest.cs:10-64 بلا وحدة	الوحدة للبريوفايل فقط
1.6.1.2.9	Normal Range Column	D	غير موجود	نفس الاستعلام	—
1.6.1.2.10	Finish Flag	D	غير موجود	PatientTest.cs:28,42,48,56,62 لا "finished"	—
1.6.1.2.11	Verify Flag	B	معروض للقراءة فقط	ResultsWorklistView.xaml:105; PatientTest.cs:42,166-176	لا مسار اعتماد من الواجهة (بحث ReviewResultCommand في Presentation = 0)
1.6.1.2.12	Result Print Flag	B	معروض للقراءة فقط	ResultsWorklistView.xaml:106; MarkResultPrintedCommandHandler.cs:35-73	الأمر لا يُستدعى من الواجهة
1.6.1.2.13	Export Flag	B	علم في Domain	PatientTest.cs:62,203-207; ExportPatientReportPdfCommandHandler.cs:175-181	يُضبط لكل المريض لا لكل اختبار، ولا يظهر في الشبكة
1.6.1.2.14	Per-Test Comment Field	B	في شاشة الإدخال البسيط	SimpleResultEntryView.xaml:42-45; EnterResultCommandHandler.cs:89	لا يظهر في الشبكة ولا في التقرير المطبوع العادي (ReportContentBuilder.cs:373-387)
1.6.1.2.15	Double-Click Test Action	A	نقرتان تفتحان شاشة الإدخال	ResultsWorklistView.xaml:79; ResultsWorklistView.xaml.cs:15-22; ResultsWorklistViewModel.cs:196-236	—
1.6.1.3	Entry Action Block	B	موزّع على ثلاث شاشات	ResultsWorklistView.xaml:37; SimpleResultEntryView.xaml:51-55; ProfileEntryView.xaml:131-140; CultureEntryView.xaml:206-213	لا كتلة إغلاق على شاشة النتائج نفسها
1.6.1.3.1	Save Results Action	B	"حفظ" على شاشات الإدخال	SimpleResultEntryView.xaml:54; ProfileEntryView.xaml:132	لا حفظ على شاشة قائمة العمل
1.6.1.3.2	Print Preview Action	B	معاينة للبروفايل/الثقافة	ProfileEntryView.xaml:136; ProfileEntryViewModel.cs:407-464; PdfPreviewService.cs:31	لا معاينة للاختبار البسيط
1.6.1.3.3	Result Report Print Action	B	طباعة للبروفايل/الثقافة	ProfileEntryView.xaml:135; ReportPrintingService.cs:37-85; ReportContentBuilder.cs:319-351; ReportDocumentContent.cs:107-110	لا طباعة للاختبار البسيط؛ ولا علم L/H/C مطبوع (ReportItemKeys.cs:12-35)
1.6.1.5	Report Entry Grid	B	شبكة للبريوفايل فقط	Views/Patients/ProfileEntryView.xaml:60-94; ProfileEntryViewModel.cs:28-61	الاختبار البسيط بحقل واحد بلا شبكة؛ بلا عمود مدى ولا تعليق لكل صف
1.6.1.5.1	Report Entry Test Name Column	A	المادة	ProfileEntryView.xaml:77	—
1.6.1.5.2	Report Entry Result Column	A	القيمة	ProfileEntryView.xaml:78; SaveProfileResultsCommandHandler.cs:130-138	—
1.6.1.5.3	Report Entry Unit Column	A	الوحدة	ProfileEntryView.xaml:79; ProfileResultItem.cs:22	—
1.6.1.5.4	Report Entry Flag Column	C	SelectedIndex + ComboBox	ProfileEntryView.xaml:83-90; ProfileResultFlag.cs:3-7 (Low=0, High=1)	دلالة معطوبة: العنصر الفارغ يُخزَّن Low، و"مرتفع" خارج المدى، ولا حالة Normal
1.6.1.5.5	Report Entry Normal Ranges Column	C	DTO يحمل المدى دون عرض	GetProfileEntryGridQueryHandler.cs:62-64,78-92; ProfileEntryView.xaml:76-93	—
1.6.1.5.6	Report Entry Verified Flag	B	مُعتمد للقراءة	ProfileEntryView.xaml:91	الاعتماد على مستوى البروفايل لا الصف
1.6.1.5.7	Report Entry Print Flag	B	مطبوع للقراءة	ProfileEntryView.xaml:92	لا مربع تحديد لكل صف
1.6.1.5.8	Report Entry Comment Field	B	تعليق واحد للشاشة	ProfileEntryView.xaml:25-28; SaveProfileResultsCommandHandler.cs:94	لا تعليق لكل صف
1.6.1.5.9	Urine Sample Sub-Row Panel	D	غير موجود	بحث UrineSample|عينة البول|No Sample|Acetone = 0	—
1.6.1.5.9.1	Urine Sample Sub-Row	D	غير موجود	نفس البحث	—
1.6.1.5.9.2	Sub-Result Row	D	غير موجود	نفس البحث	—
1.6.1.5.9.3	Missing Sample Marker	D	غير موجود	نفس البحث	—
1.6.1.6	Test Entry Variants	B	3 أشكال حسب ResultKind	ResultsWorklistViewModel.cs:207-231; ResultKind.cs	2 من 6 أشكال فقط
1.6.1.6.1	Single Test Entry	B	حقل نتيجة واحد	SimpleResultEntryView.xaml:30-33; SimpleResultEntryViewModel.cs:47-57	بلا وحدة ولا مدى (المدى لم يُحمَّل: :90-99)
1.6.1.6.2	Profile Test Entry	B	صفوف تحاليل البروفايل	ProfileEntryView.xaml:60-94; GetProfileEntryGridQueryHandler.cs:48-65	بلا مدى لكل صف
1.6.1.6.3	Paired Control And Patient Entry	D	غير موجود	بحث ControlValue|Prothrombin|INR = 0	ProfileResultItem.cs:14-34 قيمة واحدة
1.6.1.6.4	Derived Value Entry	D	غير موجود	بحث Derived = 0	—
1.6.1.6.5	Multi-Unit Entry	D	غير موجود	ProfileResultItem.cs:22 وحدة واحدة	—
1.6.1.6.6	Qualitative Result Entry	B	ResultValue نص حر	PatientTest.cs:32; EnterResultCommandHandler.cs:59-62	لا تمثيل نوعي ولا علم للقيم غير الرقمية (ResultFlagComputer.cs:69-78)
1.6.1.7	Report Screen Actions	B	مجموعة أزرار	ProfileEntryView.xaml:131-140; SimpleResultEntryView.xaml:51-55	لا تاريخ/رجوع/قائمة رئيسية
1.6.1.7.1	Save Report Action	A	"حفظ"	ProfileEntryView.xaml:132; ProfileEntryViewModel.cs:251-287	—
1.6.1.7.2	Report Screen Print Action	B	طباعة للبروفايل/الثقافة	ProfileEntryView.xaml:135; ReportPrintingService.cs:37-85	لا طباعة للاختبار البسيط
1.6.1.7.3	Report Screen Preview Action	B	معاينة للبروفايل	ProfileEntryView.xaml:136; ProfileEntryViewModel.cs:407-464	لا معاينة للاختبار البسيط
1.6.1.7.4	Report Patient History Action	D	غير موجود في شاشة الاختبار	ProfileEntryView.xaml:131-140; SimpleResultEntryView.xaml:51-55	موجود في شاشة التقرير المجمّع فقط
1.6.1.7.5	Report Screen Back Action	D	غير موجود	INavigationService.cs:5-19 (لا back stack)	—
1.6.1.7.6	Report Screen Main List Action	D	غير موجود	لا زر في ProfileEntryView.xaml:131-140	—
2.7 — الوحدة 1.6.1.4 (شاشة إعدادات التقرير)
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.6.1.4	Report Settings Screen	B	صفحات في الإعدادات لا حوار من شاشة النتائج	Views/Settings/ReportSettingsView.xaml:1-133; ReportSettingsViewModel.cs:16-300; لا تنقّل إليه من Views/Patients	4 شاشات بدل حوار واحد
1.6.1.4.1	Report Title Block	C	نصوص على مستوى عناصر التقرير	Domain/Settings/ReportItemKeys.cs:12-35; ReportItemTemplatesView.xaml:65-72	لا عنوان شاشة/رئيس/عنوان فرعي-فرعي
1.6.1.4.1.1	Report Screen Title Field	D	غير موجود	ReportContentBuilder.cs:153,252,488 (نصوص ثابتة)	—
1.6.1.4.1.2	Report Main Title Field	D	غير موجود	نفس المصدر	—
1.6.1.4.1.3	Report Sub Title Field	C	قابل للتعديل لكن مُطفأ إجباريًا	ReportItemTemplatesViewModel.cs:58; ReportSettingsViewModel.cs:195-211 → UpdateReportSettingsCommandHandler.cs:31 (SetPrintOptions(false,false))	لا يمكن تفعيله من الواجهة
1.6.1.4.1.4	Report Sub Sub-title Field	D	غير موجود	ReportPageComposer.cs:133,164-177 (3 مستويات فقط)	—
1.6.1.4.1.5	Reference Range Field	B	بادئة المدى قابلة للتعديل	ReportItemTemplate.cs:47; ReportContentBuilder.cs:718-726	القيم من بيانات لا من إعداد
1.6.1.4.1.6	Report Comment Field	B	Label + FreeTextOverride	ReportItemKeys.cs:35; ReportItemTemplate.cs:56; ReportContentBuilder.cs:242-249	على شاشة "عناصر التقرير"
1.6.1.4.1.7	Report Normal Range Field	B	NormalRangeMarker	ReportSettings.cs:48; ReportSettingsView.xaml:65-66; ReportContentBuilder.cs:39,718-726	بادئة لا نص مستقل
1.6.1.4.1.8	Main Symbol Field	C	MarkerSymbol عام لكل عنصر	ReportItemTemplate.cs:75; ReportPageComposer.cs:62	لا حقل مخصص
1.6.1.4.1.9	Sub Symbol Field	C	نفس الآلية	نفس المصدر	—
1.6.1.4.1.10	Sub-title Symbol Field	C	نفس الآلية	ReportPageComposer.cs:164-172	—
1.6.1.4.2	Report Display Option Block	B	موزّع على شاشتين	ReportSettingsView.xaml:68-76; ReportItemTemplatesView.xaml:67	خياران مفقودان والثالث عام
1.6.1.4.2.1	Show Abnormal Background Option	A	سلسلة كاملة حتى الرسم	ReportSettings.cs:25,29; ReportSettingsView.xaml:68,71-72; ReportContentBuilder.cs:656-673; ReportPageComposer.cs:296-299	—
1.6.1.4.2.2	Print Status Flag Option	D	غير موجود	ReportContentBuilder.cs:319-351 (لا نص حالة)؛ ResultFlag.cs:3-8 بلا Critical	—
1.6.1.4.2.3	Print Patient History Automatically Option	B	HistoryAutoDisplayEnabled	ReportSettings.cs:33; ReportSettingsView.xaml:76; AutoInsertHistoryCommandHandler.cs:37,44-50	عام لكل التقارير ⇒ خرق قاعدة 1.6.1.4.2.3
1.6.1.4.3	Report Typography Block	C	على شاشتين	ReportSettingsView.xaml:101-106; ReportItemTemplatesView.xaml:70-71	لا خطوط رموز ولا محاذاة ولا توسيع خلايا
1.6.1.4.3.1	Symbol Font Selectors	D	غير موجود	Printing/ReportDocument.cs:20-34 (عائلة واحدة)	—
1.6.1.4.3.2	Vertical Spacing Fields	C	تباعد وإزاحة على مستوى العنصر	ReportItemTemplate.cs:62,69; ReportPageComposer.cs:63-65,309-312	"مسافة بعد العنوان الفرعي" ثابتة (PaddingTop(8))
1.6.1.4.3.3	Normal Result Font Selector	B	منتظور خط عام	ReportSettingsView.xaml:101-103; ReportPdfWriter.cs:91	حجم الخط لا يُطبَّق على التقرير (ReportDocument.cs:24,33 = 12pt)
1.6.1.4.3.4	Text Alignment Control	D	غير موجود	ReportPageComposer.cs:133-319 محاذاة ثابتة	—
1.6.1.4.3.5	Cell Expansion Control	D	غير موجود	ReportPageComposer.cs:179-209	—
1.6.1.4.3.6	Offset Amount Control	A	LeftOffsetCm حتى الرسم	ReportItemTemplate.cs:69; ReportItemTemplatesView.xaml:71; ReportPageComposer.cs:291-294	—
1.6.1.4.4	Report Output Block	C	موزّع على 3 شاشات	ReportSettingsView.xaml:128-130; SystemSettingsView.xaml:31-33; PatientResultSheetView.xaml:89	لا كتلة إخراج واحدة
1.6.1.4.4.1	Printing Target Selector	B	منتقي طابعة فعلي	PrinterAssignment.cs:6-16; SystemSettingsView.xaml:31-33; ReportPrintingService.cs:59-70	في إعدادات النظام، عام
1.6.1.4.4.2	Save Settings Action	A	حفظ كامل	ReportSettingsView.xaml:129; ReportSettingsViewModel.cs:175-237; UpdateReportSettingsCommandHandler.cs:25-41	(يُصفّر ScientificFlagين ضمنيًا: PrintGroupSubTitle, SuppressReprintMessage)
1.6.1.4.4.3	Export Data Action	C	تصدير PDF حقيقي	PatientResultSheetViewModel.cs:127-174; PatientReportPdfExporter.cs:42-71	يتجاهل الإعدادات المحفوظة (PatientReportPdfExporter.cs:47 CreateDefault())، وليس إجراءً في شاشة الإعدادات
1.6.1.4.4.4	Close Action	B	"القائمة الرئيسية"	ReportSettingsView.xaml:9-10; ReportSettingsViewModel.cs:69	ينتقل للوحة الإعدادات بدل إغلاق حوار؛ بلا إلغاء
2.8 — الوحدات 1.7 / 1.8 / 1.9
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.7.1	Combined Report Screen	A	شاشة + مخرج مطبوع	Views/Patients/CombinedReportView.xaml:1-116; BuildCombinedReportCommandHandler.cs:23-255; ReportContentBuilder.cs:276-510	—
1.7.1.1	Patient Tests In Our Lab List	A	CombinableTests	CombinedReportView.xaml:58-82; GetCombinableTestsQueryHandler.cs:33-49	—
1.7.1.2	Available Tests In Report List	B	معاينة تُبنى بالأمر	CombinedReportView.xaml:85-109; CombinedReportViewModel.cs:194-228	لا قائمة أعضاء قبل البناء ⇒ خرق مسار العمل
1.7.1.3	Add Test To Report Action	B	مربع تحديد + زر بناء	CombinedReportView.xaml:77,27; BuildCombinedReportCommandHandler.cs:53-59,153	لا نقر مزدوج ولا سهم
1.7.1.4	Reorder Report Tests Action	A	تحريك فعلي	CombinedReportViewModel.cs:374-402	CombinedReportSelection.MoveUp/MoveDown شيفرة ميتة
1.7.1.4.1	Move Test Up Action	A	CombinableTests.Move	CombinedReportViewModel.cs:374-387	—
1.7.1.4.2	Move Test Down Action	A	CombinableTests.Move	CombinedReportViewModel.cs:389-402	—
1.7.1.5	Combined Report Option Block	D	غير موجود	CombinedReportView.xaml:26-34 (7 أزرار بلا خيارات)	الخياران نُقلوا إلى ReportSettings العامة
1.7.1.5.1	Print Sub Title Option	B	الرسم موجود والتفعيل مستحيل	ReportSettings.cs:36; ReportContentBuilder.cs:300-314; ReportSettingsViewModel.cs:195-210	لا يمكن تفعيله
1.7.1.5.2	Print Printed Test Again Option	B	محسوب في الطباعة الجماعية فقط	ReportSettings.cs:39; ExecuteBulkPrintCommandHandler.cs:59-68	لا فحص إعادة طباعة في مسار التقرير المجمّع
1.8.1	Empty Report Screen	A	شاشة	Views/Patients/BlankReportView.xaml:1-79; ReportContentBuilder.cs:512-549	—
1.8.1.1	Empty Report Open Action	B	يفتح بـpatientId = 0	PatientsHubViewModel.cs:69-77; BlankReportViewModel.cs:79-93	لا منتقي مريض ⇒ شاشة خطأ دائمًا
1.8.1.2	Enter Data And Print Option	D	غير موجود	BlankReportView.xaml:66-71 (TextBlock للقراءة)؛ ReportDtos.cs:96-104	لا حقول إدخال في الواجهة أو DTO
1.8.1.3	Patient Data Sheet Option	A	يطبع ورقة بيانات المريض فقط	PrintBlankReportCommandHandler.cs:44-54; ReportContentBuilder.cs:512-548	—
1.9.1	Culture Screen	A	شاشة	Views/Patients/CultureEntryView.xaml:1-218; ResultsWorklistViewModel.cs:224-231	—
1.9.1.1	Culture Data Block	A	6 حقول	CultureEntryView.xaml:26-60; CultureResult.cs:10-20	—
1.9.1.1.1	Sample Field	A	العينة	CultureEntryView.xaml:37-38; CultureResult.cs:10	—
1.9.1.1.2	Organism A Field	A	المستعمرة A	CultureEntryView.xaml:41-42; CultureResult.cs:12	—
1.9.1.1.3	Organism B Field	A	المستعمرة B	CultureEntryView.xaml:45-46; CultureResult.cs:14	—
1.9.1.1.4	Organism C Field	A	المستعمرة C	CultureEntryView.xaml:49-50; CultureResult.cs:16	—
1.9.1.1.5	Culture Condition Field	A	حالة المزرعة	CultureEntryView.xaml:53-54; CultureResult.cs:18	—
1.9.1.1.6	Colony Count Field	A	عدد المستعمرات	CultureEntryView.xaml:57-58; CultureResult.cs:20	—
1.9.1.2	Microscopy Block	B	كتلة موجودة	CultureEntryView.xaml:126-176; CultureMicroscopy.cs:16-32	ينقصها Reaction/Bacteria/Direct1-2/Complete
1.9.1.2.1	Reaction Field	D	غير موجود	بحث Reaction في src/**/*.cs = 0	—
1.9.1.2.2	Pus Cells Field	A	صديدية	CultureEntryView.xaml:140-141; CultureMicroscopy.cs:16; ReportCultureSection.cs:85	—
1.9.1.2.3	Red Blood Cells Field	A	كريات حمراء	CultureEntryView.xaml:144-145; CultureMicroscopy.cs:18	—
1.9.1.2.4	Epithelial Cells Field	A	خلايا بطانية	CultureEntryView.xaml:148-149; CultureMicroscopy.cs:20	—
1.9.1.2.5	Crystal Field	A	بلورات	CultureEntryView.xaml:152-153; CultureMicroscopy.cs:22	—
1.9.1.2.6	Fungi Field	A	فطريات	CultureEntryView.xaml:156-157; CultureMicroscopy.cs:24	—
1.9.1.2.7	Bacteria Field	D	غير موجود	بحث Bacteria = 0	قاعدة 1.9.1.2.7 ميتة لأن الحقل غائب
1.9.1.2.8	Additional Microscopy Fields	A	"أخرى 1/2/3"	CultureEntryView.xaml:163-174; CultureMicroscopy.cs:26-30	محدودة بثلاثة أعمدة ثابتة
1.9.1.2.9	Direct Culture Fields	B	مباشر؟ واحد	CultureEntryView.xaml:159-162; CultureMicroscopy.cs:32	لا Direct1/Direct2
1.9.1.2.10	Microscopy Complete Flag	D	غير موجود	بحث IsComplete = 0	—
1.9.1.3	Culture Flag Block	B	أزرار اعتماد/طباعة	CultureEntryView.xaml:207-214	أزرار لا مربعات تحديد
1.9.1.3.1	Culture Verified Flag	A	اعتماد/إلغاء اعتماد	CultureEntryView.xaml:209-210; CultureEntryViewModel.cs:384-456	فرق شكلي
1.9.1.3.2	Culture Print Flag	B	الطباعة كأثر	CultureEntryView.xaml:211; CultureEntryViewModel.cs:458-500	لا تحديد مسبق للطباعة
1.9.1.4	Sensitivity Block	B	شبكة حساسية	CultureEntryView.xaml:92-123; GetCultureEntryGridQueryHandler.cs:35-38	التصنيف عمود واحد لا كتلة
1.9.1.4.1	Antibiotic List	A	المضادات المرفقة	CultureEntryView.xaml:92-123; Views/Lab/CultureAttachmentView.xaml:53-64	—
1.9.1.4.2	Antibiotic Add Action	B	في شاشة المعمل لا شاشة الثقافة	CultureAttachmentView.xaml:57-59; CultureAttachmentViewModel.cs:162-178	على مستوى اختبار الثقافة لا النتيجة
1.9.1.4.3	Sensitivity Classification	B	أربع فئات + "غير محدد"	SensitivityCategory.cs:5-8; CultureEntryViewModel.cs:79-86; SaveCultureResultsCommandValidator.cs:9	القاعدة "واحدة من أربع" غير مُطبَّقة (nullable + خيار خامس)
1.9.1.4.3.1	Highly For Option	A	HighlyFor=0	SensitivityCategory.cs:5; CultureEntryViewModel.cs:82	فرق شكلي في التسمية
1.9.1.4.3.2	Moderate For Option	A	ModerateFor=1	SensitivityCategory.cs:6	فرق شكلي
1.9.1.4.3.3	Low For Option	A	LowFor=2	SensitivityCategory.cs:7	فرق شكلي
1.9.1.4.3.4	Resistant For Option	A	ResistantFor=3	SensitivityCategory.cs:8	فرق شكلي
1.9.1.4.4	Report Element Visibility Control	B	مفتاح عرض عام	ReportItemTemplatesViewModel.cs:19,37; ReportContentBuilder.cs:702-716	عام لا خاص بالثقافة؛ لا يتحكم بالاسم التجاري
1.9.1.5	Culture Action Block	B	كتلة موجودة	CultureEntryView.xaml:207-214	بلا إجراء إدخال الحساسية
1.9.1.5.1	Sensitivity Entry Action	D	غير موجود	CultureEntryView.xaml:207-214; CultureEntryViewModel.cs:207-212	—
1.9.1.5.2	Save Culture Data Action	A	"حفظ"	CultureEntryView.xaml:208; CultureEntryViewModel.cs:316-379; SaveCultureResultsCommandHandler.cs	—
1.9.1.6	Microbiology Report Output	B	يُطبع ضمن التقرير المجمّع	ResultPrintCoordinator.cs:75-77; ReportCultureSection.cs:47-124	لا وثيقة "MICROBIOLOGY REPORT"، ولا جدول مجهر بمديات مرجعية، ولا كتل Highly/Moderate/Low، ولا عمود اسم تجاري (ReportCultureSection.cs:109-112)
2.9 — الوحدات 1.10 / 1.11 / 1.12
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.10.1	Search Screen	A	شاشة	Views/Patients/PatientSearchView.xaml:1-138; PatientsHubViewModel.cs:47-51	لا أيقونة بحث في الشريط
1.10.1.1	Search Criteria Block	A	كتلة معايير	PatientSearchView.xaml:25-66	6 معايير فقط من 15
1.10.1.1.1	Search by Patient Name Exact	D	لا مساواة	SearchPatientsGlobalQueryHandler.cs:41 (Contains)	—
1.10.1.1.2	Search by Patient Name Partial	C	معطّل افتراضيًا	SearchPatientsGlobalQueryHandler.cs:25,41; SystemSettingsConfiguration.cs:27 (EnablePatientNameSearchAssist=false)	افتراضيًا لا بحث بالاسم على هذه الشاشة
1.10.1.1.3	Search by Treating Doctor	A	ComboBox + مسند مستقل	PatientSearchView.xaml:27-31; SearchPatientsGlobalQueryHandler.cs:55-59	—
1.10.1.1.4	Search by Request Date	A	From/To	PatientSearchView.xaml:60-63; SearchPatientsGlobalQueryHandler.cs:95-105	—
1.10.1.1.5	Search by Test Within Period	C	معياران منفصلان	PatientSearchView.xaml:39-43; SearchPatientsGlobalQueryHandler.cs:69-74,95-105	الفترة تقيّد تاريخ الزيارة لا تاريخ التحليل ⇒ نطاق مختلف
1.10.1.1.6	Search by Phone Number	C	بلا حقل مخصص	SearchPatientsGlobalQueryHandler.cs:34-38,44; PatientSearchView.xaml:9	يخالف "لكل معيار حقله"
1.10.1.1.7	Search by Card Number	D	غير موجود	بحث CardNumber = 0؛ Patient.cs:11-49	لا مفهوم في Domain
1.10.1.1.8	Search by Patient Age	A	مدى عمر + وحدة	PatientSearchView.xaml:51-58; SearchPatientsGlobalQueryHandler.cs:85-92	—
1.10.1.1.9	Search by Patient Gender	A	ComboBox	PatientSearchView.xaml:45-49; SearchPatientsGlobalQueryHandler.cs:77-81	—
1.10.1.1.10	Search Within Branch	D	زر "الفرع" يرفض	PatientSearchViewModel.cs:71-75; Domain/Common/BranchScope.cs:1-11	لا Patient.BranchNumber
1.10.1.1.11	Search for Results Not Entered	B	على شاشة قائمة العمل	GetResultWorklistQuery.cs:7-18; ResultsWorklistView.xaml:14-15	ليس على شاشة البحث
1.10.1.1.12	Search for Results Not Verified	B	على شاشة قائمة العمل	GetResultWorklistQueryHandler.cs:54-56; ResultsWorklistView.xaml:23-24	كسابق
1.10.1.1.13	Search for Results Not Printed	B	على شاشة قائمة العمل	GetResultWorklistQueryHandler.cs:79-82; ResultsWorklistView.xaml:34-35	كسابق
1.10.1.1.14	Search for Results Not Delivered	B	في وحدة التسليم	GetUndeliveredResultsQueryHandler.cs:44-49	كسابق
1.10.1.1.15	Date Range Mode Options	D	غير موجود	PatientSearchView.xaml:60-63 (تاريخان مستقلان)	خرق قاعدة 1.10.1.1.15
1.10.1.2	Search Scope Block	D	غير موجود	PatientSearchView.xaml:12-16; بحث Branch|Source|Backup في Features/PatientSearch/** = 0	—
1.10.1.2.1	Branch Selector	D	غير موجود	PatientSearchView.xaml:15	—
1.10.1.2.2	Source Selector	D	غير موجود	لا مسار قراءة .bak؛ SqlServerDatabaseMaintenanceService.cs:29-47 كتابة فقط	—
1.10.1.3	Search Result Block	A	كتلة	PatientSearchView.xaml:106-136,12-14	—
1.10.1.3.1	Search Result Grid	C	7 أعمدة من 8	PatientSearchView.xaml:113-135; PatientSearchDtos.cs:3-7	"جهة الإحالة" مفقودة
1.10.1.3.2	Search Result Count	D	محسوب وغير معروض	PatientSearchViewModel.cs:167-171,594 (عدد صفحات لا إجمالي)	—
1.10.1.3.3	New Search Action	B	"مسح الفلاتر"	PatientSearchView.xaml:65; PatientSearchViewModel.cs:79-100	لا يمسح SearchText/LabIdText
1.10.1.3.4	Enter ID Field	C	ينقل إلى شاشة الزيارات	PatientSearchView.xaml:12-14; PatientSearchViewModel.cs:608-644	لا يملأ شبكة النتائج
1.10.1.4	Search Action Bar	D	غير موجود ككتلة	PatientSearchView.xaml:7-22,25-66 (7 أزرار)	0 من 10 إجراءات
1.10.1.4.1	Patient Tests Action	B	عبر شاشة الزيارات	PatientSearchView.xaml:126-134; PatientVisitHistoryView.xaml:106-131	مستوى أعمق
1.10.1.4.2	Search Patient Data Action	B	في محرر المريض	PatientEditorView.xaml:66-129	ليس على شاشة البحث
1.10.1.4.3	Search Lab Results Action	B	في شاشة الزيارات	PatientVisitHistoryView.xaml:32; PatientVisitHistoryViewModel.cs:215-228	ليس على شاشة البحث
1.10.1.4.4	Delete Patient Action	B	في محرر المريض	PatientEditorViewModel.cs:1011-1029; SoftDeletePatientCommand	ليس على شاشة البحث
1.10.1.4.5	Open Account Action	B	في شاشة الزيارات	PatientVisitHistoryView.xaml:31; PatientVisitHistoryViewModel.cs:200-213	ليس على شاشة البحث
1.10.1.4.6	Restrict To VIP Action	D	غير موجود	Patient.cs:35 قيمة فقط؛ لا Where(IsVip) في أي handler	المرجع [UNVERIFIED]
1.10.1.4.7	Add Action	D	غير موجود	HistoryReportsViewModel.cs:94 (MultiPatientIds) غير مربوطة	—
1.10.1.4.8	Collected Results Action	D	غير موجود	بحث نتائج مجمعة = 0	المرجع [UNVERIFIED]
1.10.1.4.9	Modify Group Action	D	غير موجود	بحث تعديل المجموعة = 0	المرجع [UNVERIFIED]
1.10.1.4.10	Search Main List Action	B	"رجوع" → hub المرضى	PatientSearchView.xaml:16; PatientSearchViewModel.cs:70	وجهة مختلفة
1.11.1	Laboratory ID Lifecycle	A	دورة حياة المعرّف	GetNextLabIdQueryHandler.cs:18-36; Patient.cs:13,199-212; PatientEditorViewModel.cs:518-525	المرحلة 3 ناقصة
1.11.1.1	Laboratory ID Generation	A	يُنشأ ويُحفظ مع التسجيل	PatientEditorViewModel.cs:855-897; CreatePatientCommandHandler.cs:115-118,134-138	—
1.11.1.2	Laboratory ID Storage	A	عمود مفهرس مستقل	Patient.cs:13; PatientConfiguration.cs:14-18; Common/Ids/LabId.cs	—
1.11.1.3	Laboratory ID Display at Next Registration	D	كود جديد في كل زيارة	PatientEditorViewModel.cs:518-525; CreatePatientCommandHandler.cs:53-56 (يرفض إعادة الاستخدام)	خرق صريح للمتطلب
1.11.2	Visit History	A	شاشة	Views/Patients/PatientVisitHistoryView.xaml:1-155; PatientVisitHistoryViewModel.cs:18-268	—
1.11.2.1	Search by Laboratory ID	A	كل الزيارات مع تواريخها	GetPatientByLabIdQueryHandler.cs:31-33,48,55-56; VisitRollup.cs:72-80	—
1.11.2.2	Visit List Grid	C	أعمدة مختلفة	PatientVisitHistoryView.xaml:75-88; PatientSearchDtos.cs:9-14	التاريخ فقط من أصل 8
1.11.2.3	Visit Tests List	A	شبكة اختبارات الزيارة	PatientVisitHistoryView.xaml:106-131; PatientVisitHistoryViewModel.cs:177-198	—
1.12.1	Case History Screen	A	شاشة تدقيق	Views/Audit/AuditView.xaml:1-124; AuditViewModel.cs:16-188; AuditAccessPolicy.cs:8-11	—
1.12.1.1	Registration Audit Entry	A	من سجّل ومتى	AuditView.xaml:63-64; AuditableEntity.cs:14-16; AuditableEntitySaveChangesInterceptor.cs:71-77	—
1.12.1.2	Modification Count	A	عدّاد حقيقي	AuditView.xaml:65; AuditableEntity.cs:22; interceptor :79-88	—
1.12.1.3	Last Modifier Entry	A	آخر مُعدِّل + تاريخ	AuditView.xaml:66-67; GetPatientAuditQueryHandler.cs:63-65	—
1.12.1.4	Result Entry Audit Entry	A	من أدخل ومتى	AuditView.xaml:107-108; EnterResultCommandHandler.cs:89	—
1.12.1.5	Verification Audit Entry	A	من اعتمد ومتى	AuditView.xaml:109-110; ReviewResultCommandHandler.cs:47	—
1.12.1.6	Printing Count	A	عدّاد حقيقي	AuditView.xaml:112; PatientTest.cs:178-189	—
1.12.1.7	Last Printer Entry	B	الاسم فقط	AuditView.xaml:111; AuditDtos.cs:45-47 (التاريخ محمّل لكن غير مربوط)	تاريخ ووقت آخر طباعة غير معروضين
1.12.1.8	Delivery Audit Entry	A	من سلّم ومتى	AuditView.xaml:113-114; DeliverWithSettlementCommandHandler.cs:58	—
1.12.1.9	Patient Samples Block	D	غير موجود	AuditView.xaml:59-89	الكتلة في شاشات المعمل لا في شاشة الحالة
1.12.1.9.1	Tube Name Column	D	غير موجود	بحث Tube|أنبوب = 0؛ SampleCollectionDtos.cs:10-22 بلا أنبوب	لا كيان أنبوب
1.12.1.9.2	Sample Tests Column	C	صفوف لكل اختبار	Views/Lab/SampleDrawBoardView.xaml:86,124	لا تجميع اختبارات العينة
1.12.1.9.3	Sample Take Action	B	"سحب" على لوحة سحب العينات	SampleDrawBoardView.xaml:81-84; MarkSampleDrawnCommandHandler.cs:22-54	موضع مختلف
2.10 — الوحدات 1.13 / 1.14 / 1.15
المعرّف	الاسم	التصنيف	حالة التنفيذ	الأدلة	الفرق / سبب عدم التحقق
1.13.1	Upload Screen	D	غير موجود	بحث (?i)upload في src = 0؛ HttpClient = 0	—
1.13.1.1	Upload Patient Tests List	D	غير موجود	نفس البحث	—
1.13.1.2	Upload Action	D	غير موجود	لا مجلد Upload في Features/**	المخرج «نتيجة مرفوعة» غير قابل
1.13.1.3	Select All Action	D	غير موجود	GroupPatientSlipsView.xaml:18 (قصاصات فقط)	—
1.13.1.4	Unselect All Action	D	غير موجود	GroupPatientSlipsView.xaml:19	—
1.13.1.5	Upload Progress Indicator	D	غير موجود	لا نموذج تقدم رفع	—
1.13.1.6	Block Result Action	D	غير موجود	لا أمر حجب	—
1.13.1.7	Mobile Number Requirement	D	غير موجود	حقل الهاتف موجود (PatientEditorView.xaml:175-176) لكن لا مسار رفع	شرط لعملية غير موجودة
1.13.2	Uploaded Patient Count	C	يحسب المرضى المسجَّلين	StatisticsView.xaml:16-32,73; GetPatientCountStatisticsQueryHandler.cs:36-40	لا يوجد عدّ مرفوع
1.13.3	Site Access Channels	D	غير موجود	لا حقول URL في Domain/Settings/*	—
1.13.4	Site Account Types	D	غير موجود	AccountType.cs أنواع فوترة لا أدوار موقع	—
1.13.4.1	Patient Account Type	D	غير موجود	نفس المصدر	—
1.13.4.2	Doctor Account Type	D	غير موجود	Domain/ExternalEntities/* جهات إحالة لا حسابات موقع	—
1.13.4.3	Laboratory Account Type	D	غير موجود	نفس المصدر	—
1.13.5	Site Sign-In Block	D	غير موجود	تسجيل الدخول الوحيد هو للديسكتوب	—
1.13.5.1	Patient Sign-In Data	D	غير موجود	SignInCommand.cs اسم+كلمة مرور فقط	—
1.13.5.2	Doctor Or Laboratory Sign-In Data	D	غير موجود	لا ثلاثية (كود+معرف+كلمة مرور)	—
1.13.5.3	Site Sign In Action	D	غير موجود	—	—
1.13.6	Site Result View	D	غير موجود	PatientResultSheetView.xaml面向 staff لا بوابة مستخدم	—
1.13.6.1	Result View Action	D	غير موجود	GetPatientResultSheetQuery نطاقه الموظفون	—
1.13.6.2	Result Export Formats	C	PDF + طباعة فقط	PatientResultSheetViewModel.cs:127,160; PatientReportPdfExporter.cs:42-71	Word وImage مفقودان
1.13.6.2.1	Print Format	B	طباعة	ReportPrintingService.cs:37-85; PrintCombinedReportCommandHandler.cs:66-72	غير مرتبط بواجهة موقع
1.13.6.2.2	Word Document Format	D	غير موجود	بحث docx|openxml|rtf = 0	—
1.13.6.2.3	PDF Format	A	تصدير PDF حقيقي	ExportPatientReportPdfCommandHandler.cs:42-45,158-181	—
1.13.6.2.4	Image Format	D	غير موجود	بحث GenerateImages|SaveAsPng|RenderToImage = 0	—
1.13.7	Site Access Data On Receipt	D	غير موجود	ReceiptPdfWriter.cs:272-343 (تذييل فارغ :342); ReceiptSettings.cs:9-42	—
1.14.1	Barcode Label Kinds	C	نوع واحد فقط	Infrastructure/Barcode/BarcodeService.cs:22,96-97	1 من 4
1.14.1.1	Specimen Container Label	D	غير موجود	BarcodeService.cs:115-121	لا ملصق 38×25 مم
1.14.1.2	File Barcode Label	C	سطر نصي في ورقة العمل	Domain/Settings/SystemSettings.cs:23; WorkSheetPdfWriter.cs:161-164	لا ملصق مستقل
1.14.1.3	Laboratory ID Barcode Label	C	نفس الملصق العام	BarcodePayload.cs:14-22; PrintBarcodeCommandHandler.cs:40-44	—
1.14.1.4	Site Access Label	D	غير موجود	بحث natigh = 0	—
1.14.2	Specimen Container Label Content	D	باركود + نص معرّف فقط	BarcodeService.cs:94-121	—
1.14.2.1	Patient Name on Label	D	غير موجود	BarcodeService.cs:115-121	—
1.14.2.2	Sex and Age Line on Label	D	غير موجود	نفس المصدر	—
1.14.2.3	Allocated Test List on Label	D	غير موجود	لا توزيع على أوعية	—
1.14.2.4	Label Barcode	A	CODE_128 مطبوع	BarcodeLabelRenderer.cs:10-29; BarcodeService.cs:69,138-146	—
1.14.2.5	Laboratory Identifier on Label	C	معرّفLabID حسب مفتاح	BarcodeService.cs:118-120; SystemSettings.cs:11	الافتراضي معرّف المريض
1.14.2.6	Container Designation on Label	D	غير موجود	لا نموذج وعاء	—
1.14.2.7	File Barcode Number	C	معرّف مجرد	BarcodePayload.cs:14-22	لا تركيبة (رقم + كود + تسلسل)
1.14.2.8	Label Print Timestamp	C	عند تفعيل مفتاح فقط	BarcodeService.cs:65-67; SystemSettings.cs:25	—
1.14.3	Specimen Container Designations	D	غير موجود	لا enum للأوعية	—
1.14.3.1	Serum Fasting Container	D	غير موجود	Serum Fasting = 0 في src	—
1.14.3.2	Serum Postprandial Container	D	غير موجود	Serum Postprandial = 0	—
1.14.3.3	EDTA Blood Container	D	غير موجود	EDTA Blood = 0؛ PatientTest.cs:20 مجرد IsBlood	—
1.14.3.4	Citrated Blood Container	D	غير موجود	Citrated Blood = 0	—
1.14.3.5	Urine Container	D	غير موجود	PatientTest.cs:16 علم نوع عينة فقط	—
1.14.3.6	Stool Container	D	غير موجود	PatientTest.cs:18	—
1.14.3.7	Semen Container	D	غير موجود	PatientTest.cs:22	—
1.14.4	Test Distribution Across Containers	D	غير موجود	لا نموذج توزيع	—
1.14.4.1	Automatic Test Sorting	C	تجميع بمجموعة الاختبار في ورقة العمل	WorkSheetPdfWriter.cs:166-176,185-199	ليس بالمادة/الوعاء ولا تُنتج ملصقات
1.14.4.2	Container Distribution Edit	D	غير موجود	UpdatePatientTestSampleFlagsCommand فقط	—
1.14.4.3	Vertical Coding on Printed Documents	D	غير موجود	بحث (?i)vertical|rotat = 0 في Domain وPrinting	—
1.14.4.4	Repeated Sticker Printing	C	إعادة الطباعة ممكنة	PatientSearchView.xaml:20-21; PatientSearchViewModel.cs:649-661	لملصق واحد فقط
1.15.1	Automatic History Insertion	C	إدراج بزر وتأكيد	CombinedReportView.xaml:30; CombinedReportViewModel.cs:325-372; AutoInsertHistoryCommandHandler.cs:20-75	يدوي لا تلقائي
1.15.1.1	Automatic Previous Result Insertion	C	لنفس الاختبار فقط	AutoInsertHistoryCommandHandler.cs:66-71	—
1.15.1.2	Multi-Test History Insertion	C	إدراج اختبار واحد	InsertHistoryResultCommandHandler.cs:56-79; `History	