using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetPatientSamplesDetail;

public sealed class GetPatientSamplesDetailQueryHandler
    : IRequestHandler<GetPatientSamplesDetailQuery, Result<IReadOnlyList<PatientSampleDetailDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetPatientSamplesDetailQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<IReadOnlyList<PatientSampleDetailDto>>> Handle(
        GetPatientSamplesDetailQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var periodPatients = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && p.RegistrationDateUtc >= fromStart
                && p.RegistrationDateUtc < toEndExclusive)
            .ToList();
        var patientIds = periodPatients.Select(p => p.Id.Value).ToList();

        var testsByPatient = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId.Value))
            .Where(pt => pt.CreatedAtUtc >= fromStart && pt.CreatedAtUtc < toEndExclusive)
            .ToList()
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var opsByPatient = _db.Set<PaymentOperation>()
            .Where(o => patientIds.Contains(o.PatientId.Value))
            .Where(o => o.OperationAtUtc >= fromStart && o.OperationAtUtc < toEndExclusive)
            .ToList()
            .GroupBy(o => o.PatientId.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = periodPatients
            .OrderBy(p => p.Id.Value)
            .Select(p =>
            {
                var tests = testsByPatient.TryGetValue(p.Id.Value, out var t) ? t : [];
                var ops = opsByPatient.TryGetValue(p.Id.Value, out var o) ? o : [];
                var prices = tests.Select(x => x.PriceAtOrderTime).ToList();
                var charged = PatientAccountCalculator.TotalCharged(prices, ops);
                var paid = PatientAccountCalculator.TotalPaid(ops);
                var balance = PatientAccountCalculator.Balance(prices, ops);
                return (p, tests, ops, charged, paid, balance);
            })
            .Where(x => x.charged != 0m || x.paid != 0m || x.tests.Count > 0)
            .Where(x => ApplyFilters(x.balance, x.ops, request))
            .Select(x => new PatientSampleDetailDto(
                x.p.Id.Value,
                x.p.FullName,
                x.tests.Count,
                x.charged,
                x.paid,
                x.balance))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PatientSampleDetailDto>>.Success(rows));
    }

    private static bool ApplyFilters(
        decimal balance,
        IReadOnlyList<PaymentOperation> ops,
        GetPatientSamplesDetailQuery request)
    {
        if (request.UnpaidOnly && balance <= 0m)
        {
            return false;
        }

        if (request.PaidOnly)
        {
            var hasPaymentOp = ops.Any(o => !o.IsVoided && !o.IsExtraCharge);
            if (balance != 0m || !hasPaymentOp)
            {
                return false;
            }
        }

        if (request.DiscountedOnly)
        {
            var hasDiscount = ops.Any(o => !o.IsVoided && (o.DiscountAmount ?? 0m) > 0m);
            if (!hasDiscount)
            {
                return false;
            }
        }

        return true;
    }
}
