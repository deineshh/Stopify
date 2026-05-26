using SpotifyClone.Desktop.ViewModels.Base;
using System.Collections.ObjectModel;

namespace SpotifyClone.Desktop.ViewModels.Playlist;

public class PlaylistItemViewModel : ViewModelBase
{
    private string _number;
    private string _title;
    private string _album;
    private string _dateAdded;
    private string _duration;
    private string _imagePath;

    public bool IsSelected
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsPlaying
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsSaved
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Album
    {
        get => _album;
        set => SetProperty(ref _album, value);
    }

    public string DateAdded
    {
        get => _dateAdded;
        set => SetProperty(ref _dateAdded, value);
    }

    public string Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    public string SaveTo
    {
        get;
        set => SetProperty(ref field, value);
    } = "Liked Songs";

    public ObservableCollection<string> Authors { get; }

    public PlaylistItemViewModel(string number, string title, string album, string dateAdded, string duration, string imagePath)
    {
        _number = number;
        _title = title;
        _album = album;
        _dateAdded = dateAdded;
        _duration = duration;
        _imagePath = imagePath;

        Authors = new ObservableCollection<string>()
        {
            "Azahriah",
            "DESH",
            "Young Fly",
        };
    }
}
