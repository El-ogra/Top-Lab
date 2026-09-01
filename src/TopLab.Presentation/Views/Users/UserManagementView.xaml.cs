using System.Windows.Controls;

namespace TopLab.Presentation.Views.Users;

public partial class UserManagementView : UserControl
{
    public UserManagementView()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Users.UserManagementViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

    private void SecondaryPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Users.UserManagementViewModel vm)
        {
            vm.SecondaryPassword = SecondaryPasswordBox.Password;
        }
    }
}
