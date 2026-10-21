using System.Windows;

namespace TopLab.Presentation.Views.External;

/// <summary>
/// STAGE-01 (baseline 1.2.2.2.10.1 / 1.2.2.2.11.1): a read-only view of an
/// entity's additional information.
/// </summary>
public partial class ExternalEntityDetailsWindow : Window
{
    public ExternalEntityDetailsWindow(ViewModels.External.ExternalEntityDetailsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
