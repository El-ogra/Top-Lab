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
        WorkGroupLogsViewModel workLogs)
    {
        Catalog = catalog;
        Groups = groups;
        WorkLogs = workLogs;
    }

    public TestCatalogViewModel Catalog { get; }

    public TestGroupsViewModel Groups { get; }

    public WorkGroupLogsViewModel WorkLogs { get; }

    public async Task LoadAsync()
    {
        await Catalog.LoadAsync();
        await Groups.LoadAsync();
        await WorkLogs.LoadAsync();
    }
}
