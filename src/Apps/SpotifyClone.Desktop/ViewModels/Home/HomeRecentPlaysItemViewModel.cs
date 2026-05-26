using SpotifyClone.Desktop.ViewModels.Base;

namespace SpotifyClone.Desktop.ViewModels.Home;

public class HomeRecentPlaysItemViewModel(
    string title, bool isPlaying, string imagePath)
    : ViewModelBase
{
    public string Title
    {
        get;
        set => SetProperty(ref field, value);
    } = title;

    public bool IsPlaying
    {
        get;
        set => SetProperty(ref field, value);
    } = isPlaying;

    public string ImagePath
    {
        get;
        set => SetProperty(ref field, value);
    } = imagePath;
}
