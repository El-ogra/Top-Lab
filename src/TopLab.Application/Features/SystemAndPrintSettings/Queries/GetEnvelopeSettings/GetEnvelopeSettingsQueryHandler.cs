using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetEnvelopeSettings;

public sealed class GetEnvelopeSettingsQueryHandler : IRequestHandler<GetEnvelopeSettingsQuery, Result<EnvelopeSettingsDto>>
{
    private static readonly string[] CanonicalOrder = ["Name", "Code", "ReferralEntity", "Date"];

    private readonly IApplicationDbContext _db;

    public GetEnvelopeSettingsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<EnvelopeSettingsDto>> Handle(GetEnvelopeSettingsQuery request, CancellationToken cancellationToken)
    {
        var row = _db.Set<EnvelopeSettings>().SingleOrDefault(s => s.Id == 1);

        if (row is null)
        {
            return Task.FromResult(Result<EnvelopeSettingsDto>.Failure(Error.Unexpected("سجل إعدادات المظروف مفقود.")));
        }

        var positions = _db.Set<EnvelopePrintItemPosition>()
            .Where(p => CanonicalOrder.Contains(p.ItemName))
            .OrderBy(p => Array.IndexOf(CanonicalOrder, p.ItemName))
            .Select(p => new EnvelopePrintItemPositionDto(
                p.ItemName,
                p.IsEnabled,
                p.LeftOffsetCm,
                p.TopOffsetCm))
            .ToList();

        var dto = new EnvelopeSettingsDto(
            row.TopMarginCm,
            row.HeaderFooterMode,
            row.SuppressCaptions,
            positions);

        return Task.FromResult(Result<EnvelopeSettingsDto>.Success(dto));
    }
}