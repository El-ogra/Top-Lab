using System.Globalization;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;
using TopLab.Application.Features.SampleCollection.Queries.GetPatientTestsForDraw;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Lab;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>
/// Phase 2 (Bucket A, GAP-21): regression tests for SampleDrawBoardViewModel
/// Date Picker Block (8.2.1.10) — month navigation, month label, selected date indicator.
/// </summary>
public class SampleDrawBoardTests
{
    private sealed class NoOpSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetPatientTestsForDrawQuery)
            {
                var dto = new SampleDrawBoardDto(1, "L1", "أحمد", new List<PatientTestDrawDto>(), new List<PatientTestDrawDto>());
                return Task.FromResult((TResponse)(object)Result<SampleDrawBoardDto>.Success(dto));
            }
            throw new NotSupportedException(request.GetType().Name);
        }
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private static SampleDrawBoardViewModel Build() =>
        new(new NoOpSender(), new FakeDialogService(), new ResultErrorPresenter());

    [Fact]
    public void SelectedDate_DefaultsToToday()
    {
        var vm = Build();
        Assert.NotNull(vm.SelectedDate);
        Assert.Equal(DateTime.UtcNow.Date, vm.SelectedDate!.Value.Date);
    }

    [Fact]
    public void MonthLabel_IsNonEmpty()
    {
        var vm = Build();
        Assert.False(string.IsNullOrWhiteSpace(vm.MonthLabel));
    }

    [Fact]
    public void MonthLabel_Format_MatchesMMMMYYYY()
    {
        var vm = Build();
        var expected = (vm.SelectedDate ?? DateTime.UtcNow).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        Assert.Equal(expected, vm.MonthLabel);
    }

    [Fact]
    public void PreviousMonthCommand_Exists()
    {
        var vm = Build();
        Assert.NotNull(vm.PreviousMonthCommand);
    }

    [Fact]
    public void NextMonthCommand_Exists()
    {
        var vm = Build();
        Assert.NotNull(vm.NextMonthCommand);
    }

    [Fact]
    public void ShiftMonth_Forward_ChangesMonthLabel()
    {
        var vm = Build();
        var before = vm.MonthLabel;
        vm.NextMonthCommand.Execute(null);
        Assert.NotEqual(before, vm.MonthLabel);
    }

    [Fact]
    public void ShiftMonth_Backward_ChangesMonthLabel()
    {
        var vm = Build();
        var before = vm.MonthLabel;
        vm.PreviousMonthCommand.Execute(null);
        Assert.NotEqual(before, vm.MonthLabel);
    }

    [Fact]
    public void ShiftMonth_ClampsDay_ToDaysInMonth()
    {
        var vm = Build();
        vm.SelectedDate = new DateTime(2026, 1, 31);
        vm.NextMonthCommand.Execute(null); // Feb 2026
        Assert.Equal(2, vm.SelectedDate!.Value.Month);
        Assert.True(vm.SelectedDate.Value.Day <= 28);
    }

    [Fact]
    public void SelectedDate_Setter_RaisesMonthLabel()
    {
        var vm = Build();
        var raised = false;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.MonthLabel)) raised = true; };
        vm.SelectedDate = DateTime.UtcNow.AddMonths(2);
        Assert.True(raised);
    }

    [Fact]
    public async Task InitializeAsync_PopulatesPatientId()
    {
        var vm = Build();
        await vm.InitializeAsync(42);
        Assert.Equal(42, vm.PatientId);
    }
}
