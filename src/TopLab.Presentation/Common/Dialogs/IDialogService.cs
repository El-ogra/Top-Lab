namespace TopLab.Presentation.Common.Dialogs;

public interface IDialogService
{
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task ShowErrorAsync(string message);
    Task<bool> ShowSecondaryPasswordDialogAsync();
    Task<string?> PickBackupFolderAsync(string initialDirectory);
    Task<string?> PickBackupFileAsync();
}
