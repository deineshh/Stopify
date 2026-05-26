using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.ViewModels.Queue;
using SpotifyClone.Desktop.Utilities.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SpotifyClone.Desktop.ViewModels.NowPlaying;

public class NowPlayingViewModel : ViewModelBase
{
    private string? _artist;
    private string? _artistImagePath;
    private string? _monthlyListeners;
    private string? _artistDescription;

    private QueueItemViewModel _nextSong;
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

    public bool IsSaved
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsFollowing
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string PlaylistTitle
    {
        get;
        set => SetProperty(ref field, value);
    } = "Azahriah";

    public string SongImagePath
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string Title
    {
        get;
        set => SetProperty(ref field, value);
    } = "PANNONIA";

    public string SaveTo
    {
        get;
        set => SetProperty(ref field, value);
    } = "Liked Songs";

    public string? Artist
    {
        get => _artist;
        set => SetProperty(ref _artist, value);
    }

    public string? ArtistImagePath
    {
        get => _artistImagePath;
        set => SetProperty(ref _artistImagePath, value);
    }

    public string? MonthlyListeners
    {
        get => _monthlyListeners;
        set => SetProperty(ref _monthlyListeners, value);
    }

    public string? ArtistDescription
    {
        get => _artistDescription;
        set => SetProperty(ref _artistDescription, value);
    }

    public QueueItemViewModel NextSong
    {
        get => _nextSong;
        set => SetProperty(ref _nextSong, value);
    }

    public ObservableCollection<string> Authors { get; }

    public ObservableCollection<NowPlayingCreditsItemViewModel> Credits { get; }

    public NowPlayingViewModel(UIState uiState)
    {
        _artist = "Azahriah";
        _artistImagePath = string.Empty;
        _monthlyListeners = "700,000";
        _artistDescription = "creator from hungary";

        _uiState = uiState;
        _uiState.PropertyChanged += UIStatePropertyChanged;

        Authors = new ObservableCollection<string>
        {
            "Azahriah",
            "DESH",
            "Young Fly",
        };

        Credits = new ObservableCollection<NowPlayingCreditsItemViewModel>
        {
            new NowPlayingCreditsItemViewModel("Azahriah", true),
            new NowPlayingCreditsItemViewModel("DESH", false),
            new NowPlayingCreditsItemViewModel("Young Fly", false),
        };

        _nextSong = new QueueItemViewModel("BAKPAKK", string.Empty);
    }

    private void UIStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_uiState.NowPlayingCollapseState):
                OnPropertyChanged(nameof(NowPlayingCollapseState));
                break;
        }
    }
}
