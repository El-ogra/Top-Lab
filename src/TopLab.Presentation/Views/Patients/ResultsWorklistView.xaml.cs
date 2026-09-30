using System.Windows.Controls;
using System.Windows.Input;
using TopLab.Presentation.ViewModels.Patients;

namespace TopLab.Presentation.Views.Patients;

public partial class ResultsWorklistView : UserControl
{
    public ResultsWorklistView()
    {
        InitializeComponent();
    }

    /// <summary>Row double-click opens detail — no business logic here (WP-02).</summary>
    private void WorklistGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ResultsWorklistViewModel vm
            && vm.OpenDetailCommand.CanExecute(vm.SelectedItem))
        {
            vm.OpenDetailCommand.Execute(vm.SelectedItem);
        }
    }
}
