using System.Windows;

namespace TopLab.Presentation.Views.Lab;

public partial class SampleDrawBoardWindow : Window
{
    public SampleDrawBoardWindow(ViewModels.Lab.SampleDrawBoardViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
