using System.Windows;

namespace TopLab.Presentation.Views.Users;

public partial class ChangeOwnPasswordWindow : Window
{
    private readonly ViewModels.Users.ChangeOwnPasswordViewModel _vm;

    public ChangeOwnPasswordWindow(ViewModels.Users.ChangeOwnPasswordViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.CurrentPassword = CurrentPasswordBox.Password;
        _vm.NewPassword = NewPasswordBox.Password;
        _vm.ConfirmNewPassword = ConfirmPasswordBox.Password;

        bool success = await _vm.ChangeAsync();
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
