using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.ViewModels.Patients;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Patients hub (S-01 slice S5): the four-action screen behind the shell's
/// «المرضى» button. «إدخال نتائج التحاليل» gateway enabled by S-04 Slice 2.
/// </summary>
public sealed class PatientsHubViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private bool _addEditPatientEnabled;
    private bool _enterResultsEnabled;
    private bool _searchPatientEnabled;
    private bool _deliverResultsEnabled;

    public PatientsHubViewModel(INavigationService navigation)
    {
        _navigation = navigation;

        AddEditPatientEnabled = true;
        OpenAddEditPatientCommand = new RelayCommand(_ =>
        {
            _navigation.NavigateTo<PatientEditorViewModel>();
            if (_navigation.CurrentViewModel is PatientEditorViewModel vm)
            {
                _ = vm.LoadCatalogAsync();
            }
        });

        // S-04 Slice 2: enable Results Worklist gateway
        EnterResultsEnabled = true;
        OpenEnterResultsCommand = new RelayCommand(_ =>
        {
            _navigation.NavigateTo<ResultsWorklistViewModel>();
            if (_navigation.CurrentViewModel is ResultsWorklistViewModel vm)
            {
                _ = vm.LoadAsync();
            }
        });

        // S-05 Slice 0: enable M08 patient search gateway
        SearchPatientEnabled = true;
        OpenSearchPatientCommand = new RelayCommand(_ =>
        {
            _navigation.NavigateTo<PatientSearchViewModel>();
            if (_navigation.CurrentViewModel is PatientSearchViewModel vm)
            {
                _ = vm.LoadAsync();
            }
        });

        // S-05 Slice 4: enable M09 delivery gateway
        DeliverResultsEnabled = true;
        OpenDeliverResultsCommand = new RelayCommand(_ =>
        {
            _navigation.NavigateTo<ResultDeliveryViewModel>();
            if (_navigation.CurrentViewModel is ResultDeliveryViewModel vm)
            {
                _ = vm.LoadAsync();
            }
        });
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