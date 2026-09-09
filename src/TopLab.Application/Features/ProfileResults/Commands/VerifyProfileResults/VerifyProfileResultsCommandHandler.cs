using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ProfileResults.Commands.VerifyProfileResults;

/// <summary>
/// Reuses PatientTest.MarkReviewed and the idempotent per-item Verify. All
/// unprinted profile items of the test become verified and the order is reviewed.
/// </summary>
public sealed class VerifyProfileResultsCommandHandler : IRequestHandler<VerifyProfileResultsCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public VerifyProfileResultsCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(VerifyProfileResultsCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var items = _db.Set<ProfileResultItem>()
            .Where(i => i.PatientTestId.Value == pt.Id.Value && !i.IsPrinted)
            .ToList();

        foreach (var item in items)
        {
            item.Verify();
        }

        try
        {
            pt.MarkReviewed(_currentUser.UserId, _clock.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}