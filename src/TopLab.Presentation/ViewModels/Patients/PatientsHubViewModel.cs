using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Patients hub (S-01 slice S5): the four-action screen behind the shell's
/// «المرضى» button. All four buttons ship disabled in this slice — the three
/// future screens (results entry, patient search, results delivery) belong to
/// later workstreams, and «إضافة وتعديل بيانات المرضى» is enabled by S6 when
/// the unified editor lands (disabled-by-design placeholder idiom, SD-6).
/// Enablement is data-driven so S6 flips exactly one property.
/// </summary>
public sealed class PatientsHubViewModel : ViewModelBase
{
    private bool _addEditPatientEnabled;
    private bool _enterResultsEnabled;
    private bool _searchPatientEnabled;
    private bool _deliverResultsEnabled;

    public PatientsHubViewModel(INavigationService navigation)
    {
        // S6: the unified editor is live — the entry button is enabled and
        // navigates to it (mirrors the vm.LoadAsync() idiom at
        // ShellViewModel.cs:140–144). The other three buttons stay disabled
        // until their workstreams land.
        AddEditPatientEnabled = true;
        OpenAddEditPatientCommand = new RelayCommand(_ =>
        {
            navigation.NavigateTo<PatientEditorViewModel>();
            if (navigation.CurrentViewModel is PatientEditorViewModel vm)
            {
                _ = vm.LoadCatalogAsync();
            }
        });
        OpenEnterResultsCommand = new RelayCommand(_ => { });
        OpenSearchPatientCommand = new RelayCommand(_ => { });
        OpenDeliverResultsCommand = new RelayCommand(_ => { });
    }

    public string HelpText => "من هنا تدير كل ما يخص المرضى: تسجيل بيانات مريض جديد أو تعديل بيانات مسجل، ثم إدخال النتائج والبحث عن المرضى وتسليم النتائج.";

    public bool AddEditPatientEnabled
    {
        get => _addEditPatientEnabled;
        set => SetProperty(ref _addEditPatientEnabled, value);
    }

    public bool EnterResultsEnabled
    {
        get => _enterResultsEnabled;
        set => SetProperty(ref _enterResultsEnabled, value);
    }

    public bool SearchPatientEnabled
    {
        get => _searchPatientEnabled;
        set => SetProperty(ref _searchPatientEnabled, value);
    }

    public bool DeliverResultsEnabled
    {
        get => _deliverResultsEnabled;
        set => SetProperty(ref _deliverResultsEnabled, value);
    }

    public RelayCommand OpenAddEditPatientCommand { get; }

    public RelayCommand OpenEnterResultsCommand { get; }

    public RelayCommand OpenSearchPatientCommand { get; }

    public RelayCommand OpenDeliverResultsCommand { get; }
}
