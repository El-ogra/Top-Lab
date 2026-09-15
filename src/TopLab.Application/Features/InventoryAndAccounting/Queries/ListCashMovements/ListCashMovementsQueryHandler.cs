using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Domain.Accounting;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.ListCashMovements;

public sealed class ListCashMovementsQueryHandler
    : IRequestHandler<ListCashMovementsQuery, Result<IReadOnlyList<CashMovementDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public ListCashMovementsQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<IReadOnlyList<CashMovementDto>>> Handle(
        ListCashMovementsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var movements = _db.Set<CashMovement>()
            .Where(c => c.OccurredAtUtc >= fromStart && c.OccurredAtUtc < toEndExclusive)
            .OrderBy(c => c.OccurredAtUtc)
            .ToList();

        var entityIds = movements
            .Where(m => m.RelatedExternalEntityId != null)
            .Select(m => m.RelatedExternalEntityId!.Value)
            .Distinct()
            .ToList();
        var entityNames = _db.Set<ExternalEntity>()
            .Where(e => entityIds.Contains(e.Id.Value))
            .ToDictionary(e => e.Id.Value, e => e.Name);

        var userIds = movements.Select(m => m.PerformedByUserId).Distinct().ToList();
        var userNames = _db.Set<User>()
            .Where(u => userIds.Contains(u.Id.Value))
            .ToDictionary(u => u.Id.Value, u => u.UserName);

        IReadOnlyList<CashMovementDto> rows = movements
            .Select(m =>
            {
                int? entityId = m.RelatedExternalEntityId?.Value;
                string? entityName = null;
                if (entityId.HasValue)
                {
                    entityName = entityNames.TryGetValue(entityId.Value, out var name)
                        ? name
                        : entityId.Value.ToString();
                }

                var performerName = userNames.TryGetValue(m.PerformedByUserId, out var userName)
                    ? userName
                    : m.PerformedByUserId.ToString();

                return new CashMovementDto(
                    m.Id.Value,
                    m.MovementType,
                    m.Amount,
                    entityId,
                    entityName,
                    m.PerformedByUserId,
                    performerName,
                    m.OccurredAtUtc,
                    m.Notes);
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CashMovementDto>>.Success(rows));
    }
}
