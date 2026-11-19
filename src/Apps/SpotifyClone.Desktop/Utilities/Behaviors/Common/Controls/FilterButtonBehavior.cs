using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class FilterButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(FilterButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.RegisterAttached(
            "FilterText",
            typeof(TextBlock),
            typeof(FilterButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FilterIconProperty =
        DependencyProperty.RegisterAttached(
            "FilterIcon",
            typeof(TextBlock),
            typeof(FilterButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FilterTextWidthProperty =
        DependencyProperty.RegisterAttached(
            "FilterTextWidth",
            typeof(double),
            typeof(FilterButtonBehavior),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsSearchingProperty =
        DependencyProperty.RegisterAttached(
            "IsSearching",
            typeof(bool),
            typeof(FilterButtonBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSearchingChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static TextBlock GetFilterText(DependencyObject obj) =>
        (TextBlock)obj.GetValue(FilterTextProperty);
    public static void SetFilterText(DependencyObject obj, TextBlock value) =>
        obj.SetValue(FilterTextProperty, value);

    public static TextBlock GetFilterIcon(DependencyObject obj) =>
        (TextBlock)obj.GetValue(FilterIconProperty);
    public static void SetFilterIcon(DependencyObject obj, TextBlock value) =>
        obj.SetValue(FilterIconProperty, value);

    public static double GetFilterTextWidth(DependencyObject obj) =>
        (double)obj.GetValue(FilterTextWidthProperty);
    public static void SetFilterTextWidth(DependencyObject obj, double value) =>
        obj.SetValue(FilterTextWidthProperty, value);

    public static bool GetIsSearching(DependencyObject obj) =>
        (bool)obj.GetValue(IsSearchingProperty);
    public static void SetIsSearching(DependencyObject obj, bool value) =>
        obj.SetValue(IsSearchingProperty, value);

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
            element.Click += OnRecentsButtonClick;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Click -= OnRecentsButtonClick;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnIsSearchingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        SetFilterTextWidth(element, (bool)e.NewValue ? 0 : double.NaN);
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateForeground(GetFilterIcon(element), Colors.White, 0.1);
        ColorAnimations.AnimateForeground(GetFilterText(element), Colors.White, 0.1);
        ScaleAnimations.BeginScaleAnimation(element, 1.03, 0.1);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateForeground(GetFilterIcon(element), Colors.DarkGray, 0.1);
        ColorAnimations.AnimateForeground(GetFilterText(element), Colors.DarkGray, 0.1);
        ScaleAnimations.ResetScaleAnimation(element, 0.1);
    }

    private static void OnRecentsButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetIsSearching(element, false);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Click -= OnRecentsButtonClick;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
