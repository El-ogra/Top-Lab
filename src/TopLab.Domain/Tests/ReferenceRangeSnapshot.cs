using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Tests;

/// <summary>
/// Pure-Domain value object carrying the reference-range freeze-contract shape
/// (BR-05 / FR-M12-006). It is NOT an EF entity and is never registered in any
/// DbSet or IEntityTypeConfiguration. M-12 owns the shape; M-04 owns persistence.
/// </summary>
public sealed record ReferenceRangeSnapshot(
    int TestId,
    Sex? Sex,
    AgeUnit AgeUnit,
    int AgeMin,
    int AgeMax,
    decimal MinValue,
    decimal MaxValue,
    string? LowComment,
    string? HighComment,
    DateTimeOffset CapturedAtUtc);