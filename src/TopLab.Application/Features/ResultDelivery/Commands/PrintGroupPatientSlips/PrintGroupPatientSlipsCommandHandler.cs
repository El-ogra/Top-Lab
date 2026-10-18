using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetGroupPatientSlips;

namespace TopLab.Application.Features.ResultDelivery.Commands.PrintGroupPatientSlips;

/// <summary>
/// Prints the group-patient report slip batch (Phase 6, GAP-013): resolves the
/// slips through <see cref="GetGroupPatientSlipsQuery"/> (the one selection and
/// data path), wraps the batch in the print envelope token, and hands it to
/// <see cref="IGroupPatientSlipPrintingService"/>.
/// Follows the <c>PrintLabOrderCommandHandler</c> query-then-print shape.
/// </summary>
public sealed class PrintGroupPatientSlipsCommandHandler
    : IRequestHandler<PrintGroupPatientSlipsCommand, Result>
{
    private readonly ISender _sender;
    private readonly IGroupPatientSlipPrintingService _printingService;

    public PrintGroupPatientSlipsCommandHandler(
        ISender sender,
        IGroupPatientSlipPrintingService printingService)
    {
        _sender = sender;
        _printingService = printingService;
    }

    public async Task<Result> Handle(PrintGroupPatientSlipsCommand request, CancellationToken cancellationToken)
    {
        var slips = await _sender.Send(new GetGroupPatientSlipsQuery(request.PatientIds), cancellationToken);
        if (!slips.IsSuccess || slips.Value is null)
        {
            return Result.Failure(slips.Error!);
        }

        var token = GroupPatientSlipPrintEnvelope.CreateToken(slips.Value);
        return await _printingService.PrintGroupPatientSlipsAsync(token, cancellationToken);
    }
}
