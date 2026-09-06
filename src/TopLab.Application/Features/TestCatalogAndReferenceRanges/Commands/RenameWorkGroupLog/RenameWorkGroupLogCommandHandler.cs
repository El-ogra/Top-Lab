using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.RenameWorkGroupLog;

public sealed class RenameWorkGroupLogCommandHandler : IRequestHandler<RenameWorkGroupLogCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RenameWorkGroupLogCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RenameWorkGroupLogCommand request, CancellationToken cancellationToken)
    {
        var log = _db.Set<WorkGroupLog>().FirstOrDefault(l => l.Id.Value == request.Id);
        if (log is null)
        {
            return Result.Failure(Error.NotFound("مجموعة العمل غير موجودة"));
        }

        log.Rename(request.Name);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}