using System.Windows;

namespace TopLab.Presentation.Views.External;

public partial class ExternalEntityPickerWindow : Window
{
    private readonly ViewModels.External.ExternalEntityPickerViewModel _vm;

    public ExternalEntityPickerWindow(ViewModels.External.ExternalEntityPickerViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedEntity is null)
        {
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
