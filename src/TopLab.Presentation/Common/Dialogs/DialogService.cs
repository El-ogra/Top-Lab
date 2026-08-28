using System.Windows;

namespace TopLab.Presentation.Common.Dialogs;

public sealed class DialogService : IDialogService
{
    public Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task ShowErrorAsync(string message)
    {
        MessageBox.Show(message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public Task<bool> ShowSecondaryPasswordDialogAsync()
    {
        // Stub until M17 — secure by default (return false so sensitive windows stay closed)
        return Task.FromResult(false);
    }
}
