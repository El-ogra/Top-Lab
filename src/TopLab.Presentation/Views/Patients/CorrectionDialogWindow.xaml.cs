using System.Windows;

namespace TopLab.Presentation.Views.Patients;

public partial class CorrectionDialogWindow : Window
{
    private readonly ViewModels.Patients.CorrectionDialogViewModel _vm;

    public CorrectionDialogWindow(ViewModels.Patients.CorrectionDialogViewModel vm)
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
