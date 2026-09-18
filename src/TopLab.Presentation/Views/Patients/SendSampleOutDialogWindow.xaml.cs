using System.Windows;

namespace TopLab.Presentation.Views.Patients;

public partial class SendSampleOutDialogWindow : Window
{
    private readonly ViewModels.Patients.SendSampleOutDialogViewModel _vm;

    public SendSampleOutDialogWindow(ViewModels.Patients.SendSampleOutDialogViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        bool success = await _vm.SendAsync();
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
