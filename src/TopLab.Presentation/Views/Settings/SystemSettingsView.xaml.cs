using System.Windows.Controls;

namespace TopLab.Presentation.Views.Settings;

public partial class SystemSettingsView : UserControl
{
    public SystemSettingsView()
    {
        InitializeComponent();
    }

    private void DbPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Settings.SystemSettingsViewModel vm)
        {
            vm.Password = DbPasswordBox.Password;
        }
    }
}