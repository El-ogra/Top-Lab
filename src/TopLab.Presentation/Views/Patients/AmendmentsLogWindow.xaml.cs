using System.Windows;

namespace TopLab.Presentation.Views.Patients;

public partial class AmendmentsLogWindow : Window
{
    public AmendmentsLogWindow(ViewModels.Patients.AmendmentsLogViewModel vm)
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
