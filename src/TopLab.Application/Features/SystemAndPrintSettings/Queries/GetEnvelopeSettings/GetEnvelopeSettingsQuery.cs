using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetEnvelopeSettings;

public sealed record GetEnvelopeSettingsQuery : IRequest<Result<EnvelopeSettingsDto>>;