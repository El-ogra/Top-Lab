using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Infrastructure.Services;

/// <summary>
/// Workstation-local JSON store for the Tools Phone Book
/// (phone-book.json under %ProgramData%\TopLab — ADR-0021/ADR-0027 locality).
/// Atomic write: temp file then move. Missing file → empty list.
/// Never references Patient data (FR-M23-002 / SD-23-11).
/// </summary>
public sealed class JsonPhoneBookStore : IPhoneBookStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public JsonPhoneBookStore(string? directoryOverride = null)
    {
        var directory = directoryOverride
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "TopLab");
        _filePath = Path.Combine(directory, "phone-book.json");
    }

    public async Task<Result<IReadOnlyList<PhoneBookEntryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return Result<IReadOnlyList<PhoneBookEntryDto>>.Success(Array.Empty<PhoneBookEntryDto>());
            }

            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Result<IReadOnlyList<PhoneBookEntryDto>>.Success(Array.Empty<PhoneBookEntryDto>());
            }

            var entries = JsonSerializer.Deserialize<List<PhoneBookEntryDto>>(json, SerializerOptions)
                ?? new List<PhoneBookEntryDto>();
            return Result<IReadOnlyList<PhoneBookEntryDto>>.Success(entries);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return Result<IReadOnlyList<PhoneBookEntryDto>>.Failure(
                Error.Unexpected($"تعذر قراءة دفتر الهواتف المحلي. {ex.Message}"));
        }
    }

    public async Task<Result> SaveAllAsync(IReadOnlyList<PhoneBookEntryDto> entries, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath)!;
            Directory.CreateDirectory(directory);

            var tempPath = _filePath + ".tmp";
            var json = JsonSerializer.Serialize(entries, SerializerOptions);
            await File.WriteAllTextAsync(tempPath, json, cancellationToken);
            File.Move(tempPath, _filePath, overwrite: true);

            return Result.Success();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return Result.Failure(Error.Unexpected($"تعذر حفظ دفتر الهواتف المحلي. {ex.Message}"));
        }
    }
}
