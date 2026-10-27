using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Common;
using TopLab.Domain.Accounting;
using TopLab.Domain.Billing;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetAuditActivity;

/// <summary>
/// GAP-19 (4.5.1.2.2 / 4.5.1.2.3): read-only audit activity across the three
/// audit-bearing sources, filtered by an inclusive period and (optionally) by user.
/// Period is inclusive: From 00:00:00 through To 23:59:59.999.
/// UserIds null or empty means "show all".
/// </summary>
public sealed class GetAuditActivityQueryHandler
    : IRequestHandler<GetAuditActivityQuery, Result<IReadOnlyList<AuditActivityRowDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAuditActivityQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<AuditActivityRowDto>>> Handle(
        GetAuditActivityQuery request, CancellationToken cancellationToken)
    {
        var fromStart = request.From.ToDateTime(TimeOnly.MinValue);
        var toEnd = request.To.ToDateTime(new TimeOnly(23, 59, 59, 999));

        var allowed = (request.UserIds is null || request.UserIds.Count == 0)
            ? null
            : new HashSet<int>(request.UserIds);

        var userNames = _db.Set<TopLab.Domain.Users.User>()
            .ToDictionary(u => u.Id.Value, u => u.UserName);

        string Resolve(int userId) =>
            userNames.TryGetValue(userId, out var name) ? name : userId.ToString();

        var rows = new List<AuditActivityRowDto>();

        // PaymentOperation
        foreach (var o in _db.Set<PaymentOperation>().ToList())
        {
            if (o.OperationAtUtc < fromStart || o.OperationAtUtc > toEnd) continue;
            if (allowed is not null && !allowed.Contains(o.ReceivedByUserId)) continue;
            rows.Add(new AuditActivityRowDto(
                o.ReceivedByUserId,
                Resolve(o.ReceivedByUserId),
                "PaymentReceived",
                "استلام دفعة",
                o.OperationAtUtc,
                o.PatientId.Value,
                null,
                null));
        }

        // CashMovement
        foreach (var c in _db.Set<CashMovement>().ToList())
        {
            if (c.OccurredAtUtc < fromStart || c.OccurredAtUtc > toEnd) continue;
            if (allowed is not null && !allowed.Contains(c.PerformedByUserId)) continue;
            rows.Add(new AuditActivityRowDto(
                c.PerformedByUserId,
                Resolve(c.PerformedByUserId),
                "CashMovement",
                "حركة نقدية",
                c.OccurredAtUtc,
                null,
                null,
                c.Notes));
        }

        // PatientTest lifecycle
        foreach (var t in _db.Set<PatientTest>().ToList())
        {
            AddLifecycle(rows, t, allowed, fromStart, toEnd, Resolve, "ResultEntered", "إدخال نتيجة", t.EnteredByUserId, t.EnteredAtUtc, t.PatientId.Value, t.Id.Value);
            AddLifecycle(rows, t, allowed, fromStart, toEnd, Resolve, "ResultReviewed", "مراجعة نتيجة", t.ReviewedByUserId, t.ReviewedAtUtc, t.PatientId.Value, t.Id.Value);
            AddLifecycle(rows, t, allowed, fromStart, toEnd, Resolve, "ResultPrinted", "طباعة نتيجة", t.LastPrintedByUserId, t.LastPrintedAtUtc, t.PatientId.Value, t.Id.Value);
            AddLifecycle(rows, t, allowed, fromStart, toEnd, Resolve, "ResultDelivered", "تسليم نتيجة", t.DeliveredByUserId, t.DeliveredAtUtc, t.PatientId.Value, t.Id.Value);
        }

        var ordered = rows
            .OrderByDescending(r => r.OccurredAtUtc)
            .ThenBy(r => r.UserName)
            .ThenBy(r => r.ActivityCode)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<AuditActivityRowDto>>.Success(ordered));
    }

    private static void AddLifecycle(
        List<AuditActivityRowDto> rows,
        PatientTest t,
        HashSet<int>? allowed,
        DateTime fromStart,
        DateTime toEnd,
        Func<int, string> resolve,
        string code,
        string label,
        int? userId,
        DateTime? atUtc,
        int patientId,
        int patientTestId)
    {
        if (userId is null || atUtc is null) return;
        if (atUtc.Value < fromStart || atUtc.Value > toEnd) return;
        if (allowed is not null && !allowed.Contains(userId.Value)) return;
        rows.Add(new AuditActivityRowDto(
            userId.Value,
            resolve(userId.Value),
            code,
            label,
            atUtc.Value,
            patientId,
            patientTestId,
            null));
    }
}