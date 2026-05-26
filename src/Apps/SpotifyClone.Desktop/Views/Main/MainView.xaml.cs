using SpotifyClone.Desktop.ViewModels.Main;
using SpotifyClone.Desktop.Utilities.Behaviors.Main;
using System.Windows;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.Main;

public partial class MainView : Window
{
    public MainView(MainViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        BindingOperations.SetBinding(this, MainViewSizeChangeBehavior.QueueHeightProperty, new Binding()
        {
            Source = SongQueue,
            Path = new PropertyPath("Height"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, MainViewSizeChangeBehavior.QueueWidthProperty, new Binding()
        {
            Source = SongQueue,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, MainViewSizeChangeBehavior.NowPlayingActualHeightProperty, new Binding()
        {
            Source = NowPlaying,
            Path = new PropertyPath("ActualHeight")
        });

        BindingOperations.SetBinding(this, MainViewSizeChangeBehavior.NowPlayingWidthProperty, new Binding()
        {
            Source = NowPlaying,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, MainViewSizeChangeBehavior.SidebarWidthProperty, new Binding()
        {
            Source = SideBar,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, MainViewSizeChangeBehavior.QueueBorderProperty, new Binding()
        {
            Source = SongQueue
        });
    }
}
