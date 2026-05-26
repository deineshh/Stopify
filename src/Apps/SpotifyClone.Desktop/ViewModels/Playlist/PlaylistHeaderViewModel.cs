using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.Utilities.Enums.Playlist;

namespace SpotifyClone.Desktop.ViewModels.Playlist;

public class PlaylistHeaderViewModel : ViewModelBase
{
    public PlaylistSortType SortType
    {
        get;
        set => SetProperty(ref field, value);
    } = PlaylistSortType.Off;
}
