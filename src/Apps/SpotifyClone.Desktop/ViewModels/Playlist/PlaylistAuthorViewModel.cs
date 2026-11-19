using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.ViewModels.Common;

namespace SpotifyClone.Desktop.ViewModels.Playlist;

public class PlaylistAuthorViewModel : ViewModelBase
{
    private string _name;
    private string _imagePath;

    private CommonRowViewModel _moreByArtist;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    public CommonRowViewModel MoreByArtist
    {
        get => _moreByArtist;
        set => SetProperty(ref _moreByArtist, value);
    }

    public PlaylistAuthorViewModel(string name, string imagePath)
    {
        _name = name;
        _imagePath = imagePath;
        _moreByArtist = new CommonRowViewModel($"More by {_name}", _name);
    }
}
