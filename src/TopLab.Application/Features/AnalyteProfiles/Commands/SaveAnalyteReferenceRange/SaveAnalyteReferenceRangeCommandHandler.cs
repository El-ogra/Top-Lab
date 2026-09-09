using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.SaveAnalyteReferenceRange;

public sealed class SaveAnalyteReferenceRangeCommandHandler : IRequestHandler<SaveAnalyteReferenceRangeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SaveAnalyteReferenceRangeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SaveAnalyteReferenceRangeCommand request, CancellationToken cancellationToken)
    {
        var analyte = _db.Set<Analyte>().FirstOrDefault(a => a.Id.Value == request.AnalyteId);
        if (analyte is null)
        {
            return Result.Failure(Error.NotFound("المادة التحليلية غير موجودة"));
        }

        var range = _db.Set<AnalyteReferenceRange>().FirstOrDefault(r => r.AnalyteId.Equals(analyte.Id));
        if (range is null)
        {
            range = AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(0), analyte.Id);
            analyte.AttachCurrentRange(range);
            _db.Add(range);
        }

        var currentBands = _db.Set<AnalyteReferenceRangeBand>()
            .Where(b => b.AnalyteReferenceRangeId.Equals(range.Id))
            .ToList();

        foreach (var band in currentBands)
        {
            _db.Remove(band);
        }

        try
        {
            foreach (var input in request.Bands ?? Array.Empty<AnalyteBandInput>())
            {
                var band = AnalyteReferenceRangeBand.Create(
                    AnalyteReferenceRangeBandId.Create(0),
                    range.Id,
                    input.AgeUnit,
                    input.AgeMin,
                    input.AgeMax,
                    input.MinValue,
                    input.MaxValue,
                    input.Sex,
                    input.LowComment,
                    input.HighComment);
                range.AddBand(band);
                _db.Add(band);
            }
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}