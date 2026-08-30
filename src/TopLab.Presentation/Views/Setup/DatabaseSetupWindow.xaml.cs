using System.Windows;
using TopLab.Presentation.ViewModels.Setup;

namespace TopLab.Presentation.Views.Setup;

public partial class DatabaseSetupWindow : Window
{
    private readonly DatabaseSetupViewModel _vm;

    public DatabaseSetupWindow(DatabaseSetupViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.Server = ServerTextBox.Text;
        _vm.Database = DatabaseTextBox.Text;
        _vm.IntegratedSecurity = IntegratedSecurityCheckBox.IsChecked ?? false;
        _vm.Username = UsernameTextBox.Text;
        _vm.Password = PasswordBox.Password;
        await _vm.TestConnectionAsync();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.Server = ServerTextBox.Text;
        _vm.Database = DatabaseTextBox.Text;
        _vm.IntegratedSecurity = IntegratedSecurityCheckBox.IsChecked ?? false;
        _vm.Username = UsernameTextBox.Text;
        _vm.Password = PasswordBox.Password;
        if (await _vm.SaveAsync())
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