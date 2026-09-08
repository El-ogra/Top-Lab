using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordCorrection;

/// <summary>
/// Records an accounting-control correction (gated on CASH_DISBURSE_DEPOSIT).
/// Sign convention (settled pin): a positive Amount is a credit that reduces
/// the balance — Correction rows contribute to the paid side of the formula.
/// </summary>
public sealed class RecordCorrectionCommandHandler : IRequestHandler<RecordCorrectionCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RecordCorrectionCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(RecordCorrectionCommand request, CancellationToken cancellationToken)
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
                false,
                OperationType.Correction);
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
