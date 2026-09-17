using System.Windows;

namespace TopLab.Presentation.Views.Lab;

public partial class AntibioticEditorWindow : Window
{
    private readonly ViewModels.Lab.AntibioticEditorViewModel _vm;

    public AntibioticEditorWindow(ViewModels.Lab.AntibioticEditorViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        bool success = await _vm.SaveAsync();
        if (success)
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
