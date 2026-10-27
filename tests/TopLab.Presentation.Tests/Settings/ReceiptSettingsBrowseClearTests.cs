using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReceiptSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetPrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Services;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Settings;
using Xunit;

namespace TopLab.Presentation.Tests.Settings;

/// <summary>
/// Phase 2 (Bucket A, GAP-29 image part): regression tests for
/// ReceiptSettingsViewModel logo browse/clear.
/// </summary>
public class ReceiptSettingsBrowseClearTests
{
    private sealed class NullPrinterCatalog : IPrinterCatalogService
    {
        public IReadOnlyList<string> GetInstalledPrinters() => Array.Empty<string>();
    }
    private sealed class NoOpSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetReceiptSettingsQuery)
            {
                // minimal: return default via null → VM handles null gracefully? VM checks IsSuccess.
                throw new NotSupportedException("GetReceiptSettings not canned");
            }
            throw new NotSupportedException(request.GetType().Name);
        }
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    // ReceiptSettingsViewModel requires printers + mediator + presenter + nav + optional store.
    // ClearLogo is synchronous and does not touch mediator, so test it in isolation.
    private static ReceiptSettingsViewModel BuildClearOnly()
    {
        // Use a sender that throws only if touched; ClearLogo never touches it.
        var sender = new NoOpSender();
        var printers = new NullPrinterCatalog();
        return new ReceiptSettingsViewModel(sender, printers, new ResultErrorPresenter(), new FakeNavigationService(), null);
    }

    [Fact]
    public void ClearLogoCommand_SetsLogoImagePath_ToNull()
    {
        var vm = BuildClearOnly();
        vm.LogoImagePath = "C:/tmp/logo.png";
        vm.ClearLogoCommand.Execute(null);
        Assert.Null(vm.LogoImagePath);
    }

    [Fact]
    public void ClearLogoCommand_SetsStatusMessage()
    {
        var vm = BuildClearOnly();
        vm.LogoImagePath = "C:/tmp/logo.png";
        vm.ClearLogoCommand.Execute(null);
        Assert.Contains("إزالة", vm.StatusMessage);
    }

    [Fact]
    public void ClearLogoCommand_WhenAlreadyNull_DoesNotThrow()
    {
        var vm = BuildClearOnly();
        vm.LogoImagePath = null;
        var ex = Record.Exception(() => vm.ClearLogoCommand.Execute(null));
        Assert.Null(ex);
        Assert.Null(vm.LogoImagePath);
    }
}

