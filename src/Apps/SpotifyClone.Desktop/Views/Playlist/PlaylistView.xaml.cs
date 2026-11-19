using SpotifyClone.Desktop.Utilities.Behaviors.Playlist;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.Playlist;

public partial class PlaylistView : UserControl
{
    public PlaylistView()
    {
        InitializeComponent();

        Binding playlistTitleFontSizeBinding = new()
        {
            Source = PlaylistTitle,
            Path = new PropertyPath("FontSize")
        };
        BindingOperations.SetBinding(
            this, PlaylistSizeChangeBehavior.PlaylistTitleFontSizeProperty, playlistTitleFontSizeBinding);

        Binding stickyHeaderBgBackgroundBinding = new()
        {
            Source = StickyHeaderBg,
            Path = new PropertyPath("Background")
        };
        BindingOperations.SetBinding(
            this, PlaylistSizeChangeBehavior.StickyHeaderBgBackgroundProperty, stickyHeaderBgBackgroundBinding);

        Binding scrollerBgColorBinding = new()
        {
            Source = ScrollerBg,
            Path = new PropertyPath("Color")
        };
        BindingOperations.SetBinding(
            this, PlaylistSizeChangeBehavior.ScrollerBgColorProperty, scrollerBgColorBinding);
    }
}
