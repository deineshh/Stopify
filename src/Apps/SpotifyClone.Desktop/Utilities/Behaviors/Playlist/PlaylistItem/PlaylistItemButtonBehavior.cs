using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Playlist.PlaylistItem;

public static class PlaylistItemButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(PlaylistItemButtonBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.RegisterAttached(
        "IsSelected",
        typeof(bool),
        typeof(PlaylistItemButtonBehavior),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlaylistItemBorderProperty =
        DependencyProperty.RegisterAttached(
        "PlaylistItemBorder",
        typeof(Border),
        typeof(PlaylistItemButtonBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty NumberWidthProperty =
        DependencyProperty.RegisterAttached(
        "NumberWidth",
        typeof(double),
        typeof(PlaylistItemButtonBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlayButtonWidthProperty =
        DependencyProperty.RegisterAttached(
        "PlayButtonWidth",
        typeof(double),
        typeof(PlaylistItemButtonBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveButtonVisibility =
        DependencyProperty.RegisterAttached(
        "SaveButtonVisibility",
        typeof(Visibility),
        typeof(PlaylistItemButtonBehavior),
        new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty OptionsButtonVisibilityProperty =
        DependencyProperty.RegisterAttached(
        "OptionsButtonVisibility",
        typeof(Visibility),
        typeof(PlaylistItemButtonBehavior),
        new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty AlbumButton =
        DependencyProperty.RegisterAttached(
        "AlbumButton",
        typeof(Button),
        typeof(PlaylistItemButtonBehavior),
        new PropertyMetadata(null));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static bool GetIsSelected(DependencyObject obj) =>
        (bool)obj.GetValue(IsSelectedProperty);
    public static void SetIsSelected(DependencyObject obj, bool value) =>
        obj.SetValue(IsSelectedProperty, value);

    public static Border GetPlaylistItemBorder(DependencyObject obj) =>
        (Border)obj.GetValue(PlaylistItemBorderProperty);
    public static void SetPlaylistItemBorder(DependencyObject obj, Border value) =>
        obj.SetValue(PlaylistItemBorderProperty, value);

    public static double GetNumberWidth(DependencyObject obj) =>
        (double)obj.GetValue(NumberWidthProperty);
    public static void SetNumberWidth(DependencyObject obj, double value) =>
        obj.SetValue(NumberWidthProperty, value);

    public static double GetPlayButtonWidth(DependencyObject obj) =>
        (double)obj.GetValue(PlayButtonWidthProperty);
    public static void SetPlayButtonWidth(DependencyObject obj, double value) =>
        obj.SetValue(PlayButtonWidthProperty, value);

    public static Visibility GetSaveButtonVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(SaveButtonVisibility);
    public static void SetSaveButtonVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(SaveButtonVisibility, value);

    public static Visibility GetOptionsButtonVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(OptionsButtonVisibilityProperty);
    public static void SetOptionsButtonVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(OptionsButtonVisibilityProperty, value);

    public static Button GetAlbumButton(DependencyObject obj) =>
        (Button)obj.GetValue(AlbumButton);
    public static void SetAlbumButton(DependencyObject obj, Button value) =>
        obj.SetValue(AlbumButton, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.Click += OnGotFocus;
            element.GotFocus += OnGotFocus;
            element.LostFocus += OnLostFocus;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Click -= OnGotFocus;
            element.GotFocus -= OnGotFocus;
            element.LostFocus -= OnLostFocus;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!GetIsSelected(element))
        {
            ColorAnimations.AnimateBackground(GetPlaylistItemBorder(element), Color.FromArgb(100, 128, 128, 128), 0.1);
        }

        SetNumberWidth(element, 0);
        SetPlayButtonWidth(element, double.NaN);
        SetSaveButtonVisibility(element, Visibility.Visible);
        SetOptionsButtonVisibility(element, Visibility.Visible);
        ColorAnimations.AnimateForeground(GetAlbumButton(element), Brushes.White.Color, 0.2);
    }

    private static void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!GetIsSelected(element))
        {
            ColorAnimations.AnimateBackground(GetPlaylistItemBorder(element), Color.FromArgb(0, 128, 128, 128), 0.1);
        }

        SetNumberWidth(element, 29);
        SetPlayButtonWidth(element, 0);
        SetSaveButtonVisibility(element, Visibility.Hidden);
        SetOptionsButtonVisibility(element, Visibility.Hidden);
        ColorAnimations.AnimateForeground(GetAlbumButton(element), Brushes.DarkGray.Color, 0.1);
    }

    private static void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateBackground(GetPlaylistItemBorder(element), Color.FromArgb(200, 128, 128, 128), 0.1);
        SetIsSelected(element, true);
    }

    private static void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateBackground(GetPlaylistItemBorder(element), Color.FromArgb(0, 128, 128, 128), 0.1);
        SetIsSelected(element, false);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Click -= OnGotFocus;
        element.GotFocus -= OnGotFocus;
        element.LostFocus -= OnLostFocus;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
