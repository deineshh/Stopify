using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Player;

// IMPORTANT: the IsMuted property works fine only with ToggleButtonStateBehavior
public static class VolumeButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty IsMutedProperty =
        DependencyProperty.RegisterAttached(
            "IsMuted",
            typeof(bool),
            typeof(VolumeButtonBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsMutedChanged));

    public static readonly DependencyProperty VolumeBarCoverProperty =
        DependencyProperty.RegisterAttached(
            "VolumeBarCover",
            typeof(Label),
            typeof(VolumeButtonBehavior),
            new PropertyMetadata(null));

    #endregion

    #region Getters/Setters

    public static bool GetIsMuted(DependencyObject obj) =>
        (bool)obj.GetValue(IsMutedProperty);
    public static void SetIsMuted(DependencyObject obj, bool value) =>
        obj.SetValue(IsMutedProperty, value);

    public static Label GetVolumeBarCover(DependencyObject obj) =>
        (Label)obj.GetValue(VolumeBarCoverProperty);
    public static void SetVolumeBarCover(DependencyObject obj, Label value) =>
        obj.SetValue(VolumeBarCoverProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnIsMutedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        Label volumeBarCover = GetVolumeBarCover(element);

        if ((bool)e.NewValue)
        {
            element.Content = "\uf6a9";
            element.Margin = new Thickness(0, 0, 3, 0);
            volumeBarCover.Opacity = .6;
            Canvas.SetZIndex(volumeBarCover, 2);
        }
        else
        {
            element.Content = "\uf028";
            element.Margin = new Thickness(0, 0, 1, 0);
            volumeBarCover.Opacity = 0;
            Canvas.SetZIndex(volumeBarCover, 0);
        }
    }

    #endregion
}
