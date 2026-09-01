using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateEnvelopeSettings;

public sealed class UpdateEnvelopeSettingsCommandHandler : IRequestHandler<UpdateEnvelopeSettingsCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateEnvelopeSettingsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateEnvelopeSettingsCommand request, CancellationToken cancellationToken)
    {
        var row = _db.Set<EnvelopeSettings>().SingleOrDefault(s => s.Id == 1);
        if (row is null)
        {
            return Result.Failure(Error.Unexpected("سجل إعدادات المظروف مفقود."));
        }

        row.Update(request.TopMarginCm, request.HeaderFooterMode, request.SuppressCaptions);

        var positions = _db.Set<EnvelopePrintItemPosition>().ToList();
        var updatedNames = new HashSet<string>(request.Positions.Select(p => p.ItemName));

        foreach (var position in positions)
        {
            if (!updatedNames.Contains(position.ItemName))
            {
                continue;
            }

            var dto = request.Positions.First(p => p.ItemName == position.ItemName);
            position.Update(dto.IsEnabled, dto.LeftOffsetCm, dto.TopOffsetCm);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}