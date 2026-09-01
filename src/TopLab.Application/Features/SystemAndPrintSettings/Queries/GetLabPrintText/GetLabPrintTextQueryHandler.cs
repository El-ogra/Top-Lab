using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;

public sealed class GetLabPrintTextQueryHandler : IRequestHandler<GetLabPrintTextQuery, Result<LabPrintTextDto>>
{
    private readonly ILabPrintTextStore _store;

    public GetLabPrintTextQueryHandler(ILabPrintTextStore store)
    {
        _store = store;
    }

    public Task<Result<LabPrintTextDto>> Handle(GetLabPrintTextQuery request, CancellationToken cancellationToken)
    {
        return _store.GetAsync(request.Scope, cancellationToken);
    }
}