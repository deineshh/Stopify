using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SpotifyClone.Desktop.Utilities.Stores;
using SpotifyClone.Desktop.ViewModels.Home;
using SpotifyClone.Desktop.Views.Main;

namespace SpotifyClone.Desktop;

public partial class App : Application
{
    public ServiceProvider Services { get; }

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        PresentationDependencies.AddPresentationDependencies(services);
        services.AddPresentationDependencies();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        NavigationStore navigationStore = Services.GetService<NavigationStore>()!;
        navigationStore.CurrentViewModel = Services.GetService<HomeViewModel>()!;

        MainView mainWindow = Services.GetService<MainView>()!;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Services?.Dispose();
        base.OnExit(e);
    }
}
