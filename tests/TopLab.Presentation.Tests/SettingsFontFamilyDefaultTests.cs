using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Services;
using TopLab.Application.Common.Interfaces;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Settings;

namespace TopLab.Presentation.Tests;

/// <summary>D1: settings VMs default FontFamily to empty so ArabicFontResolver decides.</summary>
public sealed class SettingsFontFamilyDefaultTests
{
    private sealed class NullPrinterCatalog : IPrinterCatalogService
    {
        public IReadOnlyList<string> GetInstalledPrinters() => Array.Empty<string>();
    }

    private sealed class NullWorkstationSettings : IWorkstationConnectionSettingsProvider
    {
        public string? GetEffectiveConnectionString() => null;
        public Task<bool> TestConnectionStringAsync(string candidateConnectionString, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
        public Task SaveConnectionStringAsync(string server, string database, bool integratedSecurity, string login, string password, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private static readonly ResultErrorPresenter Presenter = new();
    private static readonly FakeNavigationService Nav = new();
    private static readonly FakeDialogService Dialogs = new();
    private static readonly FakeSender Sender = new();

    [Fact]
    public void ReceiptSettings_DefaultFontFamily_IsEmpty()
    {
        var vm = new ReceiptSettingsViewModel(Sender, new NullPrinterCatalog(), Presenter, Nav);
        Assert.Equal(string.Empty, vm.FontFamily);
    }

    [Fact]
    public void ReportSettings_DefaultFontFamily_IsEmpty()
    {
        var vm = new ReportSettingsViewModel(Sender, Presenter, Nav);
        Assert.Equal(string.Empty, vm.FontFamily);
    }

    [Fact]
    public void EnvelopeSettings_DefaultFontFamily_IsEmpty()
    {
        var vm = new EnvelopeSettingsViewModel(Sender, Presenter, Nav);
        Assert.Equal(string.Empty, vm.FontFamily);
    }

    [Fact]
    public void SystemSettings_DefaultLabFontFamily_IsEmpty()
    {
        var vm = new SystemSettingsViewModel(Sender, new NullPrinterCatalog(), new NullWorkstationSettings(), Dialogs, Presenter, Nav);
        Assert.Equal(string.Empty, vm.LabFontFamily);
    }
}
