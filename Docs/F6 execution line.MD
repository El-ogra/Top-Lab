1) وضع F5 الحالي المؤثر على F6 (مُحقق بعد التنفيذ)
main الآن على 61cad11 (دمج 94b5213 F5) ومُدفوع إلى origin/main — git log --graph يظهر ab52719 -> 94b5213 -> 61cad11
Domain مكتمل: Common/IAuditableEntity + 14 enum + 23 StrongId + 36 كيان (Patients/Tests/Results/Billing/ExternalEntities/SentOutSamples/Users/Attendance/Accounting/Settings/PatientStatus) — كلها تبني 0 Warning
Infrastructure: Persistence/Configurations/ 32 ملف + Persistence/Migrations/20260828052248_BaselineDataModel.cs (36 CreateTable) + ApplicationDbContext.DbSets.cs + DesignTimeDbContextFactory + AuditableEntitySaveChangesInterceptor مصحح إلى IAuditableEntity
TopLab.Presentation لا يزال هيكلاً فارغاً: App.xaml بـ StartupUri="MainWindow" و App.xaml.cs فارغ و MainWindow.xaml شبكة فارغة — لم يمس، وهذا هو نطاق F6 الوحيد
الاختبارات: 72 ناجحة (37 Domain +14 App +21 Infra) — لا انحدار
2) هدف F6 وحدوده
الهدف: تحويل الهيكل الفارغ إلى Composition Root عامل يقلع عبر DI ويعرض Shell رئيسي مع شريط تنقل وStatusBar وخدمات ملاحة/حوار وعرض أخطاء، دون أي جداول جديدة أو Migration.

داخل النطاق: App.xaml(.cs) Host + AddPresentation() + ViewModelBase/RelayCommand/Converters + INavigationService/NavigationService + IDialogService/DialogService + ResultErrorPresenter + CheckDatabaseConnectivityQuery + ShellViewModel/HomeViewModel + ShellView (MainWindow) + appsettings.json (إن لم يكن موجوداً بسبب .gitignore).

خارج النطاق: أي شاشات ميزات (M02..M23)، أي كيانات جديدة، أي Migration جديدة، أي حزمة إنترنت (ADR-0010).

3) المتطلبات المسبقة
F5 أخضر ومُدمج في main (محقق)
قراءة Architecture Blueprint §4.4/§6.1, UI/UX Blueprint S-02/§4.3-§4.7, Coding Standards §3.1/§5.4/§6.10, Test Strategy §3.2/§3.4/§6
Directory.Packages.props يسمح بإضافة Microsoft.Extensions.Hosting 8.0.x + Microsoft.Extensions.Configuration.Json 8.0.x عبر CPM
4) خطة التنفيذ الكاملة (تسلسل واحد بدون فروع)
الخطوة 0 — تحضير على main (لا فرع): git status نظيف، dotnet build -warnaserror و dotnet test أخضر — لقطة أساس قبل التعديل.

الخطوة 1 — Composition Root:

حذف StartupUri من App.xaml ووضع <Application.Resources> لمحولات
استبدال App.xaml.cs بـ Host.CreateApplicationBuilder يستدعي AddApplication() + AddInfrastructure(builder.Configuration) + AddPresentation() ثم GetRequiredService<MainWindow>().Show() و OnExit => host.Dispose()
إضافة appsettings.json (إن غاب بسبب .gitignore يُنشأ appsettings.example.json ويُنسخ محلياً) بمحتوى ConnectionStrings:TopLab = Server=(localdb)\mssqllocaldb;... و CopyToOutputDirectory=PreserveNewest
إضافة Directory.Packages.props + TopLab.Presentation.csproj مراجع Microsoft.Extensions.Hosting و Configuration.Json ومرجع مشروع TopLab.Infrastructure (استثناء مقصود لمركز التركيب — موثق)
الخطوة 2 — بنية MVVM المشتركة: Common/ViewModelBase.cs (INotifyPropertyChanged/SetProperty) + Common/RelayCommand.cs و AsyncRelayCommand.cs (ICommand/CanExecute/CancellationToken) + Common/Converters/ (BooleanToVisibility/NullToVisibility/InverseBoolean)

الخطوة 3 — خدمات العرض: Common/Navigation/INavigationService.cs (NavigateTo<T> where T:ViewModelBase/CurrentViewModel/Navigated) + NavigationService.cs (يحل T عبر IServiceProvider) — Singleton Common/Dialogs/IDialogService.cs (ShowConfirmationAsync/ShowErrorAsync/ShowSecondaryPasswordDialogAsync بـ MessageBox) — Singleton Common/ErrorPresentation/ResultErrorPresenter.cs مع const PermissionDeniedMessage = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام" (المصدر الوحيد) وتعيين Validation→inline / NotFound|Conflict→banner / Forbidden→الثابت / Unexpected→رسالة عامة

الخطوة 4 — استعلام فحص الاتصال (لـ StatusBar): في TopLab.Application/Features/AccessAndNavigation/Queries/CheckDatabaseConnectivity/ إنشاء CheckDatabaseConnectivityQuery : IRequest<Result<bool>> بلا تفويض + Handler يستدعي IApplicationDbContext.Database.CanConnectAsync(ct) ويحوّل الاستثناء إلى Error.Unexpected (ADR-0008)

الخطوة 5 — Shell ViewModel: ViewModels/Shell/ShellViewModel.cs : ViewModelBase يحقن IMediator/INavigationService/ICurrentUserService/IDateTimeProvider/IAppLogger/ResultErrorPresenter/IDialogService ويعرض CurrentUserName/LastLoginText (أول دخول→"أول تسجيل دخول")/IsDatabaseConnected/CurrentDateTime (DispatcherTimer)/NavigationItems (11 عنصر: Patients/Laboratory/Work sheet/Tools/Accounts/Statistics/Users/System/Setting/About Us/Exit مع IsEnabled = IsAbsolutePermission||HasPermission)/CurrentViewModel + أوامر تنقل تستدعي INavigationService و Exit→Application.Shutdown + LoadStatusAsync() تستدعي CheckDatabaseConnectivityQuery

الخطوة 6 — Shell View: إعادة كتابة MainWindow.xaml إلى DockPanel FlowDirection=RightToLeft (RTL) — أعلى ItemsControl للأزرار 11، أسفل StatusBar (User/LastLogin/ellipse أخضر/أحمر مربوط بـ IsDatabaseConnected عبر محول/CurrentDateTime)، وسط ContentControl Content={Binding CurrentViewModel} مع DataTemplate لـ HomeViewModel (أزرار: Add/Edit Patient/Result Entry/Patient Search/Result Delivery) — ViewModels/Shell/HomeViewModel.cs حامل مؤقت MainWindow.xaml.cs مُنشئ DI public MainWindow(ShellViewModel vm){InitializeComponent(); DataContext=vm;} و OnClosed يوقف المؤقت

الخطوة 7 — تسجيل DI للعرض: TopLab.Presentation/DependencyInjection.cs (AddPresentation()) يسجل INavigationService/DialogService/ResultErrorPresenter Singleton و IAppLogger→WpfAppLogger و ShellViewModel/HomeViewModel Transient و MainWindow Singleton و المحولات

الخطوة 8 — تحقق الإقلاع: dotnet build -warnaserror ثم dotnet run --project src/TopLab.Presentation — النافذة تظهر بشريط تنقل وStatusBar، مؤشر الاتصال أحمر عند عدم وجود LocalDB دون انهيار، التنقل لا يمس ApplicationDbContext مباشرةً

الخطوة 9 — اختبارات: CheckDatabaseConnectivityQueryHandlerTests (true/false/exception→Unexpected) + ShellViewModelTests (User/LastLogin أول/متصل/11 عنصر/IsEnabled حسب FakeCurrentUserService/CurrentDateTime/Tick/Navigate/Forbidden عبر ResultErrorPresenter) + ResultErrorPresenterTests (كل ErrorType) + فحص يدوي Test Strategy §7.1 الستة + تغطية App 80% لا تزال محفوظة عبر coverlet

الخطوة 10 — الإنهاء على main: dotnet build و dotnet test أخضر، grep -rn "TopLab.Infrastructure" src/TopLab.Presentation/ViewModels =0 و grep "ApplicationDbContext" في ViewModels =0 (المرجع المسموح فقط في App.xaml.cs/DependencyInjection.cs)، ثم git add -A && git commit -m "feat(presentation): composition root, shell, navigation and error presenter (F6)" مباشرة على main و git push origin main (بدون فرع جديد)

5) الملفات المتأثرة
جديدة: DependencyInjection.cs (Presentation), Common/ViewModelBase.cs, RelayCommand.cs, Converters/*, Navigation/*, Dialogs/*, ErrorPresentation/ResultErrorPresenter.cs, Application/Features/AccessAndNavigation/Queries/CheckDatabaseConnectivity/*, ViewModels/Shell/ShellViewModel.cs+HomeViewModel.cs, Views/Shell/ShellView.xaml (أو إعادة كتابة MainWindow), اختبارات CheckDatabaseConnectivityQueryHandlerTests+ShellViewModelTests تعديل: App.xaml (إزالة StartupUri + Resources), App.xaml.cs (Host), MainWindow.xaml(.cs), TopLab.Presentation.csproj + Directory.Packages.props, appsettings.json لا تُمس: كل Domain/Infrastructure/Migrations/الاختبارات الـ72 السابقة، Docs/Source/*.md

6) المخاطر بعد F5
F5 خلّف Migrations في Persistence/Migrations — F6 لا يضيف Migration، لكن AddInfrastructure سيستدعي MigrationsAssembly("TopLab.Infrastructure") ويجدها
.gitignore يحجب appsettings.json — إذا كان appsettings*.json محجوباً، يُلتزم appsettings.example.json ويُوثق النسخ المحلي
التبعية Presentation→Infrastructure استثناء موثق لمركز التركيب (وإلا فحص القواعد سيفشل)
7) معايير الإنجاز لـ F6 على main
يُعتبر F6 منجزاً فقط إذا: Host يقلع ويعرض Shell RTL بـ 11 زر وStatusBar (User/LastLogin/اتصال/وقت) + Navigation/Dialog/ErrorPresenter تعمل عبر تجريدات + جميع ViewModels لا تلمس ApplicationDbContext + CheckDatabaseConnectivityQuery يُرجع Result<bool> + dotnet build -warnaserror 0/0 + dotnet test 72+ جديد كلها خضراء + لا جداول/ Migration جديدة.

