using SpotifyClone.Desktop.ViewModels.Base;
using System.Collections.ObjectModel;

namespace SpotifyClone.Desktop.ViewModels.Queue;

public class QueueItemViewModel : ViewModelBase
{
    private string _track;
    private string _imagePath;

    private readonly ObservableCollection<string> _authors;

    public bool IsPlaying
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Track
    {
        get => _track;
        set => SetProperty(ref _track, value);
    }

    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    public IEnumerable<string> Authors => _authors;

    public QueueItemViewModel(string track, string imagePath)
    {
        _track = track;
        _imagePath = imagePath;

        _authors = new ObservableCollection<string>()
        {
            "Azahriah",
            "DESH",
            "Young Fly",
        };
    }
}
