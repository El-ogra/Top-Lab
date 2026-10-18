using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Settings;
using Xunit;

namespace TopLab.Presentation.Tests;

public class Phase9ReportSettingsViewModelTests
{
    private static readonly ResultErrorPresenter Presenter = new();
    private static readonly FakeNavigationService Nav = new();

    [Fact]
    public async Task Load_PopulatesNewFields()
    {
        var sender = new FakeSender();
        var dto = new ReportSettingsDto(
            PageMarginLeftCm: 1m, PageMarginBottomCm: 1m, ReportTopSpaceCm: 2m,
            PaperSize: PaperSize.A4, HeaderFooterMode: HeaderFooterMode.None,
            DoctorSignatureEnabled: false, HistorySortMode: HistorySortMode.ByLabCode,
            HistoryAutoDisplayEnabled: true, EnableAbnormalResultColor: true,
            AbnormalResultFontColor: "#FF0000", AbnormalResultBackgroundColor: "#FFFF00");
        sender.WithResponse(new GetReportSettingsQuery(), Result<ReportSettingsDto>.Success(dto));
        sender.WithResponse(new TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText.GetLabPrintTextQuery { Scope = TopLab.Application.Common.Interfaces.LabPrintTextScope.Report }, TopLab.Application.Common.Results.Result<TopLab.Application.Features.SystemAndPrintSettings.Common.LabPrintTextDto>.Success(new TopLab.Application.Features.SystemAndPrintSettings.Common.LabPrintTextDto("Lab", "Addr", "Phone", "Arial", 12)));

        var vm = new ReportSettingsViewModel(sender, Presenter, Nav);
        await vm.LoadAsync();

        Assert.True(vm.EnableAbnormalResultColor);
        Assert.Equal("#FF0000", vm.AbnormalResultFontColor);
        Assert.Equal("#FFFF00", vm.AbnormalResultBackgroundColor);
    }

    [Fact]
    public async Task Save_SendsNewFields()
    {
        var sender = new FakeSender();
        sender.WithResponse(new GetReportSettingsQuery(), Result<ReportSettingsDto>.Success(new ReportSettingsDto(
            1m, 1m, 2m, PaperSize.A4, HeaderFooterMode.None, false,
            HistorySortMode.ByLabCode, true)));
        sender.WithResponse(new TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText.GetLabPrintTextQuery { Scope = TopLab.Application.Common.Interfaces.LabPrintTextScope.Report }, TopLab.Application.Common.Results.Result<TopLab.Application.Features.SystemAndPrintSettings.Common.LabPrintTextDto>.Success(new TopLab.Application.Features.SystemAndPrintSettings.Common.LabPrintTextDto("Lab", "Addr", "Phone", "Arial", 12)));
        sender.WithResponse(new UpdateReportSettingsCommand(
            1m, 1m, 2m, PaperSize.A4, HeaderFooterMode.None, false,
            HistorySortMode.ByLabCode, true), Result.Success());
        sender.WithResponse(new TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText.SaveLabPrintTextCommand(
            TopLab.Application.Common.Interfaces.LabPrintTextScope.Report, "Lab", "Addr", "Phone", "Arial", 12), Result.Success());

        var vm = new ReportSettingsViewModel(sender, Presenter, Nav);
        await vm.LoadAsync();
        vm.EnableAbnormalResultColor = true;
        vm.AbnormalResultFontColor = "#FF0000";
        vm.AbnormalResultBackgroundColor = "#FFFF00";
        await vm.SaveAsync();

        Assert.Empty(vm.ErrorMessage);
    }
}
