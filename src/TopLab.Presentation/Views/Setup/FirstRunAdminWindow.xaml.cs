using System.Windows;
using TopLab.Presentation.ViewModels.Setup;

namespace TopLab.Presentation.Views.Setup;

public partial class FirstRunAdminWindow : Window
{
    private readonly FirstRunAdminViewModel _vm;

    public FirstRunAdminWindow(FirstRunAdminViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.UserName = UserNameTextBox.Text;
        _vm.Password = PasswordBox.Password;
        _vm.ConfirmPassword = ConfirmPasswordBox.Password;
        _vm.SecondaryPassword = SecondaryPasswordBox.Password;
        _vm.ConfirmSecondaryPassword = ConfirmSecondaryPasswordBox.Password;

        bool success = await _vm.CreateAsync();
        if (success)
        {
            DialogResult = true;
            Close();
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
