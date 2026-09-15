using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Accounting;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDeposit;

public sealed class RecordCashDepositCommandHandler
    : IRequestHandler<RecordCashDepositCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public RecordCashDepositCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<int>> Handle(
        RecordCashDepositCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RelatedExternalEntityId.HasValue
            && !_db.Set<ExternalEntity>().Any(e => e.Id.Value == request.RelatedExternalEntityId.Value))
        {
            return Result<int>.Failure(
                Error.NotFound("الجهة الخارجية غير موجودة.", "NotFound"));
        }

        var movement = CashMovement.Create(
            CashMovementId.Create(0),
            MovementType.Deposit,
            request.Amount,
            _currentUser.UserId,
            _dateTime.UtcNow,
            request.RelatedExternalEntityId.HasValue
                ? ExternalEntityId.Create(request.RelatedExternalEntityId.Value)
                : null,
            request.Notes);

        _db.Add(movement);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(movement.Id.Value);
    }
}
