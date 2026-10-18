using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportItemTemplates;
using TopLab.Domain.Settings;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Settings;
using Xunit;

namespace TopLab.Presentation.Tests.Settings;

/// <summary>
/// GAP-015 (Phase 8): the item-template view model round-trips the per-item
/// layout fields (LineSpacing, LeftOffsetCm, MarkerSymbol) and reports invalid
/// values instead of sending them.
///
/// Scope: these tests cover the three Phase 8 fields only. They do not close the
/// pre-existing Phase 7 gap (no view-model coverage for the other template
/// fields), which remains deferred to the final audit.
/// </summary>
public class ReportItemTemplatesViewModelLayoutTests
{
    private sealed class RecordingSender : ISender
    {
        private readonly IReadOnlyList<ReportItemTemplateDto> _loaded;

        public RecordingSender(IReadOnlyList<ReportItemTemplateDto> loaded) => _loaded = loaded;

        public UpdateReportItemTemplatesCommand? SavedCommand { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetReportItemTemplatesQuery)
            {
                return Task.FromResult((TResponse)(object)
                    Result<IReadOnlyList<ReportItemTemplateDto>>.Success(_loaded));
            }

            if (request is UpdateReportItemTemplatesCommand update)
            {
                SavedCommand = update;
                return Task.FromResult((TResponse)(object)Result.Success());
            }

            throw new NotSupportedException($"RecordingSender has no canned response for {request.GetType().Name}.");
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

    private static IReadOnlyList<ReportItemTemplateDto> LoadedRows(
        decimal? lineSpacing = null,
        decimal? leftOffsetCm = null,
        string? markerSymbol = null)
        => ReportItemKeys.All
            .Select(key => new ReportItemTemplateDto(
                key,
                ReportItemTemplateCatalog.GetDefault(key).Label,
                null,
                true,
                null,
                false,
                lineSpacing,
                leftOffsetCm,
                markerSymbol))
            .ToList();

    private static ReportItemTemplatesViewModel Screen(
        out RecordingSender sender,
        IReadOnlyList<ReportItemTemplateDto>? loaded = null)
    {
        sender = new RecordingSender(loaded ?? LoadedRows());
        return new ReportItemTemplatesViewModel(sender, new ResultErrorPresenter(), new FakeNavigationService());
    }

    private static ReportItemTemplateRow Row(ReportItemTemplatesViewModel vm, string key)
        => vm.Rows.Single(r => r.ItemKey == key);

    [Fact]
    public async Task Load_ReadsTheThreeLayoutFieldsIntoTheirRows()
    {
        var vm = Screen(out _, LoadedRows(lineSpacing: 1.75m, leftOffsetCm: 2.25m, markerSymbol: "•"));

        await vm.LoadAsync();

        foreach (var row in vm.Rows)
        {
            Assert.Equal(1.75m, row.LineSpacing);
            Assert.Equal(2.25m, row.LeftOffsetCm);
            Assert.Equal("•", row.MarkerSymbol);
        }
    }

    [Fact]
    public async Task Load_WithNullLayoutFields_LeavesTheRowsEmpty()
    {
        var vm = Screen(out _);

        await vm.LoadAsync();

        Assert.All(vm.Rows, row =>
        {
            Assert.Null(row.LineSpacing);
            Assert.Null(row.LeftOffsetCm);
            Assert.Null(row.MarkerSymbol);
        });
    }

    [Fact]
    public async Task Save_SendsTheEditedLayoutFields()
    {
        var vm = Screen(out var sender);
        await vm.LoadAsync();

        var row = Row(vm, ReportItemKeys.TestName);
        row.LineSpacing = 2.5m;
        row.LeftOffsetCm = 1.5m;
        row.MarkerSymbol = "▸";

        await vm.SaveAsync();

        Assert.NotNull(sender.SavedCommand);
        var saved = sender.SavedCommand!.Items.Single(i => i.ItemKey == ReportItemKeys.TestName);
        Assert.Equal(2.5m, saved.LineSpacing);
        Assert.Equal(1.5m, saved.LeftOffsetCm);
        Assert.Equal("▸", saved.MarkerSymbol);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Equal("تم حفظ عناصر التقرير.", vm.StatusMessage);
    }

    [Fact]
    public async Task Save_WithClearedFields_SendsNull()
    {
        var vm = Screen(out var sender, LoadedRows(lineSpacing: 2m, leftOffsetCm: 1m, markerSymbol: "*"));
        await vm.LoadAsync();

        var row = Row(vm, ReportItemKeys.TestName);
        row.LineSpacing = null;
        row.LeftOffsetCm = null;
        row.MarkerSymbol = "   ";

        await vm.SaveAsync();

        var saved = sender.SavedCommand!.Items.Single(i => i.ItemKey == ReportItemKeys.TestName);
        Assert.Null(saved.LineSpacing);
        Assert.Null(saved.LeftOffsetCm);
        Assert.Null(saved.MarkerSymbol);
    }

    [Fact]
    public async Task LoadEditSave_RoundTripsTheThreeLayoutFields()
    {
        var vm = Screen(out var sender, LoadedRows(lineSpacing: 1.25m, leftOffsetCm: 0.5m, markerSymbol: "+"));
        await vm.LoadAsync();

        var row = Row(vm, ReportItemKeys.ResultValue);
        Assert.Equal(1.25m, row.LineSpacing);
        Assert.Equal(0.5m, row.LeftOffsetCm);
        Assert.Equal("+", row.MarkerSymbol);

        await vm.SaveAsync();

        var saved = sender.SavedCommand!.Items.Single(i => i.ItemKey == ReportItemKeys.ResultValue);
        Assert.Equal(1.25m, saved.LineSpacing);
        Assert.Equal(0.5m, saved.LeftOffsetCm);
        Assert.Equal("+", saved.MarkerSymbol);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.49)]
    [InlineData(10.01)]
    public async Task Save_WithOutOfRangeLineSpacing_IsRejectedWithFeedback(double value)
    {
        var vm = Screen(out var sender);
        await vm.LoadAsync();
        Row(vm, ReportItemKeys.TestName).LineSpacing = (decimal)value;

        await vm.SaveAsync();

        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Null(sender.SavedCommand);
    }

    [Theory]
    [InlineData(10.01)]
    [InlineData(-0.5)]
    public async Task Save_WithOutOfRangeLeftOffset_IsRejectedWithFeedback(double value)
    {
        var vm = Screen(out var sender);
        await vm.LoadAsync();
        Row(vm, ReportItemKeys.TestName).LeftOffsetCm = (decimal)value;

        await vm.SaveAsync();

        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Null(sender.SavedCommand);
    }

    [Theory]
    [InlineData("**")]
    [InlineData("أ ب")]
    public async Task Save_WithMultiCharacterMarker_IsRejectedWithFeedback(string marker)
    {
        var vm = Screen(out var sender);
        await vm.LoadAsync();
        Row(vm, ReportItemKeys.TestName).MarkerSymbol = marker;

        await vm.SaveAsync();

        Assert.NotEmpty(vm.ErrorMessage);
        Assert.Null(sender.SavedCommand);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(10)]
    public async Task Save_WithBoundaryLineSpacing_IsAccepted(double value)
    {
        var vm = Screen(out var sender);
        await vm.LoadAsync();
        Row(vm, ReportItemKeys.TestName).LineSpacing = (decimal)value;

        await vm.SaveAsync();

        Assert.Empty(vm.ErrorMessage);
        Assert.NotNull(sender.SavedCommand);
        Assert.Equal((decimal)value, sender.SavedCommand!.Items.Single(i => i.ItemKey == ReportItemKeys.TestName).LineSpacing);
    }

    [Fact]
    public async Task Save_WithBoundaryLeftOffsetAndSingleMarker_IsAccepted()
    {
        var vm = Screen(out var sender);
        await vm.LoadAsync();
        var row = Row(vm, ReportItemKeys.TestName);
        row.LeftOffsetCm = ReportItemTemplate.MaxLeftOffsetCm;
        row.MarkerSymbol = "🔬";

        await vm.SaveAsync();

        Assert.Empty(vm.ErrorMessage);
        Assert.NotNull(sender.SavedCommand);
        var saved = sender.SavedCommand!.Items.Single(i => i.ItemKey == ReportItemKeys.TestName);
        Assert.Equal(ReportItemTemplate.MaxLeftOffsetCm, saved.LeftOffsetCm);
        Assert.Equal("🔬", saved.MarkerSymbol);
    }
}