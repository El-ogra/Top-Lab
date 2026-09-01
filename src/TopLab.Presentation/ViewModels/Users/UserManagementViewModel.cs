using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;
using TopLab.Application.Features.UsersAndPermissions.Commands.SaveUserPermissions;
using TopLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUserById;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Users;

public sealed class PermissionItem : ViewModelBase
{
    private bool _isGranted;
    private bool _isEnabled = true;

    public PermissionItem(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public string Code { get; }
    public string Description { get; }

    public bool IsGranted
    {
        get => _isGranted;
        set => SetProperty(ref _isGranted, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
}

public sealed class UserManagementViewModel : ViewModelBase
{
    private static readonly string[] CatalogCodes =
    [
        "ADD_EDIT_PATIENT", "EDIT_RESULTS", "REVIEW_RESULTS", "PRINT_RESULTS",
        "BLOCK_PRINT_ON_BALANCE", "DELIVER_RESULTS", "DISCOUNT_LIMIT", "PRINT_WORKSHEET",
        "DELETE_PATIENT", "EDIT_SYSTEM_SETTINGS", "CASH_DISBURSE_DEPOSIT", "STATISTICS", "PT_AUDIT_ACCESS"
    ];

    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<UserSummaryDto> _users = new();
    private UserSummaryDto? _selectedUser;
    private UserDetailDto? _selectedDetail;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _secondaryPassword = string.Empty;
    private bool _isAbsolutePermission;
    private decimal _discountLimitPercent;
    private bool _blockPrintOnRemainingBalance;
    private TimeOnly? _workStartTime;
    private TimeOnly? _workEndTime;
    private bool _hasBreakPeriod;
    private int? _breakDurationMinutes;
    private string _errorMessage = string.Empty;
    private string _lastLoginText = string.Empty;
    private bool _isBusy;

    public UserManagementViewModel(ISender mediator, IDialogService dialogs, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        PermissionItems = new ObservableCollection<PermissionItem>(
            CatalogCodes.Select(code => new PermissionItem(code, code)));

        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        CreateCommand = new AsyncRelayCommand(_ => CreateAsync());
        UpdateCommand = new AsyncRelayCommand(_ => UpdateAsync());
        SavePermissionsCommand = new AsyncRelayCommand(_ => SavePermissionsAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
    }

    public ObservableCollection<UserSummaryDto> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    public UserSummaryDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value) && value is not null)
            {
                _ = LoadDetailAsync(value.Id);
            }
        }
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string SecondaryPassword
    {
        get => _secondaryPassword;
        set => SetProperty(ref _secondaryPassword, value);
    }

    public bool IsAbsolutePermission
    {
        get => _isAbsolutePermission;
        set
        {
            if (SetProperty(ref _isAbsolutePermission, value))
            {
                UpdateAuditAccessVisibility();
            }
        }
    }

    public decimal DiscountLimitPercent
    {
        get => _discountLimitPercent;
        set => SetProperty(ref _discountLimitPercent, value);
    }

    public bool BlockPrintOnRemainingBalance
    {
        get => _blockPrintOnRemainingBalance;
        set => SetProperty(ref _blockPrintOnRemainingBalance, value);
    }

    public TimeOnly? WorkStartTime
    {
        get => _workStartTime;
        set => SetProperty(ref _workStartTime, value);
    }

    public TimeOnly? WorkEndTime
    {
        get => _workEndTime;
        set => SetProperty(ref _workEndTime, value);
    }

    public bool HasBreakPeriod
    {
        get => _hasBreakPeriod;
        set => SetProperty(ref _hasBreakPeriod, value);
    }

    public int? BreakDurationMinutes
    {
        get => _breakDurationMinutes;
        set => SetProperty(ref _breakDurationMinutes, value);
    }

    public string LastLoginText
    {
        get => _lastLoginText;
        set => SetProperty(ref _lastLoginText, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ObservableCollection<PermissionItem> PermissionItems { get; }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand CreateCommand { get; }
    public AsyncRelayCommand UpdateCommand { get; }
    public AsyncRelayCommand SavePermissionsCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }

    private void UpdateAuditAccessVisibility()
    {
        var audit = PermissionItems.FirstOrDefault(p => p.Code == "PT_AUDIT_ACCESS");
        if (audit is not null)
        {
            audit.IsEnabled = IsAbsolutePermission;
            if (!IsAbsolutePermission)
            {
                audit.IsGranted = false;
            }
        }
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetUsersQuery());
            if (result.IsSuccess && result.Value is not null)
            {
                Users = new ObservableCollection<UserSummaryDto>(result.Value);
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadDetailAsync(int userId)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery(userId));
            if (result.IsSuccess && result.Value is not null)
            {
                var detail = result.Value;
                _selectedDetail = detail;
                UserName = detail.UserName;
                IsAbsolutePermission = detail.IsAbsolutePermission;
                DiscountLimitPercent = detail.DiscountLimitPercent;
                BlockPrintOnRemainingBalance = detail.BlockPrintOnRemainingBalance;
                WorkStartTime = detail.WorkStartTime;
                WorkEndTime = detail.WorkEndTime;
                HasBreakPeriod = detail.HasBreakPeriod;
                BreakDurationMinutes = detail.BreakDurationMinutes;
                LastLoginText = detail.LastLoginAtUtc.HasValue ? detail.LastLoginAtUtc.Value.ToLocalTime().ToString("g") : "أول تسجيل دخول";
                Password = string.Empty;
                SecondaryPassword = string.Empty;

                foreach (var item in PermissionItems)
                {
                    item.IsGranted = detail.GrantedPermissionCodes.Contains(item.Code);
                }

                UpdateAuditAccessVisibility();
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CreateAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var codes = PermissionItems.Where(p => p.IsGranted && p.IsEnabled).Select(p => p.Code).ToList();
            var cmd = new CreateUserCommand(
                UserName.Trim(),
                Password,
                SecondaryPassword,
                IsAbsolutePermission,
                DiscountLimitPercent,
                BlockPrintOnRemainingBalance,
                WorkStartTime,
                WorkEndTime,
                HasBreakPeriod,
                BreakDurationMinutes,
                codes);

            var result = await _mediator.Send(cmd);
            if (result.IsSuccess)
            {
                Password = string.Empty;
                SecondaryPassword = string.Empty;
                await LoadAsync();
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task UpdateAsync()
    {
        if (_selectedDetail is null)
        {
            ErrorMessage = "لم يتم اختيار مستخدم.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var cmd = new UpdateUserCommand(
                _selectedDetail.Id,
                UserName.Trim(),
                IsAbsolutePermission,
                DiscountLimitPercent,
                BlockPrintOnRemainingBalance,
                WorkStartTime,
                WorkEndTime,
                HasBreakPeriod,
                BreakDurationMinutes,
                string.IsNullOrWhiteSpace(Password) ? null : Password,
                string.IsNullOrWhiteSpace(SecondaryPassword) ? null : SecondaryPassword);

            var result = await _mediator.Send(cmd);
            if (result.IsSuccess)
            {
                Password = string.Empty;
                SecondaryPassword = string.Empty;
                await LoadAsync();
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SavePermissionsAsync()
    {
        if (_selectedDetail is null)
        {
            ErrorMessage = "لم يتم اختيار مستخدم.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var codes = PermissionItems.Where(p => p.IsGranted && p.IsEnabled).Select(p => p.Code).ToList();
            var cmd = new SaveUserPermissionsCommand(_selectedDetail.Id, codes);
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess && result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeleteAsync()
    {
        if (_selectedDetail is null)
        {
            ErrorMessage = "لم يتم اختيار مستخدم.";
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync("تأكيد الحذف", $"هل أنت متأكد من حذف المستخدم '{_selectedDetail.UserName}'؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new DeleteUserCommand(_selectedDetail.Id));
            if (result.IsSuccess)
            {
                await LoadAsync();
                // Clear selection
                _selectedDetail = null;
                UserName = string.Empty;
                Password = string.Empty;
                SecondaryPassword = string.Empty;
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                if (result.Error.Message.Contains("سجلات مرتبطة"))
                {
                    await _dialogs.ShowErrorAsync(result.Error.Message + Environment.NewLine + "يمكنك تعطيل المستخدم بدلاً من حذفه.");
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
