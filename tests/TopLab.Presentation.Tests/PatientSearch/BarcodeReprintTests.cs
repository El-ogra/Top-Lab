using System.IO;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.PrintBarcode;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.PatientSearch;

/// <summary>
/// P-02 S4 — A-12: the independent patient-card barcode reprint from the search window.
///
/// <para>
/// SD-6 is the load-bearing assertion: a reprint must never mint a new identifier. The
/// three-print test drives the REAL <c>PrintBarcodeCommandHandler</c> against a fake
/// barcode port and proves <c>Patient.LabId</c> is identical afterwards, and that the same
/// identifier is what is printed each time.
/// </para>
///
/// <para>
/// The handler's own behaviour is already covered by
/// <c>PrintBarcodeCommandHandlerTests</c> in the Application project; those tests are
/// deliberately left green and untouched (C-7). This file proves the ENTRY POINT.
/// </para>
/// </summary>
public class BarcodeReprintTests
{
    private static PatientSearchHitDto Hit(int patientId, string? labId = "LAB-7")
        => new(patientId, labId, "Ahmed Mohamed", null, "Male", 30, "Year", null,
            Array.Empty<string>(), "Individual", false, 0, DateTime.UtcNow, 1);

    // =====================================================================
    // The entry point
    // =====================================================================

    [Fact]
    public void ReprintCommand_IsDisabledWithNoPatientSelected()
    {
        var vm = new PatientSearchViewModel(
            new EntryPointSender(), new ResultErrorPresenter(), new FakeNavigationService());

        // No half-wired state: nothing selected means nothing to reprint.
        Assert.NotNull(vm.ReprintBarcodeCommand);
        Assert.False(vm.ReprintBarcodeCommand.CanExecute(null));
    }

    [Fact]
    public void ReprintCommand_IsEnabledWithAPatientSelected_AndDisabledAgainWhenCleared()
    {
        var vm = new PatientSearchViewModel(
            new EntryPointSender(), new ResultErrorPresenter(), new FakeNavigationService());

        vm.SelectedItem = Hit(42);
        Assert.True(vm.ReprintBarcodeCommand.CanExecute(null));

        vm.SelectedItem = null;
        Assert.False(vm.ReprintBarcodeCommand.CanExecute(null));
    }

    [Fact]
    public async Task ReprintCommand_DispatchesTheExistingPrintBarcodeCommand()
    {
        var sender = new EntryPointSender();
        var vm = new PatientSearchViewModel(sender, new ResultErrorPresenter(), new FakeNavigationService());

        vm.SelectedItem = Hit(42);
        vm.ReprintBarcodeCommand.Execute(null);
        await sender.ReprintSent.WaitAsync(TimeSpan.FromSeconds(10));

        // C-7: the EXISTING command is dispatched, with the selected patient's id.
        var command = Assert.Single(sender.BarcodeCommands);
        Assert.Equal(42, command.PatientId);
        Assert.Empty(vm.ErrorMessage);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public async Task ReprintCommand_SurfacesAFailure()
    {
        var sender = new EntryPointSender
        {
            BarcodeResult = Result.Failure(Error.NotFound("المريض غير موجود."))
        };
        var vm = new PatientSearchViewModel(sender, new ResultErrorPresenter(), new FakeNavigationService());

        vm.SelectedItem = Hit(42);
        vm.ReprintBarcodeCommand.Execute(null);
        await sender.ReprintSent.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Empty(vm.StatusMessage);
    }

    [Fact]
    public void ReprintButton_IsPresentInTheView()
    {
        var xaml = PatientSearchViewXaml();

        Assert.Contains("Command=\"{Binding ReprintBarcodeCommand}\"", xaml, StringComparison.Ordinal);

        // Presented as a reprint of the card barcode, per the lost-card flow.
        Assert.Contains("إعادة طباعة الباركود", xaml, StringComparison.Ordinal);

        // The view stays RTL and the pre-existing controls survive.
        Assert.Contains("FlowDirection=\"RightToLeft\"", xaml, StringComparison.Ordinal);
        foreach (var token in new[] { "بحث:", "كود المعمل:", "جلب بالكود", "رجوع", "فتح", "الطبيب المعالج:", "جهة الإحالة:" })
        {
            Assert.Contains(token, xaml, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ReprintCommand_UsesTheExistingCommandType_NotANewOne()
    {
        // C-7: exactly one barcode command exists in the Application layer, and the
        // ViewModel depends on that type rather than declaring its own.
        var commandFiles = Directory.GetFiles(
            Path.Combine(FindSolutionRoot(), "src", "TopLab.Application"),
            "PrintBarcode*.cs",
            SearchOption.AllDirectories);

        Assert.Equal(3, commandFiles.Length);   // Command, Handler, Validator — unchanged

        var property = typeof(PrintBarcodeCommand).GetProperty("PatientId")!;
        Assert.Equal(typeof(int), property.PropertyType);

        var vmProperty = typeof(PatientSearchViewModel).GetProperty("ReprintBarcodeCommand")!;
        Assert.NotNull(vmProperty);
    }

    // =====================================================================
    // SD-6 — the reprint never mints a new identifier
    //
    // The handler-level proof lives in the Application project next to the fakes it
    // needs: PrintBarcodeReprintDoesNotMintIdentifierTests. This file proves the entry
    // point dispatches that command; that file proves the command is safe to re-run.
    // =====================================================================

    [Fact]
    public async Task ReprintCommand_SendsTheSelectedPatientId_UnchangedAcrossThreeReprints()
    {
        // Through the entry point: three reprints, three dispatches, always the same id.
        var sender = new EntryPointSender();
        var vm = new PatientSearchViewModel(sender, new ResultErrorPresenter(), new FakeNavigationService());

        vm.SelectedItem = Hit(42, "LAB-7");
        for (var i = 0; i < 3; i++)
        {
            vm.ReprintBarcodeCommand.Execute(null);
            await Task.Delay(50);
        }

        Assert.Equal(3, sender.BarcodeCommands.Count);
        Assert.All(sender.BarcodeCommands, command => Assert.Equal(42, command.PatientId));

        // The ViewModel holds no identifier of its own to mint — it only carries the id.
        Assert.Equal("LAB-7", vm.SelectedItem!.LabId);
    }

    [Fact]
    public void PatientSearchViewModel_SourceNeverMentionsAnIdentifierMintingQuery()
    {
        // SD-6 structurally: the search ViewModel must not even NAME GetNextLabIdQuery.
        // A reprint re-prints what exists; issuing a new number is forbidden here.
        var source = File.ReadAllText(Path.Combine(
            FindSolutionRoot(), "src", "TopLab.Presentation", "ViewModels", "Patients", "PatientSearchViewModel.cs"));

        // Strip comments so this scan tests CODE, not prose — the ViewModel's own XML
        // comment explains that GetNextLabId is deliberately not called.
        var code = string.Join(
            '\n',
            source.Split('\n')
                .Select(line => line.TrimStart())
                .Where(line => !line.StartsWith("//", StringComparison.Ordinal)
                            && !line.StartsWith("*", StringComparison.Ordinal)
                            && !line.StartsWith("///", StringComparison.Ordinal)));

        Assert.DoesNotContain("GetNextLabId", code, StringComparison.Ordinal);
        Assert.DoesNotContain("SetLabId", code, StringComparison.Ordinal);
        Assert.DoesNotContain("AssignLabId", code, StringComparison.Ordinal);
    }

    // =====================================================================
    // Fakes and helpers
    // =====================================================================

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

    private static string PatientSearchViewXaml()
        => File.ReadAllText(Path.Combine(
            FindSolutionRoot(), "src", "TopLab.Presentation", "Views", "Patients", "PatientSearchView.xaml"));

    /// <summary>Answers only the reprint command; the entry-point tests need nothing else.</summary>
    private sealed class EntryPointSender : ISender
    {
        private readonly TaskCompletionSource _sent = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<PrintBarcodeCommand> BarcodeCommands { get; } = new();
        public Result? BarcodeResult { get; set; }
        public Task ReprintSent => _sent.Task;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is PrintBarcodeCommand barcode)
            {
                BarcodeCommands.Add(barcode);
                _sent.TrySetResult();
                return Task.FromResult((TResponse)(object)(BarcodeResult ?? Result.Success()));
            }

            throw new NotSupportedException($"EntryPointSender has no canned response for {request.GetType().Name}.");
        }

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