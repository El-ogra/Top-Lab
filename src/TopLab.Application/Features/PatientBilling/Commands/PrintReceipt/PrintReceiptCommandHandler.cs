using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;

namespace TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;

public sealed class PrintReceiptCommandHandler : IRequestHandler<PrintReceiptCommand, Result>
{
    private readonly ISender _sender;
    private readonly IReceiptPrintingService _printingService;

    public PrintReceiptCommandHandler(ISender sender, IReceiptPrintingService printingService)
    {
        _sender = sender;
        _printingService = printingService;
    }

    public async Task<Result> Handle(PrintReceiptCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _sender.Send(new GetPatientReceiptQuery(request.PatientId), cancellationToken);
        if (!receipt.IsSuccess)
        {
            return Result.Failure(receipt.Error!);
        }

        var token = ReceiptPrintEnvelope.CreateToken(receipt.Value!);
        return await _printingService.PrintReceiptAsync(token, cancellationToken);
    }
}
