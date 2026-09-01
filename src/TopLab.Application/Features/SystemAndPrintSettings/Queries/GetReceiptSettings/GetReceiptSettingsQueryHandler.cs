using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReceiptSettings;

public sealed class GetReceiptSettingsQueryHandler : IRequestHandler<GetReceiptSettingsQuery, Result<ReceiptSettingsDto>>
{
    private readonly IApplicationDbContext _db;

    public GetReceiptSettingsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ReceiptSettingsDto>> Handle(GetReceiptSettingsQuery request, CancellationToken cancellationToken)
    {
        var row = _db.Set<ReceiptSettings>().SingleOrDefault(s => s.Id == 1);

        if (row is null)
        {
            return Task.FromResult(Result<ReceiptSettingsDto>.Failure(Error.Unexpected("سجل إعدادات الإيصال مفقود.")));
        }

        var dto = new ReceiptSettingsDto(
            row.TopMarginCm,
            row.Currency,
            row.PickupTimeDefault,
            row.PrintOnce,
            row.TestDetailDisplayMode,
            row.CashierPrinterEnabled,
            row.HeaderFooterMode);

        return Task.FromResult(Result<ReceiptSettingsDto>.Success(dto));
    }
}