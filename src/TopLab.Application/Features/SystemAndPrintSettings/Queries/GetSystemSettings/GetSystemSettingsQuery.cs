using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;

public sealed record GetSystemSettingsQuery : IRequest<Result<SystemSettingsDto>>;