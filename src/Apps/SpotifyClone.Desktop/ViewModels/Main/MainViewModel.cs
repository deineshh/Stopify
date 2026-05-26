using System.ComponentModel;
using SpotifyClone.Desktop.Utilities.Stores;
using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.ViewModels.NowPlaying;
using SpotifyClone.Desktop.ViewModels.Player;
using SpotifyClone.Desktop.ViewModels.Queue;
using SpotifyClone.Desktop.ViewModels.Search;
using SpotifyClone.Desktop.ViewModels.Sidebar;
using SpotifyClone.Desktop.ViewModels.Titlebar;

namespace SpotifyClone.Desktop.ViewModels.Main;

public class MainViewModel : ViewModelBase
{
    private readonly UIState _uiState;

    public bool SidebarCollapseState
    {
        get => _uiState.SidebarCollapseState;
        set
        {
            if (_uiState.SidebarCollapseState != value)
            {
                _uiState.SidebarCollapseState = value;
                OnPropertyChanged();
            }
        }
    }

    public bool QueueCollapseState
    {
        get => _uiState.QueueCollapseState;
        set
        {
            if (_uiState.QueueCollapseState != value)
            {
                _uiState.QueueCollapseState = value;
                OnPropertyChanged();
            }
        }
    }

    public bool NowPlayingCollapseState
    {
        get => _uiState.NowPlayingCollapseState;
        set
        {
            if (_uiState.NowPlayingCollapseState != value)
            {
                _uiState.NowPlayingCollapseState = value;
                OnPropertyChanged();
            }
        }
    }

    public static ViewModelBase MainContentViewModel => new SearchViewModel();
    public TitlebarViewModel TitlebarViewModel { get; }
    public SidebarViewModel SidebarViewModel { get; }
    public NowPlayingViewModel NowPlayingViewModel { get; }
    public QueueViewModel QueueViewModel { get; }
    public PlayerViewModel PlayerViewModel { get; }

    public MainViewModel(
        NavigationStore navigationStore,
        UIState uiState,
        TitlebarViewModel titlebarViewModel,
        SidebarViewModel sidebarViewModel,
        NowPlayingViewModel nowPlayingViewModel,
        QueueViewModel queueViewModel,
        PlayerViewModel playerViewModel)
    {
        _uiState = uiState;
        _uiState.PropertyChanged += UIStatePropertyChanged;

        navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        TitlebarViewModel = titlebarViewModel;
        SidebarViewModel = sidebarViewModel;
        NowPlayingViewModel = nowPlayingViewModel;
        QueueViewModel = queueViewModel;
        PlayerViewModel = playerViewModel;
    }

    private void OnCurrentViewModelChanged()
        => OnPropertyChanged(nameof(MainContentViewModel));

    private void UIStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_uiState.SidebarCollapseState):
                OnPropertyChanged(nameof(SidebarCollapseState));
                break;
            case nameof(_uiState.NowPlayingCollapseState):
                OnPropertyChanged(nameof(NowPlayingCollapseState));
                break;
            case nameof(_uiState.QueueCollapseState):
                OnPropertyChanged(nameof(QueueCollapseState));
                break;
        }
    }
}
