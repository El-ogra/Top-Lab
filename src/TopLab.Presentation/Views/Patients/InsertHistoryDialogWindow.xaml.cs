using System.Windows;

namespace TopLab.Presentation.Views.Patients;

public partial class InsertHistoryDialogWindow : Window
{
    private readonly ViewModels.Patients.InsertHistoryDialogViewModel _vm;

    public InsertHistoryDialogWindow(ViewModels.Patients.InsertHistoryDialogViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        bool success = await _vm.InsertAsync();
        if (success)
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // GAP-27: abandon the current history operation in the ViewModel before
        // closing — no insert is dispatched and no report is applied.
        _vm.Cancel();
        DialogResult = false;
        Close();
    }
}
