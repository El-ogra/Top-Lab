using System.Windows;

namespace TopLab.Presentation.Views.Patients;

public partial class AmendDialogWindow : Window
{
    private readonly ViewModels.Patients.AmendDialogViewModel _vm;

    public AmendDialogWindow(ViewModels.Patients.AmendDialogViewModel vm)
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
