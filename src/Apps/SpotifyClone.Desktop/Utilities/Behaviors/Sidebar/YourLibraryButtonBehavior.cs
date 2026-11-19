using SpotifyClone.Desktop.Utilities.Animations;
using SpotifyClone.Desktop.Utilities.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Sidebar;

public static class YourLibraryButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(YourLibraryButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty YourLibraryTextProperty =
        DependencyProperty.RegisterAttached(
            "YourLibraryText",
            typeof(TextBlock),
            typeof(YourLibraryButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchGridHeightProperty =
        DependencyProperty.RegisterAttached(
            "SearchGridHeight",
            typeof(double),
            typeof(YourLibraryButtonBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SidebarCollapseStateProperty =
        DependencyProperty.RegisterAttached(
            "SidebarCollapseState",
            typeof(bool),
            typeof(YourLibraryButtonBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSidebarCollapseStateChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static TextBlock GetYourLibraryText(DependencyObject obj) =>
        (TextBlock)obj.GetValue(YourLibraryTextProperty);
    public static void SetYourLibraryText(DependencyObject obj, TextBlock value) =>
        obj.SetValue(YourLibraryTextProperty, value);

    public static double GetSearchGridHeight(DependencyObject obj) =>
        (double)obj.GetValue(SearchGridHeightProperty);
    public static void SetSearchGridHeight(DependencyObject obj, double value) =>
        obj.SetValue(SearchGridHeightProperty, value);

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

    private static void OnSidebarCollapseStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        SetSearchGridHeight(element, (bool)e.NewValue ? 0 : double.NaN);
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        TextBlock yourLibraryText = GetYourLibraryText(element);

        ColorAnimations.AnimateForeground(element, Colors.White, .1);
        ColorAnimations.AnimateForeground(yourLibraryText, Colors.White, .1);

        UpdateHoverPopupDisplay(element);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        TextBlock yourLibraryText = GetYourLibraryText(element);

        ColorAnimations.AnimateForeground(element, Color.FromRgb(179, 179, 179), .1);
        ColorAnimations.AnimateForeground(yourLibraryText, Color.FromRgb(179, 179, 179), .1);

        HoverPopupHelper.HidePopup();
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetSidebarCollapseState(element, !GetSidebarCollapseState(element));
        UpdateHoverPopupDisplay(element);
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

    private static void UpdateHoverPopupDisplay(Button element)
        => HoverPopupHelper.DisplayPopupText(element, PlacementMode.MousePoint,
            !GetSidebarCollapseState(element) ? "Collapse Your Library" : "Expand Your Library");

    #endregion
}
