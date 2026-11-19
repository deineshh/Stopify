using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.ViewModels.Common;
using SpotifyClone.Desktop.ViewModels.Playlist;
using System.Collections.ObjectModel;

namespace SpotifyClone.Desktop.ViewModels.Artist;

public class ArtistViewModel : ViewModelBase
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

    public bool IsFollowing
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsFilteringPopularReleases
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public bool IsFilteringAlbums
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsFilteringSingles
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Title
    {
        get;
        set => SetProperty(ref field, value);
    } = "Azahriah";

    public string MonthlyListeners
    {
        get;
        set => SetProperty(ref field, value);
    } = "700,000";

    public string Description
    {
        get;
        set => SetProperty(ref field, value);
    } = "creator from hungary";

    public string HoverPopupText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public ObservableCollection<CommonItemViewModel> DiscographyItems { get; }

    public ObservableCollection<PlaylistItemViewModel> Populars { get; }

    public ArtistViewModel()
    {
        DiscographyItems = new ObservableCollection<CommonItemViewModel>
        {
            new CommonItemViewModel("ZHA MAJ DUR", "Latest Release · Single", string.Empty),
            new CommonItemViewModel("A ló túloldalán", "2022 · Album", string.Empty),
            new CommonItemViewModel("memento", "2023 · Album", string.Empty),
            new CommonItemViewModel("tripq", "2023 · EP", string.Empty),
            new CommonItemViewModel("silbak", "2022 · EP", string.Empty),
            new CommonItemViewModel("BAKPAKK", "2024 - Single", string.Empty),
            new CommonItemViewModel("skatulya I", "2024 · Album", string.Empty),
            new CommonItemViewModel("Puskás Aréna Live (2024)", "2024 · Album", string.Empty),
            new CommonItemViewModel("camouflage", "2021 · Album", string.Empty),
        };

        Populars = new ObservableCollection<PlaylistItemViewModel>
        {
            new PlaylistItemViewModel("1", "PANNONIA", "PANNONIA", "8 months ago", "2:27", string.Empty),
            new PlaylistItemViewModel("2", "BAKPAKK", "BAKPAKK", "8 months ago", "2:47", string.Empty),
            new PlaylistItemViewModel("3", "ZHA MAJ DUR", "ZHA MAJ DUR", "8 months ago", "3:39", string.Empty),
            new PlaylistItemViewModel("4", "Felednéd", "A ló tóloldalán", "8 months ago", "3:01", string.Empty),
            new PlaylistItemViewModel("5", "Mind1", "A ló tóloldalán", "8 months ago", "3:11", string.Empty),
            new PlaylistItemViewModel("6", "introvertált dal", "memento", "8 months ago", "2:49", string.Empty),
            new PlaylistItemViewModel("7", "Rét", "Rét", "8 months ago", "2:59", string.Empty),
            new PlaylistItemViewModel("8", "3korty", "memento", "8 months ago", "3:13", string.Empty),
            new PlaylistItemViewModel("9", "Rampapagam", "CARPE DIEM", "8 months ago", "3:09", string.Empty),
            new PlaylistItemViewModel("10", "Pullup", "A ló tóloldalán", "8 months ago", "2:17", string.Empty),
        };

        UpdateHoverPopupText();
    }

    private void UpdateHoverPopupText() =>
        HoverPopupText = $"{(IsShuffling ? "Disable" : "Enable")} Shuffle for {Title}";
}
