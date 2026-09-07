using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientRegistration.Commands.RemoveTestFromVisit;

public sealed class RemoveTestFromVisitCommandHandler : IRequestHandler<RemoveTestFromVisitCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public RemoveTestFromVisitCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(RemoveTestFromVisitCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result<bool>.Failure(Error.NotFound("التحليل غير موجود."));
        }

        if (pt.EnteredAtUtc.HasValue || pt.ResultValue is not null || pt.ResultFlag.HasValue)
        {
            return Result<bool>.Failure(Error.Conflict("لا يمكن حذف تحليل تم تسجيل نتيجته."));
        }

        _db.Remove(pt);
        await _db.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}