using System.Windows;

namespace TopLab.Presentation.Views.Dialogs;

public partial class SystemMenuPasswordDialog : Window
{
    public SystemMenuPasswordDialog()
    {
        InitializeComponent();
    }

    public string EnteredPassword => PasswordBox.Password;

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ErrorText.Text = "كلمة المرور مطلوبة.";
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
