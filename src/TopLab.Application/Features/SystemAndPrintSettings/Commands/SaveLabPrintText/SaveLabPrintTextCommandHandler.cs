using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText;

public sealed class SaveLabPrintTextCommandHandler : IRequestHandler<SaveLabPrintTextCommand, Result>
{
    private readonly ILabPrintTextStore _store;

    public SaveLabPrintTextCommandHandler(ILabPrintTextStore store)
    {
        _store = store;
    }

    public Task<Result> Handle(SaveLabPrintTextCommand request, CancellationToken cancellationToken)
    {
        var dto = new LabPrintTextDto(
            request.LabName,
            request.Address,
            request.Phone,
            request.FontFamily,
            request.FontSizePt);
        return _store.SaveAsync(request.Scope, dto, cancellationToken);
    }
}