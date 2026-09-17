using System.Windows.Threading;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.AccessAndNavigation.Commands.LockWorkstation;
using TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Views.Shell;

namespace TopLab.Presentation.ViewModels.Shell;

public sealed class NavigationItem
{
    public string Title { get; init; } = string.Empty;
    public RelayCommand Command { get; init; } = null!;
    public bool IsEnabled { get; init; } = true;
}

public sealed class ShellViewModel : ViewModelBase, IDisposable
{
    private readonly ISender _mediator;
    private readonly INavigationService _navigation;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ResultErrorPresenter _errorPresenter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;
    private readonly DispatcherTimer? _timer;

    private string _currentUserName = "—";
    private string _lastLoginText = "أول تسجيل دخول";
    private bool _isDatabaseConnected;
    private bool _isConnectionStatusKnown;
    private string _databaseConnectivityText = "غير متصل";
    private string _databaseConnectivityTooltip = "جارٍ التحقق من الاتصال…";
    private DateTime _currentDateTime;
    private ViewModelBase? _currentViewModel;

    public ShellViewModel(
        ISender mediator,
        INavigationService navigation,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ResultErrorPresenter errorPresenter,
        IDialogService dialogs,
        IServiceProvider services,
        HomeViewModel home)
    {
        _mediator = mediator;
        _navigation = navigation;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _errorPresenter = errorPresenter;
        _dialogs = dialogs;
        _services = services;
        _currentViewModel = home;
        _currentDateTime = dateTime.UtcNow.ToLocalTime();

        NavigationItems = BuildNavigationItems();

        try
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            t.Tick += (_, _) => CurrentDateTime = _dateTime.UtcNow.ToLocalTime();
            t.Start();
            _timer = t;
        }
        catch
        {
            _timer = null;
        }

        _ = LoadStatusAsync();
    }

    public IReadOnlyList<NavigationItem> NavigationItems { get; }

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public string CurrentUserName
    {
        get => _currentUserName;
        private set => SetProperty(ref _currentUserName, value);
    }

    public string LastLoginText
    {
        get => _lastLoginText;
        private set => SetProperty(ref _lastLoginText, value);
    }

    public bool IsDatabaseConnected
    {
        get => _isDatabaseConnected;
        private set
        {
            if (SetProperty(ref _isDatabaseConnected, value))
            {
                OnPropertyChanged(nameof(DatabaseConnectivityText));
            }
        }
    }

    public string DatabaseConnectivityText
    {
        get => _databaseConnectivityText;
        private set => SetProperty(ref _databaseConnectivityText, value);
    }

    public string DatabaseConnectivityTooltip
    {
        get => _databaseConnectivityTooltip;
        private set => SetProperty(ref _databaseConnectivityTooltip, value);
    }

    public bool IsConnectionStatusKnown
    {
        get => _isConnectionStatusKnown;
        private set => SetProperty(ref _isConnectionStatusKnown, value);
    }

    public DateTime CurrentDateTime
    {
        get => _currentDateTime;
        private set => SetProperty(ref _currentDateTime, value);
    }

    private IReadOnlyList<NavigationItem> BuildNavigationItems()
    {
        string[] titles = ["المرضى", "المعمل", "ورقة العمل", "الأدوات", "الحسابات", "الإحصائيات", "المستخدمون", "النظام", "الإعدادات", "حول البرنامج", "قفل المحطة", "خروج"];
        var list = new List<NavigationItem>();
        foreach (var title in titles)
        {
            var t = title;
            list.Add(new NavigationItem
            {
                Title = t,
                IsEnabled = true,
                Command = new RelayCommand(async _ =>
                {
                    if (t == "خروج")
                    {
                        bool confirm = await _dialogs.ShowConfirmationAsync("خروج", "هل تريد إنهاء البرنامج؟");
                        if (confirm)
                        {
                            System.Windows.Application.Current.Shutdown();
                        }
                    }
                    else if (t == "حول البرنامج")
                    {
                        var about = new AboutWindow
                        {
                            Owner = System.Windows.Application.Current?.MainWindow
                        };
                        about.ShowDialog();
                    }
                    else if (t == "قفل المحطة")
                    {
                        await LockWorkstationAsync();
                    }
                    else if (t == "المستخدمون")
                    {
                        bool ok = await _dialogs.ShowSecondaryPasswordDialogAsync();
                        if (!ok)
                        {
                            return;
                        }

                        _navigation.NavigateTo<ViewModels.Users.UserManagementViewModel>();
                        if (_navigation.CurrentViewModel is ViewModels.Users.UserManagementViewModel vm)
                        {
                            await vm.LoadAsync();
                        }
                    }
                    else if (t == "الإعدادات")
                    {
                        _navigation.NavigateTo<ViewModels.Settings.SettingsDashboardViewModel>();
                    }
                    else if (t == "المرضى")
                    {
                        _navigation.NavigateTo<ViewModels.Patients.PatientsHubViewModel>();
                    }
                    else
                    {
                        // Future: navigate to feature
                    }
                })
            });
        }

        return list;
    }

    public async Task LoadStatusAsync()
    {
        // User
        if (_currentUser.IsAuthenticated && !string.IsNullOrWhiteSpace(_currentUser.UserName))
        {
            CurrentUserName = _currentUser.UserName;
            try
            {
                var sessionResult = await _mediator.Send(new TopLab.Application.Features.UsersAndPermissions.Queries.GetCurrentSession.GetCurrentSessionQuery());
                if (sessionResult.IsSuccess && sessionResult.Value is not null)
                {
                    CurrentUserName = sessionResult.Value.UserName;
                    LastLoginText = sessionResult.Value.LastLoginAtUtc.HasValue
                        ? sessionResult.Value.LastLoginAtUtc.Value.ToLocalTime().ToString("g")
                        : "أول تسجيل دخول";
                }
                else
                {
                    LastLoginText = "أول تسجيل دخول";
                }
            }
            catch
            {
                LastLoginText = "أول تسجيل دخول";
            }
        }
        else if (_currentUser.IsAuthenticated)
        {
            CurrentUserName = _currentUser.UserName;
            LastLoginText = "أول تسجيل دخول";
        }
        else
        {
            CurrentUserName = "—";
            LastLoginText = "أول تسجيل دخول";
        }

        // DB
        IsConnectionStatusKnown = false;
        DatabaseConnectivityTooltip = "جارٍ التحقق من الاتصال…";
        try
        {
            var result = await _mediator.Send(new CheckDatabaseConnectivityQuery());
            IsDatabaseConnected = result.IsSuccess && result.Value is { IsConnected: true };
            IsConnectionStatusKnown = true;
            if (IsDatabaseConnected)
            {
                DatabaseConnectivityText = "متصل";
                DatabaseConnectivityTooltip = "متصل";
            }
            else
            {
                DatabaseConnectivityText = "غير متصل";
                DatabaseConnectivityTooltip = "لا يوجد اتصال بقاعدة البيانات";
            }
        }
        catch
        {
            IsDatabaseConnected = false;
            IsConnectionStatusKnown = true;
            DatabaseConnectivityText = "تعذر الاتصال بقاعدة البيانات";
            DatabaseConnectivityTooltip = "لا يوجد اتصال بقاعدة البيانات";
        }
    }

    private async Task LockWorkstationAsync()
    {
        string lockedUserName = CurrentUserName;

        await _mediator.Send(new LockWorkstationCommand());
        await LoadStatusAsync();

        var unlockVm = _services.GetRequiredService<UnlockViewModel>();
        unlockVm.Initialize(lockedUserName);
        var unlock = new UnlockWindow(unlockVm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        unlock.ShowDialog();

        await LoadStatusAsync();
    }

    public void Dispose()
    {
        _timer?.Stop();
    }
}
