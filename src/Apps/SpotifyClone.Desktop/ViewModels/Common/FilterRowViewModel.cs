using SpotifyClone.Desktop.ViewModels.Base;
using System.Collections.ObjectModel;

namespace SpotifyClone.Desktop.ViewModels.Common;

public class FilterRowViewModel : ViewModelBase
{
    private string _title;

    private readonly ObservableCollection<CommonItemViewModel> _items;

    public bool IsFilteringAll
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public bool IsFilteringMusic
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsFilteringPodcasts
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public IEnumerable<CommonItemViewModel> Items => _items;

    public FilterRowViewModel(string title)
    {
        _title = title;

        _items = new ObservableCollection<CommonItemViewModel>
        {
            new CommonItemViewModel("Azahriah", "Artist", string.Empty),
            new CommonItemViewModel("DESH", "Artist", string.Empty),
            new CommonItemViewModel("YoungFly", "Artist", string.Empty),
            new CommonItemViewModel("Nessaj", "Streamer", string.Empty),
            new CommonItemViewModel("TheBigO", "Minecraft", string.Empty),
            new CommonItemViewModel("UborCraft", "uTuber", string.Empty),
            new CommonItemViewModel("Sajt32", "Minecraft", string.Empty),
            new CommonItemViewModel("XP", "Minecraft", string.Empty),
        };
    }
}
