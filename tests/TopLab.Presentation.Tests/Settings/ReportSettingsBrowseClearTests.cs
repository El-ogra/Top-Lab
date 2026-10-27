using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Settings;
using Xunit;

namespace TopLab.Presentation.Tests.Settings;

/// <summary>
/// Phase 2 (Bucket A, GAP-29 image part): regression tests for
/// ReportSettingsViewModel header/footer image browse/clear.
/// </summary>
public class ReportSettingsBrowseClearTests
{
    private sealed class NoOpSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException(request.GetType().Name);
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private static ReportSettingsViewModel Build() =>
        new(new NoOpSender(), new ResultErrorPresenter(), new FakeNavigationService(), null);

    [Fact]
    public void ClearHeaderImageCommand_SetsHeaderImagePath_ToNull()
    {
        var vm = Build();
        vm.HeaderImagePath = "C:/tmp/h.png";
        vm.ClearHeaderImageCommand.Execute(null);
        Assert.Null(vm.HeaderImagePath);
    }

    [Fact]
    public void ClearFooterImageCommand_SetsFooterImagePath_ToNull()
    {
        var vm = Build();
        vm.FooterImagePath = "C:/tmp/f.png";
        vm.ClearFooterImageCommand.Execute(null);
        Assert.Null(vm.FooterImagePath);
    }

    [Fact]
    public void ClearHeaderImageCommand_SetsStatusMessage()
    {
        var vm = Build();
        vm.HeaderImagePath = "C:/tmp/h.png";
        vm.ClearHeaderImageCommand.Execute(null);
        Assert.Contains("إزالة", vm.StatusMessage);
    }

    [Fact]
    public void ClearFooterImageCommand_SetsStatusMessage()
    {
        var vm = Build();
        vm.FooterImagePath = "C:/tmp/f.png";
        vm.ClearFooterImageCommand.Execute(null);
        Assert.Contains("إزالة", vm.StatusMessage);
    }

    [Fact]
    public void ClearHeaderImage_WhenAlreadyNull_DoesNotThrow()
    {
        var vm = Build();
        vm.HeaderImagePath = null;
        var ex = Record.Exception(() => vm.ClearHeaderImageCommand.Execute(null));
        Assert.Null(ex);
    }

    [Fact]
    public void ClearFooterImage_WhenAlreadyNull_DoesNotThrow()
    {
        var vm = Build();
        vm.FooterImagePath = null;
        var ex = Record.Exception(() => vm.ClearFooterImageCommand.Execute(null));
        Assert.Null(ex);
    }
}
