using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class SearchButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty SearchBorderProperty =
        DependencyProperty.RegisterAttached(
            "SearchBorder",
            typeof(Border),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.RegisterAttached(
            "SearchText",
            typeof(TextBlock),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsSearchingProperty =
        DependencyProperty.RegisterAttached(
            "IsSearching",
            typeof(bool),
            typeof(SearchButtonBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MouseEnterBackgroundColorProperty =
        DependencyProperty.RegisterAttached(
            "MouseEnterBackgroundColor",
            typeof(Color),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(Color.FromRgb(39, 39, 39)));

    public static readonly DependencyProperty MouseLeaveBackgroundColorProperty =
        DependencyProperty.RegisterAttached(
            "MouseLeaveBackgroundColor",
            typeof(Color),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(Color.FromRgb(18, 18, 18)));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Border GetSearchBorder(DependencyObject obj) =>
        (Border)obj.GetValue(SearchBorderProperty);
    public static void SetSearchBorder(DependencyObject obj, Border value) =>
        obj.SetValue(SearchBorderProperty, value);

    public static TextBlock GetSearchText(DependencyObject obj) =>
        (TextBlock)obj.GetValue(SearchTextProperty);
    public static void SetSearchText(DependencyObject obj, TextBlock value) =>
        obj.SetValue(SearchTextProperty, value);

    public static bool GetIsSearching(DependencyObject obj) =>
        (bool)obj.GetValue(IsSearchingProperty);
    public static void SetIsSearching(DependencyObject obj, bool value) =>
        obj.SetValue(IsSearchingProperty, value);

    public static Color GetMouseEnterBackgroundColor(DependencyObject obj) =>
        (Color)obj.GetValue(MouseEnterBackgroundColorProperty);
    public static void SetMouseEnterBackgroundColor(DependencyObject obj, Color value) =>
        obj.SetValue(MouseEnterBackgroundColorProperty, value);

    public static Color GetMouseLeaveBackgroundColor(DependencyObject obj) =>
        (Color)obj.GetValue(MouseLeaveBackgroundColorProperty);
    public static void SetMouseLeaveBackgroundColor(DependencyObject obj, Color value) =>
        obj.SetValue(MouseLeaveBackgroundColorProperty, value);

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
            element.Click += OnSearchButtonClick;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Click -= OnSearchButtonClick;
            element.Unloaded -= DetachEvents;
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

        if (GetIsSearching(element))
        {
            Mouse.OverrideCursor = Cursors.IBeam;
        }
        else
        {
            Mouse.OverrideCursor = Cursors.Hand;
            ColorAnimations.AnimateBackground(GetSearchBorder(element), GetMouseEnterBackgroundColor(element), 0.1);
            ColorAnimations.AnimateForeground(GetSearchText(element), Colors.LightGray, 0.2);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        Mouse.OverrideCursor = Cursors.Arrow;

        if (!GetIsSearching(element))
        {
            ColorAnimations.AnimateBackground(GetSearchBorder(element), GetMouseLeaveBackgroundColor(element), 0.1);
            ColorAnimations.AnimateForeground(GetSearchText(element), Colors.DarkGray, 0.2);
        }
    }

    private static void OnSearchButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!GetIsSearching(element))
        {
            SetIsSearching(element, true);
            Mouse.OverrideCursor = Cursors.IBeam;
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
        element.Click -= OnSearchButtonClick;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
