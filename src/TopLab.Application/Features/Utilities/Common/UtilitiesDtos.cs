namespace TopLab.Application.Features.Utilities.Common;

public sealed record ConversionResultDto(
    decimal OriginalValue,
    string FromUnit,
    string ToUnit,
    decimal ConvertedValue);

public sealed record CalculationResultDto(
    string Expression,
    decimal Result);

public sealed record StopwatchElapsedDto(
    DateTime StartUtc,
    DateTime EndUtc,
    TimeSpan Elapsed);

public sealed record TestLibraryEntryDto(
    int TestId,
    string Name,
    string TestCode,
    string? GroupName);

public sealed record PurchaseItemDto(
    int Id,
    string Text,
    bool IsDone,
    DateTime CreatedAtUtc);

public sealed record PhoneBookEntryDto(
    int Id,
    string Name,
    string Phone,
    string? Notes);
