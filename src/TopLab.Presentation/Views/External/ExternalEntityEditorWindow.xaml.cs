using System.Windows;

namespace TopLab.Presentation.Views.External;

public partial class ExternalEntityEditorWindow : Window
{
    private readonly ViewModels.External.ExternalEntityEditorViewModel _vm;

    public ExternalEntityEditorWindow(ViewModels.External.ExternalEntityEditorViewModel vm)
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
