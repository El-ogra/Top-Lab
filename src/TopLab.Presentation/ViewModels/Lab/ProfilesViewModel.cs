using System.Collections.ObjectModel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Views.Lab;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class ProfileRow
{
    public ProfileRow(ProfileDefinitionDto profile, IReadOnlyDictionary<int, string> analyteNames)
    {
        Profile = profile;
        ComponentNames = profile.AnalyteIds
            .Select(id => analyteNames.GetValueOrDefault(id, $"#{id}"))
            .ToList();
    }

    public ProfileDefinitionDto Profile { get; }
    public IReadOnlyList<string> ComponentNames { get; }
    public int ComponentCount => Profile.AnalyteIds.Count;
}

/// <summary>
/// Profiles tab (S-02 Slice 4): list + composition management. No delete
/// affordance exists (no DeleteProfile command — open gap).
/// </summary>
public sealed class ProfilesViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IServiceProvider _services;

    private ObservableCollection<ProfileRow> _rows = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public ProfilesViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IServiceProvider services)
    {
        _mediator = mediator;
        _presenter = presenter;
        _services = services;

        LoadProfilesCommand = new AsyncRelayCommand(_ => LoadAsync());
        OpenCreateCommand = new AsyncRelayCommand(_ => OpenCreateAsync());
        OpenManageCommand = new AsyncRelayCommand((p, _) => OpenManageAsync(p as ProfileRow));
    }

    public ObservableCollection<ProfileRow> Rows { get => _rows; private set => SetProperty(ref _rows, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public AsyncRelayCommand LoadProfilesCommand { get; }
    public AsyncRelayCommand OpenCreateCommand { get; }
    public AsyncRelayCommand OpenManageCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var analytesResult = await _mediator.Send(new GetAnalyteDefinitionsQuery());
            var names = analytesResult.IsSuccess && analytesResult.Value is not null
                ? analytesResult.Value.ToDictionary(a => a.AnalyteId, a => a.Name)
                : new Dictionary<int, string>();

            var profilesResult = await _mediator.Send(new GetProfileDefinitionsQuery());
            if (profilesResult.IsSuccess && profilesResult.Value is not null)
            {
                Rows = new ObservableCollection<ProfileRow>(
                    profilesResult.Value.Select(p => new ProfileRow(p, names)));
            }
            else if (profilesResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(profilesResult.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenCreateAsync()
    {
        var vm = _services.GetRequiredService<ProfileEditorViewModel>();
        await vm.InitializeNewAsync();
        var window = new ProfileEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }

    private async Task OpenManageAsync(ProfileRow? row)
    {
        if (row is null)
        {
            return;
        }

        var vm = _services.GetRequiredService<ProfileEditorViewModel>();
        await vm.InitializeManageAsync(row.Profile);
        var window = new ProfileEditorWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
        await LoadAsync();
    }
}
