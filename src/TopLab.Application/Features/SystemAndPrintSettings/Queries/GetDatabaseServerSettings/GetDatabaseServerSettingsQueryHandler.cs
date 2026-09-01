using System.Data.Common;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetDatabaseServerSettings;

public sealed class GetDatabaseServerSettingsQueryHandler : IRequestHandler<GetDatabaseServerSettingsQuery, Result<DatabaseServerSettingsDto>>
{
    private readonly IWorkstationConnectionSettingsProvider _provider;

    public GetDatabaseServerSettingsQueryHandler(IWorkstationConnectionSettingsProvider provider)
    {
        _provider = provider;
    }

    public Task<Result<DatabaseServerSettingsDto>> Handle(GetDatabaseServerSettingsQuery request, CancellationToken cancellationToken)
    {
        var connectionString = _provider.GetEffectiveConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Task.FromResult(Result<DatabaseServerSettingsDto>.Failure(Error.Unexpected("لا توجد إعدادات اتصال محفوظة.")));
        }

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var server = GetValue(builder, "Server");
        var database = GetValue(builder, "Database");
        var login = GetValue(builder, "User Id");
        var integratedSecurity = builder.TryGetValue("Trusted_Connection", out var tc)
            && bool.TryParse(tc?.ToString(), out var ts)
            && ts;

        var dto = new DatabaseServerSettingsDto(server, database, integratedSecurity, login);
        return Task.FromResult(Result<DatabaseServerSettingsDto>.Success(dto));
    }

    private static string GetValue(DbConnectionStringBuilder builder, string key)
    {
        return builder.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
    }
}