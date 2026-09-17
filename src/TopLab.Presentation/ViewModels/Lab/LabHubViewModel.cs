using TopLab.Presentation.Common;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// Lab hub (S-02 Slice 3): tab container behind the shell «المعمل» button.
/// Slices 4, 5, 7 add their tabs to this same hub.
/// </summary>
public sealed class LabHubViewModel : ViewModelBase
{
    public LabHubViewModel(
        TestCatalogViewModel catalog,
        TestGroupsViewModel groups,
        WorkGroupLogsViewModel workLogs,
        AnalytesViewModel analytes,
        ProfilesViewModel profiles,
        AntibioticsViewModel antibiotics,
        CultureAttachmentViewModel cultureAttachment)
    {
        Catalog = catalog;
        Groups = groups;
        WorkLogs = workLogs;
        Analytes = analytes;
        Profiles = profiles;
        Antibiotics = antibiotics;
        CultureAttachment = cultureAttachment;
    }

    public TestCatalogViewModel Catalog { get; }

    public TestGroupsViewModel Groups { get; }

    public WorkGroupLogsViewModel WorkLogs { get; }

    public AnalytesViewModel Analytes { get; }

    public ProfilesViewModel Profiles { get; }

    public AntibioticsViewModel Antibiotics { get; }

    public CultureAttachmentViewModel CultureAttachment { get; }

    public async Task LoadAsync()
    {
        await Catalog.LoadAsync();
        await Groups.LoadAsync();
        await WorkLogs.LoadAsync();
        await Analytes.LoadAsync();
        await Profiles.LoadAsync();
        await Antibiotics.LoadAsync();
        await CultureAttachment.LoadAsync();
    }
}
