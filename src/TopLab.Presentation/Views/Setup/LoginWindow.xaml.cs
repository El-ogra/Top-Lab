using System.Windows;
using TopLab.Presentation.ViewModels.Setup;

namespace TopLab.Presentation.Views.Setup;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.UserName = UserNameTextBox.Text;
        _vm.Password = ShowPasswordCheckBox.IsChecked == true ? PlainPasswordBox.Text : PasswordBox.Password;

        bool success = await _vm.SignInAsync();
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

    private void ShowPasswordCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        bool revealed = ShowPasswordCheckBox.IsChecked == true;
        if (revealed)
        {
            PlainPasswordBox.Text = PasswordBox.Password;
            PlainPasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
        }
        else
        {
            PasswordBox.Password = PlainPasswordBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PlainPasswordBox.Visibility = Visibility.Collapsed;
        }
    }
}
