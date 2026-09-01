using System.IO;
using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Presentation.Services.Configuration;

/// <summary>
/// Workstation-local plain-text lab identification and font choices stored at
/// <c>%ProgramData%\TopLab\lab-print-text.json</c> (ADR-0027). Writes are atomic:
/// the file is written to a temp path then moved over the destination.
/// </summary>
public sealed class JsonLabPrintTextStore : ILabPrintTextStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public string GetStorePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "TopLab", "lab-print-text.json");

    public async Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default)
    {
        try
        {
            var path = GetStorePath();
            if (!File.Exists(path))
            {
                return Result<LabPrintTextDto>.Success(DefaultsFor(scope));
            }

            var json = await File.ReadAllTextAsync(path, cancellationToken);
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(scope.ToString(), out var node)
                && node.TryGetProperty("LabName", out _))
            {
                var dto = Deserialize(node);
                return Result<LabPrintTextDto>.Success(dto);
            }

            return Result<LabPrintTextDto>.Success(DefaultsFor(scope));
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return Result<LabPrintTextDto>.Failure(Error.Unexpected($"تعذر قراءة نصوص الطباعة المحلية. {ex.Message}"));
        }
    }

    public async Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(GetStorePath())!;
            Directory.CreateDirectory(directory);

            var existing = ReadExistingAsDictionary();

            existing[scope.ToString()] = ToPoco(content);

            var tempPath = GetStorePath() + ".tmp";
            var finalPath = GetStorePath();
            var json = JsonSerializer.Serialize(existing, SerializerOptions);
            await File.WriteAllTextAsync(tempPath, json, cancellationToken);
            File.Move(tempPath, finalPath, overwrite: true);

            return Result.Success();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return Result.Failure(Error.Unexpected($"تعذر حفظ نصوص الطباعة المحلية. {ex.Message}"));
        }
    }

    private Dictionary<string, object> ReadExistingAsDictionary()
    {
        var path = GetStorePath();
        if (!File.Exists(path))
        {
            return new Dictionary<string, object>();
        }

        var json = File.ReadAllText(path);
        using var document = JsonDocument.Parse(json);
        var map = new Dictionary<string, object>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            map[property.Name] = DeserializeFromElement(property.Value);
        }

        return map;
    }

    private static LabPrintTextDto DefaultsFor(LabPrintTextScope scope) => new(string.Empty, string.Empty, string.Empty, string.Empty, 0);

    private static LabPrintTextDto Deserialize(JsonElement node)
    {
        return new LabPrintTextDto(
            GetString(node, "LabName"),
            GetString(node, "Address"),
            GetString(node, "Phone"),
            GetString(node, "FontFamily") ?? string.Empty,
            node.TryGetProperty("FontSizePt", out var fs) && fs.ValueKind == JsonValueKind.Number ? fs.GetInt32() : 0);
    }

    private static string GetString(JsonElement node, string key)
    {
        return node.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? string.Empty : string.Empty;
    }

    private static object DeserializeFromElement(JsonElement element)
    {
        var dto = Deserialize(element);
        return ToPoco(dto);
    }

    private static object ToPoco(LabPrintTextDto content)
    {
        return new
        {
            content.LabName,
            content.Address,
            content.Phone,
            content.FontFamily,
            content.FontSizePt
        };
    }
}