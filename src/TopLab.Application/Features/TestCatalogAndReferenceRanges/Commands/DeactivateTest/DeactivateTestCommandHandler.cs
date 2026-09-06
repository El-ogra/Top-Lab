using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTest;

public sealed class DeactivateTestCommandHandler : IRequestHandler<DeactivateTestCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeactivateTestCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeactivateTestCommand request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.Id);
        if (test is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (!test.IsActive)
        {
            return Result.Failure(Error.Conflict("التحليل غير نشط بالفعل"));
        }

        test.Deactivate();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}