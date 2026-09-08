using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;

/// <summary>
/// Records a late-added service fee on the charged side (registrar action —
/// ungated). Carries no discount by design: the Domain guard forbids combining
/// an extra charge with a discount, surfaced here through the translator.
/// </summary>
public sealed class RecordExtraChargeCommandHandler : IRequestHandler<RecordExtraChargeCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RecordExtraChargeCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(RecordExtraChargeCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        PaymentOperation operation;
        try
        {
            operation = PaymentOperation.Create(
                PaymentOperationId.Create(0),
                PatientId.Create(request.PatientId),
                request.Amount,
                _currentUser.UserId,
                _clock.UtcNow,
                null,
                true);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(operation);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(operation.Id.Value);
    }
}
