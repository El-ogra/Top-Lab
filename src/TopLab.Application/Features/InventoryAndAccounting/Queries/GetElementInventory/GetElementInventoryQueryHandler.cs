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
using TopLab.Domain.Users;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;

public sealed class GetElementInventoryQueryHandler
    : IRequestHandler<GetElementInventoryQuery, Result<ElementInventoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetElementInventoryQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<ElementInventoryDto>> Handle(
        GetElementInventoryQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var from = request.From ?? today;
        var to = request.To ?? today;
        var fromStart = from.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var elementId = request.ElementId ?? 0;
        var elementName = string.Empty;

        if (request.Element == InventoryElementKind.User)
        {
            if (!request.ElementId.HasValue
                || !_db.Set<User>().Any(u => u.Id.Value == request.ElementId.Value))
            {
                return Task.FromResult(Result<ElementInventoryDto>.Failure(
                    Error.NotFound("المستخدم غير موجود.", "NotFound")));
            }

            elementId = request.ElementId.Value;
            elementName = _db.Set<User>().Where(u => u.Id.Value == elementId)
                .Select(u => u.UserName).FirstOrDefault() ?? elementId.ToString();
        }
        else if (request.Element is InventoryElementKind.ReferralEntity or InventoryElementKind.TreatingDoctor)
        {
            if (!request.ElementId.HasValue
                || !_db.Set<ExternalEntity>().Any(e => e.Id.Value == request.ElementId.Value))
            {
                return Task.FromResult(Result<ElementInventoryDto>.Failure(
                    Error.NotFound("الجهة الخارجية غير موجودة.", "NotFound")));
            }

            elementId = request.ElementId.Value;
            elementName = _db.Set<ExternalEntity>().Where(e => e.Id.Value == elementId)
                .Select(e => e.Name).FirstOrDefault() ?? elementId.ToString();
        }
        else if (request.Element == InventoryElementKind.AccountType)
        {
            if (!request.AccountType.HasValue)
            {
                return Task.FromResult(Result<ElementInventoryDto>.Failure(
                    Error.Validation("نوع الحساب مطلوب.", "Validation")));
            }

            elementId = (int)request.AccountType.Value;
            elementName = request.AccountType.Value.ToString();
        }

        var periodPatients = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && p.RegistrationDateUtc >= fromStart
                && p.RegistrationDateUtc < toEndExclusive)
            .ToList();

        periodPatients = request.Element switch
        {
            InventoryElementKind.ReferralEntity => periodPatients
                .Where(p => p.ReferralEntityId != null && p.ReferralEntityId.Value == elementId)
                .ToList(),
            InventoryElementKind.TreatingDoctor => periodPatients
                .Where(p => p.TreatingDoctorId != null && p.TreatingDoctorId.Value == elementId)
                .ToList(),
            InventoryElementKind.AccountType => periodPatients
                .Where(p => p.AccountType == request.AccountType!.Value)
                .ToList(),
            _ => periodPatients
        };

        var patientIds = periodPatients.Select(p => p.Id.Value).ToList();

        var patientTests = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId.Value))
            .Where(pt => pt.CreatedAtUtc >= fromStart && pt.CreatedAtUtc < toEndExclusive)
            .ToList();

        var operations = _db.Set<PaymentOperation>()
            .Where(o => patientIds.Contains(o.PatientId.Value))
            .Where(o => o.OperationAtUtc >= fromStart && o.OperationAtUtc < toEndExclusive)
            .ToList();

        if (request.Element == InventoryElementKind.User)
        {
            patientTests = patientTests
                .Where(t => t.EnteredByUserId == elementId)
                .ToList();
            operations = operations
                .Where(o => o.ReceivedByUserId == elementId)
                .ToList();
        }

        var prices = patientTests.Select(pt => pt.PriceAtOrderTime).ToList();
        var totalCharged = PatientAccountCalculator.TotalCharged(prices, operations);
        var discountsValue = operations
            .Where(o => !o.IsVoided && !o.IsExtraCharge)
            .Sum(o => o.DiscountAmount ?? 0m);
        var collected = operations
            .Where(o => !o.IsVoided && !o.IsExtraCharge)
            .Sum(o => o.Amount);
        var uncollected = PatientAccountCalculator.Balance(prices, operations);

        var cashMovements = _db.Set<CashMovement>()
            .Where(c => c.OccurredAtUtc >= fromStart && c.OccurredAtUtc < toEndExclusive)
            .AsEnumerable();
        if (request.Element is InventoryElementKind.ReferralEntity or InventoryElementKind.TreatingDoctor)
        {
            cashMovements = cashMovements
                .Where(c => c.RelatedExternalEntityId != null && c.RelatedExternalEntityId.Value == elementId);
        }
        else if (request.Element == InventoryElementKind.User)
        {
            cashMovements = cashMovements.Where(c => c.PerformedByUserId == elementId);
        }

        var cashList = cashMovements.ToList();
        var cashSupplies = cashList.Where(c => c.MovementType == MovementType.Deposit).Sum(c => c.Amount);
        var disbursements = cashList.Where(c => c.MovementType == MovementType.Disbursement).Sum(c => c.Amount);

        IEnumerable<SentOutSample> sentOut = _db.Set<SentOutSample>()
            .Where(s => s.SentAtUtc >= fromStart && s.SentAtUtc < toEndExclusive)
            .ToList();
        if (request.Element is InventoryElementKind.ReferralEntity or InventoryElementKind.TreatingDoctor)
        {
            // Sent-out is lab-bound; for entity elements only include samples linked through period patients of that entity.
            var testsOfEntityPatients = patientTests.Select(t => t.Id.Value).ToHashSet();
            sentOut = sentOut.Where(s => testsOfEntityPatients.Contains(s.PatientTestId.Value));
        }
        else if (request.Element != InventoryElementKind.SentOutSamples)
        {
            sentOut = [];
        }

        var sentOutList = sentOut.ToList();
        var sentOutIds = sentOutList.Select(s => s.Id.Value).ToList();
        var sentOutPayments = _db.Set<SentOutSamplePayment>()
            .Where(p => sentOutIds.Contains(p.SentOutSampleId.Value))
            .ToList();
        var sentOutCost = SentOutAccountCalculator.TotalCost(sentOutList);
        var sentOutPaid = SentOutAccountCalculator.TotalPaid(sentOutPayments);

        var lines = BuildLines(request, patientTests, operations, sentOutList, request.AccountType);

        var dto = new ElementInventoryDto(
            from,
            to,
            request.Element,
            request.ReportType,
            elementId,
            elementName,
            patientTests.Count,
            prices.Sum(),
            discountsValue,
            totalCharged - discountsValue,
            collected,
            uncollected,
            cashSupplies,
            disbursements,
            collected + cashSupplies - disbursements,
            sentOutList.Count,
            sentOutCost,
            sentOutPaid,
            SentOutAccountCalculator.Remaining(sentOutCost, sentOutPaid),
            uncollected,
            collected - sentOutPaid - disbursements,
            lines);

        return Task.FromResult(Result<ElementInventoryDto>.Success(dto));
    }

    private static IReadOnlyList<ElementLineDto> BuildLines(
        GetElementInventoryQuery request,
        IReadOnlyList<PatientTest> tests,
        IReadOnlyList<PaymentOperation> operations,
        IReadOnlyList<SentOutSample> sentOut,
        AccountType? accountType)
    {
        return request.ReportType switch
        {
            InventoryReportType.Summary => [],
            InventoryReportType.DetailedByResults => tests
                .GroupBy(t => t.TestId.Value)
                .OrderBy(g => g.Key)
                .Select(g => new ElementLineDto(g.Key, g.Key.ToString(), g.Count(), g.Sum(t => t.PriceAtOrderTime)))
                .ToList(),
            InventoryReportType.DetailedByPrices => operations
                .Where(o => !o.IsVoided)
                .GroupBy(o => o.ReceivedByUserId)
                .OrderBy(g => g.Key)
                .Select(g => new ElementLineDto(
                    g.Key,
                    g.Key.ToString(),
                    g.Count(),
                    g.Sum(o => o.Amount)))
                .ToList(),
            InventoryReportType.Detailed => sentOut
                .GroupBy(s => s.ExternalLabEntityId.Value)
                .OrderBy(g => g.Key)
                .Select(g => new ElementLineDto(
                    g.Key,
                    g.Key.ToString(),
                    g.Count(),
                    g.Sum(s => s.CostPrice)))
                .ToList(),
            _ => []
        };
    }
}
