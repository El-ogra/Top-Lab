using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Infrastructure.Services;

/// <summary>
/// Workstation-local JSON store for the Requirements &amp; Purchases list
/// (purchases-list.json under %ProgramData%\TopLab — ADR-0021/ADR-0027 locality).
/// Atomic write: temp file then move. Missing file → empty list.
/// </summary>
public sealed class JsonPurchasesListStore : IPurchasesListStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public JsonPurchasesListStore(string? directoryOverride = null)
    {
        var directory = directoryOverride
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "TopLab");
        _filePath = Path.Combine(directory, "purchases-list.json");
    }

    public async Task<Result<IReadOnlyList<PurchaseItemDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return Result<IReadOnlyList<PurchaseItemDto>>.Success(Array.Empty<PurchaseItemDto>());
            }

            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Result<IReadOnlyList<PurchaseItemDto>>.Success(Array.Empty<PurchaseItemDto>());
            }

            var items = JsonSerializer.Deserialize<List<PurchaseItemDto>>(json, SerializerOptions)
                ?? new List<PurchaseItemDto>();
            return Result<IReadOnlyList<PurchaseItemDto>>.Success(items);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return Result<IReadOnlyList<PurchaseItemDto>>.Failure(
                Error.Unexpected($"تعذر قراءة قائمة المشتريات المحلية. {ex.Message}"));
        }
    }

    public async Task<Result> SaveAllAsync(IReadOnlyList<PurchaseItemDto> items, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath)!;
            Directory.CreateDirectory(directory);

            var tempPath = _filePath + ".tmp";
            var json = JsonSerializer.Serialize(items, SerializerOptions);
            await File.WriteAllTextAsync(tempPath, json, cancellationToken);
            File.Move(tempPath, _filePath, overwrite: true);

            return Result.Success();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return Result.Failure(Error.Unexpected($"تعذر حفظ قائمة المشتريات المحلية. {ex.Message}"));
        }
    }
}
