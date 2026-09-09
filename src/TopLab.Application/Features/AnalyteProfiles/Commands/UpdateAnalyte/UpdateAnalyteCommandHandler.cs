using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.UpdateAnalyte;

public sealed class UpdateAnalyteCommandHandler : IRequestHandler<UpdateAnalyteCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateAnalyteCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateAnalyteCommand request, CancellationToken cancellationToken)
    {
        var analyte = _db.Set<Analyte>().FirstOrDefault(a => a.Id.Value == request.AnalyteId);
        if (analyte is null)
        {
            return Result.Failure(Error.NotFound("المادة التحليلية غير موجودة"));
        }

        var name = request.Name?.Trim();
        if (_db.Set<Analyte>().Any(a => a.Name == name && a.Id.Value != request.AnalyteId))
        {
            return Result.Failure(Error.Conflict("اسم المادة التحليلية مستخدم بالفعل"));
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(request.ReportName))
        {
            return Result.Failure(Error.Validation("الاسم واسم التقرير مطلوبان."));
        }

        analyte.Update(name, request.ReportName);

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}