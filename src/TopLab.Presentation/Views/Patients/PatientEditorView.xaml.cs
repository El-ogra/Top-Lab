using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TopLab.Presentation.Views.Patients;

public partial class PatientEditorView : UserControl
{
    public PatientEditorView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.4.2): the plain password is pushed to the view
    /// model, which hands it to the command that hashes it. It is never stored.
    /// </summary>
    private void PrintResultPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Patients.PatientEditorViewModel vm)
        {
            var value = PrintResultPasswordBox.Password;
            vm.PrintResultPasswordText = string.IsNullOrEmpty(value) ? null : value;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ViewModels.Patients.PatientEditorViewModel oldVm)
        {
            oldVm.PropertyChanged -= Vm_PropertyChanged;
        }

        if (e.NewValue is ViewModels.Patients.PatientEditorViewModel vm)
        {
            vm.PropertyChanged += Vm_PropertyChanged;
        }
    }

    /// <summary>
    /// The view model drops the plain password once it has been stored; the box
    /// must not keep showing it.
    /// </summary>
    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.Patients.PatientEditorViewModel.PrintResultPasswordText)
            && sender is ViewModels.Patients.PatientEditorViewModel vm
            && vm.PrintResultPasswordText is null
            && !string.IsNullOrEmpty(PrintResultPasswordBox.Password))
        {
            PrintResultPasswordBox.Clear();
        }
    }
}
