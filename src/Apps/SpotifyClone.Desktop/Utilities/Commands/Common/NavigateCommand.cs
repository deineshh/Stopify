using SpotifyClone.Desktop.Utilities.Commands.Base;
using SpotifyClone.Desktop.Utilities.Stores;
using SpotifyClone.Desktop.ViewModels.Base;

namespace SpotifyClone.Desktop.Utilities.Commands.Common;

public class NavigateCommand<TViewModel>(
    NavigationStore navigationStore,
    Func<TViewModel> viewModelFactory)
    : CommandBase
    where TViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore = navigationStore;
    private readonly Func<TViewModel> _viewModelFactory = viewModelFactory;

    public override void Execute(object? parameter)
        => _navigationStore.CurrentViewModel = _viewModelFactory.Invoke();
}
