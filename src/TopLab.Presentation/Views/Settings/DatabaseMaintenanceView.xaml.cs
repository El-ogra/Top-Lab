using System.Windows;
using System.Windows.Controls;

namespace TopLab.Presentation.Views.Settings;

public partial class DatabaseMaintenanceView : UserControl
{
    public DatabaseMaintenanceView()
    {
        InitializeComponent();
        Loaded += DatabaseMaintenanceView_Loaded;
    }

    private void DatabaseMaintenanceView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Settings.DatabaseMaintenanceViewModel vm
            && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }
}