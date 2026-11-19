using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SpotifyClone.Desktop.Utilities.Helpers;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class PlayBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(PlayBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.RegisterAttached(
            "IsPlaying",
            typeof(bool),
            typeof(PlayBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsPlayingChanged));

    public static readonly DependencyProperty PlayIconProperty =
        DependencyProperty.RegisterAttached(
            "PlayIcon",
            typeof(string),
            typeof(PlayBehavior),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty TrackProperty =
        DependencyProperty.RegisterAttached(
            "Track",
            typeof(string),
            typeof(PlayBehavior),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AuthorProperty =
        DependencyProperty.RegisterAttached(
            "Author",
            typeof(string),
            typeof(PlayBehavior),
            new PropertyMetadata(string.Empty));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(UIElement element) =>
        (bool)element.GetValue(EnableProperty);
    public static void SetEnable(UIElement element, bool value) =>
        element.SetValue(EnableProperty, value);

    public static bool GetIsPlaying(UIElement element) =>
        (bool)element.GetValue(IsPlayingProperty);
    public static void SetIsPlaying(UIElement element, bool value) =>
        element.SetValue(IsPlayingProperty, value);

    public static string GetPlayIcon(UIElement element) =>
        (string)element.GetValue(PlayIconProperty);
    public static void SetPlayIcon(UIElement element, string value) =>
        element.SetValue(PlayIconProperty, value);

    public static string GetTrack(UIElement element) =>
        (string)element.GetValue(TrackProperty);
    public static void SetTrack(UIElement element, string value) =>
        element.SetValue(TrackProperty, value);

    public static string GetAuthor(UIElement element) =>
        (string)element.GetValue(AuthorProperty);
    public static void SetAuthor(UIElement element, string value) =>
        element.SetValue(AuthorProperty, value);

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
            element.Click += OnClick;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Click -= OnClick;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        SetIsPlaying(element, (bool)e.NewValue);
        UpdatePlayIcon(element);
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        DisplayHoverPopup(element);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button)
        {
            return;
        }

        HoverPopupHelper.HidePopup();
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetIsPlaying(element, !GetIsPlaying(element));
        UpdatePlayIcon(element);
        DisplayHoverPopup(element);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Click -= OnClick;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void DisplayHoverPopup(Button element)
    {
        bool isPlaying = GetIsPlaying(element);
        string track = GetTrack(element);
        string author = GetAuthor(element);

        StringBuilder hoverPopupText = new();
        hoverPopupText.Append(isPlaying ? "Pause" : "Play");
        hoverPopupText.Append(string.IsNullOrEmpty(track) ? string.Empty : $" {track}");
        hoverPopupText.Append(string.IsNullOrEmpty(author) ? string.Empty : $" by {author}");

        HoverPopupHelper.DisplayPopupText(element, PlacementMode.Top, hoverPopupText.ToString());
    }

    private static void UpdatePlayIcon(Button element) =>
        SetPlayIcon(element, GetIsPlaying(element) ? "\uf04c" : "\uf04b");

    #endregion
}
