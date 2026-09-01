using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;

public sealed record GetLabPrintTextQuery : IRequest<Result<LabPrintTextDto>>
{
    public LabPrintTextScope Scope { get; init; }
}