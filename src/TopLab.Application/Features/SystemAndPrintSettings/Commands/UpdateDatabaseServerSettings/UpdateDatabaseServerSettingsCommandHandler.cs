using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateDatabaseServerSettings;

public sealed class UpdateDatabaseServerSettingsCommandHandler : IRequestHandler<UpdateDatabaseServerSettingsCommand, Result>
{
    private readonly IWorkstationConnectionSettingsProvider _provider;

    public UpdateDatabaseServerSettingsCommandHandler(IWorkstationConnectionSettingsProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result> Handle(UpdateDatabaseServerSettingsCommand request, CancellationToken cancellationToken)
    {
        var candidate = BuildConnectionString(
            request.Server,
            request.Database,
            request.IntegratedSecurity,
            request.Login,
            request.Password);

        var canConnect = await _provider.TestConnectionStringAsync(candidate, cancellationToken);
        if (!canConnect)
        {
            return Result.Failure(Error.Unexpected("تعذر الاتصال بخادم قواعد البيانات. لم يتم حفظ الإعدادات."));
        }

        await _provider.SaveConnectionStringAsync(
            request.Server,
            request.Database,
            request.IntegratedSecurity,
            request.Login,
            request.Password,
            cancellationToken);

        return Result.Success();
    }

    internal static string BuildConnectionString(string s, string db, bool integ, string? u, string? p)
        => integ
            ? $"Server={s};Database={db};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
            : $"Server={s};Database={db};User Id={u};Password={p};MultipleActiveResultSets=true;TrustServerCertificate=True";
}