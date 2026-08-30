using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace TopLab.Presentation.Services.Configuration;

public sealed class ConfigurationFileService
{
    public string GetProgramDataPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "TopLab", "appsettings.json");

    public bool TryLoadConnectionString([NotNullWhen(true)] out string? cs)
    {
        cs = null;
        var p = GetProgramDataPath();
        if (!File.Exists(p)) return false;
        try
        {
            var j = JsonDocument.Parse(File.ReadAllText(p));
            if (j.RootElement.TryGetProperty("ConnectionStrings", out var c) &&
                c.TryGetProperty("TopLab", out var v))
            {
                cs = v.GetString();
                return !string.IsNullOrWhiteSpace(cs);
            }
        }
        catch { }
        return false;
    }

    public string BuildConnectionString(string s, string db, bool integ, string? u, string? p)
        => integ
            ? $"Server={s};Database={db};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
            : $"Server={s};Database={db};User Id={u};Password={p};MultipleActiveResultSets=true;TrustServerCertificate=True";

    public void SaveConnectionString(string s, string db, bool integ, string? u, string? p)
    {
        var path = GetProgramDataPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var cs = BuildConnectionString(s, db, integ, u, p);
        var json = JsonSerializer.Serialize(
            new { ConnectionStrings = new { TopLab = cs } },
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}