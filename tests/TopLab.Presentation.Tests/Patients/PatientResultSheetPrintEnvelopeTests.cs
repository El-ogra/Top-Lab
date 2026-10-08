using System.IO;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// Phase 1 REF-066: the live results-screen envelope trigger.
/// Proves the entry point dispatches <c>PrintEnvelopeCommand</c> with the
/// loaded visit id and that the <c>طباعة ظرف</c> button is wired in the view.
/// </summary>
public class PatientResultSheetPrintEnvelopeTests
{
    private static PatientResultSheetDto Sheet(int patientId = 7)
    {
        return new PatientResultSheetDto(
            patientId,
            "أحمد محمد علي",
            "100",
            Array.Empty<ResultSheetLineDto>());
    }

    private static PatientResultSheetViewModel LoadedVm(FakeSender sender, int patientId = 7)
    {
        sender.WithResponse(
            new GetPatientResultSheetQuery(patientId),
            Result<PatientResultSheetDto>.Success(Sheet(patientId)));
        var vm = new PatientResultSheetViewModel(
            sender,
            new ResultErrorPresenter(),
            new FakeDialogService(),
            new StubServiceProvider());
        return vm;
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [Fact]
    public async Task PrintEnvelopeCommand_DispatchesWithLoadedPatientId()
    {
        var sender = new FakeSender();
        sender.WithResponse(new PrintEnvelopeCommand(7), Result.Success());
        var vm = LoadedVm(sender);
        await vm.LoadAsync(7);

        Assert.Equal(7, vm.PatientId);
        vm.PrintEnvelopeCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.NotEmpty(vm.StatusMessage);
        Assert.Empty(vm.ErrorMessage);
    }

    [Fact]
    public async Task PrintEnvelopeCommand_SurfacesFailure()
    {
        var sender = new FakeSender();
        sender.WithResponse(
            new PrintEnvelopeCommand(7),
            Result.Failure(Error.NotFound("المريض غير موجود.")));
        var vm = LoadedVm(sender);
        await vm.LoadAsync(7);

        vm.PrintEnvelopeCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Empty(vm.StatusMessage);
    }

    [Fact]
    public async Task PrintEnvelopeCommand_WithNoLoadedPatient_DispatchesNothing()
    {
        var sender = new FakeSender();
        var vm = new PatientResultSheetViewModel(
            sender,
            new ResultErrorPresenter(),
            new FakeDialogService(),
            new StubServiceProvider());

        Assert.Equal(0, vm.PatientId);
        vm.PrintEnvelopeCommand.Execute(null);
        await Task.Yield();
        await Task.Delay(100);

        Assert.Empty(vm.ErrorMessage);
        Assert.Empty(vm.StatusMessage);
    }

    [Fact]
    public void PrintEnvelopeButton_IsPresentInTheView()
    {
        var xaml = File.ReadAllText(Path.Combine(
            FindSolutionRoot(), "src", "TopLab.Presentation", "Views", "Patients", "PatientResultSheetView.xaml"));

        Assert.Contains("Command=\"{Binding PrintEnvelopeCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("طباعة ظرف", xaml, StringComparison.Ordinal);
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }
}
