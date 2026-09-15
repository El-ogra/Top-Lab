using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;

/// <summary>
/// Atomic handover action (BR-08): printed-only per-line delivery with audit,
/// plus optional settlement — a partial <c>Payment</c> by default or a "خلاص"
/// full settlement delegating to the M-03 settle-in-full logic. No balance gate
/// on delivery itself: delivery is the physical handover; the BR-07 block lives
/// before printing upstream. One handler, one <c>SaveChanges</c>.
/// </summary>
public sealed class DeliverWithSettlementCommandHandler : IRequestHandler<DeliverWithSettlementCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public DeliverWithSettlementCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(DeliverWithSettlementCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        var wantedIds = request.PatientTestIds.Distinct().ToList();
        var lines = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == request.PatientId && wantedIds.Contains(pt.Id.Value))
            .ToList();

        if (lines.Count != wantedIds.Count)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        foreach (var line in lines)
        {
            try
            {
                line.MarkDelivered(_currentUser.UserId, _clock.UtcNow);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
            }
        }

        if (request.SettleInFull)
        {
            var balance = Balance(request.PatientId);
            if (balance <= 0)
            {
                return Result.Failure(Error.Conflict("لا يوجد رصيد مستحق للتسوية."));
            }

            _db.Add(PaymentOperation.Create(
                PaymentOperationId.Create(0),
                PatientId.Create(request.PatientId),
                balance,
                _currentUser.UserId,
                _clock.UtcNow,
                null,
                false,
                OperationType.FullSettlement));
        }
        else if (request.SettleAmount.HasValue && request.SettleAmount.Value > 0)
        {
            _db.Add(PaymentOperation.Create(
                PaymentOperationId.Create(0),
                PatientId.Create(request.PatientId),
                request.SettleAmount.Value,
                _currentUser.UserId,
                _clock.UtcNow));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private decimal Balance(int patientId)
    {
        var prices = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patientId)
            .Select(pt => pt.PriceAtOrderTime)
            .ToList();

        var operations = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == patientId)
            .ToList();

        return PatientAccountCalculator.Balance(prices, operations);
    }
}
