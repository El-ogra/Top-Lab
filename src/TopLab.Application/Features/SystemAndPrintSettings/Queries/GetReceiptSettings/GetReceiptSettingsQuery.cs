using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReceiptSettings;

public sealed record GetReceiptSettingsQuery : IRequest<Result<ReceiptSettingsDto>>;