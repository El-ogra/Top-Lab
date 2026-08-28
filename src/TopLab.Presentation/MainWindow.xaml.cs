using System.Windows;
using TopLab.Presentation.ViewModels.Shell;

namespace TopLab.Presentation;

public partial class MainWindow : Window
{
    private readonly ShellViewModel _vm;

    public MainWindow(ShellViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
    }

    protected override void OnClosed(EventArgs e)
    {
        _vm.Dispose();
        base.OnClosed(e);
    }
}
