using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;

namespace TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;

public sealed class PrintWorkSheetCommandHandler : IRequestHandler<PrintWorkSheetCommand, Result>
{
    private readonly ISender _sender;
    private readonly IWorkSheetPrintingService _printingService;

    public PrintWorkSheetCommandHandler(ISender sender, IWorkSheetPrintingService printingService)
    {
        _sender = sender;
        _printingService = printingService;
    }

    public async Task<Result> Handle(PrintWorkSheetCommand request, CancellationToken cancellationToken)
    {
        var sheet = await _sender.Send(new GetVisitWorkSheetQuery(request.PatientId), cancellationToken);
        if (!sheet.IsSuccess)
        {
            return Result.Failure(sheet.Error!);
        }

        var token = WorkSheetPrintEnvelope.CreateToken(sheet.Value!);
        return await _printingService.PrintWorkSheetAsync(token, cancellationToken);
    }
}
