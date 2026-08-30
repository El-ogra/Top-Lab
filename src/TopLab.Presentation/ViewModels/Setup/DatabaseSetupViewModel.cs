using Microsoft.Data.SqlClient;
using TopLab.Presentation.Common;
using TopLab.Presentation.Services.Configuration;

namespace TopLab.Presentation.ViewModels.Setup;

public sealed class DatabaseSetupViewModel : ViewModelBase
{
    private readonly ConfigurationFileService _config;

    private string _server = "(localdb)\\mssqllocaldb";
    private string _database = "TopLab";
    private bool _integratedSecurity = true;
    private string? _username;
    private string? _password;
    private string _statusText = string.Empty;

    public DatabaseSetupViewModel(ConfigurationFileService config)
    {
        _config = config;
    }

    public string Server
    {
        get => _server;
        set => SetProperty(ref _server, value);
    }

    public string Database
    {
        get => _database;
        set => SetProperty(ref _database, value);
    }

    public bool IntegratedSecurity
    {
        get => _integratedSecurity;
        set => SetProperty(ref _integratedSecurity, value);
    }

    public string? Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public async Task TestConnectionAsync()
    {
        StatusText = "جارٍ اختبار الاتصال...";
        var cs = _config.BuildConnectionString(Server, Database, IntegratedSecurity, Username, Password);
        try
        {
            using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            StatusText = "تم الاتصال بنجاح.";
        }
        catch (Exception ex)
        {
            StatusText = "فشل الاتصال: " + ex.Message;
        }
    }

    public async Task<bool> SaveAsync()
    {
        StatusText = "جارٍ اختبار الاتصال قبل الحفظ...";
        var cs = _config.BuildConnectionString(Server, Database, IntegratedSecurity, Username, Password);
        try
        {
            using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
        }
        catch (Exception ex)
        {
            StatusText = "فشل الاتصال، لم يتم الحفظ: " + ex.Message;
            return false;
        }

        _config.SaveConnectionString(Server, Database, IntegratedSecurity, Username, Password);
        StatusText = "تم حفظ الإعدادات بنجاح.";
        return true;
    }
}