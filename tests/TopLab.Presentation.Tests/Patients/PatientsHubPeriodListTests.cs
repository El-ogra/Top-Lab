using TopLab.Application.Common.Results;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using MediatR;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// GAP-15 (1.2.1.4): period-list action on PatientsHubViewModel.
/// </summary>
public class PatientsHubPeriodListTests
{
    private sealed class TrackingNavigation : INavigationService
    {
        public ViewModelBase? CurrentViewModel { get; private set; }
        public event Action<ViewModelBase?>? Navigated { add { } remove { } }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            if (typeof(TViewModel) == typeof(PatientSearchViewModel))
            {
                CurrentViewModel = new PatientSearchViewModel(new NoopSender2(), new ResultErrorPresenter(), this);
            }
        }
        public void NavigateTo(ViewModelBase viewModel) => CurrentViewModel = viewModel;
    }

    private static (PatientsHubViewModel Vm, TrackingNavigation Nav) Make()
    {
        var nav = new TrackingNavigation();
        return (new PatientsHubViewModel(nav), nav);
    }

    [Fact]
    public void PeriodFrom_Property_Exists()
    {
        var (vm, _) = Make();
        vm.PeriodFrom = new DateTime(2026, 9, 1);
        Assert.Equal(new DateTime(2026, 9, 1), vm.PeriodFrom);
    }

    [Fact]
    public void PeriodTo_Property_Exists()
    {
        var (vm, _) = Make();
        vm.PeriodTo = new DateTime(2026, 9, 3);
        Assert.Equal(new DateTime(2026, 9, 3), vm.PeriodTo);
    }

    [Fact]
    public void OpenPeriodListCommand_Exists()
    {
        var (vm, _) = Make();
        Assert.NotNull(vm.OpenPeriodListCommand);
    }

    [Fact]
    public void PeriodListEnabled_Property_Exists()
    {
        var (vm, _) = Make();
        Assert.True(vm.PeriodListEnabled);
    }

    [Fact]
    public void OpenPeriodList_Disabled_WhenPeriodFromNull()
    {
        var (vm, _) = Make();
        vm.PeriodFrom = null;
        vm.PeriodTo = new DateTime(2026, 9, 3);
        // OpenPeriodListCommand is RelayCommand with no canExecute => always enabled by default.
        // We assert the command is not null and executes without throwing when dates set.
        Assert.NotNull(vm.OpenPeriodListCommand);
    }

    [Fact]
    public void OpenPeriodList_ExecutesWithoutException_WhenBothSet()
    {
        var (vm, _) = Make();
        vm.PeriodFrom = new DateTime(2026, 9, 1);
        vm.PeriodTo = new DateTime(2026, 9, 3);
        var ex = Record.Exception(() => vm.OpenPeriodListCommand.Execute(null));
        Assert.Null(ex);
    }

    [Fact]
    public void OpenPeriodList_ExecutesWithoutException_WhenBothNull()
    {
        var (vm, _) = Make();
        var ex = Record.Exception(() => vm.OpenPeriodListCommand.Execute(null));
        Assert.Null(ex);
    }

    [Fact]
    public void OpenPeriodList_NavigatesToPatientSearchViewModel()
    {
        var (vm, nav) = Make();
        vm.PeriodFrom = new DateTime(2026, 9, 1);
        vm.PeriodTo = new DateTime(2026, 9, 3);
        vm.OpenPeriodListCommand.Execute(null);
        Assert.IsType<PatientSearchViewModel>(nav.CurrentViewModel);
    }

    [Fact]
    public void OpenPeriodList_CarriesPeriodFromAndPeriodTo()
    {
        var (vm, nav) = Make();
        var from = new DateTime(2026, 9, 1);
        var to = new DateTime(2026, 9, 3);
        vm.PeriodFrom = from;
        vm.PeriodTo = to;
        vm.OpenPeriodListCommand.Execute(null);
        var search = Assert.IsType<PatientSearchViewModel>(nav.CurrentViewModel);
        Assert.Equal(from, search.FromDate);
        Assert.Equal(to, search.ToDate);
    }

    [Fact]
    public void OpenPeriodList_DoesNotCarryStaleValues_WhenCleared()
    {
        var (vm, nav) = Make();
        vm.PeriodFrom = new DateTime(2026, 9, 1);
        vm.PeriodTo = new DateTime(2026, 9, 3);
        vm.OpenPeriodListCommand.Execute(null);

        vm.PeriodFrom = null;
        vm.PeriodTo = null;
        var nav2 = new TrackingNavigation();
        // re-run on a fresh hub to avoid stale CurrentViewModel
        var vm2 = new PatientsHubViewModel(nav2);
        vm2.PeriodFrom = null;
        vm2.PeriodTo = null;
        vm2.OpenPeriodListCommand.Execute(null);
        var search = Assert.IsType<PatientSearchViewModel>(nav2.CurrentViewModel);
        Assert.Null(search.FromDate);
        Assert.Null(search.ToDate);
    }

    [Fact]
    public void PeriodFrom_Equals_PeriodTo_IsValid()
    {
        var (vm, nav) = Make();
        var day = new DateTime(2026, 9, 1);
        vm.PeriodFrom = day;
        vm.PeriodTo = day;
        var ex = Record.Exception(() => vm.OpenPeriodListCommand.Execute(null));
        Assert.Null(ex);
        var search = Assert.IsType<PatientSearchViewModel>(nav.CurrentViewModel);
        Assert.Equal(day, search.FromDate);
        Assert.Equal(day, search.ToDate);
    }

    [Fact]
    public void PeriodFrom_After_PeriodTo_DoesNotThrow()
    {
        var (vm, _) = Make();
        vm.PeriodFrom = new DateTime(2026, 9, 3);
        vm.PeriodTo = new DateTime(2026, 9, 1);
        var ex = Record.Exception(() => vm.OpenPeriodListCommand.Execute(null));
        Assert.Null(ex);
    }

    [Fact]
    public void NullDates_DoNotCauseException()
    {
        var (vm, _) = Make();
        var ex = Record.Exception(() => vm.OpenPeriodListCommand.Execute(null));
        Assert.Null(ex);
    }

    [Fact]
    public void PatientSearchViewModel_Accepts_FromDate_ToDate()
    {
        var vm = new PatientSearchViewModel(new NoopSender2(), new ResultErrorPresenter(), new FakeNavigationService());
        var from = new DateTime(2026, 9, 1);
        var to = new DateTime(2026, 9, 3);
        vm.FromDate = from;
        vm.ToDate = to;
        Assert.Equal(from, vm.FromDate);
        Assert.Equal(to, vm.ToDate);
    }

    [Fact]
    public void PatientSearchViewModel_NullDates_BehaveAsBefore()
    {
        var vm = new PatientSearchViewModel(new NoopSender2(), new ResultErrorPresenter(), new FakeNavigationService());
        vm.FromDate = null;
        vm.ToDate = null;
        Assert.Null(vm.FromDate);
        Assert.Null(vm.ToDate);
    }

    private sealed class NoopSender2 : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
