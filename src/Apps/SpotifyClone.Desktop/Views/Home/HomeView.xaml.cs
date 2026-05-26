using SpotifyClone.Desktop.Utilities.Behaviors.Home;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.Home;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();

        Binding scrollerBgColorBinding = new()
        {
            Source = ScrollerBg,
            Path = new PropertyPath("Color")
        };
        BindingOperations.SetBinding(
            this, HomeRecentPlaysCollectionBehavior.ScrollerBackgroundColorProperty, scrollerBgColorBinding);

        Binding stickyHeaderBgColorBinding = new()
        {
            Source = StickyHeaderBg,
            Path = new PropertyPath("Color")
        };
        BindingOperations.SetBinding(
            this, HomeRecentPlaysCollectionBehavior.StickyHeaderBackgroundColorProperty, stickyHeaderBgColorBinding);
    }
}
