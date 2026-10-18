using System.Collections.Generic;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportItemTemplates;

public sealed class GetReportItemTemplatesQueryHandler
    : IRequestHandler<GetReportItemTemplatesQuery, Result<IReadOnlyList<ReportItemTemplateDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetReportItemTemplatesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<ReportItemTemplateDto>>> Handle(
        GetReportItemTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var rows = _db.Set<ReportItemTemplate>()
            .ToList()
            .ToDictionary(r => r.ItemKey, StringComparer.Ordinal);

        // Always surface every canonical key in a stable order: a row that is
        // absent (e.g. before the seed migration is applied) falls back to the
        // catalog default so the editor can never silently lose an item.
        IReadOnlyList<ReportItemTemplateDto> items = ReportItemKeys.All
            .Select(key => rows.TryGetValue(key, out var row)
                ? ToDto(row)
                : ToDto(ReportItemTemplateCatalog.GetDefault(key)))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<ReportItemTemplateDto>>.Success(items));
    }

    private static ReportItemTemplateDto ToDto(ReportItemTemplate template) => new(
        template.ItemKey,
        template.Label,
        template.ReferenceValueExpression,
        template.ShowHide,
        template.ListMembership,
        template.FreeTextOverride,
        template.LineSpacing,
        template.LeftOffsetCm,
        template.MarkerSymbol);
}
