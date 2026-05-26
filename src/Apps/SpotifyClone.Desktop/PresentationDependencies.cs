using Microsoft.Extensions.DependencyInjection;
using SpotifyClone.Desktop.ViewModels.Artist;
using SpotifyClone.Desktop.ViewModels.Home;
using SpotifyClone.Desktop.ViewModels.Main;
using SpotifyClone.Desktop.ViewModels.NowPlaying;
using SpotifyClone.Desktop.ViewModels.Player;
using SpotifyClone.Desktop.ViewModels.Playlist;
using SpotifyClone.Desktop.ViewModels.Queue;
using SpotifyClone.Desktop.ViewModels.Search;
using SpotifyClone.Desktop.ViewModels.Sidebar;
using SpotifyClone.Desktop.ViewModels.Titlebar;
using SpotifyClone.Desktop.Utilities.Stores;
using SpotifyClone.Desktop.Views.Artist;
using SpotifyClone.Desktop.Views.Home;
using SpotifyClone.Desktop.Views.Main;
using SpotifyClone.Desktop.Views.NowPlaying;
using SpotifyClone.Desktop.Views.Player;
using SpotifyClone.Desktop.Views.Playlist;
using SpotifyClone.Desktop.Views.Queue;
using SpotifyClone.Desktop.Views.Search;
using SpotifyClone.Desktop.Views.Sidebar;
using SpotifyClone.Desktop.Views.Titlebar;

namespace SpotifyClone.Desktop;

public static class PresentationDependencies
{
    public static IServiceCollection AddPresentationDependencies(this IServiceCollection services)
    {
        services.AddViewDependencies()
                .AddViewModelDependencies()
                .AddStateDependencies();

        return services;
    }

    public static IServiceCollection AddStateDependencies(this IServiceCollection services)
    {
        services.AddSingleton<NavigationStore>()
                .AddSingleton<UIState>();

        return services;
    }

    public static IServiceCollection AddViewModelDependencies(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>()
                .AddTransient<HomeViewModel>()
                .AddTransient<SearchViewModel>()
                .AddTransient<ArtistViewModel>()
                .AddTransient<PlaylistViewModel>()
                .AddTransient<SidebarViewModel>()
                .AddTransient<NowPlayingViewModel>()
                .AddTransient<QueueViewModel>()
                .AddTransient<TitlebarViewModel>()
                .AddTransient<PlayerViewModel>();

        return services;
    }

    public static IServiceCollection AddViewDependencies(this IServiceCollection services)
    {
        services.AddSingleton<MainView>()
                .AddTransient<HomeView>()
                .AddTransient<SearchView>()
                .AddTransient<ArtistView>()
                .AddTransient<PlaylistView>()
                .AddTransient<SidebarControl>()
                .AddTransient<NowPlayingView>()
                .AddTransient<QueueView>()
                .AddTransient<TitlebarControl>()
                .AddTransient<PlayerControl>();

        return services;
    }
}
