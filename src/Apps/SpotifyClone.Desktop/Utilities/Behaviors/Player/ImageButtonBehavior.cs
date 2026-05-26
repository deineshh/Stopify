using SpotifyClone.Desktop.Utilities.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Player;

public static class ImageButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(ImageButtonBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty NowPlayingBorderVisibilityProperty = DependencyProperty.RegisterAttached(
        "NowPlayingBorderVisibility",
        typeof(Visibility),
        typeof(ImageButtonBehavior),
        new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty NowPlayingBtnContentProperty = DependencyProperty.RegisterAttached(
        "NowPlayingBtnContent",
        typeof(string),
        typeof(ImageButtonBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty NowPlayingCollapseStateProperty = DependencyProperty.RegisterAttached(
        "NowPlayingCollapseState",
        typeof(bool),
        typeof(ImageButtonBehavior),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnNowPlayingCollapseStateChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Visibility GetNowPlayingBorderVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(NowPlayingBorderVisibilityProperty);
    public static void SetNowPlayingBorderVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(NowPlayingBorderVisibilityProperty, value);

    public static string GetNowPlayingBtnContent(DependencyObject obj) =>
        (string)obj.GetValue(NowPlayingBtnContentProperty);
    public static void SetNowPlayingBtnContent(DependencyObject obj, string value) =>
        obj.SetValue(NowPlayingBtnContentProperty, value);

    public static bool GetNowPlayingCollapseState(DependencyObject obj) =>
        (bool)obj.GetValue(NowPlayingCollapseStateProperty);
    public static void SetNowPlayingCollapseState(DependencyObject obj, bool value) =>
        obj.SetValue(NowPlayingCollapseStateProperty, value);

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

    private static void OnNowPlayingCollapseStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (GetNowPlayingCollapseState(element))
        {
            SetNowPlayingBtnContent(element, "\uf106");
            //ColorAnimations.AnimateForeground(NowPlayingOption, Colors.DarkGray, .1);
        }
        else
        {
            SetNowPlayingBtnContent(element, "\uf107");
            //ColorAnimations.AnimateForeground(NowPlayingOption, Color.FromRgb(30, 215, 96), .1);
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetNowPlayingBorderVisibility(element, Visibility.Visible);

        if (GetNowPlayingCollapseState(element))
        {
            HoverPopupHelper.PopupText = "Expand";
            SetNowPlayingBtnContent(element, "\uf106");
        }
        else
        {
            HoverPopupHelper.PopupText = "Collapse";
            SetNowPlayingBtnContent(element, "\uf107");
        }

        HoverPopupHelper.DisplayPopupText(element, PlacementMode.Top);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetNowPlayingBorderVisibility(element, Visibility.Collapsed);
        HoverPopupHelper.HidePopup();
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        bool nowPlayingCollapseState = GetNowPlayingCollapseState(element);
        HoverPopupHelper.DisplayPopupText(element, PlacementMode.Top, nowPlayingCollapseState ? "Collapse" : "Expand");
        SetNowPlayingCollapseState(element, !nowPlayingCollapseState);
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
}
