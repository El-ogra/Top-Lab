using System.Windows;

namespace TopLab.Presentation.Views.Lab;

public partial class TestEditorWindow : Window
{
    public TestEditorWindow(ViewModels.Lab.TestEditorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
