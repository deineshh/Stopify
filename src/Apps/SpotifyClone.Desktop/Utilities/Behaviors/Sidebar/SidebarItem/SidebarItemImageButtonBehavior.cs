using SpotifyClone.Desktop.Utilities.Animations;
using SpotifyClone.Desktop.Utilities.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Sidebar.SidebarItem;

public static class SidebarItemImageButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SidebarItemImageButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty PlayBtnProperty =
        DependencyProperty.RegisterAttached(
            "PlayBtn",
            typeof(Button),
            typeof(SidebarItemImageButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.RegisterAttached(
            "IsPlaying",
            typeof(bool),
            typeof(SidebarItemImageButtonBehavior),
            new PropertyMetadata(false, OnIsPlayingChanged));

    public static readonly DependencyProperty PlaylistTitleProperty =
        DependencyProperty.RegisterAttached(
            "PlaylistTitle",
            typeof(string),
            typeof(SidebarItemImageButtonBehavior),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SidebarCollapseStateProperty =
        DependencyProperty.RegisterAttached(
            "SidebarCollapseState",
            typeof(bool),
            typeof(SidebarItemImageButtonBehavior),
            new PropertyMetadata(false));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Button GetPlayBtn(DependencyObject obj) =>
        (Button)obj.GetValue(PlayBtnProperty);
    public static void SetPlayBtn(DependencyObject obj, Button value) =>
        obj.SetValue(PlayBtnProperty, value);

    public static bool GetIsPlaying(DependencyObject obj) =>
        (bool)obj.GetValue(IsPlayingProperty);
    public static void SetIsPlaying(DependencyObject obj, bool value) =>
        obj.SetValue(IsPlayingProperty, value);

    public static string GetPlaylistTitle(DependencyObject obj) =>
        (string)obj.GetValue(PlaylistTitleProperty);
    public static void SetPlaylistTitle(DependencyObject obj, string value) =>
        obj.SetValue(PlaylistTitleProperty, value);

    public static bool GetSidebarCollapseState(DependencyObject obj) =>
        (bool)obj.GetValue(SidebarCollapseStateProperty);
    public static void SetSidebarCollapseState(DependencyObject obj, bool value) =>
        obj.SetValue(SidebarCollapseStateProperty, value);

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
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element || !GetEnable(element))
        {
            return;
        }

        RefreshHoverPopup(element, GetIsPlaying(element), GetPlaylistTitle(element));
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!GetSidebarCollapseState(element))
        {
            bool isPlaying = GetIsPlaying(element);
            string playlistTitle = GetPlaylistTitle(element);

            ScaleAnimations.BeginScaleAnimation(GetPlayBtn(element), 1.02, .1);
            RefreshHoverPopup(element, isPlaying, playlistTitle);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!GetSidebarCollapseState(element))
        {
            ScaleAnimations.ResetScaleAnimation(GetPlayBtn(element), .1);
            HoverPopupHelper.HidePopup();
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void RefreshHoverPopup(Button element, bool isPlaying, string playlistTitle)
    {
        HoverPopupHelper.HidePopup();
        HoverPopupHelper.DisplayPopupText(element, PlacementMode.Top,
                        isPlaying ? $"Pause {playlistTitle}" : $"Play {playlistTitle}");
    }

    #endregion
}
