using System.Runtime.InteropServices;
using System.Windows;

namespace TopLab.Presentation.Common.Clipboard;

/// <summary>
/// WPF implementation of <see cref="IClipboardService"/>.
/// <para>
/// The clipboard is a shared OS resource: another process can hold it open, in which case
/// <c>Clipboard.SetText</c> throws <see cref="COMException"/>. A copy action must never take
/// the screen down for that, so the call is retried a few times and then reports failure by
/// returning false rather than throwing. The ViewModel decides what to tell the operator.
/// </para>
/// </summary>
public sealed class WpfClipboardService : IClipboardService
{
    private const int RetryCount = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(40);

    public async Task SetTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        for (var attempt = 0; attempt < RetryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                System.Windows.Clipboard.SetText(text);
                return;
            }
            catch (COMException) when (attempt < RetryCount - 1)
            {
                await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
            }
            catch (COMException)
            {
                // The clipboard stayed locked: the caller surfaces the failure in its status line.
                return;
            }
        }
    }
}
