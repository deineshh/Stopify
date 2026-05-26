using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.ViewModels.Common;
using System.Collections.ObjectModel;

namespace SpotifyClone.Desktop.ViewModels.Home;

public class HomeViewModel : ViewModelBase
{
    public int ColumnCount
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsAllFiltered
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public bool IsMusicFiltered
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsPodcastsFiltered
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ObservableCollection<CommonRowViewModel> Rows { get; }

    public ObservableCollection<HomeRecentPlaysItemViewModel> RecentPlays { get; }

    public HomeViewModel()
    {
        Rows = new ObservableCollection<CommonRowViewModel>()
        {
            new CommonRowViewModel("Recently played"),
            new CommonRowViewModel("Your favorite artists"),
            new CommonRowViewModel("Jump back in"),
            new CommonRowViewModel("Best of artists"),
            new CommonRowViewModel("Recommended for today"),
            new CommonRowViewModel("New releases for you"),
            new CommonRowViewModel("More like {Artist}"),
            new CommonRowViewModel("Fresh new music"),
            new CommonRowViewModel("More like {Artist}"),
            new CommonRowViewModel("Popular artists"),
            new CommonRowViewModel("For fans of {Artist}"),
        };

        RecentPlays = new ObservableCollection<HomeRecentPlaysItemViewModel>
        {
            new HomeRecentPlaysItemViewModel("Azahriah", false, ""),
            new HomeRecentPlaysItemViewModel("DESH", false, ""),
            new HomeRecentPlaysItemViewModel("YoungFly", false, ""),
            new HomeRecentPlaysItemViewModel("Nessaj", false, ""),
            new HomeRecentPlaysItemViewModel("Coding Music", false, ""),
            new HomeRecentPlaysItemViewModel("Gym Songs", false, ""),
            new HomeRecentPlaysItemViewModel("Calisthenics", false, ""),
            new HomeRecentPlaysItemViewModel("Toth Gabi", false, ""),
        };

        ColumnCount = 2;
    }
}
