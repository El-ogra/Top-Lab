using System.Windows;

namespace TopLab.Presentation.Views.Lab;

public partial class AnalyteEditorWindow : Window
{
    public AnalyteEditorWindow(ViewModels.Lab.AnalyteEditorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
