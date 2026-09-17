using System.Windows;

namespace TopLab.Presentation.Views.Lab;

public partial class ProfileEditorWindow : Window
{
    public ProfileEditorWindow(ViewModels.Lab.ProfileEditorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
