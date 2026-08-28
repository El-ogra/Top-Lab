using TopLab.Presentation.Common;

namespace TopLab.Presentation.Common.Navigation;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }

    event Action<ViewModelBase?> Navigated;

    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    void NavigateTo(ViewModelBase viewModel);
}
