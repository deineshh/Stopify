using SpotifyClone.Desktop.Utilities.Behaviors.Playlist.PlaylistItem;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.Playlist;

public partial class PlaylistItem : UserControl
{
    public PlaylistItem()
    {
        InitializeComponent();

        Binding dateBtnWidthBinding = new()
        {
            Source = DateBtn,
            Path = new PropertyPath("Width")
        };
        BindingOperations.SetBinding(
            this, PlaylistItemSizeChangeBehavior.DateBtnWidthProperty, dateBtnWidthBinding);

        Binding dateColumnWidthBinding = new()
        {
            Source = DateColumn,
            Path = new PropertyPath("Width")
        };
        BindingOperations.SetBinding(
            this, PlaylistItemSizeChangeBehavior.DateColumnWidthProperty, dateColumnWidthBinding);

        Binding albumBtnWidthBinding = new()
        {
            Source = AlbumBtn,
            Path = new PropertyPath("Width")
        };
        BindingOperations.SetBinding(
            this, PlaylistItemSizeChangeBehavior.AlbumBtnWidthProperty, albumBtnWidthBinding);

        Binding albumColumnWidthBinding = new()
        {
            Source = AlbumColumn,
            Path = new PropertyPath("Width")
        };
        BindingOperations.SetBinding(
            this, PlaylistItemSizeChangeBehavior.AlbumColumnWidthProperty, albumColumnWidthBinding);
    }
}
