using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Test double for <see cref="IPrintedStateRecorder"/>.
/// <para>
/// W-02 post-implementation fix. Default mode (<c>UseReal = true</c>) delegates to the real
/// <see cref="PrintedStateRecorder"/> over the test's own fake DbContext, so a test that
/// prints through a production printing path also exercises the real domain transition
/// (<c>MarkPrinted</c>) and the real persistence call. That is deliberate: the defect this
/// fix repairs was invisible precisely because tests arranged <c>IsPrinted</c> by hand instead
/// of letting a print produce it.
/// </para>
/// <para>
/// Set <c>UseReal = false</c> only to assert that a path calls the recorder at all, or to
/// force a recording failure.
/// </para>
/// </summary>
public sealed class FakePrintedStateRecorder : IPrintedStateRecorder
{
    private readonly PrintedStateRecorder? _real;

    public FakePrintedStateRecorder(IApplicationDbContext db, ICurrentUserService? user = null, IDateTimeProvider? clock = null)
    {
        _real = new PrintedStateRecorder(
            db,
            user ?? new FakeCurrentUserService { UserId = 1 },
            clock ?? new FakeDateTimeProvider());
    }

    /// <summary>When false, nothing is recorded — used to assert "recorder was invoked".</summary>
    public bool UseReal { get; set; } = true;

    /// <summary>Forces <see cref="RecordAsync"/> to fail, to test failure propagation.</summary>
    public bool FailRecording { get; set; }

    public List<(IReadOnlyCollection<int> Tests, IReadOnlyCollection<int> Items)> Calls { get; } = new();

    public Task<Result> RecordAsync(
        IReadOnlyCollection<int> patientTestIds,
        IReadOnlyCollection<int> profileResultItemIds,
        CancellationToken cancellationToken = default)
    {
        Calls.Add((patientTestIds, profileResultItemIds));

        if (FailRecording)
        {
            return Task.FromResult(Result.Failure(Error.Conflict("تعذّر تسجيل حالة الطباعة.")));
        }

        if (!UseReal || _real is null)
        {
            return Task.FromResult(Result.Success());
        }

        return _real.RecordAsync(patientTestIds, profileResultItemIds, cancellationToken);
    }

    public Task<Result> RecordForPatientTestAsync(
        int patientTestId,
        CancellationToken cancellationToken = default)
    {
        Calls.Add(([patientTestId], Array.Empty<int>()));

        if (FailRecording)
        {
            return Task.FromResult(Result.Failure(Error.Conflict("تعذّر تسجيل حالة الطباعة.")));
        }

        if (!UseReal || _real is null)
        {
            return Task.FromResult(Result.Success());
        }

        return _real.RecordForPatientTestAsync(patientTestId, cancellationToken);
    }
}