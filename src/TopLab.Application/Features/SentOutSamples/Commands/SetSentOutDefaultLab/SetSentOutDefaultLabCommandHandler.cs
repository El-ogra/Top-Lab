using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SentOutSamples.Commands.SetSentOutDefaultLab;

public sealed class SetSentOutDefaultLabCommandHandler : IRequestHandler<SetSentOutDefaultLabCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetSentOutDefaultLabCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SetSentOutDefaultLabCommand request, CancellationToken cancellationToken)
    {
        var row = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        if (row is null)
        {
            return Result.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود."));
        }

        if (request.ExternalLabEntityId.HasValue)
        {
            var entity = _db.Set<ExternalEntity>().SingleOrDefault(e => e.Id.Value == request.ExternalLabEntityId.Value);
            if (entity is null || entity.EntityType != EntityType.PartnerLab)
            {
                return Result.Failure(Error.NotFound("الجهة الخارجية غير موجودة."));
            }
        }

        row.SetDefaultSentOutLab(request.ExternalLabEntityId);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}