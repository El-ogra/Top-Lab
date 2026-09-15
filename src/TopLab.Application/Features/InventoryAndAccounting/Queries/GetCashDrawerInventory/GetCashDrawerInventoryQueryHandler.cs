using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Domain.Accounting;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetCashDrawerInventory;

public sealed class GetCashDrawerInventoryQueryHandler
    : IRequestHandler<GetCashDrawerInventoryQuery, Result<CashDrawerInventoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetCashDrawerInventoryQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<CashDrawerInventoryDto>> Handle(
        GetCashDrawerInventoryQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var livePatientIds = _db.Set<Patient>()
            .Where(p => !p.IsDeleted)
            .Select(p => p.Id.Value)
            .ToHashSet();

        var periodPatients = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && p.RegistrationDateUtc >= fromStart
                && p.RegistrationDateUtc < toEndExclusive)
            .ToList();

        var patientIds = periodPatients.Select(p => p.Id.Value).ToList();

        var patientTests = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId.Value))
            .Where(pt => pt.CreatedAtUtc >= fromStart && pt.CreatedAtUtc < toEndExclusive)
            .ToList();

        var prices = patientTests.Select(pt => pt.PriceAtOrderTime).ToList();
        var operations = _db.Set<PaymentOperation>()
            .Where(o => patientIds.Contains(o.PatientId.Value))
            .Where(o => o.OperationAtUtc >= fromStart && o.OperationAtUtc < toEndExclusive)
            .ToList();

        var totalSamplesCount = patientTests.Count;
        var totalSamplesAmount = prices.Sum();
        var discountsValue = operations
            .Where(o => !o.IsVoided && !o.IsExtraCharge)
            .Sum(o => o.DiscountAmount ?? 0m);
        var totalCharged = PatientAccountCalculator.TotalCharged(prices, operations);
        var totalAfterDiscount = totalCharged - discountsValue;
        var collected = operations
            .Where(o => !o.IsVoided && !o.IsExtraCharge)
            .Sum(o => o.Amount);
        var uncollected = PatientAccountCalculator.Balance(prices, operations);

        var cashMovements = _db.Set<CashMovement>()
            .Where(c => c.OccurredAtUtc >= fromStart && c.OccurredAtUtc < toEndExclusive)
            .ToList();
        var cashSupplies = cashMovements.Where(c => c.MovementType == MovementType.Deposit).Sum(c => c.Amount);
        var disbursements = cashMovements.Where(c => c.MovementType == MovementType.Disbursement).Sum(c => c.Amount);
        var safeCash = collected + cashSupplies - disbursements;

        var sentOutSamples = _db.Set<SentOutSample>()
            .Where(s => s.SentAtUtc >= fromStart && s.SentAtUtc < toEndExclusive)
            .ToList();
        var sentOutIds = sentOutSamples.Select(s => s.Id.Value).ToList();
        var sentOutPayments = _db.Set<SentOutSamplePayment>()
            .Where(p => sentOutIds.Contains(p.SentOutSampleId.Value))
            .ToList();
        var sentOutCost = SentOutAccountCalculator.TotalCost(sentOutSamples);
        var sentOutPaid = SentOutAccountCalculator.TotalPaid(sentOutPayments);

        var entityIds = periodPatients
            .SelectMany(p => new[] { p.ReferralEntityId?.Value, p.TreatingDoctorId?.Value })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var entities = _db.Set<ExternalEntity>()
            .Where(e => entityIds.Contains(e.Id.Value))
            .ToDictionary(e => e.Id.Value);

        var commissions = BuildCommissions(periodPatients, patientTests, entities);

        var dto = new CashDrawerInventoryDto(
            from,
            to,
            totalSamplesCount,
            totalSamplesAmount,
            discountsValue,
            totalAfterDiscount,
            collected,
            uncollected,
            cashSupplies,
            disbursements,
            safeCash,
            sentOutSamples.Count,
            sentOutCost,
            sentOutPaid,
            SentOutAccountCalculator.Remaining(sentOutCost, sentOutPaid),
            commissions,
            uncollected,
            collected - sentOutPaid - disbursements);

        return Task.FromResult(Result<CashDrawerInventoryDto>.Success(dto));
    }

    private static IReadOnlyList<CommissionShareDto> BuildCommissions(
        IReadOnlyList<Patient> patients,
        IReadOnlyList<PatientTest> tests,
        IReadOnlyDictionary<int, ExternalEntity> entities)
    {
        var results = new List<CommissionShareDto>();
        var chargeByPatient = tests
            .GroupBy(t => t.PatientId.Value)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.PriceAtOrderTime));

        void Consider(int? entityId, IEnumerable<int> patientIds)
        {
            if (!entityId.HasValue)
            {
                return;
            }

            if (!entities.TryGetValue(entityId.Value, out var entity)
                || entity.DiscountOrCommissionPercent is not > 0)
            {
                return;
            }

            var chargeBase = patientIds.Sum(id => chargeByPatient.TryGetValue(id, out var sum) ? sum : 0m);
            if (chargeBase <= 0m)
            {
                return;
            }

            var percent = entity.DiscountOrCommissionPercent.Value;
            results.Add(new CommissionShareDto(
                entity.Id.Value,
                entity.Name,
                percent,
                chargeBase,
                percent / 100m * chargeBase));
        }

        foreach (var group in patients.GroupBy(p => p.ReferralEntityId))
        {
            Consider(group.Key?.Value, group.Select(p => p.Id.Value));
        }

        foreach (var group in patients.GroupBy(p => p.TreatingDoctorId))
        {
            Consider(group.Key?.Value, group.Select(p => p.Id.Value));
        }

        return results
            .DistinctBy(c => c.EntityId)
            .OrderBy(c => c.EntityId)
            .ToList();
    }
}
