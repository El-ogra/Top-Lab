using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateWorkGroupLog;

public sealed class CreateWorkGroupLogCommandHandler : IRequestHandler<CreateWorkGroupLogCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateWorkGroupLogCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateWorkGroupLogCommand request, CancellationToken cancellationToken)
    {
        if (_db.Set<WorkGroupLog>().Any(l => l.Name == request.Name))
        {
            return Result<int>.Failure(Error.Conflict("مجموعة العمل موجودة بالفعل"));
        }

        var log = WorkGroupLog.Create(WorkGroupLogId.Create(0), request.Name);
        _db.Add(log);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(log.Id.Value);
    }
}