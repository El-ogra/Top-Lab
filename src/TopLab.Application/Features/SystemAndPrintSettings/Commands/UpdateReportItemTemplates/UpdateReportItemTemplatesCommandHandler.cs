using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;

public sealed class UpdateReportItemTemplatesCommandHandler
    : IRequestHandler<UpdateReportItemTemplatesCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateReportItemTemplatesCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateReportItemTemplatesCommand request, CancellationToken cancellationToken)
    {
        var existing = _db.Set<ReportItemTemplate>()
            .ToList()
            .ToDictionary(r => r.ItemKey, StringComparer.Ordinal);

        foreach (var item in request.Items)
        {
            if (existing.TryGetValue(item.ItemKey, out var row))
            {
                row.Update(
                    item.Label,
                    item.ReferenceValueExpression,
                    item.ShowHide,
                    item.ListMembership,
                    item.FreeTextOverride);
            }
            else
            {
                _db.Add(ReportItemTemplate.Create(
                    item.ItemKey,
                    item.Label,
                    item.ReferenceValueExpression,
                    item.ShowHide,
                    item.ListMembership,
                    item.FreeTextOverride));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
