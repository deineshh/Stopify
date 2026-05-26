using SpotifyClone.Desktop.Utilities.Behaviors.NowPlaying;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.NowPlaying;

public partial class NowPlayingView : UserControl
{
    public NowPlayingView()
    {
        InitializeComponent();

        Binding saveTextMarginBinding = new()
        {
            Source = SaveText,
            Path = new PropertyPath("Margin"),
            Mode = BindingMode.TwoWay
        };
        BindingOperations.SetBinding(
            this, NowPlayingSizeChangeBehavior.SaveTextMarginProperty, saveTextMarginBinding);

        Binding saveTextFontSizeBinding = new()
        {
            Source = SaveText,
            Path = new PropertyPath("FontSize"),
            Mode = BindingMode.TwoWay
        };
        BindingOperations.SetBinding(
            this, NowPlayingSizeChangeBehavior.SaveTextFontSizeProperty, saveTextFontSizeBinding);

        Binding saveBtnHeightBinding = new()
        {
            Source = SaveBtn,
            Path = new PropertyPath("Height"),
            Mode = BindingMode.TwoWay
        };
        BindingOperations.SetBinding(
            this, NowPlayingSizeChangeBehavior.SaveButtonHeightProperty, saveBtnHeightBinding);

        Binding saveBorderWidthBinding = new()
        {
            Source = SaveBorder,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        };
        BindingOperations.SetBinding(
            this, NowPlayingSizeChangeBehavior.SaveBorderWidthProperty, saveBorderWidthBinding);
    }
}
