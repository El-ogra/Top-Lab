using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.SentOutSamples;

namespace TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;

public sealed class DeleteExternalEntityCommandHandler : IRequestHandler<DeleteExternalEntityCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteExternalEntityCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteExternalEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Value == request.Id);
        if (entity is null)
        {
            return Result.Failure(Error.NotFound("الجهة الخارجية غير موجودة."));
        }

        if (_db.Set<Patient>().Any(p =>
            (p.TreatingDoctorId != null && p.TreatingDoctorId.Value == request.Id)
            || (p.ReferralEntityId != null && p.ReferralEntityId.Value == request.Id)))
        {
            return Result.Failure(Error.Conflict("تعذر حذف الجهة لوجود مرضى مرتبطين بها."));
        }

        if (_db.Set<SentOutSample>().Any(s => s.ExternalLabEntityId.Value == request.Id))
        {
            return Result.Failure(Error.Conflict("تعذر حذف الجهة لوجود عينات مرسلة مرتبطة بها."));
        }

        _db.Remove(entity);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsReferenceConflict(ex))
        {
            return Result.Failure(Error.Conflict("تعذر حذف الجهة لوجود عينات مرسلة مرتبطة بها."));
        }

        return Result.Success();
    }

    private static bool IsReferenceConflict(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("REFERENCE") || msg.Contains("conflicted");
    }
}
