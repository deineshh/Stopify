using SpotifyClone.Desktop.ViewModels.Base;
using System.Collections.ObjectModel;

namespace SpotifyClone.Desktop.ViewModels.Common;

public class CommonRowViewModel : ViewModelBase
{
    private string _category;
    private string _author;

    private readonly ObservableCollection<CommonItemViewModel> _items;

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string Author
    {
        get => _author;
        set => SetProperty(ref _author, value);
    }

    public IEnumerable<CommonItemViewModel> Items => _items;

    public CommonRowViewModel(string category = "", string author = "")
    {
        _category = category;
        _author = author;

        _items = new ObservableCollection<CommonItemViewModel> {
            new CommonItemViewModel("Azahriah", "Artist", string.Empty),
            new CommonItemViewModel("DESH", "Artist", string.Empty),
            new CommonItemViewModel("YoungFly", "Artist", string.Empty),
            new CommonItemViewModel("Nessaj", "Streamer", string.Empty),
            new CommonItemViewModel("Baukó Attila", "Artist", string.Empty),
            new CommonItemViewModel("Azahriah", "Gamer", string.Empty),
            new CommonItemViewModel("Azahriah", "Bunko", string.Empty),
            new CommonItemViewModel("Azahriah", "Rozsda", string.Empty),
            new CommonItemViewModel("Azahriah", "Rozsda", string.Empty),
        };
    }
}
