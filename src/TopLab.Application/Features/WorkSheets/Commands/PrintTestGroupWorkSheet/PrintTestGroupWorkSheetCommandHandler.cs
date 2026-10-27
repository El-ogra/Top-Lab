using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;

namespace TopLab.Application.Features.WorkSheets.Commands.PrintTestGroupWorkSheet;

public sealed class PrintTestGroupWorkSheetCommandHandler
    : IRequestHandler<PrintTestGroupWorkSheetCommand, Result>
{
    private readonly ISender _sender;
    private readonly IWorkSheetPrintingService _printingService;

    public PrintTestGroupWorkSheetCommandHandler(ISender sender, IWorkSheetPrintingService printingService)
    {
        _sender = sender;
        _printingService = printingService;
    }

    public async Task<Result> Handle(
        PrintTestGroupWorkSheetCommand request, CancellationToken cancellationToken)
    {
        var sheet = await _sender.Send(
            new GetWorkSheetByTestGroupQuery(
                request.TestGroupId, null, request.From, request.To, request.UnfinishedOnly),
            cancellationToken);
        if (!sheet.IsSuccess)
        {
            return Result.Failure(sheet.Error!);
        }

        var token = WorkSheetGroupPrintEnvelope.CreateToken(sheet.Value!);
        return await _printingService.PrintGroupWorkSheetAsync(token, cancellationToken);
    }
}
