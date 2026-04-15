using Stopify.Presentation.Utilities.Stores;
using Stopify.Presentation.ViewModels.Artist;
using Stopify.Presentation.ViewModels.Base;
using Stopify.Presentation.ViewModels.NowPlaying;
using Stopify.Presentation.ViewModels.Player;
using Stopify.Presentation.ViewModels.Playlist;
using Stopify.Presentation.ViewModels.Queue;
using Stopify.Presentation.ViewModels.Search;
using Stopify.Presentation.ViewModels.Sidebar;
using Stopify.Presentation.ViewModels.Titlebar;
using System.ComponentModel;

namespace Stopify.Presentation.ViewModels.Main;

public class MainViewModel : ViewModelBase
{
    #region Fields

    private readonly UIState _uiState;
    private readonly NavigationStore _navigationStore;

    #endregion

    #region Properties

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

    public ViewModelBase MainContentViewModel => new SearchViewModel();
    public TitlebarViewModel TitlebarViewModel { get; }
    public SidebarViewModel SidebarViewModel { get; }
    public NowPlayingViewModel NowPlayingViewModel { get; }
    public QueueViewModel QueueViewModel { get; }
    public PlayerViewModel PlayerViewModel { get; }

    #endregion

    #region Constructors

    public MainViewModel(NavigationStore navigationStore,
                         UIState uiState,
                         TitlebarViewModel titlebarViewModel,
                         SidebarViewModel sidebarViewModel,
                         NowPlayingViewModel nowPlayingViewModel,
                         QueueViewModel queueViewModel,
                         PlayerViewModel playerViewModel)
    {
        _uiState = uiState;
        _uiState.PropertyChanged += UIStatePropertyChanged;

        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        TitlebarViewModel = titlebarViewModel;
        SidebarViewModel = sidebarViewModel;
        NowPlayingViewModel = nowPlayingViewModel;
        QueueViewModel = queueViewModel;
        PlayerViewModel = playerViewModel;
    }

    #endregion

    #region Event Handlers

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(MainContentViewModel));
    }

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

    #endregion
}
