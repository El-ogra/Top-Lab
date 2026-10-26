namespace TopLab.Presentation.Common.Clipboard;

/// <summary>
/// The one place the UI talks to the operating-system clipboard.
/// <para>
/// A ViewModel must never call <c>System.Windows.Clipboard</c> directly: that type lives in
/// the WPF assembly, cannot be exercised in a unit test, and would put UI plumbing inside
/// presentation logic. This port keeps the ViewModel testable (a fake records the text) and
/// keeps the WPF call in exactly one implementation.
/// </para>
/// </summary>
public interface IClipboardService
{
    /// <summary>Copies the given text to the system clipboard. Never throws for empty text.</summary>
    Task SetTextAsync(string text, CancellationToken cancellationToken = default);
}
