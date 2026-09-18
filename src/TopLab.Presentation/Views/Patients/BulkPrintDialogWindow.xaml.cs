using System.Windows;

namespace TopLab.Presentation.Views.Patients;

public partial class BulkPrintDialogWindow : Window
{
    private readonly ViewModels.Patients.BulkPrintDialogViewModel _vm;

    public BulkPrintDialogWindow(ViewModels.Patients.BulkPrintDialogViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
    {
        ((ViewModels.Patients.BulkPrintDialogViewModel)DataContext)
            .ExecuteCommand.Execute(null);
        await Task.CompletedTask;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
