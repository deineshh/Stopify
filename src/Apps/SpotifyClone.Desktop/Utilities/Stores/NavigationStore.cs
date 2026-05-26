using SpotifyClone.Desktop.ViewModels.Base;

namespace SpotifyClone.Desktop.Utilities.Stores;

public class NavigationStore
{
    public ViewModelBase CurrentViewModel
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnCurrentViewModelChanged();
            }
        }
    } = null!;
    public event Action? CurrentViewModelChanged;
    protected virtual void OnCurrentViewModelChanged()
        => CurrentViewModelChanged?.Invoke();
}
