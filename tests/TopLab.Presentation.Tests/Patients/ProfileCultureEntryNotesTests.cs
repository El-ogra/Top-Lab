using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// R1 Round 3 A3 (GAP-05): <c>PatientNotes</c> is loaded on the profile and culture
/// result-entry screens and is empty when the DTO carries null.
///
/// Round 2b claimed these tests were impossible. They are not: <see cref="ProfileEntryViewModel"/>
/// and <see cref="CultureEntryViewModel"/> take several ports, but the ones
/// <c>LoadAsync</c> does not touch (<c>IApplicationDbContext</c>, <c>IPdfPreviewService</c>,
/// <c>IResultPrintCoordinator</c>, <c>IPrintedStateRecorder</c>) are passed as <c>null!</c>
/// deliberately — <c>LoadAsync</c> reads only the mediator and sets the properties asserted
/// here, so a null dependency can never be dereferenced on this path. Every dependency the
/// path does use is a real fake (<see cref="FakeSender"/>, <see cref="FakeDialogService"/>).
/// </summary>
public class ProfileCultureEntryNotesTests
{
    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private const int PatientTestId = 42;

    /// <summary>
    /// The three ports <c>LoadAsync</c> never touches on this path. They are supplied as
    /// explicit no-op fakes (never <c>null</c>) so the test cannot pass because of a missing
    /// dependency: if a future <c>LoadAsync</c> starts using one, the fake throws loudly.
    /// </summary>
    private sealed class UnusedDbContext : IApplicationDbContext
    {
        public IQueryable<TEntity> Set<TEntity>() where TEntity : class
            => throw new NotSupportedException("LoadAsync must not touch the database on this path.");
        public void Add<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Update<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class UnusedPdfPreview : IPdfPreviewService
    {
        public Task<Result<string>> PreviewAsync(string absolutePdfPath, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("LoadAsync must not preview on this path.");
    }

    private sealed class UnusedPrintCoordinator : IResultPrintCoordinator
    {
        public Task<ResultPrintOutcome> PrintAsync(int patientTestId, ResultPrintKind kind, CancellationToken ct = default)
            => throw new NotSupportedException("LoadAsync must not print on this path.");
    }

    private sealed class UnusedPrintedState : IPrintedStateRecorder
    {
        public Task<Result> RecordAsync(IReadOnlyCollection<int> patientTestIds, IReadOnlyCollection<int> profileResultItemIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task<Result> RecordForPatientTestAsync(int patientTestId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static ProfileEntryGridDto ProfileGrid(string? notes) => new(
        PatientTestId: PatientTestId,
        PatientId: 7,
        PatientFullName: "أحمد محمد علي",
        ProfileName: "صورة دم كاملة",
        Items: Array.Empty<ProfileEntryItemDto>(),
        TestId: 3,
        PatientNotes: notes);

    private static CultureEntryGridDto CultureGrid(string? notes) => new(
        PatientTestId: PatientTestId,
        PatientId: 7,
        TestName: "مزرعة",
        TestCode: "CUL",
        Sample: null,
        OrganismA: null,
        OrganismB: null,
        OrganismC: null,
        CultureCondition: null,
        ColonyCount: null,
        Rows: Array.Empty<CultureSensitivityRowDto>(),
        ParentIsReviewed: false,
        ParentIsPrinted: false,
        Microscopy: null,
        PatientNotes: notes);

    private static ProfileEntryViewModel ProfileVm(string? notes)
    {
        var sender = new FakeSender();
        sender.WithResponse(
            new GetProfileEntryGridQuery(PatientTestId),
            Result<ProfileEntryGridDto>.Success(ProfileGrid(notes)));

        return new ProfileEntryViewModel(
            sender,
            new ResultErrorPresenter(),
            new FakeDialogService(),
            services: new StubServiceProvider(),
            pdfPreview: new UnusedPdfPreview(),
            db: new UnusedDbContext(),
            printCoordinator: new UnusedPrintCoordinator(),
            printedState: new UnusedPrintedState());
    }

    private static CultureEntryViewModel CultureVm(string? notes)
    {
        var sender = new FakeSender();
        sender.WithResponse(
            new GetCultureEntryGridQuery(PatientTestId),
            Result<CultureEntryGridDto>.Success(CultureGrid(notes)));

        return new CultureEntryViewModel(
            sender,
            new ResultErrorPresenter(),
            new FakeDialogService(),
            printCoordinator: new UnusedPrintCoordinator(),
            printedState: new UnusedPrintedState());
    }

    [Fact]
    public async Task ProfileEntry_PatientNotes_Loaded_WhenPresent()
    {
        var vm = ProfileVm("ملاحظات المريض");

        await vm.LoadAsync(PatientTestId);

        Assert.Equal("ملاحظات المريض", vm.PatientNotes);
    }

    [Fact]
    public async Task ProfileEntry_PatientNotes_Empty_WhenNull()
    {
        var vm = ProfileVm(null);

        await vm.LoadAsync(PatientTestId);

        Assert.Equal(string.Empty, vm.PatientNotes);
    }

    [Fact]
    public async Task CultureEntry_PatientNotes_Loaded_WhenPresent()
    {
        var vm = CultureVm("ملاحظات المريض");

        await vm.LoadAsync(PatientTestId);

        Assert.Equal("ملاحظات المريض", vm.PatientNotes);
    }

    [Fact]
    public async Task CultureEntry_PatientNotes_Empty_WhenNull()
    {
        var vm = CultureVm(null);

        await vm.LoadAsync(PatientTestId);

        Assert.Equal(string.Empty, vm.PatientNotes);
    }
}
