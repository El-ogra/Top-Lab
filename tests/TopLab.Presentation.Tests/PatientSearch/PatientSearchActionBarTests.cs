using MediatR;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.PatientSearch;

/// <summary>
/// GAP-09 (1.10.1.4.2 / 1.10.1.4.3 / 1.10.1.4.5 / 1.10.1.4.7): the search-screen
/// action bar completion — open account, open patient data, open lab results, and
/// add to selection basket. Commands are bound to the results-grid selection and
/// are disabled when nothing is selected (no half-wired state).
/// </summary>
public class PatientSearchActionBarTests
{
    private static PatientSearchHitDto Hit(int patientId)
        => new(patientId, "LAB-1", "Ahmed", null, "Male", 30, "Year", null,
            Array.Empty<string>(), "Individual", false, 0, DateTime.UtcNow, 1);

    private static PatientSearchViewModel Screen()
        => new(new NoopSender(), new ResultErrorPresenter(), new FakeNavigationService());

    [Fact]
    public void ActionBarCommands_Exist()
    {
        var vm = Screen();

        Assert.NotNull(vm.OpenAccountCommand);
        Assert.NotNull(vm.OpenPatientDataCommand);
        Assert.NotNull(vm.OpenLabResultsCommand);
        Assert.NotNull(vm.AddToSelectionCommand);
    }

    [Fact]
    public void ActionBarCommands_AreDisabledWithNoPatientSelected()
    {
        var vm = Screen();

        Assert.False(vm.OpenAccountCommand.CanExecute(null));
        Assert.False(vm.OpenPatientDataCommand.CanExecute(null));
        Assert.False(vm.OpenLabResultsCommand.CanExecute(null));
        Assert.False(vm.AddToSelectionCommand.CanExecute(null));
    }

    [Fact]
    public void ActionBarCommands_AreEnabledWithAPatientSelected_AndDisabledAgainWhenCleared()
    {
        var vm = Screen();

        vm.SelectedItem = Hit(42);
        Assert.True(vm.OpenAccountCommand.CanExecute(null));
        Assert.True(vm.OpenPatientDataCommand.CanExecute(null));
        Assert.True(vm.OpenLabResultsCommand.CanExecute(null));
        Assert.True(vm.AddToSelectionCommand.CanExecute(null));

        vm.SelectedItem = null;
        Assert.False(vm.OpenAccountCommand.CanExecute(null));
        Assert.False(vm.OpenPatientDataCommand.CanExecute(null));
        Assert.False(vm.OpenLabResultsCommand.CanExecute(null));
        Assert.False(vm.AddToSelectionCommand.CanExecute(null));
    }

    [Fact]
    public void AddToSelection_AddsTheSelectedPatientToTheBasket()
    {
        var vm = Screen();
        vm.SelectedItem = Hit(42);

        vm.AddToSelectionCommand.Execute(null);

        var entry = Assert.Single(vm.SelectionBasket);
        Assert.Equal(42, entry.PatientId);
    }

    [Fact]
    public void AddToSelection_DoesNotDuplicateTheSamePatient()
    {
        var vm = Screen();
        vm.SelectedItem = Hit(42);

        vm.AddToSelectionCommand.Execute(null);
        vm.AddToSelectionCommand.Execute(null);

        Assert.Single(vm.SelectionBasket);
    }

    [Fact]
    public void AddToSelection_CollectsDistinctPatients()
    {
        var vm = Screen();

        vm.SelectedItem = Hit(42);
        vm.AddToSelectionCommand.Execute(null);
        vm.SelectedItem = Hit(43);
        vm.AddToSelectionCommand.Execute(null);

        Assert.Equal(2, vm.SelectionBasket.Count);
    }

    [Fact]
    public void AddToSelection_IsANoOpWithNoSelection()
    {
        var vm = Screen();

        vm.AddToSelectionCommand.Execute(null);

        Assert.Empty(vm.SelectionBasket);
    }

    private sealed class NoopSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
