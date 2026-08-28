using Microsoft.Extensions.DependencyInjection;

namespace TopLab.Presentation.Common.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _sp;
    private ViewModelBase? _current;

    public NavigationService(IServiceProvider sp)
    {
        _sp = sp;
    }

    public ViewModelBase? CurrentViewModel => _current;

    public event Action<ViewModelBase?>? Navigated;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = _sp.GetRequiredService<TViewModel>();
        NavigateTo(vm);
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        _current = viewModel;
        Navigated?.Invoke(_current);
    }
}
