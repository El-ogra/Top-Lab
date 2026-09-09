using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;

/// <summary>
/// Authorised post-print amendment (Decision 3): the active item value/unit/flag is
/// changed immediately on the existing row (never unprint/new version) and a
/// complete immutable audit record is added in the SAME SaveChangesAsync, so both
/// commit or both roll back.
/// </summary>
public sealed class AmendProfileResultCommandHandler : IRequestHandler<AmendProfileResultCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public AmendProfileResultCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(AmendProfileResultCommand request, CancellationToken cancellationToken)
    {
        var item = _db.Set<ProfileResultItem>().FirstOrDefault(i => i.Id.Value == request.ProfileResultItemId);
        if (item is null)
        {
            return Result.Failure(Error.NotFound("نتيجة البروفايل غير موجودة."));
        }

        if (!item.IsPrinted)
        {
            return Result.Failure(Error.Conflict("لا يمكن تعديل نتيجة البروفايل قبل الطباعة."));
        }

        if (string.IsNullOrWhiteSpace(request.ResultValue))
        {
            return Result.Failure(Error.Validation("قيمة النتيجة مطلوبة."));
        }

        ProfileResultFlag? newFlag = request.Flag is null ? null : (ProfileResultFlag)request.Flag.Value;
        var oldValue = item.ResultValue;
        var oldUnit = item.Unit;
        var oldFlag = item.Flag;

        try
        {
            item.Amend(request.ResultValue, request.Unit, newFlag);

            var amendment = ProfileResultAmendment.Create(
                ProfileResultAmendmentId.Create(0),
                item.Id,
                _currentUser.UserId,
                _clock.UtcNow,
                oldValue,
                oldUnit,
                oldFlag,
                request.ResultValue,
                request.Unit,
                newFlag,
                request.Reason);

            _db.Add(amendment);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}