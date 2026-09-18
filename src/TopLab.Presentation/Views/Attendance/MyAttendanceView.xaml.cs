using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TopLab.Presentation.Views.Attendance;

public partial class MyAttendanceView : UserControl
{
    private DispatcherTimer? _clockTimer;

    public MyAttendanceView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Attendance.MyAttendanceViewModel vm)
        {
            _ = vm.LoadAsync();
        }

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) =>
        {
            if (DataContext is ViewModels.Attendance.MyAttendanceViewModel vm)
            {
                vm.UpdateClock();
            }
        };
        _clockTimer.Start();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        _clockTimer?.Stop();
        _clockTimer = null;
    }
}
