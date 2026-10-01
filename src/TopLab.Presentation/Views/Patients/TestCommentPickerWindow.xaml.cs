using System.Windows;

namespace TopLab.Presentation.Views.Patients;

public partial class TestCommentPickerWindow : Window
{
    private readonly ViewModels.Patients.TestCommentPickerViewModel _vm;

    public TestCommentPickerWindow(ViewModels.Patients.TestCommentPickerViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.Pick())
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
