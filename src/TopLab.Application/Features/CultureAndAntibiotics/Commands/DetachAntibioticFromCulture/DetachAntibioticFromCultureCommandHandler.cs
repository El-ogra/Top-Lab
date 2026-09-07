using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;

public sealed class DetachAntibioticFromCultureCommandHandler
    : IRequestHandler<DetachAntibioticFromCultureCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DetachAntibioticFromCultureCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(
        DetachAntibioticFromCultureCommand request, CancellationToken cancellationToken)
    {
        var attachment = _db.Set<CultureAntibioticAttachment>().FirstOrDefault(a =>
            a.TestId.Value == request.TestId
            && a.AntibioticId.Value == request.AntibioticId);

        if (attachment is null)
        {
            return Result.Failure(Error.NotFound("المضاد الحيوي غير مضاف لهذه المزرعة."));
        }

        _db.Remove(attachment);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}