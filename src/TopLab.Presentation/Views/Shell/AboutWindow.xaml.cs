using System.Reflection;
using System.Windows;

namespace TopLab.Presentation.Views.Shell;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "—";
        VersionText.Text = $"الإصدار: {version} [Placeholder — بانتظار قرار المالك]";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
