using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;
using DomainFailureTranslator = TopLab.Application.Features.ProfileResults.Common.DomainFailureTranslator;

namespace TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted;

/// <summary>
/// Print uses a thin loader that delegates solely to
/// <see cref="PatientAccountCalculator.Balance"/> via BalanceProbe and reuses the
/// exact M-04 per-user balance block: a non-absolute user whose account blocks
/// printing while a balance remains cannot print.
/// </summary>
public sealed class MarkProfilePrintedCommandHandler : IRequestHandler<MarkProfilePrintedCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public MarkProfilePrintedCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(MarkProfilePrintedCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (pt.EnteredAtUtc is null || !pt.IsReviewed)
        {
            return Result.Failure(Error.Conflict("لا يمكن طباعة نتيجة غير معتمدة."));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);
        if (user is not null
            && user.BlockPrintOnRemainingBalance
            && !_currentUser.IsAbsolutePermission
            && BalanceProbe.Balance(_db, patient.Id.Value) > 0)
        {
            return Result.Failure(Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة."));
        }

        var items = _db.Set<ProfileResultItem>()
            .Where(i => i.PatientTestId.Value == pt.Id.Value && !i.IsPrinted)
            .ToList();

        try
        {
            foreach (var item in items)
            {
                item.MarkPrinted(_currentUser.UserId, _clock.UtcNow);
            }

            pt.MarkPrinted(_currentUser.UserId, _clock.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}