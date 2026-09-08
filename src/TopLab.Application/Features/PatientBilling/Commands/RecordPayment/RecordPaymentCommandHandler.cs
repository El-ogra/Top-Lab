using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordPayment;

/// <summary>
/// Records a routine patient payment (registrar/cashier action — ungated).
/// When a discount is supplied it is capped per user at the application layer
/// against User.DiscountLimitPercent (Data Model §13 BR-06); absolute-permission
/// users are exempt from the cap (FR-M17-004 absolute/limited model).
/// </summary>
public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RecordPaymentCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (request.DiscountAmount.HasValue && !_currentUser.IsAbsolutePermission)
        {
            var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);
            // A missing user row carries no limit grant, so the effective cap is zero:
            // any positive discount breaches. Uses the frozen breach message — no new strings.
            var limitPercent = user?.DiscountLimitPercent ?? 0m;
            if (request.DiscountAmount.Value > request.Amount * limitPercent / 100m)
            {
                return Result<int>.Failure(Error.Validation("الخصم يتجاوز الحد المسموح به لهذا المستخدم."));
            }
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
                request.DiscountAmount);
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
