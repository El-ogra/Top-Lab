namespace TopLab.Domain.Results;

/// <summary>
/// WP-03: WPF-friendly sensitivity choice (value + display label).
/// <c>Value == null</c> means Unspecified and must not be persisted.
/// Enum values 0–3 are unchanged (SD-9).
/// </summary>
public sealed record SensitivityOption(int? Value, string Label);
