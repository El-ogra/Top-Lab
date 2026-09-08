using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultsEntry.Commands.MarkAllPatientResultsReviewed;

public sealed class MarkAllPatientResultsReviewedCommandHandler
    : IRequestHandler<MarkAllPatientResultsReviewedCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public MarkAllPatientResultsReviewedCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(MarkAllPatientResultsReviewedCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        var rows = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patient.Id.Value)
            .ToList();

        var count = 0;
        foreach (var pt in rows)
        {
            if (pt.IsReviewed || pt.EnteredAtUtc is null)
            {
                continue;
            }

            try
            {
                pt.MarkReviewed(_currentUser.UserId, _clock.UtcNow);
                count++;
            }
            catch (InvalidOperationException)
            {
                continue;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}
