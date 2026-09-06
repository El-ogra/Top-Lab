using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateReferenceRange;

public sealed class UpdateReferenceRangeCommandHandler : IRequestHandler<UpdateReferenceRangeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateReferenceRangeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateReferenceRangeCommand request, CancellationToken cancellationToken)
    {
        var range = _db.Set<ReferenceRange>().FirstOrDefault(r => r.Id.Value == request.Id);
        if (range is null)
        {
            return Result.Failure(Error.NotFound("النطاق المرجعي غير موجود"));
        }

        range.Update(
            request.Sex,
            request.AgeUnit,
            request.AgeMin,
            request.AgeMax,
            request.MinValue,
            request.MaxValue,
            request.LowComment,
            request.HighComment);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}