using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetDatabaseServerSettings;

public sealed record GetDatabaseServerSettingsQuery : IRequest<Result<DatabaseServerSettingsDto>>;