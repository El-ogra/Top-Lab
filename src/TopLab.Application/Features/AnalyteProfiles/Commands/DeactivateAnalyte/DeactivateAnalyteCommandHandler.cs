using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.DeactivateAnalyte;

public sealed class DeactivateAnalyteCommandHandler : IRequestHandler<DeactivateAnalyteCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeactivateAnalyteCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeactivateAnalyteCommand request, CancellationToken cancellationToken)
    {
        var analyte = _db.Set<Analyte>().FirstOrDefault(a => a.Id.Value == request.AnalyteId);
        if (analyte is null)
        {
            return Result.Failure(Error.NotFound("المادة التحليلية غير موجودة"));
        }

        analyte.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}