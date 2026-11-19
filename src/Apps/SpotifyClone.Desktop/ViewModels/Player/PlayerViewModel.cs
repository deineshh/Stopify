using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.ViewModels.Common;
using SpotifyClone.Desktop.Utilities.Commands.Common;
using SpotifyClone.Desktop.Utilities.Commands.Player;
using SpotifyClone.Desktop.Utilities.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SpotifyClone.Desktop.ViewModels.Player;

public class PlayerViewModel : ViewModelBase
{
    private bool _isShuffling;
    private string _title;
    private string _totalTime;
    private readonly UIState _uiState;

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

    public byte RepeatState
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double MaxMediaValue
    {
        get;
        set => SetProperty(ref field, value);
    } = 100;

    public double CurrentMediaValue
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double VolumeValue
    {
        get;
        set => SetProperty(ref field, value);
    } = 0.5;

    public bool IsSaved
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsShuffling
    {
        get => _isShuffling;
        set
        {
            SetProperty(ref _isShuffling, value);
            SetHoverPopupText("Azahriah");
        }
    }

    public bool IsPlaying
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsMuted
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string ImagePath
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string CurrentTime
    {
        get;
        set => SetProperty(ref field, value);
    } = "00:00";

    public string TotalTime
    {
        get => _totalTime;
        set => SetProperty(ref _totalTime, value);
    }

    public string HoverPopupText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public MediaPlayer MediaPlayer
    {
        get;
        set => SetProperty(ref field, value);
    } = new();
    public DispatcherTimer Timer
    {
        get;
        set => SetProperty(ref field, value);
    } = new() { Interval = TimeSpan.FromSeconds(1) };

    public ObservableCollection<AuthorItemViewModel> Authors { get; } = [];

    public ICommand NavigatePlaylistCommand { get; }
    public ICommand NavigateArtistCommand { get; }
    public ICommand SaveSongCommand { get; }
    public ICommand ShuffleQueueCommand { get; }
    public ICommand PreviousSongCommand { get; }
    public ICommand NextSongCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand RepeatSongCommand { get; }

    public PlayerViewModel(UIState uiState)
    {
        _isShuffling = false;
        _title = "introvertált dal";
        _totalTime = "2:45";

        Authors = new()
        {
            new("Azahriah", new()),
            new("DESH", new()),
            new("Young Fly", new()),
        };

        _uiState = uiState;
        _uiState.PropertyChanged += UIStatePropertyChanged;

        NavigatePlaylistCommand = new NavigatePlaylistCommand();
        NavigateArtistCommand = new NavigateArtistCommand();
        SaveSongCommand = new SaveSongCommand();
        ShuffleQueueCommand = new ShuffleQueueCommand();
        PreviousSongCommand = new PreviousSongCommand();
        NextSongCommand = new NextSongCommand();
        PlayCommand = new PlayCommand();
        RepeatSongCommand = new RepeatSongCommand();
    }

    private void UIStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_uiState.NowPlayingCollapseState):
                OnPropertyChanged(nameof(NowPlayingCollapseState));
                break;
            case nameof(_uiState.QueueCollapseState):
                OnPropertyChanged(nameof(QueueCollapseState));
                break;
        }
    }

    private void SetHoverPopupText(string playlistTitle) =>
        HoverPopupText = $"{(IsShuffling ? "Disable" : "Enable")} Shuffle for {playlistTitle}";
}
