using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;

/// <summary>
/// Pays the remaining balance in full («خلاص» then «موافق» — plain payment,
/// ungated). Carries no discount field: a cashier combining settlement with a
/// discount issues a <c>RecordPaymentCommand</c> for the balance amount instead.
/// </summary>
public sealed class SettleAccountInFullCommandHandler : IRequestHandler<SettleAccountInFullCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SettleAccountInFullCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(SettleAccountInFullCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        var prices = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patient.Id.Value)
            .Select(pt => pt.PriceAtOrderTime)
            .ToList();
        var operations = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == patient.Id.Value)
            .ToList();

        var balance = PatientAccountCalculator.Balance(prices, operations);
        if (balance <= 0)
        {
            return Result<int>.Failure(Error.Conflict("لا يوجد رصيد مستحق للتسوية."));
        }

        PaymentOperation settlement;
        try
        {
            settlement = PaymentOperation.Create(
                PaymentOperationId.Create(0),
                PatientId.Create(request.PatientId),
                balance,
                _currentUser.UserId,
                _clock.UtcNow,
                null,
                false,
                OperationType.FullSettlement);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(settlement);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(settlement.Id.Value);
    }
}
