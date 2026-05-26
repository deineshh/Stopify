using SpotifyClone.Desktop.ViewModels.Base;

namespace SpotifyClone.Desktop.ViewModels.Common;

public class CommonItemViewModel(
    string title, string description, string imagePath)
    : ViewModelBase
{
    public bool IsPlaying
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Title
    {
        get;
        set => SetProperty(ref field, value);
    } = title;

    public string Description
    {
        get;
        set => SetProperty(ref field, value);
    } = description;

    public string ImagePath
    {
        get;
        set => SetProperty(ref field, value);
    } = imagePath;
}
