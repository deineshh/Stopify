using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Playlist.PlaylistItem;

public static class PlaylistItemSizeChangeBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(PlaylistItemSizeChangeBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty DateBtnWidthProperty =
        DependencyProperty.RegisterAttached(
        "DateBtnWidth",
        typeof(double),
        typeof(PlaylistItemSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty DateColumnWidthProperty =
        DependencyProperty.RegisterAttached(
        "DateColumnWidth",
        typeof(GridLength),
        typeof(PlaylistItemSizeChangeBehavior),
        new FrameworkPropertyMetadata(new GridLength(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty AlbumBtnWidthProperty =
        DependencyProperty.RegisterAttached(
        "AlbumBtnWidth",
        typeof(double),
        typeof(PlaylistItemSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty AlbumColumnWidthProperty =
        DependencyProperty.RegisterAttached(
        "AlbumColumnWidth",
        typeof(GridLength),
        typeof(PlaylistItemSizeChangeBehavior),
        new FrameworkPropertyMetadata(new GridLength(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static double GetDateBtnWidth(DependencyObject obj) =>
        (double)obj.GetValue(DateBtnWidthProperty);
    public static void SetDateBtnWidth(DependencyObject obj, double value) =>
        obj.SetValue(DateBtnWidthProperty, value);

    public static GridLength GetDateColumnWidth(DependencyObject obj) =>
        (GridLength)obj.GetValue(DateColumnWidthProperty);
    public static void SetDateColumnWidth(DependencyObject obj, GridLength value) =>
        obj.SetValue(DateColumnWidthProperty, value);

    public static double GetAlbumBtnWidth(DependencyObject obj) =>
        (double)obj.GetValue(AlbumBtnWidthProperty);
    public static void SetAlbumBtnWidth(DependencyObject obj, double value) =>
        obj.SetValue(AlbumBtnWidthProperty, value);

    public static GridLength GetAlbumColumnWidth(DependencyObject obj) =>
        (GridLength)obj.GetValue(AlbumColumnWidthProperty);
    public static void SetAlbumColumnWidth(DependencyObject obj, GridLength value) =>
        obj.SetValue(AlbumColumnWidthProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UserControl element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.SizeChanged += OnSizeChanged;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.SizeChanged -= OnSizeChanged;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        if (element.ActualWidth >= 724)
        {
            SetDateBtnWidth(element, double.NaN);
            SetAlbumBtnWidth(element, double.NaN);

            SetDateColumnWidth(element, new GridLength(1, GridUnitType.Star));
            SetAlbumColumnWidth(element, new GridLength(1, GridUnitType.Star));
        }
        else if (element.ActualWidth >= 494)
        {
            SetAlbumBtnWidth(element, double.NaN);
            SetDateBtnWidth(element, 0);

            SetAlbumColumnWidth(element, new GridLength(1, GridUnitType.Star));
            SetDateColumnWidth(element, new GridLength(0, GridUnitType.Auto));
        }
        else
        {
            SetAlbumBtnWidth(element, 0);
            SetDateBtnWidth(element, 0);

            SetAlbumColumnWidth(element, new GridLength(0, GridUnitType.Auto));
            SetDateColumnWidth(element, new GridLength(0, GridUnitType.Auto));
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        element.SizeChanged -= OnSizeChanged;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
