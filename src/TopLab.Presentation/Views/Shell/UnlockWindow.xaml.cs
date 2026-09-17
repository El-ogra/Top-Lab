using System.Windows;

namespace TopLab.Presentation.Views.Shell;

public partial class UnlockWindow : Window
{
    private readonly ViewModels.Shell.UnlockViewModel _vm;

    public UnlockWindow(ViewModels.Shell.UnlockViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
        Closing += UnlockWindow_Closing;
    }

    private async void UnlockButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.Password = PasswordBox.Password;

        bool success = await _vm.UnlockAsync();
        if (success)
        {
            Closing -= UnlockWindow_Closing;
            DialogResult = true;
            Close();
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Closing -= UnlockWindow_Closing;
        System.Windows.Application.Current.Shutdown();
    }

    private void UnlockWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_vm.IsUnlocked)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
