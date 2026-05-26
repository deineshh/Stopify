using SpotifyClone.Desktop.Utilities.Behaviors.Artist;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.Artist;

public partial class ArtistView : UserControl
{
    public ArtistView()
    {
        InitializeComponent();

        Binding artistNameFontSizeBinding = new()
        {
            Source = ArtistName,
            Path = new PropertyPath("FontSize")
        };
        BindingOperations.SetBinding(
            this, ArtistSizeChangeBehavior.ArtistNameFontSizeProperty, artistNameFontSizeBinding);

        Binding artistNameMaxHeightBinding = new()
        {
            Source = ArtistName,
            Path = new PropertyPath("MaxHeight")
        };
        BindingOperations.SetBinding(
            this, ArtistSizeChangeBehavior.ArtistNameMaxHeightProperty, artistNameMaxHeightBinding);

        Binding scrollerBgColorBinding = new()
        {
            Source = ScrollerBg,
            Path = new PropertyPath("Color")
        };
        BindingOperations.SetBinding(
            this, ArtistSizeChangeBehavior.ScrollerBgColorProperty, scrollerBgColorBinding);

        Binding stickyHeaderBgBackgroundBinding = new()
        {
            Source = StickyHeaderBg,
            Path = new PropertyPath("Background")
        };
        BindingOperations.SetBinding(
            this, ArtistSizeChangeBehavior.StickyHeaderBgBackgroundProperty, stickyHeaderBgBackgroundBinding);
    }
}
