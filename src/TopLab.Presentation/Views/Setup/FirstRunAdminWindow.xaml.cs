using System.Windows;
using System.Windows.Controls;
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
        _vm.Password = ShowPasswordCheckBox.IsChecked == true ? PlainPasswordBox.Text : PasswordBox.Password;
        _vm.ConfirmPassword = ShowConfirmPasswordCheckBox.IsChecked == true ? PlainConfirmPasswordBox.Text : ConfirmPasswordBox.Password;
        _vm.SecondaryPassword = ShowSecondaryPasswordCheckBox.IsChecked == true ? PlainSecondaryPasswordBox.Text : SecondaryPasswordBox.Password;
        _vm.ConfirmSecondaryPassword = ShowConfirmSecondaryPasswordCheckBox.IsChecked == true ? PlainConfirmSecondaryPasswordBox.Text : ConfirmSecondaryPasswordBox.Password;

        bool success = await _vm.CreateAsync();
        if (success)
        {
            MessageBox.Show("تم إنشاء حساب مدير النظام بنجاح. سيتم الآن عرض شاشة تسجيل الدخول.", "Top-Lab", MessageBoxButton.OK, MessageBoxImage.Information);
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
        SyncSecretVisibility(ShowPasswordCheckBox.IsChecked == true, PasswordBox, PlainPasswordBox);
    }

    private void ShowConfirmPasswordCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        SyncSecretVisibility(ShowConfirmPasswordCheckBox.IsChecked == true, ConfirmPasswordBox, PlainConfirmPasswordBox);
    }

    private void ShowSecondaryPasswordCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        SyncSecretVisibility(ShowSecondaryPasswordCheckBox.IsChecked == true, SecondaryPasswordBox, PlainSecondaryPasswordBox);
    }

    private void ShowConfirmSecondaryPasswordCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        SyncSecretVisibility(ShowConfirmSecondaryPasswordCheckBox.IsChecked == true, ConfirmSecondaryPasswordBox, PlainConfirmSecondaryPasswordBox);
    }

    private static void SyncSecretVisibility(bool revealed, PasswordBox masked, TextBox plain)
    {
        if (revealed)
        {
            plain.Text = masked.Password;
            plain.Visibility = Visibility.Visible;
            masked.Visibility = Visibility.Collapsed;
        }
        else
        {
            masked.Password = plain.Text;
            masked.Visibility = Visibility.Visible;
            plain.Visibility = Visibility.Collapsed;
        }
    }
}
