using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText;

public sealed record SaveLabPrintTextCommand(
    LabPrintTextScope Scope,
    string LabName,
    string Address,
    string Phone,
    string FontFamily,
    int FontSizePt)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}