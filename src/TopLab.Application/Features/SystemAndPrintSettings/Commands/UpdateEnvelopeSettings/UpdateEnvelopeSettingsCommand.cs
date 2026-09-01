using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateEnvelopeSettings;

public sealed record UpdateEnvelopeSettingsCommand(
    decimal TopMarginCm,
    HeaderFooterMode HeaderFooterMode,
    bool SuppressCaptions,
    IReadOnlyList<EnvelopePrintItemPositionDto> Positions)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}