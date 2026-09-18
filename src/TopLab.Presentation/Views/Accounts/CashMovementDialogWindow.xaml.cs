using System.Windows;

namespace TopLab.Presentation.Views.Accounts;

public partial class CashMovementDialogWindow : Window
{
    private readonly ViewModels.Accounts.CashMovementDialogViewModel _vm;

    public CashMovementDialogWindow(ViewModels.Accounts.CashMovementDialogViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        bool success = await _vm.SaveAsync();
        if (success)
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
