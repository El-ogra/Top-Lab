using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Lab;
using Xunit;

namespace TopLab.Presentation.Tests.Lab;

/// <summary>
/// GAP-017: the profile editor must load the four per-profile result-styling values into the
/// view model and send them back on save. Phase 10 fields only.
/// </summary>
public class ProfileEditorViewModelStylingTests
{
    private static readonly ResultErrorPresenter Presenter = new();

    /// <summary>Records the CreateProfileCommand the editor actually sends.</summary>
    private sealed class RecordingSender : ISender
    {
        public CreateProfileCommand? CreatedProfile { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is CreateProfileCommand create)
            {
                CreatedProfile = create;
                return Task.FromResult((TResponse)(object)Result<int>.Success(1));
            }

            if (request is SearchTestCatalogQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<TestSummaryDto>>.Success(
                    Array.Empty<TestSummaryDto>()));
            }

            if (request is GetAnalyteDefinitionsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<AnalyteDefinitionDto>>.Success(
                    Array.Empty<AnalyteDefinitionDto>()));
            }

            if (request is GetProfileDefinitionsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<IReadOnlyList<ProfileDefinitionDto>>.Success(
                    Array.Empty<ProfileDefinitionDto>()));
            }

            throw new NotSupportedException($"RecordingSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private static ProfileEditorViewModel CreateViewModel(RecordingSender sender)
        => new(sender, new FakeDialogService(), Presenter);

    private static ProfileDefinitionDto Definition(
        string? normalFont = null,
        string? normalColor = null,
        string? abnormalFont = null,
        string? abnormalBackgroundColor = null)
        => new(
            ProfileId: 7,
            Name: "صورة الدم",
            SpecializedTestId: 50,
            SpecializedTestName: "صورة الدم",
            FixedPrice: 300m,
            IsActive: true,
            AnalyteIds: new[] { 1 },
            normalFont,
            normalColor,
            abnormalFont,
            abnormalBackgroundColor);

    [Fact]
    public async Task Load_PopulatesTheFourStylingFields()
    {
        var vm = CreateViewModel(new RecordingSender());

        await vm.InitializeManageAsync(Definition("#00FF00", "#EEEEEE", "#FF0000", "#D9D9D9"));

        Assert.Equal("#00FF00", vm.NormalFont);
        Assert.Equal("#EEEEEE", vm.NormalColor);
        Assert.Equal("#FF0000", vm.AbnormalFont);
        Assert.Equal("#D9D9D9", vm.AbnormalBackgroundColor);
    }

    [Fact]
    public async Task Load_WithoutStyling_LeavesTheFourFieldsNull()
    {
        var vm = CreateViewModel(new RecordingSender());

        await vm.InitializeManageAsync(Definition());

        Assert.Null(vm.NormalFont);
        Assert.Null(vm.NormalColor);
        Assert.Null(vm.AbnormalFont);
        Assert.Null(vm.AbnormalBackgroundColor);
    }

    [Fact]
    public async Task InitializeNew_ClearsAnyPreviouslyLoadedStyling()
    {
        var vm = CreateViewModel(new RecordingSender());
        await vm.InitializeManageAsync(Definition("#00FF00", "#EEEEEE", "#FF0000", "#D9D9D9"));

        await vm.InitializeNewAsync();

        Assert.Null(vm.NormalFont);
        Assert.Null(vm.NormalColor);
        Assert.Null(vm.AbnormalFont);
        Assert.Null(vm.AbnormalBackgroundColor);
    }

    [Fact]
    public async Task Save_SendsTheFourStylingFields()
    {
        var sender = new RecordingSender();
        var vm = CreateViewModel(sender);
        await vm.InitializeNewAsync();

        vm.Name = "صورة الدم";
        vm.SpecializedTestId = 50;
        vm.FixedPrice = 300m;
        vm.NormalFont = "#00FF00";
        vm.NormalColor = "#EEEEEE";
        vm.AbnormalFont = "#FF0000";
        vm.AbnormalBackgroundColor = "#D9D9D9";

        await vm.SaveAsync();

        Assert.Empty(vm.ErrorMessage);
        Assert.NotNull(sender.CreatedProfile);
        Assert.Equal("#00FF00", sender.CreatedProfile!.NormalFont);
        Assert.Equal("#EEEEEE", sender.CreatedProfile.NormalColor);
        Assert.Equal("#FF0000", sender.CreatedProfile.AbnormalFont);
        Assert.Equal("#D9D9D9", sender.CreatedProfile.AbnormalBackgroundColor);
    }

    [Fact]
    public async Task Save_WithoutStyling_SendsNulls()
    {
        var sender = new RecordingSender();
        var vm = CreateViewModel(sender);
        await vm.InitializeNewAsync();

        vm.Name = "صورة الدم";
        vm.SpecializedTestId = 50;
        vm.FixedPrice = 300m;

        await vm.SaveAsync();

        Assert.Empty(vm.ErrorMessage);
        Assert.NotNull(sender.CreatedProfile);
        Assert.Null(sender.CreatedProfile!.NormalFont);
        Assert.Null(sender.CreatedProfile.NormalColor);
        Assert.Null(sender.CreatedProfile.AbnormalFont);
        Assert.Null(sender.CreatedProfile.AbnormalBackgroundColor);
    }
}