using SpotifyClone.Desktop.ViewModels.Base;
using System.Collections.ObjectModel;

namespace SpotifyClone.Desktop.ViewModels.Playlist;

public class PlaylistViewModel : ViewModelBase
{
    public bool IsPlaying
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsShuffling
    {
        get;
        set
        {
            SetProperty(ref field, value);
            UpdateHoverPopupText();
        }
    }

    public bool IsSaved
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsSearching
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Title
    {
        get;
        set => SetProperty(ref field, value);
    } = "Coding Music Programming Playlist";

    public string Type
    {
        get;
        set => SetProperty(ref field, value);
    } = "Public Playlist";

    public string Description
    {
        get;
        set => SetProperty(ref field, value);
    } = "best coding music - best coding songs - lofi code song - ";

    public string Saves
    {
        get;
        set => SetProperty(ref field, value);
    } = "125,000";

    public string Songs
    {
        get;
        set => SetProperty(ref field, value);
    } = "201";

    public string Duration
    {
        get;
        set => SetProperty(ref field, value);
    } = "7 hr 21 min";

    public string HoverPopupText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string SaveTo
    {
        get;
        set => SetProperty(ref field, value);
    } = "Liked Songs";

    public string SearchText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string ImagePath
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public ObservableCollection<PlaylistAuthorViewModel> Authors { get; }

    public ObservableCollection<PlaylistItemViewModel> SongItems { get; }

    public ObservableCollection<PlaylistItemViewModel> RecommendedItems { get; }

    public PlaylistViewModel()
    {
        Authors = new ObservableCollection<PlaylistAuthorViewModel>()
        {
            new PlaylistAuthorViewModel("Azahriah", string.Empty),
            new PlaylistAuthorViewModel("DESH", string.Empty),
            new PlaylistAuthorViewModel("Young Fly", string.Empty),
        };

        SongItems = new ObservableCollection<PlaylistItemViewModel>()
        {
            new PlaylistItemViewModel("1", "Tisztán iszom", "A ló túloldalán", "3 years ago", "3:00", string.Empty),
            new PlaylistItemViewModel("2", "Drogba", "A ló túloldalán", "3 years ago", "2:43", string.Empty),
            new PlaylistItemViewModel("3", "Miafasz", "A ló túloldalán", "3 years ago", "3:13", string.Empty),
            new PlaylistItemViewModel("4", "Felednéd", "A ló túloldalán", "3 years ago", "3:01", string.Empty),
            new PlaylistItemViewModel("5", "Okari", "A ló túloldalán", "3 years ago", "4:09", string.Empty),
            new PlaylistItemViewModel("6", "Pullup", "A ló túloldalán", "3 years ago", "2:17", string.Empty),
            new PlaylistItemViewModel("7", "Habibi", "A ló túloldalán", "3 years ago", "2:41", string.Empty),
            new PlaylistItemViewModel("8", "tevagyazalány", "A ló túloldalán", "3 years ago", "2:18", string.Empty),
            new PlaylistItemViewModel("9", "Mind1", "A ló túloldalán", "3 years ago", "3:11", string.Empty),
            new PlaylistItemViewModel("10", "Lóerő", "A ló túloldalán", "3 years ago", "2:57", string.Empty),
            new PlaylistItemViewModel("11", "Megmentő", "A ló túloldalán", "3 years ago", "2:54", string.Empty),
            new PlaylistItemViewModel("12", "Domapin (Bonus Track)", "A ló túloldalán", "3 years ago", "2:08", string.Empty),
        };

        RecommendedItems = new ObservableCollection<PlaylistItemViewModel>()
        {
            new PlaylistItemViewModel("1", "Tisztán iszom", "A ló túloldalán", "3 years ago", "3:00", string.Empty),
            new PlaylistItemViewModel("2", "Drogba", "A ló túloldalán", "3 years ago", "2:43", string.Empty),
            new PlaylistItemViewModel("3", "Miafasz", "A ló túloldalán", "3 years ago", "3:13", string.Empty),
            new PlaylistItemViewModel("4", "Felednéd", "A ló túloldalán", "3 years ago", "3:01", string.Empty),
            new PlaylistItemViewModel("5", "Okari", "A ló túloldalán", "3 years ago", "4:09", string.Empty),
            new PlaylistItemViewModel("6", "Pullup", "A ló túloldalán", "3 years ago", "2:17", string.Empty),
            new PlaylistItemViewModel("7", "Habibi", "A ló túloldalán", "3 years ago", "2:41", string.Empty),
            new PlaylistItemViewModel("8", "tevagyazalány", "A ló túloldalán", "3 years ago", "2:18", string.Empty),
            new PlaylistItemViewModel("9", "Mind1", "A ló túloldalán", "3 years ago", "3:11", string.Empty),
            new PlaylistItemViewModel("10", "Lóerő", "A ló túloldalán", "3 years ago", "2:57", string.Empty),
        };

        UpdateHoverPopupText();
    }

    private void UpdateHoverPopupText() =>
        HoverPopupText = $"{(IsShuffling ? "Disable" : "Enable")} Shuffle for {Title}";
}
