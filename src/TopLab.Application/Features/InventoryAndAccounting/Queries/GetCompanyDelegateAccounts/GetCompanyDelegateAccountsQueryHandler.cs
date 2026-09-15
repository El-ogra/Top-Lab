using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Domain.Accounting;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetCompanyDelegateAccounts;

public sealed class GetCompanyDelegateAccountsQueryHandler
    : IRequestHandler<GetCompanyDelegateAccountsQuery, Result<IReadOnlyList<CompanyDelegateAccountDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetCompanyDelegateAccountsQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<IReadOnlyList<CompanyDelegateAccountDto>>> Handle(
        GetCompanyDelegateAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        if (request.ExternalEntityId.HasValue
            && !_db.Set<ExternalEntity>().Any(e => e.Id.Value == request.ExternalEntityId.Value))
        {
            return Task.FromResult(Result<IReadOnlyList<CompanyDelegateAccountDto>>.Failure(
                Error.NotFound("الجهة الخارجية غير موجودة.", "NotFound")));
        }

        var cashQuery = _db.Set<CashMovement>()
            .Where(c => c.OccurredAtUtc >= fromStart && c.OccurredAtUtc < toEndExclusive)
            .Where(c => c.RelatedExternalEntityId != null);

        if (request.ExternalEntityId.HasValue)
        {
            var filterId = request.ExternalEntityId.Value;
            cashQuery = cashQuery.Where(c => c.RelatedExternalEntityId!.Value == filterId);
        }

        var movements = cashQuery.ToList();
        var entityIds = movements
            .Select(m => m.RelatedExternalEntityId!.Value)
            .Distinct()
            .ToList();

        var entityNames = _db.Set<ExternalEntity>()
            .Where(e => entityIds.Contains(e.Id.Value))
            .ToDictionary(e => e.Id.Value, e => e.Name);

        var sentOutQuery = _db.Set<SentOutSample>()
            .Where(s => s.SentAtUtc >= fromStart && s.SentAtUtc < toEndExclusive);
        var sentOut = sentOutQuery.ToList();
        var sentOutIds = sentOut.Select(s => s.Id.Value).ToList();
        var sentOutPayments = _db.Set<SentOutSamplePayment>()
            .Where(p => sentOutIds.Contains(p.SentOutSampleId.Value))
            .ToList();
        var paymentsBySample = sentOutPayments
            .GroupBy(p => p.SentOutSampleId.Value)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.AmountPaid));

        var rows = movements
            .GroupBy(m => m.RelatedExternalEntityId!.Value)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var deposits = g.Where(m => m.MovementType == MovementType.Deposit).Sum(m => m.Amount);
                var disbursements = g.Where(m => m.MovementType == MovementType.Disbursement).Sum(m => m.Amount);
                var name = entityNames.TryGetValue(g.Key, out var entityName)
                    ? entityName
                    : g.Key.ToString();

                // Sent-out settlement position: samples for this lab entity in the period.
                var labSamples = sentOut.Where(s => s.ExternalLabEntityId.Value == g.Key).ToList();
                var labPaid = labSamples
                    .Select(s => paymentsBySample.TryGetValue(s.Id.Value, out var paid) ? paid : 0m)
                    .Sum();
                var labCost = SentOutAccountCalculator.TotalCost(labSamples);

                return new CompanyDelegateAccountDto(
                    g.Key,
                    name,
                    deposits,
                    disbursements,
                    deposits - disbursements,
                    labSamples.Count,
                    labCost,
                    labPaid,
                    SentOutAccountCalculator.Remaining(labCost, labPaid));
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CompanyDelegateAccountDto>>.Success(rows));
    }
}
