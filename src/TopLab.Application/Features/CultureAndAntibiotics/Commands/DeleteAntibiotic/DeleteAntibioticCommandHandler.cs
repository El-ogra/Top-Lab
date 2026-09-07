using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;

public sealed class DeleteAntibioticCommandHandler
    : IRequestHandler<DeleteAntibioticCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteAntibioticCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(
        DeleteAntibioticCommand request, CancellationToken cancellationToken)
    {
        var antibiotic = _db.Set<Antibiotic>().FirstOrDefault(a => a.Id.Value == request.Id);
        if (antibiotic is null)
        {
            return Result.Failure(Error.NotFound("المضاد الحيوي غير موجود."));
        }

        if (_db.Set<CultureAntibioticAttachment>().Any(a => a.AntibioticId.Value == request.Id))
        {
            return Result.Failure(Error.Conflict("تعذر حذف المضاد الحيوي لارتباطه بمزرعة."));
        }

        if (_db.Set<CultureAntibioticResult>().Any(r => r.AntibioticId.Value == request.Id))
        {
            return Result.Failure(Error.Conflict("تعذر حذف المضاد الحيوي لوجود نتائج مسجلة به."));
        }

        _db.Remove(antibiotic);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsReferenceConflict(ex))
        {
            return Result.Failure(Error.Conflict("تعذر حذف المضاد الحيوي لوجود نتائج مسجلة به."));
        }

        return Result.Success();
    }

    private static bool IsReferenceConflict(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("REFERENCE") || msg.Contains("conflicted");
    }
}