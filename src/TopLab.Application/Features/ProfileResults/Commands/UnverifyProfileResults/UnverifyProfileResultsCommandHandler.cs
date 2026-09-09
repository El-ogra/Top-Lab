using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ProfileResults.Commands.UnverifyProfileResults;

/// <summary>
/// Reuses PatientTest.Unreview and the idempotent per-item Unverify; allowed only
/// when nothing is printed. Printed results cannot be un-reviewed (M-04 guard).
/// </summary>
public sealed class UnverifyProfileResultsCommandHandler : IRequestHandler<UnverifyProfileResultsCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UnverifyProfileResultsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UnverifyProfileResultsCommand request, CancellationToken cancellationToken)
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
            item.Unverify();
        }

        try
        {
            pt.Unreview();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}