using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReceiptSettings;

public sealed class UpdateReceiptSettingsCommandHandler : IRequestHandler<UpdateReceiptSettingsCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateReceiptSettingsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateReceiptSettingsCommand request, CancellationToken cancellationToken)
    {
        var row = _db.Set<ReceiptSettings>().SingleOrDefault(s => s.Id == 1);
        if (row is null)
        {
            return Result.Failure(Error.Unexpected("سجل إعدادات الإيصال مفقود."));
        }

        row.Update(
            request.TopMarginCm,
            request.Currency,
            request.PickupTimeDefault,
            request.PrintOnce,
            request.TestDetailDisplayMode,
            request.CashierPrinterEnabled,
            request.HeaderFooterMode);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}