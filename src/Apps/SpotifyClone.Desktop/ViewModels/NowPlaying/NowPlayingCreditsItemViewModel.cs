using SpotifyClone.Desktop.ViewModels.Base;

namespace SpotifyClone.Desktop.ViewModels.NowPlaying;

public class NowPlayingCreditsItemViewModel(
    string artist, bool isFollowing)
    : ViewModelBase
{
    public bool IsFollowing
    {
        get;
        set => SetProperty(ref field, value);
    } = isFollowing;

    public string Artist
    {
        get;
        set => SetProperty(ref field, value);
    } = artist;
}
