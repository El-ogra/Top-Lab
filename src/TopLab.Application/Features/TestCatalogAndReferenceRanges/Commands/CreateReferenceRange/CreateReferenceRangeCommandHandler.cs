using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateReferenceRange;

public sealed class CreateReferenceRangeCommandHandler : IRequestHandler<CreateReferenceRangeCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateReferenceRangeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateReferenceRangeCommand request, CancellationToken cancellationToken)
    {
        if (!_db.Set<Test>().Any(t => t.Id.Value == request.TestId))
        {
            return Result<int>.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var range = ReferenceRange.Create(
            ReferenceRangeId.Create(0),
            TestId.Create(request.TestId),
            request.AgeUnit,
            request.AgeMin,
            request.AgeMax,
            request.MinValue,
            request.MaxValue,
            request.Sex,
            request.LowComment,
            request.HighComment);

        _db.Add(range);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(range.Id.Value);
    }
}